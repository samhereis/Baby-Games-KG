using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

public class CameraAdjuster : MonoBehaviour
{
    private void OnEnable()
    {
        Init();
    }

    [Button]
    private void Init()
    {
        FitCameraToCollider(Camera.main, GetComponent<BoxCollider2D>());
    }

    public void FitCameraToCollider(Camera cam, BoxCollider2D col, float padding = 0f)
    {
        if (cam == null || col == null || !cam.orthographic)
        {
            return;
        }

        Bounds bounds = col.bounds;
        Vector2 size = bounds.size * (1 + padding);

        float targetSize;
        float cameraAspect = cam.aspect;

        if (size.x > size.y * cameraAspect)
        {
            targetSize = size.x / cameraAspect / 2f;
        }
        else
        {
            targetSize = size.y / 2f;
        }

        cam.DOOrthoSize(Mathf.Max(targetSize, 0.01f), 0.25f);

        var position = cam.transform.position;
        cam.transform.DOMoveX(transform.position.x, 0.25f);
        cam.transform.DOMoveY(transform.position.y, 0.25f);
    }
}
