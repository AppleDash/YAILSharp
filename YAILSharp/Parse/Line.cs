namespace YAILSharp
{
    public class Line
    {
        public string Hostmask { get; set; }
        public string Command { get; set; }
        public string[] Params { get; set; }
        public string Raw { get; set; }

        public override string ToString()
        {
            return this.Raw;
        }
    }
}