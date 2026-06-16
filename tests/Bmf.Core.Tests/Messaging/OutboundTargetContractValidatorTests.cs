using System;
using System.Collections.Generic;
using FluentAssertions;
using Bmf.Core.Messaging;
using Bmf.Core.Messaging.Outbound;
using Bmf.Core.Tests.Messaging.TestSupport;
using Xunit;

namespace Bmf.Core.Tests.Messaging;

public sealed class OutboundTargetContractValidatorTests
{
    [Fact]
    public void CollectValidationErrors_ReportsEveryTypedTargetWithoutCanonicalDiscriminator()
    {
        var registry = new MessageContractRegistry(
            new Dictionary<Type, string>(),
            new Dictionary<string, Type>(),
            new Dictionary<Type, string>()
        );
        KeyValuePair<string, Type>[] typedTargets =
        [
            new ("first", typeof(SampleMessage)),
            new ("second", typeof(SampleMessage))
        ];
        List<string> validationErrors = [];

        OutboundTargetContractValidator.CollectValidationErrors(registry, typedTargets, validationErrors);

        validationErrors.Should().Equal(
            "Outbound target 'first' publishes unregistered CloudEvents message type 'Bmf.Core.Tests.Messaging.TestSupport.SampleMessage'. Register its canonical discriminator with MessageContractRegistryBuilder.Map<T>(...) or MapOutbound<T>(...).",
            "Outbound target 'second' publishes unregistered CloudEvents message type 'Bmf.Core.Tests.Messaging.TestSupport.SampleMessage'. Register its canonical discriminator with MessageContractRegistryBuilder.Map<T>(...) or MapOutbound<T>(...)."
        );
    }

    [Fact]
    public void CollectValidationErrors_DoesNotReportRegisteredTargets()
    {
        var registry = CloudEventsTestFactory.CreateRegistry();
        KeyValuePair<string, Type>[] typedTargets = [new ("sample", typeof(SampleMessage))];
        List<string> validationErrors = [];

        OutboundTargetContractValidator.CollectValidationErrors(registry, typedTargets, validationErrors);

        validationErrors.Should().BeEmpty();
    }
}
