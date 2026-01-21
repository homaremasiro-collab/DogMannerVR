using System;
using System.Collections;
using UnityEngine;

public class DogPointMover : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Transform dog;
    [SerializeField] private Animator animator;

    [Header("Targets")]
    [SerializeField] private Transform walkTarget;
    [SerializeField] private Transform cameraTarget;

    [Header("Move States (FULL name)")]
    [SerializeField] private string walkBackStateFull = "アーマチュア|walk_back";
    [SerializeField] private string turnStateFull     = "アーマチュア|turn";
    [SerializeField] private string runStateFull      = "アーマチュア|run";
    [SerializeField] private string standupStateFull  = "アーマチュア|standup";

    [Header("Action States")]
    [SerializeField] private string enterAdjustState  = "Enter_Adjust";
    [SerializeField] private string lookAtTiltState   = "LookAtCamera_Tilt";
    [SerializeField] private string happyBuildState   = "HappyBuild";
    [SerializeField] private string goodActionState   = "GoodAction";
    [SerializeField] private string poseHoldGoodState = "PoseHold_Good";

    [Header("Move")]
    [SerializeField] private float walkSpeed = 0.7f;
    [SerializeField] private float runSpeed  = 1.8f;
    [SerializeField] private float rotateSpeed = 12f;
    [SerializeField] private float arriveDistance = 0.9f;

    [Header("Durations")]
    [SerializeField] private float standupSec      = 0.9f;
    [SerializeField] private float enterAdjustSec  = 0.35f;
    [SerializeField] private float lookAtTiltSec   = 1.0f;
    [SerializeField] private float happyBuildSec   = 1.2f;
    [SerializeField] private float goodActionSec   = 1.2f;
    [SerializeField] private float poseHoldGoodSec = 1.2f;

    [Header("Turn Show (角度で進行)")]
    [Tooltip("この角度ぶん回ってるのが見えたら次へ（おすすめ 35〜55）")]
    [SerializeField] private float turnShowAngle = 45f;

    [Tooltip("見せターンの最大秒数（安全装置）。おすすめ 0.25〜0.45")]
    [SerializeField] private float turnMaxShowSeconds = 0.35f;

    [Header("Options")]
    [SerializeField] private bool doWalkToWalkTargetAtStart = true;
    [SerializeField] private bool doReturnToWalkTargetAtEnd = true;

    [Header("Debug")]
    [SerializeField] private bool debugLog = true;

    public event Action OnFinished;

    private Coroutine _seq;
    private bool _running;

    public void StartSequence()
    {
        if (_running)
        {
            if (debugLog) Debug.LogWarning("[DogPointMover] StartSequence ignored (already running)");
            return;
        }

        if (_seq != null) StopCoroutine(_seq);
        _seq = StartCoroutine(SequenceRoutine());
    }

    private IEnumerator SequenceRoutine()
    {
        _running = true;

        if (dog == null || animator == null || cameraTarget == null)
        {
            Debug.LogError("[DogPointMover] Missing refs (dog/animator/cameraTarget)");
            _running = false;
            yield break;
        }

        if ((doWalkToWalkTargetAtStart || doReturnToWalkTargetAtEnd) && walkTarget == null)
        {
            Debug.LogError("[DogPointMover] walkTarget 未設定");
            _running = false;
            yield break;
        }

        if (doWalkToWalkTargetAtStart)
        {
            Log("walk_back -> walkTarget (START)");
            PlayStateChecked(walkBackStateFull);
            yield return MoveTo(walkTarget.position, walkSpeed);
        }

        // ===== turn -> cameraTarget（角度で進行）=====
        Log("turn (show by angle) -> cameraTarget");
        yield return ShowTurnToward(cameraTarget.position);

        Log("run -> cameraTarget");
        PlayStateChecked(runStateFull);
        yield return MoveTo(cameraTarget.position, runSpeed);

        // actions
        PlayStateChecked(enterAdjustState);  yield return Wait(enterAdjustSec);
        PlayStateChecked(lookAtTiltState);   yield return Wait(lookAtTiltSec);
        PlayStateChecked(happyBuildState);   yield return Wait(happyBuildSec);
        PlayStateChecked(goodActionState);   yield return Wait(goodActionSec);
        PlayStateChecked(poseHoldGoodState); yield return Wait(poseHoldGoodSec);

        // standup
        Log("standup");
        PlayStateChecked(standupStateFull);
        yield return Wait(standupSec);

        if (doReturnToWalkTargetAtEnd)
        {
            Log("turn (show by angle) -> walkTarget");
            yield return ShowTurnToward(walkTarget.position);

            Log("walk_back -> walkTarget (END)");
            PlayStateChecked(walkBackStateFull);
            yield return MoveTo(walkTarget.position, walkSpeed);
        }

        Log("Sequence done");
        OnFinished?.Invoke();
        _running = false;
    }

    /// <summary>
    /// turnアニメを再生しつつ、目標方向との差角が減ったら次へ進む（時間ではなく角度）
    /// </summary>
    private IEnumerator ShowTurnToward(Vector3 targetPos)
    {
       PlayStateChecked(turnStateFull);

    float needReduce = Mathf.Clamp(turnShowAngle, 5f, 170f);

    float startAngle = AngleToTarget(targetPos);
    float t = 0f;

    while (true)
    {
        // ターン中もTransformを回す（ここが重要）
        RotateToward(targetPos, rotateSpeed);

        t += Time.deltaTime;
        if (turnMaxShowSeconds > 0f && t >= turnMaxShowSeconds) break;

        float nowAngle = AngleToTarget(targetPos);
        float reduced = startAngle - nowAngle;
        if (reduced >= needReduce) break;

        yield return null;
    }

    // 最後は“軽く”合わせるだけ（Snapで一気にやらない）
    for (int i = 0; i < 2; i++)
    {
        RotateToward(targetPos, rotateSpeed * 2f);
        yield return null;
    }
}

private void RotateToward(Vector3 targetPos, float rotSpeed)
{
    Vector3 to = targetPos - dog.position;
    to.y = 0f;
    if (to.sqrMagnitude < 0.0001f) return;

    Quaternion targetRot = Quaternion.LookRotation(to.normalized, Vector3.up);
    dog.rotation = Quaternion.Slerp(dog.rotation, targetRot, rotSpeed * Time.deltaTime);
    }

    private float AngleToTarget(Vector3 targetPos)
    {
        Vector3 to = targetPos - dog.position;
        to.y = 0f;
        if (to.sqrMagnitude < 0.0001f) return 0f;
        Vector3 fwd = dog.forward;
        fwd.y = 0f;
        return Vector3.Angle(fwd, to.normalized);
    }

    private void SnapLook(Vector3 targetPos)
    {
        Vector3 to = targetPos - dog.position;
        to.y = 0f;
        if (to.sqrMagnitude < 0.0001f) return;
        dog.rotation = Quaternion.LookRotation(to.normalized, Vector3.up);
    }

    private void PlayStateChecked(string stateName)
    {
        if (string.IsNullOrWhiteSpace(stateName))
        {
            Debug.LogError("[DogPointMover] state name is empty");
            return;
        }

        int hash = Animator.StringToHash(stateName);
        if (!animator.HasState(0, hash))
        {
            Debug.LogError($"[DogPointMover] Animator state NOT FOUND: '{stateName}' (Layer0)");
            return;
        }

        animator.Play(stateName, 0, 0f);
    }

    private IEnumerator MoveTo(Vector3 targetPos, float speed)
    {
        targetPos.y = dog.position.y;

        while (true)
        {
            Vector3 pos = dog.position;
            Vector3 to = targetPos - pos;
            to.y = 0f;

            float dist = to.magnitude;
            if (dist <= arriveDistance) yield break;

            Vector3 dir = to.normalized;

            if (dir.sqrMagnitude > 0.0001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(dir, Vector3.up);
                dog.rotation = Quaternion.Slerp(dog.rotation, targetRot, rotateSpeed * Time.deltaTime);
            }

            dog.position += dir * speed * Time.deltaTime;
            yield return null;
        }
    }

    private IEnumerator Wait(float sec)
    {
        float t = 0f;
        while (t < sec)
        {
            t += Time.deltaTime;
            yield return null;
        }
    }

    private void Log(string msg)
    {
        if (debugLog) Debug.Log("[DogPointMover] " + msg);
    }
}
