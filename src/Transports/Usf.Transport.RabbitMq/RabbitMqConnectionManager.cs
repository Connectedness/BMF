using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using RabbitMQ.Client;
using Usf.Core.Messaging.Errors;

namespace Usf.Transport.RabbitMq;

public sealed class RabbitMqConnectionManager : IAsyncDisposable, IDisposable
{
    private readonly Func<CancellationToken, Task<IConnection>> _createConnectionAsync;
    private readonly SemaphoreSlim _gate = new (1, 1);
    private readonly ILogger<RabbitMqConnectionManager> _logger;
    private readonly int _worstCaseChannelCount;
    private readonly string _worstCaseChannelCountDescription;
    private volatile Task<IConnection>? _connectionTask;
    private int _disposed;

    public RabbitMqConnectionManager(RabbitMqPublishingConfiguration configuration, IServiceProvider serviceProvider)
        : this(
            configuration,
            cancellationToken =>
            {
                var connectionFactory = (configuration ?? throw new ArgumentNullException(nameof(configuration)))
                                       .ConnectionFactoryFactory?.Invoke(
                                            serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider))
                                        ) ??
                                        throw new MessageTopologyValidationException(
                                            ["A RabbitMQ connection factory must be configured."]
                                        );

                return connectionFactory.CreateConnectionAsync(cancellationToken);
            },
            (serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider))).GetService(
                typeof(ILoggerFactory)
            ) as
            ILoggerFactory
        ) { }

    public RabbitMqConnectionManager(
        RabbitMqPublishingConfiguration configuration,
        Func<CancellationToken, Task<IConnection>> createConnectionAsync,
        ILoggerFactory? loggerFactory = null
    )
    {
        if (configuration is null)
        {
            throw new ArgumentNullException(nameof(configuration));
        }

        _ = configuration.ConnectionFactoryFactory ??
            throw new MessageTopologyValidationException(["A RabbitMQ connection factory must be configured."]);
        _createConnectionAsync =
            createConnectionAsync ?? throw new ArgumentNullException(nameof(createConnectionAsync));
        _logger =
            (loggerFactory ?? NullLoggerFactory.Instance).CreateLogger<RabbitMqConnectionManager>();
        var (worstCaseChannelCount, description) = RabbitMqChannelBudget.Calculate(configuration);
        _worstCaseChannelCount = worstCaseChannelCount;
        _worstCaseChannelCountDescription = description;
    }

    public async ValueTask DisposeAsync()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0)
        {
            return;
        }

        _gate.Dispose();

        if (_connectionTask is not null && _connectionTask.Status == TaskStatus.RanToCompletion)
        {
            var connection = await _connectionTask.ConfigureAwait(false);
            if (connection is IAsyncDisposable asyncDisposable)
            {
                await asyncDisposable.DisposeAsync().ConfigureAwait(false);
            }
            else
            {
                connection.Dispose();
            }
        }
    }

    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0)
        {
            return;
        }

        _gate.Dispose();

        if (_connectionTask is not null && _connectionTask.Status == TaskStatus.RanToCompletion)
        {
            _connectionTask.Result.Dispose();
        }
    }

    public async Task<IConnection> GetConnectionAsync(CancellationToken cancellationToken)
    {
        ThrowIfDisposed();

        var existingTask = _connectionTask;

        if (existingTask is not null)
        {
            return await existingTask.ConfigureAwait(false);
        }

        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            ThrowIfDisposed();

            if (_connectionTask is null)
            {
                _connectionTask = CreateValidatedConnectionAsync(cancellationToken);
            }

            return await _connectionTask.ConfigureAwait(false);
        }
        finally
        {
            _gate.Release();
        }
    }

    private async Task<IConnection> CreateValidatedConnectionAsync(CancellationToken cancellationToken)
    {
        var connection = await _createConnectionAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            ValidateChannelCapacity(connection);
            return connection;
        }
        catch
        {
            if (connection is IAsyncDisposable asyncDisposable)
            {
                await asyncDisposable.DisposeAsync().ConfigureAwait(false);
            }
            else
            {
                connection.Dispose();
            }

            throw;
        }
    }

    private void ValidateChannelCapacity(IConnection connection)
    {
        if (_worstCaseChannelCount == 0 || connection.ChannelMax == 0)
        {
            return;
        }

        if (_worstCaseChannelCount <= connection.ChannelMax)
        {
            _logger.LogDebug(
                "RabbitMQ channel budget validated against broker channel_max {ChannelMax}: {Description}",
                connection.ChannelMax,
                _worstCaseChannelCountDescription
            );
            return;
        }

        throw new MessageTopologyValidationException(
            new List<string>
            {
                $"RabbitMQ publish topology may open up to {_worstCaseChannelCount} channels ({_worstCaseChannelCountDescription}), but the broker negotiated channel_max={connection.ChannelMax}."
            }
        );
    }

    private void ThrowIfDisposed()
    {
        if (Volatile.Read(ref _disposed) != 0)
        {
            throw new ObjectDisposedException(nameof(RabbitMqConnectionManager));
        }
    }
}
