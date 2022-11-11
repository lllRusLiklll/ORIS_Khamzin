using Microsoft.Extensions.Caching.Memory;
using System.Net;

namespace WebServer
{
    public class SessionManager
    {
        private static SessionManager _instance;
        private MemoryCache _cache;

        private SessionManager()
        {
            _cache = new MemoryCache(new MemoryCacheOptions());
        }

        public void CreateSession(Session session)
        {
            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromSeconds(120));
            _cache.Set(session.Id, session, cacheEntryOptions);
        }

        public bool CheckSession(Guid id)
        {
            Session result;
            return _cache.TryGetValue(id, out result);
        }



        public Session GetInformation(Guid id) {
            Session result;
            if (!_cache.TryGetValue(id, out result))
            {
                return null;
            }
            return result;
        }

        public static SessionManager GetInstance()
        {
            if (_instance == null)
                _instance = new SessionManager();
            return _instance;
        }
    }

    public class Session
    {
        public Guid Id { get; set; }
        public int AccountId { get; set; }
        public string Email { get; set; }
        public DateTime CreatedDateTime { get; set; }
    }
}
