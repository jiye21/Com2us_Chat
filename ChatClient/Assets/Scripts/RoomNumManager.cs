using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RoomNumManager : MonoBehaviour
{
    private static RoomNumManager instance;

    // LobbyManager�κ��� ���� ������ �� ��ȣ �޾ƿ�
    public string myRoomNum;

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
