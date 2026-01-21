using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameFlow : MonoBehaviour
{
    public static GameFlow Instance { get; private set; }

    [Header("Config")]
    [SerializeField] private FlowConfig config;

    [Header("XR (Optional)")]
    [Tooltip("Bootstrapに置いた XR Origin / XR Rig のルートを入れる（任意だが強く推奨）")]
    [SerializeField] private Transform xrOriginRoot;

    [Tooltip("移動先を探すタグ。各シーンに SpawnPoint を1つ置いてこのタグにする")]
    [SerializeField] private string spawnPointTag = "SpawnPoint";

    [Header("Debug")]
    [SerializeField] private bool autoStartOnPlay = true;
    [SerializeField] private bool debugLog = true;

    private string _currentAdditiveScene = "";
    private bool _isLoading = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private IEnumerator Start()
    {
        if (ScreenFader.Instance) ScreenFader.Instance.InstantBlack();

        if (!autoStartOnPlay) yield break;
        if (config == null)
        {
            Debug.LogError("[GameFlow] FlowConfig が未設定です（BootstrapのGameFlowに割り当て）");
            yield break;
        }

        // 初期化安定用に1フレ
        yield return null;

        // 最初のシーン（あなたの場合は game）を Additive で読む
        var first = config.GetFirstStageName();
        yield return LoadAdditiveAndSetActive(first);

        // XRをSpawnへ（あれば）
        TryMoveXROriginToSpawn(first);

        if (ScreenFader.Instance) yield return ScreenFader.Instance.FadeIn();
    }

    // --------------------
    // Public APIs
    // --------------------

    /// <summary>
    /// game(導入) から Stage1(=Stage_Dog1)へ開始
    /// </summary>
    public void StartFromHub()
    {
        if (_isLoading) return;
        StartCoroutine(StartFromHubRoutine());
    }

    /// <summary>
    /// ステージ完了（Stage1〜4）
    /// </summary>
    public void CompleteStage(StageId stageId, StageOutcome outcome)
    {
        if (_isLoading) return;
        StartCoroutine(CompleteStageRoutine(stageId, outcome));
    }

    /// <summary>
    /// どこかのシーンへ（Additiveで差し替え）
    /// </summary>
    public void GoToSceneAdditive(string sceneName, bool resetResult = false)
    {
        if (_isLoading) return;
        StartCoroutine(GoToSceneAdditiveRoutine(sceneName, resetResult));
    }

    /// <summary>
    /// どこかのシーンへ（Single。タイトル等で使う）
    /// </summary>
    public void GoToSceneSingle(string sceneName, bool resetResult = false)
    {
        if (_isLoading) return;
        StartCoroutine(GoToSceneSingleRoutine(sceneName, resetResult));
    }

    /// <summary>
    /// 最初（First Stage Scene = game）からやり直し
    /// </summary>
    public void RestartFromBeginning()
    {
        if (_isLoading) return;
        GoToSceneAdditive(config.GetFirstStageName(), resetResult: true);
    }

    // --------------------
    // Routines
    // --------------------

    private IEnumerator StartFromHubRoutine()
    {
        _isLoading = true;
        if (ScreenFader.Instance) yield return ScreenFader.Instance.FadeOut();

        // 結果リセット（導入からの開始は汚したくない）
        if (ResultStore.Instance) ResultStore.Instance.ResetAll();

        // 現在（game）を落として Stage1へ
        if (!string.IsNullOrEmpty(_currentAdditiveScene))
            yield return UnloadIfLoaded(_currentAdditiveScene);

        var stage1 = config.GetStageSceneName(StageId.Stage1);
        yield return LoadAdditiveAndSetActive(stage1);

        TryMoveXROriginToSpawn(stage1);

        if (ScreenFader.Instance) yield return ScreenFader.Instance.FadeIn();
        _isLoading = false;
    }

    private IEnumerator CompleteStageRoutine(StageId stageId, StageOutcome outcome)
    {
        _isLoading = true;

        // 結果保存
        if (ResultStore.Instance) ResultStore.Instance.Add(outcome);
        else Debug.LogError("[GameFlow] ResultStore が見つかりません（Bootstrapに置く）");

        if (ScreenFader.Instance) yield return ScreenFader.Instance.FadeOut();

        // 現在のAdditiveを落とす
        if (!string.IsNullOrEmpty(_currentAdditiveScene))
            yield return UnloadIfLoaded(_currentAdditiveScene);

        // 次を決定
        if (stageId == StageId.Stage4)
        {
            // Stage5へ分岐
            var finalOutcome = ResultStore.Instance
                ? ResultStore.Instance.GetFinalOutcome(config.tieBreakPriority)
                : StageOutcome.Good;

            var stage5 = config.GetStage5SceneName(finalOutcome);
            yield return LoadAdditiveAndSetActive(stage5);
            TryMoveXROriginToSpawn(stage5);
        }
        else
        {
            var nextId = (StageId)((int)stageId + 1);
            var nextScene = config.GetStageSceneName(nextId);
            yield return LoadAdditiveAndSetActive(nextScene);
            TryMoveXROriginToSpawn(nextScene);
        }

        if (ScreenFader.Instance) yield return ScreenFader.Instance.FadeIn();
        _isLoading = false;
    }

    private IEnumerator GoToSceneAdditiveRoutine(string sceneName, bool resetResult)
    {
        _isLoading = true;
        if (ScreenFader.Instance) yield return ScreenFader.Instance.FadeOut();

        if (resetResult && ResultStore.Instance) ResultStore.Instance.ResetAll();

        if (!string.IsNullOrEmpty(_currentAdditiveScene))
            yield return UnloadIfLoaded(_currentAdditiveScene);

        yield return LoadAdditiveAndSetActive(sceneName);
        TryMoveXROriginToSpawn(sceneName);

        if (ScreenFader.Instance) yield return ScreenFader.Instance.FadeIn();
        _isLoading = false;
    }

    private IEnumerator GoToSceneSingleRoutine(string sceneName, bool resetResult)
    {
        _isLoading = true;
        if (ScreenFader.Instance) yield return ScreenFader.Instance.FadeOut();

        if (resetResult && ResultStore.Instance) ResultStore.Instance.ResetAll();

        // Additive管理しているシーンを落としてからSingleへ
        if (!string.IsNullOrEmpty(_currentAdditiveScene))
            yield return UnloadIfLoaded(_currentAdditiveScene);

        _currentAdditiveScene = "";

        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);

        // 1フレ後にフェードイン
        yield return null;
        TryMoveXROriginToSpawn(sceneName);

        if (ScreenFader.Instance) yield return ScreenFader.Instance.FadeIn();
        _isLoading = false;
    }

    // --------------------
    // Core Load/Unload
    // --------------------

    private IEnumerator LoadAdditiveAndSetActive(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("[GameFlow] sceneName が空です（FlowConfig設定を確認）");
            yield break;
        }

        if (debugLog) Debug.Log($"[GameFlow] Load Additive: {sceneName}");

        // すでにロード済みならそのままActiveに
        var already = SceneManager.GetSceneByName(sceneName);
        if (already.isLoaded)
        {
            SceneManager.SetActiveScene(already);
            _currentAdditiveScene = sceneName;
            yield break;
        }

        var op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        if (op == null)
        {
            Debug.LogError($"[GameFlow] LoadSceneAsync(Additive) 失敗: {sceneName}");
            yield break;
        }

        while (!op.isDone) yield return null;

        var loaded = SceneManager.GetSceneByName(sceneName);
        if (!loaded.IsValid())
        {
            Debug.LogError($"[GameFlow] Scene invalid after load: {sceneName}");
            yield break;
        }

        SceneManager.SetActiveScene(loaded);
        _currentAdditiveScene = sceneName;
    }

    private IEnumerator UnloadIfLoaded(string sceneName)
    {
        var s = SceneManager.GetSceneByName(sceneName);
        if (!s.isLoaded) yield break;

        if (debugLog) Debug.Log($"[GameFlow] Unload: {sceneName}");

        var op = SceneManager.UnloadSceneAsync(sceneName);
        if (op == null) yield break;
        while (!op.isDone) yield return null;
    }

    // --------------------
    // XR Spawn
    // --------------------

    private void TryMoveXROriginToSpawn(string sceneName)
    {
        if (xrOriginRoot == null) return;

        // ActiveScene（今ロードしたシーン）内で SpawnPointTag を探す
        var active = SceneManager.GetActiveScene();
        if (!active.IsValid() || !active.isLoaded) return;

        GameObject spawn = FindWithTagInScene(active, spawnPointTag);
        if (spawn == null)
        {
            if (debugLog) Debug.Log($"[GameFlow] SpawnPoint not found in {active.name} (tag={spawnPointTag})");
            return;
        }

        xrOriginRoot.position = spawn.transform.position;
        xrOriginRoot.rotation = spawn.transform.rotation;

        if (debugLog) Debug.Log($"[GameFlow] XR moved to SpawnPoint in {active.name}");
    }

    private static GameObject FindWithTagInScene(Scene scene, string tag)
    {
        var roots = scene.GetRootGameObjects();
        for (int i = 0; i < roots.Length; i++)
        {
            var t = roots[i].transform.GetComponentsInChildren<Transform>(true);
            for (int j = 0; j < t.Length; j++)
            {
                if (t[j].CompareTag(tag))
                    return t[j].gameObject;
            }
        }
        return null;
    }
}
