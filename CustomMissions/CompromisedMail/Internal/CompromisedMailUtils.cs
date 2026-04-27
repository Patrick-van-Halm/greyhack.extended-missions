using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ExtendedMissions.Registries
{
    internal static class CompromisedMailUtils
    {
        internal sealed class CompromisedMailRecipient
        {
            public string PublicIp { get; set; } = string.Empty;
            public Computer Computer { get; set; } = null!;
            public string UserName { get; set; } = string.Empty;
            public Persona Persona { get; set; } = null!;
        }

        public static CompromisedMailRecipient? GetRandomRecipient(Router router, System.Random random)
        {
            var candidates = new List<CompromisedMailRecipient>();
            foreach (var computer in router.GetComputers(true))
            {
                if (computer == null || computer.IsCctvOrSmartDevice()) continue;

                foreach (var user in computer.GetUsers(false))
                {
                    var persona = computer.GetPersona(user.nombreUsuario);
                    if (persona == null || string.IsNullOrEmpty(persona.GetMailAdress())) continue;
                    candidates.Add(new CompromisedMailRecipient {
                        PublicIp = router.GetPublicIP()!,
                        Computer = computer,
                        UserName = user.nombreUsuario,
                        Persona = persona
                    });
                }
            }

            return candidates.Count == 0 ? null : candidates[random.Next(candidates.Count)];
        }

        internal static bool AttachmentMatches(string serialAttach, string attachmentName)
        {
            if (string.IsNullOrEmpty(serialAttach) || string.IsNullOrEmpty(attachmentName)) return false;

            try
            {
                var attachment = JsonConvert.DeserializeObject<FileSystem.Archivo>(serialAttach);
                return attachment != null &&
                       attachment.GetNombre().Equals(attachmentName, StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }
    }
}
