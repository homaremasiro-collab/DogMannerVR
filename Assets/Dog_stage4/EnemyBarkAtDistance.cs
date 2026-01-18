using UnityEngine;

public class EnemyBarkAtDistance : MonoBehaviour
{
    public Transform playerDog;
    public DogArousalController playerArousal;
    public float barkDistance = 2.0f;
    bool barked = false;

    void Update()
    {
        if (playerDog == null || playerArousal == null) return;

        float d = Vector3.Distance(transform.position, playerDog.position);

        if (!barked && d < barkDistance)
        {
            barked = true;
            playerArousal.StartBark();
            Debug.Log("Enemy Bark!");
        }

        if (barked && d > barkDistance * 1.5f)
        {
            barked = false; // 離れたら再発可
        }
    }
}
