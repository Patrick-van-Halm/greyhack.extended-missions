namespace ExtendedMissions.Missions.Test
{
    internal class TestHiddenMission : ExtendedHiddenMission<TestHiddenMission>
    {
        internal class Data { }

        protected override int RewardTier => 0;

        public TestHiddenMission()
        {
            var idx = AddGeneratedNpc(isGuilty: true);
            var pc = AddComputerMission(cloneNpcIndex: idx);
            AddTextFileMissionItem(pc, "Hello world", true);
        }
    }
}
