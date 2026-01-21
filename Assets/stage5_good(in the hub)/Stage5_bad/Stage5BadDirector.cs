using System.Collections;
using UnityEngine;

public class Stage5BadDirector : MonoBehaviour
{
    [SerializeField] private DogPointMoverBad mover;
    [SerializeField] private bool debugLog = true;

    private bool _running;

    private void Start()
    {
        if (_running) return;
        StartCoroutine(Sequence());
    }

    private IEnumerator Sequence()
    {
        _running = true;

        if (mover == null)
        {
            Debug.LogError("[Stage5BadDirector] mover is null. InspectorにDogPointMoverBadを入れてください。");
            _running = false;
            yield break;
        }

        if (debugLog) Debug.Log("[Stage5BadDirector] Start -> DogPointMoverBad");

        bool finished = false;
        void OnDone() => finished = true;

        mover.OnFinished += OnDone;
        mover.StartSequence();

        while (!finished)
            yield return null;

        mover.OnFinished -= OnDone;

        if (debugLog) Debug.Log("[Stage5BadDirector] Sequence done");
        _running = false;
    }
}
