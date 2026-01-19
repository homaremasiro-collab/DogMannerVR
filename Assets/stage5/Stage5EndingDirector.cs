using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Stage5EndingDirector : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Animator dogAnimator;

    [Header("Animator Triggers")]
    [SerializeField] private string runTrigger = "Run";
    [SerializeField] private string happyTrigger = "Happy";
    [SerializeField] private string barkTrigger = "Bark";
    [SerializeField] private string walkBackTrigger = "WalkBack";

    [Header("Timing (seconds)")]
    [SerializeField] private float runSec = 2.0f;
    [SerializeField] private float happySec = 1.0f;
    [SerializeField] private float barkSec = 0.5f;
    [SerializeField] private float walkBackSec = 3.0f;

    [Header("Next Scene")]
    [SerializeField] private string nextScene = "Stage5_Summary"; // 無ければ空でもOK
    [SerializeField] private bool autoGoNext = true;

    private void Start()
    {
        if (!dogAnimator) dogAnimator = FindObjectOfType<Animator>();
        StartCoroutine(PlaySequence());
    }

    private IEnumerator PlaySequence()
    {
        // 走る
        if (!string.IsNullOrEmpty(runTrigger)) dogAnimator.SetTrigger(runTrigger);
        yield return new WaitForSeconds(runSec);

        // 嬉しい
        if (!string.IsNullOrEmpty(happyTrigger)) dogAnimator.SetTrigger(happyTrigger);
        yield return new WaitForSeconds(happySec);

        // 吠える
        if (!string.IsNullOrEmpty(barkTrigger)) dogAnimator.SetTrigger(barkTrigger);
        yield return new WaitForSeconds(barkSec);

        // 去る
        if (!string.IsNullOrEmpty(walkBackTrigger)) dogAnimator.SetTrigger(walkBackTrigger);
        yield return new WaitForSeconds(walkBackSec);

        if (autoGoNext && !string.IsNullOrEmpty(nextScene))
            SceneManager.LoadScene(nextScene);
    }
}
