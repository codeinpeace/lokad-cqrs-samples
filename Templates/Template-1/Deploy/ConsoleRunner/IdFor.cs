#region (c) 2010-2011 Lokad CQRS - New BSD License 
// Copyright (c) Lokad SAS 2010-2011 (http://www.lokad.com)
// This code is released as Open Source under the terms of the New BSD Licence
// Homepage: http://lokad.github.com/lokad-cqrs/
#endregion
namespace ConsoleRunner
{
    /// <summary>
    /// Some hard-coded identifiers
    /// </summary>
    public static class IdFor
    {
        public const string StorageConnectionValueName = "StorageConnectionString";
        public const string EventsQueue = "template-events";
        public const string CommandsQueue = "template-commands";
        public const string ErrorBlob = "template-errors";
    }
}