using System.Collections;
using UnityEngine;
using UnityEngine.AI;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class BarkSequenceController : MonoBehaviour
{
    [Header("Refs (必須)")]
    public PlayerDogAutoWalk playerWalk;
    public Animator playerAnimator;
    public Animator enemyAnimator;
    public NavMeshAgent enemyAgent;

    [Header("Points（犬同士距離/向き）")]
    public Transform playerPoint;
    public Transform enemyPoint;

    [Header("Owner (飼い主)")]
    public Transform head; // ★Main Camera（HMD）推奨

    [Header("Audio (呼びかけSE)")]
    public AudioSource callAudioSource;
    public AudioClip callSfx;
    [Range(0f, 1f)] public float callVolume = 0.8f;

    [Header("Call Input")]
    public KeyCode callKey = KeyCode.J; // Editor確認用
#if ENABLE_INPUT_SYSTEM
    public InputActionReference callAction; // Quest用（入っていれば優先）
#endif

    [Header("Cooldown")]
    public float cooldownSeconds = 1.0f;

    [Header("Bark Trigger")]
    public float triggerDistance = 2.0f;

    [Header("Bark Timing")]
    public float enemyBarkLeadTime = 0.8f;
    public float faceSpeed = 8.0f;

    [Header("Bark Animation")]
    public string barkStateName = "Bark";
    [Range(0f, 1f)] public float enemyBarkStartNormalized = 0.0f;
    [Range(0f, 1f)] public float playerBarkStartNormalized = 0.25f;

    [Header("Decision Window（入力受付時間）")]
    [Tooltip("吠えホールド中の入力受付時間。この間は確定せず、時間切れで Good/Normal/Bad を確定する")]
    public float responseWindowSeconds = 2.5f;

    [Header("GOOD条件（呼びかけ必須 + 視線外し）")]
    public float goodLookAwayAngle = 15f;
    public float goodHoldSeconds = 0.25f;

    [Header("Release Timing")]
    public float normalReleaseDelay = 0.5f;
    public float badExtraHoldSeconds = 1.0f;

    [Header("Reaction Facing（Happy/Calm中は「こちら」を向かせる）")]
    public float reactionFaceSpeed = 12f;
    public float happyHoldSeconds = 0.8f;
    public float calmHoldSeconds  = 0.6f;
    public float angryHoldSeconds = 0.8f;

    [Header("Turn Animation（反応後に元の向きへ戻る際に振り返りを入れる）")]
    public string trigTurn = "DoTurn";      // AnimatorのTrigger名
    public float turnClipSeconds = 0.95f;   // だいたいでOK（あなたのturnは0.958秒）

    [Header("Animator Params")]
    public string paramIsBarking = "IsBarking";
    public string trigHappy = "DoHappy";
    public string trigCalm  = "DoCalm";
    public string trigAngry = "DoAngry";

    [Header("Debug")]
    public bool debugLog = true;

    private Transform PlayerT => playerPoint != null ? playerPoint : playerWalk.transform;
    private Transform EnemyT  => enemyPoint  != null ? enemyPoint  : enemyAgent.transform;

    private Transform _enemyRoot;
    private bool _enemyUpdateRotationBackup;

    private bool _barking;
    private bool _cooldown;
    private bool _playerBarkStarted;
    private bool _resolvedOnce = false;

    // ★反応中（Happy/Calm/Angry演出中）は “こちら向きモード”
    private bool _reacting;

    private float _holdStartTime;

    // 入力状態
    private bool _called;
    private bool _callSoundPlayed;
    private float _goodTimer;

    // Good達成保持（達成しても即反応しない）
    private bool _goodAchieved;

    // 受付時間が終わったか
    private bool _windowEnded;

    // 向き復元用（反応後に「元の進行方向」に戻す）
    private Quaternion _playerRotBeforeBark;
    private Quaternion _enemyRotBeforeBark;

    private Coroutine _seq;

    private enum Decision { None, Good, Normal, Bad }
    private Decision _decision = Decision.None;

    private void Awake()
    {
        _enemyRoot = enemyAgent != null ? enemyAgent.transform : transform;
    }

    private void OnEnable()
    {
#if ENABLE_INPUT_SYSTEM
        if (callAction != null) callAction.action.Enable();
#endif
    }

    private void OnDisable()
    {
#if ENABLE_INPUT_SYSTEM
        if (callAction != null) callAction.action.Disable();
#endif
    }

    private bool Ready()
    {
        return playerWalk != null
            && playerAnimator != null
            && enemyAnimator != null
            && enemyAgent != null
            && head != null;
    }

    private void Update()
    {
        if (!Ready())
        {
            if (debugLog) Debug.LogWarning("[BarkSequenceController] Refs不足（head/anim/agent等）");
            return;
        }

        if (_resolvedOnce) return;

        float dogDist = Vector3.Distance(PlayerT.position, EnemyT.position);

        if (_barking)
        {
            KeepStoppedEveryFrame();

            // ★反応中は「こちら（head）」を見る。それ以外は犬同士を見る
            if (_reacting) FaceOwnerForReaction();
            else FaceEachOther();

            EvaluateDecisionWindow();
            return;
        }

        if (_cooldown) return;

        if (dogDist <= triggerDistance)
        {
            // 開始ガードは Sequence 内の _barking/_cooldown で止まるのでこれでOK
            _seq = StartCoroutine(Sequence());
        }
    }

    private IEnumerator Sequence()
    {
        if (_barking || _cooldown) yield break;

        _barking = true;
        _reacting = false;
        _playerBarkStarted = false;

        // 向き保存（最後にここへ戻して歩かせる）
        _playerRotBeforeBark = playerWalk.transform.rotation;
        _enemyRotBeforeBark  = _enemyRoot.rotation;

        // 判定初期化
        _decision = Decision.None;
        _holdStartTime = Time.time;
        _windowEnded = false;

        _called = false;
        _callSoundPlayed = false;
        _goodTimer = 0f;
        _goodAchieved = false;

        EnterStop();

        // enemy bark
        yield return StartCoroutine(FaceTowardsWithHardStop(_enemyRoot, PlayerT.position, faceSpeed));
        enemyAnimator.SetBool(paramIsBarking, true);
        enemyAnimator.Play(barkStateName, 0, enemyBarkStartNormalized);

        if (debugLog) Debug.Log("[Bark] Enemy bark ON");

        yield return new WaitForSeconds(enemyBarkLeadTime);

        // player bark（最初は敵犬を見る）
        yield return StartCoroutine(FaceTowardsWithHardStop(playerWalk.transform, EnemyT.position, faceSpeed));
        playerAnimator.SetBool(paramIsBarking, true);
        playerAnimator.Play(barkStateName, 0, playerBarkStartNormalized);

        _playerBarkStarted = true;
        if (debugLog) Debug.Log("[Bark] Player bark ON");

        _seq = null;
    }

    // -----------------------
    // 入力受付時間の評価（確定は時間切れで1回だけ）
    // -----------------------
    private void EvaluateDecisionWindow()
    {
        // 反応中は入力判定しない
        if (_reacting) return;

        float now = Time.time;
        float elapsed = now - _holdStartTime;

        // 受付時間内：材料を集める（確定しない）
        if (!_windowEnded && elapsed < responseWindowSeconds)
        {
            if (!_called && IsCallPressedThisFrame())
            {
                _called = true;

                if (!_callSoundPlayed)
                {
                    PlayCallSfx();
                    _callSoundPlayed = true;
                }

                if (debugLog) Debug.Log("[Decision] Call registered");
            }

            // Good判定：呼びかけ必須 + 視線外し維持（達成しても即反応しない）
            if (_called && !_goodAchieved)
            {
                float lookAway = GetLookAwayAngleDeg();
                bool lookAwayOk = lookAway >= goodLookAwayAngle;

                if (lookAwayOk)
                {
                    _goodTimer += Time.deltaTime;
                    if (_goodTimer >= goodHoldSeconds)
                    {
                        _goodAchieved = true;
                        if (debugLog) Debug.Log("[Decision] Good achieved (resolve at time up)");
                    }
                }
                else
                {
                    _goodTimer = 0f;
                }
            }

            return;
        }

        // 受付時間が終わった：ここで一回だけ確定して反応開始
       // 受付時間が終わった：ここで一回だけ確定して反応開始
if (_windowEnded) return;
_windowEnded = true;

if (_decision != Decision.None) return;

if (_goodAchieved)
{
    _decision = Decision.Good;

    // ★ Stage4：Good 確定（ステージ5用に加算）
    ResultStore.Instance?.AddGood();

    if (debugLog) Debug.Log("[Decision] Window end -> Good");
    StartCoroutine(ReactTurnRelease(trigHappy));
}
else if (_called)
{
    _decision = Decision.Normal;

    // ★ Stage4：Normal 確定（ステージ5用に加算）
    ResultStore.Instance?.AddNormal();

    if (debugLog) Debug.Log("[Decision] Window end -> Normal");
    StartCoroutine(ReactTurnRelease(trigCalm));
}
else
{
    _decision = Decision.Bad;

    // ★ Stage4：Bad 確定（ステージ5用に加算）
    ResultStore.Instance?.AddBad();

    if (debugLog) Debug.Log("[Decision] Window end -> Bad");
    StartCoroutine(ReactTurnRelease(trigAngry));
}
    }
    /// <summary>
    /// 反応（こちら向き）→ 振り返り(turn) → 解放（元の向きに戻して歩かせる）
    /// </summary>
    private IEnumerator ReactTurnRelease(string triggerName)
    {
        // Bark OFF
        enemyAnimator.SetBool(paramIsBarking, false);
        playerAnimator.SetBool(paramIsBarking, false);

        // ★反応中：こちら向きモード
        _reacting = true;

        // Reaction Trigger
        if (!string.IsNullOrEmpty(triggerName))
        {
            playerAnimator.ResetTrigger(trigHappy);
            playerAnimator.ResetTrigger(trigCalm);
            playerAnimator.ResetTrigger(trigAngry);
            if (!string.IsNullOrEmpty(trigTurn)) playerAnimator.ResetTrigger(trigTurn);

            playerAnimator.SetTrigger(triggerName);
        }

        // 反応を見せる時間
        float wait = 0f;
        if (triggerName == trigHappy) wait = happyHoldSeconds;
        else if (triggerName == trigCalm) wait = calmHoldSeconds + normalReleaseDelay;
        else if (triggerName == trigAngry) wait = angryHoldSeconds + badExtraHoldSeconds;

        if (wait > 0f) yield return new WaitForSeconds(wait);

        // ---- ここから Turn を挟む ----

        // まず「元の進行方向」に揃える（turnが変な方向に見えないように）
        playerWalk.transform.rotation = _playerRotBeforeBark;

        // turn中は「こちら向き制御」を止める（アニメで見せたいので）
        _reacting = false;

        // DoTurn を叩く（Animatorで AnyState->turn を作っておく）
        if (!string.IsNullOrEmpty(trigTurn))
        {
            playerAnimator.ResetTrigger(trigTurn);
            playerAnimator.SetTrigger(trigTurn);

            // turn を見せる時間だけ待つ
            if (turnClipSeconds > 0f)
                yield return new WaitForSeconds(turnClipSeconds);
        }

        // 最後にもう一度「元の向き」を保証
        playerWalk.transform.rotation = _playerRotBeforeBark;

        ReleaseHoldImmediate();
    }

    private bool IsCallPressedThisFrame()
    {
#if ENABLE_INPUT_SYSTEM
        if (callAction != null) return callAction.action.WasPressedThisFrame();
#endif
        return Input.GetKeyDown(callKey);
    }

    private float GetLookAwayAngleDeg()
    {
        Vector3 toEnemy = (EnemyT.position - head.position);
        toEnemy.y = 0f;
        if (toEnemy.sqrMagnitude < 0.0001f) return 180f;

        Vector3 fwd = head.forward;
        fwd.y = 0f;

        return Vector3.Angle(fwd.normalized, toEnemy.normalized);
    }

    private void PlayCallSfx()
    {
        if (callAudioSource == null || callSfx == null) return;
        callAudioSource.PlayOneShot(callSfx, callVolume);
    }

    private void ReleaseHoldImmediate()
    {
        if (_seq != null) { StopCoroutine(_seq); _seq = null; }

        enemyAnimator.SetBool(paramIsBarking, false);
        playerAnimator.SetBool(paramIsBarking, false);

        // 向きを元に戻す（最終保証）
        playerWalk.transform.rotation = _playerRotBeforeBark;
        _enemyRoot.rotation = _enemyRotBeforeBark;

        ExitStop();

        _barking = false;
        _playerBarkStarted = false;

        _resolvedOnce = true;

        StartCoroutine(Cooldown());
    }

    // -----------------------
    // Stop / Face
    // -----------------------
    private void EnterStop()
    {
        playerWalk.Walking = false;

        enemyAgent.isStopped = true;
        enemyAgent.ResetPath();
        enemyAgent.velocity = Vector3.zero;

        _enemyUpdateRotationBackup = enemyAgent.updateRotation;
        enemyAgent.updateRotation = false;
    }

    private void KeepStoppedEveryFrame()
    {
        playerWalk.Walking = false;
        enemyAgent.isStopped = true;
        enemyAgent.velocity = Vector3.zero;
    }

    private void ExitStop()
    {
        playerWalk.Walking = true;
        enemyAgent.isStopped = false;
        enemyAgent.updateRotation = _enemyUpdateRotationBackup;
    }

    private void FaceEachOther()
    {
        // 敵犬は常にプレイヤー犬を見る
        SmoothLookAt(_enemyRoot, PlayerT.position, faceSpeed);

        // プレイヤー犬は吠え開始後は敵犬を見る
        if (_playerBarkStarted)
            SmoothLookAt(playerWalk.transform, EnemyT.position, faceSpeed);
    }

    private void FaceOwnerForReaction()
    {
        // ★プレイヤー犬だけ “飼い主(head)” を向く
        SmoothLookAt(playerWalk.transform, head.position, reactionFaceSpeed);

        // 敵犬はプレイヤー犬を見続ける（自然）
        SmoothLookAt(_enemyRoot, PlayerT.position, faceSpeed);
    }

    private static void SmoothLookAt(Transform t, Vector3 targetPos, float speed)
    {
        Vector3 dir = targetPos - t.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f) return;

        Quaternion targetRot = Quaternion.LookRotation(dir.normalized, Vector3.up);
        t.rotation = Quaternion.Slerp(t.rotation, targetRot, Time.deltaTime * speed);
    }

    private IEnumerator FaceTowardsWithHardStop(Transform t, Vector3 targetPos, float speed)
    {
        for (int i = 0; i < 90; i++)
        {
            KeepStoppedEveryFrame();

            Vector3 dir = targetPos - t.position;
            dir.y = 0f;
            if (dir.sqrMagnitude < 0.0001f) break;

            Quaternion targetRot = Quaternion.LookRotation(dir.normalized, Vector3.up);
            t.rotation = Quaternion.Slerp(t.rotation, targetRot, Time.deltaTime * speed);

            if (Quaternion.Angle(t.rotation, targetRot) < 3f) break;
            yield return null;
        }
    }

    private IEnumerator Cooldown()
    {
        _cooldown = true;
        yield return new WaitForSeconds(cooldownSeconds);
        _cooldown = false;
    }
}
