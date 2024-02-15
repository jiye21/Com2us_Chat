using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Net;
using System.Net.Sockets;
using System.Text;

public class LoginManager : MonoBehaviour
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

    UdpClient m_udpClient;
    Queue<string> m_msgQueue;

    private void Start()
    {
        //  udp Ŭ���̾�Ʈ ����
        m_udpClient = new UdpClient();
        m_udpClient.Connect(m_ipAddr, m_port);

        //  udp ���Ŵ��
        m_udpClient.BeginReceive(requestCall, null);

        //  �޽��� ť
        m_msgQueue = new Queue<string>();
    }

    //  udp ������ ���ź�
    private void requestCall(System.IAsyncResult ar)
    {
        try
        {
            IPEndPoint ipEndPoint = null;
            byte[] data = m_udpClient.EndReceive(ar, ref ipEndPoint);
            m_msgQueue.Enqueue(Encoding.UTF8.GetString(data));
        }
        catch (SocketException e) { }

        m_udpClient.BeginReceive(requestCall, null);
    }

    private void Update()
    {
        if (m_msgQueue.Count > 0)
        {
            var msg = m_msgQueue.Dequeue();

            //  ���ڿ� �ڸ���
            var textList = msg.Split(" : ");

            // ������Ʈ ����
            var newtextobj = Instantiate(m__basePrefab, Vector3.zero, Quaternion.identity);

            // �ؽ�Ʈ �ֱ�
            Text[] texts = newtextobj.GetComponentsInChildren<Text>();
            texts[0].text = textList[0];
            texts[1].text = textList[1];

            // ��ڽ��� �ְ� ���� ����
            newtextobj.transform.SetParent(m_uiView.content, false);
            LayoutRebuilder.ForceRebuildLayoutImmediate(m_uiView.content);

            //  ��ũ�� ����
            var view = m_uiView.transform as RectTransform;
            if (view.rect.height < m_uiView.content.rect.height)
            {
                m_uiView.content.anchoredPosition = new Vector2(0, m_uiView.content.rect.height);
            }
        }
    }

    public void InputEnter()
    {
        //  �̸��̳� ������ ������ �Ⱥ���
        if (m_uiID.text.Length == 0 || m_uiMsg.text.Length == 0)
            return;

        //  udp�� ������ �����ϱ�
        var data = Encoding.UTF8.GetBytes(m_uiID.text + " : " + m_uiMsg.text);
        m_udpClient.Send(data, data.Length);

        //  ���ڿ� �ʱ�ȭ
        m_uiMsg.text = "";

        //  �Է¹ڽ� ��Ŀ�� ����
        m_uiMsg.Select();
        m_uiMsg.ActivateInputField();
    }

    private void OnDestroy()
    {
        if (m_udpClient != null)
            m_udpClient.Close();
    }
}
