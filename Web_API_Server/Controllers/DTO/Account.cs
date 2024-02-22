// 요청 데이터
public class LoginRequest
{
    public string LoginId { get; set; }
    public string Password { get; set; }
}

public class UserInfo
{
    public int Id { get; set; }
    public string LoginId { get; set; }
    public string PasswordHash { get; set; } // 실제로는 비밀번호 해시값이 여기에 저장됨
                                              
}

public class RegisterRequest
{
    public string LoginId { get; set; }
    public string Password { get; set; } 
                                         
}
public class UpdateRequest
{
    public int Id { get; set; }
    public string LoginId { get; set; }
    public string Passwd { get; set; } 
}
