using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sharparam.PdhoBot.Commands
{
    public struct CommandResult
    {
        public readonly bool Success;
        public readonly string Message;
        public readonly CommandError Error;

        internal CommandResult(bool success, string message = null, CommandError error = CommandError.None)
        {
            Success = success;
            Message = message;
            Error = error;
        }
    }
}
