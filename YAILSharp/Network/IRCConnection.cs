using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace YAILSharp.Network
{
    public class IRCConnection
    {
        private readonly ServerConfiguration _serverConfiguration;
        private LineBuffer _lineBuffer = new LineBuffer();
        private byte[] _recvBuffer = new byte[4096];

        public IRCConnection(ServerConfiguration serverConfiguration)
        {
            this._serverConfiguration = serverConfiguration;
        }

        public Socket Socket { get; private set; }

        public void Connect()
        {
            var hostEntry = Dns.GetHostEntry(this._serverConfiguration.Host);
            var selectedAddress = hostEntry.AddressList[0]; // Slightly unintelligent, but hey.
            
            this.Socket = new Socket(selectedAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            this.Socket.Connect(selectedAddress, this._serverConfiguration.Port);
            
            // Alright, let's hope we're connected now, time to send the initial auth handshake...
            this.WriteLine($"NICK {_serverConfiguration.Nickname}");
            this.WriteLine($"USER {_serverConfiguration.Username} * * {_serverConfiguration.Realname}");
        }

        public List<Line> HandleReceive()
        {
            var receivedLines = new List<Line>();
            var nBytesReceived = this.Socket.Receive(_recvBuffer);
            _lineBuffer.Append(_recvBuffer, nBytesReceived);

            while (_lineBuffer.HasLine())
            {
                var line = LineParser.Parse(_lineBuffer.PopLine());

                if (!this.HandleLine(line)) // returns true if we should eat it, false otherwise.
                {
                    receivedLines.Add(line);
                }
            }

            return receivedLines;
        }

        public void WriteLine(string line)
        {
            var bytes = Encoding.UTF8.GetBytes(line + "\r\n");
            var bytesSent = 0;

            // send in a loop to ensure that we send everything even if the first send is a partial one.
            while (bytesSent < bytes.Length)
            {
                var bytesSentThisTime = this.Socket.Send(bytes, bytesSent, bytes.Length - bytesSent, SocketFlags.None);
                
                bytesSent += bytesSentThisTime;
            }
            
            Console.WriteLine("<<< " + line);
        }

        private bool HandleLine(Line line)
        {
            Console.WriteLine(">>> " + line);

            if (line.Command == "PING")
            {
                this.WriteLine($"PONG :{line.Params[0]}");
                
                return true;
            }

            return false;
        }
    }
}