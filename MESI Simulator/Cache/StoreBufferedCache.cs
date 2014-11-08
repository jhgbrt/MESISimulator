using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace MESI_Simulator
{
    class StoreBufferedCache : ICache
    {
        private ICache _cache;
        private readonly int _id;

        public StoreBufferedCache(ICache cache, int id)
        {
            _cache = cache;
            _id = id;
        }

        public Task<byte[]> Read(MemoryAddress address)
        {
            byte[] result;
            if (TryGetPendingResult(address, out result))
                return Task.FromResult(result);
            return _cache.Read(address);
        }

        public Task<byte[]> ReadExclusive(MemoryAddress address)
        {
            byte[] result;
            if (TryGetPendingResult(address, out result))
                return Task.FromResult(result);
            return _cache.ReadExclusive(address);
        }

        private bool TryGetPendingResult(MemoryAddress address, out byte[] result)
        {
            var found = false;
            result = null;
            byte[] pending;
            if (_pendingStores.TryGetValue(address, out pending))
            {
                var line = new Line(16);
                var alignedAddress = address.Align();
                var offset = (int)alignedAddress.OffsetFor(address);
                line.Write(pending, offset);
                result = line.GetData();
                found = true;
            }
            return found;
        }

        private Task _bufferedStore;

        public Task Store(byte[] result, MemoryAddress address)
        {
            _bufferedStore = BufferedStore(result, address);
            return Task.FromResult(true);
        }

        private ConcurrentDictionary<MemoryAddress, byte[]> _pendingStores = new ConcurrentDictionary<MemoryAddress, byte[]>();

        private async Task BufferedStore(byte[] result, MemoryAddress address)
        {
            Console.WriteLine("CPU {1} STOREBUFFER - Recording STORE for address {0}", address, _id);
            _pendingStores.AddOrUpdate(address, result, (x, y) => result);
            await _cache.Store(result, address);
            byte[] ignored;
            _pendingStores.TryRemove(address, out ignored);
            Console.WriteLine("CPU {1} STOREBUFFER - STORE completed for address {0}", address, _id);
        }
    }
}