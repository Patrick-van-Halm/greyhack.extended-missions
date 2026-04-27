using ExtendedMissions.Utils;
using MissionConfig;
using UnityEngine;
using System.Collections.Generic;
using ExtendedMissions.Missions;

namespace ExtendedMissions.Registries
{
    public class SendMoneyMission : ExtendedDirectMission<SendMoneyMission, SendMoneyMission.Data>
    {
        public class Data
        {
            public string FromAccount { get; set; } = string.Empty;
            public string ToAccount { get; set; } = string.Empty;
            public string LastTransactionDate { get; set; } = string.Empty;
            public int Amount { get; set; } = 0;
            public bool Precise { get; set; } = false;
        }

        private const string CONDITION_AT_LEAST_KEY = "AT_LEAST";
        private const string CONDITION_PRECISE_KEY = "PRECISE";

        protected override string Title => "Wire Money";
        protected override string Preview => "The client wants you to wire some money from a bank account.";
        protected override string Details => "It is necessary that you infiltrate in the network to gather the bank account credentials.";
        protected override string Mail => "The client wants you to wire some money from a bank account.\n\nThe remote ip of the victim is $PUBLIC_IP and the LAN address where the bank credentials of $USERNAME are located is $LOCAL_IP.\n\nThe money should be wired to $RECEIVER."; 
        protected override DirectMissionTarget DirectMissionBoard => DirectMissionTarget.Hackshop;

        override protected int MinReputation => 0;
        override protected int MaxReputation => 1;

        protected override Dictionary<string, string>? Conditions => new Dictionary<string, string>
        {
            [CONDITION_AT_LEAST_KEY] = "Client wants to receive at least: $$AMOUNT.",
            [CONDITION_PRECISE_KEY] = "Client wants to receive precisely: $$AMOUNT."
        };

        protected override PreparedMission? PrepareMission(DirectMission mission, string language, PlayerMissions playerMissions)
        {
            DebugLogger.Log("[SendMoneyMission] Add Mission to User");
            var random = RandomUtils.CreateRandom();
            var router = SpawnMissionRouter(mission, random);
            var routerReceiver = SpawnMissionRouter(mission, random);
            var preciseTransfer = mission.condition.condition.ToExtendedConditionId() == GetConditionId(CONDITION_PRECISE_KEY);

            var computerReceiver = MissionUtils.GetRandomMissionComputer(routerReceiver, false);
            var userReceiver = MissionUtils.GetRandomUser(computerReceiver, random);
            var personaReceiver = computerReceiver.GetPersona(userReceiver.nombreUsuario);

            var amount = preciseTransfer
                ? random.Next(500, 2500)
                : random.Next(100, 1000);
            var computer = MissionUtils.GetRandomMissionComputer(router, mission.rep > 0);
            var user = MissionUtils.GetRandomUser(computer, random);
            var persona = computer.GetPersona(user.nombreUsuario);

            var account = Database.Singleton.GetBankAccount(persona.GetUserBank().userName, out _);
            account.Ingreso("Unknown", "Job payout", Mathf.CeilToInt(random.Next(amount * (1 + BankAccount.TRANSACTION_FEE * 0.01f), amount * 2)));
            var targetUser = user.nombreUsuario;
            var targetBank = persona.GetUserBank().userName;

            var text = BuildMissionText(
                mission,
                language,
                ("$PUBLIC_IP", router.GetPublicIP()!.BoldString()),
                ("$LOCAL_IP", computer.GetLocalIP()!.BoldString()),
                ("$USERNAME", targetUser.BoldString()),
                ("$RECEIVER", personaReceiver.GetUserBank().userName.BoldString()),
                ("$AMOUNT", amount.ToString().BoldString())
            );

            var receiverBank = Database.Singleton.GetBankAccount(personaReceiver.GetUserBank().userName, out _);
            BankAccount.Transaccion? lastTransaction = null;
            foreach (var transaction in receiverBank.transacciones)
            {
                if (lastTransaction == null)
                {
                    lastTransaction = transaction;
                    continue;
                }
                var lastDate = DateTimeUtils.ParseFormat(lastTransaction.fecha, "dd/MMM/yyyy - HH:mm");
                var date = DateTimeUtils.ParseFormat(transaction.fecha, "dd/MMM/yyyy - HH:mm");
                if (date < lastDate) continue;
                lastTransaction = transaction;
            }

            return new PreparedMission
            {
                TargetComputer = computer,
                KarmaType = KarmaSystem.KarmaType.GREY,
                MissionData = new Data()
                {
                    FromAccount = targetBank,
                    ToAccount = personaReceiver.GetUserBank().userName,
                    LastTransactionDate = lastTransaction?.fecha ?? "01/Jan/1975 - 00:00",
                    Amount = amount,
                    Precise = preciseTransfer
                },
                Text = text
            };
        }

        protected override bool ValidateMission(ActiveMission mission, string message, FileSystem.Archivo attachment)
        {
            var data = GetData(mission);
            var bankReceiver = Database.Singleton.GetBankAccount(data.ToAccount, out _);
            var lastTransactionDate = DateTimeUtils.ParseFormat(data.LastTransactionDate, "dd/MMM/yyyy - HH:mm");
            foreach (var transaction in bankReceiver.transacciones)
            {
                var date = DateTimeUtils.ParseFormat(transaction.fecha, "dd/MMM/yyyy - HH:mm");
                if (date <= lastTransactionDate) continue;
                if (transaction.cuenta != data.FromAccount) continue;
                

                switch (data.Precise)
                {
                    case true when transaction.cantidad == data.Amount:
                    case false when transaction.cantidad >= data.Amount:
                        return true;
                }
            }

            return false;
        }

        protected override bool IsSoftLocked(ActiveMission mission)
        {
            var data = GetData(mission);
            var bankTarget = Database.Singleton.GetBankAccount(data.FromAccount, out _);
            return bankTarget.GetMoney() < data.Amount * (1 + BankAccount.TRANSACTION_FEE * 0.01);
        }
    }
}
