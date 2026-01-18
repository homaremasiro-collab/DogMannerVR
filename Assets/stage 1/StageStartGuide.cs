using UnityEngine;

public class StageStartGuide : MonoBehaviour
{
    [Header("UI Reference")]
    [SerializeField] private FloatingGuideUI guideUI;

    [Header("Guide Message")]
    [TextArea(3, 10)]
    [SerializeField]
    private string message =
        "ステージ1：初対面の犬\n\n" +
        "犬は急に触られると怖がることがあります。\n" +
        "しゃがんで、手をゆっくり差し出してみましょう。";

    [Header("Timing")]
    [SerializeField] private float delaySeconds = 0.2f;

    private void Start()
    {
        if (guideUI == null)
        {
            Debug.LogWarning("StageStartGuide : guideUI が設定されていません");
            return;
        }

        Invoke(nameof(ShowGuide), delaySeconds);
    }

    private void ShowGuide()
    {
        // ★追加：カメラ前に配置
        var placer = guideUI.GetComponent<UIFollowCameraOnShow>();
        if (placer == null) placer = guideUI.GetComponentInParent<UIFollowCameraOnShow>();
        if (placer != null) placer.PlaceNow();

        // 表示
        guideUI.ShowMessage(message);
    }
}
