using Microsoft.Extensions.DependencyInjection;
using Bmf.Core.Messaging;
using Bmf.Core.Messaging.Outbound;

namespace Bmf.Transport.RabbitMq.Tests.TestSupport;

public static class RabbitMqCloudEventsTestFactory
{
    public const string AuditMessageDiscriminator = "tests.rabbitmq.audit";
    public const string PublishMessageDiscriminator = "tests.rabbitmq.publish";
    public const string ValidationMessageADiscriminator = "tests.rabbitmq.validation-a";
    public const string ValidationMessageBDiscriminator = "tests.rabbitmq.validation-b";

    public static BmfBuilder AddTestCloudEvents(this IServiceCollection services)
    {
        return services
           .AddBmf()
           .UseCloudEvents(options => options.Source = "/tests/rabbitmq")
           .MapMessageContracts(
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
        builder
           .Map<RabbitMqPublishMessage>(PublishMessageDiscriminator)
           .WithDataSchema("/schemas/rabbitmq-publish");
        builder.Map<ValidationMessageA>(ValidationMessageADiscriminator);
        builder.Map<ValidationMessageB>(ValidationMessageBDiscriminator);
        return builder.Build();
    }

    public static CloudEventMessageSerializer CreateSerializer()
    {
        return new CloudEventMessageSerializer(
            new Utf8JsonPayloadCodec(),
            new CloudEventsOptions
            {
                Source = "/tests/rabbitmq"
            }
        );
    }
}
