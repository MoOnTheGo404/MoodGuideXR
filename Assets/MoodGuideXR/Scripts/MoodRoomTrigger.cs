using UnityEngine;

public class MoodRoomTrigger : MonoBehaviour
{
    public MoodAnimationController moodController;

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
        if (!other.CompareTag("Player"))
            return;

        if (moodController == null)
        {
            Debug.LogWarning("MoodController is missing on " + gameObject.name);
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

        Debug.Log("Entered mood room: " + moodType);
    }
}