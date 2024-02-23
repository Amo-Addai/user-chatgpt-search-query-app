using System;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net.Http;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Sqlite;
using Microsoft.EntityFrameworkCore.SqlServer;
using Pomelo.EntityFrameworkCore.MySql;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using DotNetEnv;

using ChatGPTBackend.Data;
using ChatGPTBackend.Services;


namespace ChatGPTBackend
{
    public class Startup
    {
        public IConfiguration _configuration { get; }

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<HttpClient>();
            services.AddControllers();

            // Database connection
            // Configure database context based on configuration
            var databaseType = _configuration["DatabaseType" ?? ""];
            switch (databaseType)
            {
                case "Sqlite":
                    services.AddDbContext<AppDbContext>(options =>
                        options.UseSqlite(_configuration.GetConnectionString("Sqlite")));
                    break;
                case "SqlServer":
                    services.AddDbContext<AppDbContext>(options =>
                        options.UseSqlServer(_configuration.GetConnectionString("SqlServer")));
                    break;
                case "MySql":
                    services.AddDbContext<AppDbContext>(options =>
                        options.UseMySql(_configuration.GetConnectionString("MySql"), new MySqlServerVersion(new Version(8, 0, 28))));
                    break;
                default:
                    services.AddDbContext<AppDbContext>(options => 
                        options.UseSqlite(_configuration.GetConnectionString("Default")));
                    break;
            }

            services.AddCors(options =>
            {
                options.AddPolicy("AllowSpecificOrigins",
                    builder =>
                    {
                        builder 
                            .AllowAnyOrigin() // todo: .WithOrigins("http://yourflutterclient.com") // Replace with the origin of your Flutter client
                            .AllowAnyHeader()
                            .AllowAnyMethod();
                    });
            });

            // JWT Authentication
            var key = Encoding.ASCII.GetBytes(_configuration["JwtSettings:Secret"] ?? "");
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "JwtBearer";
                options.DefaultChallengeScheme = "JwtBearer";
            })
            .AddJwtBearer("JwtBearer", jwtBearerOptions =>
            {
                jwtBearerOptions.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(5)
                };

                jwtBearerOptions.Events = new JwtBearerEvents
                {
                    OnTokenValidated = context =>
                    {
                        var userService = context.HttpContext.RequestServices.GetRequiredService<IUserService>();
                        var userId = context.Principal?.FindFirstValue("id");
                        if (userId != null)
                        {
                            var user = userService?.GetUserById(userId);
                            // Return unauthorized if user no longer exists
                            if (user == null) context.Fail("Unauthorized");
                        } else context.Fail("Unauthorized");
                        return Task.CompletedTask;
                    }
                };
            });

            // Services
            services.AddScoped<DbSeeder>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IQueryService, QueryService>();
            services.AddScoped<IRequestService, RequestService>();
            
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, DbSeeder dbSeeder)
        {
            // Load variables from .env file
            DotNetEnv.Env.Load();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // Seed the database
            dbSeeder.Seed();

            app.UseHttpsRedirection();

            app.UseRouting();
            
            app.UseCors("AllowSpecificOrigins");

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

        }
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
