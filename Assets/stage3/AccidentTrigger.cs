using UnityEngine;

public class AccidentTrigger : MonoBehaviour
{
    public DogAccidentReaction dog;

    void OnTriggerEnter(Collider other)
    {
        if (!other.name.Contains("Player")) return; // ざっくりでもOK
        if (dog) dog.StartAccident();
    }
}
