using System;
using System.Threading.Tasks;
using BrilliantMessaging.GuardClauses;
using Microsoft.Extensions.DependencyInjection;

namespace BrilliantMessaging.Core.Messaging.Inbound;

/// <summary>
/// Inbound middleware that decodes the transport body into the resolved message type (using the endpoint's
/// deserializer) and stores it on the context before invoking the next stage. It is a no-op when the message has
/// already been materialized by an inspector.
/// </summary>
public sealed class MessageDeserializationMiddleware : IMessageMiddleware
{
    /// <inheritdoc />
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="context" /> or <paramref name="next" /> is <see langword="null" />.</exception>
    public async Task InvokeAsync(IncomingMessageContext context, MessageDelegate next)
    {
        context.MustNotBeNull();
        next.MustNotBeNull();

        if (context.Message is null)
        {
            var deserializer = (IMessageDeserializer) context.Services.GetRequiredService(
                context.Endpoint.DeserializerType
            );

            try
            {
                context.Message = await deserializer
                   .DeserializeAsync(context, context.CancellationToken)
                   .ConfigureAwait(false);
            }
            catch (MessageDeserializationException)
            {
                throw;
            }
            catch (OperationCanceledException exception)
            {
                if (context.CancellationToken.IsCancellationRequested)
                {
                    throw;
                }

                throw new MessageDeserializationException(context.MessageType, exception);
            }
            catch (Exception exception)
            {
                throw new MessageDeserializationException(context.MessageType, exception);
            }
        }

        await next(context).ConfigureAwait(false);
    }
}
