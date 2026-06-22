using System;
using DG.Tweening;
using UnityEngine;

namespace config
{
    [Serializable]
    public class AnimationSpeedConfig
    {
        [Header("Position")]
        [Tooltip("Seconds to reach the target position while settling")]
        public float positionDuration = 0.3f;
        public Ease positionEase = Ease.OutQuart;

        [Tooltip("Seconds to reach the target position when releasing a dragged card")]
        public float releasePositionDuration = 0.15f;
        public Ease releasePositionEase = Ease.OutBack;

        [Header("Rotation")]
        [Tooltip("Seconds to reach the target rotation")]
        public float rotationDuration = 0.3f;
        public Ease rotationEase = Ease.OutCubic;

        [Header("Scale")]
        [Tooltip("Seconds to reach the target scale")]
        public float zoomDuration = 0.15f;
        public Ease zoomEase = Ease.OutCubic;
    }
}
