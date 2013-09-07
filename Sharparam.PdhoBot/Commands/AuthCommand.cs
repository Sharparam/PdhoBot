using System;
using IrcDotNet;

namespace Sharparam.PdhoBot.Commands
{
    public class AuthCommand : ICommand
    {
        public string Name { get { return "auth"; } }
        public string Help { get { return "Auths you to the mod channel if you are a mod."; } }
        public string Usage { get { return "auth request|verify <username> [<code>]"; } }

        public bool OpOnly { get { return false; } }

        public CommandResult Run(PdhoBot bot, IrcUser sender, string arg, params string[] args)
        {
            if (args == null || args.Length < 2)
                return new CommandResult(false, null, CommandError.Syntax);

            CommandResult result;

            switch (args[0])
            {
                case "request":
                    try
                    {
                        bot.AuthAdd(args[1], sender);
                        result = new CommandResult(true, "Success! Check your reddit inbox for instructions on how to complete authentication.");
                    }
                    catch (Exception ex)
                    {
                        result = new CommandResult(false, String.Format("{0}: {1}", ex.GetType(), ex.Message),
                                                   CommandError.Exception);
                    }
                    break;
                case "verify":
                    if (args.Length < 3)
                        result = new CommandResult(false, null, CommandError.Syntax);
                    else if (!bot.AuthPending(args[1]))
                        result = new CommandResult(false, "No auth is pending for that user.", CommandError.Other);
                    else if (!bot.AuthCheck(args[1], args[2]))
                        result = new CommandResult(false, "Invalid auth code!", CommandError.Other);
                    else
                    {
                        bot.CompleteAuth(args[1], sender);
                        result = new CommandResult(true, "Success! You should now be able to join the mod channel!");
                    }
                    break;
                default:
                    result = new CommandResult(false, null, CommandError.Syntax);
                    break;
            }

            return result;
        }
    }
}
