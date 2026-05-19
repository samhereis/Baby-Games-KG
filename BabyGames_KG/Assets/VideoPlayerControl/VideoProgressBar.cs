using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Video;

public class VideoProgressBar : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private VideoPlayer videoPlayer;

    private Slider progress;

    private bool _followVideoPlayer = true;

    private void Awake()
    {
        progress = GetComponent<Slider>();
    }

    private void Update()
    {
        if (_followVideoPlayer == false) { return; }

        if (videoPlayer.frameCount > 0)
        {
            progress.value = (float)videoPlayer.frame / (float)videoPlayer.frameCount;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        TrySkip(eventData);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _followVideoPlayer = false;
        TrySkip(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _followVideoPlayer = true;
    }

    private void TrySkip(PointerEventData eventData)
    {
        SkipToPercent(progress.value);
    }

    private void SkipToPercent(float pct)
    {
        var frame = videoPlayer.frameCount * pct;
        videoPlayer.frame = (long)frame;
    }
}