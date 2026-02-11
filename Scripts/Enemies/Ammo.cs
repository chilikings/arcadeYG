using UnityEngine;
using GAME.Audio.SFX;
using GAME.Managers.Game;
using GAME.Managers.Audio;
using GAME.Extensions.Colliders;

namespace GAME.Enemies.Ammo
{
    public class EnemyAmmo : MonoBehaviour
    {
        [SerializeField][Range(0, 10)] float _speed = 6f;
        [SerializeField][Range(1, 10)] float _lifeTime = 4f;

        AudioManager AUDIO;
        Rigidbody2D _rb;
        GameObject _go;
        Transform _tr;
        bool? _isInited;
        Vector2 _moveDirection;
        float _speedFactor = 1f;

        public void Shoot(Vector2 position, Vector2 direction, bool bounce)
        {
            _isInited ??= InitComponents(bounce);

            Enable();
            _tr.position = position;
            _moveDirection = direction;
            _speedFactor = GameManager.I.EnemySpeedFactor;  
            SetVelocity();
            Invoke(nameof(Disable), _lifeTime);
            AUDIO.PlaySFX(SFXName.Shot);
        }

        public void Disable() => _go?.SetActive(false);


        void Awake() 
        {
            GameManager.I.OnEnemyDebuff.AddListener(SetSpeedFactor);
        }

        void OnDisable()
        {
            CancelInvoke();
        }

        void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.collider.IsFieldEdge()) AUDIO.PlaySFX(SFXName.Ricochet);        
        }

        bool InitComponents(bool bounce)
        {
            _go = gameObject;
            _tr = transform;
            _rb = GetComponent<Rigidbody2D>();
            AUDIO = AudioManager.I;
            return true;
        }
       
        void SetSpeedFactor(float factor)
        {
            _speedFactor = factor;
            SetVelocity();
        }

        void SetVelocity()
        {
            _rb.linearVelocity = _speed * _speedFactor * _moveDirection;
        }

        void Enable() => _go.SetActive(true);

    }
}
