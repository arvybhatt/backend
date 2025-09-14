using System.Collections.Concurrent;

namespace Backend.Services
{
    public static class ServiceBusMessageStore
    {
        public static ConcurrentQueue<string> Messages { get; } = new ConcurrentQueue<string>();
    }
}
