using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class LoginManager_HTTP : MonoBehaviour
{
    public GameObject UserName;
    public GameObject Password;

    public string username;
    public string password;

    void Start()
    {

    }


    void Update()
    {
        
    }

    public void LoginBtn()
    {
        username = UserName.GetComponent<TMP_InputField>().text;
        password = Password.GetComponent<TMP_InputField>().text;
        StartCoroutine(SendLoginRequest());
    }

    public void SignUpBtn()
    {
        username = UserName.GetComponent<TMP_InputField>().text;
        password = Password.GetComponent<TMP_InputField>().text;
        StartCoroutine(SendSignUpRequest());
    }

    // 네트워크 작업은 시간이 오래 걸릴 수 있고, 이 작업이 완료될 때까지 대기해야 한다.
    // 코루틴은 이러한 작업을 비동기적으로 처리하고, 작업이 완료될 때까지 기다릴 수 있게 해준다.
    // 또한 코루틴은 Unity에서 네트워크 작업과 같은 긴 작업을 실행할 때 일종의 스레드처럼 동작하여,
    // 메인 스레드를 차단하지 않으면서도 시간이 오래 걸리는 작업을 수행할 수 있도록 해준다.
    IEnumerator SendLoginRequest()
    {
        // 요청을 보낼 URL
        string uri = "https://localhost:7270/account/login";


        //string loginData = "{\"loginId\":\"junmo\", \"Password\":\"1234\"}";
        string loginData = $"{{\"loginId\":\"{username}\", \"Password\":\"{password}\"}}";


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
        }
        else
        {
            // 서버 응답을 받아서 처리
            // downloadHandler는 UnityWebRequest의 속성 중 하나로, HTTP 응답 데이터를 처리한다.
            // .text는 HTTP 응답의 본문을 텍스트 형식으로 가져온다. 
            Debug.Log("서버 응답: " + request.downloadHandler.text);
            yield break;
        }
    }

    IEnumerator SendSignUpRequest()
    {
        // 요청을 보낼 URL
        string uri = "https://localhost:7270/account/register";


        //string loginData = "{\"loginId\":\"junmo\", \"Password\":\"1234\"}";
        string signupData = $"{{\"loginId\":\"{username}\", \"Password\":\"{password}\"}}";


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
        }
        else
        {
            // 서버 응답을 받아서 처리
            // downloadHandler는 UnityWebRequest의 속성 중 하나로, HTTP 응답 데이터를 처리한다.
            // .text는 HTTP 응답의 본문을 텍스트 형식으로 가져온다. 
            Debug.Log("서버 응답: " + request.downloadHandler.text);
            yield break;
        }
    }
}
