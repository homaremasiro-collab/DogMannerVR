// ScreenFader.cs
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ScreenFader : MonoBehaviour
{
    public static ScreenFader Instance { get; private set; }

    [Header("Fade")]
    [SerializeField] private float fadeOutSeconds = 0.25f;
    [SerializeField] private float fadeInSeconds = 0.25f;

    private Canvas _canvas;
    private Image _img;
    private Coroutine _co;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        BuildCanvasIfNeeded();
        SetAlpha(1f); // 起動時は黒
    }

    private void BuildCanvasIfNeeded()
    {
        if (_canvas != null) return;

        var go = new GameObject("FaderCanvas");
        go.transform.SetParent(transform);

        _canvas = go.AddComponent<Canvas>();
        _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        _canvas.sortingOrder = 9999;

        go.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        go.AddComponent<GraphicRaycaster>();

        var imgGo = new GameObject("FadeImage");
        imgGo.transform.SetParent(go.transform);
        _img = imgGo.AddComponent<Image>();
        _img.color = new Color(0, 0, 0, 1);

        var rt = _img.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    public void InstantBlack() => SetAlpha(1f);
    public void InstantClear() => SetAlpha(0f);

    public Coroutine FadeOut() => FadeTo(1f, fadeOutSeconds);
    public Coroutine FadeIn() => FadeTo(0f, fadeInSeconds);

    public Coroutine FadeTo(float targetAlpha, float seconds)
    {
        if (_co != null) StopCoroutine(_co);
        _co = StartCoroutine(FadeRoutine(targetAlpha, seconds));
        return _co;
    }

    private IEnumerator FadeRoutine(float target, float seconds)
    {
        BuildCanvasIfNeeded();
        float start = _img.color.a;
        float t = 0f;

        if (seconds <= 0f)
        {
            SetAlpha(target);
            yield break;
        }

        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / seconds;
            float a = Mathf.Lerp(start, target, Mathf.Clamp01(t));
            SetAlpha(a);
            yield return null;
        }
        SetAlpha(target);
    }

    private void SetAlpha(float a)
    {
        if (_img == null) return;
        var c = _img.color;
        c.a = Mathf.Clamp01(a);
        _img.color = c;
    }
}
