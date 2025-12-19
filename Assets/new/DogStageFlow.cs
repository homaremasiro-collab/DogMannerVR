using UnityEngine;
using UnityEngine.SceneManagement;

public class DogStageFlow : MonoBehaviour
{
    [Header("Scene Names")]
    public string nextSceneName = "Stage_Dog2";

    [Header("How long to keep walking before switching")]
    public float walkLeadTime = 2.5f; // 2〜5秒くらいがちょうど良い

    [Header("Optional: Fade CanvasGroup")]
    public CanvasGroup fade; // なくてもOK
    public float fadeTime = 0.6f;

    public void GoToDog2AfterShortWalk()
    {
        StartCoroutine(Co_Go());
    }

    System.Collections.IEnumerator Co_Go()
    {
        // 1) 少し歩かせる（今の犬が歩いてる前提）
        yield return new WaitForSeconds(walkLeadTime);

        // 2) フェードアウト（任意）
        if (fade != null)
        {
            float t = 0f;
            while (t < fadeTime)
            {
                t += Time.deltaTime;
                fade.alpha = Mathf.Clamp01(t / fadeTime);
                yield return null;
            }
        }

        // 3) 次のシーンへ
        SceneManager.LoadScene(nextSceneName);
    }
}
