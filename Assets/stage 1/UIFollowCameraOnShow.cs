using UnityEngine;

public class UIFollowCameraOnShow : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;
    [SerializeField] private float distance = 1.4f;
    [SerializeField] private float heightOffset = 0.0f;

    // ちょい調整：視線の上下を反映したくないなら true
    [SerializeField] private bool ignorePitch = false;

    void Reset()
    {
        targetCamera = Camera.main;
    }

    public void PlaceNow()
    {
        if (targetCamera == null) targetCamera = Camera.main;
        if (targetCamera == null) return;

        Transform camT = targetCamera.transform;

        Vector3 forward = camT.forward;
        if (ignorePitch)
        {
            forward.y = 0f;
            if (forward.sqrMagnitude < 0.0001f) forward = camT.forward;
            forward.Normalize();
        }

        Vector3 pos = camT.position + forward * distance;
        pos += Vector3.up * heightOffset;

        transform.position = pos;

        // UIをカメラに向ける（Billboardがあるなら不要だけど安全に）
        transform.rotation = Quaternion.LookRotation(transform.position - camT.position);
        // ↑これだと逆向く場合は下に変える：
        // transform.rotation = Quaternion.LookRotation(camT.position - transform.position);
    }
}
