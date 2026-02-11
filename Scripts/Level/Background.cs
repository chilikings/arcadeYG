using UnityEngine;
using GAME.Managers.Level;
using GAME.Settings.Levels;
using GAME.Extensions.Sprites;

namespace GAME.Level.Background
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class LevelBackground : MonoBehaviour
    {
        //[SerializeField, Range(0.5f, 2f), Space(2)] float _sizeFactor = 1.5f;
        [SerializeField, Space(4)] PolygonCollider2D _cameraBounds;

        const string _ShadowSprite = "Inner Shadow";
        Transform _shadowTransform;
        LevelManager LEVEL;
        SpriteRenderer _bgrndSprite, _shadowSprite;
        bool? _isInited;


        public void ApplyLevelInfo(LevelInfo level)
        {
            _isInited ??= Init();
            if (_bgrndSprite.sprite != LEVEL.Background)
            {
                _bgrndSprite.sprite = LEVEL.Background;
                _bgrndSprite.color = Color.white;
                FitToScreen();
            }
        }

        public void FitToScreen()
        {
            _isInited ??= Init();
            //_sr.FitToScreen(_sizeFactor);
            _bgrndSprite.size = _cameraBounds.bounds.size;
            _shadowSprite.FillScreen();
        }

        bool Init()
        {
            LEVEL = LevelManager.I;
            _bgrndSprite = GetComponent<SpriteRenderer>();
            _shadowTransform = transform.Find(_ShadowSprite);
            _shadowSprite = _shadowTransform?.GetComponent<SpriteRenderer>();
            return true;
        }

        void LateUpdate()
        {
            _shadowTransform.position = Camera.main.transform.position;
        }

    }
}
