public enum EErrorCode
{
    None = 0,

    // 인증 관련
    CreateAccountFail = 1001,
    LoginFail = 1002,
    AuthFailInvalidResponse = 1003,
    LoginFailUserNotExist = 1004,
    LoginFailAddRedis = 1005,
    LogoutFail = 1006,
}
