using System;
using System.Collections.Generic;
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

        string myRoomNum = "-1";

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
                        ChatRoom room = GetOrCreateRoom(roomName);
                        // 방에 client 추가
                        room.AddClient(client);
                                                

                        // 내 방번호 저장
                        myRoomNum = roomName;

                        Console.WriteLine("Client joined room: " + roomName);

                        string data = "200 : ok";

                        var responseData = Encoding.ASCII.GetBytes(data);
                        stream.Write(responseData, 0, responseData.Length);
                        stream.Flush();
                    }
                }
                else
                {
                    // 현재 0번 방에게 broadcast 중
                    string[] parts = message.Split(':');
                    if (parts.Length == 2)
                    {
                        //rooms[0].BroadcastMessage(message, client);
                    }

                    
                    // Broadcast message to client's room
                    foreach (ChatRoom room in rooms)
                    {
                        if (room.Name == myRoomNum)
                        {
                            room.BroadcastMessage(message, client);
                            break;
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