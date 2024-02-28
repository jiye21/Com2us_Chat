using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
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

    [SerializeField]
    TMP_InputField roomText;

    [SerializeField]
    GameObject createRoomCanvas;

    [SerializeField]
    GameObject roomPrefab;

    [SerializeField]
    ScrollRect uiView;

    GameObject alertPanelInCreateCanvas;

    public GameObject alertPanel;

    public class ListData
    {
        public string[] roomName;
        public string[] userCount;
    }

    void Start()
    {
        msgQueue = new Queue<string>();

        ConnectTCP();


        // 방 목록 가져오는 코루틴 시작. 3초마다 갱신함. 
        StartCoroutine("GetRoomList");

        alertPanelInCreateCanvas = createRoomCanvas.transform.GetChild(5).gameObject;
    }


    void Update()
    {
        // SceneManager.LoadScene 작업은 메인 스레드에서만 해야 해서 들어온 데이터를 큐에 담아 메인 스레드에서 처리
        if (msgQueue.Count > 0)
        {
            var msg = msgQueue.Dequeue();

            Debug.Log("LoginScene : Received from Server  " + msg);

            // 방 입장 승인 처리
            if (msg.StartsWith("200"))
            {
                var textList = msg.Split(":");

                // 맨 마지막 문자열의 공백 제거 후 GameManager에 방 번호를 저장해준다. 
                GameObject.Find("GameManager").GetComponent<GameManager>().myRoomName = textList[1].TrimEnd('\0');
                SceneManager.LoadScene("ChatScene");
            }

            // 세션이 Redis에 없을 때 연결 종료처리
            else if (msg.StartsWith("300"))
            {
                // 경고메세지 추후 로그인씬에 뜨게 하기
                var textList = msg.Split(":");
                //textList[1] + textList[2]

                SceneManager.LoadScene("LoginScene");
            }
            else if (msg.StartsWith("400"))
            {
                var textList = msg.Split(":");

                // 경고 메세지 출력
                StartCoroutine(SetAlertPanel(textList[1]));
            }
            else if (msg.StartsWith("401"))
            {
                var textList = msg.Split(":");

                // 경고 메세지 출력                
                StartCoroutine(SetAlertPanelInCreateCanvas(textList[1]));
            }
            /*
            // 방 목록 처리
            if (msg.StartsWith("/list"))
            {
                var textList = msg.Split(":");
                var roomInfo = textList[1].Split("&");

                if (roomInfo[0] != "")
                {
                    var roomNameList = roomInfo[0].Split(", ");
                    var userCount = roomInfo[1].Split(", ");

                    // 맨 마지막 문자열의 공백 제거
                    userCount[userCount.Length - 1] = userCount[userCount.Length - 1].TrimEnd('\0');
                
                    SetRoomList(roomNameList, userCount);

                }

            }
            */
            else if (msg.StartsWith("Invalid"))
            {
                // 경고 메세지 출력
                
                StartCoroutine(SetAlertPanelInCreateCanvas(msg));
            }
            else
            {
                // 방 목록 처리
                ListData listData = JsonUtility.FromJson<ListData>(msg);

                SetRoomList(listData.roomName, listData.userCount);
            }


        }

    }

    public void LogoutBtn()
    {
        StartCoroutine(LogoutRequest());
    }

    // 로그아웃 처리
    IEnumerator LogoutRequest()
    {
        // 요청을 보낼 URL
        string uri = "https://localhost:7270/account/logout";

        // UnityWebRequest를 사용하여 POST 요청을 보냄
        UnityWebRequest request = new UnityWebRequest(uri, "POST");

        // 요청을 보내고 응답을 기다림
        yield return request.SendWebRequest();

        // 에러를 확인하고 처리
        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(request.error);
        }
        else
        {            
            // 로그아웃 성공시 로그인 페이지 이동
            SceneManager.LoadScene("LoginScene");
            yield break;
        }
    }


    // 2.5초간 경고창 출력후 사라짐 (Create Canvas 안의 AlertPanel)
    IEnumerator SetAlertPanelInCreateCanvas(string msg)
    {
        alertPanelInCreateCanvas.SetActive(true);
        alertPanelInCreateCanvas.GetComponentInChildren<TMP_Text>().text = msg.TrimEnd('\0');

        yield return new WaitForSecondsRealtime(2.5f);

        alertPanel.SetActive(false);

        yield break;
    }

    // 2.5초간 경고창 출력후 사라짐
    IEnumerator SetAlertPanel(string msg)
    {
        alertPanel.SetActive(true);
        alertPanel.GetComponentInChildren<TMP_Text>().text = msg.TrimEnd('\0');

        yield return new WaitForSecondsRealtime(2.5f);

        alertPanel.SetActive(false);

        yield break;
    }


    // 약 3초에 한번 /list 명령어 보냄
    IEnumerator GetRoomList()
    {
        while (true)
        {
            yield return null;
            if (tcpClient == null || !tcpClient.Connected) continue;

            var data = Encoding.UTF8.GetBytes("/list");
            tcpClient.GetStream().Write(data);

            yield return new WaitForSecondsRealtime(3.0f);
        }
    }


    void SetRoomList(string[] roomNameList, string[] userCountList)
    {
        int cnt = content.transform.childCount;

        // 현재 내 방제목이 서버에서 보내준 방제목과 같을 때 인원수만 갱신. (버튼에 달린 방제목 수정 불필요. )
        // 현재 떠있는 방 목록만 검사중
        for (int i = 0; i < cnt; i++)
        {
            // 서버와 클라의 방 목록을 대조하기 위한 변수
            bool isRoomExists = false;

            Transform roomObj = content.transform.GetChild(i);

            TMP_Text[] texts = roomObj.GetComponentsInChildren<TMP_Text>();

            for (int j = 0; j < roomNameList.Length; j++)
            {
                if (texts[0].text == roomNameList[j])
                {
                    texts[1].text = userCountList[j] + "/4";

                    // 방이 서버 방 목록에 있고 클라 방 목록에도 있으므로 true로 체크
                    isRoomExists = true;
                    // 반영한 방은 삭제하기 위해 null 대입. 
                    userCountList[j] = null;
                }
                
            }

            // 원래 있었던 방이 삭제되었을 때
            if(!isRoomExists)
            {
                Destroy(roomObj.gameObject);
            }
        }

        

        // 새로운 방이 생겼다면(=데이터가 반영이 안되었다면) 방 prefab을 새로 만들어준다. 
        for (int i = 0; i < roomNameList.Length; i++)
        {
            if (userCountList[i] != null)
            {
                // 오브젝트 생성
                var newtextobj = Instantiate(roomPrefab, Vector3.zero, Quaternion.identity);
                string roomName = roomNameList[i];

                // 텍스트 넣기
                TMP_Text[] texts = newtextobj.GetComponentsInChildren<TMP_Text>();
                texts[0].text = roomName;
                texts[1].text = userCountList[i] + "/4";

                // 버튼 리스너 달아줌
                Button enterBtn = newtextobj.GetComponentInChildren<Button>();
                enterBtn.onClick.AddListener(() =>
                {
                    RoomEnterBtn(roomName);
                });


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


    /*
    void SetRoomList(string[] roomNameList, string[] userCountList)
    {
        int cnt = content.transform.childCount;



        // 현재 내 방제목이 서버에서 보내준 방제목과 같을 때 인원수만 갱신. (버튼에 달린 방제목 수정 불필요. )
        // 만약 내 인원수가 0이라면 삭제. 
        for (int i = 0; i < cnt; i++)
        {
            Transform roomObj = content.transform.GetChild(i);

            TMP_Text[] texts = roomObj.GetComponentsInChildren<TMP_Text>();


            // 텍스트 넣기
            for (int j = 0; j < roomNameList.Length; j++)
            {
                if (texts[0].text == roomNameList[j])
                {
                    if(userCountList[j] == "0")
                    {
                        Destroy(roomObj.gameObject);
                    }
                    else
                    {
                    texts[1].text = userCountList[j] + "/4";
                    }

                    // 반영한 유저수는 삭제하기 위해 null 대입. 
                    userCountList[j] = null;
                }
            }
        }


        // 새로운 방이 생겼다면(=데이터가 반영이 안되었다면) 방 prefab을 새로 만들어준다. 
        // 새로 방 prefab을 추가해 데이터를 적용해준다. 
        for (int i = 0; i < roomNameList.Length; i++)
        {
            if (userCountList[i] != null && userCountList[i] != "0")
            {
                // 오브젝트 생성
                var newtextobj = Instantiate(roomPrefab, Vector3.zero, Quaternion.identity);
                string roomName = roomNameList[i];

                // 텍스트 넣기
                TMP_Text[] texts = newtextobj.GetComponentsInChildren<TMP_Text>();
                texts[0].text = roomName;
                texts[1].text = userCountList[i] + "/4";

                // 버튼 리스너 달아줌
                Button enterBtn = newtextobj.GetComponentInChildren<Button>();
                enterBtn.onClick.AddListener(() =>
                {
                    RoomEnterBtn(roomName);
                });


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
    */

    /// <summary>
    /// TCP client callback. TCP 연결 시도가 성공적으로 완료되었을 때 실행됨
    /// </summary>
    /// <param name="ar"></param>
    private void StartReadingTCP(System.IAsyncResult ar)
    {
        byte[] buf = new byte[512];


        // GameManager로부터 세션 정보 받아오기
        GameManager gameManager = GameManager.Instance;
        string sessionId = gameManager.SessionId;
        string username = gameManager.Username;

        // sessionId와 username 전송
        string sessionInfoMessage = $"/session,{sessionId},{username}";
        var data = Encoding.UTF8.GetBytes(sessionInfoMessage);
        tcpClient.GetStream().Write(data);



        // TCP 클라이언트의 네트워크 스트림에서 비동기적으로 데이터를 읽고,
        // 데이터를 읽은 후에는 OnTCPDataReceived 함수를 호출하여 데이터를 처리
        tcpClient.GetStream().BeginRead(buf, 0, buf.Length, OnTCPDataReceived, buf);
    }

    // 수신된 데이터가 처리되는 함수
    // TCP 통신에서 데이터를 수신할 때, 데이터를 처리하는 작업은 비동기적으로 이루어집니다.
    // 따라서 데이터가 도착하는 순서와 처리하는 순서가 일치하지 않을 수 있습니다.
    // 이를 관리하기 위해 데이터를 큐에 넣어 처리 대기열을 관리합니다.
    private void OnTCPDataReceived(System.IAsyncResult ar)
    {
        try
        {
            var byteRead = tcpClient.GetStream().EndRead(ar);

            if (byteRead > 0)
            {
                byte[] data = (byte[])ar.AsyncState;

                string msg = Encoding.UTF8.GetString(data);

                var texts = msg.Split("\0");

                //msgQueue.Enqueue(Encoding.UTF8.GetString(data));
                msgQueue.Enqueue(texts[0]);

                tcpClient.GetStream().BeginRead(data, 0, data.Length, OnTCPDataReceived, data);
            }
            else
            {
                // 연결이 끊어지면 프로그램 종료. 
                tcpClient.GetStream().Close();
                tcpClient.Close();
                tcpClient = null;

                Application.Quit();
            }
        }
        catch (SocketException e)
        {
            Debug.LogException(e);
            Application.Quit();
        }
    }

    

    // 방 입장 버튼 리스너
    public void RoomEnterBtn(string btnName)
    {
        if (tcpClient == null || !tcpClient.Connected) return;

        //  서버에 전송하기 (GetStream().Write)
        var data = Encoding.UTF8.GetBytes("/join " + btnName);
        tcpClient.GetStream().Write(data);
    }

    // 로비 상단의 방 생성 버튼, 방제목 입력 Canvas 띄워줌. 수동으로 달아줌. 
    public void CreateBtnInLobby()
    {
        createRoomCanvas.SetActive(true);
    }

    // 방 생성 패널의 방 생성 버튼, 수동으로 달아줌. 
    public void CreateBtn()
    {
        if (tcpClient == null || !tcpClient.Connected) return;


        //  서버에 전송하기 (GetStream().Write)
        string roomName = roomText.text;
        Debug.Log("btn: create " + roomName);

        var data = Encoding.UTF8.GetBytes("/create " + roomName);
        tcpClient.GetStream().Write(data);

        
    }

    // Canvas 닫는 버튼, 수동으로 달아줌. 
    public void CloseBtn()
    {
        createRoomCanvas.SetActive(false);
    }


    public void ConnectTCP()
    {
        tcpClient = new TcpClient();
        tcpClient.BeginConnect(ipAddr, port, StartReadingTCP, null);
    }


    private void OnDestroy()
    {
        // 방목록을 받아오고 세팅하는 코루틴들 종료
        StopAllCoroutines();

        if (tcpClient != null)
        {
            tcpClient.GetStream().Close();
            tcpClient.Close();
            tcpClient = null;
        }

    }
    
}
