using System;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Mvc.Filters;


namespace ChatGPTBackend.Middlewares
{

    // Custom middleware to extract user ID from JWT token
    public class JwtMiddleware
    {
        public IConfiguration _configuration { get; }
        private readonly RequestDelegate _next;

        public JwtMiddleware(IConfiguration configuration, RequestDelegate next)
        {
            _configuration = configuration;
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            // Console.WriteLine($"\nJwt Middleware Token - {token}\n");
            if (token != null)
            {
                AttachUserIdToContext(context, ValidateToken(token));
            }

            await _next(context);
        }

        private string ValidateToken(string token)
        {
            return MiddlewareMethods.ValidateToken(token, _configuration);
        }

        private void AttachUserIdToContext(HttpContext context, string userId)
        {
            // Console.WriteLine($"\nJwt Middleware User-Id - {userId}\n");
            context.Items["Authorized-User-Id"] = userId;
            // Console.WriteLine($"\nJwt Middleware User-Id - {context.Items["Authorized-User-Id"]}\n");
        }
    }

    // Custom filter to extract user ID from JWT token
    public class JwtAuthFilter : IAsyncActionFilter
    {
        public IConfiguration _configuration { get; }

        public JwtAuthFilter(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var token = context.HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            // Console.WriteLine($"\nJwt Filter Token - {token}\n");
            if (token != null)
            {
                var userId = ValidateToken(token);
                // Console.WriteLine($"\nJwt Filter User-Id - {userId}\n");
                context.HttpContext.Items["Authorized-User-Id"] = userId;
                // Console.WriteLine($"\nJwt Filter User-Id - {context.HttpContext.Items["Authorized-User-Id"]}\n");
            }

            await next();
        }

        private string ValidateToken(string token)
        {
            return MiddlewareMethods.ValidateToken(token, _configuration);
        }
    }

    public class MiddlewareMethods {

        public static string ValidateToken(string token, IConfiguration config)
        {
            // Validate and decode the JWT token here
            var userId = DecodeTokenAndGetUserId(token, config);
            return userId;
        }

        private static string DecodeTokenAndGetUserId(string token, IConfiguration config)
        {
            string userId = "";
            // Decode and validate the JWT token
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(config["JwtSettings:Secret"] ?? "");
            var parameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false
            };

            try
            {
                // Extract the user ID from the JWT token
                var claimsPrincipal = tokenHandler.ValidateToken(token, parameters, out var validatedToken);
                var jwtToken = (JwtSecurityToken)validatedToken;
                userId = jwtToken.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value ?? "";
                return userId;
            }
            catch (Exception e)
            {
                // Handle token validation errors
                Console.WriteLine($"\nToken validation failed: {e.Message}\n");
                Console.WriteLine($"\nStackTrace: {e.StackTrace}\n");
            }
            return userId;
        }

    }
}

/* // TODO: ERROR

Token validation failed: IDX10517: Signature validation failed. The token's kid is missing. Keys tried: 'Microsoft.IdentityModel.Tokens.SymmetricSecurityKey, KeyId: '', InternalId: 'VBXX5IdhkdiwHvWvz8d4jQlE5oh8LPquxXHtCQiG6Hw'. , KeyId: 
'. Number of keys in TokenValidationParameters: '1'. 
Number of keys in Configuration: '0'. 
Exceptions caught:
 '[PII of type 'System.Text.StringBuilder' is hidden. For more details, see https://aka.ms/IdentityModel/PII.]'.
token: '[PII of type 'System.IdentityModel.Tokens.Jwt.JwtSecurityToken' is hidden. For more details, see https://aka.ms/IdentityModel/PII.]'. See https://aka.ms/IDX10503 for details.

*/