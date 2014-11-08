using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace MESI_Simulator
{
    class Program
    {
        private static void Main(string[] args)
        {

            var addressOfA = new MemoryAddress(0xAAu);
            var addressOfB = new MemoryAddress(0xBBu);
            
            Func<Processor[], Task> loadAddressOfAInCacheOfCpu1 = async cpus =>
            {
                await cpus[1].Load(addressOfA, Register.A);
            };

            Func<Processor[], Task> loadAddressOfBInCacheOfCpu0 = async cpus =>
            {
                await cpus[0].Load(addressOfB, Register.A);
            };

            Func<Processor[], Task> b_equals_a_plus_1 = async cpus =>
                                          {
                                              // a = 1
                                              await cpus[0].LoadExclusive(addressOfA, Register.A);
                                              cpus[0].SetRegisterValue(Register.A, 1);
                                              await cpus[0].Store(Register.A, addressOfA);
                                              cpus[0].SetRegisterValue(Register.B, 1);
                                              cpus[0].Add(Register.A, Register.B, Register.C);
                                              await cpus[0].Store(Register.C, addressOfB);
                                              await cpus[0].Load(addressOfA, Register.A);
                                              await cpus[0].Load(addressOfB, Register.B);
                                              var a = cpus[0].GetRegisterValue(Register.A);
                                              var b = cpus[0].GetRegisterValue(Register.B);
                                              if (a != 1)
                                                  Console.WriteLine("FAILED! a == {0} (expected 1)", a);
                                              if (b != 2)
                                                  Console.WriteLine("FAILED! b == {0} (expected 2)", b);
                                              if (a == 1 && b == 2)
                                                  Console.WriteLine("SUCCESS!");
                                          };

            Func<Processor[], Task> foo = async cpus =>
                                            {
                                                await cpus[0].LoadExclusive(addressOfA, Register.A);
                                                cpus[0].SetRegisterValue(Register.A, 1);
                                                await cpus[0].Store(Register.A, addressOfA);
                                                await cpus[0].LoadExclusive(addressOfB, Register.B);
                                                cpus[0].SetRegisterValue(Register.B, 1);
                                                await cpus[0].Store(Register.B, addressOfB);
                                            };

            Func<Processor[], Task> bar = async cpus =>
                                            {
                                                int b = 0;
                                                while (b == 0)
                                                {
                                                    await cpus[1].Load(addressOfB, Register.A);
                                                    b = cpus[1].GetRegisterValue(Register.A);
                                                }
                                                await cpus[1].Load(addressOfA, Register.A);
                                                var a = cpus[1].GetRegisterValue(Register.A);
                                                if (a == 1)
                                                    Console.WriteLine("SUCCESS!");
                                                if (a == 1)
                                                    Console.WriteLine("FAILED!");
                                            };


            var s = new Simulator(16, 16, 16 * 1024, 2);
            
            var t = s.Run(loadAddressOfAInCacheOfCpu1, loadAddressOfBInCacheOfCpu0);
            t.Wait();

            Console.WriteLine();
            Console.WriteLine("====================");
            Console.WriteLine("Program initialized.");
            Console.WriteLine("====================");
            Console.WriteLine();
            Console.ReadLine();
            Console.Clear();
            
            s.Run(b_equals_a_plus_1).Wait();
            //s.Run(foo, bar).Wait();
            Console.ReadLine();
        }
    }
}
