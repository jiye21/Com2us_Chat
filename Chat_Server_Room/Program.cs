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
    private List<TcpClient> clients = new List<TcpClient>();

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
    public static List<ChatRoom> rooms = new List<ChatRoom>();

    public ChatServer(int port)
    {
        listener = new TcpListener(IPAddress.Any, port);
    }

    public void Start()
    {
        listener.Start();
        Console.WriteLine("Chat server started...");

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
        TcpClient client = (TcpClient)obj;
        NetworkStream stream = client.GetStream();

        byte[] buffer = new byte[1024];
        int bytesRead;

        ChatRoom room = null;

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
                        var responseData = Encoding.ASCII.GetBytes("200 : " + roomName);
                        stream.Write(responseData, 0, responseData.Length);
                        stream.Flush();
                    }
                }
                else
                {
                    // 로비에서 접속을 끊고 채팅방으로 이동(새로운 접속) 후의 상황. 
                    // 클라이언트에서 몇번 방인지 같이 보내준다. 
                    // ex. "0)id:msg"
                    string[] parts = message.Split(')');
                    // parts[0] = 방번호
                    // parts[1] = 메세지 내용
                    Console.WriteLine(parts[0]);
                    Console.WriteLine(parts[1]);

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
                        foreach (ChatRoom r in rooms)
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

    private ChatRoom GetOrCreateRoom(string roomName)
    {
        foreach (ChatRoom room in rooms)
        {
            if (room.Name == roomName)
            {
                return room;
            }
        }

        ChatRoom newRoom = new ChatRoom(roomName);
        // roomlist에 room 추가
        rooms.Add(newRoom);
        return newRoom;
    }

    public static void Main(string[] args)
    {
        int port = 9999; // Change this to desired port number
        ChatServer server = new ChatServer(port);
        server.Start();
    }
}