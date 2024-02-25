using System;
using System.Text;
using System.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

using ChatGPTBackend.Models;
using ChatGPTBackend.Middlewares;
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
            }); // todo: response data representation on client: { id, username, token }
        }

        private string GenerateJwtToken(User user)
        {
            try {
                var tokenHandler = new JwtSecurityTokenHandler();
                string? secret = (_configuration["JwtSettings:Secret"] ?? "").Length > 1 
                    ? _configuration["JwtSettings:Secret"] 
                    : GenerateRandomKey(32);
                var key = Encoding.ASCII.GetBytes(secret ?? "");
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new Claim[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, $"{user?.Id}")
                    }),
                    Expires = DateTime.UtcNow.AddDays(7),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                };
                var token = tokenHandler.CreateToken(tokenDescriptor);
                return tokenHandler.WriteToken(token);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return "token";
            }
        }

        // TODO: Generating a Random Key is inefficient; should be persisted, to be later retrieved to validate a user access-token
        private string GenerateRandomKey(int keySizeInBytes)
        {
            // Create a new byte array to store the key
            byte[] key = new byte[keySizeInBytes];

            // Use RNGCryptoServiceProvider to generate a secure random key
            using (var rng = RandomNumberGenerator.Create()) // new RNGCryptoServiceProvider()) - obsolete method
            {
                rng.GetBytes(key);
            }

            // Convert the byte array to a base64-encoded string
            string base64Key = Convert.ToBase64String(key);

            // Console.WriteLine(base64Key);
            return base64Key;
        }
    }

    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUserService _userService;

        public UserController(IHttpContextAccessor httpContextAccessor, IUserService userService)
        {
            _httpContextAccessor = httpContextAccessor;
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
    [ServiceFilter(typeof(JwtAuthFilter))]
    public class QueryController : ControllerBase
    {
        public IConfiguration _configuration { get; }
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IQueryService _queryService;
        private readonly IRequestService _requestService;

        public QueryController(IConfiguration configuration, IHttpContextAccessor httpContextAccessor, IQueryService queryService, IRequestService requestService)
        {
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _queryService = queryService;
            _requestService = requestService;
        }

        [HttpPost]
        public async Task<IActionResult> PostQuery(QueryRequest model)
        {
            string query = model.Query;
            string userId = model.UserId; // todo: remove after it's not needed anymore

            // TODO: Handle retrieving the user-id from the access-token
            /** 
            string? userId = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
            Console.WriteLine($"1 - {userId}");
            
            userId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
            Console.WriteLine($"2 - {userId}");

            userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
            Console.WriteLine($"3 - {userId}");

            userId = _httpContextAccessor.HttpContext?.Items?["Authorized-User-Id"]?.ToString() ?? "";
            Console.WriteLine($"4 - {userId}");

            userId = HttpContext?.Items?["Authorized-User-Id"]?.ToString();
            Console.WriteLine($"5 - {userId}");

            // Inside your controller method where you want to retrieve the userId
            var token = HttpContext?.Request.Headers["Authorization"].ToString().Split(" ")[1]; // Assuming the token is in the Authorization header
            var handler = new JwtSecurityTokenHandler();
            var tokenS = handler.ReadJwtToken(token);
            userId = tokenS.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value ?? "";

            Console.WriteLine($"6 - {userId}");

            token = Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            if (token != null)
            {
                userId = MiddlewareMethods.ValidateToken(token, _configuration);
                Console.WriteLine($"7 - {userId}");
            }
            Console.WriteLine();
            */

            // Handle the query and generate response
            string? response = await _requestService.GetGptResponse(query);

            if (response != null) {
                // Save the query to the database
                _queryService.SaveQuery(new Query
                {
                    UserId = userId,
                    QueryText = query,
                    ResponseText = response
                });
            }
            else
            {
                response = "ChatGPT Error.";
            }

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

        [HttpPost("new")]
        public IActionResult SaveQuery(SaveQueryRequest model)
        {
            string query = model.Query;
            string response = model.Response;

            _queryService.SaveQuery(new Query
            {
                UserId = "",
                QueryText = query,
                ResponseText = response
            });
            return Ok(new { Data = "Success" });
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
        public string UserId { get; set; } = ""; // todo: remove when not needed anymore
        public string Query { get; set; } = "";
    }

    public class SaveQueryRequest
    {
        public string Query { get; set; } = "";
        public string Response { get; set; } = "";
    }
}
