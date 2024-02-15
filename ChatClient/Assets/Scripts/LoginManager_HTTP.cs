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

    // ��Ʈ��ũ �۾��� �ð��� ���� �ɸ� �� �ְ�, �� �۾��� �Ϸ�� ������ ����ؾ� �Ѵ�.
    // �ڷ�ƾ�� �̷��� �۾��� �񵿱������� ó���ϰ�, �۾��� �Ϸ�� ������ ��ٸ� �� �ְ� ���ش�.
    // ���� �ڷ�ƾ�� Unity���� ��Ʈ��ũ �۾��� ���� �� �۾��� ������ �� ������ ������ó�� �����Ͽ�,
    // ���� �����带 �������� �����鼭�� �ð��� ���� �ɸ��� �۾��� ������ �� �ֵ��� ���ش�.
    IEnumerator SendLoginRequest()
    {
        // ��û�� ���� URL
        string uri = "https://localhost:7270/account/login";


        //string loginData = "{\"loginId\":\"junmo\", \"Password\":\"1234\"}";
        string loginData = $"{{\"loginId\":\"{username}\", \"Password\":\"{password}\"}}";


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
        }
        else
        {
            // ���� ������ �޾Ƽ� ó��
            // downloadHandler�� UnityWebRequest�� �Ӽ� �� �ϳ���, HTTP ���� �����͸� ó���Ѵ�.
            // .text�� HTTP ������ ������ �ؽ�Ʈ �������� �����´�. 
            Debug.Log("���� ����: " + request.downloadHandler.text);
            yield break;
        }
    }

    IEnumerator SendSignUpRequest()
    {
        // ��û�� ���� URL
        string uri = "https://localhost:7270/account/register";


        //string loginData = "{\"loginId\":\"junmo\", \"Password\":\"1234\"}";
        string signupData = $"{{\"loginId\":\"{username}\", \"Password\":\"{password}\"}}";


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
        }
        else
        {
            // ���� ������ �޾Ƽ� ó��
            // downloadHandler�� UnityWebRequest�� �Ӽ� �� �ϳ���, HTTP ���� �����͸� ó���Ѵ�.
            // .text�� HTTP ������ ������ �ؽ�Ʈ �������� �����´�. 
            Debug.Log("���� ����: " + request.downloadHandler.text);
            yield break;
        }
    }
}
