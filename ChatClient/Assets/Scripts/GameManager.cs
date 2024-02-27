using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;

    // LobbyManager�κ��� ���� ������ �� ���� �޾ƿ�
    public string myRoomName;

    // ���� Ű �� ����� ������ ����
    private string sessionId;
    private string result;
    private string username;

    // �̱��� �ν��Ͻ��� ����
    public static GameManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<GameManager>();
                if (instance != null)
                {
                    GameObject obj = new GameObject("GameManager");
                    instance = obj.AddComponent<GameManager>();
                }
            }
            return instance;
        }
    }

    void Start()
    {
        // singleton
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }


        // �ʱ⿡ ���� �ػ� ����
        SetResolution();
    }

    void Update()
    {

    }

    // ���� Ű �� ����� �����ϴ� �޼���
    public void SaveSessionInfo(string newResult, string NewSessionId, string newUsername)
    {
        sessionId = NewSessionId;
        result = newResult;
        username = newUsername;

        // ���� Ű�� �� ���޵Ǿ��ٸ� ����� �α� ���
        Debug.Log($"GameManager : ���� Ű�� ���������� ���޹޾ҽ��ϴ�. Session ID: {sessionId}, Username: {username}");
    }

    public string SessionId
    {
        get { return sessionId; }
    }

    public string Username
    {
        get { return username; }
    }




    /* �ػ� �����ϴ� �Լ� */
    public void SetResolution()
    {
        int setWidth = 800; // ����� ���� �ʺ�
        int setHeight = 600; // ����� ���� ����

        int deviceWidth = Screen.width; // ��� �ʺ� ����
        int deviceHeight = Screen.height; // ��� ���� ����

        Screen.SetResolution(setWidth, (int)(((float)deviceHeight / deviceWidth) * setWidth), false); // SetResolution �Լ� ����� ����ϱ�

        if ((float)setWidth / setHeight < (float)deviceWidth / deviceHeight) // ����� �ػ� �� �� ū ���
        {
            float newWidth = ((float)setWidth / setHeight) / ((float)deviceWidth / deviceHeight); // ���ο� �ʺ�
            Camera.main.rect = new Rect((1f - newWidth) / 2f, 0f, newWidth, 1f); // ���ο� Rect ����
        }
        else // ������ �ػ� �� �� ū ���
        {
            float newHeight = ((float)deviceWidth / deviceHeight) / ((float)setWidth / setHeight); // ���ο� ����
            Camera.main.rect = new Rect(0f, (1f - newHeight) / 2f, 1f, newHeight); // ���ο� Rect ����
        }
    }
    
}
