using UnityEngine;
using GAME.Utils.Core;
using GAME.Extensions.Lines;

namespace GAME.Level.Zone.Border
{
    [RequireComponent(typeof(LineRenderer))]
    public class ZoneBorder : MonoBehaviour
    {
        [SerializeField] Gradient _color;
        [SerializeField][Space(4)][Range(0, 0.5f)] float _width;
        [SerializeField][Space(2)][Range(0, 10)] int _smoothness;

        Transform _tr;
        LineRenderer _lr;


        public void SetPoints(Vector2[] points, bool loop)
        {
            points = Helper.TransPoints(points, _tr, false);
            _lr.SetPoints(points, loop);
        }

        public void Reset() => _lr.positionCount = 0;

        void Awake()
        {
            _tr = transform;
            _lr = GetComponent<LineRenderer>();
        }

        void Start()
        {
            ApplySettings(_color, _width, _smoothness);
        }

        void ApplySettings(Gradient color, float width, int smoothness)
        {
            _lr.colorGradient = color;
            _lr.endWidth = _lr.startWidth = width;
            _lr.numCapVertices = _lr.numCornerVertices = smoothness;
        }

        void CreatePoint(Vector2 point, string name = "Point")
        {
            var obj = new GameObject(name);
            obj.transform.position = point;
        }

    }
}
