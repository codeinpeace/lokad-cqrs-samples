using System.Runtime.Serialization;
using Lokad.Cqrs;

namespace Domain.Messages
{
    [DataContract]
    public sealed class RunTaskCommand : Define.Command
    {
        [DataMember(Order = 1)] public readonly long SolutionId;

        public RunTaskCommand(long solutionId)
        {
            SolutionId = solutionId;
        }

        // ReSharper disable UnusedMember.Local
        RunTaskCommand()
        // ReSharper restore UnusedMember.Local
        {
        }

        public override string ToString()
        {
            return string.Format("RunTask: ");
        }
    }
}
