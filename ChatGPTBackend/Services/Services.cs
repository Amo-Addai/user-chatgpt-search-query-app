using System.Text;
using System.Text.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Linq;

using ChatGPTBackend.Data;
using ChatGPTBackend.Models;


namespace ChatGPTBackend.Services
{
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
            const string ChatGPTAPIEndpoint = "API/ChatGPT";
            
            // Define request payload
            var requestPayload = new
            {
                Text = query
            };

            // Serialize request payload
            var jsonRequest = JsonSerializer.Serialize(requestPayload);

            // Define request content
            var content = new StringContent(jsonRequest, Encoding.UTF8, new MediaTypeHeaderValue("application/json")?.MediaType ?? "application/octet-stream");

            // Send request to ChatGPT API
            var response = await _httpClient.PostAsync(ChatGPTAPIEndpoint, content);

            // Check if request was successful
            if (response.IsSuccessStatusCode)
            {
                // Read response content
                var jsonResponse = await response.Content.ReadAsStringAsync();

                // Deserialize response
                var responseObject = JsonSerializer.Deserialize<ChatGptResponse>(jsonResponse);

                // Return response text
                return responseObject?.ResponseText;
            }
            else
            {
                // Request failed, return null
                return null;
            }
        }
    }

    public class ChatGptResponse
    {
        public string? ResponseText { get; set; }
    }
}
