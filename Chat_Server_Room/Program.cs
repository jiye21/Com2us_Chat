using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class ChatRoom
{
    public string Name { get; }
    public List<Socket> Clients = new List<Socket>();

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

            // 자기 자신에게는 되보내지 않음
            if (client != sender)
            {
            }
        }
    }

    public void AddClient(Socket client)
    {
        // 이미 client가 list에 담겨 있는 상태라면 바로 함수를 종료한다. 
        foreach(Socket c in Clients)
        {
            if (c == client) return;
        }

        Clients.Add(client);
    }

    public void RemoveClient(Socket client)
    {
        Clients.Remove(client);
    }
}

public class ChatServer
{
    public static Dictionary<string, ChatRoom> rooms = new Dictionary<string, ChatRoom>();

    // 현재 내가 접속한 방 정보
    public ChatRoom currentRoom;


    public void Start()
    {
        TcpListener serverSocket = new TcpListener(IPAddress.Any, 9999);

        serverSocket.Start();
        Console.WriteLine("Chat server started...");


        // 테스트용 dummy rooms
        ChatRoom room01 = new ChatRoom("room01");
        ChatRoom room02 = new ChatRoom("22222");
        ChatRoom room03 = new ChatRoom("hello_world");

        /*
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
        */

        rooms.Add(room01.Name, room01);
        rooms.Add(room02.Name, room02);
        rooms.Add(room03.Name, room03);



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

        currentRoom = null;

        // Get client's name
        // 클라이언트는 최초 접속시 자신의 이름을 보내준다. 
        bytesRead = networkStream.Read(buffer, 0, buffer.Length);
        string clientName = Encoding.ASCII.GetString(buffer, 0, bytesRead);


        try
        {
            while ((bytesRead = networkStream.Read(buffer, 0, buffer.Length)) > 0)
            {
                string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                Console.WriteLine("Received from client " + clientName + ": " + message);

                // Check if message is a command to join a room
                // ex) "/join 10"
                if (message.StartsWith("/join"))
                {
                    string[] parts = message.Split(' ');
                    if (parts.Length == 2)
                    {
                        string roomName = parts[1];

                        if (rooms.ContainsKey(roomName))
                        {
                            // 방 접속 승인, 진짜 방 입장은 ChatScene에서 이루어짐
                            string msg = "200:" + roomName;
                            SendMessage(msg, clientSocket);
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
                    if (parts.Length == 2)
                    {
                        string roomName = parts[1];
                        CreateRoom(clientSocket, roomName);
                    }
                    else
                    // 클라이언트에 예외메세지 출력 구현 필요
                    {
                        SendMessage("Invalid /create command. Usage: /create [room_name]", clientSocket);
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
                        string roomName = parts[1];
                        JoinRoom(clientSocket, clientName, roomName);
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

                        foreach(ChatRoom r in rooms.Values)
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
            // client와 연결이 종료될 때 방 목록에서도 삭제해준다. 
            if(currentRoom != null) currentRoom.RemoveClient(clientSocket);
            clientSocket.Close();
        }
    }

    private void SendMessage(string message, Socket clientSocket)
    {
        byte[] buffer = Encoding.UTF8.GetBytes(message+"\0");
        NetworkStream networkStream = new NetworkStream(clientSocket);
        networkStream.Write(buffer, 0, buffer.Length);
        networkStream.Flush();

    }

    private void JoinRoom(Socket clientSocket, string clientName, string roomName)
    {
        if (rooms.ContainsKey(roomName))
        {
            rooms[roomName].AddClient(clientSocket);
            currentRoom = rooms[roomName];
            SendMessage("Joined room: " + roomName, clientSocket);
            rooms[roomName].BroadcastMessage(clientName + " has joined the room.", clientSocket);
        }
        else
        {
            // 클라 예외처리 구현 필요
            SendMessage("Room does not exist: " + roomName, clientSocket);
        }
    }

    private void SendRoomList(Socket clientSocket)
    {
        string roomList = string.Join(", ", rooms.Keys);
        List<string> userCount = new List<string>();
        foreach (ChatRoom r in rooms.Values)
        {
            userCount.Add(r.Clients.Count.ToString());
        }
        string userCountList = string.Join(", ", userCount);

        SendMessage("/list:" + roomList + "&" + userCountList, clientSocket);
    }

    private void CreateRoom(Socket clientSocket, string roomName)
    {
        var checkSpace = roomName.Split(' ');
        if (checkSpace.Length > 2 )
        {
            SendMessage("Invalid roomName. No spaces allowed. ", clientSocket);
            return;
        }

        // 방 생성 후 바로 입장 승인. 실제 방 입장은 ChatScene에서 이루어짐. 
        if (!rooms.ContainsKey(roomName))
        {
            rooms.Add(roomName, new ChatRoom(roomName));
            SendMessage("200:"+roomName, clientSocket);
            //SendMessage("Room created: " + roomName, clientSocket);
        }
        else
        {
            // 클라 예외처리 구현 필요
            SendMessage("Room already exists: " + roomName, clientSocket);
        }
    }



    public static void Main(string[] args)
    {
        ChatServer server = new ChatServer();
        server.Start();

        
    }
}