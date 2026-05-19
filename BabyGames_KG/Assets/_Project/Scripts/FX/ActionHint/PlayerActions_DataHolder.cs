using Agents;
using CustomAttributes;
using Helpers;
using Services;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerActions_DataHolder : MonoBehaviour
{
    public static PlayerActions_DataHolder instance;

    public Transform hintVisual_Hand;
    public AnimationAgent hintVisual_HandWithAnimation;

    [Fg_Se] public float drawAnimationDuration = 5;

    public DateTime lastActionTime { get; private set; } = DateTime.Now;
    [field: SerializeField] public float timeSinceLastAction { get; private set; }

    public void Awake()
    {
        instance = this;
        ResetTime();
    }

    private void OnEnable()
    {
        LazyUpdator_Service.instance.AddToQueue(UpdateData);
    }

    private void OnDisable()
    {
        LazyUpdator_Service.instance.RemoveFromQueue(UpdateData);
    }

    private async Task UpdateData()
    {
        if (Pointer.current?.press.isPressed == true)
        {
            ResetTime();
            return;
        }

        timeSinceLastAction = (DateTime.Now - lastActionTime).Seconds;
        await AsyncHelper.NextFrame();
    }

    [Button]
    public static void ResetTime()
    {
        instance.lastActionTime = DateTime.Now;
        instance.timeSinceLastAction = 0f;
    }
}