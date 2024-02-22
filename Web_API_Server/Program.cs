var builder = WebApplication.CreateBuilder(args);                      // 빌더 생성 - 애플리케이션의 구성을 설정하고 서비스를 등록하는 데 사용됩니다. 

// Add services to the container.

builder.Services.AddControllers();                                      // 컨트롤러 추가                      

builder.Services.AddStackExchangeRedisCache(options =>                  // 분산캐시 서비스를 추가
{
    options.Configuration = "127.0.0.1";                                // redis 주소
    //options.InstanceName = "your_instance_name";
});

builder.Services.AddSession(options =>                                  // 세션 서비스 추가
{
    options.Cookie.Name = ".AdventureWorks.Session";
    options.IdleTimeout = TimeSpan.FromMinutes(20);
    options.Cookie.IsEssential = true;
});

var app = builder.Build();                                              // 빌더를 사용하여 애플리케이션을 빌드

// Configure the HTTP request pipeline.
app.UseHttpsRedirection();                                              

app.UseAuthorization();                                                 // 인증을 사용하여 애플리케이션에 접근하는 사용자를 제어합니다.        

app.UseSession();                                                       // 애플리케이션에 세션 미들웨어를 추가합니다. 이것은 세션 데이터를 처리하는 데 사용됩니다.

app.MapControllers();                                                   // 애플리케이션에 등록된 모든 컨트롤러에 대한 요청을 매핑합니다. 이것은 MVC 컨트롤러를 라우팅하는 데 사용됩니다.

// DB 초기화 (DB에 연결할 때 사용하는 mysql connection string을 설정)
Database.Init(app.Configuration);

app.Run();                                                              // 실행


