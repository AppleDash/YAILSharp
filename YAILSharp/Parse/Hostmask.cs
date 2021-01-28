namespace YAILSharp
{
    public class Hostmask
    {
        public string Nickname { get; set; }
        public string Username { get; set; }
        public string Hostname { get; set; }
        
        public string Raw { get; set; }

        public override string ToString()
        {
            return this.Raw;
        }

        public static Hostmask Parse(string raw)
        {
            var hostSeparatorIdx = raw.IndexOf('@');

            if (hostSeparatorIdx == -1) // nothing but a host
            {
                return new Hostmask { Hostname = raw, Raw = raw };
            }

            var nickSeparatorIdx = raw.IndexOf('!');

            if (nickSeparatorIdx != -1) // have a username
            {
                return new Hostmask()
                {
                    Nickname = raw.Substring(0, nickSeparatorIdx),
                    Username = raw.Substring(nickSeparatorIdx + 1, hostSeparatorIdx - nickSeparatorIdx),
                    Hostname = raw.Substring(hostSeparatorIdx + 1),
                    Raw = raw
                };
            }

            // have just a nick and host
            return new Hostmask()
            {
                Nickname = raw.Substring(0, hostSeparatorIdx),
                Hostname = raw.Substring(hostSeparatorIdx + 1),
                Raw = raw
            };
        }
    }
}