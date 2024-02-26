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

    // ��Ʈ��ũ �۾��� �ð��� ���� �ɸ� �� �ְ�, �� �۾��� �Ϸ�� ������ ����ؾ� �Ѵ�.
    // �ڷ�ƾ�� �̷��� �۾��� �񵿱������� ó���ϰ�, �۾��� �Ϸ�� ������ ��ٸ� �� �ְ� ���ش�.
    // ���� �ڷ�ƾ�� Unity���� ��Ʈ��ũ �۾��� ���� �� �۾��� ������ �� ������ ������ó�� �����Ͽ�,
    // ���� �����带 �������� �����鼭�� �ð��� ���� �ɸ��� �۾��� ������ �� �ֵ��� ���ش�.
    IEnumerator LoginRequest()
    {
        // ��û�� ���� URL
        string uri = "https://localhost:7270/account/login";


        string loginData = $"{{\"userID\":\"{username}\", \"userPW\":\"{password}\"}}";


        // UnityWebRequest�� ����Ͽ� POST ��û�� ����
        UnityWebRequest request = new UnityWebRequest(uri, "POST");

        // ��û�� JSON �����͸� �߰�
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(loginData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();

        // Content-Type ����
        request.SetRequestHeader("Content-Type", "application/json");


        // ��û�� ������ ������ ��ٸ�
        yield return request.SendWebRequest();


        // ������ Ȯ���ϰ� ó��
        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(request.error);

            // ���� �޼��� ���
            StartCoroutine(SetAlertPanel(request.error));
        }
        else
        {
            // ���� ������ �޾Ƽ� ó��
            // downloadHandler�� UnityWebRequest�� �Ӽ� �� �ϳ���, HTTP ���� �����͸� ó���Ѵ�.

            // ���� ������ ServerResponse ��ü�� �Ľ�
            ServerResponse serverResponse = JsonUtility.FromJson<ServerResponse>(request.downloadHandler.text);

            if(serverResponse.result != "����")
            {
                StartCoroutine(SetAlertPanel(serverResponse.result));

                yield break;
            }
            else
            {
                // GameManager�� ���, ���� Ű �� username ����
                GameManager gameManager = FindObjectOfType<GameManager>();

                if (gameManager != null)
                {
                    gameManager.SaveSessionInfo(serverResponse.result, serverResponse.sessionId, serverResponse.username);
                }
                // �α��� ������ �κ�� �̵�, ���� ���� üũ ���ǽ� �ʿ�
                SceneManager.LoadScene("LobbyScene");

                yield break;
            }
        }
    }

    IEnumerator SignUpRequest()
    {
        // ��û�� ���� URL
        string uri = "https://localhost:7270/account/register";


        string signupData = $"{{\"userID\":\"{username}\", \"userPW\":\"{password}\"}}";


        // UnityWebRequest�� ����Ͽ� POST ��û�� ����
        UnityWebRequest request = new UnityWebRequest(uri, "POST");

        // ��û�� JSON �����͸� �߰�
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(signupData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();

        // Content-Type ����
        request.SetRequestHeader("Content-Type", "application/json");


        // ��û�� ������ ������ ��ٸ�
        yield return request.SendWebRequest();


        // ������ Ȯ���ϰ� ó��
        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(request.error);

            // ���� �޼��� ���
            StartCoroutine(SetAlertPanel(request.error));
        }
        else
        {
            // ���� ������ �޾Ƽ� ó��
            // downloadHandler�� UnityWebRequest�� �Ӽ� �� �ϳ���, HTTP ���� �����͸� ó���Ѵ�.

            // ȸ������ �� InputField�� ����ش�. 
            UserName.GetComponent<TMP_InputField>().text = "";
            Password.GetComponent<TMP_InputField>().text = "";


            // ���� ������ ServerResponse ��ü�� �Ľ�
            ServerResponse serverResponse = JsonUtility.FromJson<ServerResponse>(request.downloadHandler.text);

            // ȸ������ ��� �г� ǥ��
            StartCoroutine(SetAlertPanel(serverResponse.result));
            
            yield break;
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

}
