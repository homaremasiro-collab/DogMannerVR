using UnityEngine;

public class MovieDirector : MonoBehaviour
{
    public MovieDogMove mover;
    public Transform lookTarget; // カメラを入れる

    public float runTime = 2.0f;
    public float happyTime = 1.5f;

    void Start()
    {
        // 走って近づく
        mover.move = true;
        if (lookTarget) mover.transform.LookAt(new Vector3(lookTarget.position.x, mover.transform.position.y, lookTarget.position.z));

        Invoke(nameof(StopMove), runTime);
        Invoke(nameof(WalkAway), runTime + happyTime);
    }

    void StopMove()
    {
        mover.move = false;
    }

    void WalkAway()
    {
        // 180度回して去る
        mover.transform.Rotate(0f, 180f, 0f);
        mover.move = true;
    }
}
