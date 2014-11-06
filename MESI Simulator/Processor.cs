using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MESI_Simulator
{
    public class Processor
    {
        private readonly int _id;
        private readonly ICache  _cache;
        private readonly int[] _registers = new int[2];

        public Processor(Bus bus, int id, int nofCacheLines, int lineSize)
        {
            _id = id;
            _registers = new int[Enum.GetValues(typeof (Register)).OfType<Register>().Count()];
            _cache = new Cache(bus, id, nofCacheLines, lineSize);
            //_cache = new NaiveStoreBufferedCache(new Cache(bus, id, nofCacheLines, lineSize), id);
            //_cache = new StoreBufferedCache(new Cache(bus, id, nofCacheLines, lineSize), id);
        }

        public async Task Load(uint address, Register register)
        {
            Console.WriteLine("CPU {0} Loads value at 0x{1:x8} into register {2}.", _id, address, register);
            var data = await _cache.Read(address);
            var int32 = Convert(data, address, BitConverter.ToInt32);
            _registers[(uint)register] = int32;
            Console.WriteLine("CPU {0} Loaded value at 0x{1:x8} into register {2}. Result = {3}", _id, address, register, int32);
        }

        public async Task LoadExclusive(uint address, Register register)
        {
            Console.WriteLine("CPU {0} Loads value at 0x{1:x8} for exclusive access into register {2}.", _id, address, register);
            var data = await _cache.ReadExclusive(address);
            var int32 = Convert(data, address, BitConverter.ToInt32); 
            _registers[(uint)register] = int32;
            Console.WriteLine("CPU {0} Loaded value at 0x{1:x8} for exclusive access into register {2}. Result = {3}", _id, address, register, int32);
        }

        public void Add(Register left, Register right, Register result)
        {
            _registers[(int)result] = _registers[(int) left]+_registers[(int) right];
            Console.WriteLine("CPU summed value in all registers. Result = {0}, stored in register {1}.", _registers[(int) result], result);
        }

        public async Task Store(Register register, uint address)
        {
            Console.WriteLine("CPU {0} Store value {1} (from register {2}) at memory address 0x{3:x8}.", _id, _registers[(int) register], register, address);
            var bytes = BitConverter.GetBytes(_registers[(int)register]);
            await _cache.Store(bytes, address);
            Console.WriteLine("CPU {0} Stored value {1} (from register {2}) at memory address 0x{3:x8}.", _id, _registers[(int)register], register, address);
        }

        public int GetRegisterValue(Register register)
        {
            return _registers[(int)register];
        }
        public void SetRegisterValue(Register register, int value)
        {
            _registers[(int) register] = value;
        }

        private T Convert<T>(byte[] line, uint address, Func<byte[], int, T> convertor)
        {
            var alignedAddress = address.Align();
            var offset = (int) (address - alignedAddress);
            return convertor(line, offset);
        }
    }
}