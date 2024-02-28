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

    // LobbyManager로부터 내가 접속할 방 제목 받아옴
    public string myRoomName;

    // 세션 키 및 결과를 저장할 변수
    private string sessionId;
    private string result;
    private string username;

    // 싱글톤 인스턴스에 접근
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


        // 초기에 게임 해상도 고정
        SetResolution();
    }

    void Update()
    {

    }

    // 세션 키 및 결과를 저장하는 메서드
    public void SaveSessionInfo(string newResult, string NewSessionId, string newUsername)
    {
        sessionId = NewSessionId;
        result = newResult;
        username = newUsername;

        // 세션 키가 잘 전달되었다면 디버그 로그 출력
        Debug.Log($"GameManager : 세션 키를 정상적으로 전달받았습니다. Session ID: {sessionId}, Username: {username}");
    }

    public string SessionId
    {
        get { return sessionId; }
    }

    public string Username
    {
        get { return username; }
    }




    /* 해상도 설정하는 함수 */
    public void SetResolution()
    {
        int setWidth = 800; // 사용자 설정 너비
        int setHeight = 600; // 사용자 설정 높이

        int deviceWidth = Screen.width; // 기기 너비 저장
        int deviceHeight = Screen.height; // 기기 높이 저장

        Screen.SetResolution(setWidth, (int)(((float)deviceHeight / deviceWidth) * setWidth), false); // SetResolution 함수 제대로 사용하기

        if ((float)setWidth / setHeight < (float)deviceWidth / deviceHeight) // 기기의 해상도 비가 더 큰 경우
        {
            float newWidth = ((float)setWidth / setHeight) / ((float)deviceWidth / deviceHeight); // 새로운 너비
            Camera.main.rect = new Rect((1f - newWidth) / 2f, 0f, newWidth, 1f); // 새로운 Rect 적용
        }
        else // 게임의 해상도 비가 더 큰 경우
        {
            float newHeight = ((float)deviceWidth / deviceHeight) / ((float)setWidth / setHeight); // 새로운 높이
            Camera.main.rect = new Rect(0f, (1f - newHeight) / 2f, 1f, newHeight); // 새로운 Rect 적용
        }
    }
    
}
