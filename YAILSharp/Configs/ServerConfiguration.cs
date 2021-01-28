namespace YAILSharp
{
    public class ServerConfiguration
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public bool UseTls { get; set; }
        
        public string Nickname { get; set; }
        public string Username { get; set; }
        public string Realname { get; set; }
    }
}