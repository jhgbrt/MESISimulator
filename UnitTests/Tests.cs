using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using MESI_Simulator;
using NUnit.Framework;

namespace UnitTests
{
    [TestFixture]
    public class Tests
    {
        [Test]
        public void Test()
        {
            var s = new Simulator(16, 256, 256 * 1024, 2);
            byte[] bytes = new byte[1234];
            for (int i = 0; i < bytes.Length; i++)
            {
                int b = i%8;
                bytes[i] = BitConverter.GetBytes(b)[0];
            }
            int result = 0;
            s.Run(async cpus =>
                  {
                      await cpus[0].Load(new MemoryAddress(0), Register.A);
                      result = cpus[0].GetRegisterValue(Register.A);
                  });

            Console.WriteLine("{0} = {0}", result);
        }

        public void BEqualsAPlusOne()
        {
            int a = 1;
            int b = a + 1;
        }

        [Test]
        public void SetGetRegisterValue()
        {
            var s = new Simulator(16, 256, 256 * 1024, 2);
            int result = 0;
            s.Run(cpus =>
            {
                cpus[0].SetRegisterValue(Register.A, 1234);
                result = cpus[0].GetRegisterValue(Register.A);
                return Task.FromResult(0);
            });

            Assert.AreEqual(1234, result);
        }

        [Test]
        public void GetHashSlotTests()
        {
            for (uint i = 0; i < 0xF; i++)
            {
                Assert.AreEqual(0, i.Align(), string.Format("{0}.Align() returned {1}", i, i.Align()));
                Assert.AreEqual(0, i.GetHashSlot(), string.Format("{0}.GetHashSlot() returned {1}", i, i.GetHashSlot()));
            }

            for (uint i = 0; i < 0xF; i++)
            {
                var j = i + 0xABC30;
                Assert.AreEqual(0xABC30, j.Align(), string.Format("{0}.Align() returned {1}", j, j.Align()));
                Assert.AreEqual(3, j.GetHashSlot(), string.Format("{0}.GetHashSlot() returned {1}", j, j.GetHashSlot()));
            }
        }

    }

}
