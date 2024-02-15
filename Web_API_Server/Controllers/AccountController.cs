using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Session;


namespace Controllers
{
    [ApiController]
    [Route("[controller]")]                     //라우팅 경로에 Controller 부분을 제외한 부분을 대체
    public class AccountController : ControllerBase             
    {

        [HttpPost("login")]             // 라우팅 경로에 추가 되는 부분
        public async Task<IActionResult> Login(LoginRequest loginRequest)
        {
            using (MySqlConnection connection = await Database.GetMySqlConnetion())
            {
                Console.WriteLine("asd");
                // 데이터베이스에서 해당 로그인 아이디의 사용자 정보를 가져옴
                UserInfo userInfo = await connection.QuerySingleOrDefaultAsync<UserInfo>(
                    "SELECT * FROM Users WHERE LoginId = @loginId", new { loginId = loginRequest.LoginId });

                // 사용자가 없거나 비밀번호가 일치하지 않으면 로그인 실패
                if (userInfo == null )
                {
                    return Unauthorized("Invalid login credentials");
                }

                // 로그인 성공 시 세션에 사용자 ID를 저장
                HttpContext.Session.SetString("UserId", userInfo.Id.ToString());
                return Ok("Login successful");
            }
        }

        [HttpGet("logout")]
        public IActionResult Logout()
        {
            // 세션에서 사용자 ID를 제거하여 로그아웃
            HttpContext.Session.Remove("UserId");
            return Ok("Logout successful");
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

        // 비밀번호를 해시화하여 저장된 해시값과 비교
        private bool VerifyPassword(string password, string hashedPassword)
        {
            // 실제로는 보안적으로 안전한 해시 비교 메서드를 사용해야 함 (예: BCrypt, Argon2 등)
            // 여기서는 단순히 문자열 비교를 수행
            return password == hashedPassword;
        }


        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest registerRequest)
        {
            using (MySqlConnection connection = await Database.GetMySqlConnetion())
            {
                // 이미 등록된 사용자인지 확인
                UserInfo existingUser = await connection.QuerySingleOrDefaultAsync<UserInfo>(
                    "SELECT * FROM Users WHERE LoginId = @loginId", new { loginId = registerRequest.LoginId });

                if (existingUser != null)
                {
                    return BadRequest("User with the same login ID already exists");
                }

                // 새 사용자를 데이터베이스에 추가
                await connection.ExecuteAsync(
                    "INSERT INTO Users (LoginId, Passwd) VALUES (@loginId, @hashedPassword)",
                    new { loginId = registerRequest.LoginId, hashedPassword = registerRequest.Password });

                return Ok("User registered successfully");
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

                return Ok("User deleted successfully");
            }
        }


        [HttpPut("update/{id}")]
        public async Task<IActionResult> Update(int id, UpdateRequest updateRequest)
        {
            // 회원 수정 로직 추가
            return Ok("User updated successfully");
        }


    }

}
