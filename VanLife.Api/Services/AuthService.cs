using VanLife.Api.Data;
using VanLife.Api.Models;

namespace VanLife.Api.Services;

public class AuthService(InMemoryStore store)
{
    public object SignUp(SignUpRequest request)
    {
        if (request.Password != request.ConfirmPassword)
        {
            return new { success = false, message = "Password and confirm password must match." };
        }

        var exists = store.Users.Any(u => u.Email.Equals(request.Email, StringComparison.OrdinalIgnoreCase));
        if (exists)
        {
            return new { success = false, message = "Email already exists." };
        }

        var user = new UserAccount
        {
            Id = Guid.NewGuid(),
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Password = request.Password,
            Role = request.Role
        };

        store.Users.Add(user);
        return new { success = true, message = "Account created.", userId = user.Id };
    }

    public object Login(LoginRequest request, UserRole expectedRole)
    {
        var user = store.Users.FirstOrDefault(u =>
            u.Email.Equals(request.Email, StringComparison.OrdinalIgnoreCase) &&
            u.Password == request.Password &&
            u.Role == expectedRole);

        if (user is null)
        {
            return new { success = false, message = "Invalid credentials for this login type." };
        }

        return new
        {
            success = true,
            message = $"{expectedRole} login successful.",
            user = new { user.Id, user.FirstName, user.LastName, user.Email, user.Role }
        };
    }

    public object ForgotPassword(ForgotPasswordRequest request)
    {
        var user = store.Users.FirstOrDefault(u => u.Email.Equals(request.Email, StringComparison.OrdinalIgnoreCase));
        if (user is null)
        {
            return new { success = false, message = "Email not found." };
        }

        var fakeToken = Guid.NewGuid().ToString("N");
        return new
        {
            success = true,
            message = "Reset token created. In production, email this token.",
            resetToken = fakeToken
        };
    }

    public object ResetPassword(ResetPasswordRequest request)
    {
        if (request.NewPassword != request.ConfirmPassword)
        {
            return new { success = false, message = "New password and confirm password must match." };
        }

        var user = store.Users.FirstOrDefault(u => u.Email.Equals(request.Email, StringComparison.OrdinalIgnoreCase));
        if (user is null)
        {
            return new { success = false, message = "Email not found." };
        }

        user.Password = request.NewPassword;
        return new { success = true, message = "Password has been reset." };
    }
}

