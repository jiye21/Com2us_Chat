using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using StackExchange.Redis;

// 사용자의 세션 상태 관리 클래스
public class User : TcpClient
{
    public bool isSession; // 세션 상태
    public TcpClient tcpClient;

    public User()
    {
        this.isSession = false;
    }

    // 사용자의 세션 키 검증 메서드
    public void CheckSessionInRedis(string redisSessionKey, string clientSessionKey)
    {
        if (redisSessionKey == clientSessionKey)
        {
            isSession = true;
        }
        else
        {
            isSession = false;
        }
    }
}

public class ChatRoom
{
    public string Name { get; }
    public List<TcpClient> clients = new List<TcpClient>();

    public ChatRoom(string name)
    {
        Name = name;
    }

    public void BroadcastMessage(string message, TcpClient sender)
    {
        byte[] buffer = Encoding.UTF8.GetBytes(message);
        foreach (TcpClient client in clients)
        {
            NetworkStream stream = client.GetStream();
            stream.Write(buffer, 0, buffer.Length);
            stream.Flush();

            // 자기 자신에게는 되보내지 않음
            if (client != sender)
            {
            }
        }
    }

    public void AddClient(TcpClient client)
    {
        // 이미 client가 list에 담겨 있는 상태라면 바로 함수를 종료한다. 
        foreach(TcpClient c in clients)
        {
            if (c == client) return;
        }

        clients.Add(client);
    }

    public void RemoveClient(TcpClient client)
    {
        clients.Remove(client);
    }
}

public class ChatServer
{
    private TcpListener listener;
    //public static List<ChatRoom> rooms = new List<ChatRoom>();
    public static Dictionary<string, ChatRoom> rooms = new Dictionary<string, ChatRoom>();

    private ConnectionMultiplexer redis; // redis : 레디스 서버와의 연결을 관리
    private IDatabase db; // db : ConnectionMultiplexer를 통해 얻은 데이터베이스

    public ChatServer(int port)
    {
        listener = new TcpListener(IPAddress.Any, port);

        // Redis 서버에 연결하는 정보 설정
        ConfigurationOptions options = new ConfigurationOptions
        {
            EndPoints = { "127.0.0.1:6379" }, // Redis 서버의 주소 및 포트
            //Password = "1", // Redis에 암호가 설정되어 있는 경우 암호 설정
            // 다른 설정도 추가할 수 있음
        };

        // ConnectionMultiplexer를 통해 Redis에 연결
        ConnectionMultiplexer connection = ConnectionMultiplexer.Connect(options);

        // 연결된 Redis 인스턴스로부터 IDatabase 인터페이스 얻기
        db = connection.GetDatabase();

    }

    public void Start()
    {
        listener.Start();
        Console.WriteLine("Chat server started...");


        // 테스트용 dummy rooms
        ChatRoom room01 = new ChatRoom("room01");
        ChatRoom room02 = new ChatRoom("22222");
        ChatRoom room03 = new ChatRoom("hello world");

        TcpClient dummy01 = new TcpClient();
        TcpClient dummy02 = new TcpClient();
        TcpClient dummy03 = new TcpClient();
        TcpClient dummy04 = new TcpClient();
        TcpClient dummy05 = new TcpClient();
        TcpClient dummy06 = new TcpClient();

        room01.AddClient(dummy01);
        room01.AddClient(dummy02);
        room02.AddClient(dummy03);
        room03.AddClient(dummy04);
        room03.AddClient(dummy05);
        room03.AddClient(dummy06);

        rooms.Add("room01", room01);
        rooms.Add("room02", room02);
        rooms.Add("room03", room03);



        while (true)
        {
            TcpClient client = listener.AcceptTcpClient();
            Console.WriteLine("Client connected: " + client.Client.RemoteEndPoint);

            Thread clientThread = new Thread(HandleClient);
            clientThread.Start(client);
        }
    }

    private void HandleClient(object obj)
    {
        //TcpClient client = (TcpClient)obj;
        User client = new User();
        client.tcpClient = (TcpClient)obj;
        NetworkStream stream = client.tcpClient.GetStream();

        byte[] buffer = new byte[2048];
        int bytesRead;

        ChatRoom room = null;

        // 데이터를 최초 한번 읽음
        bytesRead = stream.Read(buffer, 0, buffer.Length);

        // 클라가 보낸 세션키 저장, 클라가 연결되자마자 보내는 데이터가 세션키라고 가정함. 

        //string clientSessionKey = Encoding.UTF8.GetString(buffer, 0, bytesRead);
        string clientSessionKey = "568fa0dc-1e39-45eb-bcf7-73d3d3fd98da";

        Console.WriteLine("세션키 잘 받았다~" + clientSessionKey);

        // 세션 키를 레디스에서 가져오기
        string redisSessionKey = GetSessionKeyFromRedis();

        // 클라이언트의 세션 확인
        client.CheckSessionInRedis(redisSessionKey, clientSessionKey);

        if (client.isSession)
        {
            var responseData = Encoding.UTF8.GetBytes("200 : " + "ok");
            stream.Write(responseData, 0, responseData.Length);
            stream.Flush();

            Console.WriteLine("Received session key from client: " + clientSessionKey);
        }
        else
        {
            Console.WriteLine("세션키가 달라~");
            client.Close();
            return;
        }

        try
        {
            while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
            {
                string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                Console.WriteLine("Received from client: " + message);

                // Check if message is a command to join a room
                // ex) "/join 10"
                if (message.StartsWith("/join"))
                {
                    string[] parts = message.Split(' ');
                    if (parts.Length == 2)
                    {
                        string roomName = parts[1];
                        // roomName으로 방 생성 후 방 list에 저장
                        room = GetOrCreateRoom(roomName);

                        Console.WriteLine("Client joined room: " + roomName);

                        // 응답코드와 방번호 전송, 200 = ok.
                        var responseData = Encoding.UTF8.GetBytes("200 : " + roomName);
                        stream.Write(responseData, 0, responseData.Length);
                        stream.Flush();
                    }
                }
                else if (message.StartsWith("/create"))
                {
                    string[] parts = message.Split(' ');
                    if (parts.Length == 2)
                    {
                        string roomName = parts[1];

                        var responseData = Encoding.UTF8.GetBytes("200 : " + roomName);
                        stream.Write(responseData, 0, responseData.Length);
                        stream.Flush();
                    }
                    else
                    {
                        //SendMessage("Invalid /create command. Usage: /create [room_name]", clientSocket);
                    }
                }
                else if (message.StartsWith("/list"))
                {
                    List<string> userCount = new List<string>();
                    foreach (ChatRoom r in rooms.Values)
                    {
                        userCount.Add(r.clients.Count.ToString());
                    }
                    string aa = "Available_rooms : " + 
                        string.Join(", ", rooms.Keys) + 
                        "and" + string.Join(", ", userCount);

                    Console.WriteLine(aa);

                    SendMessage(aa, client);
                    //var responseData = Encoding.UTF8.GetBytes(data);
                    //stream.Write(responseData, 0, responseData.Length);
                    //stream.Flush();
                }
                else
                {
                    // 로비에서 접속을 끊고 채팅방으로 이동(새로운 접속) 후의 상황. 
                    // 클라이언트에서 몇번 방인지 같이 보내준다. ex. "0)id:msg"

                    // parts[0] = 방번호, parts[1] = 메세지 내용
                    string[] parts = message.Split(')');
                    if (parts.Length == 2)
                    {
                        string roomName = parts[0];
                        string responseData = parts[1];

                        // 최초 접속시에만 실행
                        if (parts[1] == "new client connected")
                        {
                            // roomName으로 방정보 Get
                            room = GetOrCreateRoom(roomName);
                            // 방에 client 추가, 만약 이미 client가 방에 추가되어 있으면 추가하지 않고 함수 종료. 
                            room.AddClient(client);

                            responseData = " " + ":" + responseData;
                        }


                        // Broadcast message to client's room
                        foreach (ChatRoom r in rooms.Values)
                        {
                            if (r.Name == roomName)
                            {
                                r.BroadcastMessage(responseData, client);
                                break;
                            }
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Client disconnected: " + ex.Message);
        }
        finally
        {
            // client와 연결이 종료될 때 방 목록에서도 삭제해준다. 
            if(room != null) room.RemoveClient(client);
            client.Close();
        }
    }

    // 레디스에서 세션키 가져오기
    private string GetSessionKeyFromRedis()
    {
        string sessionKey = db.StringGet("2d163123-93ad-4ceb-a98e-d022500e3645");
        return sessionKey;
    }

    private void SendMessage(string message, TcpClient clientSocket)
    {
        byte[] buffer = Encoding.UTF8.GetBytes(message);
        NetworkStream stream = clientSocket.GetStream();
        stream.Write(buffer, 0, buffer.Length);
        stream.Flush();
    }

    private ChatRoom GetOrCreateRoom(string roomName)
    {
        foreach (ChatRoom room in rooms.Values)
        {
            if (room.Name == roomName)
            {
                return room;
            }
        }

        ChatRoom newRoom = new ChatRoom(roomName);
        // roomlist에 room 추가
        rooms.Add(roomName, newRoom);
        return newRoom;
    }

    public static void Main(string[] args)
    {
        int port = 9999; // Change this to desired port number
        ChatServer server = new ChatServer(port);
        server.Start();
    }
}