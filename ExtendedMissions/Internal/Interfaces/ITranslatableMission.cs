using System.Collections.Generic;

namespace ExtendedMissions.Missions
{
    internal interface ITranslatableMission : IExtendedMission
    {
        void RegisterTranslations(IDictionary<string, string> translations);
    }
}