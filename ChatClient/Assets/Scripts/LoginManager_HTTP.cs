using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoginManager_HTTP : MonoBehaviour
{
    public class ServerResponse
    {
        public string result;
        public string sessionId;
        public string username;
    }

    public GameObject UserName;
    public GameObject Password;

    public string username;
    public string password;

    public GameObject alertPanel;


    void Start()
    {

    }


    void Update()
    {
        
    }

    public void ExitBtn()
    {
        Application.Quit();
    }

    public void LoginBtn()
    {
        username = UserName.GetComponent<TMP_InputField>().text;
        password = Password.GetComponent<TMP_InputField>().text;
        StartCoroutine(LoginRequest());
    }

    public void SignUpBtn()
    {
        username = UserName.GetComponent<TMP_InputField>().text;
        password = Password.GetComponent<TMP_InputField>().text;
        StartCoroutine(SignUpRequest());
    }

    // 네트워크 작업은 시간이 오래 걸릴 수 있고, 이 작업이 완료될 때까지 대기해야 한다.
    // 코루틴은 이러한 작업을 비동기적으로 처리하고, 작업이 완료될 때까지 기다릴 수 있게 해준다.
    // 또한 코루틴은 Unity에서 네트워크 작업과 같은 긴 작업을 실행할 때 일종의 스레드처럼 동작하여,
    // 메인 스레드를 차단하지 않으면서도 시간이 오래 걸리는 작업을 수행할 수 있도록 해준다.
    IEnumerator LoginRequest()
    {
        // 요청을 보낼 URL
        string uri = "https://localhost:7270/account/login";


        string loginData = $"{{\"userID\":\"{username}\", \"userPW\":\"{password}\"}}";


        // UnityWebRequest를 사용하여 POST 요청을 보냄
        UnityWebRequest request = new UnityWebRequest(uri, "POST");

        // 요청에 JSON 데이터를 추가
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(loginData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();

        // Content-Type 설정
        request.SetRequestHeader("Content-Type", "application/json");


        // 요청을 보내고 응답을 기다림
        yield return request.SendWebRequest();


        // 에러를 확인하고 처리
        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(request.error);

            // 에러 메세지 출력
            StartCoroutine(SetAlertPanel(request.error));
        }
        else
        {
            // 서버 응답을 받아서 처리
            // downloadHandler는 UnityWebRequest의 속성 중 하나로, HTTP 응답 데이터를 처리한다.

            // 서버 응답을 ServerResponse 객체로 파싱
            ServerResponse serverResponse = JsonUtility.FromJson<ServerResponse>(request.downloadHandler.text);

            if(serverResponse.result != "성공")
            {
                StartCoroutine(SetAlertPanel(serverResponse.result));

                yield break;
            }
            else
            {
                // GameManager에 결과, 세션 키 및 username 저장
                GameManager gameManager = FindObjectOfType<GameManager>();

                if (gameManager != null)
                {
                    gameManager.SaveSessionInfo(serverResponse.result, serverResponse.sessionId, serverResponse.username);
                }
                // 로그인 성공시 로비로 이동, 추후 세션 체크 조건식 필요
                SceneManager.LoadScene("LobbyScene");

                yield break;
            }
        }
    }

    IEnumerator SignUpRequest()
    {
        // 요청을 보낼 URL
        string uri = "https://localhost:7270/account/register";


        string signupData = $"{{\"userID\":\"{username}\", \"userPW\":\"{password}\"}}";


        // UnityWebRequest를 사용하여 POST 요청을 보냄
        UnityWebRequest request = new UnityWebRequest(uri, "POST");

        // 요청에 JSON 데이터를 추가
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(signupData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();

        // Content-Type 설정
        request.SetRequestHeader("Content-Type", "application/json");


        // 요청을 보내고 응답을 기다림
        yield return request.SendWebRequest();


        // 에러를 확인하고 처리
        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(request.error);

            // 에러 메세지 출력
            StartCoroutine(SetAlertPanel(request.error));
        }
        else
        {
            // 서버 응답을 받아서 처리
            // downloadHandler는 UnityWebRequest의 속성 중 하나로, HTTP 응답 데이터를 처리한다.

            // 회원가입 후 InputField를 비워준다. 
            UserName.GetComponent<TMP_InputField>().text = "";
            Password.GetComponent<TMP_InputField>().text = "";


            // 서버 응답을 ServerResponse 객체로 파싱
            ServerResponse serverResponse = JsonUtility.FromJson<ServerResponse>(request.downloadHandler.text);

            // 회원가입 결과 패널 표시
            StartCoroutine(SetAlertPanel(serverResponse.result));
            
            yield break;
        }
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

}
