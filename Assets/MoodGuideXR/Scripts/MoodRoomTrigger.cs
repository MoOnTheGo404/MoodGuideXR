using UnityEngine;
using TMPro;

public class MoodRoomTrigger : MonoBehaviour
{
    public MoodAnimationController moodController;
    public TMP_Text moodMessageText;

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
                ShowMessage("Take a deep breath. You are safe and doing okay.");
                break;

            case MoodType.Confused:
                moodController.PlayConfused();
                ShowMessage("It is okay to feel confused. Take one small step at a time.");
                break;

            case MoodType.Celebrate:
                moodController.PlayCelebrate();
                ShowMessage("Great job! You should be proud of your progress.");
                break;

            case MoodType.Focus:
                moodController.PlayFocus();
                ShowMessage("Stay focused. You have the ability to keep moving forward.");
                break;
        }

        Debug.Log("Entered mood room: " + moodType);
    }

    private void ShowMessage(string message)
    {
        if (moodMessageText != null)
        {
            moodMessageText.text = message;
        }

        Debug.Log("Mood message: " + message);
    }
}