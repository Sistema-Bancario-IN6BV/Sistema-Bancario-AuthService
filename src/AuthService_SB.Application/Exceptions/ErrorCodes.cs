namespace AuthService_SB.Application.Exceptions;

public static class ErrorCodes
{
    public const string EMAIL_ALREADY_EXISTS = "EMAIL_ALREADY_EXISTS";
    public const string USERNAME_ALREADY_EXISTS = "USERNAME_ALREADY_EXISTS";
    public const string INVALID_CREDENTIALS = "INVALID_CREDENTIALS";
    public const string USER_ACCOUNT_DISABLED = "USER_ACCOUNT_DISABLED";
    public const string IMAGE_UPLOAD_FAILED = "IMAGE_UPLOAD_FAILED";
    public const string INVALID_FILE_FORMAT = "INVALID_FILE_FORMAT";
    public const string FILE_TOO_LARGE = "FILE_TOO_LARGE";
    public const string USER_NOT_FOUND = "USER_NOT_FOUND";
    public const string INVALID_PASSWORD = "INVALID_PASSWORD";
    public const string PASSWORD_MISMATCH = "PASSWORD_MISMATCH";
    public const string ROLE_NOT_FOUND = "ROLE_NOT_FOUND";
    public const string USER_NOT_VERIFIED = "USER_NOT_VERIFIED";
}
