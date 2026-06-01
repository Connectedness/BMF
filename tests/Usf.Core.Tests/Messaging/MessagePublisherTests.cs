using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Usf.Core.Messaging;
using Usf.Core.Messaging.Errors;
using Usf.Core.Messaging.Serialization;
using Usf.Core.Tests.Messaging.TestSupport;
using Xunit;

namespace Usf.Core.Tests.Messaging;

public sealed class MessagePublisherTests
{
    [Fact]
    public async Task PublishMessageAsync_UsesTopologyResolvedTarget_WhenNoExplicitTargetIsProvided()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var target = new RecordingTarget<SampleMessage>("default", CloudEventsTestFactory.CreateSerializer());
        var topology = new OutboundTopology(
            new Dictionary<Type, OutboundTarget>
            {
                [typeof(SampleMessage)] = target
            },
            new Dictionary<string, OutboundTarget>(StringComparer.Ordinal)
        );
        var publisher = new MessagePublisher(topology, CloudEventsTestFactory.CreateRegistry());
        var message = new SampleMessage("hello");

        await publisher.PublishMessageAsync(message, cancellationToken: cancellationToken);

        target.Messages.Should().ContainSingle().Which.Should().Be(message);
        var envelope = target.CloudEventEnvelopes.Should().ContainSingle().Which;
        Encoding.UTF8.GetString(envelope.Data).Should().Be("{\"Value\":\"hello\"}");
        envelope.Type.Should().Be(CloudEventsTestFactory.SampleDiscriminator);
        envelope.Source.Should().Be("/tests/core");
        envelope.DataContentType.Should().Be("application/json");
    }

    [Fact]
    public async Task PublishMessageAsync_UsesExplicitTarget_WhenProvided()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var explicitTarget = new RecordingTarget<SampleMessage>("explicit", CloudEventsTestFactory.CreateSerializer());
        var publisher = new MessagePublisher(new EmptyOutboundTopology(), CloudEventsTestFactory.CreateRegistry());
        var message = new SampleMessage("hello");

        await publisher.PublishMessageAsync(message, explicitTarget, cancellationToken);

        explicitTarget.Messages.Should().ContainSingle().Which.Should().Be(message);
        Encoding.UTF8.GetString(explicitTarget.CloudEventEnvelopes.Should().ContainSingle().Which.Data)
           .Should()
           .Be("{\"Value\":\"hello\"}");
    }

    [Fact]
    public async Task PublishMessageAsync_UsesExplicitMetadata_ForMessagesThatCannotImplementICloudEvent()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var registry = CloudEventsTestFactory.CreateRegistry(
            new KeyValuePair<Type, string>(typeof(ThirdPartyMessage), "tests.third-party")
        );
        var serializer = new CloudEventMessageSerializer(
            registry,
            new Utf8JsonPayloadCodec(),
            new CloudEventsOptions { Source = "/tests/core" }
        );
        var target = new RecordingTarget<ThirdPartyMessage>("third-party", serializer);
        var publisher = new MessagePublisher(new EmptyOutboundTopology(), registry);
        CloudEventMetadata metadata = new (
            Guid.Parse("f39b562b-b846-48e6-a693-4108015e7c82"),
            new DateTimeOffset(2026, 5, 31, 12, 34, 56, TimeSpan.Zero),
            "subject"
        );

        await publisher.PublishMessageAsync(
            new ThirdPartyMessage("hello"),
            in metadata,
            target,
            cancellationToken
        );

        var envelope = target.CloudEventEnvelopes.Should().ContainSingle().Which;
        envelope.Id.Should().Be("f39b562b-b846-48e6-a693-4108015e7c82");
        envelope.Subject.Should().Be("subject");
        envelope.Type.Should().Be("tests.third-party");
    }

    [Fact]
    public async Task PublishMessageAsync_RejectsNullMessages()
    {
        var publisher = new MessagePublisher(new EmptyOutboundTopology(), CloudEventsTestFactory.CreateRegistry());
        var metadata = default(CloudEventMetadata);

        var action = async () => await publisher.PublishMessageAsync<string>(null!, in metadata);

        await action.Should().ThrowAsync<ArgumentNullException>().WithParameterName("message");
    }

    [Fact]
    public async Task PublishMessageAsync_ThrowsWhenNoTargetIsConfigured()
    {
        var publisher = new MessagePublisher(new EmptyOutboundTopology(), CloudEventsTestFactory.CreateRegistry());

        var action = async () => await publisher.PublishMessageAsync(new SampleMessage("hello"));

        await action.Should().ThrowAsync<OutboundTargetNotFoundException>();
    }

    [Fact]
    public async Task PublishMessageAsync_ThrowsWhenExplicitTargetDoesNotMatchMessageType()
    {
        var target = new RecordingTarget<OtherMessage>("other", CloudEventsTestFactory.CreateSerializer());
        var publisher = new MessagePublisher(new EmptyOutboundTopology(), CloudEventsTestFactory.CreateRegistry());

        var action = async () => await publisher.PublishMessageAsync(new SampleMessage("hello"), target);

        await action.Should().ThrowAsync<OutboundTargetTypeMismatchException>();
    }

    [Fact]
    public async Task PublishRawAsync_PublishesSerializedMessageWithoutInvokingSerializer()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var target = new RecordingTarget<SampleMessage>(
            "raw",
            new ThrowingSerializer(new InvalidOperationException("serializer should not run"))
        );
        var publisher = new MessagePublisher(new EmptyOutboundTopology(), CloudEventsTestFactory.CreateRegistry());
        SerializedMessage message = new (
            "prepared"u8.ToArray(),
            "application/custom",
            "utf-8",
            new Dictionary<string, string?>(StringComparer.Ordinal)
            {
                ["tenant"] = "a"
            },
            "message-id",
            "correlation-id"
        );

        await publisher.PublishRawAsync(message, target, cancellationToken);

        target.Messages.Should().BeEmpty();
        target.SerializedMessages.Should().ContainSingle().Which.Should().Be(message);
    }

    [Fact]
    public async Task PublishRawAsync_RejectsMessagesWithoutABody()
    {
        var target = new RecordingTarget<SampleMessage>("raw", CloudEventsTestFactory.CreateSerializer());
        var publisher = new MessagePublisher(new EmptyOutboundTopology(), CloudEventsTestFactory.CreateRegistry());

        var action = async () => await publisher.PublishRawAsync(default, target);

        await action.Should().ThrowAsync<ArgumentException>().WithParameterName("message");
        target.SerializedMessages.Should().BeEmpty();
    }

    [Fact]
    public async Task PublishMessageAsync_TagsDeliveryFailureReason()
    {
        var measurements = new List<KeyValuePair<string, object?>[]>();
        using var listener = new MeterListener();
        listener.InstrumentPublished = (instrument, meterListener) =>
        {
            if (instrument.Name == "usf.outbound.publish.failures")
            {
                meterListener.EnableMeasurementEvents(instrument);
            }
        };
        listener.SetMeasurementEventCallback<long>((_, _, tags, _) => measurements.Add(tags.ToArray()));
        listener.Start();

        var deliveryException = new MessageDeliveryException(
            "target",
            MessageDeliveryFailureReason.Returned,
            new InvalidOperationException("returned")
        );
        var target = new ThrowingTarget<SampleMessage>(
            "target",
            CloudEventsTestFactory.CreateSerializer(),
            deliveryException
        );
        var publisher = new MessagePublisher(new EmptyOutboundTopology(), CloudEventsTestFactory.CreateRegistry());

        var action = async () => await publisher.PublishMessageAsync(new SampleMessage("hello"), target);

        await action.Should().ThrowAsync<MessageDeliveryException>();
        measurements.Should().ContainSingle();
        measurements[0].Should().Contain(
            new KeyValuePair<string, object?>(OutboundDiagnostics.OutcomeTagName, "failure")
        );
        measurements[0].Should().Contain(
            new KeyValuePair<string, object?>(OutboundDiagnostics.DeliveryFailureReasonTagName, "returned")
        );
        measurements[0].Should().Contain(
            new KeyValuePair<string, object?>(
                OutboundDiagnostics.MessageTypeTagName,
                CloudEventsTestFactory.SampleDiscriminator
            )
        );
    }
}
