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


// Redis ���� ����� ���
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = "127.0.0.1";                    //���� Redis ������ ȣ��Ʈ �ּ�
    //options.InstanceName = "your_instance_name"; // �ɼ����� �����մϴ�.

});


// ���� ���� ���
builder.Services.AddSession(options =>
{
    options.Cookie.Name = ".AdventureWorks.Session";        // AspNetCore.Session�� ���� �����ӿ�ũ�� �ڵ����� �����ϴ� �̸��� ����մϴ�.
    options.IdleTimeout = TimeSpan.FromMinutes(50);
    options.Cookie.IsEssential = true;                      // �ɼ�
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

// DB �ʱ�ȭ (DB�� ������ �� ����ϴ� mysql connection string�� ����)
Database.Init(app.Configuration);

app.Run();


