namespace YAILSharp
{
    public class ServerConfiguration
    {
        public string Host { get; set; }
        public IPv6Mode V6Mode { get; set; } = IPv6Mode.Deny;
        public int Port { get; set; } = 6667;
        public bool UseTLS { get; set; }
        
        public UserIdentity Identity { get; set; }
        public string[] Channels { get; set; }
    }
}