using IrcDotNet;

namespace Sharparam.PdhoBot.Commands
{
    public interface ICommand
    {
        string Name { get; }
        string Help { get; }
        string Usage { get; }

        bool OpOnly { get; }

        CommandResult Run(PdhoBot bot, IrcUser sender, string arg, params string[] args);
    }
}
