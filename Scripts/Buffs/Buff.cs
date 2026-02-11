using UnityEngine;
using GAME.Utils.Core;
using System.Collections;
using GAME.Extensions.Colliders;

namespace GAME.Buffs
{
    public enum BuffType { None, Boost, Shield, Slowmo, Berserk }

    [RequireComponent(typeof(Collider2D), typeof(SpriteRenderer))]
    public class Buff : MonoBehaviour
    {
        [SerializeField] BuffType _type;
        [SerializeField][Range(0, 10)][Space(2)] float _lifeTime;
        [SerializeField][Range(0, 10)] float _duration;
        [SerializeField][Range(0, 2)][Space(2)] float _value;
        [SerializeField][Space(4)] Sprite _halo;
        [SerializeField][Space(2)] Color _color;
        [SerializeField][Space] bool _logging;

        const string _Absorb = "Absorb", _FadeOut = "FadeOut";
        GameObject _go;
        Transform _tr;
        Collider2D _cl;
        Coroutine _living;
        Animator _animator;
        bool _isLoaded = false;

        public BuffType Type => _type;
        public Sprite Halo => _halo;
        public Color Color => _color;
        public float Duration => _duration;
        public float Value => _value;


        public void Spawn(Vector2 position, Transform spawner)
        {
            if (!_isLoaded) _isLoaded = CacheComponents(spawner);
            Enable();
            _tr.position = position;
            Helper.StopCoroutine(ref _living, this);
            _living ??= StartCoroutine(Living());
            if (_logging) Debug.Log($"[{_tr.name}]: Spawn");
        }

        public void Disable() => _go?.SetActive(false);

        void OnTriggerEnter2D(Collider2D collider)
        {
            if (!_cl.enabled)
            {
                if (_logging) Debug.LogWarning($"Collision Cancelled");
                return;
            }
            if (collider.IsZone()) FadeOut();
            else if (collider.IsPlayer()) Absorb();
        }

        bool CacheComponents(Transform parent)
        {
            _tr = transform;
            _go = gameObject;
            _cl = GetComponent<Collider2D>();
            _animator = GetComponent<Animator>();
            return true;
        }

        IEnumerator Living()
        {
            yield return new WaitForSeconds(_lifeTime);
            _living = null;
            FadeOut();
        }

        void Absorb() => _animator.SetTrigger(_Absorb);

        void FadeOut() => _animator.SetTrigger(_FadeOut);

        void EnableCollider() => _cl.enabled = true;

        void DisableCollider() => _cl.enabled = false;

        void Enable() => _go?.SetActive(true);

    }
}
