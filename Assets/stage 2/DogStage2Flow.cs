using System.Collections;
using UnityEngine;

public class DogStage2Flow : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Animator animator;

    [Tooltip("歩かせる系スクリプトを入れる（空腹/食事中は止める）")]
    [SerializeField] private MonoBehaviour[] disableMoveScripts;

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

    // 外から DogMoveStraight が参照する
    public bool CanMove { get; private set; } = true;

    private bool isHungry = false;
    private bool isEating = false;

    private void Awake()
    {
        if (!animator) animator = GetComponentInChildren<Animator>();
    }

    // DogHungerAfterWalk / DogWalkThenHungry などから呼ぶ想定
    public void OnDogBecameHungry()
    {
        if (isHungry || isEating) return;

        isHungry = true;
        CanMove = false;
        SetMoveScriptsEnabled(false);

        if (animator)
        {
            animator.ResetTrigger(startEatTrigger);
            animator.ResetTrigger(reactGoodTrigger);
            animator.ResetTrigger(reactBadTrigger);
            animator.SetTrigger(hungryTrigger);
        }

        if (verboseLog) Debug.Log("[DogStage2Flow] Dog became hungry -> Trigger Hungry");
    }

    // FoodSpotTrigger から呼ぶ
    public void OnFoodPlaced(FoodType type, bool isSafe)
    {
        if (!isHungry || isEating)
        {
            if (verboseLog) Debug.Log($"[DogStage2Flow] Ignore food (hungry={isHungry}, eating={isEating})");
            return;
        }

        if (verboseLog) Debug.Log($"[DogStage2Flow] Food placed: {type}, safe={isSafe}");

        StartCoroutine(EatAndReact(isSafe));
    }

    private IEnumerator EatAndReact(bool isSafe)
    {
        isEating = true;

        // 食べ始め
        if (animator) animator.SetTrigger(startEatTrigger);
        if (verboseLog) Debug.Log("[DogStage2Flow] Trigger StartEat");

        // 食べてる間は止める
        CanMove = false;
        SetMoveScriptsEnabled(false);

        yield return new WaitForSeconds(eatSeconds);

        // 反応
        if (animator)
        {
            animator.SetTrigger(isSafe ? reactGoodTrigger : reactBadTrigger);
        }
        if (verboseLog) Debug.Log($"[DogStage2Flow] Trigger React {(isSafe ? "Good" : "Bad")}");

        // ちょい待ってから歩き再開（反応が見えるように）
        yield return new WaitForSeconds(0.3f);

        // 次へ進むため歩き再開
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
}
