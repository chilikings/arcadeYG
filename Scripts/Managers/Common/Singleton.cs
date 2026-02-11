using UnityEngine;

namespace GAME.Managers.Singleton
{
    public abstract class Singleton<TManager, TSettings> : MonoBehaviour where TManager : MonoBehaviour
    {
        public static TManager I => _this;

        [SerializeField] protected TSettings _settings;

        protected static TManager _this;


        //public virtual void Setup(TSettings settings)
        //{
        //    if (settings is not null)
        //    {
        //        if (_settings is null) _settings = settings;
        //        else LogWarning(_settings, true);
        //    }
        //    else LogWarning(settings, false);
        //}

        protected virtual void Awake()
        {
            if (_this is null) Initialize();
            else LogWarning(_this, true);
        }

        protected virtual void Initialize()
        {
            _this = GetComponent<TManager>();
            //DontDestroyOnLoad(gameObject);

            //else if (_instance != this)
            //{
            //    Destroy(gameObject);
            //    LogWarning(_instance, true);
            //}
        }

        protected virtual void OnDestroy()
        {
            if (_this == this) _this = null;
        }
        void LogWarning<T>(T value, bool isInited)
        {
            Debug.LogWarning($"[{typeof(T).Name}] is {(isInited ? "already Initialized" : "Not Initialized yet")}");
        }
        
    }
}