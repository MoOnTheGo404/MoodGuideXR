using UnityEngine;
using TMPro;

public class MoodRoomTrigger : MonoBehaviour
{
    public MoodAnimationController moodController;
    public TMP_Text moodMessageText;
    public GeminiManager geminiManager;

    public enum MoodType
    {
        Calm,
        Confused,
        Celebrate,
        Focus
    }

    public MoodType moodType;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Something entered trigger: " + other.name + " | Tag: " + other.tag);

        if (!other.CompareTag("Player"))
            return;

        if (moodController == null)
        {
            Debug.LogWarning("MoodController missing on " + gameObject.name);
            return;
        }

        switch (moodType)
        {
            case MoodType.Calm:
                moodController.PlayCalm();
                break;

            case MoodType.Confused:
                moodController.PlayConfused();
                break;

            case MoodType.Celebrate:
                moodController.PlayCelebrate();
                break;

            case MoodType.Focus:
                moodController.PlayFocus();
                break;
        }

        if (geminiManager != null)
        {
            StartCoroutine(geminiManager.GenerateMoodMessage(moodType.ToString(), ShowMessage));
        }
        else
        {
            ShowMessage(GetFallbackMessage(moodType));
        }

        Debug.Log("Entered mood room: " + moodType);
    }

    private void ShowMessage(string message)
    {
        if (moodMessageText != null)
        {
            moodMessageText.text = message;
        }
        else
        {
            Debug.LogWarning("Mood Message Text is not assigned on " + gameObject.name);
        }

        Debug.Log("Mood message: " + message);
    }

    private string GetFallbackMessage(MoodType mood)
    {
        switch (mood)
        {
            case MoodType.Calm:
                return "Take a deep breath. You are safe and doing okay.";
            case MoodType.Confused:
                return "It is okay to feel confused. Take one small step at a time.";
            case MoodType.Celebrate:
                return "Great job! You should be proud of your progress.";
            case MoodType.Focus:
                return "Stay focused. You have the ability to keep moving forward.";
            default:
                return "You are doing your best, and that matters.";
        }
    }
}