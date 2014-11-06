using System;

namespace MESI_Simulator
{
    internal class Crash : Exception
    {
        public Crash(string message)
            : base(message)
        {
        }
    }
}