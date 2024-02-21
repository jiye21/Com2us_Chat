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

        // ���߿� �� ��� ������Ʈ �Ǹ� �� ��ư ������ �޾���� ��
        SetEnterBtn();
    }


    void Update()
    {
        // SceneManager.LoadScene �۾��� ���� �����忡���� �ؾ� �ؼ� ���� �����͸� ť�� ��� ���� �����忡�� ó��
        if (msgQueue.Count > 0)
        {
            var msg = msgQueue.Dequeue();

            //  ���ڿ� �ڸ���
            var textList = msg.Split(" : ");

            if (textList[0] == "200")
            {
                SceneManager.LoadScene("ChatScene");
            }

        }

    }

    /// <summary>
    /// TCP client callback. TCP ���� �õ��� ���������� �Ϸ�Ǿ��� �� �����
    /// </summary>
    /// <param name="ar"></param>
    private void StartReadingTCP(System.IAsyncResult ar)
    {
        byte[] buf = new byte[512];

        // TCP Ŭ���̾�Ʈ�� ��Ʈ��ũ ��Ʈ������ �񵿱������� �����͸� �а�,
        // �����͸� ���� �Ŀ��� OnTCPDataReceived �Լ��� ȣ���Ͽ� �����͸� ó��
        tcpClient.GetStream().BeginRead(buf, 0, buf.Length, OnTCPDataReceived, buf);
    }

    // ���ŵ� �����Ͱ� ó���Ǵ� �Լ�
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

        //  ������ �����ϱ� (GetStream().Write)
        var data = Encoding.UTF8.GetBytes("/join " + btnNum.ToString());
        tcpClient.GetStream().Write(data);

    }


    // ��ư�� onClick ������ ���, �� ��ȣ�� 0������ ���ʷ� ���� 
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
