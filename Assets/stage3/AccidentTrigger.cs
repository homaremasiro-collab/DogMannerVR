using UnityEngine;

public class AccidentTrigger : MonoBehaviour
{
    public DogAccidentReaction dog;

    [Header("Trigger Control")]
    public bool oneShot = true;
    public float rearmSeconds = 0f;

    [Header("Detect")]
    [Tooltip("XR Origin(親)に付いている CharacterController で判定する")]
    public bool useCharacterController = true;

    [Header("Debug")]
    public bool debugLog = true;

    bool _fired;
    Collider _col;

    void Awake()
    {
        _col = GetComponent<Collider>();
        if (_col) _col.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (_fired && oneShot) return;
        if (!dog) return;

        // --- 判定 ---
        bool isPlayer = false;

        if (useCharacterController)
        {
            // XR Origin の親のどこかに CharacterController が居れば「プレイヤー」とみなす
            var cc = other.GetComponentInParent<CharacterController>();
            isPlayer = (cc != null);
        }
        else
        {
            // 従来互換（名前判定）
            isPlayer = other.name.Contains("Player");
        }

        if (debugLog) Debug.Log($"[AccidentTrigger] Enter other={other.name} isPlayer={isPlayer}");

        if (!isPlayer) return;

        dog.StartAccident();

        if (oneShot)
        {
            _fired = true;

            // もう反応させないのが一番安定
            if (_col) _col.enabled = false;

            if (rearmSeconds > 0f)
                Invoke(nameof(Rearm), rearmSeconds);
        }
    }

    void Rearm()
    {
        _fired = false;
        if (_col) _col.enabled = true;
    }
}
