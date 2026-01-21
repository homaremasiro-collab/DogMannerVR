using UnityEngine;

public class PortalNextStage : MonoBehaviour
{
    [SerializeField] private string targetSceneName = "Stage_Dog2";
    [SerializeField] private string requiredTag = "Hand";
    private bool _used = false;

    private void OnTriggerEnter(Collider other)
    {
        if (_used) return;
        if (!other.CompareTag(requiredTag)) return;

        _used = true;

        GameFlow.Instance.GoToSceneAdditive(targetSceneName);
    }
}
