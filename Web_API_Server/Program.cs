using Microsoft.AspNetCore.Session;
using Microsoft.Extensions.Caching.StackExchangeRedis;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.


builder.Services.AddControllers();


builder.Services.AddDistributedMemoryCache();

/*builder.Services.AddSession(options =>
{
    options.Cookie.Name = ".AdventureWorks.Session";
    options.IdleTimeout = TimeSpan.FromSeconds(50);
    options.Cookie.IsEssential = true;
});*/


// Redis 세션 스토어 등록
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = "127.0.0.1";                    //실제 Redis 서버의 호스트 주소
    //options.InstanceName = "your_instance_name"; // 옵션으로 설정합니다.

});


// 세션 서비스 등록
builder.Services.AddSession(options =>
{
    options.Cookie.Name = ".AdventureWorks.Session";        // AspNetCore.Session과 같이 프레임워크가 자동으로 설정하는 이름을 사용합니다.
    options.IdleTimeout = TimeSpan.FromMinutes(50);
    options.Cookie.IsEssential = true;                      // 옵션
});


var app = builder.Build();



// Configure the HTTP request pipeline.
app.UseHttpsRedirection();

app.UseAuthorization();

app.UseSession();

/*app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});*/

app.MapControllers();

// DB 초기화 (DB에 연결할 때 사용하는 mysql connection string을 설정)
Database.Init(app.Configuration);

app.Run();


