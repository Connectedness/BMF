using Microsoft.Extensions.DependencyInjection;
using Usf.Core.Messaging;
using Usf.Core.Messaging.Serialization;

namespace Usf.Transport.RabbitMq.Tests.TestSupport;

public static class RabbitMqCloudEventsTestFactory
{
    public const string AuditMessageDiscriminator = "tests.rabbitmq.audit";
    public const string PublishMessageDiscriminator = "tests.rabbitmq.publish";
    public const string ValidationMessageADiscriminator = "tests.rabbitmq.validation-a";
    public const string ValidationMessageBDiscriminator = "tests.rabbitmq.validation-b";

    public static IServiceCollection AddTestCloudEvents(this IServiceCollection services)
    {
        return services.AddCloudEvents(
            options => options.Source = "/tests/rabbitmq",
            contracts =>
            {
                contracts.Map<RabbitMqAuditMessage>(AuditMessageDiscriminator);
                contracts.Map<RabbitMqPublishMessage>(PublishMessageDiscriminator)
                   .WithDataSchema("/schemas/rabbitmq-publish");
                contracts.Map<ValidationMessageA>(ValidationMessageADiscriminator);
                contracts.Map<ValidationMessageB>(ValidationMessageBDiscriminator);
            }
        );
    }

    public static IMessageContractRegistry CreateRegistry()
    {
        MessageContractRegistryBuilder builder = new ();
        builder.Map<RabbitMqAuditMessage>(AuditMessageDiscriminator);
        builder.Map<RabbitMqPublishMessage>(PublishMessageDiscriminator)
           .WithDataSchema("/schemas/rabbitmq-publish");
        builder.Map<ValidationMessageA>(ValidationMessageADiscriminator);
        builder.Map<ValidationMessageB>(ValidationMessageBDiscriminator);
        return builder.Build();
    }

    public static CloudEventMessageSerializer CreateSerializer()
    {
        return new CloudEventMessageSerializer(
            CreateRegistry(),
            new Utf8JsonPayloadCodec(),
            new CloudEventsOptions
            {
                Source = "/tests/rabbitmq"
            }
        );
    }
}
