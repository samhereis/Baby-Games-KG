using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GeneratePointsInBox : MonoBehaviour
{
    [Tooltip("Spacing between points in world units.")]
    public float distance = 0.1f;

    [Tooltip("Alpha threshold above which a pixel is considered opaque.")]
    [Range(0f, 1f)]
    public float alphaThreshold = 0.1f;

    public List<Transform> pointObjects = new List<Transform>();
    public List<Transform> pointObjects_Secondary = new List<Transform>();

    public Transform _position;
    public Transform _positionParent;

    [Button]
    public void GeneratePoints()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr == null)
        {
            Debug.LogError("No SpriteRenderer found on the GameObject.");
            return;
        }

        Sprite sprite = sr.sprite;
        Texture2D texture = sprite.texture;

        Rect rect = sprite.rect;
        int xMin = (int)rect.x;
        int yMin = (int)rect.y;
        int width = (int)rect.width;
        int height = (int)rect.height;

        int pixelStep = Mathf.Max(1, Mathf.RoundToInt(distance * sprite.pixelsPerUnit));

        for (int x = xMin; x < xMin + width; x += pixelStep)
        {
            for (int y = yMin; y < yMin + height; y += pixelStep)
            {
                Color pixelColor = texture.GetPixel(x, y);

                if (pixelColor.a > alphaThreshold)
                {
                    float localX = (x - (xMin + sprite.pivot.x)) / sprite.pixelsPerUnit;
                    float localY = (y - (yMin + sprite.pivot.y)) / sprite.pixelsPerUnit;
                    Vector3 localPosition = new Vector3(localX, localY, 0f);

                    Vector3 worldPos = transform.TransformPoint(localPosition);
                    var pointObject = Instantiate(_position, worldPos, Quaternion.identity, transform);
                    pointObjects.Add(pointObject.transform);
                }
            }
        }
    }

    [Button]
    public void Sync()
    {
        pointObjects.RemoveAll(x => x == null);
        pointObjects = _positionParent.GetComponentsInChildren<Transform>().Where(x => x.name.StartsWith("_") == false && x.name != _positionParent.name).ToList();
        pointObjects_Secondary = _positionParent.GetComponentsInChildren<Transform>().Where(x => x.name.StartsWith("_") == true && x.name != _positionParent.name).ToList();
    }
}
