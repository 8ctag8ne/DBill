using Microsoft.Extensions.Caching.Memory;

public interface ISessionService
{
    string GetSessionId();
    T GetOrCreateService<T>(Func<T> factory) where T : class;
    T GetService<T>(string sessionId) where T : class;
}

public class SessionService : ISessionService
{
    private readonly IMemoryCache _cache;
    private readonly IHttpContextAccessor _httpContext;
    private const string SESSION_ID_HEADER = "X-Session-Id";
    private const int SESSION_TIMEOUT_MINUTES = 120;

    public SessionService(IMemoryCache cache, IHttpContextAccessor httpContext)
    {
        _cache = cache;
        _httpContext = httpContext;
    }

    public string GetSessionId()
    {
        var context = _httpContext.HttpContext;
        
        // Спробуйте отримати з заголовку
        if (context.Request.Headers.TryGetValue(SESSION_ID_HEADER, out var sessionId))
            return sessionId.ToString();
        
        // Спробуйте з cookies
        if (context.Request.Cookies.TryGetValue(SESSION_ID_HEADER, out var cookieSession))
            return cookieSession;
        
        // Створіть новий sessionId
        var newSessionId = Guid.NewGuid().ToString();
        context.Response.Cookies.Append(SESSION_ID_HEADER, newSessionId);
        return newSessionId;
    }

    public T GetOrCreateService<T>(Func<T> factory) where T : class
    {
        var sessionId = GetSessionId();
        var cacheKey = $"service_{typeof(T).Name}_{sessionId}";
        
        if (!_cache.TryGetValue(cacheKey, out T service))
        {
            service = factory();
            var cacheOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(SESSION_TIMEOUT_MINUTES));
            _cache.Set(cacheKey, service, cacheOptions);
        }
        
        return service;
    }

    public T GetService<T>(string sessionId) where T : class
    {
        var cacheKey = $"service_{typeof(T).Name}_{sessionId}";
        _cache.TryGetValue(cacheKey, out T service);
        return service;
    }
}