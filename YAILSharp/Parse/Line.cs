using System.Collections.Generic;
using System.Linq;

namespace YAILSharp
{
    public class Line
    {
        public Hostmask Hostmask { get; set; }
        public string Command { get; set; }
        public string[] Params { get; set; }
        public string Raw { get; set; }

        public override string ToString()
        {
            return this.Raw;
        }
        
        public static Line Parse(string rawLine)
        {
            var line = rawLine; // We manipulate this string in various ways, but still want the original raw line.
            var parms = new List<string>();
            
            string hostmask = null;

            if (line[0] == ':') // We have a hostmask
            {
                var spaceIndex = line.IndexOf(' ');
                hostmask = line.Substring(1, spaceIndex - 1); // Grab the hostmask off the front of the line
                line = line.Substring(spaceIndex).TrimStart();
            }

            // Grab the command/numeric out of the line
            var command = string.Join("", line.TakeWhile(c => " :".IndexOf(c) == -1));
            line = line.Substring(command.Length).TrimStart().TrimEnd('\r', '\n');
            
            // collect the args, including the long arg if present
            while (line.Length > 0)
            {
                string thisParam;

                if (line[0] == ':') // "long" arg, time to collect it and bail out.
                {
                    thisParam = line.Substring(1);
                    line = "";
                }
                else // regular ol' short arg
                {
                    thisParam = string.Join("", line.TakeWhile(c => c != ' '));
                    line = line.Substring(thisParam.Length).Trim();
                }
                
                parms.Add(thisParam);
            }

            return new Line
            {
                Hostmask = hostmask == null ? null : Hostmask.Parse(hostmask),
                Command = command,
                Params = parms.ToArray(),
                Raw = rawLine
            };
        }
    }
}