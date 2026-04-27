using System;
using System.Collections.Generic;
using ExtendedMissions.Missions;
using ExtendedMissions.Utils;
using MissionConfig;
using Random = System.Random;

namespace ExtendedMissions.Registries
{
    public class CompromisedMailMission : ExtendedDirectMission<CompromisedMailMission, CompromisedMailMission.Data>
    {
        public class Data
        {
            public string SenderMail { get; set; } = string.Empty;
            public string RecipientComputerId { get; set; } = string.Empty;
            public string RecipientUserName { get; set; } = string.Empty;
            public string Subject { get; set; } = string.Empty;
            public string Body { get; set; } = string.Empty;
            public string? AttachmentName { get; set; } = null;
        }

        public enum EmailSubjectType
        {
            Invoice,
            SecurityUpdate,
            QuarterlyReport,
            PolicyUpdate,
            AccountVerification,
            PaymentReceipt,
            NewLogin,
            SubscriptionRenewal
        }

        private const string CONDITION_KNOWN_RECIPIENT = "KNOWN_RECIPIENT";
        private const string CONDITION_KNOWN_RECIPIENT_WITH_ATTACHMENT = "KNOWN_RECIPIENT_ATTACHMENT";
        private const string CONDITION_DISCOVER_RECIPIENT = "DISCOVER_RECIPIENT";
        private const string CONDITION_DISCOVER_RECIPIENT_WITH_ATTACHMENT = "DISCOVER_RECIPIENT_ATTACHMENT";

        protected override string Title => "Compromised Mail";
        protected override string Preview => "Recover real mail credentials from a compromised workstation and send the required message from that account.";
        protected override string Details => "Some jobs reveal the recipient mailbox directly. Others only give you the recipient workstation and username, forcing you to discover the real mailbox first.";
        protected override string Mail => "The client wants an email sent from a compromised employee account.\nThe subject must include $SUBJECT_TOKEN.\nThe message to be sent must be: $MESSAGE.";
        protected override DirectMissionTarget DirectMissionBoard => DirectMissionTarget.Hackshop;

        override protected int MinReputation => 1;
        override protected int MaxReputation => 2;

        public override FileSystem.Archivo? GetMailAttachment(ActiveMission mission)
        {
            var data = GetData(mission);
            if (data.AttachmentName == null) return null;
            return GenerationUtils.CreateMailAttachment(data.AttachmentName, string.Empty, true);
        }

        protected override Dictionary<string, string>? Conditions => new Dictionary<string, string>
        {
            [CONDITION_KNOWN_RECIPIENT] = "The sender workstation is at $SENDER_PUBLIC_IP with LAN address $SENDER_LOCAL_IP.\nThe sender username is $SENDER_USERNAME.\nThe recipient address is $TARGET_EMAIL.\nYou must recover the sender account credentials and send the message from that real account.",
            [CONDITION_KNOWN_RECIPIENT_WITH_ATTACHMENT] = "The sender workstation is at $SENDER_PUBLIC_IP with LAN address $SENDER_LOCAL_IP.\nThe sender username is $SENDER_USERNAME.\nThe recipient address is $TARGET_EMAIL.\nYou must recover the sender account credentials and send the message from that real account.\nYou must also attach a file named $ATTACHMENT_NAME.",
            [CONDITION_DISCOVER_RECIPIENT] = "The sender workstation is at $SENDER_PUBLIC_IP with LAN address $SENDER_LOCAL_IP.\nThe sender username is $SENDER_USERNAME.\nThe recipient workstation is at $RECIP_PUBLIC_IP with LAN address $RECIP_LOCAL_IP.\nThe recipient username is $RECIP_USERNAME.\nYou must discover the recipient mailbox from that second machine, then send the message from the real sender account.",
            [CONDITION_DISCOVER_RECIPIENT_WITH_ATTACHMENT] = "The sender workstation is at $SENDER_PUBLIC_IP with LAN address $SENDER_LOCAL_IP.\nThe sender username is $SENDER_USERNAME.\nThe recipient workstation is at $RECIP_PUBLIC_IP with LAN address $RECIP_LOCAL_IP.\nThe recipient username is $RECIP_USERNAME.\nYou must discover the recipient mailbox from that second machine, then send the message from the real sender account.\nYou must also attach a file named $ATTACHMENT_NAME.",
        };

        protected override PreparedMission? PrepareMission(DirectMission mission, string language, PlayerMissions playerMissions)
        {
            DebugLogger.Log("[CompromisedMailMission] Add Mission to User");
            var random = RandomUtils.CreateRandom();
            var senderRouter = SpawnMissionRouter(mission, random);
            var recipientRouter = SpawnMissionRouter(mission, random);

            var senderComputer = MissionUtils.GetRandomMissionComputer(senderRouter, mission.rep != 1);
            var senderUser = MissionUtils.GetRandomUser(senderComputer, random);
            var senderPersona = senderComputer.GetPersona(senderUser.nombreUsuario);
            if (senderPersona == null) return null;

            var recipientContext = CompromisedMailUtils.GetRandomRecipient(recipientRouter, random);
            if (recipientContext == null) return null;

            var senderMail = senderPersona.GetMailAdress();
            var recipientMail = recipientContext.Persona.GetMailAdress();
            if (string.IsNullOrEmpty(senderMail) || string.IsNullOrEmpty(recipientMail) ||
                senderMail.Equals(recipientMail, StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            var conditionId = mission.condition.condition.ToExtendedConditionId();
            var requireAttachment = conditionId == GetConditionId(CONDITION_DISCOVER_RECIPIENT_WITH_ATTACHMENT) ||
                                    conditionId == GetConditionId(CONDITION_KNOWN_RECIPIENT_WITH_ATTACHMENT);

            var subjectType = GetRandomType(random);
            var subject = GetSubject(subjectType, random);
            var body = GetBody(subjectType, random);
            var attachmentName = requireAttachment ? GetAttachmentName(subjectType) : string.Empty;

            var text = BuildMissionText(
                mission,
                language,
                ("$SENDER_PUBLIC_IP", senderRouter.GetPublicIP()!.BoldString()),
                ("$SENDER_LOCAL_IP", senderComputer.GetLocalIP()!.BoldString()),
                ("$SENDER_USERNAME", senderUser.nombreUsuario.BoldString()),
                ("$PUBLIC_IP", senderRouter.GetPublicIP()!.BoldString()),
                ("$LOCAL_IP", senderComputer.GetLocalIP()!.BoldString()),
                ("$USERNAME", senderUser.nombreUsuario.BoldString()),
                ("$TARGET_EMAIL", recipientMail.BoldString()),
                ("$RECIP_PUBLIC_IP", recipientContext.PublicIp.BoldString()),
                ("$RECIP_LOCAL_IP", recipientContext.Computer.GetLocalIP()!.BoldString()),
                ("$RECIP_USERNAME", recipientContext.UserName.BoldString()),
                ("$SUBJECT_TOKEN", subject.BoldString()),
                ("$BODY_TOKEN", body.BoldString()),
                ("$ATTACHMENT_NAME", attachmentName.BoldString())
            );

            return new PreparedMission
            {
                MissionData = new Data
                {
                    SenderMail = senderMail,
                    RecipientComputerId = recipientContext.Computer.GetID(),
                    RecipientUserName = recipientContext.UserName,
                    Subject = subject,
                    Body = body,
                    AttachmentName = requireAttachment ? attachmentName : null
                },
                TargetComputer = senderComputer,
                KarmaType = KarmaSystem.KarmaType.GREY,
                Text = text,
            };        
        }

        protected override bool ValidateMission(ActiveMission mission, string message, FileSystem.Archivo attachment)
        {
            var data = GetData(mission);
            var recipientComputer = ServerMap.Singleton.GetRemoteComputer(data.RecipientComputerId);
            if (recipientComputer == null) return false;
            var recipientUser = recipientComputer.GetUser(data.RecipientUserName);
            if (recipientUser == null) return false;
            var recipientPersona = recipientComputer.GetPersona(recipientUser.nombreUsuario);
            if (recipientPersona == null) return false;
            var recipientMailbox = Database.Singleton.GetMailAccount(recipientPersona.GetMailAdress(), out _);
            if (recipientMailbox == null) return false;

            foreach (var mail in recipientMailbox.emails)
            {
                if (!mail.otherMail.Equals(data.SenderMail, StringComparison.OrdinalIgnoreCase)) continue;

                foreach (var mailMessage in mail.messages)
                {
                    if (string.IsNullOrEmpty(mailMessage.titulo) ||
                        !mailMessage.titulo.Equals(data.Subject, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    if (string.IsNullOrEmpty(mailMessage.mensaje) ||
                        !mailMessage.mensaje.Equals(data.Body, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    if (data.AttachmentName != null && !CompromisedMailUtils.AttachmentMatches(mailMessage.serialAttach, data.AttachmentName))
                    {
                        continue;
                    }

                    return true;
                }
            }

            return false;
        }

        private static string GetSubject(EmailSubjectType type, Random random)
        {
            return type switch
            {
                EmailSubjectType.Invoice => GetRandom(random, new[]
                {
                    "Invoice for your recent purchase",
                    "Your invoice is ready",
                    "Billing statement attached",
                    "Invoice details for your order"
                }),

                EmailSubjectType.SecurityUpdate => GetRandom(random, new[]
                {
                    "Important: Security update required",
                    "Critical security notice",
                    "Action needed: Security update",
                    "Security alert for your account"
                }),

                EmailSubjectType.QuarterlyReport => GetRandom(random, new[]
                {
                    "Your quarterly financial report",
                    "Q report is now available",
                    "Quarterly performance summary",
                    "Financial report attached"
                }),

                EmailSubjectType.PolicyUpdate => GetRandom(random, new[]
                {
                    "Policy update notification",
                    "Important changes to our policy",
                    "Updated terms and policies",
                    "Please review our updated policy"
                }),

                EmailSubjectType.AccountVerification => GetRandom(random, new[]
                {
                    "Action required: Account verification",
                    "Verify your account now",
                    "Account verification needed",
                    "Confirm your account details"
                }),

                EmailSubjectType.PaymentReceipt => GetRandom(random, new[]
                {
                    "Payment receipt confirmation",
                    "Your payment was successful",
                    "Receipt for your transaction",
                    "Payment confirmation"
                }),

                EmailSubjectType.NewLogin => GetRandom(random, new[]
                {
                    "New login detected on your account",
                    "Suspicious login alert",
                    "New sign-in notification",
                    "Login activity notice"
                }),

                EmailSubjectType.SubscriptionRenewal => GetRandom(random, new[]
                {
                    "Subscription renewal notice",
                    "Your subscription is expiring",
                    "Renewal reminder",
                    "Subscription update"
                }),

                _ => "Notification"
            };
        }

        private static string GetAttachmentName(EmailSubjectType type)
        {
            var clock = ClockServer.Singleton;

            return type switch
            {
                EmailSubjectType.Invoice =>
                    $"invoice_{clock.currentTime:yyyy_MM_dd}.pdf",

                EmailSubjectType.SecurityUpdate =>
                    "security_update_instructions.pdf",

                EmailSubjectType.QuarterlyReport =>
                    $"financial_report_Q{GetQuarter(clock.currentTime)}_{clock.currentTime:yyyy}.xlsx",

                EmailSubjectType.PolicyUpdate =>
                    "policy_changes.docx",

                EmailSubjectType.AccountVerification =>
                    "account_verification_form.pdf",

                EmailSubjectType.PaymentReceipt =>
                    $"receipt_{Guid.NewGuid().ToString()[..8]}.pdf",

                EmailSubjectType.NewLogin =>
                    "login_activity_report.pdf",

                EmailSubjectType.SubscriptionRenewal =>
                    "subscription_details.pdf",

                _ => "attachment.pdf"
            };
        }

        private static int GetQuarter(DateTime dt)
        {
            return (dt.Month - 1) / 3 + 1;
        }

        private static string GetBody(EmailSubjectType type, Random random)
        {
            return type switch
            {
                EmailSubjectType.Invoice => GetRandom(random, new[]
                {
                    "Thank you for your purchase. Please find your invoice attached.",
                    "Your invoice is attached for your records.",
                    "Please review the attached invoice for your recent order."
                }),

                EmailSubjectType.SecurityUpdate => GetRandom(random, new[]
                {
                    "We released a security update. Please follow the attached instructions.",
                    "Immediate action is required. See attached security details.",
                    "Review the attached document to secure your account."
                }),

                EmailSubjectType.QuarterlyReport => GetRandom(random, new[]
                {
                    "Your quarterly report is attached for review.",
                    "Please find the financial report attached.",
                    "Review the attached quarterly performance summary."
                }),

                EmailSubjectType.PolicyUpdate => GetRandom(random, new[]
                {
                    "Our policies have changed. See the attached document.",
                    "Please review the updated policy attached.",
                    "Important policy changes are included in the attachment."
                }),

                EmailSubjectType.AccountVerification => GetRandom(random, new[]
                {
                    "Please verify your account using the attached form.",
                    "Account verification is required. See attachment.",
                    "Complete the attached form to verify your account."
                }),

                EmailSubjectType.PaymentReceipt => GetRandom(random, new[]
                {
                    "Your payment has been processed successfully.",
                    "Please find your receipt attached.",
                    "Payment confirmed. Receipt is included."
                }),

                EmailSubjectType.NewLogin => GetRandom(random, new[]
                {
                    "A new login was detected. Review the attached report.",
                    "Please confirm this login activity.",
                    "See attached details for recent login activity."
                }),

                EmailSubjectType.SubscriptionRenewal => GetRandom(random, new[]
                {
                    "Your subscription will renew soon.",
                    "Please review your subscription details.",
                    "Renewal information is attached."
                }),

                _ => "Please see the attachment."
            };
        }

        private static string GetRandom(Random random, string[] values)
        {
            return values[random.Next(values.Length)];
        }

        private static EmailSubjectType GetRandomType(Random random)
        {
            var values = Enum.GetValues(typeof(EmailSubjectType));
            return (EmailSubjectType)values.GetValue(random.Next(values.Length))!;
        }
    }
}
