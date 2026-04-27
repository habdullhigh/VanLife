using Microsoft.AspNetCore.Mvc;
using VanLife.Api.Models;
using VanLife.Api.Services;

namespace VanLife.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(AuthService authService) : ControllerBase
{
    [HttpPost("signup")]
    public IActionResult SignUp([FromBody] SignUpRequest request) => Ok(authService.SignUp(request));

    [HttpPost("login/buyer")]
    public IActionResult BuyerLogin([FromBody] LoginRequest request) => Ok(authService.Login(request, UserRole.Buyer));

    [HttpPost("login/seller")]
    public IActionResult SellerLogin([FromBody] LoginRequest request) => Ok(authService.Login(request, UserRole.Seller));

    [HttpPost("login/admin")]
    public IActionResult AdminLogin([FromBody] LoginRequest request) => Ok(authService.Login(request, UserRole.Admin));

    [HttpPost("forgot-password")]
    public IActionResult ForgotPassword([FromBody] ForgotPasswordRequest request) => Ok(authService.ForgotPassword(request));

    [HttpPost("reset-password")]
    public IActionResult ResetPassword([FromBody] ResetPasswordRequest request) => Ok(authService.ResetPassword(request));
}

