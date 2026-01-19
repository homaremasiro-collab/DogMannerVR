using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Stage5GoodDirector : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Transform dog;          // DogMover(Transform) を入れる
    [SerializeField] private Animator dogAnimator;   // iiiiinu2(Animator) を入れる

    [Header("Points")]
    [SerializeField] private Transform runStart;
    [SerializeField] private Transform runEnd;
    [SerializeField] private Transform toCamera;
    [SerializeField] private Transform disappearPoint;

    [Header("Animator State Names (State OR Clip name OK)")]
    [SerializeField] private string runState = "アーマチュア|run";
    [SerializeField] private string happyState = "アーマチュア|happy";
    [SerializeField] private string walkBackState = "アーマチュア|walk_back";

    [Header("Timing (seconds)")]
    [SerializeField] private float runSeconds = 2f;
    [SerializeField] private float happySeconds = 1f;
    [SerializeField] private float walkSeconds = 3f;

    [Header("Next Scene")]
    [SerializeField] private string nextScene = "Stage5_Summary";

    private void Start()
    {
        if (dog == null) Debug.LogError("[Stage5GoodDirector] Dog is null");
        if (dogAnimator == null) Debug.LogError("[Stage5GoodDirector] DogAnimator is null");

        StartCoroutine(Sequence());
    }

    private IEnumerator Sequence()
    {
        // 0) 初期位置
        if (runStart != null) SetDogPose(runStart.position, runStart.rotation);

        // 1) 走る（RunEndへ）
        PlayByName(runState);
        yield return MoveForSeconds(runEnd, runSeconds);

        // 2) カメラの方へ行く（ToCameraへ）
        //    ※ “walk_back” が「後ろ歩き」なら、見た目に合わせてコレでOK
        PlayByName(walkBackState);
        yield return MoveForSeconds(toCamera, walkSeconds);

        // 3) しっぽ振り等（happy）
        PlayByName(happyState);
        yield return new WaitForSeconds(happySeconds);

        // 4) 踵返して消える地点へ
        //    ここは「run」でも「walk_back」でも見た目が合う方にしてOK
        PlayByName(runState);
        yield return MoveForSeconds(disappearPoint, 1.2f);

        // 5) 光になって消える（とりあえず非表示）
        SetVisible(false);

        // 6) 次へ
        yield return new WaitForSeconds(0.2f);
        if (!string.IsNullOrEmpty(nextScene))
            SceneManager.LoadScene(nextScene);
    }

    private void PlayByName(string stateOrClip)
    {
        if (dogAnimator == null || string.IsNullOrEmpty(stateOrClip)) return;

        // まず「そのまま」CrossFade（State名一致を狙う）
        dogAnimator.CrossFade(stateOrClip, 0.05f, 0, 0f);

        // もし「アーマチュア|run」みたいな名前で、
        // 実体が別名ステートだった時の保険として、
        // “|” の後ろも試す（run / happy / walk_back）
        int bar = stateOrClip.LastIndexOf('|');
        if (bar >= 0 && bar < stateOrClip.Length - 1)
        {
            string shortName = stateOrClip.Substring(bar + 1);
            dogAnimator.CrossFade(shortName, 0.05f, 0, 0f);
        }
    }

    private IEnumerator MoveForSeconds(Transform target, float seconds)
    {
        if (dog == null || target == null || seconds <= 0f)
        {
            yield return new WaitForSeconds(Mathf.Max(0.01f, seconds));
            yield break;
        }

        float t = 0f;
        Vector3 startPos = dog.position;
        Quaternion startRot = dog.rotation;

        // 目的方向へ向ける（キモ）
        Vector3 dir = (target.position - dog.position);
        dir.y = 0f;
        if (dir.sqrMagnitude > 0.0001f)
            dog.rotation = Quaternion.LookRotation(dir.normalized, Vector3.up);

        while (t < seconds)
        {
            t += Time.deltaTime;
            float a = Mathf.Clamp01(t / seconds);
            dog.position = Vector3.Lerp(startPos, target.position, a);

            // 常にターゲット方向へ向く（横滑りしにくい）
            Vector3 d = (target.position - dog.position);
            d.y = 0f;
            if (d.sqrMagnitude > 0.0001f)
                dog.rotation = Quaternion.Slerp(dog.rotation, Quaternion.LookRotation(d.normalized, Vector3.up), 10f * Time.deltaTime);

            yield return null;
        }
    }

    private void SetDogPose(Vector3 pos, Quaternion rot)
    {
        if (dog == null) return;
        dog.position = pos;
        dog.rotation = rot;
    }

    private void SetVisible(bool visible)
    {
        if (dog == null) return;

        foreach (var r in dog.GetComponentsInChildren<Renderer>(true))
            r.enabled = visible;
    }
}
