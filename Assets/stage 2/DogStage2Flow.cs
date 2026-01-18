using System.Collections;
using UnityEngine;

public class DogStage2Flow : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Animator animator;

    [Tooltip("歩かせる系スクリプトを入れる（空腹/食事中は止める）")]
    [SerializeField] private MonoBehaviour[] disableMoveScripts;

    [Header("Food Spot (show only when hungry)")]
    [SerializeField] private GameObject foodSpotRoot;

    [Header("Animator Trigger Names")]
    [SerializeField] private string hungryTrigger = "Hungry";
    [SerializeField] private string startEatTrigger = "StartEat";
    [SerializeField] private string reactGoodTrigger = "ReactGood";
    [SerializeField] private string reactBadTrigger = "ReactBad";

    [Header("Animator State Names (Base Layer) ※デバッグ表示用/保険")]
    [SerializeField] private string hungryStateName = "scared_pose";
    [SerializeField] private string eatStateName = "eat";
    [SerializeField] private string goodStateName = "happy";
    [SerializeField] private string badStateName = "nervous";

    [Header("Timing")]
    [SerializeField] private float eatSeconds = 2.0f;

    [Header("Options")]
    [SerializeField] private bool useExternalHungrySignal = true;

    [Header("Debug")]
    [SerializeField] private bool verboseLog = true;

    public bool CanMove { get; private set; } = true;

    private bool isHungry = false;
    private bool isEating = false;

    private void Awake()
    {
        if (!animator) animator = GetComponentInChildren<Animator>();
        SetFoodSpotVisible(false);
    }

    public void OnDogBecameHungry()
    {
        if (isHungry || isEating) return;

        isHungry = true;
        SetFoodSpotVisible(true);

        CanMove = false;
        SetMoveScriptsEnabled(false);

        if (animator)
        {
            animator.ResetTrigger(startEatTrigger);
            animator.ResetTrigger(reactGoodTrigger);
            animator.ResetTrigger(reactBadTrigger);
            animator.SetTrigger(hungryTrigger);
        }

        if (verboseLog) Debug.Log("[DogStage2Flow] Dog became hungry -> Trigger Hungry (FoodSpot ON)");
    }

    public void OnFoodPlaced(FoodType type, bool isSafe)
    {
        if (!isHungry || isEating)
        {
            if (verboseLog) Debug.Log($"[DogStage2Flow] Ignore food (hungry={isHungry}, eating={isEating})");
            return;
        }

        if (verboseLog) Debug.Log($"[DogStage2Flow] Food placed: {type}, safe={isSafe}");
        SetFoodSpotVisible(false);

        StartCoroutine(EatAndReact(isSafe));
    }

    private IEnumerator EatAndReact(bool isSafe)
    {
        isEating = true;

        if (animator) animator.SetTrigger(startEatTrigger);
        if (verboseLog) Debug.Log("[DogStage2Flow] Trigger StartEat");

        CanMove = false;
        SetMoveScriptsEnabled(false);

        yield return new WaitForSeconds(eatSeconds);

        // ★ Stage2：ここが確定点（Safe=Good / Unsafe=Bad）
        if (isSafe) ResultStore.Instance?.AddGood();
        else ResultStore.Instance?.AddBad();

        if (animator)
        {
            animator.SetTrigger(isSafe ? reactGoodTrigger : reactBadTrigger);
        }
        if (verboseLog) Debug.Log($"[DogStage2Flow] Trigger React {(isSafe ? "Good" : "Bad")}");

        yield return new WaitForSeconds(0.3f);

        isHungry = false;
        isEating = false;

        CanMove = true;
        SetMoveScriptsEnabled(true);

        if (verboseLog) Debug.Log("[DogStage2Flow] Flow done -> resume walking");
    }

    private void SetMoveScriptsEnabled(bool enabled)
    {
        if (disableMoveScripts == null) return;
        foreach (var m in disableMoveScripts)
        {
            if (!m) continue;
            m.enabled = enabled;
        }
    }

    private void SetFoodSpotVisible(bool visible)
    {
        if (!foodSpotRoot) return;
        if (foodSpotRoot.activeSelf != visible)
            foodSpotRoot.SetActive(visible);
    }
}
