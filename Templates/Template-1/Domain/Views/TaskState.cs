using System.Runtime.Serialization;
using Lokad.Cqrs;

namespace Domain.Views
{
    [DataContract]
    public class TaskState : Define.AtomicEntity
    {
    }
}
