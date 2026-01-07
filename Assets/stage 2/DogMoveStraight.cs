using UnityEngine;

public class DogMoveStraight : MonoBehaviour
{
    [SerializeField] private Transform dogRoot;  // いぬ本体(動かしたいTransform)
    [SerializeField] private float speed = 1.0f;
    [SerializeField] private Vector3 worldDirection = Vector3.forward; // Z+へ進む

    [Header("Optional: controlled by DogStage2Flow")]
    [SerializeField] private DogStage2Flow flow;
    [SerializeField] private bool moveOnlyWhenFlowSays = true;

    private void Awake()
    {
        if (!dogRoot) dogRoot = transform;
        if (!flow) flow = FindObjectOfType<DogStage2Flow>();
        worldDirection = worldDirection.normalized;
    }

    private void Update()
    {
        if (moveOnlyWhenFlowSays && flow != null && !flow.CanMove)
            return;

        dogRoot.position += worldDirection * speed * Time.deltaTime;
    }
}
