using System.Collections;
using TMPro;
using UnityEngine;

public class FloatingGuideUI : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Timings")]
    [SerializeField] private float charsPerSecond = 35f;
    [SerializeField] private float holdSeconds = 1.0f;
    [SerializeField] private float fadeOutSeconds = 0.35f;

    [Header("Optional")]
    [SerializeField] private bool startHidden = true;

    private Coroutine routine;

    private void Awake()
    {
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        if (text == null) text = GetComponentInChildren<TextMeshProUGUI>();

        if (startHidden) HideImmediate(); // ← 透明にするだけ（OFFにしない）
    }

    public void ShowMessage(string message)
    {
        // 念のため、親がOFFならONにする（Canvas側がOFFにされても復帰できる）
        if (!gameObject.activeSelf) gameObject.SetActive(true);

        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(ShowRoutine(message));
    }

    public void HideImmediate()
    {
        if (routine != null) StopCoroutine(routine);
        routine = null;

        if (text != null) text.text = "";

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        // ★ここが重要：gameObject.SetActive(false) はしない
    }

    private IEnumerator ShowRoutine(string message)
    {
        // 表示状態へ
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        if (text != null) text.text = "";

        float interval = 1f / Mathf.Max(1f, charsPerSecond);

        for (int i = 0; i < message.Length; i++)
        {
            if (text != null) text.text += message[i];
            yield return new WaitForSeconds(interval);
        }

        yield return new WaitForSeconds(holdSeconds);

        // フェードアウト
        float t = 0f;
        float startA = (canvasGroup != null) ? canvasGroup.alpha : 1f;

        while (t < fadeOutSeconds)
        {
            t += Time.deltaTime;
            if (canvasGroup != null)
                canvasGroup.alpha = Mathf.Lerp(startA, 0f, t / fadeOutSeconds);

            yield return null;
        }

        if (canvasGroup != null) canvasGroup.alpha = 0f;
        if (text != null) text.text = "";

        routine = null;
    }
}
