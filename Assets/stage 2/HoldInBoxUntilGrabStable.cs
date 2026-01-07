using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(XRGrabInteractable))]
public class HoldInBoxUntilGrabStable : MonoBehaviour
{
    [Header("Lock Target")]
    public Transform lockPoint;

    [Header("Options")]
    public bool lockRotation = true;

    [Tooltip("Hoverした時点で固定解除（おすすめ）")]
    public bool releaseOnHover = true;

    [Tooltip("Select（掴み）でも固定解除（保険）")]
    public bool releaseOnGrab = true;

    Rigidbody rb;
    XRGrabInteractable grab;

    bool locked = true;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        grab = GetComponent<XRGrabInteractable>();

        // ★重要：固定中でも Dynamic のままにする
        rb.isKinematic = false;
        rb.useGravity = false;

        // イベント
        grab.hoverEntered.AddListener(OnHoverEntered);
        grab.selectEntered.AddListener(OnSelectEntered);
    }

    void OnDestroy()
    {
        if (grab != null)
        {
            grab.hoverEntered.RemoveListener(OnHoverEntered);
            grab.selectEntered.RemoveListener(OnSelectEntered);
        }
    }

    void OnEnable()
    {
        Lock();
    }

    void FixedUpdate()
    {
        if (!locked) return;
        if (lockPoint == null) return;

        // 物理を止める
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // MovePositionより「直書き」の方がXRIと喧嘩しにくい
        rb.position = lockPoint.position;
        if (lockRotation)
            rb.rotation = lockPoint.rotation;
    }

    void OnHoverEntered(HoverEnterEventArgs _)
    {
        if (locked && releaseOnHover)
            Unlock();
    }

    void OnSelectEntered(SelectEnterEventArgs _)
    {
        if (locked && releaseOnGrab)
            Unlock();
    }

    public void Lock()
    {
        locked = true;

        rb.useGravity = false;
        rb.isKinematic = false; // ★ここ絶対 false
        rb.constraints = RigidbodyConstraints.FreezeAll;

        if (lockPoint != null)
        {
            rb.position = lockPoint.position;
            if (lockRotation) rb.rotation = lockPoint.rotation;
        }
    }

    public void Unlock()
    {
        locked = false;

        rb.constraints = RigidbodyConstraints.None;
        rb.useGravity = true;
        rb.isKinematic = false;
    }
}
