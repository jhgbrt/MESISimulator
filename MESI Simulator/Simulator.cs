using System;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace MESI_Simulator
{
    public class Simulator
    {
        private Bus _bus;
        private Memory _memory;
        private static Processor[] _cpus;

        public Simulator(int nofCacheLines, int lineSize, int totalMemorySize, int nofCpus)
        {
            _bus = new Bus();

            _memory = new Memory(_bus, totalMemorySize, lineSize);

            _cpus = Enumerable.Range(0, nofCpus).Select(i => new Processor(_bus, i, nofCacheLines, lineSize)).ToArray();

        }

        public Task Run(params Func<Processor[], Task>[] programs)
        {
            var tasks = (from p in programs
                select p(_cpus)).ToArray();
            return Task.WhenAll(tasks);
        }

    }
}