using Dapper;                                       // 데이터베이스 작업을 딘순화하는 마이크로 ORM이다.  SQL 쿼리를 작성하고 결과를 C# 객체로 매핑하는 데 사
using Microsoft.AspNetCore.Mvc;                     
using MySqlConnector;                               // MySQL 데이터 베이스에 연결하고 쿼리를 실행하는데 사용된다.
using Microsoft.Extensions.Caching.Distributed;     // 분산 캐싱 기능을 제공 
using BCrypt.Net;                                   // 해쉬 알고리즘을 위함


namespace Controllers
{
    [ApiController]                             // API 컨트롤러임을 명시
    [Route("[controller]")]                     //라우팅 경로에 Controller 부분을 제외한 부분을 대체
    public class AccountController : ControllerBase             
    {

        private readonly IDistributedCache _redisCache;               // 분산캐싱 기능을 제공

        public AccountController(IDistributedCache redisCache)        //  redisCache라는 이름으로 redis를 사용할것이라는걸 명시
        {
            _redisCache = redisCache;
        }


        [HttpPost("logout")]                    // 로그아웃 라우팅 경로
        public async Task<IActionResult> Logout()
        {
            // 클라이언트에서 전송한 세션 키를 얻어옴
            string sessionKey = HttpContext.Request.Cookies["sessionId"];   //HttpContext 객체의 Request 속성에 있는 Cookies 컬렉션을 사용하여 sessionId 쿠키의 값을 가져옵니다.

            if (sessionKey != null)
            {
                // Redis에서 해당 세션 키를 제거
                await _redisCache.RemoveAsync(sessionKey);

                // 클라이언트에게 해당 세션 키를 지우도록 쿠키를 전송
                HttpContext.Response.Cookies.Delete("sessionId");

                return Ok("Logout successful");
            }
            else
            {
                return BadRequest("No session found to logout");
                //return EErrorCode.LogoutFail;
            }
        }

        [HttpGet("userinfo")]
        public async Task<IActionResult> GetUserInfo()

        {
            // 세션에서 사용자 ID를 가져옴
            var userId = HttpContext.Session.GetString("UserId");
            if (userId == null)
            {
                return Unauthorized("User not logged in");
            }

            // 사용자 정보를 데이터베이스에서 가져와 반환
            using (MySqlConnection connection = await Database.GetMySqlConnetion())
            {
                UserInfo userInfo = await connection.QuerySingleOrDefaultAsync<UserInfo>(
                    "SELECT * FROM Users WHERE Id = @Id", new { Id = userId });

                return Ok(userInfo);
            }
        }


        [HttpPost("register")]                  //// 회원가입 라우팅 경로
        public async Task<IActionResult> Register(RegisterRequest registerRequest)
        {
            using (MySqlConnection connection = await Database.GetMySqlConnetion())     // DB연결
            {
                // 이미 등록된 사용자인지 확인
                UserInfo existingUser = await connection.QuerySingleOrDefaultAsync<UserInfo>(
                    "SELECT * FROM Users WHERE LoginId = @loginId", new { loginId = registerRequest.LoginId });         //쿼리문확인해서 existingUser에 저장

                //회원가입 유무  existingUser이 null이어야 가능
                if (existingUser != null)
                {
                    return BadRequest($"User with the same login ID already exists {StatusCode((int)EErrorCode.LogoutFail)}");
                    //return StatusCode((int)EErrorCode.LogoutFail);
                } 

                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(registerRequest.Password);               // 입력한 비밀번호를 해쉬화해서 저장

                await connection.ExecuteAsync(
                    "INSERT INTO Users (LoginId, PasswordHash) VALUES (@loginId, @passwordHash)",
                    new { loginId = registerRequest.LoginId, passwordHash = hashedPassword });

                // Redis에 새 사용자 정보 저장
                await _redisCache.SetStringAsync(registerRequest.LoginId, "registered");

                return Ok("User registered successfully");
            }
        }

        [HttpPost("login")]             // 라우팅 경로에 추가 되는 부분
        public async Task<IActionResult> Login(LoginRequest loginRequest)                   // 데이터베이스 쿼리는 I/O 작업이기 때문에 비동기
        {
            using (MySqlConnection connection = await Database.GetMySqlConnetion())         // 데이터 베이스 연결

            {
                // 데이터베이스에서 해당 로그인 아이디의 사용자 정보를 가져옴
                UserInfo userInfo = await connection.QuerySingleOrDefaultAsync<UserInfo>(
                    "SELECT * FROM Users WHERE LoginId = @loginId", new { loginId = loginRequest.LoginId });        //쿼리문을 실행해서 결과를 userInfo에 담기

                // 사용자가 없거나 비밀번호가 일치하지 않으면 로그인 실패
                if (userInfo == null )
                {
                    return Unauthorized("Invalid login credentials");
                }

                bool isPasswordCorrect = BCrypt.Net.BCrypt.Verify(loginRequest.Password, userInfo.PasswordHash);
                Console.WriteLine(isPasswordCorrect);
                if (!isPasswordCorrect)
                {
                    return Unauthorized("Invalid login credentials");
                }

                string sessionId = Guid.NewGuid().ToString();
                //새로운 GUID(전역 고유 식별자)를 생성하고 이를 문자열로 변환하는 것입니다.
                //GUID는 매우 난수적이며 중복될 가능성이 매우 낮은 값으로, 일반적으로 고유한 식별자를 생성하는 데 사용됩니다.

                // Redis에 세션 ID와 사용자 ID 저장
                await _redisCache.SetStringAsync(sessionId, userInfo.Id.ToString());        // redis에 (key, value)를 저장
                var value = await _redisCache.GetStringAsync(sessionId);                    // value값을 보기위함
                // 클라이언트에게 세션 ID를 쿠키로 전달
                HttpContext.Response.Cookies.Append("sessionId", sessionId);                // 클라이언트 브라우저에 세션 ID를 쿠키로 전달...

                Console.WriteLine($"Session Key: {loginRequest.LoginId} {sessionId}, Value: {value}");

                Console.WriteLine($"HttpContext.sessionId : {sessionId}"); 

                return Ok("Login successful");


            }
        }

        [HttpDelete("delete/{Id}")]
        public async Task<IActionResult> Delete(int Id)
        {
            using (MySqlConnection connection = await Database.GetMySqlConnetion())
            {
                // 해당 ID를 가진 사용자가 존재하는지 확인
                UserInfo existingUser = await connection.QuerySingleOrDefaultAsync<UserInfo>(
                    "SELECT * FROM Users WHERE Id = @id", new { Id });

                if (existingUser == null)
                {
                    return NotFound("User not found");
                }

                // 사용자 삭제
                await connection.ExecuteAsync("DELETE FROM Users WHERE Id = @id", new { Id });

                // Redis에서 사용자 정보 제거
                await _redisCache.RemoveAsync(existingUser.LoginId);

                return Ok("User deleted successfully");
            }
        }


    }

}
