using UnityEngine;
using UnityEngine.SceneManagement;

public class Stage5Router : MonoBehaviour
{
    [Header("Scene Names (must match Build Settings)")]
    [SerializeField] private string goodScene = "Stage5_Good";
    [SerializeField] private string normalScene = "Stage5_Normal";
    [SerializeField] private string badScene = "Stage5_Bad";

    private void Start()
    {
        var rs = ResultStore.Instance;
        if (rs == null)
        {
            Debug.LogError("[Stage5Router] ResultStore.Instance が見つかりません。Stage1にResultStoreがありDontDestroyOnLoadになっているか確認。");
            return;
        }

        Debug.Log($"[Stage5Router] Good={rs.Good}, Normal={rs.Normal}, Bad={rs.Bad}");

        // 一番多い結果に分岐（同点は Good→Normal→Bad 優先）
        if (rs.Good >= rs.Normal && rs.Good >= rs.Bad)
            SceneManager.LoadScene(goodScene);
        else if (rs.Normal >= rs.Good && rs.Normal >= rs.Bad)
            SceneManager.LoadScene(normalScene);
        else
            SceneManager.LoadScene(badScene);
    }
}
