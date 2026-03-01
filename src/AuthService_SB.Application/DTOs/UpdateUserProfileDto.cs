namespace AuthService_SB.Application.DTOs;

public class UpdateUserProfileDto
{
    public string ProfilePicture { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string JobName { get; set; } = string.Empty;
    public decimal MonthlyIncome { get; set; }
}
