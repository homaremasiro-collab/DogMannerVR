using System.Collections;
using UnityEngine;

public class DogTouchJudge : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Animator dogAnimator;
    [SerializeField] private Transform hmd;
    [SerializeField] private DogTurnController turnController;
    [SerializeField] private Transform nextDirectionTarget;
    [SerializeField] private NextStageGate nextStageGate;

    [Header("Animator Trigger Names")]
    [SerializeField] private string trigBody = "TouchBody";
    [SerializeField] private string trigHead = "TouchHead";
    [SerializeField] private string trigSniffOk = "SniffOK";

    [Header("Animator State Names (Copy Path推奨)")]
    [Tooltip("例: アーマチュア|turn")]
    [SerializeField] private string turnStateName = "アーマチュア|turn";
    [Tooltip("例: アーマチュア|walk_back")]
    [SerializeField] private string walkBackStateName = "アーマチュア|walk_back";

    [Header("Sniff settings")]
    [SerializeField] private float crouchHeight = 1.2f;
    [SerializeField] private float sniffHoldSeconds = 1.0f;

    [Header("Reaction guarantee")]
    [SerializeField] private float reactionHoldSeconds = 2.0f;

    [Header("Debug")]
    [SerializeField] private bool debugLog = true;

    private bool decided = false;
    private bool processing = false;
    private float sniffTimer = 0f;

    private void Awake()
    {
        if (dogAnimator == null)
        {
            dogAnimator = GetComponentInChildren<Animator>();
            if (dogAnimator == null) dogAnimator = GetComponent<Animator>();
        }

        if (hmd == null && Camera.main != null)
            hmd = Camera.main.transform;
    }

    // --- DogTrigger.cs が呼ぶメソッド名に合わせる ---

    public void OnBodyTouched()
    {
        if (decided) return;
        StartCoroutine(ReactionSequence(trigBody));
    }

    public void OnHeadTouched()
    {
        if (decided) return;
        StartCoroutine(ReactionSequence(trigHead));
    }

    public void OnSniffStay()
    {
        if (decided) return;
        if (hmd == null) return;

        // しゃがみ判定（HMDの高さ）
        bool crouching = hmd.position.y < crouchHeight;
        if (!crouching)
        {
            sniffTimer = 0f;
            return;
        }

        sniffTimer += Time.deltaTime;
        if (sniffTimer >= sniffHoldSeconds)
        {
            sniffTimer = 0f;
            StartCoroutine(ReactionSequence(trigSniffOk));
        }
    }

    public void OnSniffExit()
    {
        sniffTimer = 0f;
    }

    private IEnumerator ReactionSequence(string triggerName)
    {
        if (processing) yield break;
        processing = true;
        decided = true;

        if (debugLog)
            Debug.Log($"[DogTouchJudge] Trigger: {triggerName}");

        // ① リアクション発火
        if (dogAnimator != null)
        {
            dogAnimator.ResetTrigger(trigBody);
            dogAnimator.ResetTrigger(trigHead);
            dogAnimator.ResetTrigger(trigSniffOk);
            dogAnimator.SetTrigger(triggerName);
        }

        // ② リアクションを必ず再生
        yield return new WaitForSeconds(reactionHoldSeconds);

        // ③ ターン（物理回転）
        if (turnController != null && nextDirectionTarget != null)
        {
            turnController.StartTurn(nextDirectionTarget.position);
            while (turnController.IsTurning) yield return null;
        }
        else
        {
            // turnController無しでも見た目だけturn stateへ
            if (dogAnimator != null && !string.IsNullOrEmpty(turnStateName))
                dogAnimator.CrossFade(turnStateName, 0.05f);
            yield return new WaitForSeconds(0.3f);
        }

        // ④ walk_back
        if (dogAnimator != null && !string.IsNullOrEmpty(walkBackStateName))
            dogAnimator.CrossFade(walkBackStateName, 0.05f);

        // ⑤ ゲート解放
        if (nextStageGate != null)
            nextStageGate.EnableGate();
    }
}
