var builder = WebApplication.CreateBuilder(args);                      // ���� ���� - ���ø����̼��� ������ �����ϰ� ���񽺸� ����ϴ� �� ���˴ϴ�. 

// Add services to the container.

builder.Services.AddControllers();                                      // ��Ʈ�ѷ� �߰�                      

builder.Services.AddStackExchangeRedisCache(options =>                  // �л�ĳ�� ���񽺸� �߰�
{
    options.Configuration = "127.0.0.1";                                // redis �ּ�
    //options.InstanceName = "your_instance_name";
});

builder.Services.AddSession(options =>                                  // ���� ���� �߰�
{
    options.Cookie.Name = ".AdventureWorks.Session";
    options.IdleTimeout = TimeSpan.FromMinutes(20);
    options.Cookie.IsEssential = true;
});

var app = builder.Build();                                              // ������ ����Ͽ� ���ø����̼��� ����

// Configure the HTTP request pipeline.
app.UseHttpsRedirection();                                              

app.UseAuthorization();                                                 // ������ ����Ͽ� ���ø����̼ǿ� �����ϴ� ����ڸ� �����մϴ�.        

app.UseSession();                                                       // ���ø����̼ǿ� ���� �̵��� �߰��մϴ�. �̰��� ���� �����͸� ó���ϴ� �� ���˴ϴ�.

app.MapControllers();                                                   // ���ø����̼ǿ� ��ϵ� ��� ��Ʈ�ѷ��� ���� ��û�� �����մϴ�. �̰��� MVC ��Ʈ�ѷ��� ������ϴ� �� ���˴ϴ�.

// DB �ʱ�ȭ (DB�� ������ �� ����ϴ� mysql connection string�� ����)
Database.Init(app.Configuration);

app.Run();                                                              // ����


