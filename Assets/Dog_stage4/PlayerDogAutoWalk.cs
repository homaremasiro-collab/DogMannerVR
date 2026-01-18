using UnityEngine;

public class PlayerDogAutoWalk : MonoBehaviour
{
    [Header("Move")]
    public float moveSpeed = 1.0f;

    [SerializeField]
    private bool walking = true;

    // 他スクリプトから触る用
    public bool Walking
    {
        get => walking;
        set => walking = value;
    }

    [Header("Optional: keep Y")]
    public bool lockY = true;
    private float _baseY;

    private void Awake()
    {
        _baseY = transform.position.y;
    }

    // ★ここが重要：LateUpdateで動かす（停止指示が同フレームで反映される）
    private void LateUpdate()
    {
        if (!walking) return;

        Vector3 delta = transform.forward * (moveSpeed * Time.deltaTime);
        transform.position += delta;

        if (lockY)
        {
            Vector3 p = transform.position;
            p.y = _baseY;
            transform.position = p;
        }
    }

    public void StopWalk() => walking = false;
    public void ResumeWalk() => walking = true;
}
