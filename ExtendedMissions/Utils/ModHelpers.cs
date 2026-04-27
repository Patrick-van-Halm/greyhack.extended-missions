using System;
using System.Globalization;

namespace ExtendedMissions.Utils
{
    public static class GameHelpers
    {
        public static Computer.User GetPlayerUser(this PlayerComputer pc)
        {
            var users = pc.GetUsers(false);
            if (users == null || users.Count == 0)
            {
                throw new InvalidOperationException("Player computer does not have a playable user account.");
            }

            return users[0];
        }

        public static Computer.User GetRepUser(this PlayerComputer pc)
        {
            var users = pc.GetUsers(true);
            if (users != null && users.Count > 1)
            {
                return users[1];
            }

            return pc.GetPlayerUser();
        }
    }

    public static class DebugLogger
    {
        public static void Log(string text)
        {
#if DEBUG
            Plugin.LogMessage(text);
#endif
        }
    }

    public static class DateTimeUtils
    {
        public static DateTime ParseFormat(string input, string format)
        {
            return DateTime.ParseExact(input, format, CultureInfo.InvariantCulture);
        }
    }
}
