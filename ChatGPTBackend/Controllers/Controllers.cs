using System;
using System.Text;
using System.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

using ChatGPTBackend.Models;
using ChatGPTBackend.Services;


namespace ChatGPTBackend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        public IConfiguration _configuration { get; }
        private readonly IUserService _userService;

        public AuthController(IConfiguration configuration, IUserService userService)
        {
            _configuration = configuration;
            _userService = userService;
        }

        [HttpPost("login")]
        public IActionResult Login(LoginRequest model)
        {
            var user = _userService?.Authenticate(model.Username, model.Password);
            if (user == null)
            {
                return Unauthorized(new { message = "Invalid username or password" });
            }

            return Ok(new
            {
                Id = user.Id,
                Username = user.Username,
                Token = GenerateJwtToken(user)
            });
        }

        // Helper method to generate JWT token
        private string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["JwtSettings:Secret"] ?? "");
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim("id", user?.Id?.ToString() ?? String.Empty)
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }

    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet("{id}")]
        public IActionResult GetUser(string id)
        {
            var user = _userService.GetUserById(id);
            if (user == null)
            {
                return NotFound();
            }
            return Ok(new { Data = user });
        }
    }

    [ApiController]
    [Route("[controller]")]
    public class QueryController : ControllerBase
    {
        private readonly IQueryService _queryService;
        private readonly IRequestService _requestService;

        public QueryController(IQueryService queryService, IRequestService requestService)
        {
            _queryService = queryService;
            _requestService = requestService;
        }

        [HttpPost]
        public async Task<IActionResult> PostQuery(QueryRequest model)
        {
            string query = model.Text;

            // Handle the query and generate response
            string? response = await _requestService.GetGptResponse(query);

            if (response != null) {
                // Save the query to the database
                _queryService.SaveQuery(new Query
                {
                    UserId = "USER_ID", // todo: get user id from token,
                    QueryText = query,
                    ResponseText = response
                });
            }
            else
            {
                response = "ChatGPT Error.";
            }

            // Return response
            return Ok(new { Data = response });
        }

        [HttpGet("{userId}")]
        public IActionResult GetUserQueries(string userId)
        {
            var queries = _queryService.GetUserQueries(userId);
            if (queries == null)
            {
                return NotFound();
            }
            return Ok(new { Data = queries });
        }
    }

    // Models for request payloads

    public class LoginRequest
    {
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
    }

    public class QueryRequest
    {
        public string Text { get; set; } = "";
    }
}