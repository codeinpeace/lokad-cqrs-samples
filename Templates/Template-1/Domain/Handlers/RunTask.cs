using Domain.Messages;
using Lokad.Cqrs;

namespace Domain.Handlers
{
    public class RunTask : Define.Handle<RunTaskCommand>
    {
        public void Handle(RunTaskCommand message)
        {

        }
    }
}
