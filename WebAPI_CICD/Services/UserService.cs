using System;
using System.Text.Json;
using WebAPI_CICD.Data;
using StackExchange.Redis;
using Microsoft.EntityFrameworkCore;

namespace WebAPI_CICD.Services
{
    public class UserService
    {
        private readonly AppDbContext _db;
        private readonly IDatabase _redis;

        public UserService(AppDbContext db, IConnectionMultiplexer redis)
        {
            _db = db;
            _redis = redis.GetDatabase();
        }

        private string GetCacheKey(int id) => $"user:{id}";

        public async Task<User?> GetUserAsync(int id)
        {
            string cacheKey = GetCacheKey(id);

            // 1️⃣ 先查缓存
            var cached = await _redis.StringGetAsync(cacheKey);
            if (cached.HasValue)
                return JsonSerializer.Deserialize<User>(cached);

            // 2️⃣ 缓存未命中，查数据库
            var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);
            if (user != null)
            {
                // 3️⃣ 写入 Redis 缓存，防止击穿
                await _redis.StringSetAsync(cacheKey, JsonSerializer.Serialize(user), TimeSpan.FromMinutes(5));
            }

            return user;
        }

        public async Task<User> CreateUserAsync(string username, string email)
        {
            var user = new User { Username = username, Email = email };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            // 写入缓存
            string cacheKey = GetCacheKey(user.Id);
            await _redis.StringSetAsync(cacheKey, JsonSerializer.Serialize(user), TimeSpan.FromMinutes(5));

            return user;
        }
    }
}
