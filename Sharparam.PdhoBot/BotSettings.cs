using System;
using System.IO;
using IrcDotNet;
using Newtonsoft.Json;

namespace Sharparam.PdhoBot
{
    [JsonObject(MemberSerialization.OptIn)]
    public class BotSettings
    {
        private static readonly Lazy<JsonSerializer> JsonSerializer = new Lazy<JsonSerializer>();

        public const string SettingsFile = "settings.json";

        public string Filename { get; private set; }

        [JsonProperty("server")]
        public readonly string Server;

        [JsonProperty("port")]
        public readonly ushort Port;

        [JsonProperty("nick")]
        public readonly string Nickname;

        [JsonProperty("ircuser")]
        public readonly string IrcUsername;

        [JsonProperty("realname")]
        public readonly string Realname;

        [JsonProperty("ircpass")]
        public readonly string Ircpw;

        [JsonProperty("chan")]
        public readonly string Channel;

        [JsonProperty("modchan")]
        public readonly string ModChannel;

        [JsonProperty("reddituser")]
        public readonly string RedditUsername;

        [JsonProperty("redditpass")]
        public readonly string RedditPassword;

        [JsonProperty("subreddit")]
        public readonly string Subreddit;

        [JsonConstructor]
        private BotSettings()
        {
            
        }

        public BotSettings(string server, ushort port, string nickname, string ircUsername, string realname,
                           string ircpw, string channel, string modchannel, string reddituser, string redditpw,
                           string sub)
        {
            Server = server;
            Port = port;
            Nickname = nickname;
            IrcUsername = ircUsername;
            Realname = realname;
            Ircpw = ircpw;
            Channel = channel;
            ModChannel = modchannel;
            RedditUsername = reddituser;
            RedditPassword = redditpw;
            Subreddit = sub;
        }

        private static string ReadString()
        {
            var result = Console.ReadLine();

            while (String.IsNullOrEmpty(result))
            {
                Console.Write("Try again: ");
                result = Console.ReadLine();
            }

            return result;
        }

        private static ushort ReadUShort()
        {
            var input = Console.ReadLine();
            ushort result;
            var valid = UInt16.TryParse(input, out result);

            while (!valid)
            {
                Console.Write("Try again: ");
                input = Console.ReadLine();
                valid = UInt16.TryParse(input, out result);
            }

            return result;
        }

        public static BotSettings RequestFromStdIn()
        {
            Console.WriteLine("No settings file found, please set up bot...");
            Console.WriteLine("Settings that have [] markings is set to value within brackets if no value is given.");
            Console.WriteLine("-- IRC --");
            Console.Write("Server: ");
            var server = ReadString();
            Console.Write("Port: ");
            var port = ReadUShort();
            Console.Write("Nickname: ");
            var nickname = ReadString();
            Console.Write("NickServ PW: ");
            var password = ReadString();
            Console.Write("Username: [{0}] ", nickname);
            var username = Console.ReadLine();
            if (String.IsNullOrEmpty(username))
                username = nickname;
            Console.Write("Real name: [{0}] ", username);
            var realname = Console.ReadLine();
            if (String.IsNullOrEmpty(realname))
                realname = username;
            Console.Write("Channel: ");
            var channel = ReadString();
            if (!channel.StartsWith("#"))
                channel = '#' + channel;
            Console.Write("Mod channel: ");
            var modchannel = ReadString();
            if (!modchannel.StartsWith("#"))
                modchannel = '#' + modchannel;
            Console.WriteLine("-- REDDIT --");
            Console.Write("Username: ");
            var reddituser = ReadString();
            Console.Write("Password: ");
            var redditpw = ReadString();
            Console.Write("Subreddit: ");
            var sub = ReadString();
            if (!sub.StartsWith("/r/"))
                sub = "/r/" + sub;

            return new BotSettings(server, port, nickname, username, realname, password, channel, modchannel, reddituser,
                                   redditpw, sub);
        }

        public static BotSettings Load(string file = SettingsFile)
        {
            BotSettings result;

            using (var reader = new StreamReader(file))
            {
                using (var jsonReader = new JsonTextReader(reader))
                {
                    result = JsonSerializer.Value.Deserialize<BotSettings>(jsonReader);
                    jsonReader.Close();
                }
                reader.Close();
            }

            result.Filename = file;

            return result;
        }

        public IrcRegistrationInfo GetRegistrationInfo()
        {
            return new IrcUserRegistrationInfo
            {
                NickName = Nickname,
                UserName = IrcUsername,
                RealName = Realname,
                Password = Ircpw
            };
        }

        public void Save(string file = null)
        {
            if (String.IsNullOrEmpty(file))
                file = String.IsNullOrEmpty(Filename) ? SettingsFile : Filename;

            using (var writer = new StreamWriter(file, false))
            {
                using (var jsonWriter = new JsonTextWriter(writer){Formatting = Formatting.Indented})
                {
                    JsonSerializer.Value.Serialize(jsonWriter, this);
                    jsonWriter.Close();
                }
                writer.Close();
            }
        }
    }
}
