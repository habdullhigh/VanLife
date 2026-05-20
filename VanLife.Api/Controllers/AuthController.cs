using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VanLife.Api.Models;
using VanLife.Api.Services;

namespace VanLife.Api.Controllers;

[ApiController]
[Route("api/auth")]
[Consumes("application/json")]
public class AuthController(AuthService authService) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("signup")]
    public async Task<IActionResult> SignUp([FromBody] SignUpRequest request)
    {
        var result = await authService.SignUp(request);
        var success = result.GetType().GetProperty("success")?.GetValue(result) as bool? ?? false;
        return success ? Ok(result) : BadRequest(result);
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await authService.Login(request);
        var success = result.GetType().GetProperty("success")?.GetValue(result) as bool? ?? false;
        return success ? Ok(result) : Unauthorized(result);
    }

    [AllowAnonymous]
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        var result = await authService.ForgotPassword(request);
        var success = result.GetType().GetProperty("success")?.GetValue(result) as bool? ?? false;
        return success ? Ok(result) : NotFound(result);
    }

    [AllowAnonymous]
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        var result = await authService.ResetPassword(request);
        var success = result.GetType().GetProperty("success")?.GetValue(result) as bool? ?? false;
        return success ? Ok(result) : BadRequest(result);
    }
}
