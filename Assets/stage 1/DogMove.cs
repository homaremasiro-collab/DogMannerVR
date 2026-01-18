using UnityEngine;

public class DogMoveOnWalk : MonoBehaviour
{
    public Animator animator;
    public string walkStateName = "アーマチュア|walk_front";
    public float moveSpeed = 0.8f;

    void Reset()
    {
        animator = GetComponentInChildren<Animator>();
    }

    void Awake()
    {
        if (animator != null) animator.applyRootMotion = false;
    }

    void Update()
    {
        if (animator == null) return;

        var st = animator.GetCurrentAnimatorStateInfo(0);

        if (st.IsName(walkStateName))
        {
            // DogMoverの向いている方向へ前進
            transform.position += transform.forward * moveSpeed * Time.deltaTime;
        }
    }
}
