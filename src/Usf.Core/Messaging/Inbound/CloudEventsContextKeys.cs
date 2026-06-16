namespace Usf.Core.Messaging.Inbound;

public static class CloudEventsContextKeys
{
    public static MessageContextKey<CloudEventEnvelope> Envelope { get; } = new ("cloudevents.envelope");
}
