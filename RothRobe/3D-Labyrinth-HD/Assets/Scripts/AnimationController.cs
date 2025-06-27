using UnityEngine;
using UnityEngine.InputSystem;

public class AnimationController : MonoBehaviour
{
    private Animator animator;
    private static readonly int CrawlTrigger = Animator.StringToHash("Crawl Trigger");
    private static readonly int WalkTrigger = Animator.StringToHash("Walk Trigger");
    private static readonly int IdleTrigger = Animator.StringToHash("Idle Trigger");

    void Start()
    {
        animator = GetComponent<Animator>();
        Debug.Log(animator);
    }

    void Update()
    {
        if (Keyboard.current.fKey.wasPressedThisFrame)
        {
            StartIdleAnimation();
        }

        if (Keyboard.current.gKey.wasPressedThisFrame)
        {
            StartWalkAnimation();
        }

        if (Keyboard.current.hKey.wasPressedThisFrame)
        {
            StartCrawlAnimation();
        }
        
    }

    void StartIdleAnimation()
    {
        Debug.Log("Idle Trigger");
        animator.SetTrigger(IdleTrigger);
    }

    void StartWalkAnimation()
    {
        Debug.Log("Walk Trigger");
        animator.SetTrigger(WalkTrigger);
    }

    void StartCrawlAnimation()
    {
        Debug.Log("Crawl Trigger");
        animator.SetTrigger(CrawlTrigger);
    }
}
