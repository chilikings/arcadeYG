using GAME.Extensions.Cinemachine;
using GAME.Extensions.Common;
using GAME.Settings.Camera;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

namespace GAME.Controllers.Cameras
{
    public enum MotionState { Idle, Move }

    public class CameraController : MonoBehaviour
    {
        [SerializeField] CameraSettings _settings;

        [Space]
        [Header("DEBUG")]
        [SerializeField] MotionState _cameraState;
        [SerializeField][Space(2)] MotionState _playerState;
        [SerializeField][Space(4)] float _timeInIdle;


        Transform _brainTransform, _player;
        CinemachineCamera _majorCamera, _minorCamera;
        CinemachineBrain _brainCamera;
        CinemachinePositionComposer _majorComposer, _minorComposer;
        const string _PlayerTag = "Player", _MajorTag = "MajorCamera", _MinorTag = "MinorCamera";
        const int _LowPriority = 0, _HighPriority = 1;
        bool? _isInited, _isSetup;

        //bool IsPlayerMoving => _playerRB.linearVelocity.sqrMagnitude >= Mathf.Pow(_settings.SpeedThreshold, 2);
        bool IsReady => _isInited.IsTrue() && _isSetup.IsTrue();


        void Awake()
        {
            _isInited ??= Initialize();
        }

        void Start()
        {
            _isSetup ??= SetupAll(_settings);
        }

        void Update()
        {
            if (!IsReady) return;

            //_playerState = IsPlayerMoving ? MotionState.Move : MotionState.Idle;
        }

        bool Initialize()
        {
            CacheCameras(_MajorTag, _MinorTag);
            CachePlayer(_PlayerTag);
            return _brainCamera && _majorCamera && _minorCamera && _majorComposer && _minorComposer && _player;
        }

        void CacheCameras(string majorTag, string minorTag)
        {
            _brainCamera = GetComponentInChildren<CinemachineBrain>(true);
            _brainTransform = _brainCamera?.OutputCamera?.transform;

            _majorCamera = this.GetCameraByTag(majorTag);
            _majorComposer = _majorCamera?.GetComponent<CinemachinePositionComposer>();

            _minorCamera = this.GetCameraByTag(minorTag);
            _minorComposer = _minorCamera?.GetComponent<CinemachinePositionComposer>();
        }

        void CachePlayer(string tag)
        {
            _player = GameObject.FindGameObjectWithTag(tag)?.transform;
        }

        bool SetupAll(CameraSettings settings)
        {
            //CacheTarget();
            SetupTarget();
            ApplySettings(settings);
            SetState(MotionState.Idle);
            return _majorCamera.Target.TrackingTarget && _minorCamera.Target.TrackingTarget && _brainCamera.CustomBlends;
        }

        void SetupTarget()
        {
            _majorCamera.SetTarget(_player);
            _minorCamera.SetTarget(_player);
        }

        void ApplySettings(CameraSettings settings)
        {
            _majorCamera.BlendHint |= CinemachineCore.BlendHints.InheritPosition;
            _minorCamera.BlendHint |= CinemachineCore.BlendHints.InheritPosition;

            _brainCamera.CustomBlends = settings.BlendSettings;
            _majorComposer.Damping = settings.Damping;
            _majorCamera.Lens.OrthographicSize = settings.Distance;
            _majorComposer.TargetOffset = settings.Offset;
        }

        void SetState(MotionState state)
        {
            switch (state)
            {
                case MotionState.Idle:
                    _cameraState = MotionState.Idle;
                    SwitchCameraOn(_majorCamera, _majorComposer, _brainTransform);
                    SwitchCameraOff(_minorCamera, _minorComposer);
                    break;
                    //case MotionState.Move:
                    //    break;
            }
        }

        void SwitchCameraOff(CinemachineCamera camera, CinemachinePositionComposer composer)
        {
            camera.Priority = _LowPriority;
            composer.enabled = false;
        }

        void SwitchCameraOn(CinemachineCamera camera, CinemachinePositionComposer composer, Transform transform)
        {
            camera.Priority = _HighPriority;
            camera.Teleport(transform);
            StartCoroutine(EnableComposer(composer));
        }

        IEnumerator EnableComposer(CinemachinePositionComposer composer)
        {
            yield return null;
            composer.enabled = true;
        }

    }
}