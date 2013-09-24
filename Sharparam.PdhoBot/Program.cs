using System;
using System.IO;
using System.Linq;

namespace Sharparam.PdhoBot
{
    public class Program
    {
        private PdhoBot _bot;
        private bool _run;

        public static void Main(string[] args)
        {
            BotSettings settings;

            if (args.Length > 0 && File.Exists(args[0]))
                settings = BotSettings.Load(args[0]);
            else if (File.Exists(BotSettings.SettingsFile))
                settings = BotSettings.Load();
            else
                settings = BotSettings.RequestFromStdIn();

            settings.Save();

            new Program(settings).Run();
        }

        private Program(BotSettings settings)
        {
            _bot = new PdhoBot(settings);
            _bot.Connected += (sender, args) => Console.WriteLine("Connected!");

            _bot.ConnectFailed += (sender, args) =>
            {
                Console.WriteLine("Connect failed: {0}", args.Error);
                Console.WriteLine("Reconnecting...");
                _bot.Reconnect();
            };

            _bot.Disconnected += (sender, args) =>
            {
                Console.WriteLine("Disconnected!");
                Console.WriteLine("Reconnecting...");
                _bot.Reconnect();
            };

            _bot.MotdReceived += (sender, args) => Console.WriteLine("MOTD Received!");
            _bot.Registered += (sender, args) => Console.WriteLine("Registered!");
            _bot.MessageReceived += (sender, args) => Console.WriteLine("PRIV ({0}): {1}", args.Source.Name, args.Text);
            _bot.JoinedChannel += (sender, args) => Console.WriteLine("Joined channel!");
            _bot.JoinedModChannel += (sender, args) => Console.WriteLine("Joined mod channel!");
            _bot.LeftChannel += (sender, args) => Console.WriteLine("Left channel!");
            _bot.LeftModChannel += (sender, args) => Console.WriteLine("Left mod channel!");
            _bot.ChannelMessageReceived += (sender, args) => Console.WriteLine("CHAN ({0}): {1}", args.Source.Name, args.Text);
            _bot.ModChannelMessageReceived += (sender, args) => Console.WriteLine("MODCHAN ({0}): {1}", args.Source.Name, args.Text);
        }

        private void Run()
        {
            _run = true;

            _bot.Connect();

            while (_run)
            {
                var input = Console.ReadLine();
                if (String.IsNullOrEmpty(input))
                    continue;
                var split = input.Split(' ');
                var cmd = split[0];
                var args = split.Length > 0 ? split.Skip(1).ToArray() : new string[0];
                var arg = args.Length > 0 ? String.Join(" ", args) : String.Empty;
                HandleCommand(cmd, arg, args);
            }

            _bot.Disconnect();
        }

        private void HandleCommand(string command, string arg, string[] args)
        {
            switch (command)
            {
                case "bc":
                case "broadcast":
                    _bot.Broadcast(arg);
                    break;
                case "q":
                case "exit":
                case "quit":
                    _run = false;
                    break;
                default:
                    Console.WriteLine("Unknown command.");
                    break;
            }
        }
    }
}
