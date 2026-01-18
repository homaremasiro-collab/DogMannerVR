using UnityEngine;

public class CloudUVScroll : MonoBehaviour
{
    [SerializeField] Renderer targetRenderer;
    [SerializeField] Vector2 scrollSpeed = new Vector2(0.005f, 0.002f);

    Material _mat;
    Vector2 _offset;

    void Awake()
    {
        if (!targetRenderer) targetRenderer = GetComponent<Renderer>();
        _mat = targetRenderer.material; // インスタンス化されるのでOK
    }

    void Update()
    {
        _offset += scrollSpeed * Time.deltaTime;
        _mat.SetTextureOffset("_BaseMap", _offset);
    }
}
