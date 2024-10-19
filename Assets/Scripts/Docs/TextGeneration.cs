using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using System.Text.RegularExpressions;

public class TextGeneration : MonoBehaviour
{
    private const string apiUrl = "https://api.openai.com/v1/chat/completions";
    private const string apiKey = "";
    public string prompt = "You are a helpful assistant.";
    public string content = "Write a haiku about recursion in programming.";

    // OpenAI API에 요청을 보내는 코루틴 함수

    void Start(){
        StartCoroutine(SendOpenAIRequest());
    }
    public IEnumerator SendOpenAIRequest()
    {
        // JSON 형식의 데이터를 생성
        string jsonData = @"{
            ""model"": ""gpt-4o"",
            ""messages"": [
                {
                    ""role"": ""system"",
                    ""content"": """ + prompt + @"""
                },
                {
                    ""role"": ""user"",
                    ""content"": """ + content + @"""
                }
            ]
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
                }
                else
                {
                    Debug.LogWarning("No valid response found.");
                }
            }
        }
    }
}
