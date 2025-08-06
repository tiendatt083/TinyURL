using Microsoft.AspNetCore.Mvc;
using AuthService.Models;
using AuthService.Services;

namespace AuthService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthenticationService _authService;

        public AuthController(AuthenticationService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Đăng nhập
        /// </summary>
        /// <param name="request">Thông tin đăng nhập</param>
        /// <returns>Token và thông tin user</returns>
        [HttpPost("login")]
        public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
            {
                return BadRequest(new AuthResponse
                {
                    Success = false,
                    Message = "Username and password are required"
                });
            }

            var result = await _authService.LoginAsync(request);
            
            if (!result.Success)
            {
                return Unauthorized(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Đăng ký
        /// </summary>
        /// <param name="request">Thông tin đăng ký</param>
        /// <returns>Token và thông tin user</returns>
        [HttpPost("register")]
        public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request)
        {
            if (string.IsNullOrEmpty(request.Username) || 
                string.IsNullOrEmpty(request.Email) || 
                string.IsNullOrEmpty(request.Password))
            {
                return BadRequest(new AuthResponse
                {
                    Success = false,
                    Message = "Username, email, and password are required"
                });
            }

            var result = await _authService.RegisterAsync(request);
            
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Lấy thông tin user theo ID
        /// </summary>
        /// <param name="id">User ID</param>
        /// <returns>Thông tin user</returns>
        [HttpGet("user/{id}")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            var user = await _authService.GetUserByIdAsync(id);
            
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            // Remove password hash from response
            return Ok(new UserInfo
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email
            });
        }

        /// <summary>
        /// Kiểm tra tính hợp lệ của token
        /// </summary>
        /// <returns>Trạng thái token</returns>
        [HttpGet("validate")]
        public ActionResult ValidateToken()
        {
            // This endpoint can be used by other services to validate tokens
            return Ok(new { message = "Token is valid", timestamp = DateTime.UtcNow });
        }
    }
}
