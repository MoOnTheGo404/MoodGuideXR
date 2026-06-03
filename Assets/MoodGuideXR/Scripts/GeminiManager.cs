using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class GeminiManager : MonoBehaviour
{
    [Header("Gemini API")]
    public string apiKey = "";

    public IEnumerator GenerateMoodMessage(string mood, Action<string> onComplete)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            onComplete?.Invoke(GetFallbackMessage(mood));
            yield break;
        }

        string url = "https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key=" + apiKey;

        string prompt = $"Write one short supportive sentence for a user who feels {mood}. Keep it warm and under 20 words.";

        string jsonBody =
            "{\"contents\":[{\"parts\":[{\"text\":\"" +
            EscapeJson(prompt) +
            "\"}]}]}";

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning("Gemini request failed: " + request.error);
                onComplete?.Invoke(GetFallbackMessage(mood));
                yield break;
            }

            string response = request.downloadHandler.text;
            string message = ExtractText(response);

            if (string.IsNullOrWhiteSpace(message))
                message = GetFallbackMessage(mood);

            onComplete?.Invoke(message);
        }
    }

    private string GetFallbackMessage(string mood)
    {
        switch (mood)
        {
            case "Calm":
                return "Take a deep breath. You are safe and doing okay.";
            case "Confused":
                return "It is okay to feel confused. Take one small step at a time.";
            case "Celebrate":
                return "Great job! You should be proud of your progress.";
            case "Focus":
                return "Stay focused. You have the ability to keep moving forward.";
            default:
                return "You are doing your best, and that matters.";
        }
    }

    private string ExtractText(string json)
    {
        string marker = "\"text\": \"";
        int start = json.IndexOf(marker);
        if (start < 0) return "";

        start += marker.Length;
        int end = json.IndexOf("\"", start);
        if (end < 0) return "";

        return json.Substring(start, end - start)
            .Replace("\\n", " ")
            .Replace("\\\"", "\"");
    }

    private string EscapeJson(string text)
    {
        return text.Replace("\\", "\\\\").Replace("\"", "\\\"");
    }
}