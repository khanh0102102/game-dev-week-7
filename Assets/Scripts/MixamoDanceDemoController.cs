using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Animator))]
public sealed class MixamoDanceDemoController : MonoBehaviour
{
    private static readonly int IsDancingHash = Animator.StringToHash("IsDancing");
    private static readonly int DanceStateHash = Animator.StringToHash("Base Layer.Swing Dancing");

    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        if (animator == null || animator.runtimeAnimatorController == null)
        {
            Debug.LogError(
                "Mixamo dance cannot start: Animator or Runtime Animator Controller is missing. " +
                "Run Tools > Week 7 > Setup Mixamo X Bot Dance again.");
            enabled = false;
            return;
        }

        animator.enabled = true;
        animator.speed = 1f;
        animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
        animator.applyRootMotion = false;
        animator.SetBool(IsDancingHash, true);

        // Play the state directly instead of waiting for the Idle transition to fire.
        animator.Play(DanceStateHash, 0, 0f);

        Debug.Log(
            "Mixamo dance started. Controller: " + animator.runtimeAnimatorController.name +
            "; Avatar: " + (animator.avatar != null ? animator.avatar.name : "MISSING") +
            ". Press Space to toggle Idle/Dance.");
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null && keyboard.spaceKey.wasPressedThisFrame)
            animator.SetBool(IsDancingHash, !animator.GetBool(IsDancingHash));
    }
}
