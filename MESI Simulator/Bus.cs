using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MESI_Simulator
{
    public class Bus
    {
        public async Task SendAsync(Message message)
        {
            Console.WriteLine("{0} Sends 0x{2:x8} - {1}", message.Sender, message.MessageType, message.Address);

            _responses[message] = new ConcurrentBag<Message>();

            var l1Tasks = (
                from handler in _l1Handlers
                select Task.Run(() => handler(message.Sender, message))
                ).ToArray();

            await Task.WhenAll(l1Tasks);

            var l2Tasks = (
                from handler in _l2Handlers
                select Task.Run(() => handler(message.Sender, message))
                ).ToArray();

            await Task.WhenAll(l2Tasks);
        }

        private ConcurrentDictionary<Message, ConcurrentBag<Message>> _responses = new ConcurrentDictionary<Message, ConcurrentBag<Message>>();

        public void Respond(Message message, Message response)
        {
            Console.WriteLine("{0} Responds 0x{2:x8} - {1}", response.Sender, response.MessageType, response.Address);
            _responses[message].Add(response);
        }

        private List<Cache> _caches = new List<Cache>();
        private Memory _memory;

        public void Connect(Cache cache)
        {
            _caches.Add(cache);
            _l1Handlers.Add(cache.OnMessage);
        }

        public void Connect(Memory memory)
        {
            _memory = memory;
            _l2Handlers.Add(_memory.OnMessage);
        }

        private  delegate Task<bool> MessageHandler(object sender, Message message);

        private readonly List<MessageHandler> _l1Handlers = new List<MessageHandler>();
        private readonly List<MessageHandler> _l2Handlers = new List<MessageHandler>();

        public int NofConnectedParties
        {
            get
            {
                return _l1Handlers.Count + _l2Handlers.Count;
            }
        }
        public Message[] GetResponses(Message message)
        {
            var responses = _responses[message];
            return responses.ToArray();
        }


        public Message[] GetResponsesOfType(Message message, MESIMessage type)
        {
            var responses = _responses[message];
            return responses.Where(m => m.MessageType == type).ToArray();
        }
    }
}