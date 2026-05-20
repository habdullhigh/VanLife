using Microsoft.EntityFrameworkCore;
using VanLife.Api.Data;
using VanLife.Api.Models;

namespace VanLife.Api.Services;

public class AuthService(AppDbContext db)
{
    public async Task<object> SignUp(SignUpRequest request)
    {
        if (request.Password != request.ConfirmPassword)
        {
            return new { success = false, message = "Password and confirm password must match." };
        }

        var exists = await db.Users.AnyAsync(u => u.Email.ToLower() == request.Email.ToLower());
        if (exists)
        {
            return new { success = false, message = "Email already exists." };
        }

        var user = new UserAccount
        {
            Id = Guid.NewGuid(),
            FirstName = request.FirstName,
            LastName = request.LastName,
            Username = request.Username,
            IdNumber = request.IdNumber,
            Address = request.Address,
            Phone = request.Phone,
            NextOfKin = request.NextOfKin,
            Email = request.Email,
            Password = PasswordHasher.Hash(request.Password),
            Role = request.Role
        };

        db.Users.Add(user);

        if (request.Role == UserRole.Seller)
        {
            db.Sellers.Add(new Seller { SellerId = user.Id, Username = user.Username });
        }
        else if (request.Role == UserRole.Buyer)
        {
            db.Buyers.Add(new Buyer { BuyerId = user.Id, Username = user.Username });
        }

        await db.SaveChangesAsync();
        return new { success = true, message = "Account created.", userId = user.Id };
    }

    public async Task<object> Login(LoginRequest request)
    {
        var user = await db.Users.FirstOrDefaultAsync(u =>
            u.Email.ToLower() == request.Email.ToLower() &&
            u.Role == request.Role);

        if (user is null || !PasswordHasher.Verify(request.Password, user.Password))
        {
            return new { success = false, message = "Invalid credentials for this login type." };
        }

        return new
        {
            success = true,
            message = $"{request.Role} login successful.",
            user = new { user.Id, user.FirstName, user.LastName, user.Email, user.Role }
        };
    }

    public async Task<object> ForgotPassword(ForgotPasswordRequest request)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower());
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

    public async Task<object> ResetPassword(ResetPasswordRequest request)
    {
        if (request.NewPassword != request.ConfirmPassword)
        {
            return new { success = false, message = "New password and confirm password must match." };
        }

        var user = await db.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower());
        if (user is null)
        {
            return new { success = false, message = "Email not found." };
        }

        user.Password = PasswordHasher.Hash(request.NewPassword);
        await db.SaveChangesAsync();
        return new { success = true, message = "Password has been reset." };
    }
}
