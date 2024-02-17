using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

// 클라이언트 소켓을 다루는 클래스
class ClientHandler
{
    NetworkStream stream = null; // 네트워크로 데이터 초기화 받기 위한 객체 생성
    Socket socket = null; // AcceptSocket()하면 생성자로 소켓을 던져준다. 이때 그 소켓을 받아낼 변수

    public ClientHandler(Socket socket)
    {
        this.socket = socket; // this.socket == 10번 라인 socket
        ChatServer.list.Add(socket); // 리스트에 소켓을 담는다.
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
                foreach (Socket s in ChatServer.list)
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
            ChatServer.list.Remove(socket);
            socket.Close();
            socket = null;
        }
    }
}

class ChatServer
{
    // 클라이언트 소켓을 보관할 리스트 생성
    public static List<Socket> list = new List<Socket>();

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

                // 클라이언트 소켓을 쓰레드에 싣는 부분
                ClientHandler cHandler = new ClientHandler(clientsocket);
                Thread t = new Thread(new ThreadStart(cHandler.chat));
                t.Start();
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
