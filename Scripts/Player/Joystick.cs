using UnityEngine;
using GAME.Managers.Game;
using GAME.Extensions.UI;
using UnityEngine.UIElements;

namespace GAME.Player.Joystick
{
    [RequireComponent(typeof(UIDocument))]
    public class PlayerJoystick : MonoBehaviour
    {
        [SerializeField][Range(10, 30)][Tooltip("% from Area size")] float _baseSize = 20f;
        [SerializeField][Range(20, 80)][Space][Tooltip("% from Base size")] float _handleSize = 50f;
        [SerializeField][Range(20, 80)][Space(2)][Tooltip("% from Base size")] float _handleLength = 50f;

        const float _OnePercent = 0.01f;
        const string _Area = "JoystickArea", _Base = "JoystickBase", _Handle = "JoystickHandle";
        VisualElement _root, _area, _base, _handle;
        Vector2 _direction, _basePosition;
        float _baseHalf, _handleHalf, _handleLengthPx;
        bool _isVisible = true, _isReged = false;
        bool? _isInited;


        public Vector2 Direction => _direction;

        void OnEnable()
        {
            _isInited ??= Initialize();
            RegisterAll(ref _isReged);
            Hide(ref _isVisible);
        }

        void OnDisable()
        {
            UnregisterAll(ref _isReged);
            Hide(ref _isVisible);
        }

        bool Initialize()
        {
            _root = GetComponent<UIDocument>().rootVisualElement;
            _area = _root.Q<VisualElement>(_Area);
            _base = _root.Q<VisualElement>(_Base);
            _handle = _root.Q<VisualElement>(_Handle);
            return true;
        }

        void OnGeometryChanged(GeometryChangedEvent evt)
        {
            float areaWidth = _area.layout.width, basePx = areaWidth * _baseSize * _OnePercent, handlePx = basePx * _handleSize * _OnePercent;
            _handleLengthPx = basePx * _handleLength * _OnePercent;
            _baseHalf = basePx * 0.5f;
            _handleHalf = handlePx * 0.5f;
            _base.SetSquareSize(basePx);
            _handle.SetSquareSize(handlePx);
        }

        void OnDown(PointerDownEvent evt)
        {
            if (!GameManager.I.IsMobile || _isVisible) return;
            _area.CapturePointer(evt.pointerId);
            Show(ref _isVisible);
            SetPosition(evt);
        }

        void SetPosition(PointerDownEvent evt)
        {
            var touchPos = evt.localPosition;
            _basePosition = touchPos;
            _base.style.left = touchPos.x - _baseHalf;
            _base.style.top = touchPos.y - _baseHalf;
            SetHandlePosition(touchPos);
        }

        void SetHandlePosition(Vector2 touchPos)
        {
            _handle.style.left = touchPos.x - _handleHalf;
            _handle.style.top = touchPos.y - _handleHalf;
        }

        void OnMove(PointerMoveEvent e)
        {
            if (!_isVisible) return;
            Vector2 touchPos = e.localPosition, touchDist = touchPos - _basePosition,
                    clampDist = Vector2.ClampMagnitude(touchDist, _handleLengthPx),
                    handlePos = _basePosition + clampDist;
            _direction = new Vector2(clampDist.x, -clampDist.y) / _handleLengthPx;
            SetHandlePosition(handlePos);
        }

        void OnUp(PointerUpEvent e)
        {
            if (!_isVisible) return;
            _area.ReleasePointer(e.pointerId);
            _direction = Vector2.zero;
            Hide(ref _isVisible);
        }

        void RegisterAll(ref bool isReged)
        {
            if (isReged) return;
            _area.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            _area.RegisterCallback<PointerDownEvent>(OnDown);
            _area.RegisterCallback<PointerMoveEvent>(OnMove);
            _area.RegisterCallback<PointerUpEvent>(OnUp);
            isReged = true;
        }

        void UnregisterAll(ref bool isReged)
        {
            if (!isReged) return;
            _area.UnregisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            _area.UnregisterCallback<PointerDownEvent>(OnDown);
            _area.UnregisterCallback<PointerMoveEvent>(OnMove);
            _area.UnregisterCallback<PointerUpEvent>(OnUp);
            isReged = false;
        }

        void Hide(ref bool isVisible) { if (isVisible) isVisible = _base.visible = _handle.visible = false;}
        void Show(ref bool isVisible) { if (!isVisible) isVisible = _base.visible = _handle.visible = true; }

    }
}
