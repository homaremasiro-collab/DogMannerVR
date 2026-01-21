using System;
using System.Collections;
using UnityEngine;

public class Stage5GoodDirector : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private DogPointMover mover;

    [Header("Sequence")]
    [SerializeField] private bool autoStart = true;
    [SerializeField] private bool waitMoverFinished = true;

    [Tooltip("無限待ち防止。0なら無効")]
    [SerializeField] private float timeoutSeconds = 30f;

    [Header("Debug")]
    [SerializeField] private bool debugLog = true;

    private bool _started;
    private bool _running;

    private void Start()
    {
        if (!autoStart) return;
        StartGood();
    }

    /// <summary>
    /// 手動で開始したい時に呼べる（Button/他スクリプトから）
    /// </summary>
    public void StartGood()
    {
        if (_started) return;
        _started = true;

        if (_running) return;
        _running = true;

        StartCoroutine(Sequence());
    }

    private IEnumerator Sequence()
    {
        if (mover == null)
        {
            Debug.LogError("[Stage5GoodDirector] mover is NULL. Assign DogPointMover.");
            _running = false;
            yield break;
        }

        if (!mover.gameObject.activeInHierarchy)
        {
            Debug.LogError("[Stage5GoodDirector] mover GameObject is inactive. Enable it.");
            _running = false;
            yield break;
        }

        if (debugLog) Debug.Log("[Stage5GoodDirector] Start -> DogPointMover");

        bool finished = false;
        Action onDone = () => finished = true;

        // 念のため二重登録を防ぐ
        mover.OnFinished -= onDone;
        mover.OnFinished += onDone;

        // 開始
        mover.StartSequence();

        if (waitMoverFinished)
        {
            float t = 0f;
            while (!finished)
            {
                if (timeoutSeconds > 0f)
                {
                    t += Time.deltaTime;
                    if (t >= timeoutSeconds)
                    {
                        Debug.LogError("[Stage5GoodDirector] Timeout waiting for mover.OnFinished. " +
                                       "DogPointMoverがMissing refs/State NOT FOUND等で途中停止してないかConsole確認。");
                        break;
                    }
                }
                yield return null;
            }
        }
        else
        {
            yield return null;
        }

        mover.OnFinished -= onDone;

        if (debugLog) Debug.Log("[Stage5GoodDirector] Sequence done");

        _running = false;
    }
}
