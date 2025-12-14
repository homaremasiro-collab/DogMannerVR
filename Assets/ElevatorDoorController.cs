using UnityEngine;

public class ElevatorDoorController : MonoBehaviour
{
    Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        // テスト用：キーボードの E キーでドアを開く
        if (Input.GetKeyDown(KeyCode.E))
        {
            animator.SetTrigger("OpenDoor");
        }
    }
}
