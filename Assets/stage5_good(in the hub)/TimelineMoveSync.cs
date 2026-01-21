using UnityEngine;
using UnityEngine.Playables;

public class TimelineMoveSync : MonoBehaviour
{
    [Header("Timeline")]
    public PlayableDirector director;

    [Header("Points")]
    public Transform runStart;
    public Transform runEnd;
    public Transform walkEnd;

    [Header("Times (seconds on Timeline)")]
    public double runStartTime = 0.0;
    public double runEndTime   = 2.0;
    public double walkStartTime = 3.0;
    public double walkEndTime   = 6.0;

    [Header("Rotation")]
    public bool faceRunEnd = true;   // run中、runEnd方向を向く
    public bool faceWalkEnd = true;  // walk中、walkEnd方向を向く

    void Reset()
    {
        director = FindObjectOfType<PlayableDirector>();
    }

    void LateUpdate()
    {
        if (!director || !runStart || !runEnd || !walkEnd) return;

        double t = director.time;

        // Run区間：runStart -> runEnd
        if (t >= runStartTime && t <= runEndTime)
        {
            float u = (float)((t - runStartTime) / (runEndTime - runStartTime));
            transform.position = Vector3.Lerp(runStart.position, runEnd.position, u);
            if (faceRunEnd) LookAtFlat(runEnd.position);
            return;
        }

        // Happy区間：runEndで固定（必要なら）
        if (t > runEndTime && t < walkStartTime)
        {
            transform.position = runEnd.position;
            return;
        }

        // Walk区間：runEnd -> walkEnd
        if (t >= walkStartTime && t <= walkEndTime)
        {
            float u = (float)((t - walkStartTime) / (walkEndTime - walkStartTime));
            transform.position = Vector3.Lerp(runEnd.position, walkEnd.position, u);
            if (faceWalkEnd) LookAtFlat(walkEnd.position);
            return;
        }

        // 終了後：walkEndに固定
        if (t > walkEndTime)
        {
            transform.position = walkEnd.position;
        }
    }

    void LookAtFlat(Vector3 target)
    {
        Vector3 p = target;
        p.y = transform.position.y;
        Vector3 dir = (p - transform.position);
        if (dir.sqrMagnitude > 0.0001f)
            transform.rotation = Quaternion.LookRotation(dir.normalized, Vector3.up);
    }
}
