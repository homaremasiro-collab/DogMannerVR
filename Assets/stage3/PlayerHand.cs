using UnityEngine;
using UnityEngine.XR;

public class PlayerHand : MonoBehaviour
{
    [Header("Which XR hand this is")]
    public XRNode node = XRNode.LeftHand;

    [Header("Read only")]
    public float speed;              // m/s
    public bool gripPressed;         // grip button
    public bool triggerPressed;      // index trigger button

    Vector3 _prevPos;

    void Awake()
    {
        _prevPos = transform.position;
    }

    void Update()
    {
        // speed (position delta)
        var pos = transform.position;
        speed = (pos - _prevPos).magnitude / Mathf.Max(Time.deltaTime, 0.0001f);
        _prevPos = pos;

        // buttons
        var device = InputDevices.GetDeviceAtXRNode(node);

        if (device.isValid)
        {
            device.TryGetFeatureValue(CommonUsages.gripButton, out gripPressed);
            device.TryGetFeatureValue(CommonUsages.triggerButton, out triggerPressed);
        }
        else
        {
            gripPressed = false;
            triggerPressed = false;
        }
    }
}
