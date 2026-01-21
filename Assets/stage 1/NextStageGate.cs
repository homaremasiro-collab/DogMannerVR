using UnityEngine;
using UnityEngine.SceneManagement;

public class NextStageGate : MonoBehaviour
{
    [Header("Next Scene (must match Build Settings)")]
    [SerializeField] private string nextSceneName = "Stage_Dog2";

    [Header("Player Tag (XR Origin etc.)")]
    [SerializeField] private string playerTag = "Player";

    [Header("Gate starts hidden until stage complete")]
    [SerializeField] private bool startHidden = true;

    private bool canGoNext = false;

    private void Awake()
    {
        if (startHidden)
            SetGateActive(false);
        else
            EnableGate();
    }

    // ステージ完了時に呼ぶ
    public void EnableGate()
    {
        canGoNext = true;
        SetGateActive(true);
        Debug.Log("[NextStageGate] Gate enabled.");
    }

    public void DisableGate()
    {
        canGoNext = false;
        SetGateActive(false);
        Debug.Log("[NextStageGate] Gate disabled.");
    }

    private void SetGateActive(bool active)
    {
        // Gateオブジェクト丸ごとON/OFFでもOKだが、
        // スクリプトは生かしたいので「見える部分やCollider」だけ切り替える方式が安全
        var colliders = GetComponentsInChildren<Collider>(true);
        foreach (var c in colliders) c.enabled = active;

        var renderers = GetComponentsInChildren<Renderer>(true);
        foreach (var r in renderers) r.enabled = active;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!canGoNext) return;

        // XRだと「プレイヤー本体」がCollider持ってないことがあるので、
        // タグ判定は親まで辿る（otherが手や子Colliderでも拾える）
        var root = other.attachedRigidbody ? other.attachedRigidbody.transform : other.transform;
        if (!root.CompareTag(playerTag) && root.GetComponentInParent<Transform>()?.CompareTag(playerTag) != true)
        {
            // 親にPlayerが居るかもチェック
            var p = other.GetComponentInParent<Transform>();
            if (p == null || !p.CompareTag(playerTag)) return;
        }

        Debug.Log($"[NextStageGate] Player entered gate. Load {nextSceneName}");
        SceneManager.LoadScene(nextSceneName);
    }
}
