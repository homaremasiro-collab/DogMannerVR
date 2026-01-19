using UnityEngine;

public class MovieDogMove : MonoBehaviour
{
    public float speed = 1.5f;
    public bool move = false;

    void Update()
    {
        if (move) transform.position += transform.forward * speed * Time.deltaTime;
    }
}
