namespace MESI_Simulator
{
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