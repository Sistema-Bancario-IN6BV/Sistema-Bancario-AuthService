namespace AuthService_SB.Domain.Constants;

public static class RoleConstants
{
    // Roles para Sistema de Restaurantes
    public const string CUSTOMER = "CUSTOMER";
    public const string RESTAURANT_ADMIN = "RESTAURANT_ADMIN";
    public const string PLATFORM_ADMIN = "PLATFORM_ADMIN";
    // Roles para Sistema Bancario
    public const string BANK_ADMIN = "BANK_ADMIN";
    public const string CLIENT = "CLIENT";

    public static readonly string[] AllowedRoles = [CUSTOMER, RESTAURANT_ADMIN, PLATFORM_ADMIN, BANK_ADMIN, CLIENT];
}