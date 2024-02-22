using MySqlConnector;

public class Database
{
    static string dbConnectionString;

    public static void Init(IConfiguration Configuration)
    {
        dbConnectionString = Configuration.GetSection("DBConnection")["MySql"]; 
    }

    public static async Task<MySqlConnection> GetMySqlConnetion()
    {
        MySqlConnection connection = new MySqlConnection(dbConnectionString);
        await connection.OpenAsync();

        return connection;
    }
}

// 데이터베이스를 연결하기 위한 코드들..
// redis는 program.cs에서 바로 연결 했는데 MySPL은 따로 빼둔이유??
// 사실 redis도 따로 빼두는게 좋다....
// 유지 보수, program.cs는 진입점으로 역할이 있기 때문에 그걸 준수 해주기위함