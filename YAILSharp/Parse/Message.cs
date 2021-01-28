namespace YAILSharp
{
    public class Message
    {
        public Line Line { get; }
        public Type MessageType { get; }

        public Message(Line line, Type type)
        {
            this.Line = line;
            this.MessageType = type;
        }
        
        public enum Type
        {
            Raw,
            PrivateMessage,
            ChannelMessage,
            Notice,
            Motd,
            EndOfMotd,
            Ping,
            Join,
            Part,
        }
    }
}