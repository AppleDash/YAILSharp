using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Threading;
using YAILSharp.Event;
using YAILSharp.Network;

namespace YAILSharp
{
    public class IRCClient
    {
        public delegate void LineHandler(object sender, LineEventArgs e);

        public delegate void MessageHandler(object sender, MessageEventArgs e);

        public event LineHandler LineSent;
        public event LineHandler LineReceived;

        public event MessageHandler MessageReceived;

        private readonly YAILConfiguration _configuration;
        private readonly IRCConnection _connection;
        private readonly ConcurrentQueue<string> _outgoingLines = new ConcurrentQueue<string>();

        public IRCClient(YAILConfiguration configuration)
        {
            _configuration = configuration;
            _connection = new IRCConnection(_configuration.Server);
        }

        public void Connect()
        {
            var identity = _configuration.Server.Identity ?? _configuration.GlobalIdentity;
            
            _connection.Connect();
            
            // Alright, let's hope we're connected now, time to send the initial auth handshake...
            _connection.WriteLine($"NICK {identity.Nickname}");
            _connection.WriteLine($"USER {identity.Username} * * {identity.Realname}");
        }

        public void HandleReceive()
        {
            while (_outgoingLines.TryDequeue(out var line))
            {
                this._connection.WriteLine(line);
                LineSent?.Invoke(this, new LineEventArgs(Line.Parse(line)));
            }
            
            var selectableSockets = new List<Socket>() { _connection.Socket };

            Socket.Select(selectableSockets, null, null, 1000);

            if (selectableSockets.Count == 0)
            {
                return;
            }
            
            var linesReceived = _connection.HandleReceive();

            foreach (var line in linesReceived)
            {
                if (line.Command == "PING") // Automatically respond to ping
                {
                    this._connection.WriteLine($"PONG :{line.Params[0]}");
                }

                // Gross and basic parsing, for demonstration purposes.
                Message.Type type = line.Command switch
                {
                    "372" => Message.Type.Motd,
                    "376" => Message.Type.EndOfMotd,
                    "PRIVMSG" => "#&".IndexOf(line.Params[0][0]) != -1 ? Message.Type.ChannelMessage : Message.Type.PrivateMessage,
                    "PING" => Message.Type.Ping,
                    "JOIN" => Message.Type.Join,
                    "PART" => Message.Type.Part,
                    "NOTICE" => Message.Type.Notice,
                    _ => Message.Type.Raw
                };

                MessageReceived?.Invoke(this, new MessageEventArgs(new Message(line, type)));
                LineReceived?.Invoke(this, new LineEventArgs(line));
            }
        }
        
        public void SendLine(string line)
        {
            this._outgoingLines.Enqueue(line);
        }
    }
}