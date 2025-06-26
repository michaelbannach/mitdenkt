using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using MitDenkt.Models;
using MitDenkt.Dtos;



namespace MitDenkt.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<Customer> _userManager;
    private readonly SignInManager<Customer> _signInManager;

    public AuthController(UserManager<Customer> userManager, SignInManager<Customer> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        var user = new Customer
        {
            UserName = dto.Email,
            Email = dto.Email,
            FirstName = dto.FirstName,
            LastName = dto.LastName
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        return result.Succeeded ? Ok("Registrierung erfolgreich.") : BadRequest(result.Errors);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var result = await _signInManager.PasswordSignInAsync(dto.Email, dto.Password, isPersistent: true, lockoutOnFailure: false);
        return result.Succeeded ? Ok("Login erfolgreich.") : Unauthorized("Login fehlgeschlagen.");
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return Ok("Logout erfolgreich.");
    }

    [HttpGet("me")]
    public IActionResult Me()
    {
        return User.Identity?.IsAuthenticated == true
            ? Ok(User.Identity.Name)
            : Unauthorized();
    }
}
