using UnityEngine;

public class FoodSpotTrigger : MonoBehaviour
{
    [SerializeField] private DogStage2Flow stageFlow;
    [SerializeField] private bool destroyFoodOnPlace = true;

    private void Awake()
    {
        if (!stageFlow) stageFlow = FindObjectOfType<DogStage2Flow>();
    }

    private void OnTriggerEnter(Collider other)
    {
        var food = other.GetComponentInParent<FoodData>();
        if (food == null) return;

        stageFlow?.OnFoodPlaced(food.type, food.isSafe);

        if (destroyFoodOnPlace)
            Destroy(food.gameObject);
    }
}
