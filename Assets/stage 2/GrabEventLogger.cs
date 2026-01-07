using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class GrabEventLogger : MonoBehaviour
{
    void Awake()
    {
        var g = GetComponent<XRGrabInteractable>();
        if (!g) { Debug.LogError($"{name}: no XRGrabInteractable"); return; }

        g.hoverEntered.AddListener(_ => Debug.Log($"{name}: hoverEntered"));
        g.hoverExited.AddListener(_ => Debug.Log($"{name}: hoverExited"));
        g.selectEntered.AddListener(_ => Debug.Log($"{name}: selectEntered"));
        g.selectExited.AddListener(_ => Debug.Log($"{name}: selectExited"));
    }
}
