using System;
using DG.Tweening;
using events;
using UnityEngine;
using UnityEngine.Events;

namespace config {
    [Serializable]
    public class CardConfig {

        [Header("Zoom")]
        [SerializeField]
        public bool zoomOnHover;
    
        [Range(1f, 5f)]
        [SerializeField]
        public float multiplier = 1;
        
        [Tooltip("This is the Y position of the card when it is zoomed in. If this is -1, the card will not be moved on the Y axis.")]
        [SerializeField]
        public float overrideYPosition = -1;

        [Header("UI Layer")]
        [Tooltip("This is the sorting order of the first card when it is not zoomed in. Subsequent cards will have a higher sorting order.")]
        [SerializeField]
        public int defaultSortOrder;
        
        [SerializeField]
        public bool bringToFrontOnHover;
        
        [Tooltip("This is the sorting order of the card when it is zoomed in.")]
        [SerializeField]
        public int zoomedSortOrder;

        [SerializeField]
        public bool resetRotationOnZoom;

        [Header("Animation Speed")]
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

        [Header("Entry Animation")]
        [Tooltip("Seconds for a new card to fly in from off-screen")]
        public float entryDuration = 0.5f;
        public Ease entryEase = Ease.OutCubic;

        [Header("Events")]
        [SerializeField]
        public UnityEvent<CardPlayed> OnCardPlayed;

        [SerializeField]
        public UnityEvent<CardHover> OnCardHover;

        [SerializeField]
        public UnityEvent<CardUnhover> OnCardUnhover;

        [SerializeField]
        public UnityEvent<CardDestroy> OnCardDestroy;

        [Header("Card Play Area")]
        [SerializeField]
        public RectTransform playArea;

        [SerializeField]
        public bool destroyOnPlay;
    }
}
