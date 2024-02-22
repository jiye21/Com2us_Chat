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

    [SerializeField]
    string ipAddr = "127.0.0.1";
    [SerializeField]
    int port = 9999;


    TcpClient tcpClient;

    [SerializeField]
    GameObject content;

    Queue<string> msgQueue;

    string roomName;

    [SerializeField]
    TMP_InputField roomText;

    [SerializeField]
    GameObject createRoomCanvas;

    [SerializeField]
    GameObject roomPrefab;

    [SerializeField]
    ScrollRect uiView;

    void Start()
    {
        msgQueue = new Queue<string>();

        ConnectTCP();

        // 방 목록 가져오는 코루틴 시작. 3초마다 갱신함. 
        StartCoroutine("GetRoomList");
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
                GameObject.Find("GameManager").GetComponent<GameManager>().myRoomNum = textList[1];
                SceneManager.LoadScene("ChatScene");
            }

            // 방 목록이 들어오면
            if (textList[0] == "Available_rooms")
            {
                var roomInfo = textList[1].Split("and");

                var roomList = roomInfo[0].Split(", ");
                var userCount = roomInfo[1].Split(", ");

                Debug.Log(roomInfo[0]);
                Debug.Log(roomInfo[1]);
                Debug.Log(roomList[0]);
                Debug.Log(userCount[2]);


                SetRoomList(roomList, userCount);
            }
        }

    }

    IEnumerator GetRoomList()
    {
        while(true)
        {
            if (tcpClient == null || !tcpClient.Connected) continue;

            var data = Encoding.UTF8.GetBytes("/list");
            tcpClient.GetStream().Write(data);

            yield return new WaitForSecondsRealtime(3.0f);
        }
    }


    int flag = 0;

    void SetRoomList(string[] roomList, string[] userCount)
    {
        int cnt = content.transform.childCount;

        // 현재 내 방제목이 서버에서 보내준 방제목과 같을 때 인원수만 갱신. 
        for (int i = 0 ;  i < cnt; i++)
        {
            Transform roomObj = content.transform.GetChild(i);

            TMP_Text[] texts = roomObj.GetComponentsInChildren<TMP_Text>();

            // 텍스트 넣기
            for(int j = 0; j<roomList.Length; j++)
            {
                if(texts[0].text == roomList[j])
                {
                    texts[1].text = userCount[j] + "/10";

                    // 반영한 유저수는 삭제하기 위해 -1 대입. 
                    userCount[j] = "-1";
                }
            }
        }

        // 새로운 방이 생겼다면(데이터가 반영이 안됨) 방 prefab을 새로 만들어준다. 
        // 새로 방 prefab을 추가해 데이터를 적용해준다. 
        for (int j = 0; j < roomList.Length; j++)
        {
            if (userCount[j] != "-1")
            {
                var newRoomList = Instantiate(roomPrefab, Vector3.zero, Quaternion.identity);

                TMP_Text[] texts = newRoomList.GetComponentsInChildren<TMP_Text>();

                texts[0].text = roomList[j];
                texts[1].text = userCount[j] + "/10";

                // 뷰박스에 넣고 정보 갱신
                newRoomList.transform.SetParent(uiView.content, false);
                LayoutRebuilder.ForceRebuildLayoutImmediate(uiView.content);

                //  스크롤 갱신
                var view = uiView.transform as RectTransform;
                if (view.rect.height < uiView.content.rect.height)
                {
                    uiView.content.anchoredPosition = new Vector2(0, uiView.content.rect.height);
                }
            }

        }

        if(flag == 0)
        {
            SetBtn();
            flag++;
        }

        //SetBtn();
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


    public void SetBtn()
    {
        // 방 입장 버튼 init
        // 버튼에 onClick 리스너 등록, 방 번호는 0번부터 차례로 시작 
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


    // 방 입장 버튼 리스너
    public void EnterBtn(int btnNum)
    {
        Debug.Log("btnNum: " + btnNum);
        if (tcpClient == null || !tcpClient.Connected) return;

        //  서버에 전송하기 (GetStream().Write)
        var data = Encoding.UTF8.GetBytes("/join " + btnNum.ToString());
        tcpClient.GetStream().Write(data);

    }

    // 로비의 방 생성 버튼, 방제목 입력 Canvas 띄워줌. 수동으로 달아줌. 
    public void CreateBtnInLobby()
    {
        createRoomCanvas.SetActive(true);


    }

    // 방 생성 버튼, 수동으로 달아줌. 
    public void CreateBtn()
    {
        if (tcpClient == null || !tcpClient.Connected) return;


        //  서버에 전송하기 (GetStream().Write)
        roomName = roomText.text;
        Debug.Log("btn: create" + roomName);

        var data = Encoding.UTF8.GetBytes("/create " + roomName);
        tcpClient.GetStream().Write(data);

        // Canvas 닫음
        createRoomCanvas.SetActive(false);
    }

    // Canvas 닫는 버튼, 수동으로 달아줌. 
    public void CloseBtn()
    {
        createRoomCanvas.SetActive(false);
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

        // 방목록을 받아오고 세팅하는 코루틴들 종료
        StopAllCoroutines();
    }
    
}
