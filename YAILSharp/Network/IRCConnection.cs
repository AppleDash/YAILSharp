using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;

namespace YAILSharp.Network
{
    public class IRCConnection
    {
        private readonly ServerConfiguration _serverConfiguration;
        private LineBuffer _lineBuffer = new LineBuffer();
        private byte[] _recvBuffer = new byte[4096];
        private Stream _stream;

        public IRCConnection(ServerConfiguration serverConfiguration)
        {
            this._serverConfiguration = serverConfiguration;
        }

        public Socket Socket { get; private set; }

        public void Connect()
        {
            var selectedAddress = this.ResolveHost(this._serverConfiguration.Host, this._serverConfiguration.V6Mode);
            this.Socket = new Socket(selectedAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            this.Socket.Connect(selectedAddress, this._serverConfiguration.Port);
            this._stream = new NetworkStream(this.Socket);

            if (this._serverConfiguration.UseTLS)
            {
                var sslStream = new SslStream(this._stream);
                sslStream.AuthenticateAsClient(this._serverConfiguration.Host);

                this._stream = sslStream;
            }
        }

        public List<Line> HandleReceive()
        {
            var receivedLines = new List<Line>();
            var nBytesReceived = this._stream.Read(_recvBuffer);
            
            _lineBuffer.Append(_recvBuffer, nBytesReceived);

            while (_lineBuffer.HasLine())
            {
                receivedLines.Add(Line.Parse(_lineBuffer.PopLine()));
            }

            return receivedLines;
        }

        public void WriteLine(string line)
        {
            this._stream.Write(Encoding.UTF8.GetBytes(line + "\r\n"));
        }

        private IPAddress ResolveHost(string host, IPv6Mode v6Mode)
        {
            var hostEntry = Dns.GetHostEntry(host);

            IPAddress v4Candidate = null;
            IPAddress v6Candidate = null;

            foreach (var addressCandidate in hostEntry.AddressList)
            {
                var addressFamily = addressCandidate.AddressFamily;
                
                if (addressFamily == AddressFamily.InterNetwork)
                {
                    v4Candidate = addressCandidate;
                }
                else
                {
                    v6Candidate = addressCandidate;
                }

                // We found both a candidate V4 and V6 address to try.
                if (v4Candidate != null && v6Candidate != null)
                {
                    break;
                }
            }

            switch (v6Mode)
            {
                case IPv6Mode.Require:
                    return v6Candidate;
                case IPv6Mode.Deny:
                    return v4Candidate;
                case IPv6Mode.Allow:
                    return v6Candidate ?? v4Candidate;
                default:
                    throw new ArgumentOutOfRangeException(nameof(v6Mode), v6Mode, null);
            }
        }
    }
}