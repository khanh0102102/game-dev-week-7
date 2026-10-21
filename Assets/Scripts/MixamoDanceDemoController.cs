using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Animator))]
public sealed class MixamoDanceDemoController : MonoBehaviour
{
    private static readonly int IsDancingHash = Animator.StringToHash("IsDancing");
    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        animator.SetBool(IsDancingHash, true);
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null && keyboard.spaceKey.wasPressedThisFrame)
            animator.SetBool(IsDancingHash, !animator.GetBool(IsDancingHash));
    }
}
