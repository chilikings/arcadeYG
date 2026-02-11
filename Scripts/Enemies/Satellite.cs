using System;
using UnityEngine;
using GAME.Utils.Core;
using GAME.Utils.Conversion;
using Random = UnityEngine.Random;

namespace GAME.Enemies.Satellite
{
    public class EnemySatellite : MonoBehaviour
    {
        GameObject _go;
        Transform _tr, _planet;
        CircleCollider2D _cl;
        Rigidbody2D _rb;
        Animator _an;
        Vector2 _direction;
        float _angleRad, _radius, _speed, _speedFactor = 1;
        bool _clockwise;

        Vector2 Offset => Conv.RadToVector(_angleRad, _radius);


        public EnemySatellite Init(Transform planet, float radius, float speed, bool clockwise)
        {
            _go ??= gameObject;
            _tr ??= transform;
            _planet ??= planet;
            _cl ??= GetComponent<CircleCollider2D>();
            _rb ??= GetComponent<Rigidbody2D>();
            _an ??= GetComponent<Animator>();
            _tr.SetParent(_planet);
            _radius = radius;
            _speed = speed;
            _clockwise = clockwise;
            return this;
        }

        public EnemySatellite Spawn()
        {
            Enable();
            EnablePhysics();
            _angleRad = Random.Range(0f, 2 * Mathf.PI); //_angleRad = Random.Range(0f, 360f);
            _tr.position = (Vector2)_planet?.position + Offset;
            return this;
        }    

        public void SetSpeedFactor(float factor) => _speedFactor = factor;

        public void StartDeath()
        {
            DisablePhysics();
            _an.SetTrigger(Helper.DeathName);
        }

        void FixedUpdate()
        {
            Rotate(Time.fixedDeltaTime);
        }

        void Rotate(float deltaTime)
        {
            _angleRad += (_clockwise ? 1 : -1) * _speedFactor * _speed * deltaTime;
            _tr.position = (Vector2)_planet.position + Offset;
        }

        void EnablePhysics() => _rb.simulated = _cl.enabled = true;
        void DisablePhysics() => _rb.simulated = _cl.enabled = false;
        void Enable() => _go.SetActive(true);
        void Disable() => _go.SetActive(false);

        //IEnumerator Rotation()
        //{
        //    while (true) 
        //    {
        //        yield return new WaitForEndOfFrame();
        //        _tr.RotateAround(_planet.position, Vector3.forward, _speed * Time.fixedDeltaTime);
        //    }
        //}  

    }

    [Serializable]
    public class SatelliteInfo
    {
        [SerializeField] Transform _prefab;
        [SerializeField] float _radius;
        [SerializeField] float _speed;
        [SerializeField] bool _clockwise;

        public Transform Prefab => _prefab;
        public float Radius => _radius;
        public float Speed => _speed;
        public bool Clockwise => _clockwise;
    }

}
