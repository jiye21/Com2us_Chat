using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Net;
using System.Net.Sockets;
using System.Text;

public class LoginManager_TCP : MonoBehaviour
{
    [SerializeField]
    string m_ipAddr = "127.0.0.1";
    [SerializeField]
    int m_port = 12345;

    [SerializeField]
    InputField m_uiID;
    [SerializeField]
    InputField m_uiMsg;

    [SerializeField]
    ScrollRect m_uiView;

    [SerializeField]
    GameObject m__basePrefab;

    TcpClient m_tcpClient;
    Queue<string> m_msgQueue;

    private void Start()
    {
        
        //  메시지 큐
        m_msgQueue = new Queue<string>();
    }

    private void requestCall(System.IAsyncResult ar)
    {
        byte[] buf = new byte[512];
        m_tcpClient.GetStream().BeginRead(buf, 0, buf.Length, requestCallTCP, buf);
    }

    private void requestCallTCP(System.IAsyncResult ar)
    {
        try
        {
            var byteRead = m_tcpClient.GetStream().EndRead(ar);

            if(byteRead>0)
            {
                byte[] data = (byte[])ar.AsyncState;

                m_msgQueue.Enqueue(Encoding.UTF8.GetString(data));

                m_tcpClient.GetStream().BeginRead(data, 0, data.Length, requestCallTCP, data);
            }
            else
            {
                m_tcpClient.GetStream().Close();
                m_tcpClient.Close();
                m_tcpClient = null;
            }
        }
        catch (SocketException e) 
        {
        }
    }
    private void Update()
    {
        if (m_msgQueue.Count > 0)
        {
            var msg = m_msgQueue.Dequeue();

            //  문자열 자르기
            var textList = msg.Split(" : ");

            // 오브젝트 생성
            var newtextobj = Instantiate(m__basePrefab, Vector3.zero, Quaternion.identity);

            // 텍스트 넣기
            Text[] texts = newtextobj.GetComponentsInChildren<Text>();
            texts[0].text = textList[0];
            texts[1].text = textList[1];

            // 뷰박스에 넣고 정보 갱신
            newtextobj.transform.SetParent(m_uiView.content, false);
            LayoutRebuilder.ForceRebuildLayoutImmediate(m_uiView.content);

            //  스크롤 갱신
            var view = m_uiView.transform as RectTransform;
            if (view.rect.height < m_uiView.content.rect.height)
            {
                m_uiView.content.anchoredPosition = new Vector2(0, m_uiView.content.rect.height);
            }
        }
    }

    public void InputEnter()
    {
        //  이름이나 내용이 없으면 안보냄
        if (m_uiID.text.Length == 0 || m_uiMsg.text.Length == 0)
            return;

        if(m_tcpClient == null && m_uiID.text == "connect" )
        {
            m_tcpClient = new TcpClient();
            m_tcpClient.BeginConnect(m_ipAddr, m_port, requestCall, null);

            m_uiID.text     = "";
            m_uiMsg.text    = "";

            return;
        }

        //  서버에 전송하기
        var data = Encoding.UTF8.GetBytes(m_uiID.text + " : " + m_uiMsg.text + "\0");

        if(m_tcpClient != null && m_tcpClient.Connected)
            m_tcpClient.GetStream().Write(data);

        //  문자열 초기화
        m_uiMsg.text = "";

        //  입력박스 포커스 유지
        m_uiMsg.Select();
        m_uiMsg.ActivateInputField();
    }

    private void OnDestroy()
    {
        if (m_tcpClient != null)
            m_tcpClient.Close();
    }
}