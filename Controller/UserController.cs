using Client_Api.Model;
using Client_Api.Service.Interface;
using Microsoft.AspNetCore.Mvc;


namespace Client_Api.Controller;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateUser([FromBody] RegisterModel registerModel)
    {
        var user = new User
        {
            FirstName = registerModel.FirstName,
            LastName = registerModel.LastName,
            Username = registerModel.Username,
            Adress = registerModel.Adress,
        };
        var userId = await _userService.CreateUser(registerModel.Email, registerModel.Password, user);
        return Ok(new { UserId = userId });
    }

    [HttpGet("{userId}")]
    public async Task<IActionResult> GetUserById(string userId)
    {
        var user = await _userService.GetUserById(userId);
        if (user == null)
            return NotFound();
        return Ok(user);
    }

    [HttpGet("all")]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _userService.GetAllUsers();
        return Ok(users);
    }

    [HttpPut("{userId}")]
    public async Task<IActionResult> UpdateUser(string userId, [FromBody] User user)
    {
        await _userService.UpdateUser(userId, user);
        return NoContent();
    }

    [HttpDelete("{userId}")]
    public async Task<IActionResult> DeleteUser(string userId)
    {
        await _userService.DeleteUser(userId);
        return NoContent();
    }

    [HttpPost("login")]
    public async Task<IActionResult> LoginUser([FromBody] LoginModel loginModel)
    {
        var token = await _userService.LoginUser(loginModel.Email, loginModel.Password);
        return Ok(new { Token = token });
    }

    [HttpPost("logout")]
    public async Task<IActionResult> LogoutUser([FromBody] string uid)
    {
        await _userService.LogoutUser(uid);
        return NoContent();
    }
}