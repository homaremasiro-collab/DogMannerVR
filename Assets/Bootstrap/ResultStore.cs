// ResultStore.cs
using UnityEngine;

public class ResultStore : MonoBehaviour
{
    public static ResultStore Instance { get; private set; }

    public int Good { get; private set; }
    public int Normal { get; private set; }
    public int Bad { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        ResetAll();
    }

    public void ResetAll()
    {
        Good = 0;
        Normal = 0;
        Bad = 0;
    }

    public void Add(StageOutcome o)
    {
        switch (o)
        {
            case StageOutcome.Good: Good++; break;
            case StageOutcome.Normal: Normal++; break;
            case StageOutcome.Bad: Bad++; break;
        }
        Debug.Log($"[ResultStore] Good={Good} Normal={Normal} Bad={Bad}");
    }

    public StageOutcome GetFinalOutcome(StageOutcome[] tieBreakPriority)
    {
        // max値
        int max = Good;
        if (Normal > max) max = Normal;
        if (Bad > max) max = Bad;

        // 同点候補
        bool goodTop = Good == max;
        bool normalTop = Normal == max;
        bool badTop = Bad == max;

        // tie-break
        foreach (var p in tieBreakPriority)
        {
            if (p == StageOutcome.Good && goodTop) return StageOutcome.Good;
            if (p == StageOutcome.Normal && normalTop) return StageOutcome.Normal;
            if (p == StageOutcome.Bad && badTop) return StageOutcome.Bad;
        }

        // fallback
        return StageOutcome.Good;
    }
}
