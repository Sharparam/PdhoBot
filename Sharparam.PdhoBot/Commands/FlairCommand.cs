using System;
using System.Linq;
using IrcDotNet;

namespace Sharparam.PdhoBot.Commands
{
    public class FlairCommand : ICommand
    {
        public string Name { get { return "flair"; } }
        public string Help { get { return "Sets a reddit user's flair."; } }
        public string Usage { get { return "flair <user> <class> <text>"; } }

        public bool OpOnly { get { return true; } }
        
        public CommandResult Run(PdhoBot bot, IrcUser sender, string arg, params string[] args)
        {
            if (args == null || args.Length < 3)
                return new CommandResult(false, null, CommandError.Syntax);

            var user = args[0];
            var css = args[1];
            var text = String.Join(" ", args.Skip(2));

            try
            {
                bot.SetFlair(user, css, text);
            }
            catch (Exception ex)
            {
                return new CommandResult(false, String.Format("{0}: {1}", ex.GetType(), ex.Message), CommandError.Exception);
            }

            return new CommandResult(true, "Successfully set the flair of user " + user + "!");
        }
    }
}
