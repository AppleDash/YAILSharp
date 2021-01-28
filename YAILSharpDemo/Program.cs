using System;
using System.Net.Sockets;
using YAILSharp;
using YAILSharp.Network;

namespace YAILSharpDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            new YailSharpDemo(new YAILConfiguration()
            {
                Server = new ServerConfiguration()
                {
                    Host = "irc.canternet.org",
                    Port = 6697,
                    UseTLS = true,
                    Identity = new UserIdentity("appledasht", "appled", "Apple Dash"),
                    Channels = new[] { "#test" }
                }
            }).Go();
        }
    }
}