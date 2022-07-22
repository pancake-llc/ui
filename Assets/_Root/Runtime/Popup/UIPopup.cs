#pragma warning disable 0649
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Pancake.Common;
using Pancake.Tween;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Pancake.UI
{
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(Canvas))]
    [RequireComponent(typeof(GraphicRaycaster))]
    public abstract class UIPopup : MonoBehaviour, IPopup
    {
        #region implementation

        [SerializeField] protected bool ignoreTimeScale;
        [SerializeField] protected UnityEvent onBeforeShow;
        [SerializeField] protected UnityEvent onAfterShow;
        [SerializeField] protected UnityEvent onBeforeClose;
        [SerializeField] protected UnityEvent onAfterClose;
        [SerializeField] protected bool closeByClickContainer;
        [SerializeField] protected bool closeByClickBackground;
        [SerializeField] protected bool closeByBackButton;
        [SerializeField] private List<Button> closeButtons = new List<Button>();
        [SerializeField] protected Vector2 startScale;

        public EMotionAffect motionAffectDisplay = EMotionAffect.Scale;
        public Vector2 endValueDisplay = Vector2.one;
        public Vector2 positionToDisplay;
        [Range(0.01f, 3f)] public float durationDisplay = 0.25f;
        public Interpolator interpolatorDisplay;

        public EMotionAffect motionAffectHide = EMotionAffect.Scale;
        public Vector2 endValueHide = Vector2.zero;
        public Vector2 positionToHide;
        [Range(0.01f, 3f)] public float durationHide = 0.25f;
        public Interpolator interpolatorHide;

        [SerializeField] private Canvas canvas;
        [SerializeField] private GraphicRaycaster graphicRaycaster;
        [SerializeField] private RectTransform backgroundTransform;
        [SerializeField] private Canvas backgroundCanvas;
        [SerializeField] private GraphicRaycaster backgroundGraphicRaycaster;
        [SerializeField] private CanvasGroup backgroundCanvasGroup;
        [SerializeField] private RectTransform containerTransform;
        [SerializeField] private Canvas containerCanvas;
        [SerializeField] private GraphicRaycaster containerGraphicRaycaster;
        [SerializeField] private CanvasGroup containerCanvasGroup;

        private bool _canActuallyClose;
        private Vector3 _defaultContainerScale;
        private CancellationTokenSource _tokenSourceCheckPressButton;
        public GameObject GameObject => gameObject;
        public bool BackButtonPressed { get; private set; }
        public Canvas Canvas => canvas;
        public bool CloseByBackButton => closeByBackButton;
        public bool CloseByClickBackground => closeByClickBackground;
        public bool CloseByClickContainer => closeByClickContainer;
        public GraphicRaycaster GraphicRaycaster => graphicRaycaster;
        public RectTransform BackgroundTransform => backgroundTransform;
        public Canvas BackgroundCanvas => backgroundCanvas;
        public GraphicRaycaster BackgroundGraphicRaycaster => backgroundGraphicRaycaster;
        public CanvasGroup BackgroundCanvasGroup => backgroundCanvasGroup;
        public Canvas ContainerCanvas => containerCanvas;
        public RectTransform ContainerTransform => containerTransform;
        public CanvasGroup ContainerCanvasGroup => containerCanvasGroup;
        public GraphicRaycaster ContainerGraphicRaycaster => containerGraphicRaycaster;
        public bool Active { get; protected set; }
        public CancellationTokenSource TokenSourceCheckPressButton => _tokenSourceCheckPressButton ?? (_tokenSourceCheckPressButton = new CancellationTokenSource());


#if UNITY_EDITOR
        /// <summary>
        /// do not use this
        /// </summary>
        [Obsolete] public List<Button> CloseButtons => closeButtons;
#endif

        private void Awake() { _defaultContainerScale = containerTransform.localScale; }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape)) BackButtonPressed = true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public virtual async void Show(CancellationToken token = default)
        {
            var btns = GetClosePopupButtons();
            OnBeforeShow();
            ActivePopup();
            MotionDisplay();

            using (_tokenSourceCheckPressButton = CancellationTokenSource.CreateLinkedTokenSource(token))
            {
                try
                {
                    var linkedToken = TokenSourceCheckPressButton.Token;
                    var buttonTask = PopupHelper.SelectButton(linkedToken, btns);
                    Task finishedTask;
                    if (closeByBackButton)
                    {
                        var pressBackButtonTask = PopupHelper.WaitForPressBackButton(this, linkedToken);
                        //_tokenSourceCheckPressButton.Token.ThrowIfCancellationRequested();
                        finishedTask = await Task.WhenAny(buttonTask, pressBackButtonTask);
                    }
                    else
                    {
                        //_tokenSourceCheckPressButton.Token.ThrowIfCancellationRequested();
                        finishedTask = await Task.WhenAny(buttonTask);
                    }

                    await finishedTask; // Propagate exception if the task finished because of exceptio
                    TokenSourceCheckPressButton?.Cancel();
                }
                finally
                {
                    TokenSourceCheckPressButton?.Dispose();
                    if (Application.isPlaying) Close();
                }
            }

            OnAfterShow();
        }

        /// <summary>
        /// get close button
        /// </summary>
        /// <returns></returns>
        protected virtual Button[] GetClosePopupButtons() { return closeButtons.ToArray(); }

        /// <summary>
        /// close popup
        /// </summary>
        public virtual void Close()
        {
            OnBeforeClose();
            MotionHide();
            ActuallyClose();
        }

        protected virtual async void ActuallyClose()
        {
            while (!_canActuallyClose)
            {
                await Task.Yield();
            }

            DeActivePopup();
            OnAfterClose();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sortingOrder"></param>
        public virtual void UpdateSortingOrder(int sortingOrder) { canvas.sortingOrder = sortingOrder; }

        /// <summary>
        /// 
        /// </summary>
        public virtual void Refresh() { }

        /// <summary>
        /// 
        /// </summary>
        public void ActivePopup()
        {
            Active = true;
            gameObject.SetActive(true);
            BackButtonPressed = false;
            _canActuallyClose = false;
        }

        /// <summary>
        /// 
        /// </summary>
        public void DeActivePopup()
        {
            Active = false;
            gameObject.SetActive(false);
        }

        public virtual void Rise()
        {
            if (BackgroundCanvasGroup != null) BackgroundCanvasGroup.alpha = 1;
        }

        public virtual void Collapse() { if (BackgroundCanvasGroup != null) BackgroundCanvasGroup.alpha = 0; }

        #endregion

        #region abstract behaviour

        /// <summary>
        /// before popup showing
        /// </summary>
        protected virtual void OnBeforeShow() { onBeforeShow?.Invoke(); }

        /// <summary>
        /// after popup showing
        /// </summary>
        protected virtual void OnAfterShow() { onAfterShow?.Invoke(); }

        /// <summary>
        /// before popup closed
        /// </summary>
        protected virtual void OnBeforeClose() { onBeforeClose?.Invoke(); }

        /// <summary>
        /// after popup closed
        /// </summary>
        protected virtual void OnAfterClose()
        {
            Popup.Close();
            onAfterClose?.Invoke();
            TokenSourceCheckPressButton?.Dispose();
        }

        private void OnApplicationQuit()
        {
            TokenSourceCheckPressButton?.Cancel();
            TokenSourceCheckPressButton?.Dispose();
        }

        #endregion

        #region motion
        
        /// <summary>
        /// 
        /// </summary>
        protected virtual void MotionDisplay()
        {
            containerGraphicRaycaster.enabled = false;
            containerTransform.gameObject.SetActive(true);
            switch (motionAffectDisplay)
            {
                case EMotionAffect.Scale:
                    containerTransform.localScale = startScale;
                    containerTransform.TweenLocalScale(endValueDisplay, durationDisplay)
                        .SetEase(interpolatorDisplay)
                        .OnComplete(() => containerGraphicRaycaster.enabled = true)
                        .Play();
                    break;
                case EMotionAffect.Position:
                    containerTransform.localScale = _defaultContainerScale;
                    containerTransform.localPosition = positionToHide;
                    containerTransform.TweenLocalPosition(positionToDisplay, durationDisplay)
                        .SetEase(interpolatorDisplay)
                        .OnComplete(() => containerGraphicRaycaster.enabled = true)
                        .Play();
                    break;
                case EMotionAffect.PositionScale:
                    containerTransform.localScale = startScale;
                    containerTransform.localPosition = positionToHide;
                    var sequense = TweenManager.Sequence();
                    sequense.Join(containerTransform.TweenLocalScale(endValueDisplay, durationDisplay).SetEase(interpolatorDisplay));
                    sequense.Join(containerTransform.TweenLocalPosition(positionToDisplay, durationDisplay).SetEase(interpolatorDisplay));
                    sequense.OnComplete(() => containerGraphicRaycaster.enabled = true);
                    sequense.Play();
                    break;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected virtual void MotionHide()
        {
            containerGraphicRaycaster.enabled = false;

            void End()
            {
                containerTransform.gameObject.SetActive(false);
                _canActuallyClose = true;
            }

            switch (motionAffectHide)
            {
                case EMotionAffect.Scale:
                    containerTransform.TweenLocalScale(endValueHide, durationHide).SetEase(interpolatorHide).OnComplete(End).Play();
                    break;
                case EMotionAffect.Position:
                    containerTransform.TweenLocalPosition(positionToHide, durationHide).SetEase(interpolatorHide).OnComplete(End).Play();
                    break;
                case EMotionAffect.PositionScale:
                    var sequense = TweenManager.Sequence();
                    sequense.Join(containerTransform.TweenLocalScale(endValueHide, durationHide).SetEase(interpolatorHide));
                    sequense.Join(containerTransform.TweenLocalPosition(positionToHide, durationHide).SetEase(interpolatorHide));
                    sequense.OnComplete(End);
                    sequense.Play();
                    break;
            }
        }

        #endregion
    }
}