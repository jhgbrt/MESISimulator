using System;
using System.Threading.Tasks;

namespace MESI_Simulator
{
    class NaiveStoreBufferedCache : ICache
    {
        private ICache _cache;
        private readonly int _id;

        public NaiveStoreBufferedCache(ICache cache, int id)
        {
            _cache = cache;
            _id = id;
        }

        public Task<byte[]> Read(MemoryAddress address)
        {
            return _cache.Read(address);
        }

        public Task<byte[]> ReadExclusive(MemoryAddress address)
        {
            return _cache.ReadExclusive(address);
        }

        private Task _bufferedStore;

        public Task Store(byte[] result, MemoryAddress address)
        {
            _bufferedStore = BufferedStore(result, address);
            return Task.FromResult(true);
        }

        private async Task BufferedStore(byte[] result, MemoryAddress address)
        {
            await _cache.Store(result, address);
            Console.WriteLine("CPU {1} STOREBUFFER - STORE completed for address {0}", address, _id);
        }
    }
}