using UnityEngine;

public class PlayerState : MonoBehaviour
{
    public Transform head;
    public bool isCrouching;

    void Update()
    {
        isCrouching = head.position.y < 1.2f;
    }
}
