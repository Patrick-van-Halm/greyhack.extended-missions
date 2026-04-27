using System;

namespace ExtendedMissions.Utils
{
    /// <summary>
    /// Utility methods for selecting mission targets from generated computers.
    /// </summary>
    public static class MissionUtils
    {
        /// <summary>
        /// Selects a random computer from a router.
        /// </summary>
        /// <param name="router">The router containing the candidate computers.</param>
        /// <param name="includeWithoutServices">Whether computers without services may be selected.</param>
        /// <returns>The selected computer.</returns>
        public static Computer GetRandomMissionComputer(Router router, bool includeWithoutServices = false)
        {
            var randomComputer = router.GetRandomComputer(!includeWithoutServices);
            return router.GetComputer(randomComputer.localIp);
        }

        /// <summary>
        /// Selects a random user from a computer.
        /// </summary>
        /// <param name="computer">The computer containing the candidate users.</param>
        /// <param name="random">The random number generator used for selection.</param>
        /// <param name="includeRoot">Whether the root user may be selected.</param>
        /// <returns>The selected computer user.</returns>
        public static Computer.User GetRandomUser(Computer computer, Random random, bool includeRoot = false)
        {
            var users = computer.GetUsers(includeRoot);
            return users[random.Next(users.Count)];
        }
    }
}