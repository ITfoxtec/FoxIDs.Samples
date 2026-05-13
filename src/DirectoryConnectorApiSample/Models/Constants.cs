namespace DirectoryConnectorApiSample.Models;

public static class Constants
{
    public const string BasicAuthAppId = "directory_connector";

    public static class Errors
    {
        public const string InvalidApiIdOrSecret = "invalid_api_id_secret";
        public const string UserNotExists = "user_not_exists";
        public const string InvalidPassword = "invalid_password";
        public const string InvalidCurrentPassword = "invalid_current_password";
        public const string UserDisabled = "user_disabled";
        public const string UserDeleted = "user_deleted";
        public const string PasswordNotAccepted = "password_not_accepted";
        public const string PasswordMinLength = "password_min_length";
        public const string PasswordMaxLength = "password_max_length";
        public const string PasswordBannedCharacters = "password_banned_characters";
        public const string PasswordComplexity = "password_complexity";
        public const string PasswordEmailTextComplexity = "password_email_text_complexity";
        public const string PasswordPhoneTextComplexity = "password_phone_text_complexity";
        public const string PasswordUsernameTextComplexity = "password_username_text_complexity";
        public const string PasswordUrlTextComplexity = "password_url_text_complexity";
        public const string PasswordRisk = "password_risk";
        public const string PasswordHistory = "password_history";
        public const string PasswordExpired = "password_expired";
        public const string NewPasswordEqualsCurrent = "new_password_equals_current";

        public static readonly IReadOnlyDictionary<string, string> Descriptions = new Dictionary<string, string>
        {
            [InvalidApiIdOrSecret] = "The HTTP Basic API username or secret is invalid.",
            [UserNotExists] = "The user does not exist in the directory.",
            [InvalidPassword] = "The password was rejected by the directory.",
            [InvalidCurrentPassword] = "The current password in a change-password request was rejected by the directory.",
            [UserDisabled] = "The user exists in the directory but is disabled.",
            [UserDeleted] = "The user no longer exists or is deleted in the directory.",
            [PasswordNotAccepted] = "The new password was rejected by a directory password rule that does not map to a more specific code.",
            [PasswordMinLength] = "The new password is shorter than the directory password minimum length.",
            [PasswordMaxLength] = "The new password is longer than the directory password maximum length.",
            [PasswordBannedCharacters] = "The new password contains one or more characters or words rejected by the directory.",
            [PasswordComplexity] = "The new password does not satisfy the directory complexity requirements.",
            [PasswordEmailTextComplexity] = "The new password contains the user's email or part of it.",
            [PasswordPhoneTextComplexity] = "The new password contains the user's phone number or part of it.",
            [PasswordUsernameTextComplexity] = "The new password contains the user's username or part of it.",
            [PasswordUrlTextComplexity] = "The new password contains text related to the FoxIDs URL.",
            [PasswordRisk] = "The new password is known to be risky, compromised, or otherwise unsafe.",
            [PasswordHistory] = "The new password was rejected because it has been used before.",
            [PasswordExpired] = "The current password is expired and must be changed before authentication can continue.",
            [NewPasswordEqualsCurrent] = "The new password is the same as the current password."
        };
    }
}
