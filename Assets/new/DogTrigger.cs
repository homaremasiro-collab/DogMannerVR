using UnityEngine;

public class DogTrigger : MonoBehaviour
{
    public enum Type { Body, Head, Sniff }
    public Type triggerType;
    public DogTouchJudgeSimple manager;

    // どのゾーンか分かりやすいように表示名
    [SerializeField] string zoneName = "";

    private void Awake()
    {
        if (string.IsNullOrEmpty(zoneName))
            zoneName = gameObject.name;
    }

    private void OnTriggerEnter(Collider other)
    {
        // ★デバッグ：何が入ってきたか必ず出す（Sniffが入ってない問題の特定用）
        if (triggerType == Type.Sniff)
        {
            Debug.Log($"[Sniff ENTER] zone={zoneName} other={other.name} tag={other.tag}");
        }

        if (!other.CompareTag("Hand")) return;
        if (manager == null) return;

        if (triggerType == Type.Body) manager.OnBodyTouched();
        if (triggerType == Type.Head) manager.OnHeadTouched();
        // SniffはStayで判定するのでEnterでは何もしない
    }

    private void OnTriggerStay(Collider other)
    {
        // ★デバッグ：Sniffに手が入ってる間ずっと出す（うるさければ後で消す）
        if (triggerType == Type.Sniff)
        {
            Debug.Log($"[Sniff STAY] zone={zoneName} other={other.name} tag={other.tag}");
        }

        if (!other.CompareTag("Hand")) return;
        if (manager == null) return;

        if (triggerType == Type.Sniff) manager.OnSniffStay();
    }

    private void OnTriggerExit(Collider other)
    {
        // ★デバッグ：出たことも分かる
        if (triggerType == Type.Sniff)
        {
            Debug.Log($"[Sniff EXIT] zone={zoneName} other={other.name} tag={other.tag}");
        }

        if (!other.CompareTag("Hand")) return;
        if (manager == null) return;

        if (triggerType == Type.Sniff) manager.OnSniffExit();
    }
}
