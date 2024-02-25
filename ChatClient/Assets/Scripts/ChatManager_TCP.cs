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
using UnityEngine.SceneManagement;


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

    
    TcpClient tcpClient = null;
    
    Queue<string> msgQueue;

    public string myRoomName;

    public TMP_Text roomName;

    private string username;


    public void Start()
    {
        // GameManager���� �޾ƿ� username�� ���� ������ ����
        username = GameObject.Find("GameManager").GetComponent<GameManager>().Username;

        myRoomName = GameObject.Find("GameManager").GetComponent<GameManager>().myRoomName;
        // �� ������ ǥ��
        roomName.text = myRoomName;
        //  �޽��� ť
        msgQueue = new Queue<string>();

        ConnectTCP();

        StartCoroutine(SendInitData());

    }


    void Update()
    {
        if (msgQueue.Count > 0)
        {
            var msg = msgQueue.Dequeue();

            Debug.Log("ChatScene : Received from Server  " + msg);

            // �� �����ߴٴ� �޼��� ó��
            if (msg.StartsWith("Joined room: "))
            {
                // ������Ʈ ����
                var newtextobj = Instantiate(msgPrefab, Vector3.zero, Quaternion.identity);

                // �ؽ�Ʈ �ֱ�
                TMP_Text[] texts = newtextobj.GetComponentsInChildren<TMP_Text>();
                texts[0].text = "";
                texts[1].text = msg;
                

                // ��ڽ��� �ְ� ���� ����
                newtextobj.transform.SetParent(uiView.content, false);
                LayoutRebuilder.ForceRebuildLayoutImmediate(uiView.content);
            }
            // ä�� �����Ͱ� ���� �� ó��
            else
            {
                Debug.Log(msg);

                //  ���ڿ� �ڸ���
                var textList = msg.Split(":");
                if (textList.Length == 2)
                {
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
        }

       
    }

    IEnumerator SendInitData()
    {
        while(true)
        {
            yield return null;

            // ������ �����ɶ����� ��ٸ�. 
            if(tcpClient == null)
            {
                continue;
            }

            // ������ �ڽ��� �̸��� �ѹ� �����ش�. 
            var myName = Encoding.UTF8.GetBytes(username);
            tcpClient.GetStream().Write(myName);


            // ������ ���� ����濡 �����ߴ��� �ѹ� �����ش�. 
            // ������ �� ��ȣ �����͸� �ϴ� �޾ƾ� Ŭ���̾�Ʈ�� �濡 �����Ű�� ó���� �ϱ� ����. 
            var data = Encoding.UTF8.GetBytes("/newchat " + myRoomName);
            tcpClient.GetStream().Write(data);
            
            yield break;
        }


    }

    public void ExitBtn()
    {
        SceneManager.LoadScene("LobbyScene");
    }


    // TCP ���� �õ��� ���������� �Ϸ�Ǿ��� �� �����
    private void requestCall(System.IAsyncResult ar)
    {
        byte[] buf = new byte[2048];

        

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

                string msg = Encoding.UTF8.GetString(data);

                var texts = msg.Split("\0");

                Debug.Log("ť�� �� ������ " + texts[0]);

                //msgQueue.Enqueue(Encoding.UTF8.GetString(data));
                msgQueue.Enqueue(texts[0]);

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
        var data = Encoding.UTF8.GetBytes(myRoomName + " " + uiID.text + ":" + uiMsg.text + "\0");
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
