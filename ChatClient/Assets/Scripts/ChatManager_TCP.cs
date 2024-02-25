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
        // GameManager에서 받아온 username을 전역 변수에 저장
        username = GameObject.Find("GameManager").GetComponent<GameManager>().Username;

        myRoomName = GameObject.Find("GameManager").GetComponent<GameManager>().myRoomName;
        // 내 방제목 표시
        roomName.text = myRoomName;
        //  메시지 큐
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

            // 방 입장했다는 메세지 처리
            if (msg.StartsWith("Joined room: "))
            {
                // 오브젝트 생성
                var newtextobj = Instantiate(msgPrefab, Vector3.zero, Quaternion.identity);

                // 텍스트 넣기
                TMP_Text[] texts = newtextobj.GetComponentsInChildren<TMP_Text>();
                texts[0].text = "";
                texts[1].text = msg;
                

                // 뷰박스에 넣고 정보 갱신
                newtextobj.transform.SetParent(uiView.content, false);
                LayoutRebuilder.ForceRebuildLayoutImmediate(uiView.content);
            }
            // 채팅 데이터가 왔을 때 처리
            else
            {
                Debug.Log(msg);

                //  문자열 자르기
                var textList = msg.Split(":");
                if (textList.Length == 2)
                {
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
        }

       
    }

    IEnumerator SendInitData()
    {
        while(true)
        {
            yield return null;

            // 연결이 수립될때까지 기다림. 
            if(tcpClient == null)
            {
                continue;
            }

            // 서버에 자신의 이름을 한번 보내준다. 
            var myName = Encoding.UTF8.GetBytes(username);
            tcpClient.GetStream().Write(myName);


            // 서버에 내가 몇번방에 입장했는지 한번 보내준다. 
            // 서버는 방 번호 데이터를 일단 받아야 클라이언트를 방에 입장시키는 처리를 하기 때문. 
            var data = Encoding.UTF8.GetBytes("/newchat " + myRoomName);
            tcpClient.GetStream().Write(data);
            
            yield break;
        }


    }

    public void ExitBtn()
    {
        SceneManager.LoadScene("LobbyScene");
    }


    // TCP 연결 시도가 성공적으로 완료되었을 때 실행됨
    private void requestCall(System.IAsyncResult ar)
    {
        byte[] buf = new byte[2048];

        

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

                string msg = Encoding.UTF8.GetString(data);

                var texts = msg.Split("\0");

                Debug.Log("큐에 들어간 데이터 " + texts[0]);

                //msgQueue.Enqueue(Encoding.UTF8.GetString(data));
                msgQueue.Enqueue(texts[0]);

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
        var data = Encoding.UTF8.GetBytes(myRoomName + " " + uiID.text + ":" + uiMsg.text + "\0");
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
