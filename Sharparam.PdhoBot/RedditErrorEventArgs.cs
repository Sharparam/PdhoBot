using System;

namespace Sharparam.PdhoBot
{
    public class RedditErrorEventArgs : EventArgs
    {
        public readonly string Message;

        internal RedditErrorEventArgs(string message)
        {
            Message = message;
        }
    }
}
