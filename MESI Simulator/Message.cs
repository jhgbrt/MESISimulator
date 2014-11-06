namespace MESI_Simulator
{
    public class Message
    {
        Message(object sender, uint address, MESIMessage messageType, byte[] data)
        {
            Sender = sender;
            Data = data;
            MessageType = messageType;
            Address = address;
        }

        public MESIMessage MessageType { get; private set; }

        public byte[] Data { get; private set; }

        public bool ResponseSent { get; set; }
        
        public uint Address { get; private set; }

        public object Sender { get; private set; }

        public static Message Read(object sender, uint address)
        {
            return new Message(sender, address, MESIMessage.READ, null);
        }

        public static Message ReadResponse(object sender, uint addres, byte[] data)
        {
            return new Message(sender, addres, MESIMessage.READ_RESPONSE, data);
        }

        public static Message ReadInvalidate(object sender, uint address)
        {
            return new Message(sender, address, MESIMessage.READ_INVALIDATE, null);
        }

        public static Message Invalidate(object sender, uint address)
        {
            return new Message(sender, address, MESIMessage.INVALIDATE, null);
        }

        public static Message InvalidateAcknowledge(object sender, uint address)
        {
            return new Message(sender, address, MESIMessage.INVALIDATE_ACKNOWLEDGE, null);
        }

        public static Message Writeback(object sender, uint address, byte[] data)
        {
            return new Message(sender, address, MESIMessage.WRITEBACK, data);
        }
    }
}