
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using WebAPI_CICD.Data;
using WebAPI_CICD.Services;

namespace WebAPI_CICD
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);


            // PostgreSQL
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

            // Redis
            builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(
                builder.Configuration.GetValue<string>("Redis:Configuration")));

            // 高并发服务
            builder.Services.AddScoped<UserService>();

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();
            //builder.Services.AddHttpsRedirection(options =>
            //{
            //    options.RedirectStatusCode = StatusCodes.Status308PermanentRedirect;
            //    options.HttpsPort = 443; // 确保这里的端口号与你的应用程序实际使用的 HTTPS 端口一致
            //});
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();



            var app = builder.Build();

            // 本地开发用
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "WebAPI_CICD v1");
                    // 如果想让 UI 显示在根路径，取消下一行的注释：
                    // c.RoutePrefix = string.Empty;
                });
                app.UseHttpsRedirection(); 
            }
            // 自动迁移
            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                db.Database.Migrate();
            }


            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
