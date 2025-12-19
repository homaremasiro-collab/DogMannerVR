using UnityEngine;

public class FoodSpotTrigger : MonoBehaviour
{
    [Header("Refs")]
    public Animator dogAnimator;      // 犬のAnimator
    public GameObject hungryUI;       // 空腹UI（消したい）
    public string eatTriggerName = "Eat";

    [Header("Optional")]
    public bool destroyFood = true;   // 食べ物を消すか

    private bool alreadyFed = false;

    private void OnTriggerEnter(Collider other)
    {
        if (alreadyFed) return;

        // 食べ物タグの物だけ反応
        if (!other.CompareTag("Food")) return;

        alreadyFed = true;

        // 犬が食べる
        if (dogAnimator != null)
            dogAnimator.SetTrigger(eatTriggerName);

        // UIを消す
        if (hungryUI != null)
            hungryUI.SetActive(false);

        // 食べ物は消す（好みで）
        if (destroyFood)
            Destroy(other.gameObject);
    }
}
