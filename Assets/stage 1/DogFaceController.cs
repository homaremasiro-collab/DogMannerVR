using UnityEngine;

public class DogFaceController : MonoBehaviour
{
    public Transform cameraTransform;

    void Reset()
    {
        if (Camera.main != null) cameraTransform = Camera.main.transform;
    }

    // カメラに背中を向ける（＝カメラと逆方向を向く）
    public void FaceAwayFromCamera()
    {
        if (cameraTransform == null) return;

        Vector3 dir = transform.position - cameraTransform.position; // カメラ→犬
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.0001f) return;

        transform.rotation = Quaternion.LookRotation(dir.normalized);
    }
}
