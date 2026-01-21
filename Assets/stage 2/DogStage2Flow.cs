using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class DogStage2Flow : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Animator animator;
    [SerializeField] private MonoBehaviour[] disableMoveScripts;

    [Header("Food Spot (show only when hungry)")]
    [SerializeField] private GameObject foodSpotRoot;

    [Header("Next Stage Gate (show when stage2 done)")]
    [SerializeField] private NextStageGate nextStageGate;

    [Header("Animator Trigger Names")]
    [SerializeField] private string hungryTrigger = "Hungry";
    [SerializeField] private string startEatTrigger = "StartEat";
    [SerializeField] private string reactGoodTrigger = "ReactGood";
    [SerializeField] private string reactBadTrigger = "ReactBad";

    [Header("Animator State Names (Base Layer)")]
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

    // ★追加：NavMeshAgent を使ってるなら自動で掴む（無くてもOK）
    private NavMeshAgent agent;

    public bool CanMove { get; private set; } = true;

    private bool isHungry = false;
    private bool isEating = false;
    private bool stage2Decided = false;

    private void Awake()
    {
        if (!animator) animator = GetComponentInChildren<Animator>();
        agent = GetComponentInChildren<NavMeshAgent>(); // ★子に付いてても拾う
        SetFoodSpotVisible(false);

        if (nextStageGate != null) nextStageGate.DisableGate();
    }

    public void OnDogBecameHungry()
    {
        if (!useExternalHungrySignal) return;
        if (isHungry || isEating) return;

        isHungry = true;
        SetFoodSpotVisible(true);

        CanMove = false;
        SetMoveScriptsEnabled(false);
        ForceAgentStop(true);

        if (animator)
        {
            animator.ResetTrigger(startEatTrigger);
            animator.ResetTrigger(reactGoodTrigger);
            animator.ResetTrigger(reactBadTrigger);
            animator.SetTrigger(hungryTrigger);
        }

        if (verboseLog) Debug.Log("[DogStage2Flow] Dog became hungry -> Hungry");
    }

    public void OnFoodPlaced(FoodType type, bool isSafe)
    {
        if (!isHungry || isEating) return;

        if (verboseLog) Debug.Log($"[DogStage2Flow] Food placed: {type}, safe={isSafe}");
        SetFoodSpotVisible(false);

        StartCoroutine(EatAndReact(isSafe));
    }

    private IEnumerator EatAndReact(bool isSafe)
    {
        isEating = true;

        // 食べる
        if (animator) animator.SetTrigger(startEatTrigger);
        if (verboseLog) Debug.Log("[DogStage2Flow] Trigger StartEat");

        CanMove = false;
        SetMoveScriptsEnabled(false);
        ForceAgentStop(true);

        yield return new WaitForSeconds(eatSeconds);

        // 結果加算
     
        // ゲート出す（1回）
        if (!stage2Decided)
        {
            stage2Decided = true;
            if (nextStageGate != null) nextStageGate.EnableGate();
        }

        // 反応
        if (animator)
            animator.SetTrigger(isSafe ? reactGoodTrigger : reactBadTrigger);

        if (verboseLog) Debug.Log($"[DogStage2Flow] Trigger React {(isSafe ? "Good" : "Bad")}");

        // ★ここが修繕ポイント：
        // 反応ステートに入って、終わるまで待ってから移動再開する
        string reactState = isSafe ? goodStateName : badStateName;

        yield return WaitEnterState(animator, reactState, 1.0f);
        yield return WaitStateFinish(animator, 0, 3.0f); // 3秒まで待つ（足りなければ増やす）

        // 移動再開
        isHungry = false;
        isEating = false;

        CanMove = true;
        ForceAgentStop(false);     // ★NavMeshAgentが止まってたら解除
        SetMoveScriptsEnabled(true);

        if (verboseLog) Debug.Log("[DogStage2Flow] React finished -> resume walking");
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

    // ★NavMeshAgent の停止/再開（使ってないなら無視される）
    private void ForceAgentStop(bool stop)
    {
        if (!agent) return;
        agent.isStopped = stop;
        if (!stop)
        {
            // まれに停止解除しても動かないケースの保険
            agent.ResetPath();
        }
    }

    // ===== Animator待機ヘルパ =====
    private IEnumerator WaitEnterState(Animator a, string stateName, float timeout)
    {
        if (!a) yield break;
        float t = 0f;
        while (t < timeout)
        {
            var st = a.GetCurrentAnimatorStateInfo(0);
            if (st.IsName(stateName)) yield break;
            t += Time.deltaTime;
            yield return null;
        }
    }

    private IEnumerator WaitStateFinish(Animator a, int layer, float timeout)
    {
        if (!a) yield break;
        float t = 0f;
        while (t < timeout)
        {
            var st = a.GetCurrentAnimatorStateInfo(layer);
            // ループじゃない前提：normalizedTimeが1に近づいたら終了扱い
            if (st.normalizedTime >= 0.98f) yield break;
            t += Time.deltaTime;
            yield return null;
        }
    }
}
