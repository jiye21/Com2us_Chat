// 요청 데이터
public class LoginRequest
{
    public string userID { get; set; }
    public string userPW { get; set; }
}

public class UserInfo
{
    public int userNum { get; set; }
    public string userID { get; set; }
    public string userPW { get; set; } // 실제로는 비밀번호 해시값이 여기에 저장됨
                                              
}

public class RegisterRequest
{
    public string userID { get; set; }
    public string userPW { get; set; } 
                                         
}
