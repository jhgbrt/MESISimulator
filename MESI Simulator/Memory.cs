using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MESI_Simulator
{
    public class Memory
    {
        private readonly Bus _bus;
        private readonly IDictionary<uint, Line> _data = new Dictionary<uint, Line>();

        public Memory(Bus bus, int size, int lineSize)
        {
            _bus = bus;
            _bus.Connect(this);

            for (int i = 0; i < size; i += lineSize)
            {
                var line = new Line(lineSize, MESIState.S);
                _data[(uint)i] = line;
            }
        }

        public async Task<bool> OnMessage(object sender, Message message)
        {
            if (message.Sender == this)
                return false;

            Console.WriteLine("{0} Receives 0x{1:x8} - {2}", this, message.Address, message.MessageType);
            var alignedAddress = message.Address.Align();
            var line = _data[alignedAddress];

            switch (message.MessageType)
            {
                case MESIMessage.READ:
                    {
                        if (message.ResponseSent)
                            return false;
                        if (line.State == MESIState.I)
                            throw new Crash("READ request for line that is marked INVALID in memory");
                        await Stall();
                        SendReadResponse(message, line, alignedAddress);
                        return true;
                    }
                case MESIMessage.INVALIDATE:
                    line.State = MESIState.I;
                    _bus.Respond(message, Message.InvalidateAcknowledge(this, line.Address));
                    break;
                case MESIMessage.READ_INVALIDATE:
                    _bus.Respond(message, Message.InvalidateAcknowledge(this, message.Address));
                    if (message.ResponseSent)
                    {
                        line.State = MESIState.I;
                        return false;
                    }
                    if (line.State == MESIState.I) throw new Crash("READ request for line that is marked INVALID in memory");
                    await Stall();
                    SendReadResponse(message, line, alignedAddress);
                    break;
                case MESIMessage.WRITEBACK:
                    {
                        await Stall();
                        line.Write(message.Data, 0);
                    }
                    break;
                case MESIMessage.READ_RESPONSE:
                case MESIMessage.INVALIDATE_ACKNOWLEDGE:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return false;
        }

        private void SendReadResponse(Message message, Line line, uint alignedAddress)
        {
            var bytes = line.GetData();
            message.ResponseSent = true;
            _bus.Respond(message, Message.ReadResponse(this, alignedAddress, bytes));
        }

        private static async Task Stall()
        {
            Console.WriteLine("...");
            await Task.Delay(1000);
        }

        public override string ToString()
        {
            return "MEMORY ";
        }
    }
}