using System;
using System.Collections.Generic;
using FluentAssertions;
using Usf.Core.Messaging;
using Usf.Core.Messaging.Errors;
using Usf.Core.Tests.Messaging.TestSupport;
using Xunit;

namespace Usf.Core.Tests.Messaging;

public sealed class MessageContractOutboundTopologyValidatorTests
{
    [Fact]
    public void Validate_ReportsEveryTypedTargetWithoutCanonicalDiscriminator()
    {
        var first = new RecordingTarget<SampleMessage>("first", CloudEventsTestFactory.CreateSerializer());
        var second = new RecordingTarget<SampleMessage>("second", CloudEventsTestFactory.CreateSerializer());
        var topology = new OutboundTopology(
            new Dictionary<Type, OutboundTarget>
            {
                [typeof(SampleMessage)] = first
            },
            new Dictionary<string, OutboundTarget>(StringComparer.Ordinal)
            {
                ["second"] = second
            }
        );
        var validator = new MessageContractOutboundTopologyValidator(
            topology,
            new MessageContractRegistry(
                new Dictionary<Type, string>(),
                new Dictionary<string, Type>(),
                new Dictionary<Type, string>()
            )
        );

        Action action = validator.Validate;

        var exception = action.Should().Throw<OutboundTopologyValidationException>().Which;
        exception.ValidationErrors.Should().Equal(
            "Outbound target 'first' publishes unregistered CloudEvents message type 'Usf.Core.Tests.Messaging.TestSupport.SampleMessage'. Register its canonical discriminator with MessageContractRegistryBuilder.Map<T>(...) or MapOutbound<T>(...).",
            "Outbound target 'second' publishes unregistered CloudEvents message type 'Usf.Core.Tests.Messaging.TestSupport.SampleMessage'. Register its canonical discriminator with MessageContractRegistryBuilder.Map<T>(...) or MapOutbound<T>(...)."
        );
    }

    [Fact]
    public void Validate_IgnoresRawOnlyTargets()
    {
        var topology = new OutboundTopology(
            new Dictionary<Type, OutboundTarget>(),
            new Dictionary<string, OutboundTarget>(StringComparer.Ordinal)
            {
                ["raw"] = new RawOutboundTarget()
            }
        );
        var validator = new MessageContractOutboundTopologyValidator(
            topology,
            new MessageContractRegistry(
                new Dictionary<Type, string>(),
                new Dictionary<string, Type>(),
                new Dictionary<Type, string>()
            )
        );

        validator.Validate();
    }

    private sealed class RawOutboundTarget : OutboundTarget
    {
        public RawOutboundTarget()
            : base("raw", "test") { }

        public override System.Threading.Tasks.Task PublishSerializedAsync(
            SerializedMessage message,
            System.Threading.CancellationToken cancellationToken = default
        )
        {
            return System.Threading.Tasks.Task.CompletedTask;
        }
    }
}
