using HarmonyLib;
using NetworkMessages;

using ExtendedMissions.CustomMissions.SendMoney;

namespace ExtendedMissions.Patches.Bugs
{
    [HarmonyPatch(typeof(BankHelperServer), nameof(BankHelperServer.BankTransactionServerRpc), typeof(string), typeof(int), typeof(string), typeof(string), typeof(int))]
    public class Fix_From_Field_Transactions
    {
        public static bool Prefix(string fromAccountPass, int amount, string fromAccount, string toAccount, int windowPID,
            BankHelperServer __instance)
        {
            SendMoneyPlugin.Logger?.LogMessage("Patching bug #560 [When transferring money from User to User, the accounts do not show up correctly]");

            MessageClient messageClient = new MessageClient(IdClient.ErrorTransactionClientRpc);
            messageClient.AddInt(windowPID);
            if (fromAccount.Equals(toAccount))
            {
                messageClient.AddString("The target account can not be the same as the source account.");
                __instance.player.SendData(messageClient);
                return false;
            }
            string text;
            BankAccount bankAccount = Database.Singleton.GetBankAccount(fromAccount, fromAccountPass, out text);
            if (!string.IsNullOrEmpty(text))
            {
                messageClient.AddString(text);
                __instance.player.SendData(messageClient);
                return false;
            }
            BankAccount bankAccount2 = Database.Singleton.GetBankAccount(toAccount, out text);
            if (!string.IsNullOrEmpty(text))
            {
                messageClient.AddString(text);
                __instance.player.SendData(messageClient);
                return false;
            }
            float money = bankAccount.GetMoney();
            if (amount <= 0 || money < (float)amount)
            {
                messageClient.AddString("Error: Insufficient funds in the account");
                __instance.player.SendData(messageClient);
                return false;
            }
            int feeAmount = TransactionsPanel.GetFeeAmount(amount);
            int num = amount - feeAmount;
            bool flag = __instance.UpdateBankLogs(bankAccount.origBankAddress, bankAccount, bankAccount2, num, windowPID);
            bool flag2 = __instance.UpdateBankLogs(bankAccount2.origBankAddress, bankAccount, bankAccount2, num, windowPID);
            if (flag && flag2 && !bankAccount.isPlayer && bankAccount2.isPlayer && __instance.player.GetComputer().GetUserBank() != null && !__instance.bankTraceSystem.AddBank(bankAccount.origBankAddress, fromAccount))
            {
                messageClient.AddString("Error: Too many transactions have been made in a short period of time, please wait a few moments before making another transaction.");
                __instance.player.SendData(messageClient);
                return false;
            }
            bankAccount.Ingreso(fromAccount, "Transaction fee", -feeAmount, true, "", false);
            bankAccount.Ingreso(fromAccount, "Money transfer", -num, true, "", false);
            bankAccount2.Ingreso(fromAccount, "Money transfer", num, true, "", false);
            byte[] array = Database.Singleton.BankTransaction(bankAccount, bankAccount2);
            messageClient = new MessageClient(IdClient.ResumeTransactionClientRpc);
            messageClient.AddString("Transaction successful");
            messageClient.AddByte(array);
            messageClient.AddInt(windowPID);
            __instance.player.SendData(messageClient);
            return false;
        }
    }
}
