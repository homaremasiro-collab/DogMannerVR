using UnityEngine;

public class DogTrigger : MonoBehaviour
{
    public enum Type { Body, Head, Sniff }
    public Type triggerType;

    public DogTouchJudge manager;
    public bool debugLog = true;

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Hand")) return;
        if (manager == null) return;

        if (debugLog) Debug.Log($"[{triggerType} ENTER] other={other.name} tag={other.tag}");

        if (triggerType == Type.Body) manager.OnBodyTouched();
        if (triggerType == Type.Head) manager.OnHeadTouched();
    }

    void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Hand")) return;
        if (manager == null) return;

        if (triggerType == Type.Sniff) manager.OnSniffStay();
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Hand")) return;
        if (manager == null) return;

        if (debugLog) Debug.Log($"[{triggerType} EXIT] other={other.name} tag={other.tag}");

        if (triggerType == Type.Sniff) manager.OnSniffExit();
    }
}
