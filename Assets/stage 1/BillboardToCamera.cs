using UnityEngine;

public class BillboardToCamera : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;
    [SerializeField] private bool lockX = false; // trueにすると上下回転を固定（酔いにくい）

    void LateUpdate()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;
        if (targetCamera == null) return;

        Vector3 dir = transform.position - targetCamera.transform.position;
        if (dir.sqrMagnitude < 0.0001f) return;

        if (lockX) dir.y = 0f; // 水平だけ向ける

        transform.rotation = Quaternion.LookRotation(dir);
    }
}
