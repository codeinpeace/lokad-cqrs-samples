using Lokad.Cqrs.Feature.HandlerClasses;

namespace Domain.Design
{
    public interface IBusClient
    {
        void Send(params IMessage[] messages);
    }
}
