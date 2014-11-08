using System;
using System.Text;

namespace MESI_Simulator
{
    class Line
    {
        public Line(int lineSize) : this(lineSize, MESIState.I)
        {
        }
 
        public Line(int lineSize, MESIState state)
        {
            _data = new byte[lineSize];
            State = state;
        }
        public MESIState State { get; set; }
        public AlignedAddress Address { get; private set; }
        private readonly byte[] _data;

        public void Set(AlignedAddress address, byte[] data)
        {
            Address = address;
            Array.Copy(data, _data, Math.Min(data.Length, _data.Length));
        }

        public void Write(byte[] data, int offset)
        {
            Array.Copy(data, 0, _data, offset, Math.Min(data.Length, _data.Length - offset));
            if (State == MESIState.I)
                State = MESIState.S;
        }

        public override string ToString()
        {
            var sb = new StringBuilder().AppendFormat("0x{0} ({1}) - ", Address, State);
            foreach (var b in _data) sb.AppendFormat("{0:x}", b);
            return sb.ToString();
        }

        public void Reset(AlignedAddress alignedAddress)
        {
            State= MESIState.I;
            Address = alignedAddress;
        }

        public byte[] GetData()
        {
            return (byte[]) _data.Clone();
        }
    }
}