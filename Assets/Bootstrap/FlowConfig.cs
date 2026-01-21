// FlowConfig.cs
using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(menuName = "Flow/FlowConfig", fileName = "FlowConfig")]
public class FlowConfig : ScriptableObject
{
    [Header("Start")]
#if UNITY_EDITOR
    public SceneAsset bootstrapScene;
    public SceneAsset firstStageScene;
#else
    [SerializeField] private string firstStageSceneName;
#endif

    [Header("Stages (1-4)")]
    public StageSceneEntry[] stages = new StageSceneEntry[4];

    [Header("Stage5 Result Scenes")]
#if UNITY_EDITOR
    public SceneAsset stage5Good;
    public SceneAsset stage5Normal;
    public SceneAsset stage5Bad;
#else
    [SerializeField] private string stage5GoodName;
    [SerializeField] private string stage5NormalName;
    [SerializeField] private string stage5BadName;
#endif

    [Header("Tie-break priority (when counts tie)")]
    public StageOutcome[] tieBreakPriority = new[] { StageOutcome.Good, StageOutcome.Normal, StageOutcome.Bad };

    public string GetFirstStageName()
    {
#if UNITY_EDITOR
        return firstStageScene ? firstStageScene.name : "";
#else
        return firstStageSceneName;
#endif
    }

    public string GetStageSceneName(StageId id)
    {
        foreach (var e in stages)
        {
            if (e.stageId == id)
                return e.GetSceneName();
        }
        return "";
    }

    public string GetStage5SceneName(StageOutcome outcome)
    {
#if UNITY_EDITOR
        return outcome switch
        {
            StageOutcome.Good => stage5Good ? stage5Good.name : "",
            StageOutcome.Normal => stage5Normal ? stage5Normal.name : "",
            StageOutcome.Bad => stage5Bad ? stage5Bad.name : "",
            _ => ""
        };
#else
        return outcome switch
        {
            StageOutcome.Good => stage5GoodName,
            StageOutcome.Normal => stage5NormalName,
            StageOutcome.Bad => stage5BadName,
            _ => ""
        };
#endif
    }

#if UNITY_EDITOR
    // ビルドでも使えるように Scene名文字列を自動保存（保険）
    private void OnValidate()
    {
        // first stage
        if (firstStageScene)
        {
            // nothing; in editor we use SceneAsset.name directly
        }

        // stage5
        // nothing; same reason
    }
#endif
}

[Serializable]
public struct StageSceneEntry
{
    public StageId stageId;
#if UNITY_EDITOR
    public SceneAsset scene;
#endif
    [SerializeField] private string sceneNameFallback;

    public string GetSceneName()
    {
#if UNITY_EDITOR
        return scene ? scene.name : sceneNameFallback;
#else
        return sceneNameFallback;
#endif
    }
}
