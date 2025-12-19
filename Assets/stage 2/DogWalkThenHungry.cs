using UnityEngine;

public class DogWalkThenHungry : MonoBehaviour
{
    [Header("Move")]
    public float moveSpeed = 0.8f;
    public float walkSeconds = 4.5f;

    // 例：青矢印方向へ進めたいなら (0,0,1)、黄色方向なら (0,0,-1)
    public Vector3 moveDirection = new Vector3(0, 0, 1);

    [Header("Dog Animator")]
    public Animator dogAnimator;
    public string scaredTriggerName = "scared";

    [Header("UI / SFX (optional)")]
    public GameObject hungryUI;   // CanvasやPanelなど。表示したい物を入れる
    public AudioSource hungerAudio;

    float t;
    bool finished;

    void Reset()
    {
        dogAnimator = GetComponentInChildren<Animator>();
    }

    void Start()
    {
        t = 0f;
        finished = false;

        if (hungryUI != null) hungryUI.SetActive(false);
    }

    void Update()
    {
        if (finished) return;

        // 方向は「ワールド方向」で固定（向きに依存しない）
        Vector3 dir = moveDirection;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f) dir = Vector3.forward;
        dir.Normalize();

        // 移動（DogRootが動く）
        transform.position += dir * moveSpeed * Time.deltaTime;

        t += Time.deltaTime;

        if (t >= walkSeconds)
        {
            finished = true;

            // 止まる（もう移動しない）
            // scared再生
            if (dogAnimator != null && !string.IsNullOrEmpty(scaredTriggerName))
                dogAnimator.SetTrigger(scaredTriggerName);

            // 空腹UI
            if (hungryUI != null) hungryUI.SetActive(true);

            // 音
            if (hungerAudio != null) hungerAudio.Play();
        }
    }
}
