using System;
using System.Collections;
using UnityEngine;

public class DogPointMoverNormal : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Transform dogRoot;      // 犬全体の親Transform（DogMoverなど）
    [SerializeField] private Animator animator;      // 犬のAnimator

    [Header("Targets")]
    [SerializeField] private Transform walkTargetA;
    [SerializeField] private Transform goalTarget;
    [SerializeField] private Transform cameraTarget;

    [Header("State Names (FULL name)")]
    [SerializeField] private string walkBackStateFull = "アーマチュア|walk_back";
    [SerializeField] private string turnStateFull     = "アーマチュア|turn";
    [SerializeField] private string faceStateFull     = "アーマチュア|FacePlayer";
    [SerializeField] private string tailStateFull     = "アーマチュア|TailBigWag";
    [SerializeField] private string barkStateFull     = "アーマチュア|singlebark";

    [Header("Move")]
    [SerializeField] private float walkSpeed = 0.7f;
    [SerializeField] private float arriveDistance = 0.9f;

    [Header("Turn")]
    [Tooltip("回転速度（度/秒）。360で1秒に1回転。")]
    [SerializeField] private float rotateDegPerSec = 540f;

    [Tooltip("ここ以下の角度になったら回転完了扱い")]
    [SerializeField] private float finishAngle = 1.5f;

    [Tooltip("回転が一瞬で終わらないように最低回転時間を入れる（見せたい場合）")]
    [SerializeField] private float minTurnSeconds = 0.15f;

    public enum TurnDirection
    {
        Shortest,               // 最短
        AlwaysClockwise,        // 常に右回り
        AlwaysCounterClockwise  // 常に左回り
    }
    [SerializeField] private TurnDirection turnDirection = TurnDirection.Shortest;

    [Header("Timings")]
    [SerializeField] private float faceSeconds = 0.5f;
    [SerializeField] private float tailSeconds = 2f;
    [SerializeField] private float barkSeconds = 0.5f;

    [Header("Options")]
    [SerializeField] private bool debugLog = true;

    public event Action OnFinished;

    private Coroutine _co;

    // Director から呼ぶ用
    public void StartSequence()
    {
        if (_co != null) StopCoroutine(_co);
        _co = StartCoroutine(Sequence());
    }

    private IEnumerator Sequence()
    {
        if (debugLog) Debug.Log("[DogPointMoverNormal] Sequence start");

        if (dogRoot == null)
        {
            Debug.LogError("[DogPointMoverNormal] dogRoot is null（DogMoverなど犬の親Transformを入れてください）");
            yield break;
        }
        if (animator == null)
        {
            Debug.LogError("[DogPointMoverNormal] animator is null（犬のAnimatorを入れてください）");
            yield break;
        }
        if (walkTargetA == null || goalTarget == null || cameraTarget == null)
        {
            Debug.LogError("[DogPointMoverNormal] targets are missing（WalkTargetA / Goal / CameraTarget を入れてください）");
            yield break;
        }

        // 1) Walk to A
        TryCrossFade(walkBackStateFull);
        yield return MoveTo(walkTargetA.position);

        // 2) Turn to camera
        yield return TurnTo(cameraTarget.position);

        // 3) Face
        TryCrossFade(faceStateFull);
        yield return new WaitForSeconds(faceSeconds);

        // 4) Tail
        TryCrossFade(tailStateFull);
        yield return new WaitForSeconds(tailSeconds);

        // 5) Bark
        TryCrossFade(barkStateFull);
        yield return new WaitForSeconds(barkSeconds);

        // 6) Turn to Goal
        yield return TurnTo(goalTarget.position);

        // 7) Walk to Goal
        TryCrossFade(walkBackStateFull);
        yield return MoveTo(goalTarget.position);

        if (debugLog) Debug.Log("[DogPointMoverNormal] Sequence done");
        OnFinished?.Invoke();
        _co = null;
    }

    private IEnumerator MoveTo(Vector3 targetPos)
    {
        while (true)
        {
            Vector3 pos = dogRoot.position;
            Vector3 to = targetPos - pos;
            to.y = 0f;

            if (to.magnitude <= arriveDistance) break;

            Vector3 step = to.normalized * (walkSpeed * Time.deltaTime);
            dogRoot.position = pos + step;

            yield return null;
        }
    }

    private IEnumerator TurnTo(Vector3 lookWorldPos)
    {
        // turnアニメを再生（見せたい場合）
        TryCrossFade(turnStateFull);

        Vector3 from = dogRoot.position;
        Vector3 dir = lookWorldPos - from;
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.0001f)
            yield break;

        float startTime = Time.time;

        // 目標Yaw角（Y回転）
        Quaternion lookRot = Quaternion.LookRotation(dir.normalized, Vector3.up);
        float targetYaw = lookRot.eulerAngles.y;

        while (true)
        {
            float currentYaw = dogRoot.eulerAngles.y;
            float delta = Mathf.DeltaAngle(currentYaw, targetYaw); // -180..180

            // 回転方向固定
            switch (turnDirection)
            {
                case TurnDirection.AlwaysClockwise:
                    if (delta < 0f) delta += 360f;   // 常に右回り側に寄せる
                    break;

                case TurnDirection.AlwaysCounterClockwise:
                    if (delta > 0f) delta -= 360f;   // 常に左回り側に寄せる
                    break;
            }

            float abs = Mathf.Abs(delta);

            bool angleOk = abs <= finishAngle;
            bool timeOk  = (Time.time - startTime) >= minTurnSeconds;

            if (angleOk && timeOk)
                break;

            float step = rotateDegPerSec * Time.deltaTime; // 度/秒
            float move = Mathf.Clamp(delta, -step, step);
            float newYaw = currentYaw + move;

            dogRoot.rotation = Quaternion.Euler(0f, newYaw, 0f);

            yield return null;
        }

        // 最後にピタ止め
        dogRoot.rotation = Quaternion.Euler(0f, targetYaw, 0f);
    }

    private void TryCrossFade(string fullStateName, float fade = 0.08f)
    {
        if (string.IsNullOrEmpty(fullStateName)) return;

        int layer = 0;

        // Layer名/Indexがズレてると警告出るので、必ず0で存在確認
        if (!animator.HasState(layer, Animator.StringToHash(fullStateName)))
        {
            if (debugLog) Debug.LogWarning($"[DogPointMoverNormal] Animator state not found: {fullStateName}");
            return;
        }

        animator.CrossFade(fullStateName, fade, layer, 0f);
    }
}
