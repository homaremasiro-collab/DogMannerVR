using UnityEngine;

public class PortalStartGame : MonoBehaviour
{
    [SerializeField] private string requiredTag = "Hand"; // PlayerでもOK
    private bool _used = false;

    private void OnTriggerEnter(Collider other)
    {
        if (_used) return;
        if (!other.CompareTag(requiredTag)) return;

        _used = true;

        if (GameFlow.Instance == null)
        {
            Debug.LogError("[PortalStartGame] GameFlow.Instance がありません");
            return;
        }

        GameFlow.Instance.StartFromHub();
    }
}
