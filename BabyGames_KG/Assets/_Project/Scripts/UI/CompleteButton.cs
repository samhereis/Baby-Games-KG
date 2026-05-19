using DG.Tweening;
using Helpers;
using Loggers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class CompleteButton : MonoBehaviour, IPointerClickHandler
{
    public static CompleteButton instance;

    public bool hasEverClicked;

    public Action onBeforeClick;
    public UnityEvent onClick;

    [SerializeField] private Graphic _graphic;
    [SerializeField] private Graphic _outline;
    [SerializeField] private Animator _animation;

    public List<Func<Task>> actionsOnComplete = new();

    private void Awake()
    {
        instance = this;
    }

    private void OnDestroy()
    {
        actionsOnComplete.Clear();
    }

    private void Update()
    {
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            Click();
        }
    }

    public void SetAcitve(bool setActive)
    {
        if (_graphic == null) { _graphic = GetComponent<Graphic>(); }
        if (_animation == null) { _animation = GetComponent<Animator>(); }

        transform?.DOScale(setActive ? 1 : 0, 0.25f);
        gameObject?.SetActive(setActive);

        _graphic?.SetEnabled(setActive);

        if (_animation != null)
        {
            _animation.SetEnabled(!hasEverClicked);
            _outline?.SetEnabled(!hasEverClicked);
            if (hasEverClicked == false)
            {
                _animation.SetEnabled(true);
                _animation.Play("CompleteButton");
            }
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Click();
    }

    private async void Click()
    {
        try
        {
            onBeforeClick?.Invoke();
        }
        catch (Exception ex)
        {
            CustomLogger.instance.LogException(ex);
        }

        try
        {
            actionsOnComplete.RemoveNulls();
            await Task.WhenAll(actionsOnComplete.Select(x => x?.Invoke()));
        }
        catch (Exception e)
        {
            CustomLogger.instance.LogException(e);
        }

        try
        {
            onClick?.Invoke();
        }
        catch (Exception e)
        {
            CustomLogger.instance.LogException(e);
        }

        hasEverClicked = true;
    }
}