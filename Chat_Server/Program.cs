using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

/*
 C#에서는 #define 전처리기 지시문을 사용하여 일반적으로 C와 C++에서 사용되는 방식으로 상수를 정의할 수 없다. 
 정수가 아닌 상수를 정의하는 한 가지 방법은 Constants라는 단일 정적 클래스로 그룹화하는 것이다. 
 이 경우 상수에 대한 모든 참조 앞에 클래스 이름(Constants)이 와야 한다. 
*/

enum PACKET_TYPE
{
    LOGIN,
    CHAT,
    ROOM_ID,
    //GET_ID,
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
class BasePacket
{
    public ushort packet_len;
    public ushort packet_id;
};

[StructLayout(LayoutKind.Sequential, Pack = 1)]
class Room_IDPacket : BasePacket
{
    public int roomID;

    public Room_IDPacket()
    {
        roomID = 0;
        packet_id = (ushort)PACKET_TYPE.ROOM_ID;
        packet_len = (ushort)Marshal.SizeOf(typeof(Room_IDPacket));
    }
};


// 클라이언트 소켓을 다루는 클래스
class ClientHandler
{
    NetworkStream stream = null; // 네트워크로 데이터 초기화 받기 위한 객체 생성
    Socket socket = null; // AcceptSocket()하면 생성자로 소켓을 던져준다. 이때 그 소켓을 받아낼 변수
    int roomIndex;

    public ClientHandler(Socket socket, int roomIndex)
    {
        this.socket = socket; // this.socket == 10번 라인 socket
        this.roomIndex = roomIndex;
        ChatServer.roomList[roomIndex].Add(socket); // 리스트에 소켓을 담는다.
    }

    // 클라이언트 소켓을 처리하는 메소드
    public void chat()
    {
        stream = new NetworkStream(socket);
        try
        {
            byte[] buffer = new byte[1024];
            int bytesRead;
            while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) != 0)
            {
                // 클라이언트가 보낸 글을 읽는 부분
                string dataReceived = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                Console.WriteLine("클라이언트로부터 수신한 데이터: " + dataReceived);

                // 클라이언트에게 응답 전송 (echo)
                byte[] responseData = Encoding.UTF8.GetBytes(dataReceived);

                // 배열리스트에 보관된 모든 클라이언트 처리 소켓만큼
                // 현재 접속한 모든 클라이언트에게 글을 쓴다.
                foreach (Socket s in ChatServer.roomList[roomIndex])
                {
                    stream = new NetworkStream(s);
                    stream.Write(responseData, 0, responseData.Length);
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
        finally
        {
            ChatServer.roomList[roomIndex].Remove(socket);
            socket.Close();
            socket = null;
        }
    }
}



class PacketHandler
{

    public void AddPacket()
    {

    }

    public void ReadPacket()
    {

    }
}

class ChatServer
{
    // 클라이언트 소켓을 보관할 리스트 생성
    public static List<List<Socket>> roomList = new List<List<Socket>>();

    public static List<Socket> room01 = new List<Socket>();
    public static List<Socket> room02 = new List<Socket>();
    public static List<Socket> room03 = new List<Socket>();


    public static void Main()
    {
        TcpListener tcpListener = null;
        Socket clientsocket = null;
        try
        {
            // TcpListener를 생성 시 인자로 사용할 객체 생성
            IPAddress iPAd = IPAddress.Parse("127.0.0.1");
            tcpListener = new TcpListener(iPAd, 9999);
            tcpListener.Start();

            Console.WriteLine("서버 시작..클라이언트 연결 대기중");

            while (true)
            {
                clientsocket = tcpListener.AcceptSocket(); // 서버 대기
                Console.WriteLine("Connection accepted.");

                PacketHandler pHandler = new PacketHandler();
                pHandler.

                // 패킷 처리
                switch()
                {
                    case 0:
                        // 클라이언트 소켓을 쓰레드에 싣는 부분
                        ClientHandler cHandler = new ClientHandler(clientsocket, 0);
                        Thread t = new Thread(new ThreadStart(cHandler.chat));
                        t.Start();
                        break;
                    case 1:
                        ClientHandler cHandler = new ClientHandler(clientsocket, 1);
                        Thread t = new Thread(new ThreadStart(cHandler.chat));
                        t.Start();
                        break;
                    case 2:
                        ClientHandler cHandler = new ClientHandler(clientsocket, 2);
                        Thread t = new Thread(new ThreadStart(cHandler.chat));
                        t.Start();
                        break;
                }

                // 클라이언트 소켓을 쓰레드에 싣는 부분
                //ClientHandler cHandler = new ClientHandler(clientsocket, 0);
                //Thread t = new Thread(new ThreadStart(cHandler.chat));
                //t.Start();
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
        finally
        {
            clientsocket.Close();
        }
    }

}
