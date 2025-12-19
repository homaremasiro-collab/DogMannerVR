using System.Collections;
using UnityEngine;

public class DogHungerAfterWalk : MonoBehaviour
{
    [Header("Refs")]
    public Transform xrOrigin;
    public Animator dogAnimator;
    public AudioSource hungerAudio;
    public GameObject hungerUI;

    [Header("Settings")]
    public float startMoveThreshold = 0.15f;
    public float secondsAfterStart = 2.5f;
    public string scaredTriggerName = "Scared"; // ← ここ重要

    Vector3 startPos;
    bool startedWalking = false;
    bool fired = false;

    void Start()
    {
        startPos = xrOrigin.position;
        if (hungerUI != null) hungerUI.SetActive(false);
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

        // 怖がるアニメーション
        if (dogAnimator != null)
            dogAnimator.SetTrigger(scaredTriggerName);

        // お腹の音（なくてもOK）
        if (hungerAudio != null)
            hungerAudio.Play();

        // UI表示（任意）
        if (hungerUI != null)
            hungerUI.SetActive(true);

        Debug.Log("Dog is scared & hungry → 食べ物選択へ");
    }
}
