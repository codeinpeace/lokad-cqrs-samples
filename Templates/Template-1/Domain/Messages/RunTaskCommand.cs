#region (c) 2010-2011 Lokad CQRS - New BSD License 
// Copyright (c) Lokad SAS 2010-2011 (http://www.lokad.com)
// This code is released as Open Source under the terms of the New BSD Licence
// Homepage: http://lokad.github.com/lokad-cqrs/
#endregion

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
