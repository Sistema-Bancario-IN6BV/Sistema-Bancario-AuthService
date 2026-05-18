using AuthService_SB.Application.DTOs;
using AuthService_SB.Application.DTOs.Email;

namespace AuthService_SB.Application.Interfaces;

public interface IAuthService
{
    Task<RegisterResponseDto> RegisterAsync(RegisterDto registerDto);
    Task<AuthResponseDto> LoginAsync(LoginDto loginDto);
    Task<EmailResponseDto> VerifyEmailAsync(VerifyEmailDto verifyEmailDto);
    Task<EmailResponseDto> ResendVerificationEmailAsync(ResendVerificationDto resendDto);
    Task<EmailResponseDto> ForgotPasswordAsync(ForgotPasswordDto forgotPasswordDto);
    Task<EmailResponseDto> ResetPasswordAsync(ResetPasswordDto resetPasswordDto);
    Task<UserResponseDto?> GetUserByIdAsync(string userId);
    Task<bool> ChangePasswordAsync(string userId, ChangePasswordDto changePasswordDto);
    Task<UserResponseDto?> UpdateUserProfileAsync(string userId, UpdateUserProfileDto updateUserProfileDto);
    Task<UserResponseDto?> UpdateClientProfileAsync(string userId, UpdateClientProfileDto updateClientProfileDto);
    Task<UserResponseDto?> CreateUserByAdminAsync(CreateUserByAdminDto createUserByAdminDto);
    Task<UserResponseDto?> UpdateUserByAdminAsync(string userId, UpdateUserByAdminDto updateUserByAdminDto);
    Task<bool> DeleteUserAsync(string userId);
}