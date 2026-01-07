using System.Collections;
using UnityEngine;

public class DogHungerAfterWalk : MonoBehaviour
{
    [Header("Refs")]
    public Transform xrOrigin;
    public DogStage2Flow stageFlow;   // ★追加

    [Header("Settings")]
    public float startMoveThreshold = 0.15f;
    public float secondsAfterStart = 2.5f;

    Vector3 startPos;
    bool startedWalking = false;
    bool fired = false;

    void Start()
    {
        startPos = xrOrigin.position;

        if (stageFlow == null)
            stageFlow = FindObjectOfType<DogStage2Flow>();
    }

    void Update()
    {
        if (fired) return;

        if (!startedWalking)
        {
            float moved = Vector3.Distance(startPos, xrOrigin.position);
            if (moved >= startMoveThreshold)
            {
                startedWalking = true;
                StartCoroutine(HungerSequence());
            }
        }
    }

    IEnumerator HungerSequence()
    {
        yield return new WaitForSeconds(secondsAfterStart);
        fired = true;

        // ★ Animatorは触らない！
        if (stageFlow != null)
            stageFlow.OnDogBecameHungry();

        Debug.Log("Dog became hungry → StageFlowに通知");
    }
}
