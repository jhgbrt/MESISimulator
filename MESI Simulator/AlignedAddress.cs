namespace MESI_Simulator
{
    public struct AlignedAddress
    {
        public readonly uint Value;

        public AlignedAddress(uint value)
            : this()
        {
            Value = value.Align();
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public uint GetHashSlot()
        {
            return Value.GetHashSlot();
        }

        public uint OffsetFor(MemoryAddress address)
        {
            return address.Value - Value;
        }

        public static bool operator ==(AlignedAddress left, AlignedAddress right)
        {
            return left.Value == right.Value;
        }

        public static bool operator !=(AlignedAddress left, AlignedAddress right)
        {
            return !(left == right);
        }

        public override string ToString()
        {
            return string.Format("0x{0:x8}", Value);
        }
    }
}