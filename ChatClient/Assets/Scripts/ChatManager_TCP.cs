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

        //  메시지 큐
        msgQueue = new Queue<string>();

        ConnectTCP();
    }


    void Update()
    {
        if (msgQueue.Count > 0)
        {
            var msg = msgQueue.Dequeue();

            //  문자열 자르기
            var textList = msg.Split(":");

            Debug.Log(msg);

            // 오브젝트 생성
            var newtextobj = Instantiate(msgPrefab, Vector3.zero, Quaternion.identity);

            // 텍스트 넣기
            TMP_Text[] texts = newtextobj.GetComponentsInChildren<TMP_Text>();
            texts[0].text = textList[0];
            texts[1].text = textList[1];

            // 뷰박스에 넣고 정보 갱신
            newtextobj.transform.SetParent(uiView.content, false);
            LayoutRebuilder.ForceRebuildLayoutImmediate(uiView.content);

            //  스크롤 갱신
            var view = uiView.transform as RectTransform;
            if (view.rect.height < uiView.content.rect.height)
            {
                uiView.content.anchoredPosition = new Vector2(0, uiView.content.rect.height);
            }
        }

       
    }



    
    // TCP 연결 시도가 성공적으로 완료되었을 때 실행됨
    private void requestCall(System.IAsyncResult ar)
    {
        byte[] buf = new byte[512];

        // 서버에 내가 몇번방에 입장했는지 한번 보내준다. 
        // 서버는 방 번호 데이터를 일단 받아야 클라이언트를 방에 입장시키는 처리를 하기 때문. 
        var data = Encoding.UTF8.GetBytes(myRoomNum + ")" + "new client connected");
        tcpClient.GetStream().Write(data);

        // TCP 클라이언트의 네트워크 스트림에서 비동기적으로 데이터를 읽고,
        // 데이터를 읽은 후에는 requestCallTCP 함수를 호출하여 데이터를 처리
        tcpClient.GetStream().BeginRead(buf, 0, buf.Length, requestCallTCP, buf);
    }
        
    // 수신된 데이터가 처리되는 함수
    private void requestCallTCP(System.IAsyncResult ar)
    {
        try
        {
            var byteRead = tcpClient.GetStream().EndRead(ar);

            if (byteRead > 0)
            {
                byte[] data = (byte[])ar.AsyncState;

                msgQueue.Enqueue(Encoding.UTF8.GetString(data));

                // 다시 데이터를 읽기 시작. 
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
    


    // MsgInputfield에 이벤트 등록 되어있음
    public void InputEnter()
    {
        //  이름이나 내용이 없으면 안보냄
        if (uiID.text.Length == 0 || uiMsg.text.Length == 0)
            return;
        if (tcpClient == null || !tcpClient.Connected)
            return;


        //  서버에 전송하기 (GetStream().Write)
        var data = Encoding.UTF8.GetBytes(myRoomNum + ")" + uiID.text + ":" + uiMsg.text + "\0");
        tcpClient.GetStream().Write(data);

        //  문자열 초기화
        uiMsg.text = "";

        //  입력박스 포커스 유지
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
