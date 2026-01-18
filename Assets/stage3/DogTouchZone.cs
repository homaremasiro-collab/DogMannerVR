using UnityEngine;

public class DogTouchZone : MonoBehaviour
{
    public enum ZoneType { Head, Nose, Collar }

    public ZoneType type = ZoneType.Head;
    public DogAccidentReaction reaction; // 犬本体の DogAccidentReaction を入れる

    [Header("Debug")]
    public bool debugLog = false;

    void Reset()
    {
        reaction = GetComponentInParent<DogAccidentReaction>();
        var col = GetComponent<Collider>();
        if (col) col.isTrigger = true;
    }

    void Awake()
    {
        // Resetが動かないケース（Prefabなど）に備えてAwakeでも拾う
        if (!reaction) reaction = GetComponentInParent<DogAccidentReaction>();

        var col = GetComponent<Collider>();
        if (col) col.isTrigger = true;
    }

    void OnTriggerStay(Collider other)
    {
        if (!reaction) return;

        var hand = other.GetComponentInParent<PlayerHand>();
        if (!hand) return;

        if (debugLog)
            Debug.Log($"[DogTouchZone] Stay type={type} other={other.name} hand={hand.name}");

        reaction.OnHandStay(type, hand);
    }

    void OnTriggerExit(Collider other)
    {
        if (!reaction) return;

        var hand = other.GetComponentInParent<PlayerHand>();
        if (!hand) return;

        if (debugLog)
            Debug.Log($"[DogTouchZone] Exit type={type} other={other.name} hand={hand.name}");

        reaction.OnHandExit(type, hand);
    }
}
