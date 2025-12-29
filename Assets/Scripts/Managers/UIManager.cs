using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [SerializeField] private Animator helperTextAnimator;
    [SerializeField] private KeyCode triggerKey = KeyCode.Space;

    private bool hasTriggered = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Update()
    {
        if (!hasTriggered && Input.GetKeyDown(triggerKey))
        {
            TriggerHelperTextScroll();
        }
    }

    private void TriggerHelperTextScroll()
    {
        if (helperTextAnimator != null)
        {
            helperTextAnimator.SetTrigger("ScrollDown");
            hasTriggered = true;
        }
    }
}