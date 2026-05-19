using System;
using DG.Tweening;
using Helpers;
using Loggers;
using Modes.Coloring;
using Services;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

public class Move_FX : MonoBehaviour
{
    public float fail_ShakePos_Duration = 0.5f;
    public float fail_ShakePos_Strength = 0.25f;
    public int fail_ShakePos_Vibro = 25;

    [Space]
    public float fail_ShakeScale_Duration = 0.5f;
    public float fail_ShakeScale_Strength = 0.25f;
    public int fail_ShakeScale_Vibro = 25;

    [Space]
    public float sucess_ShakePos_Duration = 0.25f;
    public float sucess_ShakePos_Strength = 0.1f;
    public int sucess_ShakePos_Vibro = 5;

    [Space]
    public float sucess_ShakeScale_Duration = 0.25f;
    public float sucess_ShakeScale_Strength = 0.1f;
    public int sucess_ShakeScale_Vibro = 5;

    [Inject] private static Content _content;

    [Button]
    public void Fail()
    {
        Fail(transform);
    }

    [Button]
    public void Success()
    {
        Success(transform);
    }

    public void Fail(Transform go, float duration = 0)
    {
        if (duration == 0) { duration = fail_ShakePos_Duration; }

        go.DOKill();
        go.DOShakePosition(fail_ShakePos_Duration, fail_ShakePos_Strength, fail_ShakePos_Vibro);
        go.DOShakeScale(fail_ShakeScale_Duration, fail_ShakeScale_Strength, fail_ShakeScale_Vibro);
    }

    public void Success(Transform go, float duration = 0)
    {
        if (duration == 0) { duration = fail_ShakeScale_Duration; }

        go.DOKill();
        go.DOShakeScale(sucess_ShakeScale_Duration, sucess_ShakeScale_Strength, sucess_ShakeScale_Vibro);
    }


    public static void MakeDonePunch(Transform obj, int? Scale = null)
    {
        try
        {
            var scale = obj.localScale;
            obj.DOPunchScale(scale * 0.5f, 0.5f, 7, 2f).OnKill(() => { obj.DOScale(scale, 0.25f); });
        } catch (Exception e) { CustomLogger.instance?.LogException(e); }
    }

    public static async void MakeDoneParticle(Vector3 position, float? scale = null)
    {
        try
        {
            if (_content == null) { _content = DiService.Get<Content>(); }
            if (scale == null) { scale = 1; }

            var particle = await _content.mediumConfetti.GetRandom().InstantiateAsync();
            particle.transform.position = position;
            particle.transform.localScale = Vector3.one * scale.Value;
            particle.Play();
            Destroy(particle.gameObject, particle.main.duration);
        } catch (Exception e) { CustomLogger.instance?.LogException(e); }
    }
}