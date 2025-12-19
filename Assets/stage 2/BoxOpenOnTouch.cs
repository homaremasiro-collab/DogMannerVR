using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class BoxOpenOnTouch : MonoBehaviour
{
    [Header("Assign")]
    [SerializeField] private Transform lidPivot;     // ← LidPivot を入れる
    [SerializeField] private Transform itemsRoot;    // Items を入れる

    [Header("Lid Rotation (LOCAL)")]
    [SerializeField] private Vector3 closedEuler = Vector3.zero;
    [SerializeField] private Vector3 openEuler = new Vector3(-110f, 0f, 0f);
    [SerializeField] private float openSpeed = 180f;

    [Header("Freeze Items Until Open")]
    [SerializeField] private bool disableGrabUntilOpen = true;
    [SerializeField] private bool keepItemsKinematicUntilOpen = true;

    private Quaternion _closedRot;
    private Quaternion _openRot;
    private bool _opening;
    private bool _opened;

    private readonly List<Rigidbody> _itemBodies = new();
    private readonly List<XRGrabInteractable> _itemGrabs = new();

    private void Awake()
    {
        _closedRot = Quaternion.Euler(closedEuler);
        _openRot   = Quaternion.Euler(openEuler);

        // ★ここが重要：開始時に必ず閉じる（今が開きっぱ問題を強制解決）
        if (lidPivot != null)
        {
            lidPivot.localRotation = _closedRot;
        }

        CacheItems();
        FreezeItems(true);
    }

    private void CacheItems()
    {
        _itemBodies.Clear();
        _itemGrabs.Clear();

        if (itemsRoot == null) return;

        itemsRoot.GetComponentsInChildren(true, _itemBodies);
        itemsRoot.GetComponentsInChildren(true, _itemGrabs);
    }

    private void FreezeItems(bool freeze)
    {
        if (keepItemsKinematicUntilOpen)
        {
            foreach (var rb in _itemBodies)
            {
                if (!rb) continue;
                rb.isKinematic = freeze;
                rb.useGravity = !freeze;
                if (freeze)
                {
                    rb.velocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                }
            }
        }

        if (disableGrabUntilOpen)
        {
            foreach (var grab in _itemGrabs)
            {
                if (!grab) continue;
                grab.enabled = !freeze;
            }
        }
    }

    private void Update()
    {
        if (!_opening || _opened || lidPivot == null) return;

        lidPivot.localRotation = Quaternion.RotateTowards(
            lidPivot.localRotation,
            _openRot,
            openSpeed * Time.deltaTime
        );

        if (Quaternion.Angle(lidPivot.localRotation, _openRot) < 0.5f)
        {
            lidPivot.localRotation = _openRot;
            _opened = true;
            _opening = false;

            FreezeItems(false);
            Debug.Log("BOX OPENED");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // もうタグ判定なし。触れたら一回だけ開く。
        if (_opened || _opening) return;

        Debug.Log("Box Trigger Enter: " + other.name);
        _opening = true;
    }
}
