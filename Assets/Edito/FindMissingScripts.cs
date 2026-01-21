#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class FindMissingScripts
{
    [MenuItem("Tools/Find Missing Scripts (Open Scenes)")]
    public static void FindInOpenScenes()
    {
        int count = 0;
        var all = Object.FindObjectsOfType<GameObject>(true);

        foreach (var go in all)
        {
            var monos = go.GetComponents<MonoBehaviour>();
            for (int i = 0; i < monos.Length; i++)
            {
                if (monos[i] == null)
                {
                    count++;
                    Debug.LogWarning($"[MissingScript] {GetPath(go)}", go);
                }
            }
        }

        Debug.Log($"Done. Missing scripts found: {count}");
    }

    private static string GetPath(GameObject go)
    {
        string path = go.name;
        var t = go.transform;
        while (t.parent != null)
        {
            t = t.parent;
            path = t.name + "/" + path;
        }
        return path;
    }
}
#endif
