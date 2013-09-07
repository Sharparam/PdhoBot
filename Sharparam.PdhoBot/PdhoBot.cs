using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IrcDotNet;
using RedditSharp;
using Sharparam.PdhoBot.Commands;

namespace Sharparam.PdhoBot
{
    public class PdhoBot
    {
        private const char CommandChar = '!';
        private const string BroadcastFormat = "[BROADCAST] {0}";
        private const string ModAddFormat = "FLAGS {0} {1} +Oiorv";

        public event EventHandler<EventArgs> Connected;
        public event EventHandler<IrcErrorEventArgs> ConnectFailed;
        public event EventHandler<EventArgs> Disconnected;
        public event EventHandler<EventArgs> MotdReceived;
        public event EventHandler<EventArgs> Registered;
        public event EventHandler<IrcMessageEventArgs> MessageReceived;
        public event EventHandler<IrcChannelEventArgs> JoinedChannel;
        public event EventHandler<IrcChannelEventArgs> LeftChannel;
        public event EventHandler<IrcChannelEventArgs> JoinedModChannel;
        public event EventHandler<IrcChannelEventArgs> LeftModChannel;
        public event EventHandler<IrcMessageEventArgs> ChannelMessageReceived;
        public event EventHandler<IrcMessageEventArgs> ModChannelMessageReceived;

        public event EventHandler<RedditErrorEventArgs> RedditError;

        private BotSettings _settings;

        private IrcClient _client;
        private IrcLocalUser _user;
        private IrcChannel _channel;
        private IrcChannel _modChannel;

        private Reddit _reddit;
        private AuthenticatedUser _redditUser;
        private Subreddit _subreddit;

        private List<ICommand> _commands;

        private Dictionary<string, string> _auth;

        public static readonly Random Rand = new Random();

        private void OnConnected()
        {
            var func = Connected;
            if (func != null)
                func(this, null);
        }

        private void OnConnectFailed(IrcErrorEventArgs err)
        {
            var func = ConnectFailed;
            if (func != null)
                func(this, err);
        }

        private void OnDisconnected()
        {
            var func = Disconnected;
            if (func != null)
                func(this, null);
        }

        private void OnMotdReceived()
        {
            var func = MotdReceived;
            if (func != null)
                func(this, null);
        }

        private void OnRegistered()
        {
            var func = Registered;
            if (func != null)
                func(this, null);
        }

        private void OnMessageReceived(IrcMessageEventArgs args)
        {
            var func = MessageReceived;
            if (func != null)
                func(this, args);
        }

        private void OnJoinedChannel(IrcChannelEventArgs args)
        {
            var func = JoinedChannel;
            if (func != null)
                func(this, args);
        }

        private void OnLeftChannel(IrcChannelEventArgs args)
        {
            var func = LeftChannel;
            if (func != null)
                func(this, args);
        }

        private void OnJoinedModChannel(IrcChannelEventArgs args)
        {
            var func = JoinedModChannel;
            if (func != null)
                func(this, args);
        }

        private void OnLeftModChannel(IrcChannelEventArgs args)
        {
            var func = LeftModChannel;
            if (func != null)
                func(this, args);
        }

        private void OnChannelMessageReceived(IrcMessageEventArgs args)
        {
            var func = ChannelMessageReceived;
            if (func != null)
                func(this, args);
        }

        private void OnModChannelMessageReceived(IrcMessageEventArgs args)
        {
            var func = ModChannelMessageReceived;
            if (func != null)
                func(this, args);
        }

        private void OnRedditError(string message)
        {
            var func = RedditError;
            if (func != null)
                func(this, new RedditErrorEventArgs(message));
        }

        public PdhoBot(BotSettings settings)
        {
            _settings = settings;

            _client = new IrcClient();
            _client.Connected += ClientOnConnected;
            _client.ConnectFailed += ClientOnConnectFailed;
            _client.Disconnected += ClientOnDisconnected;
            _client.MotdReceived += ClientOnMotdReceived;
            _client.Registered += ClientOnRegistered;

            _reddit = new Reddit();
            _redditUser = _reddit.LogIn(_settings.RedditUsername, _settings.RedditPassword);
            _subreddit = _reddit.GetSubreddit(_settings.Subreddit);

            _commands = new List<ICommand>
            {
                new FlairCommand(),
                new AuthCommand()
            };

            _auth = new Dictionary<string, string>();
        }

        public void Connect()
        {
            _client.Connect(_settings.Server, _settings.Port, false, _settings.GetRegistrationInfo());
        }

        public void Disconnect()
        {
            _client.Disconnected -= ClientOnDisconnected;
            _client.Disconnect();
        }

        private void ClientOnConnected(object sender, EventArgs eventArgs)
        {
            OnConnected();
        }

        private void ClientOnConnectFailed(object sender, IrcErrorEventArgs ircErrorEventArgs)
        {
            OnConnectFailed(ircErrorEventArgs);
        }

        private void ClientOnDisconnected(object sender, EventArgs eventArgs)
        {
            OnDisconnected();
        }

        private void ClientOnMotdReceived(object sender, EventArgs eventArgs)
        {
            OnMotdReceived();
        }

        private void ClientOnRegistered(object sender, EventArgs eventArgs)
        {
            OnRegistered();

            _user = _client.LocalUser;
            _user.MessageReceived += UserOnMessageReceived;
            _user.JoinedChannel += UserOnJoinedChannel;
            _user.LeftChannel += UserOnLeftChannel;
            
            _client.Channels.Join(_settings.Channel, _settings.ModChannel);
        }

        private void UserOnMessageReceived(object sender, IrcMessageEventArgs args)
        {
            OnMessageReceived(args);

            var user = args.Source as IrcUser;

            if (user == null)
                return;

            HandleMessage(user, args.Text);
        }

        private void UserOnJoinedChannel(object sender, IrcChannelEventArgs args)
        {
            var name = args.Channel.Name;

            if (name == _settings.Channel)
            {
                OnJoinedChannel(args);
                _channel = args.Channel;
                _channel.MessageReceived += ChannelOnMessageReceived;
            }
            else if (name == _settings.ModChannel)
            {
                OnJoinedModChannel(args);
                _modChannel = args.Channel;
                _modChannel.MessageReceived += ModChannelOnMessageReceived;
            }
        }

        private void UserOnLeftChannel(object sender, IrcChannelEventArgs args)
        {
            var name = args.Channel.Name;

            if (name == _settings.Channel)
            {
                OnLeftChannel(args);
                _channel.MessageReceived -= ChannelOnMessageReceived;
                _channel = null;
            }
            else if (name == _settings.ModChannel)
            {
                OnLeftModChannel(args);
                _modChannel.MessageReceived -= ModChannelOnMessageReceived;
                _modChannel = null;
            }
        }

        private void ChannelOnMessageReceived(object sender, IrcMessageEventArgs args)
        {
            OnChannelMessageReceived(args);

            if (!args.Targets.Contains(_channel) || String.IsNullOrEmpty(args.Text) || args.Text[0] != CommandChar) // If this is true then something is horribly wrong
                return;

            var user = args.Source as IrcUser;
            
            if (user == null)
                return;

            HandleMessage(user, args.Text);
        }

        private void ModChannelOnMessageReceived(object sender, IrcMessageEventArgs args)
        {
            OnModChannelMessageReceived(args);

            if (!args.Targets.Contains(_modChannel) || String.IsNullOrEmpty(args.Text) || args.Text[0] != CommandChar) // If this is true then something is horribly wrong
                return;

            var user = args.Source as IrcUser;

            if (user == null)
                return;

            HandleMessage(user, args.Text);
        }

        public bool IsOp(string nick)
        {
            return _modChannel.Users.Any(user => user.User.NickName == nick && user.Modes.Contains('o'));
        }

        private void HandleMessage(IrcUser source, string message)
        {
            if (String.IsNullOrEmpty(message))
                return;

            var split = message.Split(' ');
            var cmd = split[0].TrimStart(CommandChar);
            string arg = null;
            string[] args = null;

            if (split.Length > 1)
            {
                args = split.Skip(1).ToArray();
                arg = String.Join(" ", args);
            }

            var command = _commands.FirstOrDefault(c => c.Name == cmd);

            if (command == null)
                return;

            if (command.OpOnly && !IsOp(source.NickName))
            {
                _user.SendNotice(source, "'{0}' is available for moderators only.", command.Name);
                return;
            }

            var result = command.Run(this, source, arg, args);

            if (result.Success)
                _user.SendNotice(source, result.Message);
            else
            {
                switch (result.Error)
                {
                    case CommandError.Syntax:
                        _user.SendNotice(source, "Usage: {0}", command.Usage);
                        break;
                    case CommandError.Exception:
                        _user.SendNotice(source, "Exception while carrying out command '{0}': {1}", command.Name,
                                         result.Message);
                        break;
                    case CommandError.Other:
                        _user.SendNotice(source, "Unknown error while carrying out command '{0}': {1}", command.Name,
                                         result.Message);
                        break;
                    default:
                        _user.SendNotice(source, "Malformed CommandResult, tell dev: {0}; {1}; {2}", result.Success,
                                         result.Error, result.Message);
                        break;
                }
            }
        }

        public void Broadcast(string message)
        {
            message = String.Format(BroadcastFormat, message);

            if (_channel != null)
                _user.SendSafeMessage(_channel, message);

            if (_modChannel != null)
                _user.SendSafeMessage(_modChannel, message);
        }

        #region Reddit Methods

        public void SendRedditMessage(string user, string subject, string message)
        {
            _reddit.ComposePrivateMessage(subject, message, user);
        }

        public void SetFlair(string user, string css, string text)
        {
            var rUser = _reddit.GetUser(user);

            if (rUser == null)
                throw new RedditException("Could not find user " + user);

            _subreddit.SetUserFlair(user, css, text);
        }

        public bool IsSubMod(string user)
        {
            user = user.ToLower();

            var mods = _subreddit.GetModerators();
            return mods.Any(u => u.Name.ToLower() == user);
        }

        public void AuthAdd(string user, IrcUser issuer)
        {
            user = user.ToLower();

            if (!IsSubMod(user))
                throw new RedditException(user + " is not a mod of " + _settings.Subreddit);

            if (IsOp(issuer.NickName))
                throw new Exception("You are already a registered mod. Try /msg ChanServ UNBAN " + _modChannel.Name);

            if (AuthPending(user))
                CancelAuth(user);

            var code = Guid.NewGuid().ToString();

            _auth.Add(user, code);

            var msg =
                String.Format(
                    "Please use the following command on IRC (if you didn't issue this auth request, please ignore message): /msg PDHO-bot auth verify {0} {1}",
                    user, code);

            _reddit.ComposePrivateMessage("PaydayTheHeistOnline IRC mod authentication", msg, user);

            // DEBUG:
            Console.WriteLine("{0} = {1}", user, code);
        }

        public bool AuthPending(string user)
        {
            return _auth.ContainsKey(user.ToLower());
        }

        public bool AuthCheck(string user, string code)
        {
            user = user.ToLower();

            if (!AuthPending(user))
                return false;

            return _auth[user] == code;
        }

        public void CancelAuth(string user)
        {
            user = user.ToLower();

            if (AuthPending(user))
                _auth.Remove(user);
        }

        public void CompleteAuth(string redditUser, IrcUser ircUser)
        {
            redditUser = redditUser.ToLower();

            if (AuthPending(redditUser))
                _auth.Remove(redditUser);
            _user.SendMessage("ChanServ", String.Format(ModAddFormat, _modChannel.Name, ircUser.NickName));
        }

        #endregion Reddit Methods
    }
}
