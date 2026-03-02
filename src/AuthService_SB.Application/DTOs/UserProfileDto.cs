namespace AuthService_SB.Application.DTOs;

public class UserProfileDto
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string ProfilePicture { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Dpi { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string JobName { get; set; } = string.Empty;
    public decimal MonthlyIncome { get; set; }
}
