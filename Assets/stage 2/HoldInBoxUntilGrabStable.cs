using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[DefaultExecutionOrder(-50)]
public class HoldInBoxUntilGrabStable : MonoBehaviour
{
    [Header("Lock Target")]
    [SerializeField] private Transform lockPoint;
    [SerializeField] private bool lockRotation = true;

    [Header("Behavior")]
    [SerializeField] private bool releaseOnFirstGrab = true;

    private XRGrabInteractable grab;
    private Rigidbody rb;

    private bool isLocked = true;
    private bool hasEverGrabbed = false;

    private bool savedUseGravity;
    private bool savedIsKinematic;

    void Reset()
    {
        grab = GetComponent<XRGrabInteractable>();
        rb = GetComponent<Rigidbody>();
    }

    void Awake()
    {
        // ★参照を必ず自動取得（ここが肝）
        if (!grab) grab = GetComponent<XRGrabInteractable>();
        if (!rb) rb = GetComponent<Rigidbody>();

        if (!grab || !rb)
        {
            Debug.LogError($"[HoldInBoxUntilGrabStable] Missing grab or rigidbody on {name}", this);
            enabled = false;
            return;
        }

        grab.selectEntered.AddListener(OnSelectEntered);
        grab.selectExited.AddListener(OnSelectExited);

        // 物理設定を保存
        savedUseGravity = rb.useGravity;
        savedIsKinematic = rb.isKinematic;

        Lock();
    }

    void OnDestroy()
    {
        if (grab)
        {
            grab.selectEntered.RemoveListener(OnSelectEntered);
            grab.selectExited.RemoveListener(OnSelectExited);
        }
    }

    void FixedUpdate()
    {
        if (!isLocked || !lockPoint) return;

        // ★固定（物理で暴れないように位置を維持）
        rb.MovePosition(lockPoint.position);
        if (lockRotation) rb.MoveRotation(lockPoint.rotation);
    }

    private void Lock()
    {
        isLocked = true;

        // ★固定中は重力OFF & kinematic ON（落下/転がり防止）
        rb.useGravity = false;
        rb.isKinematic = true;

        // 初期位置を即合わせ
        if (lockPoint)
        {
            transform.position = lockPoint.position;
            if (lockRotation) transform.rotation = lockPoint.rotation;
        }
    }

    private void Unlock()
    {
        isLocked = false;

        // ★掴めるように物理を戻す（ただし掴み中はXR側が動かす）
        rb.isKinematic = savedIsKinematic;
        rb.useGravity = savedUseGravity;
    }

    private void OnSelectEntered(SelectEnterEventArgs args)
    {
        hasEverGrabbed = true;
        Unlock();

        // 1回掴んだら、このスクリプト自体を止める（以降は通常掴み）
        if (releaseOnFirstGrab)
            enabled = false;
    }

    private void OnSelectExited(SelectExitEventArgs args)
    {
        // 「掴むまで固定」用途なら、掴んだ後は固定しない
        // （ここで再Lockしない）
    }
}
