using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using YAILSharp;
using YAILSharp.Event;
using YAILSharp.Network;

namespace YAILSharpDemo
{
    public class YailSharpDemo
    {
        private readonly YAILConfiguration _config;
        private IRCClient _client;
        private bool _running;
        private string _channel;

        public YailSharpDemo(YAILConfiguration config)
        {
            _config = config;
        }

        public void Go()
        {
            _client = new IRCClient(_config);
            
            //_client.LineReceived += ClientOnLineReceived;
            _client.LineSent += ClientOnLineSent; 
            _client.MessageReceived += ClientOnMessageReceived;
            
            _client.Connect();

            _running = true;
            _channel = _config.Server.Channels[0];
            
            new Thread(() =>
            {
                while (_running)
                {
                    Console.Write("> ");
                    var input = Console.ReadLine();
                    
                    if (string.IsNullOrEmpty(input))
                    {
                        continue;
                    }

                    if (input[0] == '/')
                    {
                        if (input.Length > 1)
                        {
                            var split = input.Split(" ", 2);
                            if (split.Length == 2 && split[0] == "/me")
                            {
                                this._client.SendLine($"PRIVMSG {_channel} {(char)1}ACTION {split[1]}{(char)1}");
                            }
                            else
                            {
                                this._client.SendLine(input.Substring(1));
                            }
                        }
                    }
                    else
                    {
                        this._client.SendLine($"PRIVMSG {_channel} :{input}");
                    }
                }
            }).Start();
            
            MainLoop();
        }

        private void ClientOnMessageReceived(object sender, MessageEventArgs e)
        {
            var formattedMessage = this.FormatMessage(e.Message);

            if (formattedMessage != null)
            {
                Console.WriteLine(formattedMessage);
            }
            
            if (e.Message.MessageType == Message.Type.EndOfMotd) // Reasonably safe to start sending commands by this point.
            {
                foreach (var channel in _config.Server.Channels)
                {
                    _client.SendLine($"JOIN {channel}");
                }
            }
        }

        private void ClientOnLineSent(object sender, LineEventArgs e)
        {
            Console.WriteLine($"<<< {e.Line.Raw}");
        }

        private void MainLoop()
        {
            while (_running)
            {
                _client.HandleReceive();;
            }
        }

        private string FormatMessage(Message message)
        {
            var line = message.Line;
            
            // I could make this 100 times better by simply making Message do the parsing of the data,
            // and having different classes for different messages in an OOP manner, but... I don't want to!
            switch (message.MessageType)
            {
                case Message.Type.Motd:
                    return $"[MOTD] {line.Params.Last()}";
                case Message.Type.EndOfMotd:
                    return "--- End of MOTD. ---";
                case Message.Type.Join:
                    return $"--- {line.Params[0]} Joins: {line.Hostmask.Nickname}";
                case Message.Type.Part:
                    return $"--- {line.Params[0]} Parts: {line.Hostmask.Nickname}";
                case Message.Type.ChannelMessage:
                    return $"[{line.Params[0]}] <{line.Hostmask.Nickname}> {line.Params.Last()}";
                case Message.Type.PrivateMessage:
                    var body = line.Params.Last();

                    if (body.Length > 8 && body.StartsWith("\x01" + "ACTION") && body[body.Length - 1] == '\x01')
                    {
                        return $"<{line.Hostmask.Nickname}> -> * {line.Params[0]} {line.Params.Last()}";
                    }
                    else
                    {
                        return $"<{line.Hostmask.Nickname}> -> <{line.Params[0]}> {line.Params.Last()}";
                    }
                case Message.Type.Notice:
                    return $"[NOTICE] <{line.Hostmask.Nickname ?? line.Hostmask.Hostname}> -> <{line.Params[0]}>: {line.Params.Last()}";
                case Message.Type.Ping:
                    // Ignored
                    return null;
                default:
                    return $">>> {line}";
            }
        }
    }
}