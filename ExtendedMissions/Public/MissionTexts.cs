namespace ExtendedMissions.Texts
{
    /// <summary>
    /// Provides localized vanilla mission text lookups used by custom mission implementations.
    /// </summary>
    public static class MissionTexts
    {
        private const string NoneKey = "NONE";
        private const string MissionOkKey = "MISSION_OK";
        private const string MissionIntrFailedKey = "MISSION_INTR_FAILED";
        private const string MissionNotCompletedKey = "MISSION_NO_COMPLETED";
        private const string MissionAnyUserKey = "MISSION_ANY_USER";
        private const string MissionSpecificUserKey = "MISSION_SPECIFIC_USER";
        private const string MissionCredentialsKey = "MISSION_CREDENTIALS";
        private const string MissionCredentialsTitleKey = "MISSION_CREDENTIALS_TITLE";
        private const string MissionCredentialsContentKey = "MISSION_CREDENTIALS_CONTENT";
        private const string MissionAcademicTitleKey = "MISSION_ACADEMIC_TITLE";
        private const string MissionAcademicContentKey = "MISSION_ACADEMIC_CONTENT";
        private const string MissionAcademicChangesKey = "MISSION_ACADEMIC_CHANGES";
        private const string MissionTargetNotFoundKey = "MISSION_TARGET_NOT_FOUND";
        private const string MissionModifyRegKey = "MISSION_MODIFY_REG";
        private const string MissionClientRegKey = "MISSION_CLIENT_REG";
        private const string MissionSubjectApprovedKey = "MISSION_SUBJECT_APPROVED";
        private const string MissionSubjectImprovedKey = "MISSION_SUBJECT_IMPROVED";
        private const string MissionAlreadyCompletedKey = "MISSION_ALREADY_COMPLETED";
        private const string MissionWrongFileKey = "MISSION_WRONG_FILE";
        private const string MissionErrorKey = "MISSION_ERROR";
        private const string MissionLanIpKey = "MISSION_LAN_IP";
        private const string MissionServicePcNotExistKey = "MISSION_SERVICE_PC_NOT_EXIST";
        private const string MissionServiceWrongIpKey = "MISSION_SERVICE_WRONG_IP";
        private const string MissionServiceStartKey = "MISSION_SERVICE_START";
        private const string MissionServiceFailedIniKey = "MISSION_SERVICE_FAILED_INI";
        private const string MissionAlreadyStartedKey = "MISSION_ALREADY_STARTED";
        private const string MissionPoliceTitleKey = "MISSION_POLICE_TITLE";
        private const string MissionPoliceContentKey = "MISSION_POLICE_CONTENT";
        private const string MissionPoliceChangesKey = "MISSION_POLICE_CHANGES";
        private const string MissionPoliceRemoveKey = "MISSION_POLICE_REMOVE";
        private const string MissionPoliceAddChargeKey = "MISSION_POLICE_ADD_CHARGE";
        private const string MissionPoliceRemoveChargeKey = "MISSION_POLICE_REMOVE_CHARGE";
        private const string MissionPoliceModRegKey = "MISSION_POLICE_MODREG";
        private const string MissionPoliceDetailRemoveChargeKey = "MISSION_POLICE_DETAIL_REMOVE_CHARGE";
        private const string MissionPoliceDetailAddChargeKey = "MISSION_POLICE_DETAIL_ADD_CHARGE";
        private const string MissionPoliceDetailDeleteKey = "MISSION_POLICE_DETAIL_DELETE";
        private const string MissionFailedKey = "MISSION_FAILED";
        private const string MissionDestroyPcTitleKey = "MISSION_DESTROY_PC_TITLE";
        private const string MissionDestroyPcContentKey = "MISSION_DESTROY_PC_CONTENT";
        private const string MissionDestroyPcKey = "MISSION_DESTROY_PC";
        private const string MissionTipNoPortsKey = "MISSION_TIP_NOPORTS";
        private const string MissionOkCouponKey = "MISSION_OK_COUPON";
        private const string MissionOkNoCouponKey = "MISSION_OK_NO_COUPON";

        private static string Get(string key, string language) => TranslationManager.GetText(key, string.Empty);

        /// <summary>Empty text.</summary>
        public static string None(string language) => Get(NoneKey, language);

        /// <summary>The customer is satisfied with the job. There has been an income in your account.</summary>
        public static string MissionOk(string language) => Get(MissionOkKey, language);

        /// <summary>The victim has discovered the intrusion, the job is canceled.</summary>
        public static string MissionIntrFailed(string language) => Get(MissionIntrFailedKey, language);

        /// <summary>You have not completed the order requirements, the mission is still active.</summary>
        public static string MissionNotCompleted(string language) => Get(MissionNotCompletedKey, language);

        /// <summary>The client wants the login credentials of any user.</summary>
        public static string MissionAnyUser(string language) => Get(MissionAnyUserKey, language);

        /// <summary>The client wants the login credentials of a particular user.</summary>
        public static string MissionSpecificUser(string language) => Get(MissionSpecificUserKey, language);

        /// <summary>Credentials.</summary>
        public static string MissionCredentials(string language) => Get(MissionCredentialsKey, language);

        /// <summary>Credentials needed.</summary>
        public static string MissionCredentialsTitle(string language) => Get(MissionCredentialsTitleKey, language);

        /// <summary>Client wants to access to the remote machine.</summary>
        public static string MissionCredentialsContent(string language) => Get(MissionCredentialsContentKey, language);

        /// <summary>Academic changes.</summary>
        public static string MissionAcademicTitle(string language) => Get(MissionAcademicTitleKey, language);

        /// <summary>Client wants to change some grades in his academic record.</summary>
        public static string MissionAcademicContent(string language) => Get(MissionAcademicContentKey, language);

        /// <summary>It is necessary that you infiltrate in the network to modify some data.</summary>
        public static string MissionAcademicChanges(string language) => Get(MissionAcademicChangesKey, language);

        /// <summary>The victim's computer is not online. Mission canceled.</summary>
        public static string MissionTargetNotFound(string language) => Get(MissionTargetNotFoundKey, language);

        /// <summary>The record to modify belongs to:</summary>
        public static string MissionModifyReg(string language) => Get(MissionModifyRegKey, language);

        /// <summary>Client wants to modify the subject.</summary>
        public static string MissionClientReg(string language) => Get(MissionClientRegKey, language);

        /// <summary>to change the note to approved at least.</summary>
        public static string MissionSubjectApproved(string language) => Get(MissionSubjectApprovedKey, language);

        /// <summary>to increase the academic qualification by at least one point</summary>
        public static string MissionSubjectImproved(string language) => Get(MissionSubjectImprovedKey, language);

        /// <summary>The mission does not exist.</summary>
        public static string MissionAlreadyCompleted(string language) => Get(MissionAlreadyCompletedKey, language);

        /// <summary>You have not completed the mission. The file you have sent is not the one the client wants. You can delete this message.</summary>
        public static string MissionWrongFile(string language) => Get(MissionWrongFileKey, language);

        /// <summary>This mission is no longer available. Refresh the page to get new contracts and try again.</summary>
        public static string MissionError(string language) => Get(MissionErrorKey, language);

        /// <summary>It's important that you access the correct machine behind the public ip. The victim's ip LAN is</summary>
        public static string MissionLanIp(string language) => Get(MissionLanIpKey, language);

        /// <summary>The IP address you have provided does not belong to any computer. Make sure the address is correct and belongs to an accessible computer. You can delete this message.</summary>
        public static string MissionServicePcNotExist(string language) => Get(MissionServicePcNotExistKey, language);

        /// <summary>The format of the IP address that you have provided is incorrect. Please try again and make sure you enter the address correctly. You can delete this message.</summary>
        public static string MissionServiceWrongIp(string language) => Get(MissionServiceWrongIpKey, language);

        /// <summary>The data you have provided is correct. The mission is now active. Make sure you do not interrupt access to the machine until the end date.</summary>
        public static string MissionServiceStart(string language) => Get(MissionServiceStartKey, language);

        /// <summary>You have taken too long to configure the server and the client has canceled the job.</summary>
        public static string MissionServiceFailedIni(string language) => Get(MissionServiceFailedIniKey, language);

        /// <summary>The mission was already active. Remember not to interrupt access to the server.</summary>
        public static string MissionAlreadyStarted(string language) => Get(MissionAlreadyStartedKey, language);

        /// <summary>Police record.</summary>
        public static string MissionPoliceTitle(string language) => Get(MissionPoliceTitleKey, language);

        /// <summary>The client wants to modify the information of a police record.</summary>
        public static string MissionPoliceContent(string language) => Get(MissionPoliceContentKey, language);

        /// <summary>It is necessary that you infiltrate the network to modify the date of certain charges.</summary>
        public static string MissionPoliceChanges(string language) => Get(MissionPoliceChangesKey, language);

        /// <summary>It is necessary that you infiltrate in the network to eliminate the complete record of a person.</summary>
        public static string MissionPoliceRemove(string language) => Get(MissionPoliceRemoveKey, language);

        /// <summary>You need to infiltrate the network to add a charge to a person's record.</summary>
        public static string MissionPoliceAddCharge(string language) => Get(MissionPoliceAddChargeKey, language);

        /// <summary>You need to infiltrate the network to remove a charge to a person's record.</summary>
        public static string MissionPoliceRemoveCharge(string language) => Get(MissionPoliceRemoveChargeKey, language);

        /// <summary>The client wants to modify the charge of [CRIME] so that the date of the crime is [YEARS] years before it was committed.</summary>
        public static string MissionPoliceModReg(string language) => Get(MissionPoliceModRegKey, language);

        /// <summary>The client wants to delete the charge of [CRIME]. To avoid raising suspicion, you only have to eliminate the indicated crime, do not eliminate any other.</summary>
        public static string MissionPoliceDetailRemoveCharge(string language) => Get(MissionPoliceDetailRemoveChargeKey, language);

        /// <summary>The client wants to add the charge of [CRIME] in the year [YEAR]. To avoid raising suspicion, you only have to add the indicated crime.</summary>
        public static string MissionPoliceDetailAddCharge(string language) => Get(MissionPoliceDetailAddChargeKey, language);

        /// <summary>The client wants to delete the record completely.</summary>
        public static string MissionPoliceDetailDelete(string language) => Get(MissionPoliceDetailDeleteKey, language);

        /// <summary>You have not completed the order requirements. The job is cancelled.</summary>
        public static string MissionFailed(string language) => Get(MissionFailedKey, language);

        /// <summary>Corrupt data.</summary>
        public static string MissionDestroyPcTitle(string language) => Get(MissionDestroyPcTitleKey, language);

        /// <summary>The client wants the remote machine to stop working.</summary>
        public static string MissionDestroyPcContent(string language) => Get(MissionDestroyPcContentKey, language);

        /// <summary>Be careful since the administrator will want to know who was responsible.</summary>
        public static string MissionDestroyPc(string language) => Get(MissionDestroyPcKey, language);

        /// <summary>Keep in mind that if the public address does not have open ports, you will need to hack the router and from there enter to the local network to reach the victim.</summary>
        public static string MissionTipNoPorts(string language) => Get(MissionTipNoPortsKey, language);

        /// <summary>The customer is satisfied with the job. To receive money you must wait 24 hours before accepting this type of job.</summary>
        public static string MissionOkCoupon(string language) => Get(MissionOkCouponKey, language);

        /// <summary>The customer is satisfied with the job. To receive money you must wait 24 hours before accepting this type of job.</summary>
        public static string MissionOkNoCoupon(string language) => Get(MissionOkNoCouponKey, language);
    }
}
