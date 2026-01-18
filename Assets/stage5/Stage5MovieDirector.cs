using UnityEngine;
using UnityEngine.Video;

public class Stage5MovieDirector : MonoBehaviour
{
    [Header("Video Player")]
    [SerializeField] private VideoPlayer videoPlayer;

    [Header("Movies")]
    [SerializeField] private VideoClip goodMovie;
    [SerializeField] private VideoClip normalMovie;
    [SerializeField] private VideoClip badMovie;

    [Header("Option")]
    [SerializeField] private bool playOnStart = true;

    private void Awake()
    {
        // VideoPlayer が未指定なら同じ GameObject から取得
        if (!videoPlayer)
            videoPlayer = GetComponent<VideoPlayer>();
    }

    private void Start()
    {
        if (playOnStart)
        {
            PlayByResult();
        }
    }

    /// <summary>
    /// ResultStore の合計を見て Movie を分岐再生
    /// </summary>
    public void PlayByResult()
    {
        if (ResultStore.Instance == null)
        {
            Debug.LogError("[Stage5MovieDirector] ResultStore.Instance が見つかりません");
            return;
        }

        int g = ResultStore.Instance.Good;
        int n = ResultStore.Instance.Normal;
        int b = ResultStore.Instance.Bad;

        Debug.Log($"[Stage5MovieDirector] Result g={g}, n={n}, b={b}");

        VideoClip clip = DecideClip(g, n, b);

        if (clip == null)
        {
            Debug.LogError("[Stage5MovieDirector] 再生する VideoClip が設定されていません");
            return;
        }

        if (!videoPlayer)
        {
            Debug.LogError("[Stage5MovieDirector] VideoPlayer が設定されていません");
            return;
        }

        videoPlayer.Stop();
        videoPlayer.clip = clip;
        videoPlayer.Play();

        Debug.Log($"[Stage5MovieDirector] Play movie: {clip.name}");
    }

    /// <summary>
    /// 評価ルール
    /// ・一番多いカテゴリを採用
    /// ・同点の場合は Good > Normal > Bad の優先順
    /// </summary>
    private VideoClip DecideClip(int g, int n, int b)
    {
        if (g >= n && g >= b)
            return goodMovie;

        if (n >= g && n >= b)
            return normalMovie;

        return badMovie;
    }
}
