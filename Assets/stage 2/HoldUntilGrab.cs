using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[DisallowMultipleComponent]
public class HoldInBoxUntilGrab : MonoBehaviour
{
    [Header("Lock Target")]
    public Transform lockPoint;   // EggSpot / fish point など

    [Header("Lock Options")]
    public bool lockRotation = true;
    public bool freezeConstraintsWhileLocked = true;

    Rigidbody rb;
    XRGrabInteractable grab;

    bool locked = true;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        grab = GetComponent<XRGrabInteractable>();

        if (rb == null) rb = gameObject.AddComponent<Rigidbody>();
        if (grab == null) grab = gameObject.AddComponent<XRGrabInteractable>();

        // 重要：掴んだ瞬間にロック解除
        grab.selectEntered.AddListener(OnSelectEntered);
        grab.selectExited.AddListener(OnSelectExited);
    }

    void OnEnable()
    {
        ApplyLock(true);
    }

    void OnDisable()
    {
        if (grab != null)
        {
            grab.selectEntered.RemoveListener(OnSelectEntered);
            grab.selectExited.RemoveListener(OnSelectExited);
        }
    }

    void FixedUpdate()
    {
        if (!locked) return;
        if (lockPoint == null) return;

        // 掴もうとしている最中に位置を戻すと「掴めない」になるので、
        // “選択中”は絶対に固定処理を止める
        if (grab != null && grab.isSelected) return;

        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        rb.MovePosition(lockPoint.position);

        if (lockRotation)
            rb.MoveRotation(lockPoint.rotation);
    }

    void OnSelectEntered(SelectEnterEventArgs _)
    {
        // 掴んだ瞬間に完全解除
        ApplyLock(false);
    }

    void OnSelectExited(SelectExitEventArgs _)
    {
        // 離した後に“また固定したい”なら true に戻す（今回は不要ならコメントアウト）
        // ApplyLock(true);
    }

    void ApplyLock(bool on)
    {
        locked = on;

        if (rb == null) return;

        if (on)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            if (freezeConstraintsWhileLocked)
                rb.constraints = RigidbodyConstraints.FreezeAll;

            if (lockPoint != null)
            {
                transform.position = lockPoint.position;
                if (lockRotation) transform.rotation = lockPoint.rotation;
            }
        }
        else
        {
            rb.constraints = RigidbodyConstraints.None;
            rb.isKinematic = false;
            rb.useGravity = true;
        }
    }
}
