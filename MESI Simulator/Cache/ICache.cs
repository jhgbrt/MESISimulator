using System.Threading.Tasks;

namespace MESI_Simulator
{
    public interface ICache
    {
        Task<byte[]> Read(MemoryAddress address);
        Task<byte[]> ReadExclusive(MemoryAddress address);
        Task Store(byte[] result, MemoryAddress address);
    }
}