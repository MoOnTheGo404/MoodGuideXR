using UnityEngine;
using UnityEngine.InputSystem;

public class MoodKeyboardTester : MonoBehaviour
{
    public MoodAnimationController moodController;

    private void Update()
    {
        if (Keyboard.current == null || moodController == null)
            return;

        if (Keyboard.current.digit1Key.wasPressedThisFrame)
            moodController.PlayCalm();

        if (Keyboard.current.digit2Key.wasPressedThisFrame)
            moodController.PlayConfused();

        if (Keyboard.current.digit3Key.wasPressedThisFrame)
            moodController.PlayCelebrate();

        if (Keyboard.current.digit4Key.wasPressedThisFrame)
            moodController.PlayFocus();
    }
}