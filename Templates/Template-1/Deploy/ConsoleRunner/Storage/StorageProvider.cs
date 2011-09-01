using Lokad.Cqrs.Feature.StreamingStorage;

namespace ConsoleRunner.Storage
{
    public sealed class StorageProvider
    {
        public readonly IStreamingRoot Local;
        public readonly IStreamingRoot Remote;

        public StorageProvider(IStreamingRoot local, IStreamingRoot remote)
        {
            Local = local;
            Remote = remote;
        }
    }
}