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

        // �� ��� �������� �ڷ�ƾ ����. 3�ʸ��� ������. 
        StartCoroutine("GetRoomList");
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
                GameObject.Find("GameManager").GetComponent<GameManager>().myRoomNum = textList[1];
                SceneManager.LoadScene("ChatScene");
            }

            // �� ����� ������
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

        // ���� �� �������� �������� ������ ������� ���� �� �ο����� ����. 
        for (int i = 0 ;  i < cnt; i++)
        {
            Transform roomObj = content.transform.GetChild(i);

            TMP_Text[] texts = roomObj.GetComponentsInChildren<TMP_Text>();

            // �ؽ�Ʈ �ֱ�
            for(int j = 0; j<roomList.Length; j++)
            {
                if(texts[0].text == roomList[j])
                {
                    texts[1].text = userCount[j] + "/10";

                    // �ݿ��� �������� �����ϱ� ���� -1 ����. 
                    userCount[j] = "-1";
                }
            }
        }

        // ���ο� ���� ����ٸ�(�����Ͱ� �ݿ��� �ȵ�) �� prefab�� ���� ������ش�. 
        // ���� �� prefab�� �߰��� �����͸� �������ش�. 
        for (int j = 0; j < roomList.Length; j++)
        {
            if (userCount[j] != "-1")
            {
                var newRoomList = Instantiate(roomPrefab, Vector3.zero, Quaternion.identity);

                TMP_Text[] texts = newRoomList.GetComponentsInChildren<TMP_Text>();

                texts[0].text = roomList[j];
                texts[1].text = userCount[j] + "/10";

                // ��ڽ��� �ְ� ���� ����
                newRoomList.transform.SetParent(uiView.content, false);
                LayoutRebuilder.ForceRebuildLayoutImmediate(uiView.content);

                //  ��ũ�� ����
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


    public void SetBtn()
    {
        // �� ���� ��ư init
        // ��ư�� onClick ������ ���, �� ��ȣ�� 0������ ���ʷ� ���� 
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


    // �� ���� ��ư ������
    public void EnterBtn(int btnNum)
    {
        Debug.Log("btnNum: " + btnNum);
        if (tcpClient == null || !tcpClient.Connected) return;

        //  ������ �����ϱ� (GetStream().Write)
        var data = Encoding.UTF8.GetBytes("/join " + btnNum.ToString());
        tcpClient.GetStream().Write(data);

    }

    // �κ��� �� ���� ��ư, ������ �Է� Canvas �����. �������� �޾���. 
    public void CreateBtnInLobby()
    {
        createRoomCanvas.SetActive(true);


    }

    // �� ���� ��ư, �������� �޾���. 
    public void CreateBtn()
    {
        if (tcpClient == null || !tcpClient.Connected) return;


        //  ������ �����ϱ� (GetStream().Write)
        roomName = roomText.text;
        Debug.Log("btn: create" + roomName);

        var data = Encoding.UTF8.GetBytes("/create " + roomName);
        tcpClient.GetStream().Write(data);

        // Canvas ����
        createRoomCanvas.SetActive(false);
    }

    // Canvas �ݴ� ��ư, �������� �޾���. 
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

        // ������ �޾ƿ��� �����ϴ� �ڷ�ƾ�� ����
        StopAllCoroutines();
    }
    
}
