using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;




public class User
{
    public bool sessionExists;
    public ChatRoom currentRoom;
    public string userName;

    public User(string userName)
    {
        sessionExists = false;
        this.userName = userName;
    }
    public void SetCurrentRoom(ChatRoom room)
    {
        currentRoom = room;
    }
}

public class ChatRoom
{
    public string Name { get; }
    public List<Socket> Clients = new List<Socket>();
    public int UserCount => Clients.Count; // 새로운 멤버 변수로 유저 수 제공
    public ChatRoom(string name)
    {
        Name = name;
    }
    public void BroadcastMessage(string message, Socket sender)
    {
        byte[] buffer = Encoding.UTF8.GetBytes(message);
        foreach (Socket client in Clients)
        {
            NetworkStream stream = new NetworkStream(client);
            stream.Write(buffer, 0, buffer.Length);
            stream.Flush();
        }
    }
    public void AddClient(Socket client)
    {
        // 이미 client가 list에 담겨 있는 상태라면 바로 함수를 종료한다.
        foreach (Socket c in Clients)
        {
            if (c == client) return;
        }
        Clients.Add(client);
        Console.WriteLine("AddClient호출 " + client);
        Console.WriteLine("User count in room '" + Name + "': " + UserCount); // 유저 수 출력
    }
    public void RemoveClient(Socket client)
    {
        Clients.Remove(client);
        Console.WriteLine("RemoveClien호출 " + client);
        Console.WriteLine("RemoveClien호출 " + Clients.Count);
    }
}
public class ChatServer
{
    public static Dictionary<string, ChatRoom> rooms = new Dictionary<string, ChatRoom>();
    private ConnectionMultiplexer redis;
    private IDatabase db;



    public void Start()
    {
        TcpListener serverSocket = new TcpListener(IPAddress.Any, 9999);
        serverSocket.Start();
        Console.WriteLine("Chat server started...");

        // Connect to Redis
        redis = ConnectionMultiplexer.Connect("127.0.0.1:6379");
        db = redis.GetDatabase();



        while (true)
        {
            Socket clientSocket = serverSocket.AcceptSocket();
            Console.WriteLine("Client connected: " + clientSocket.RemoteEndPoint);
            Thread clientThread = new Thread(() => HandleClient(clientSocket));
            clientThread.Start();
        }
    }
    private void HandleClient(Socket clientSocket)
    {
        NetworkStream networkStream = new NetworkStream(clientSocket);
        byte[] buffer = new byte[2048];
        int bytesRead;



        // Get client's name
        // 클라이언트는 최초 접속시 자신의 이름을 보내준다.
        bytesRead = networkStream.Read(buffer, 0, buffer.Length);
        string initData = Encoding.ASCII.GetString(buffer, 0, bytesRead);

        // 세션 정보를 콤마(,)로 분리
        string[] sessionInfoParts = initData.Split(',');
        // 클라 이름 저장
        string userName = sessionInfoParts[2].TrimEnd('\0');

        // 세션 체크와 현재 내가 접속한 방 정보 저장을 위한 User 클래스의 객체 생성
        User user = new User(userName);

        if (sessionInfoParts.Length == 3 && sessionInfoParts[0].Trim() == "/session")
        {
            string sessionId = sessionInfoParts[1].Trim();

            // 세션 정보 유효성 체크 : Redis에서 세션 조회.
            // 이곳에서 sessionExists를 true로 바꿔주지 않으면 아래의 while문이 실행되지 않는다.
            CheckSessionInRedis(sessionId, user);
            if (!user.sessionExists) // sessionId
            {
                // 세션이 Redis에 없으면 연결 종료
                Console.WriteLine("Invalid session: " + sessionId); // sessionId
                string msg = "300:";
                SendMessage(msg, clientSocket);
            }
        }




        try
        {
            while ((bytesRead = networkStream.Read(buffer, 0, buffer.Length)) > 0 && user.sessionExists)
            {
                string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                Console.WriteLine("Received from client " + user.userName + ": " + message);
                if (message.StartsWith("/join"))
                {
                    string[] parts = message.Split(' ');
                    if (parts.Length == 2)
                    {
                        string roomName = parts[1].TrimEnd('\0');
                        if (rooms.ContainsKey(roomName))
                        {
                            // 방의 인원이 4명보다 적으면
                            if (rooms[roomName].Clients.Count < 4)
                            {
                                // 방 접속 승인, 진짜 방 입장은 ChatScene에서 이루어짐
                                string msg = "200:" + roomName;
                                SendMessage(msg, clientSocket);
                            }
                            else
                            {
                                // 클라 예외처리 구현 필요
                                SendMessage("400: 방이 가득 찼습니다. " + roomName, clientSocket);
                            }
                        }
                    }
                    else
                    // 클라이언트에 예외메세지 출력 구현 필요
                    {
                        SendMessage("Invalid /join command. Usage: /join [room_name]", clientSocket);
                    }
                }
                else if (message.StartsWith("/create"))
                {
                    string[] parts = message.Split(' ');
                    string roomName = parts[1].TrimEnd('\0');
                    if (parts.Length == 2)
                    {
                        CreateRoom(clientSocket, roomName);
                    }
                    else
                    {
                        SendMessage("Invalid Roomname. No spaces allowed. ", clientSocket);
                    }
                }
                else if (message.StartsWith("/list"))
                {
                    SendRoomList(clientSocket);
                }
                // 로비에서 접속을 끊고 채팅방으로 이동(새로운 접속) 후의 상황.
                // 클라이언트에서 방 제목도 같이 보내준다. ex. "/newchat [room_name]"
                // 방 입장 처리를 이곳에서 한다.
                else if (message.StartsWith("/newchat"))
                {
                    string[] parts = message.Split(' ');
                    if (parts.Length == 2)
                    {
                        string roomName = parts[1].TrimEnd('\0');
                        JoinRoom(clientSocket, roomName, user);
                    }
                }
                // 채팅 메세지 처리
                else
                {
                    // parts[0] = 방번호, parts[1] = 메세지 내용
                    string[] parts = message.Split(' ');
                    if (parts.Length == 2)
                    {
                        string roomName = parts[0];
                        string responseData = parts[1];
                        foreach (ChatRoom r in rooms.Values)
                        {
                            if (r.Name == roomName)
                            {
                                r.BroadcastMessage(responseData, clientSocket);
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
            // client와 연결이 종료될 때 유저 목록에서 삭제해준다.
            if (user.currentRoom != null)
            {
                user.currentRoom.RemoveClient(clientSocket);

                // 만약 모두 나가서 방의 인원수가 0이 되면 해당 방을 방 목록에서 삭제해준다. 
                if (user.currentRoom.Clients.Count == 0)
                {
                    rooms.Remove(user.currentRoom.Name);
                }
            }

            clientSocket.Close();
        }
    }
    private void CheckSessionInRedis(string sessionId, User user)
    {
        try
        {
            IDatabase redisDb = redis.GetDatabase();
            // Redis에서 세션 조회
            if (!redisDb.KeyExists(sessionId))
            {
                // 세션이 Redis에 없는 경우 처리
                Console.WriteLine("Session not found in Redis: " + sessionId);
                user.sessionExists = false;
            }
            else
            {
                // Redis에 세션이 존재하는 경우 처리
                Console.WriteLine("Session found in Redis: " + sessionId);
                user.sessionExists = true;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error checking session in Redis: " + ex.Message);
        }
    }
    private void SendMessage(string message, Socket clientSocket)
    {
        byte[] buffer = Encoding.UTF8.GetBytes(message + "\0");
        //byte[] buffer = Encoding.UTF8.GetBytes(message);
        NetworkStream networkStream = new NetworkStream(clientSocket);
        networkStream.Write(buffer, 0, buffer.Length);
        networkStream.Flush();
    }
    private void JoinRoom(Socket clientSocket, string roomName, User user)
    {
        rooms[roomName].AddClient(clientSocket);
        user.currentRoom = rooms[roomName];
        SendMessage("Joined room: " + roomName, clientSocket);
        //rooms[roomName].BroadcastJoinMessage(user.userName + " has joined the room.", clientSocket);
        
    }
    private void SendRoomList(Socket clientSocket)
    {
        JObject jsonData = new JObject();

        List<string> roomList = new List<string>();
        List<string> userCountList = new List<string>();
        foreach (ChatRoom r in rooms.Values)
        {
            roomList.Add(r.Name);
            userCountList.Add(r.Clients.Count.ToString());
        }

        jsonData.Add(new JProperty("roomName", JArray.FromObject(roomList)));
        jsonData.Add(new JProperty("userCount", JArray.FromObject(userCountList)));

        SendMessage(JsonConvert.SerializeObject(jsonData), clientSocket);
    }
    private void CreateRoom(Socket clientSocket, string roomName)
    {
        // 방 생성 후 바로 입장 승인. 실제 방 입장은 ChatScene에서 이루어짐.
        if (!rooms.ContainsKey(roomName))
        {
            rooms.Add(roomName, new ChatRoom(roomName));
            SendMessage("200:" + roomName, clientSocket);
        }
        else
        {
            // 클라 예외처리 구현 필요
            SendMessage("401: 해당 제목의 방이 이미 존재합니다. " + roomName, clientSocket);
        }
    }
    public static void Main(string[] args)
    {
        ChatServer server = new ChatServer();
        server.Start();
    }
}