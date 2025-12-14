using System.Collections;
using UnityEngine;
using Unity.XR.CoreUtils;   // XROrigin 用

public class VRSpawnPoint : MonoBehaviour
{
    [SerializeField] private Transform spawnPoint;

    private XROrigin xrOrigin;

    private IEnumerator Start()
    {
        xrOrigin = GetComponent<XROrigin>();
        if (xrOrigin == null || spawnPoint == null) yield break;

        // ★ 1フレーム待って、XRトラッキングが有効になってから処理する
        yield return null;  // 足りなければ WaitForSeconds(0.1f) くらいにしてもOK

        // カメラ(頭)をワールドの spawnPoint に合わせる便利メソッド
        xrOrigin.MoveCameraToWorldLocation(spawnPoint.position);

        // 向きも SpawnPoint に合わせたい場合（任意）
        var euler = xrOrigin.transform.eulerAngles;
        euler.y = spawnPoint.eulerAngles.y;
        xrOrigin.transform.eulerAngles = euler;
    }
}
