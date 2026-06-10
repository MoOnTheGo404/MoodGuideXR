using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

// Drop-in replacement for the original GeminiManager.
// Public surface is UNCHANGED (public string apiKey + IEnumerator GenerateMoodMessage(string, Action<string>)),
// so the scene reference and MoodRoomTrigger keep working with no re-wiring.
public class GeminiManager : MonoBehaviour
{
    [Header("Gemini API")]
    [Tooltip("Leave EMPTY in the committed scene. Anyone testing pastes their own key here at runtime. " +
             "Note: a key in a client build is extractable - fine for a class demo, not for production.")]
    public string apiKey = "this is my api key";

    [Tooltip("Current stable model. gemini-1.5-* models are shut down and now return 404.")]
    public string model = "gemini-2.5-flash-lite";

    public IEnumerator GenerateMoodMessage(string mood, Action<string> onComplete)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            onComplete?.Invoke(GetFallbackMessage(mood));
            yield break;
        }

        string url = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={apiKey}";

        string prompt = $"Write one short supportive sentence for a user who feels {mood}. Keep it warm and under 20 words.";

        // Build the request body with JsonUtility so escaping is always correct.
        string jsonBody = JsonUtility.ToJson(new GeminiRequest(prompt));

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning($"Gemini request failed ({request.responseCode}): {request.error}\n{request.downloadHandler.text}");
                onComplete?.Invoke(GetFallbackMessage(mood));
                yield break;
            }

            string message = ExtractText(request.downloadHandler.text);

            if (string.IsNullOrWhiteSpace(message))
                message = GetFallbackMessage(mood);

            onComplete?.Invoke(message.Trim());
        }
    }

    // Robust parse via JsonUtility - whitespace/escaping/unicode safe, unlike substring scraping.
    private string ExtractText(string json)
    {
        try
        {
            GeminiResponse parsed = JsonUtility.FromJson<GeminiResponse>(json);
            if (parsed != null &&
                parsed.candidates != null && parsed.candidates.Length > 0 &&
                parsed.candidates[0].content != null &&
                parsed.candidates[0].content.parts != null &&
                parsed.candidates[0].content.parts.Length > 0)
            {
                return parsed.candidates[0].content.parts[0].text;
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning("Gemini parse failed: " + e.Message);
        }
        return "";
    }

    private string GetFallbackMessage(string mood)
    {
        switch (mood)
        {
            case "Calm": return "Take a deep breath. You are safe and doing okay.";
            case "Confused": return "It is okay to feel confused. Take one small step at a time.";
            case "Celebrate": return "Great job! You should be proud of your progress.";
            case "Focus": return "Stay focused. You have the ability to keep moving forward.";
            default: return "You are doing your best, and that matters.";
        }
    }

    // ---- Serializable shapes for JsonUtility ----
    [Serializable]
    private class GeminiRequest
    {
        public Content[] contents;
        public GeminiRequest(string text)
        {
            contents = new[] { new Content { parts = new[] { new Part { text = text } } } };
        }
    }
    [Serializable] private class GeminiResponse { public Candidate[] candidates; }
    [Serializable] private class Candidate { public Content content; }
    [Serializable] private class Content { public Part[] parts; }
    [Serializable] private class Part { public string text; }
}