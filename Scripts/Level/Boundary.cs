using UnityEngine;
using UnityEngine.Events;
using GAME.Extensions.Colliders;

namespace GAME.Level.Boundary
{
    [RequireComponent(typeof(EdgeCollider2D))]
    public class LevelBoundary : MonoBehaviour
    {
        [SerializeField, Range(0, 0.2f), Space(2)] float _topbarHeight = 0.1f;
        [SerializeField, Range(0.5f, 2f), Space(4)] float _confinerFactor = 1.5f;
        [SerializeField, Space(4)] Vector2 _boundarySize = new(60, 35);
        //[SerializeField, Space(2)] Vector2 _confinerSize;
        [SerializeField, Space(4)] Vector2 _offset = Vector2.zero;
        [SerializeField, Space(6)] UnityEvent<Bounds> _onResize;
        [SerializeField, Space(8)] bool _logging;

        Transform _tr;
        EdgeCollider2D _playerBounds;
        PolygonCollider2D _cameraBounds;


        public void FitToScreen()
        {
            if (_logging) Debug.Log("[LevelBoundary]: Fit Bounds to Screen");
            //_ec?.FitToBounds(_tr.position, _topbarHeight);
            _playerBounds?.FitToBounds(_boundarySize, _offset);
            _cameraBounds?.FitToBounds(_boundarySize * _confinerFactor, _offset);
            _onResize?.Invoke(_playerBounds.bounds);
            //Physics2D.SyncTransforms();
        }

        void Awake()
        {
            _tr = GetComponent<Transform>();
            _playerBounds = GetComponent<EdgeCollider2D>();
            _cameraBounds = GetComponent<PolygonCollider2D>();
        }

        void Start()
        {
            _onResize?.Invoke(_playerBounds.bounds);
        }

    }
}