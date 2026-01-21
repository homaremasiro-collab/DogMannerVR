using System.Collections;
using UnityEngine;

public class Stage5NormalDirector : MonoBehaviour
{
    [SerializeField] private DogPointMoverNormal mover;
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
            Debug.LogError("[Stage5NormalDirector] mover is null. InspectorにDogPointMoverNormalを入れてください。");
            _running = false;
            yield break;
        }

        if (debugLog) Debug.Log("[Stage5NormalDirector] Start -> DogPointMoverNormal");

        bool finished = false;
        void OnDone() => finished = true;

        mover.OnFinished += OnDone;
        mover.StartSequence();

        while (!finished)
            yield return null;

        mover.OnFinished -= OnDone;

        if (debugLog) Debug.Log("[Stage5NormalDirector] Sequence done");
        _running = false;
    }
}
