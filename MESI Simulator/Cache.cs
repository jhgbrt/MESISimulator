using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace MESI_Simulator
{
    public interface ICache
    {
        Task<byte[]> Read(uint address);
        Task<byte[]> ReadExclusive(uint address);
        Task Store(byte[] result, uint address);
    }

    public class Cache : ICache
    {
        private readonly Bus _bus;
        private readonly int _id;
        private readonly int _lineSize;
        private readonly Line[] _lines;

        public Cache(Bus bus, int id, int nofCacheLines, int lineSize)
        {
            _bus = bus;
            _id = id;
            _lineSize = lineSize;

            _bus.Connect(this);
             
            _lines = new Line[nofCacheLines];

            for (int i = 0; i < nofCacheLines; i++)
                _lines[i] = new Line(lineSize);
        }

        public async Task<byte[]> Read(uint address)
        {
            var alignedAddress = address.Align();
            var cacheLine = await GetCacheLineFor(address);

            Console.WriteLine("{0} Read address {1}. Aligned address = {2}, cache line state = {3}", this, address, cacheLine.Address, cacheLine.State);

            if (cacheLine.State == MESIState.I)
            {
                Console.WriteLine("{0} - CACHE MISS", this);
                var readMessage = Message.Read(this, alignedAddress);
                await _bus.SendAsync(readMessage);
                var response = _bus.GetResponses(readMessage).First();
                cacheLine.State = MESIState.S;
                cacheLine.Set(response.Address, response.Data);
            }


            return cacheLine.GetData();
        }

        public async Task<byte[]> ReadExclusive(uint address)
        {
            var cacheLine = await GetCacheLineFor(address);
            Console.WriteLine("{0} Read address {1}. Aligned address = {2}, cache line state = {3}", this, address, cacheLine.Address, cacheLine.State);

            switch (cacheLine.State)
            {
                case MESIState.I:
                {
                    Console.WriteLine("{0} - CACHE MISS", this);
                    await ReadInvalidate(cacheLine);
                    break;
                }
                case MESIState.M:
                {
                    await Writeback(cacheLine);
                    break;
                }
            }

            Debug.Assert(cacheLine.State == MESIState.E);
            return cacheLine.GetData();
        }

        public async Task Store(byte[] result, uint address)
        {
            var cacheLine = await GetCacheLineFor(address);

            await ReadInvalidate(cacheLine);

            int offset = (int) (address - cacheLine.Address);
            cacheLine.Write(result, offset);

            await Writeback(cacheLine);
        }

        public async Task<bool> OnMessage(object sender, Message message)
        {
            if (message.Sender == this) return false;
            Console.WriteLine("{0} Receives 0x{1:x8} - {2}", this, message.Address, message.MessageType);
            var cacheLine = await GetCacheLineFor(message.Address);
            Console.WriteLine("{0}          0x{1:x8} - {2} ({3})", this, message.Address, message.MessageType, cacheLine.State);
            
            switch (message.MessageType)
            {
                case MESIMessage.READ:
                {
                    // a read copy of this cache line is requested by another cache. 
                    // if we modified it, first write back to memory
                    // if we have it exclusively, the line becomes shared
                    if (cacheLine.State == MESIState.M)
                    {
                        await Writeback(cacheLine);
                    }
                    if (cacheLine.State == MESIState.E)
                    {
                        cacheLine.State = MESIState.S;
                    }
                    if (cacheLine.State == MESIState.S)
                    {
                        message.ResponseSent = true;
                        _bus.Respond(message, Message.ReadResponse(this, cacheLine.Address, cacheLine.GetData()));
                        return true;
                    }
                }
                    break;
                case MESIMessage.READ_RESPONSE:
                    break;
                case MESIMessage.INVALIDATE:
                    cacheLine.State = MESIState.I;
                    _bus.Respond(message, Message.InvalidateAcknowledge(this, message.Address));
                    break;
                case MESIMessage.INVALIDATE_ACKNOWLEDGE:
                    break;
                case MESIMessage.READ_INVALIDATE:
                {
                    // a read copy of this cache line is requested by another cache for exclusive access
                    // if we modified it, first write back to memory
                    if (cacheLine.State == MESIState.M)
                    {
                        await Writeback(cacheLine);
                    }
                    if (cacheLine.State == MESIState.S
                    || cacheLine.State == MESIState.E)
                    {
                        message.ResponseSent = true;
                        _bus.Respond(message, Message.ReadResponse(this, cacheLine.Address, cacheLine.GetData()));
                    }
                    cacheLine.State = MESIState.I;
                    _bus.Respond(message, Message.InvalidateAcknowledge(this, cacheLine.Address));
                }
                    break;
                case MESIMessage.WRITEBACK:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return false;
        }

        private async Task<Line> GetCacheLineFor(uint address)
        {
            var alignedAddress = address.Align();
            var hashSlot = alignedAddress.GetHashSlot();
            var cacheLine = _lines[hashSlot];
            
            // if the cache line at this slot does not correspond to the address asked, invalidate it
            // if the line is modified, it must be written back to memory first
            if (alignedAddress != cacheLine.Address)
            {
                if (cacheLine.State == MESIState.M)
                    await Writeback(cacheLine);
                cacheLine.Reset(alignedAddress);
            }

            return cacheLine;
        }

        public override string ToString()
        {
            return string.Format("CPU {0} CACHE", _id);
        }

        /// <summary>
        /// request all other parties that have this line in cache to invalidate it
        /// </summary>
        /// <param name="line"></param>
        private async Task Invalidate(Line line)
        {
            if (line.State == MESIState.M || line.State == MESIState.E)
                return;
            var invalidate = Message.Invalidate(this, line.Address);
            await _bus.SendAsync(invalidate);
            var acks = _bus.GetResponsesOfType(invalidate, MESIMessage.INVALIDATE_ACKNOWLEDGE);
            Debug.Assert(acks.Length == _bus.NofConnectedParties - 1);
            line.State = MESIState.E;
        }

        private async Task ReadInvalidate(Line line)
        {
            var readInvalidate = Message.ReadInvalidate(this, line.Address);

            await _bus.SendAsync(readInvalidate);
            
            var acks = _bus.GetResponsesOfType(readInvalidate, MESIMessage.INVALIDATE_ACKNOWLEDGE);
            Debug.Assert(acks.Length == _bus.NofConnectedParties - 1);

            var response = _bus.GetResponsesOfType(readInvalidate, MESIMessage.READ_RESPONSE).First();
            line.Set(response.Address, response.Data);
            line.State = MESIState.E;
        }

        private async Task Writeback(Line line)
        {
            var writeback = Message.Writeback(this, line.Address, line.GetData());
            await _bus.SendAsync(writeback);
            line.State = MESIState.E;
        }
    }

    public static class Extensions
    {
        public static uint GetHashSlot(this uint value)
        {
            return (value & 0xF0) >> 4;
        }

        public static uint Align(this uint value)
        {
            return value & 0xFFFFFFF0;
        }
    }
}