using UnityEngine;

public class DogArousalController : MonoBehaviour
{
    [Range(0f, 1f)]
    public float arousal = 0f;

    public Animator animator;
    public Transform otherDog;
    public Transform player;

    [Header("Tuning")]
    public float triggerDistance = 3f;
    public float calmDownPerSec = 0.2f;
    public float distanceArousalGainPerSec = 1.0f;
    public float speedArousalGain = 0.15f;

    Vector3 _prevPlayerPos;

    void Start()
    {
        if (player != null) _prevPlayerPos = player.position;
    }

    void Update()
    {
        if (animator == null || otherDog == null || player == null) return;

        UpdateArousal();
        animator.SetFloat("Arousal", arousal);
    }

    void UpdateArousal()
    {
        float distance = Vector3.Distance(transform.position, otherDog.position);

        // プレイヤー速度（Rigidbody不要）
        float playerSpeed = 0f;
        Vector3 now = player.position;
        playerSpeed = (now - _prevPlayerPos).magnitude / Mathf.Max(Time.deltaTime, 0.0001f);
        _prevPlayerPos = now;

        // 近いほど興奮（triggerDistance 内のみ増える）
        float distFactor = Mathf.Clamp01((triggerDistance - distance) / triggerDistance);
        arousal += distFactor * distanceArousalGainPerSec * Time.deltaTime;

        // 速い動きほど興奮
        arousal += playerSpeed * speedArousalGain * Time.deltaTime;

        // 自然に落ち着く
        arousal -= calmDownPerSec * Time.deltaTime;

        arousal = Mathf.Clamp01(arousal);
    }

    public void StartBark()
    {
        arousal = Mathf.Max(arousal, 0.6f);
    }
}
