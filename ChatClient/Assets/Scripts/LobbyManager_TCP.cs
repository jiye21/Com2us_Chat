using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyManager_TCP : MonoBehaviour
{
    //private static LobbyManager_TCP instance;

    [SerializeField]
    string ipAddr = "127.0.0.1";
    [SerializeField]
    int port = 9999;


    TcpClient tcpClient;

    [SerializeField]
    GameObject content;

    Queue<string> msgQueue;


    void Start()
    {
        // singleton
        //if (instance != null) DestroyImmediate(gameObject);

        //instance = this;
        //DontDestroyOnLoad(gameObject);

        msgQueue = new Queue<string>();

        ConnectTCP();

        // 나중에 방 목록 업데이트 되면 또 버튼 리스너 달아줘야 함
        SetEnterBtn();
    }


    void Update()
    {
        // SceneManager.LoadScene 작업은 메인 스레드에서만 해야 해서 들어온 데이터를 큐에 담아 메인 스레드에서 처리
        if (msgQueue.Count > 0)
        {
            var msg = msgQueue.Dequeue();

            //  문자열 자르기
            var textList = msg.Split(" : ");

            if (textList[0] == "200")
            {
                SceneManager.LoadScene("ChatScene");
            }

        }

    }

    /// <summary>
    /// TCP client callback. TCP 연결 시도가 성공적으로 완료되었을 때 실행됨
    /// </summary>
    /// <param name="ar"></param>
    private void StartReadingTCP(System.IAsyncResult ar)
    {
        byte[] buf = new byte[512];

        // TCP 클라이언트의 네트워크 스트림에서 비동기적으로 데이터를 읽고,
        // 데이터를 읽은 후에는 OnTCPDataReceived 함수를 호출하여 데이터를 처리
        tcpClient.GetStream().BeginRead(buf, 0, buf.Length, OnTCPDataReceived, buf);
    }

    // 수신된 데이터가 처리되는 함수
    private void OnTCPDataReceived(System.IAsyncResult ar)
    {
        try
        {
            var byteRead = tcpClient.GetStream().EndRead(ar);

            if (byteRead > 0)
            {
                byte[] data = (byte[])ar.AsyncState;
                string msg = Encoding.UTF8.GetString(data);

                msgQueue.Enqueue(Encoding.UTF8.GetString(data));

                tcpClient.GetStream().BeginRead(data, 0, data.Length, OnTCPDataReceived, data);
            }
            else
            {
                tcpClient.GetStream().Close();
                tcpClient.Close();
                tcpClient = null;
            }
        }
        catch (SocketException e)
        {
            Debug.LogException(e);
        }
    }


    public void EnterBtn(int btnNum)
    {
        Debug.Log("btnNum: "+btnNum);
        if (tcpClient == null || !tcpClient.Connected) return;

        //  서버에 전송하기 (GetStream().Write)
        var data = Encoding.UTF8.GetBytes("/join " + btnNum.ToString());
        tcpClient.GetStream().Write(data);

    }


    // 버튼에 onClick 리스너 등록, 방 번호는 0번부터 차례로 시작 
    public void SetEnterBtn()
    {
        int cnt = content.transform.childCount;
        for(int i = 0; i < cnt; i++)
        {
            Transform btn = content.transform.GetChild(i);
            int index = i;
            btn.GetComponentInChildren<Button>().onClick.AddListener(() => 
            {
                EnterBtn(index);
            });

        }

    }


    public void ConnectTCP()
    {
        if (tcpClient != null)
            return;

        tcpClient = new TcpClient();
        tcpClient.BeginConnect(ipAddr, port, StartReadingTCP, null);
    }


    private void OnDestroy()
    {
        if (tcpClient != null)
        {
            tcpClient.GetStream().Close();
            tcpClient.Close();
            tcpClient = null;
        }
    }
    
}
