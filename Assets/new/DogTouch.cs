using UnityEngine;

public class DogTouchJudge : MonoBehaviour
{
    [Header("Auto Assign (空でOK)")]
    [Tooltip("空なら、このオブジェクト or 子から Animator を自動で拾います")]
    public Animator dogAnimator;

    [Tooltip("空なら MainCamera を自動で探して入れます")]
    public Transform hmd;

    [Header("Animator Trigger Names")]
    public string trigBody = "TouchBody";
    public string trigHead = "TouchHead";
    public string trigSniffOk = "SniffOK";

    [Header("Sniff settings")]
    public float crouchHeight = 1.2f;
    public float sniffHoldSeconds = 0.6f;

    [Header("Debug")]
    public bool debugLog = true;

    bool decided = false;
    bool sniffInProgress = false;
    float sniffTimer = 0f;

    void Awake()
    {
        // Animator 自動取得
        if (dogAnimator == null)
        {
            dogAnimator = GetComponentInChildren<Animator>();
            if (dogAnimator == null) dogAnimator = GetComponent<Animator>();
        }

        // HMD 自動取得（MainCamera）
        if (hmd == null)
        {
            var cam = Camera.main;
            if (cam != null) hmd = cam.transform;
        }

        if (debugLog)
        {
            Debug.Log($"[DogTouchJudge] Animator={(dogAnimator ? dogAnimator.name : "NULL")}, HMD={(hmd ? hmd.name : "NULL")}");
        }
    }

    public void OnBodyTouched()
    {
        if (decided) return;
        if (sniffInProgress) return;

        decided = true;
        if (dogAnimator != null) dogAnimator.SetTrigger(trigBody);
        if (debugLog) Debug.Log("判定：✖ いきなり触る（体）");
    }

    public void OnHeadTouched()
    {
        if (decided) return;
        if (sniffInProgress) return;

        decided = true;
        if (dogAnimator != null) dogAnimator.SetTrigger(trigHead);
        if (debugLog) Debug.Log("判定：△ 頭を触った（怖がる可能性）");
    }

    public void OnSniffStay()
    {
        if (decided) return;

        if (hmd == null)
        {
            // 実行中に MainCamera が取れる場合もあるので再取得
            var cam = Camera.main;
            if (cam != null) hmd = cam.transform;
            if (hmd == null) return;
        }

        bool crouching = hmd.position.y < crouchHeight;
        if (!crouching)
        {
            sniffInProgress = false;
            sniffTimer = 0f;
            return;
        }

        sniffInProgress = true;
        sniffTimer += Time.deltaTime;

        if (sniffTimer >= sniffHoldSeconds)
        {
            decided = true;
            if (dogAnimator != null) dogAnimator.SetTrigger(trigSniffOk);
            if (debugLog) Debug.Log("判定：○ しゃがんで手を差し出した（鼻でスニッフ）");
        }
    }

    public void OnSniffExit()
    {
        sniffInProgress = false;
        sniffTimer = 0f;
        if (debugLog) Debug.Log("Sniff中断");
    }

    public void ResetJudge()
    {
        decided = false;
        sniffInProgress = false;
        sniffTimer = 0f;
        if (debugLog) Debug.Log("Judge Reset");
    }
}
