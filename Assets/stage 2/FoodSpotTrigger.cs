using UnityEngine;

public class FoodSpotTrigger : MonoBehaviour
{
    [SerializeField] private DogStage2Flow dogFlow;

    private void Awake()
    {
        if (!dogFlow) dogFlow = FindObjectOfType<DogStage2Flow>();
    }

    private void OnTriggerEnter(Collider other)
    {
        // FoodDataが付いてる物だけ反応（親に付けてる場合もあるのでInParent）
        var data = other.GetComponentInParent<FoodData>();
        if (data == null) return;

        if (!dogFlow)
        {
            Debug.LogError("[FoodSpotTrigger] DogStage2Flow が見つかりません");
            return;
        }

        // ★ここが重要：type と isSafe を渡す
        dogFlow.OnFoodPlaced(data.type, data.isSafe);

        Debug.Log($"[FoodSpotTrigger] Food detected: {data.type}, safe={data.isSafe}");
    }
}
