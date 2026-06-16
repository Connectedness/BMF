namespace Bmf.Transport.RabbitMq.Outbound;

public enum RabbitMqOutboundRouteScenario
{
    Fanout = 0,
    Direct = 1,
    Topic = 2,
    Headers = 3
}
