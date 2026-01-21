using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class DispenserButton : MonoBehaviour
{
    [Header("XR")]
    [SerializeField] private XRBaseInteractable interactable; // XRSimpleInteractableでOK

    [Header("Refs")]
    [SerializeField] private DogStage2Flow stage2Flow;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Transform itemsRoot;     // 空のGameObject推奨（Canvasは避ける）
    [SerializeField] private GameObject hintUI;       // 説明UI（選択肢なし）

    [Header("Spawn Prefab (this machine only)")]
    [SerializeField] private GameObject spawnPrefab;  // ★この自販機から出す1個だけ

    [Header("Spawn")]
    [SerializeField] private float pushOutForce = 0.6f;

    [Header("Options")]
    [SerializeField] private bool onlyWhenHungry = true;
    [SerializeField] private float pressCooldown = 0.5f;
    [SerializeField] private float uiShowSeconds = 2.0f;
    [SerializeField] private bool preventDuplicateSpawn = true;

    private float _lastPress = -999f;

    private void Reset()
    {
        interactable = GetComponent<XRBaseInteractable>();
    }

    private void Awake()
    {
        if (!interactable) interactable = GetComponent<XRBaseInteractable>();
        if (!stage2Flow) stage2Flow = FindObjectOfType<DogStage2Flow>();

        if (interactable != null)
        {
            // 触って押す(Activate)・つかむ(Select)どっちでも反応
            interactable.activated.AddListener(_ => TryPress("activate"));
            interactable.selectEntered.AddListener(_ => TryPress("select"));
        }

        if (hintUI != null) hintUI.SetActive(false);
    }

    private void OnDestroy()
    {
        if (interactable != null)
        {
            interactable.activated.RemoveAllListeners();
            interactable.selectEntered.RemoveAllListeners();
        }
    }

    private void TryPress(string by)
    {
        if (Time.time - _lastPress < pressCooldown) return;
        _lastPress = Time.time;

        if (onlyWhenHungry && stage2Flow != null && stage2Flow.CanMove)
            return;

        if (hintUI != null)
            StartCoroutine(ShowHintUI());

        SpawnOne();
    }

    private IEnumerator ShowHintUI()
    {
        hintUI.SetActive(true);
        yield return new WaitForSeconds(uiShowSeconds);
        if (hintUI != null) hintUI.SetActive(false);
    }

    private void SpawnOne()
    {
        if (spawnPoint == null)
        {
            Debug.LogWarning("[DispenserButton] spawnPoint 未設定");
            return;
        }

        if (spawnPrefab == null)
        {
            Debug.LogWarning("[DispenserButton] spawnPrefab 未設定");
            return;
        }

        if (preventDuplicateSpawn && itemsRoot != null)
        {
            // 既に何か出てたら増殖させない
            if (itemsRoot.GetComponentInChildren<FoodData>(true) != null)
                return;
        }

        var go = Instantiate(spawnPrefab, spawnPoint.position, spawnPoint.rotation);

        if (itemsRoot != null)
            go.transform.SetParent(itemsRoot, true);

        var rb = go.GetComponent<Rigidbody>();
        if (rb != null)
            rb.AddForce(spawnPoint.forward * pushOutForce, ForceMode.VelocityChange);
    }
}
