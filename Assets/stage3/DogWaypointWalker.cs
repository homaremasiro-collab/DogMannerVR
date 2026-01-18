using UnityEngine;

public class DogWaypointWalker : MonoBehaviour
{
    [Header("Waypoints")]
    public Transform[] points;

    [Header("Movement")]
    public float speed = 1.2f;
    public float arriveDistance = 0.25f;

    private int index = 0;
    private bool paused = false;

    void Update()
    {
        if (paused) return;
        if (points == null || points.Length == 0) return;
        if (index >= points.Length) return;
        if (points[index] == null) return;

        Vector3 dir = points[index].position - transform.position;
        dir.y = 0f;

        if (dir.magnitude <= arriveDistance)
        {
            index++;
            return;
        }

        if (dir.sqrMagnitude > 0.0001f)
        {
            Quaternion rot = Quaternion.LookRotation(dir.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, rot, 8f * Time.deltaTime);
        }

        transform.position += transform.forward * speed * Time.deltaTime;
    }

    public void PauseWalk()
    {
        paused = true;
    }

    public void ResumeWalk()
    {
        paused = false;
    }
}
