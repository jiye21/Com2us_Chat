using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text;
using System.IO;
using System.Threading;
using System;
using System.Threading.Tasks;


public class ChatManager_TCP : MonoBehaviour
{
    [SerializeField]
    string ipAddr = "127.0.0.1";
    [SerializeField]
    int port = 9999;

    [SerializeField]
    TMP_InputField uiID;
    [SerializeField]
    TMP_InputField uiMsg;

    [SerializeField]
    ScrollRect uiView;

    [SerializeField]
    GameObject msgPrefab;

    
    TcpClient tcpClient;
    
    Queue<string> msgQueue;

    string myRoomNum;

    public void Start()
    {
        myRoomNum = GameObject.Find("RoomNumManager").GetComponent<RoomNumManager>().myRoomNum;

        //  �޽��� ť
        msgQueue = new Queue<string>();

        ConnectTCP();
    }


    void Update()
    {
        if (msgQueue.Count > 0)
        {
            var msg = msgQueue.Dequeue();

            //  ���ڿ� �ڸ���
            var textList = msg.Split(":");

            Debug.Log(msg);

            // ������Ʈ ����
            var newtextobj = Instantiate(msgPrefab, Vector3.zero, Quaternion.identity);

            // �ؽ�Ʈ �ֱ�
            TMP_Text[] texts = newtextobj.GetComponentsInChildren<TMP_Text>();
            texts[0].text = textList[0];
            texts[1].text = textList[1];

            // ��ڽ��� �ְ� ���� ����
            newtextobj.transform.SetParent(uiView.content, false);
            LayoutRebuilder.ForceRebuildLayoutImmediate(uiView.content);

            //  ��ũ�� ����
            var view = uiView.transform as RectTransform;
            if (view.rect.height < uiView.content.rect.height)
            {
                uiView.content.anchoredPosition = new Vector2(0, uiView.content.rect.height);
            }
        }

       
    }



    
    // TCP ���� �õ��� ���������� �Ϸ�Ǿ��� �� �����
    private void requestCall(System.IAsyncResult ar)
    {
        byte[] buf = new byte[512];

        // ������ ���� ����濡 �����ߴ��� �ѹ� �����ش�. 
        // ������ �� ��ȣ �����͸� �ϴ� �޾ƾ� Ŭ���̾�Ʈ�� �濡 �����Ű�� ó���� �ϱ� ����. 
        var data = Encoding.UTF8.GetBytes(myRoomNum + ")" + "new client connected");
        tcpClient.GetStream().Write(data);

        // TCP Ŭ���̾�Ʈ�� ��Ʈ��ũ ��Ʈ������ �񵿱������� �����͸� �а�,
        // �����͸� ���� �Ŀ��� requestCallTCP �Լ��� ȣ���Ͽ� �����͸� ó��
        tcpClient.GetStream().BeginRead(buf, 0, buf.Length, requestCallTCP, buf);
    }
        
    // ���ŵ� �����Ͱ� ó���Ǵ� �Լ�
    private void requestCallTCP(System.IAsyncResult ar)
    {
        try
        {
            var byteRead = tcpClient.GetStream().EndRead(ar);

            if (byteRead > 0)
            {
                byte[] data = (byte[])ar.AsyncState;

                msgQueue.Enqueue(Encoding.UTF8.GetString(data));

                // �ٽ� �����͸� �б� ����. 
                tcpClient.GetStream().BeginRead(data, 0, data.Length, requestCallTCP, data);
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
    


    // MsgInputfield�� �̺�Ʈ ��� �Ǿ�����
    public void InputEnter()
    {
        //  �̸��̳� ������ ������ �Ⱥ���
        if (uiID.text.Length == 0 || uiMsg.text.Length == 0)
            return;
        if (tcpClient == null || !tcpClient.Connected)
            return;


        //  ������ �����ϱ� (GetStream().Write)
        var data = Encoding.UTF8.GetBytes(myRoomNum + ")" + uiID.text + ":" + uiMsg.text + "\0");
        tcpClient.GetStream().Write(data);

        //  ���ڿ� �ʱ�ȭ
        uiMsg.text = "";

        //  �Է¹ڽ� ��Ŀ�� ����
        uiMsg.Select();
        uiMsg.ActivateInputField();
    }
    
    
    
    public void ConnectTCP()
    {
        if (tcpClient != null)
            return;

        tcpClient = new TcpClient();
        tcpClient.BeginConnect(ipAddr, port, requestCall, null);
    }
    
    
    private void OnDestroy()
    {
        if (tcpClient != null)
            tcpClient.Close();
    }
    
}
