using UnityEngine;

public class MoodAnimationController : MonoBehaviour
{
    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void PlayCalm()
    {
        animator.SetTrigger("Calm");
    }

    public void PlayConfused()
    {
        animator.SetTrigger("Confused");
    }

    public void PlayCelebrate()
    {
        animator.SetTrigger("Celebrate");
    }

    public void PlayFocus()
    {
        animator.SetTrigger("Focus");
    }
}