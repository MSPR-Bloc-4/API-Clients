using Client_Api.Model;
using Client_Api.Repository.Interface;
using Microsoft.AspNetCore.Mvc;

namespace Client_Api.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository _userRepository;

        public UserController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        // Register a new user
        [HttpPost("register")]
        public async Task<IActionResult> CreateUser([FromBody] RegisterModel registerModel)
        {
            var user = new User
            {
                FirstName = registerModel.FirstName,
                LastName = registerModel.LastName,
                Username = registerModel.Username,
                Adress = registerModel.Adress,
                OrderIds = registerModel.OrderIds
            };

            var userId = await _userRepository.CreateUser(registerModel.Email, registerModel.Password, user);
            return Ok(new { UserId = userId });
        }

        // Login an existing user
        [HttpPost("login")]
        public async Task<IActionResult> LoginUser([FromBody] LoginModel loginModel)
        {
            var token = await _userRepository.LoginUser(loginModel.Email, loginModel.Password);
            return Ok(new { Token = token });
        }

        // Logout a user
        [HttpPost("logout")]
        public async Task<IActionResult> LogoutUser([FromBody] string uid)
        {
            await _userRepository.LogoutUser(uid);
            return Ok();
        }

        // Get a user by ID
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetUserById(string userId)
        {
            var user = await _userRepository.GetUserById(userId);
            if (user == null)
            {
                return NotFound();
            }
            return Ok(user);
        }

        // Get all users
        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userRepository.GetAllUsers();
            return Ok(users);
        }

        // Update a user by ID
        [HttpPut("{userId}")]
        public async Task<IActionResult> UpdateUser(string userId, [FromBody] User user)
        {
            await _userRepository.UpdateUser(userId, user);
            return NoContent();
        }

        // Delete a user by ID
        [HttpDelete("{userId}")]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            await _userRepository.DeleteUser(userId);
            return NoContent();
        }
    }
}
