using System;
using System.Text;
using System.Text.Json;
// using System.Collections.Generic;
using System.Linq;
// using System.Linq.Expressions;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using ChatGPTBackend.Data;
using ChatGPTBackend.Models;


namespace ChatGPTBackend.Services
{

    public class DbSeeder
    {
        private readonly IServiceProvider _serviceProvider;

        public DbSeeder(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public void Seed()
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            if (dbContext?.Users != null)
            {
                // Check if the database is already seeded
                if (dbContext.Users.Any())
                {
                    Console.WriteLine("Database has already been seeded");
                    return; // or, clean up the database, then re-seed it
                }

                // Seed the database with initial data
                dbContext.Users.AddRange(
                    new User { Username = "user1", Password = "password1" },
                    new User { Username = "user2", Password = "password2" }
                );

                dbContext.SaveChanges();
            }
            else Console.WriteLine("Database has no User collection");
        }
    }

    public interface IDataService<TEntity>
    {
        IEnumerable<TEntity> GetAll();
        TEntity? GetById(object id);
        void Insert(TEntity entity);
        void Update(TEntity entity);
        void Delete(object id);
    }

    public class DataService<TEntity> : IDataService<TEntity> where TEntity : class
    {
        private readonly AppDbContext _context;

        public DataService(AppDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public IEnumerable<TEntity> GetAll()
        {
            return _context.Set<TEntity>();
        }

        public TEntity? GetById(object id)
        {
            return _context.Set<TEntity>().Find(id);
        }

        public void Insert(TEntity entity)
        {
            _context.Set<TEntity>().Add(entity);
            _context.SaveChanges();
        }

        public void Update(TEntity entity)
        {
            _context.Set<TEntity>().Update(entity);
            _context.SaveChanges();
        }

        public void Delete(object id)
        {
            TEntity? entityToDelete = _context.Set<TEntity>().Find(id);
            if (entityToDelete != null)
            {
                _context.Set<TEntity>().Remove(entityToDelete);
                _context.SaveChanges();
            }
        }
    }

    public interface IUserService
    {
        User? Authenticate(string username, string password);
        User? GetUserById(string id);
    }

    public class UserService : IUserService
    {
        private readonly AppDbContext? _context;
        private readonly DataService<User> _dataService;

        public UserService(AppDbContext context)
        {
            _context = context;
            _dataService = new DataService<User>(_context);
        }

        public User? Authenticate(string username, string password)
        {
            return _dataService.GetAll().FirstOrDefault(u => u.Username == username && u.Password == password);
        }

        public User? GetUserById(string id)
        {
            return _dataService.GetById(id);
        }
    }

    public interface IQueryService
    {
        IEnumerable<Query>? GetUserQueries(string userId);
        void SaveQuery(Query query);
    }

    public class QueryService : IQueryService
    {
        private readonly AppDbContext? _context;
        private readonly DataService<Query> _dataService;

        public QueryService(AppDbContext context)
        {
            _context = context;
            _dataService = new DataService<Query>(_context);
        }

        public IEnumerable<Query>? GetUserQueries(string userId)
        {
            // return _dataService.GetAll().Where(q => q.UserId == userId) / .Select(q => q);;
            return from q in _dataService.GetAll() ?? Array.Empty<Query>() where q.UserId == userId select q;
        }

        public void SaveQuery(Query query)
        {
            _dataService.Insert(query);
        }
    }

    public interface IRequestService
    {
        Task<string?> GetGptResponse(string query);
    }

    public class RequestService : IRequestService
    {
        private readonly HttpClient _httpClient;
        
        public RequestService(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public async Task<string?> GetGptResponse(string query)
        {
            const string apiUrl = "https://api.openai.com/v1";
            const string apiKey = "GET FROM ENVIRONMENT";

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            
            // Define request payload
            var requestPayload = new
            {
                model = "text-davinci-003",
                prompt = query, // Text = query
                max_tokens = 7,
                temperature = 0
            };

            // Serialize request payload
            var jsonRequest = JsonSerializer.Serialize(requestPayload);

            // Define request content
            var content = new StringContent(jsonRequest, Encoding.UTF8, new MediaTypeHeaderValue("application/json")?.MediaType ?? "application/octet-stream");

            // Send request to ChatGPT API
            var response = await _httpClient.PostAsync($"{apiUrl}/completions", content);

            // response.EnsureSuccessStatusCode();
            // Or, Check if request was successful
            if (response.IsSuccessStatusCode)
            {
                // Read response content
                var jsonResponse = await response.Content.ReadAsStringAsync();

                // Deserialize response
                var responseData = JsonSerializer.Deserialize<ChatGptResponse>(jsonResponse);
                if (responseData != null)
                {
                    Console.WriteLine(responseData);

                    // Return response text
                    return responseData?.choices?[0]?.text?.Trim();
                }
            }
            // Request failed, return null
            return null;
        }
    }

    public class ChatGptResponse
    {
        public ChatGptChoice[]? choices { get; set; }
    }

    public class ChatGptChoice
    {
        public string? text { get; set; }
    }

}
