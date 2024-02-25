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

    [SerializeField]
    TMP_InputField roomText;

    [SerializeField]
    GameObject createRoomCanvas;

    [SerializeField]
    GameObject roomPrefab;

    [SerializeField]
    ScrollRect uiView;

    GameObject alertPanel;

    void Start()
    {
        msgQueue = new Queue<string>();

        ConnectTCP();


        // �� ��� �������� �ڷ�ƾ ����. 3�ʸ��� ������. 
        StartCoroutine("GetRoomList");
    }


    void Update()
    {
        // SceneManager.LoadScene �۾��� ���� �����忡���� �ؾ� �ؼ� ���� �����͸� ť�� ��� ���� �����忡�� ó��
        if (msgQueue.Count > 0)
        {
            var msg = msgQueue.Dequeue();

            Debug.Log("LoginScene : Received from Server  " + msg);

            if (msg.StartsWith("200"))
            {
                var textList = msg.Split(":");

                // �� ������ ���ڿ��� ���� ���� �� GameManager�� �� ��ȣ�� �������ش�. 
                GameObject.Find("GameManager").GetComponent<GameManager>().myRoomName = textList[1].TrimEnd('\0');
                SceneManager.LoadScene("ChatScene");
            }

            // �� ��� ó��
            if (msg.StartsWith("/list"))
            {
                var textList = msg.Split(":");
                var roomInfo = textList[1].Split("&");

                var roomNameList = roomInfo[0].Split(", ");
                var userCount = roomInfo[1].Split(", ");

                // �� ������ ���ڿ��� ���� ����
                userCount[userCount.Length - 1] = userCount[userCount.Length - 1].TrimEnd('\0');


                SetRoomList(roomNameList, userCount);
            }

            if(msg.StartsWith("Invalid"))
            {
                // ��� �޼��� ���
                alertPanel = createRoomCanvas.transform.GetChild(5).gameObject;
                StartCoroutine(SetAlertPanel(msg));
            }

        }

    }

    // 2.5�ʰ� ���â ����� �����
    IEnumerator SetAlertPanel(string msg)
    {
        alertPanel.SetActive(true);
        alertPanel.GetComponentInChildren<TMP_Text>().text = msg.TrimEnd('\0');

        yield return new WaitForSecondsRealtime(2.5f);

        alertPanel.SetActive(false);

        yield break;
    }


    // �� 3�ʿ� �ѹ� /list ��ɾ� ����
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

        // ���� �� �������� �������� ������ ������� ���� �� �ο����� ����. (��ư�� �޸� ������ ���� ���ʿ�. )
        for (int i = 0; i < cnt; i++)
        {
            Transform roomObj = content.transform.GetChild(i);

            TMP_Text[] texts = roomObj.GetComponentsInChildren<TMP_Text>();

            // �ؽ�Ʈ �ֱ�
            for (int j = 0; j < roomNameList.Length; j++)
            {
                if (texts[0].text == roomNameList[j])
                {
                    texts[1].text = userCountList[j] + "/10";

                    // �ݿ��� �������� �����ϱ� ���� null ����. 
                    userCountList[j] = null;
                }
            }
        }


        // ���ο� ���� ����ٸ�(=�����Ͱ� �ݿ��� �ȵǾ��ٸ�) �� prefab�� ���� ������ش�. 
        // ���� �� prefab�� �߰��� �����͸� �������ش�. 
        for (int i = 0; i < roomNameList.Length; i++)
        {
            if (userCountList[i] != null)
            {
                // ������Ʈ ����
                var newtextobj = Instantiate(roomPrefab, Vector3.zero, Quaternion.identity);
                string roomName = roomNameList[i];

                // �ؽ�Ʈ �ֱ�
                TMP_Text[] texts = newtextobj.GetComponentsInChildren<TMP_Text>();
                texts[0].text = roomName;
                texts[1].text = userCountList[i] + "/10";

                // ��ư ������ �޾���
                Button enterBtn = newtextobj.GetComponentInChildren<Button>();
                enterBtn.onClick.AddListener(() =>
                {
                    RoomEnterBtn(roomName);
                });


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

    /// <summary>
    /// TCP client callback. TCP ���� �õ��� ���������� �Ϸ�Ǿ��� �� �����
    /// </summary>
    /// <param name="ar"></param>
    private void StartReadingTCP(System.IAsyncResult ar)
    {
        byte[] buf = new byte[512];


        // GameManager�κ��� ���� ���� �޾ƿ���
        GameManager gameManager = GameManager.Instance;
        string sessionId = gameManager.SessionId;
        string username = gameManager.Username;

        // ���߿� ���� ID �޾ƿͼ� ����ϰ� �ٲٱ�
        string sessionInfoMessage = $"/session,{sessionId},{username}";
        var data = Encoding.UTF8.GetBytes(sessionInfoMessage);
        tcpClient.GetStream().Write(data);



        // TCP Ŭ���̾�Ʈ�� ��Ʈ��ũ ��Ʈ������ �񵿱������� �����͸� �а�,
        // �����͸� ���� �Ŀ��� OnTCPDataReceived �Լ��� ȣ���Ͽ� �����͸� ó��
        tcpClient.GetStream().BeginRead(buf, 0, buf.Length, OnTCPDataReceived, buf);
    }

    // ���ŵ� �����Ͱ� ó���Ǵ� �Լ�
    // TCP ��ſ��� �����͸� ������ ��, �����͸� ó���ϴ� �۾��� �񵿱������� �̷�����ϴ�.
    // ���� �����Ͱ� �����ϴ� ������ ó���ϴ� ������ ��ġ���� ���� �� �ֽ��ϴ�.
    // �̸� �����ϱ� ���� �����͸� ť�� �־� ó�� ��⿭�� �����մϴ�.
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
                // ������ �������� ���α׷� ����. 
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

    

    // �� ���� ��ư ������
    public void RoomEnterBtn(string btnName)
    {
        Debug.Log("Clicked Button: " + btnName);
        if (tcpClient == null || !tcpClient.Connected) return;

        //  ������ �����ϱ� (GetStream().Write)
        var data = Encoding.UTF8.GetBytes("/join " + btnName);
        tcpClient.GetStream().Write(data);
    }

    // �κ� ����� �� ���� ��ư, ������ �Է� Canvas �����. �������� �޾���. 
    public void CreateBtnInLobby()
    {
        createRoomCanvas.SetActive(true);
    }

    // �� ���� �г��� �� ���� ��ư, �������� �޾���. 
    public void CreateBtn()
    {
        if (tcpClient == null || !tcpClient.Connected) return;


        //  ������ �����ϱ� (GetStream().Write)
        string roomName = roomText.text;
        Debug.Log("btn: create " + roomName);

        var data = Encoding.UTF8.GetBytes("/create " + roomName);
        tcpClient.GetStream().Write(data);

        
    }

    // Canvas �ݴ� ��ư, �������� �޾���. 
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
        // ������ �޾ƿ��� �����ϴ� �ڷ�ƾ�� ����
        StopAllCoroutines();

        if (tcpClient != null)
        {
            tcpClient.GetStream().Close();
            tcpClient.Close();
            tcpClient = null;
        }

    }
    
}
