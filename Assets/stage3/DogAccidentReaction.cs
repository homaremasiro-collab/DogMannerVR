using System.Collections;
using UnityEngine;

public class DogAccidentReaction : MonoBehaviour
{
    public enum ResultAction
    {
        None,
        SootheBad,      // △
        LeaveGood,      // ◎
        WaitWorst       // ×
    }

    [Header("Refs")]
    public Animator dogAnimator;
    public DogWaypointWalker walker;
    public AudioSource accidentAudio;
    public Transform accidentPoint;
    public Transform soundSource;

    [Header("Animator Triggers (Parameters名に合わせる)")]
    public string barkTriggerName = "Excited";
    public string happyTriggerName = "Happy";
    public string calmTriggerName = "Calm";
    public string angryTriggerName = "Angry";

    [Header("Timing")]
    public float barkSeconds = 1.2f;
    public float actionTimeLimit = 6.0f;
    public float afterResultDelay = 0.8f;

    [Header("見やすさ調整（短いアニメでも見せる）")]
    public float resultHoldSeconds = 1.2f;

    [Header("Turn / Lead")]
    public float turnSpeed = 10f;
    public float leadMoveSpeed = 2.0f;
    public float safeDistanceFromAccident = 4.0f;

    [Header("Soothe Detect")]
    public float sootheNeedSeconds = 1.0f;
    public float sootheMaxHandSpeed = 0.35f;

    [Header("Debug")]
    public bool debugLog = true;

    bool _reacting;
    bool _acceptingAction;
    float _timer;
    float _headTouchTime;

    ResultAction _result = ResultAction.None;

    PlayerHand _leadingHand;
    PlayerHand _lastHand;
    DogTouchZone.ZoneType _lastZone;

    Coroutine _flow;

    void Awake()
    {
        if (!dogAnimator) dogAnimator = GetComponent<Animator>();
        if (!soundSource) soundSource = transform;
        if (!accidentPoint && soundSource) accidentPoint = soundSource;
    }

    public void StartAccident()
    {
        if (_reacting) return;
        _flow = StartCoroutine(AccidentFlow());
    }

    IEnumerator AccidentFlow()
    {
        _reacting = true;
        _result = ResultAction.None;

        _leadingHand = null;
        _lastHand = null;
        _headTouchTime = 0f;

        if (walker) walker.PauseWalk();

        if (debugLog) Debug.Log("[DogAccidentReaction] Accident start");

        if (accidentAudio) accidentAudio.Play();

        yield return StartCoroutine(FaceAccidentPoint(0.35f));

        if (dogAnimator && !string.IsNullOrEmpty(barkTriggerName))
        {
            dogAnimator.SetTrigger(barkTriggerName);
            if (debugLog) Debug.Log("[DogAccidentReaction] Trigger bark");
        }

        yield return new WaitForSeconds(barkSeconds);

        _acceptingAction = true;
        _timer = 0f;
        _headTouchTime = 0f;

        while (_acceptingAction)
        {
            _timer += Time.deltaTime;

            if (_leadingHand != null)
            {
                Vector3 target = _leadingHand.transform.position;
                target.y = transform.position.y;
                transform.position = Vector3.MoveTowards(transform.position, target, leadMoveSpeed * Time.deltaTime);

                if (accidentPoint && Vector3.Distance(transform.position, accidentPoint.position) >= safeDistanceFromAccident)
                {
                    Decide(ResultAction.LeaveGood);
                    break;
                }
            }

            if (_timer >= actionTimeLimit)
            {
                Decide(ResultAction.WaitWorst);
                break;
            }

            yield return null;
        }

        PlayResultAnimation();

        if (resultHoldSeconds > 0f)
            yield return new WaitForSeconds(resultHoldSeconds);

        yield return new WaitForSeconds(afterResultDelay);

        if (walker) walker.ResumeWalk();

        if (debugLog) Debug.Log("[DogAccidentReaction] Accident end");

        _leadingHand = null;
        _lastHand = null;
        _reacting = false;
        _acceptingAction = false;
    }

    void PlayResultAnimation()
    {
        if (!dogAnimator) return;

        switch (_result)
        {
            case ResultAction.LeaveGood:
                if (!string.IsNullOrEmpty(happyTriggerName)) dogAnimator.SetTrigger(happyTriggerName);
                else if (!string.IsNullOrEmpty(calmTriggerName)) dogAnimator.SetTrigger(calmTriggerName);
                break;

            case ResultAction.SootheBad:
                if (!string.IsNullOrEmpty(calmTriggerName)) dogAnimator.SetTrigger(calmTriggerName);
                else if (!string.IsNullOrEmpty(angryTriggerName)) dogAnimator.SetTrigger(angryTriggerName);
                break;

            case ResultAction.WaitWorst:
                if (!string.IsNullOrEmpty(angryTriggerName)) dogAnimator.SetTrigger(angryTriggerName);
                else if (!string.IsNullOrEmpty(calmTriggerName)) dogAnimator.SetTrigger(calmTriggerName);
                break;

            default:
                if (!string.IsNullOrEmpty(calmTriggerName)) dogAnimator.SetTrigger(calmTriggerName);
                break;
        }

        if (debugLog) Debug.Log($"[DogAccidentReaction] Result => {_result}");
    }

    IEnumerator FaceAccidentPoint(float seconds)
    {
        if (!accidentPoint) yield break;

        float t = 0f;
        while (t < seconds)
        {
            t += Time.deltaTime;

            Vector3 dir = accidentPoint.position - transform.position;
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.0001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(dir);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * turnSpeed);
            }
            yield return null;
        }
    }

    void Decide(ResultAction action)
    {
        if (_result != ResultAction.None) return;

        _result = action;
        _acceptingAction = false;
        _leadingHand = null;

        // ★ Stage3：ここが確定点（◎=Good / △=Normal / ×=Bad）
        switch (_result)
        {
            case ResultAction.LeaveGood: ResultStore.Instance?.AddGood(); break;
            case ResultAction.SootheBad: ResultStore.Instance?.AddNormal(); break;
            case ResultAction.WaitWorst: ResultStore.Instance?.AddBad(); break;
        }

        if (debugLog) Debug.Log($"[DogAccidentReaction] Decide: {_result}");
    }

    // ---- Called from DogTouchZone ----
    public void OnHandStay(DogTouchZone.ZoneType zone, PlayerHand hand)
    {
        if (!_reacting) return;
        if (!hand) return;

        _lastHand = hand;
        _lastZone = zone;

        if (!_acceptingAction) return;

        if (zone == DogTouchZone.ZoneType.Collar)
        {
            if (hand.gripPressed)
            {
                _leadingHand = hand;
                if (debugLog) Debug.Log("[DogAccidentReaction] Leading start");
            }
            return;
        }

        if (zone == DogTouchZone.ZoneType.Head)
        {
            if (hand.speed <= sootheMaxHandSpeed)
                _headTouchTime += Time.deltaTime;
            else
                _headTouchTime = 0f;

            if (_headTouchTime >= sootheNeedSeconds)
            {
                Decide(ResultAction.SootheBad);
            }
            return;
        }
    }

    public void OnHandExit(DogTouchZone.ZoneType zone, PlayerHand hand)
    {
        if (!_reacting) return;
        if (!hand) return;

        if (zone == DogTouchZone.ZoneType.Collar)
        {
            if (_leadingHand == hand) _leadingHand = null;
        }

        if (zone == DogTouchZone.ZoneType.Head)
        {
            _headTouchTime = 0f;
        }
    }
}
