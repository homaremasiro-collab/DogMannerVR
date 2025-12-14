using UnityEngine;

public class DogTouchJudgeSimple : MonoBehaviour
{
    public Transform hmd;                // XR Origin の Main Camera を入れる
    public float crouchHeight = 1.2f;     // これ以下なら「しゃがみ扱い」
    public float sniffHoldSeconds = 1.0f; // においゾーン維持時間

    bool decided = false;
    float sniffTimer = 0f;

    public void OnBodyTouched()
    {
        if (decided) return;
        decided = true;
        Debug.Log("判定：❌ いきなり触る（体に触った）");
    }

    public void OnHeadTouched()
    {
        if (decided) return;
        decided = true;
        Debug.Log("判定：△ 頭の上から触った（怖がる可能性）");
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
            decided = true;
            Debug.Log("判定：◯ しゃがんで手を差し出した（安心）");
        }
    }

    public void OnSniffExit()
    {
        if (decided) return;
        sniffTimer = 0f;
    }
}
