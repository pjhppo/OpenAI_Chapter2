using System.Collections;
using System.Text;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Text.RegularExpressions;

public class ContextTextGeneration : MonoBehaviour
{
    private const string apiUrl = "https://api.openai.com/v1/chat/completions";
    private const string apiKey = "";
    public string prompt = "You are a helpful assistant.";
    public string content = "Write a haiku about recursion in programming.";

    // 이전 대화 내역을 저장할 리스트
    private List<Dictionary<string, string>> messages = new List<Dictionary<string, string>>();

    void Start()
    {
        // 첫 번째 시스템 메시지 추가
        messages.Add(new Dictionary<string, string> {
            { "role", "system" },
            { "content", prompt }
        });

        // 첫 번째 사용자 메시지 추가
        AddUserMessage(content);

        // API 요청 시작
        StartCoroutine(SendOpenAIRequest());
    }

    // 사용자 메시지를 추가하는 함수
    public void AddUserMessage(string userMessage)
    {
        messages.Add(new Dictionary<string, string> {
            { "role", "user" },
            { "content", userMessage }
        });
    }

    // 도우미(assistant) 메시지를 추가하는 함수
    public void AddAssistantMessage(string assistantMessage)
    {
        messages.Add(new Dictionary<string, string> {
            { "role", "assistant" },
            { "content", assistantMessage }
        });
    }

    public IEnumerator SendOpenAIRequest()
    {
        // JSON 형식의 메시지 배열을 문자열로 변환
        StringBuilder jsonMessages = new StringBuilder();
        jsonMessages.Append("[");
        foreach (var message in messages)
        {
            jsonMessages.Append("{");
            jsonMessages.AppendFormat("\"role\":\"{0}\",", message["role"]);
            jsonMessages.AppendFormat("\"content\":\"{0}\"", message["content"]);
            jsonMessages.Append("},");
        }
        jsonMessages.Remove(jsonMessages.Length - 1, 1); // 마지막 콤마 제거
        jsonMessages.Append("]");

        // 요청에 사용할 JSON 데이터 구성
        string jsonData = @"{
            ""model"": ""gpt-4o"",
            ""messages"": " + jsonMessages.ToString() + @"
        }";

        // UTF-8 인코딩으로 JSON 데이터를 바이트 배열로 변환
        byte[] postData = Encoding.UTF8.GetBytes(jsonData);

        // UnityWebRequest를 사용하여 POST 요청을 생성
        using (UnityWebRequest request = new UnityWebRequest(apiUrl, "POST"))
        {
            // 요청 헤더 설정
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", "Bearer " + apiKey);

            // 요청 데이터 설정
            request.uploadHandler = new UploadHandlerRaw(postData);
            request.downloadHandler = new DownloadHandlerBuffer();

            // 요청 전송
            yield return request.SendWebRequest();

            // 에러 핸들링
            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Error: " + request.error);
            }
            else
            {
                // 응답 처리
                string responseText = request.downloadHandler.text;
                Debug.Log("Response: " + responseText);

                // JSON 응답에서 "content" 값을 추출하는 정규식
                string pattern = @"""content"":\s*""(.*?)""";
                Match match = Regex.Match(responseText, pattern);

                if (match.Success)
                {
                    string assistantMessage = match.Groups[1].Value;
                    Debug.Log("Assistant Message: " + assistantMessage);

                    // 받은 도우미 메시지를 대화에 추가
                    AddAssistantMessage(assistantMessage);
                }
                else
                {
                    Debug.LogWarning("No valid response found.");
                }
            }
        }
    }
}
