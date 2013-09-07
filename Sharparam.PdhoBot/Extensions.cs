using System;
using System.Collections.Generic;
using IrcDotNet;

namespace Sharparam.PdhoBot
{
    public static class Extensions
    {
        public static void SendMessage(this IrcLocalUser user, IIrcMessageTarget target, string format, params object[] args)
        {
            user.SendMessage(target, String.Format(format, args));
        }

        public static void SendMessage(this IrcLocalUser user, IEnumerable<IIrcMessageTarget> targets, string format, params object[] args)
        {
            user.SendMessage(targets, String.Format(format, args));
        }

        public static void SendSafeMessage(this IrcLocalUser user, IIrcMessageTarget target, string format, params object[] args)
        {
            var message = String.Format(format, args).Replace("\r\n", " ").Replace('\r', ' ').Replace('\n', ' ');
            user.SendMessage(target, message);
        }

        public static void SendSafeMessage(this IrcLocalUser user, IEnumerable<IIrcMessageTarget> targets, string format,
                                           params object[] args)
        {
            var message = String.Format(format, args).Replace("\r\n", " ").Replace('\r', ' ').Replace('\n', ' ');
            user.SendMessage(targets, message);
        }

        public static void SendNotice(this IrcLocalUser user, IIrcMessageTarget target, string format,
                                      params object[] args)
        {
            user.SendNotice(target, String.Format(format, args));
        }

        public static void SendNotice(this IrcLocalUser user, IEnumerable<IIrcMessageTarget> targets, string format, params object[] args)
        {
            user.SendNotice(targets, String.Format(format, args));
        }

        public static void SendSafeNotice(this IrcLocalUser user, IIrcMessageTarget target, string format, params object[] args)
        {
            var message = String.Format(format, args).Replace("\r\n", " ").Replace('\r', ' ').Replace('\n', ' ');
            user.SendNotice(target, message);
        }

        public static void SendSafeNotice(this IrcLocalUser user, IEnumerable<IIrcMessageTarget> targets, string format,
                                           params object[] args)
        {
            var message = String.Format(format, args).Replace("\r\n", " ").Replace('\r', ' ').Replace('\n', ' ');
            user.SendNotice(targets, message);
        }
    }
}
