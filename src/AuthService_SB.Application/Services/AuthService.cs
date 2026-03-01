using AuthService_SB.Application.DTOs;
using AuthService_SB.Application.Interfaces;
using AuthService_SB.Application.Exceptions;
using AuthService_SB.Application.Extensions;
using AuthService_SB.Application.Validators;
using AuthService_SB.Domain.Constants;
using AuthService_SB.Domain.Entities;
using AuthService_SB.Domain.Interfaces;
using AuthService_SB.Domain.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using AuthService_SB.Application.DTOs.Email;

namespace AuthService_SB.Application.Services;

public class AuthService(
    IUserRepository userRepository,
    IRoleRepository roleRepository,
    IPasswordHashService passwordHashService,
    IJwtTokenService jwtTokenService,
    ICloudinaryService cloudinaryService,
    IEmailService emailService,
    IConfiguration configuration,
    ILogger<AuthService> logger) : IAuthService
{
    private readonly ICloudinaryService _cloudinaryService = cloudinaryService;
    public async Task<RegisterResponseDto> RegisterAsync(RegisterDto registerDto)
    {
        if (await userRepository.ExistsByEmailAsync(registerDto.Email))
        {
            logger.LogRegistrationWithExistingEmail();
            throw new BusinessException(ErrorCodes.EMAIL_ALREADY_EXISTS, "Email already exists");
        }
        if (await userRepository.ExistsByUsernameAsync(registerDto.Username))
        {
            logger.LogRegistrationWithExistingUsername();
            throw new BusinessException(ErrorCodes.USERNAME_ALREADY_EXISTS, "Username already exists");
        }

        string profilePicturePath;

        if (registerDto.ProfilePicture != null && registerDto.ProfilePicture.Size > 0)
        {
            var (isValid, errorMessage) = FileValidator.ValidateImage(registerDto.ProfilePicture);
            if (!isValid)
            {
                logger.LogWarning($"File validation failed: {errorMessage}");
                throw new BusinessException(ErrorCodes.INVALID_FILE_FORMAT, errorMessage!);
            }

            try
            {
                var fileName = FileValidator.GenerateSecureFileName(registerDto.ProfilePicture.FileName);
                profilePicturePath = await _cloudinaryService.UploadImageAsync(registerDto.ProfilePicture, fileName);
            }
            catch (Exception)
            {
                logger.LogImageUploadError();
                throw new BusinessException(ErrorCodes.IMAGE_UPLOAD_FAILED, "Failed to upload profile image");
            }
        }
        else
        {
            profilePicturePath = _cloudinaryService.GetDefaultAvatarUrl();
        }

        var emailVerificationToken = TokenGeneratorService.GenerateEmailVerificationToken();

        var userId = UuidGenerator.GenerateUserId();
        var userProfileId = UuidGenerator.GenerateUserId();
        var userEmailId = UuidGenerator.GenerateUserId();
        var userRoleId = UuidGenerator.GenerateUserId();

        var defaultRole = await roleRepository.GetByNameAsync(RoleConstants.USER_ROLE);
        if (defaultRole == null)
        {
            throw new InvalidOperationException($"Default role '{RoleConstants.USER_ROLE}' not found. Ensure seeding runs before registration.");
        }

        var user = new User
        {
            Id = userId,
            Name = registerDto.Name,
            Surname = registerDto.Surname,
            Username = registerDto.Username,
            Email = registerDto.Email.ToLowerInvariant(),
            Password = passwordHashService.HashPassword(registerDto.Password),
            Status = false,
            UserProfile = new UserProfile
            {
                Id = userProfileId,
                UserId = userId,
                ProfilePicture = profilePicturePath,
                Phone = registerDto.Phone,
                Dpi = registerDto.Dpi,
                Address = registerDto.Address,
                JobName = registerDto.JobName,
                MonthlyIncome = registerDto.MonthlyIncome
            },
            UserEmail = new UserEmail
            {
                Id = userEmailId,
                UserId = userId,
                EmailVerified = false,
                EmailVerificationToken = emailVerificationToken,
                EmailVerificationTokenExpiry = DateTime.UtcNow.AddHours(24)
            },
            UserRoles =
            [
                new Domain.Entities.UserRole
                {
                    Id = userRoleId,
                    UserId = userId,
                    RoleId = defaultRole.Id
                }
            ],
            UserPasswordReset = new UserPasswordReset //Generar el objeto.
            {
                Id = UuidGenerator.GenerateUserId(),
                UserId = userId,
                PasswordResetToken = null,
                PasswordResetTokenExpiry = null
            },
        };

        // Guardar usuario y entidades relacionadas
        var createdUser = await userRepository.CreateAsync(user);

        logger.LogUserRegistered(createdUser.Username);

        // Enviar email de verificación en background
        _ = Task.Run(async () =>
        {
            try
            {
                await emailService.SendEmailVerificationAsync(createdUser.Email, createdUser.Username, emailVerificationToken);
                logger.LogInformation("Verification email sent");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to send verification email");
            }
        });

        return new RegisterResponseDto
        {
            Success = true,
            User = MapToUserResponseDto(createdUser),
            Message = "Usuario registrado exitosamente. Por favor, verifica tu email para activar la cuenta.",
            EmailVerificationRequired = true
        };
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto)
    {
        // Buscar usuario por email o username
        User? user = null;

        if (loginDto.EmailOrUsername.Contains('@'))
        {
            // Es un email
            user = await userRepository.GetByEmailAsync(loginDto.EmailOrUsername.ToLowerInvariant());
        }
        else
        {
            // Es un username
            user = await userRepository.GetByUsernameAsync(loginDto.EmailOrUsername);
        }

        // Verificar si el usuario existe
        if (user == null)
        {
            logger.LogFailedLoginAttempt();
            throw new UnauthorizedAccessException("Invalid credentials");
        }

        // Verificar si el usuario está activo
        if (!user.Status)
        {
            logger.LogFailedLoginAttempt();
            throw new BusinessException(ErrorCodes.USER_NOT_VERIFIED, "Debes verificar tu correo electrónico antes de iniciar sesión");
        }

        // Verificar contraseña
        if (!passwordHashService.VerifyPassword(loginDto.Password, user.Password))
        {
            logger.LogFailedLoginAttempt();
            throw new UnauthorizedAccessException("Invalid credentials");
        }

        logger.LogUserLoggedIn();

        // Generar token JWT
        var token = jwtTokenService.GenerateToken(user);
        var expiryMinutes = int.Parse(configuration["JwtSettings:ExpiryInMinutes"] ?? "30");

        // Crear respuesta compacta
        return new AuthResponseDto
        {
            Success = true,
            Message = "Login exitoso",
            Token = token,
            UserDetails = MapToUserDetailsDto(user),
            ExpiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes)
        };
    }

    private UserResponseDto MapToUserResponseDto(User user)
    {
        var userRole = user.UserRoles.FirstOrDefault()?.Role?.Name ?? RoleConstants.USER_ROLE;
        return new UserResponseDto
        {
            Id = user.Id,
            Name = user.Name,
            Surname = user.Surname,
            Username = user.Username,
            Email = user.Email,
            ProfilePicture = _cloudinaryService.GetFullImageUrl(user.UserProfile?.ProfilePicture ?? string.Empty),
            Phone = user.UserProfile?.Phone ?? string.Empty,
            Dpi = user.UserProfile?.Dpi ?? string.Empty,
            Address = user.UserProfile?.Address ?? string.Empty,
            JobName = user.UserProfile?.JobName ?? string.Empty,
            MonthlyIncome = user.UserProfile?.MonthlyIncome ?? 0,
            Role = userRole,
            Status = user.Status,
            IsEmailVerified = user.UserEmail?.EmailVerified ?? false,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        };
    }

    private UserDetailsDto MapToUserDetailsDto(User user)
    {
        return new UserDetailsDto
        {
            Id = user.Id,
            Username = user.Username,
            ProfilePicture = _cloudinaryService.GetFullImageUrl(user.UserProfile?.ProfilePicture ?? string.Empty),
            Role = user.UserRoles.FirstOrDefault()?.Role?.Name ?? RoleConstants.USER_ROLE
        };
    }

    public async Task<EmailResponseDto> VerifyEmailAsync(VerifyEmailDto verifyEmailDto)
    {
        var user = await userRepository.GetByEmailVerificationTokenAsync(verifyEmailDto.Token);
        if (user == null || user.UserEmail == null)
        {
            return new EmailResponseDto
            {
                Success = false,
                Message = "Invalid or expired verification token"
            };
        }

        user.UserEmail.EmailVerified = true;
        user.Status = true;
        user.UserEmail.EmailVerificationToken = null;
        user.UserEmail.EmailVerificationTokenExpiry = null;

        await userRepository.UpdateAsync(user);

        // Enviar email de bienvenida
        try
        {
            await emailService.SendWelcomeEmailAsync(user.Email, user.Username);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send welcome email to {Email}", user.Email);
        }

        logger.LogInformation("Email verified successfully for user {Username}", user.Username);

        return new EmailResponseDto
        {
            Success = true,
            Message = "Email verificado exitosamente",
            Data = new
            {
                email = user.Email,
                verified = true
            }
        };
    }

    public async Task<EmailResponseDto> ResendVerificationEmailAsync(ResendVerificationDto resendDto)
    {
        var user = await userRepository.GetByEmailAsync(resendDto.Email);
        if (user == null || user.UserEmail == null)
        {
            return new EmailResponseDto
            {
                Success = false,
                Message = "Usuario no encontrado",
                Data = new { email = resendDto.Email, sent = false }
            };
        }

        if (user.UserEmail.EmailVerified)
        {
            return new EmailResponseDto
            {
                Success = false,
                Message = "El email ya ha sido verificado",
                Data = new { email = user.Email, verified = true }
            };
        }

        // Generar nuevo token
        var newToken = TokenGeneratorService.GenerateEmailVerificationToken();
        user.UserEmail.EmailVerificationToken = newToken;
        user.UserEmail.EmailVerificationTokenExpiry = DateTime.UtcNow.AddHours(24);

        await userRepository.UpdateAsync(user);

        // Enviar email
        try
        {
            await emailService.SendEmailVerificationAsync(user.Email, user.Username, newToken);
            return new EmailResponseDto
            {
                Success = true,
                Message = "Email de verificación enviado exitosamente",
                Data = new { email = user.Email, sent = true }
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to resend verification email to {Email}", user.Email);
            return new EmailResponseDto
            {
                Success = false,
                Message = "Error al enviar el email de verificación",
                Data = new { email = user.Email, sent = false }
            };
        }
    }

    public async Task<EmailResponseDto> ForgotPasswordAsync(ForgotPasswordDto forgotPasswordDto)
    {
        var user = await userRepository.GetByEmailAsync(forgotPasswordDto.Email);
        if (user == null)
        {
            // Por seguridad, siempre devolvemos éxito aunque el usuario no exista
            return new EmailResponseDto
            {
                Success = true,
                Message = "Si el email existe, se ha enviado un enlace de recuperación",
                Data = new { email = forgotPasswordDto.Email, initiated = true }
            };
        }

        // Generar token de reset
        var resetToken = TokenGeneratorService.GeneratePasswordResetToken();

        if (user.UserPasswordReset == null)
        {
            user.UserPasswordReset = new UserPasswordReset
            {
                UserId = user.Id,
                PasswordResetToken = resetToken,
                PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(1)
            };
        }
        else
        {
            user.UserPasswordReset.PasswordResetToken = resetToken;
            user.UserPasswordReset.PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(1); // 1 hora para resetear
        }

        await userRepository.UpdateAsync(user);

        // Enviar email
        try
        {
            await emailService.SendPasswordResetAsync(user.Email, user.Username, resetToken);
            logger.LogInformation("Password reset email sent to {Email}", user.Email);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send password reset email to {Email}", user.Email);
        }

        return new EmailResponseDto
        {
            Success = true,
            Message = "Si el email existe, se ha enviado un enlace de recuperación",
            Data = new { email = forgotPasswordDto.Email, initiated = true }
        };
    }

    public async Task<EmailResponseDto> ResetPasswordAsync(ResetPasswordDto resetPasswordDto)
    {
        var user = await userRepository.GetByPasswordResetTokenAsync(resetPasswordDto.Token);
        if (user == null || user.UserPasswordReset == null)
        {
            return new EmailResponseDto
            {
                Success = false,
                Message = "Token de reset inválido o expirado",
                Data = new { token = resetPasswordDto.Token, reset = false }
            };
        }

        // Actualizar contraseña
        user.Password = passwordHashService.HashPassword(resetPasswordDto.NewPassword);
        user.UserPasswordReset.PasswordResetToken = null;
        user.UserPasswordReset.PasswordResetTokenExpiry = null;

        await userRepository.UpdateAsync(user);

        logger.LogInformation("Password reset successfully for user {Username}", user.Username);

        return new EmailResponseDto
        {
            Success = true,
            Message = "Contraseña actualizada exitosamente",
            Data = new { email = user.Email, reset = true }
        };
    }

    public async Task<UserResponseDto?> GetUserByIdAsync(string userId)
    {
        var user = await userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            return null;
        }

        return MapToUserResponseDto(user);
    }

    public async Task<bool> ChangePasswordAsync(string userId, ChangePasswordDto changePasswordDto)
    {
        var user = await userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            throw new BusinessException(ErrorCodes.USER_NOT_FOUND, "User not found");
        }

        // Validate current password
        if (!passwordHashService.VerifyPassword(changePasswordDto.CurrentPassword, user.Password))
        {
            throw new BusinessException(ErrorCodes.INVALID_PASSWORD, "Current password is incorrect");
        }

        // Validate new password matches confirm password
        if (changePasswordDto.NewPassword != changePasswordDto.ConfirmPassword)
        {
            throw new BusinessException(ErrorCodes.PASSWORD_MISMATCH, "New password and confirm password do not match");
        }

        // Update password
        user.Password = passwordHashService.HashPassword(changePasswordDto.NewPassword);
        await userRepository.UpdateAsync(user);

        logger.LogInformation("Password changed successfully for user {Username}", user.Username);
        return true;
    }

    public async Task<UserResponseDto?> UpdateUserProfileAsync(string userId, UpdateUserProfileDto updateUserProfileDto)
    {
        var user = await userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            return null;
        }

        // Update profile fields
        if (user.UserProfile != null)
        {
            if (!string.IsNullOrEmpty(updateUserProfileDto.Phone))
            {
                user.UserProfile.Phone = updateUserProfileDto.Phone;
            }
            if (!string.IsNullOrEmpty(updateUserProfileDto.Address))
            {
                user.UserProfile.Address = updateUserProfileDto.Address;
            }
            if (!string.IsNullOrEmpty(updateUserProfileDto.JobName))
            {
                user.UserProfile.JobName = updateUserProfileDto.JobName;
            }
            if (updateUserProfileDto.MonthlyIncome > 0)
            {
                user.UserProfile.MonthlyIncome = updateUserProfileDto.MonthlyIncome;
            }
            if (!string.IsNullOrEmpty(updateUserProfileDto.ProfilePicture))
            {
                user.UserProfile.ProfilePicture = updateUserProfileDto.ProfilePicture;
            }
        }

        await userRepository.UpdateAsync(user);
        logger.LogInformation("User profile updated successfully for user {Username}", user.Username);

        return MapToUserResponseDto(user);
    }

    public async Task<UserResponseDto?> UpdateClientProfileAsync(string userId, UpdateClientProfileDto updateClientProfileDto)
    {
        var user = await userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            return null;
        }

        // Update client profile fields
        user.Name = updateClientProfileDto.Name;
        user.Surname = updateClientProfileDto.Surname;

        if (user.UserProfile != null)
        {
            user.UserProfile.Address = updateClientProfileDto.Address;
            user.UserProfile.JobName = updateClientProfileDto.JobName;
            user.UserProfile.MonthlyIncome = updateClientProfileDto.MonthlyIncome;
        }

        await userRepository.UpdateAsync(user);
        logger.LogInformation("Client profile updated successfully for user {Username}", user.Username);

        return MapToUserResponseDto(user);
    }

    public async Task<UserResponseDto?> CreateUserByAdminAsync(CreateUserByAdminDto createUserByAdminDto)
    {
        // Verificar si el email ya existe
        if (await userRepository.ExistsByEmailAsync(createUserByAdminDto.Email))
        {
            logger.LogRegistrationWithExistingEmail();
            throw new BusinessException(ErrorCodes.EMAIL_ALREADY_EXISTS, "Email already exists");
        }

        // Verificar si el username ya existe
        if (await userRepository.ExistsByUsernameAsync(createUserByAdminDto.Username))
        {
            logger.LogRegistrationWithExistingUsername();
            throw new BusinessException(ErrorCodes.USERNAME_ALREADY_EXISTS, "Username already exists");
        }

        // Validar y manejar la imagen de perfil
        string profilePicturePath;

        if (createUserByAdminDto.ProfilePicture != null && createUserByAdminDto.ProfilePicture.Size > 0)
        {
            var (isValid, errorMessage) = FileValidator.ValidateImage(createUserByAdminDto.ProfilePicture);
            if (!isValid)
            {
                logger.LogWarning($"File validation failed: {errorMessage}");
                throw new BusinessException(ErrorCodes.INVALID_FILE_FORMAT, errorMessage!);
            }

            try
            {
                var fileName = FileValidator.GenerateSecureFileName(createUserByAdminDto.ProfilePicture.FileName);
                profilePicturePath = await _cloudinaryService.UploadImageAsync(createUserByAdminDto.ProfilePicture, fileName);
            }
            catch (Exception)
            {
                logger.LogImageUploadError();
                throw new BusinessException(ErrorCodes.IMAGE_UPLOAD_FAILED, "Failed to upload profile image");
            }
        }
        else
        {
            profilePicturePath = _cloudinaryService.GetDefaultAvatarUrl();
        }

        // Obtener el rol especificado
        var role = await roleRepository.GetByNameAsync(createUserByAdminDto.RoleName);
        if (role == null)
        {
            throw new BusinessException(ErrorCodes.ROLE_NOT_FOUND, $"Role '{createUserByAdminDto.RoleName}' not found");
        }

        var userId = UuidGenerator.GenerateUserId();
        var userProfileId = UuidGenerator.GenerateUserId();
        var userEmailId = UuidGenerator.GenerateUserId();
        var userRoleId = UuidGenerator.GenerateUserId();

        // Crear nuevo usuario
        var user = new User
        {
            Id = userId,
            Name = createUserByAdminDto.Name,
            Surname = createUserByAdminDto.Surname,
            Username = createUserByAdminDto.Username,
            Email = createUserByAdminDto.Email.ToLowerInvariant(),
            Password = passwordHashService.HashPassword(createUserByAdminDto.Password),
            Status = true, // Admin-created users are active by default
            UserProfile = new UserProfile
            {
                Id = userProfileId,
                UserId = userId,
                ProfilePicture = profilePicturePath,
                Phone = createUserByAdminDto.Phone,
                Dpi = createUserByAdminDto.Dpi,
                Address = createUserByAdminDto.Address,
                JobName = createUserByAdminDto.JobName,
                MonthlyIncome = createUserByAdminDto.MonthlyIncome
            },
            UserEmail = new UserEmail
            {
                Id = userEmailId,
                UserId = userId,
                EmailVerified = true, // Admin-created users have verified email
                EmailVerificationToken = null,
                EmailVerificationTokenExpiry = null
            },
            UserRoles =
            [
                new Domain.Entities.UserRole
                {
                    Id = userRoleId,
                    UserId = userId,
                    RoleId = role.Id
                }
            ],
            UserPasswordReset = new UserPasswordReset
            {
                Id = UuidGenerator.GenerateUserId(),
                UserId = userId,
                PasswordResetToken = null,
                PasswordResetTokenExpiry = null
            }
        };

        var createdUser = await userRepository.CreateAsync(user);
        logger.LogUserRegistered(createdUser.Username);

        return MapToUserResponseDto(createdUser);
    }

    public async Task<UserResponseDto?> UpdateUserByAdminAsync(string userId, UpdateUserByAdminDto updateUserByAdminDto)
    {
        var user = await userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            return null;
        }

        // Update basic user fields if provided
        if (!string.IsNullOrEmpty(updateUserByAdminDto.Name))
        {
            user.Name = updateUserByAdminDto.Name;
        }
        if (!string.IsNullOrEmpty(updateUserByAdminDto.Surname))
        {
            user.Surname = updateUserByAdminDto.Surname;
        }
        if (!string.IsNullOrEmpty(updateUserByAdminDto.Email))
        {
            user.Email = updateUserByAdminDto.Email.ToLowerInvariant();
        }

        // Update profile fields if provided
        if (user.UserProfile != null)
        {
            if (!string.IsNullOrEmpty(updateUserByAdminDto.Phone))
            {
                user.UserProfile.Phone = updateUserByAdminDto.Phone;
            }
            if (!string.IsNullOrEmpty(updateUserByAdminDto.Address))
            {
                user.UserProfile.Address = updateUserByAdminDto.Address;
            }
            if (!string.IsNullOrEmpty(updateUserByAdminDto.JobName))
            {
                user.UserProfile.JobName = updateUserByAdminDto.JobName;
            }
            if (updateUserByAdminDto.MonthlyIncome.HasValue && updateUserByAdminDto.MonthlyIncome.Value > 0)
            {
                user.UserProfile.MonthlyIncome = updateUserByAdminDto.MonthlyIncome.Value;
            }

            // Handle profile picture upload if provided
            if (updateUserByAdminDto.ProfilePicture != null && updateUserByAdminDto.ProfilePicture.Size > 0)
            {
                var (isValid, errorMessage) = FileValidator.ValidateImage(updateUserByAdminDto.ProfilePicture);
                if (!isValid)
                {
                    logger.LogWarning($"File validation failed: {errorMessage}");
                    throw new BusinessException(ErrorCodes.INVALID_FILE_FORMAT, errorMessage!);
                }

                try
                {
                    var fileName = FileValidator.GenerateSecureFileName(updateUserByAdminDto.ProfilePicture.FileName);
                    var profilePicturePath = await _cloudinaryService.UploadImageAsync(updateUserByAdminDto.ProfilePicture, fileName);
                    user.UserProfile.ProfilePicture = profilePicturePath;
                }
                catch (Exception)
                {
                    logger.LogImageUploadError();
                    throw new BusinessException(ErrorCodes.IMAGE_UPLOAD_FAILED, "Failed to upload profile image");
                }
            }
        }

        await userRepository.UpdateAsync(user);
        logger.LogInformation("User updated successfully by admin for user {Username}", user.Username);

        return MapToUserResponseDto(user);
    }

    public async Task<bool> DeleteUserAsync(string userId)
    {
        var user = await userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            return false;
        }

        // Soft delete: set Status to false instead of removing from database
        var result = await userRepository.SetStatusAsync(userId, false);
        if (result)
        {
            logger.LogInformation("User deactivated successfully (soft delete): {UserId}", userId);
        }

        return result;
    }
}
