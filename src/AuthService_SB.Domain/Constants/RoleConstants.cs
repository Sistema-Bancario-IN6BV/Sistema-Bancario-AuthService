namespace AuthService_SB.Domain.Constants;

public static class RoleConstants
{
    // Roles para Sistema Bancario
    public const string ADMIN_ROLE = "ADMIN_ROLE";
    public const string USER_ROLE = "USER_ROLE";

    public static readonly string[] AllowedRoles = [ADMIN_ROLE, USER_ROLE];
}