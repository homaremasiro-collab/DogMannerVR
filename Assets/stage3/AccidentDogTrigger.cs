using UnityEngine;

public class AccidentDogTrigger : MonoBehaviour
{
    [Header("Target")]
    public DogAccidentReaction dog;

    [Header("Filter")]
    public string dogTag = "Dog";   // 犬オブジェクトのTag
    public bool requireTag = true;  // Tagで絞る

    [Header("One Shot")]
    public bool oneShot = true;

    [Header("Debug")]
    public bool debugLog = true;

    private bool _fired;
    private Collider _col;

    void Awake()
    {
        _col = GetComponent<Collider>();
        if (_col) _col.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (_fired && oneShot) return;
        if (!dog) return;

        // 犬だけに反応（犬本体or子ColliderでもOKにする）
        GameObject root = other.transform.root.gameObject;

        bool ok = !requireTag || root.CompareTag(dogTag);
        if (!ok)
        {
            if (debugLog) Debug.Log($"[AccidentDogTrigger] ignore enter: {root.name}");
            return;
        }

        if (debugLog) Debug.Log($"[AccidentDogTrigger] FIRE by: {root.name}");

        dog.StartAccident();

        if (oneShot)
        {
            _fired = true;

            // 2度と入っても発火しないようにTrigger自体を止めるのが最強
            if (_col) _col.enabled = false;
        }
    }
}
