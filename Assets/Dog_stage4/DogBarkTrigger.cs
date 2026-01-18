using UnityEngine;

public class DogBarkTrigger : MonoBehaviour
{
    public DogArousalController playerDog;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerDog.StartBark();
        }
    }
}
