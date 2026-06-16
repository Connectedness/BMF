using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Bmf.Core.Messaging.Inbound;

public sealed class MessageDeserializationMiddleware : IMessageMiddleware
{
    public async Task InvokeAsync(IncomingMessageContext context, MessageDelegate next)
    {
        if (context is null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        if (next is null)
        {
            throw new ArgumentNullException(nameof(next));
        }

        if (context.Message is null)
        {
            var deserializer = (IMessageDeserializer) context.Services.GetRequiredService(
                context.Endpoint.DeserializerType
            );
            context.Message = await deserializer
               .DeserializeAsync(context, context.CancellationToken)
               .ConfigureAwait(false);
        }

        await next(context).ConfigureAwait(false);
    }
}
