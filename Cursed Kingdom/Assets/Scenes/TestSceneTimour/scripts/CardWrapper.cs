using config;
using DG.Tweening;
using events;
using UnityEngine;
using UnityEngine.EventSystems;

public class CardWrapper : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler,
    IPointerUpHandler
{

    public float targetRotation;
    public Vector2 targetPosition;
    public float targetVerticalDisplacement;
    public int uiLayer;

    private RectTransform rectTransform;
    private Canvas canvas;

    public CardConfig cardConfig;
    public CardContainer container;

    private bool isHovered;
    private bool isDragged;
    private bool isEntering;
    private Vector2 dragStartPos;
    public bool preventCardInteraction;

    // Cached tweens so we can Kill() before restarting
    private Tween positionTween;
    private Tween rotationTween;
    private Tween scaleTween;

    // Tracked to avoid restarting identical tweens every frame
    private Vector2 lastTweenedPosition;
    private float lastTweenedRotation;
    private float lastTweenedScale;

    public float width => rectTransform.rect.width * rectTransform.localScale.x;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    private void Start()
    {
        canvas = GetComponent<Canvas>();
    }

    private void Update()
    {
        if (isDragged)
        {
            // While dragging, follow the cursor directly — no tween
            rectTransform.position = (Vector2)Input.mousePosition + dragStartPos;
            UpdateRotation();
            return;
        }

        UpdatePosition();
        UpdateRotation();
        UpdateScale();
        UpdateUILayer();
    }

    private void UpdateUILayer()
    {
        if (!isHovered && !isDragged)
        {
            canvas.sortingOrder = uiLayer;
        }
    }

    private void UpdatePosition()
    {
        if (isEntering) return;

        var target = new Vector2(targetPosition.x, targetPosition.y + targetVerticalDisplacement);
        if (isHovered && cardConfig.overrideYPosition != -1)
        {
            target = new Vector2(target.x, cardConfig.overrideYPosition);
        }

        if (target == lastTweenedPosition) return;
        lastTweenedPosition = target;

        var releasing = rectTransform.position.y > target.y || rectTransform.position.y < 0;
        var duration = releasing ? cardConfig.releasePositionDuration : cardConfig.positionDuration;
        var ease = releasing ? cardConfig.releasePositionEase : cardConfig.positionEase;

        positionTween?.Kill();
        positionTween = rectTransform
            .DOMove(target, duration)
            .SetEase(ease);
    }

    private void UpdateRotation()
    {
        var targetRot = isHovered && cardConfig.resetRotationOnZoom
            ? 0f
            : targetRotation;

        if (Mathf.Approximately(targetRot, lastTweenedRotation)) return;
        lastTweenedRotation = targetRot;

        rotationTween?.Kill();
        rotationTween = rectTransform
            .DORotate(new Vector3(0, 0, targetRot), cardConfig.rotationDuration)
            .SetEase(cardConfig.rotationEase);
    }

    private void UpdateScale()
    {
        var targetZoom = (isDragged || isHovered) && cardConfig.zoomOnHover ? cardConfig.multiplier : 1f;

        if (Mathf.Approximately(targetZoom, lastTweenedScale)) return;
        lastTweenedScale = targetZoom;

        scaleTween?.Kill();
        scaleTween = rectTransform
            .DOScale(targetZoom, cardConfig.zoomDuration)
            .SetEase(cardConfig.zoomEase);
    }

    public void SetAnchor(Vector2 min, Vector2 max)
    {
        rectTransform.anchorMin = min;
        rectTransform.anchorMax = max;
    }

    public void SnapToPosition()
    {
        var target = new Vector2(targetPosition.x, targetPosition.y + targetVerticalDisplacement);
        rectTransform.position = target;
        lastTweenedPosition = target;
    }
    public void MoveOffScreen()
    {
        rectTransform.position = new Vector2(Screen.width + rectTransform.rect.width, rectTransform.position.y + 500);

    }

    public void PlayEntryAnimation()
    {
        isEntering = true;

        var duration = cardConfig?.entryDuration ?? 0.5f;
        var ease = cardConfig?.entryEase ?? Ease.OutCubic;

        positionTween?.Kill();
        positionTween = rectTransform
            .DOMove(targetPosition, duration)
            .SetEase(ease)
            .OnComplete(() =>
            {
                isEntering = false;
                lastTweenedPosition = targetPosition;
            });
    }

    private void OnDestroy()
    {
        // Clean up any running tweens when the card is destroyed
        positionTween?.Kill();
        rotationTween?.Kill();
        scaleTween?.Kill();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isDragged || preventCardInteraction) return;
        if (cardConfig.bringToFrontOnHover)
        {
            canvas.sortingOrder = cardConfig.zoomedSortOrder;
        }
        cardConfig?.OnCardHover?.Invoke(new CardHover(this));
        isHovered = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (isDragged || preventCardInteraction) return;
        canvas.sortingOrder = uiLayer;
        isHovered = false;
        cardConfig?.OnCardUnhover?.Invoke(new CardUnhover(this));
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (preventCardInteraction) return;
        isDragged = true;

        // Kill position tween so the card doesn't fight the cursor
        positionTween?.Kill();

        dragStartPos = new Vector2(transform.position.x - eventData.position.x,
            transform.position.y - eventData.position.y);
        container.OnCardDragStart(this);
        cardConfig?.OnCardUnhover?.Invoke(new CardUnhover(this));
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isDragged = false;
        // Reset so UpdatePosition fires a fresh tween on the next frame
        lastTweenedPosition = Vector2.negativeInfinity;
        container.OnCardDragEnd();
    }
}