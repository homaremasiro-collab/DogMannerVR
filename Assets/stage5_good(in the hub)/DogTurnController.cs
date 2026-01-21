using UnityEngine;

public class DogTurnController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Transform dogRoot;
    [SerializeField] private Animator animator;

    [Header("Turn")]
    [SerializeField] private float rotateSpeed = 180f;
    [SerializeField] private float finishAngle = 1f;

    [Tooltip("例: アーマチュア|turn （Copy Path/Copy Nameの結果に合わせる）")]
    [SerializeField] private string turnStateName = "アーマチュア|turn";

    private bool turning;
    private Quaternion targetRot;

    public bool IsTurning => turning;

    private void Awake()
    {
        if (!dogRoot) dogRoot = transform;
        if (!animator) animator = GetComponentInChildren<Animator>(true);
    }

    public void StartTurn(Vector3 lookAtWorld)
    {
        if (dogRoot == null) return;

        Vector3 dir = lookAtWorld - dogRoot.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f) return;

        targetRot = Quaternion.LookRotation(dir.normalized);

        // ★ここで「本当にそのAnimatorにステートがあるか」を確認
        if (animator != null)
        {
            int layer = 0;
            int hash = Animator.StringToHash(turnStateName);

            bool has = animator.HasState(layer, hash);
            if (!has)
            {
                Debug.LogError(
                    $"[DogTurnController] Turn state NOT FOUND: '{turnStateName}' " +
                    $"on Animator='{animator.name}', Controller='{animator.runtimeAnimatorController?.name}'"
                );
            }
            else
            {
                animator.CrossFadeInFixedTime(turnStateName, 0.05f, layer);
            }
        }
        else
        {
            Debug.LogError("[DogTurnController] Animator is NULL");
        }

        turning = true;
    }

    private void Update()
    {
        if (!turning || dogRoot == null) return;

        dogRoot.rotation = Quaternion.RotateTowards(
            dogRoot.rotation,
            targetRot,
            rotateSpeed * Time.deltaTime
        );

        if (Quaternion.Angle(dogRoot.rotation, targetRot) <= finishAngle)
        {
            dogRoot.rotation = targetRot;
            turning = false;
        }
    }
}
