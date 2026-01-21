using System.Collections;
using UnityEngine;

public class Stage5CutsceneDirector : MonoBehaviour
{
    public enum EndType { Good, Normal, Bad }

    [Header("Refs")]
    [SerializeField] private Animator dogAnimator;
    [SerializeField] private Transform dogRoot;     // 犬の位置/回転を動かすなら
    [SerializeField] private GameObject glowFx;     // 光のPrefabがあれば
    [SerializeField] private Renderer[] dogRenderers; // 消す用（SkinnedMesh含む）

    [Header("Which end to play (テスト用)")]
    [SerializeField] private EndType debugEnd = EndType.Good;

    [Header("Animator Triggers")]
    [SerializeField] private string trigRun = "Run";
    [SerializeField] private string trigWalk = "Walk";
    [SerializeField] private string trigHappy = "Happy";
    [SerializeField] private string trigBark = "Bark";
    [SerializeField] private string trigIdle = "Idle";

    [Header("Timings")]
    [SerializeField] private float runSeconds = 1.5f;
    [SerializeField] private float happySeconds = 1.5f;
    [SerializeField] private float barkDelay = 0.6f;
    [SerializeField] private float walkSeconds = 2.0f;

    [Header("LookBack (Normal)")]
    [SerializeField] private bool useRotateLookBack = true;
    [SerializeField] private float lookBackTurnAngle = 140f;  // 振り返り角
    [SerializeField] private float lookBackTurnSeconds = 0.4f;
    [SerializeField] private float lookBackHoldSeconds = 0.4f;

    void Start()
    {
        // ResultStore の結果で決めるのが本番
        // まずは debugEnd で動作確認
        StartCoroutine(PlayEnd(debugEnd));
    }

    IEnumerator PlayEnd(EndType end)
    {
        // どのルートでも最後は歩いて消えるので、共通部分に寄せる

        if (end == EndType.Good)
        {
            // 走って近づく（※実際の移動は DogMoveOnWalk / NavMesh でもOK）
            Trigger(trigRun);
            yield return new WaitForSeconds(runSeconds);

            // 近くで尻尾振り（Happy/Idleでも可）
            Trigger(trigHappy);
            yield return new WaitForSeconds(barkDelay);

            // 1回吠える
            Trigger(trigBark);
            yield return new WaitForSeconds(happySeconds);

            // 振り返って歩く
            Trigger(trigWalk);
            yield return new WaitForSeconds(walkSeconds);

            yield return Vanish();
            yield break;
        }

        if (end == EndType.Normal)
        {
            // 普通に歩く
            Trigger(trigWalk);
            yield return new WaitForSeconds(0.8f);

            // 1回振り返る（アニメがないなら回転で代用）
            if (useRotateLookBack && dogRoot != null)
            {
                yield return RotateBy(dogRoot, lookBackTurnAngle, lookBackTurnSeconds);
                yield return new WaitForSeconds(lookBackHoldSeconds);
                yield return RotateBy(dogRoot, -lookBackTurnAngle, lookBackTurnSeconds);
            }

            // また歩き続ける
            Trigger(trigWalk);
            yield return new WaitForSeconds(walkSeconds);

            yield return Vanish();
            yield break;
        }

        // Bad
        {
            // 何も反応せず歩いて消える
            Trigger(trigWalk);
            yield return new WaitForSeconds(walkSeconds + 0.8f);

            yield return Vanish();
        }
    }

    void Trigger(string t)
    {
        if (dogAnimator == null) return;
        if (string.IsNullOrEmpty(t)) return;
        dogAnimator.SetTrigger(t);
    }

    IEnumerator Vanish()
    {
        // 光FX（あれば）
        if (glowFx != null)
        {
            glowFx.SetActive(true);
        }

        // 少し待ってから犬を消す（簡易）
        yield return new WaitForSeconds(0.5f);

        if (dogRenderers != null)
        {
            foreach (var r in dogRenderers)
                if (r != null) r.enabled = false;
        }

        yield return new WaitForSeconds(0.5f);
    }

    IEnumerator RotateBy(Transform tr, float angle, float seconds)
    {
        var start = tr.rotation;
        var end = start * Quaternion.Euler(0f, angle, 0f);

        float t = 0f;
        while (t < seconds)
        {
            t += Time.deltaTime;
            float a = Mathf.Clamp01(t / seconds);
            tr.rotation = Quaternion.Slerp(start, end, a);
            yield return null;
        }
        tr.rotation = end;
    }
}
