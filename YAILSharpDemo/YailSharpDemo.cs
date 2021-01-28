using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using YAILSharp;
using YAILSharp.Network;

namespace YAILSharpDemo
{
    public class YailSharpDemo
    {
        private readonly ServerConfiguration _config;
        private IRCConnection _connection;

        public YailSharpDemo(ServerConfiguration config)
        {
            _config = config;
        }

        public void Go()
        {
            _connection = new IRCConnection(new ServerConfiguration()
            {
                Host = "irc.canternet.org",
                Port = 6667,
                Nickname = "appledasht",
                Realname = "appled",
                Username = "appled"
            });
            
            _connection.Connect();

            MainLoop();
        }

        private void MainLoop()
        {
            while (true)
            {
                var selectableSockets = new List<Socket>() {_connection.Socket};

                Socket.Select(selectableSockets, null, null, -1);

                if (selectableSockets.Count == 0)
                {
                    Thread.Sleep(100);
                    continue;
                }
                
                _connection.HandleReceive();
            }
        }
    }
}