// StageCompleteCaller.cs
using UnityEngine;

public class StageCompleteCaller : MonoBehaviour
{
    [SerializeField] private StageId stageId;
    [SerializeField] private StageOutcome outcome;

    // UI ButtonのOnClickから呼べる
    public void Complete()
    {
        if (GameFlow.Instance == null)
        {
            Debug.LogError("[StageCompleteCaller] GameFlow.Instance が見つかりません（Bootstrap起動してますか？）");
            return;
        }
        GameFlow.Instance.CompleteStage(stageId, outcome);
    }
}
