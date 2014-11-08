namespace MESI_Simulator
{
    public struct MemoryAddress
    {
        public readonly uint Value;

        public MemoryAddress(uint value)
            : this()
        {
            Value = value;
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public AlignedAddress Align()
        {
            return new AlignedAddress(Value);
        }

        public override string ToString()
        {
            return string.Format("0x{0:x8}", Value);
        }
    }
}