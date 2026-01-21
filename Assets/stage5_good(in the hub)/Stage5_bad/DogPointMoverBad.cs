using System;
using System.Collections;
using UnityEngine;

public class DogPointMoverBad : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Transform dogRoot;
    [SerializeField] private Animator animator;

    [Header("Target")]
    [SerializeField] private Transform goalTarget;

    [Header("Animator State Names (FULL name)")]
    [SerializeField] private string walkBackStateFull  = "アーマチュア|walk_back";
    [SerializeField] private string lookBackStateFull  = "アーマチュア|LookBack";
    [SerializeField] private string idleDropStateFull  = "アーマチュア|Idle_Drop";
    [SerializeField] private string droopWalkStateFull = "アーマチュア|Droopwalk";

    [Header("Move")]
    [SerializeField] private float walkSpeed = 0.7f;
    [SerializeField] private float droopWalkSpeed = 0.55f;
    [SerializeField] private float arriveDistance = 0.9f;

    [Header("Yaw (body rotation)")]
    [Tooltip("体のY回転を開始Yawからこの範囲に制限（0～3推奨）")]
    [SerializeField] private float maxYawChangeDeg = 1.0f;

    [Tooltip("Yawを少しだけ動かす場合の回転速度（度/秒）")]
    [SerializeField] private float yawDegPerSec = 60f;

    [Header("Timings (show time)")]
    [SerializeField] private float lookBackSeconds = 0.35f;
    [SerializeField] private float idleDropSeconds = 3f;

    [Header("Options")]
    [SerializeField] private bool debugLog = true;

    public event Action OnFinished;
    private Coroutine _co;
    private float _startYaw;

    public void StartSequence()
    {
        if (_co != null) StopCoroutine(_co);
        _co = StartCoroutine(Sequence());
    }

    private IEnumerator Sequence()
    {
        if (debugLog) Debug.Log("[DogPointMoverBad] Sequence start");

        if (dogRoot == null || animator == null || goalTarget == null)
        {
            Debug.LogError("[DogPointMoverBad] Refs missing (dogRoot/animator/goalTarget)");
            yield break;
        }

        _startYaw = dogRoot.eulerAngles.y;

        // 1) walk_back（Goalへ歩く：途中まで）
        TryCrossFade(walkBackStateFull);
        yield return MoveToGoal_YawAlmostFixed(walkSpeed, stopNearGoal: true);

        // 2) LookBack（体Yawは回さない。アニメで見せる）
        TryCrossFade(lookBackStateFull);
        yield return new WaitForSeconds(lookBackSeconds);

        // 3) Idle_Drop（ここが“見せ場”。時間で保持）
        TryCrossFade(idleDropStateFull);
        yield return new WaitForSeconds(idleDropSeconds);

        // 4) Droopwalk（去る）
        TryCrossFade(droopWalkStateFull);
        yield return MoveToGoal_YawAlmostFixed(droopWalkSpeed, stopNearGoal: false);

        if (debugLog) Debug.Log("[DogPointMoverBad] Sequence done");
        OnFinished?.Invoke();
        _co = null;
    }

    private IEnumerator MoveToGoal_YawAlmostFixed(float speed, bool stopNearGoal)
    {
        float stopDist = stopNearGoal ? (arriveDistance * 2f) : arriveDistance;

        while (true)
        {
            Vector3 pos = dogRoot.position;
            Vector3 to = goalTarget.position - pos;
            to.y = 0f;

            float dist = to.magnitude;
            if (dist <= stopDist) break;

            Vector3 moveDir = to.normalized;

            // 体のYawは開始Yawからほぼ動かさない（必要なら少しだけ制限付きで寄せる）
            float goalYaw = Quaternion.LookRotation(moveDir, Vector3.up).eulerAngles.y;
            float deltaFromStart = Mathf.DeltaAngle(_startYaw, goalYaw);
            deltaFromStart = Mathf.Clamp(deltaFromStart, -maxYawChangeDeg, maxYawChangeDeg);
            float desiredYaw = _startYaw + deltaFromStart;

            float currentYaw = dogRoot.eulerAngles.y;
            float delta = Mathf.DeltaAngle(currentYaw, desiredYaw);
            float step = yawDegPerSec * Time.deltaTime;
            float move = Mathf.Clamp(delta, -step, step);
            dogRoot.rotation = Quaternion.Euler(0f, currentYaw + move, 0f);

            dogRoot.position = pos + moveDir * (speed * Time.deltaTime);
            yield return null;
        }
    }

    private void TryCrossFade(string fullStateName, float fade = 0.08f)
    {
        if (string.IsNullOrEmpty(fullStateName)) return;

        int layer = 0;
        if (!animator.HasState(layer, Animator.StringToHash(fullStateName)))
        {
            if (debugLog) Debug.LogWarning($"[DogPointMoverBad] State not found: {fullStateName}");
            return;
        }

        animator.CrossFade(fullStateName, fade, layer, 0f);
    }
}
