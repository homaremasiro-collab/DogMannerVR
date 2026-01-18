using UnityEngine;

public class ResultStore : MonoBehaviour
{
    public static ResultStore Instance { get; private set; }

    [Header("Counts")]
    [SerializeField] private int good;
    [SerializeField] private int normal;
    [SerializeField] private int bad;

    public int Good => good;
    public int Normal => normal;
    public int Bad => bad;

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

    public void AddGood(int n = 1)
    {
        good += n;
        Debug.Log($"[ResultStore] Good={good}, Normal={normal}, Bad={bad}");
    }

    public void AddNormal(int n = 1)
    {
        normal += n;
        Debug.Log($"[ResultStore] Good={good}, Normal={normal}, Bad={bad}");
    }

    public void AddBad(int n = 1)
    {
        bad += n;
        Debug.Log($"[ResultStore] Good={good}, Normal={normal}, Bad={bad}");
    }

    public void ResetAll()
    {
        good = normal = bad = 0;
        Debug.Log("[ResultStore] ResetAll");
    }
}
