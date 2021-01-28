using System;

namespace YAILSharp.Event
{
    public class MessageEventArgs : EventArgs
    {
        public Message Message { get; private set; }

        public MessageEventArgs(Message message)
        {
            this.Message = message;
        }
    }
}