using System;
using UnityEngine;

namespace Modes.Coloring
{
    [Serializable]
    public class EyesSettings
    {
        [field: SerializeField] public float eyeMaxRadius = 0.1f;

        [Space]
        [field: SerializeField] public float eyeMovementSpeed = 1;
        [field: SerializeField] public float resetTargetPositionDelay = 2f;

        [Space]
        [field: SerializeField] public Vector2 eyesOpenDuration = new Vector2(2f, 10f);
        [field: SerializeField] public Vector2 eyesClosedDuration = new Vector2(0.1f, 1f);
    }
}