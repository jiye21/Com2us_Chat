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

    public string sessionKey;

    void Start()
    {
        // singleton
        if (instance != null) DestroyImmediate(gameObject);

        instance = this;
        DontDestroyOnLoad(gameObject);
    }


    void Update()
    {

    }
}
