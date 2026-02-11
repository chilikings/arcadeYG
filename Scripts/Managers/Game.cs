using YG;
using UnityEditor;
using UnityEngine;
using GAME.Audio.SFX;
using GAME.Utils.Core;
using GAME.Settings.Game;
using UnityEngine.Events;
using GAME.Managers.Audio;
using GAME.Managers.Saves;
using System.Threading.Tasks;
using GAME.Managers.Singleton;
using UnityEngine.Localization;
using UnityEngine.SceneManagement;
using UnityEngine.Localization.Settings;

namespace GAME.Managers.Game
{
    public class GameManager : Singleton<GameManager, GameSettings>
    {
        [SerializeField] UnityEvent _onGameStart;
        [SerializeField] UnityEvent<string> _onNumberEnter;
        [SerializeField] UnityEvent _onLoseAllHearts;
        [SerializeField] UnityEvent<int> _onLivesCountChanged;
        [SerializeField] UnityEvent<float> _onEnemyDebuff;
        [SerializeField][Space] bool _logging;

        GameState _prevState;
        const int _LocaleTimeout = 5000;
        float _enemySpeedFactor = 1f;
        bool? _isMobile;


        public UnityEvent OnGameStart => _onGameStart;
        public UnityEvent OnLoseAllHearts => _onLoseAllHearts;
        public UnityEvent<int> OnLivesCountChanged => _onLivesCountChanged;
        public UnityEvent<float> OnEnemyDebuff => _onEnemyDebuff;

        public float EnemySpeedFactor => _enemySpeedFactor;
        public bool IsMobile => _isMobile ??= (_settings.Platform == Platform.Auto ? YG2.envir.isMobile : _settings.Platform == Platform.Mobile);
        public bool IsBerserk {  get; private set; }
        public GameState State { get; private set; }


        public void SetPlayerNumber(string text) => _onNumberEnter?.Invoke(text);

        public void Win() => SetGameState(GameState.Win);

        public void Lose() => SetGameState(GameState.Lose);

        public void Pause() => SetGameState(GameState.Pause);

        public void Play() => SetGameState(GameState.Playing);

        public void Launch() => SetGameState(GameState.Loading);

        public void SetPrevState() { if (_prevState != GameState.Loading) SetGameState(_prevState); }

        public void Restart()
        {
            SceneManager.LoadScene(0, LoadSceneMode.Single);
            //Helper.Reset();
        }
        
        public void SetLivesCount(int newCount)
        {
            if (newCount < 0) Invoke(nameof(Lose), 1);
            else
            {
                if (State == GameState.Lose) Play();
                _onLivesCountChanged?.Invoke(newCount);
            }
        }

        public void SetEnemySpeed(float factor)
        {
            _enemySpeedFactor = factor;
            _onEnemyDebuff?.Invoke(factor);
        }

        public void SetBerserk(bool enable = true) => IsBerserk = enable;

        protected override void Initialize()
        {
            base.Initialize();
            SetGameState(GameState.Playing);
        }

        void Start()
        {
            Launch();
            //_ = SetupLocaleAsync();
            SetupLocaleAsync();
        }

        //async void Start()
        //{
        //    await SetupLocaleAsync();
        //    Launch();
        //}

        void SetGameState(GameState state)
        {
            _prevState = State;
            State = state;
            switch (state)
            {
                case GameState.Loading:
                    //StopTime();
                    if (SaveManager.I.LoadNumber() == 0)
                        SetLivesCount(_settings.StartLivesCount);              
                    _onGameStart?.Invoke();
                    break;
                case GameState.Playing: StartTime(); break;
                case GameState.Pause: StopTime(); break;
                case GameState.Lose:
                    _onLoseAllHearts?.Invoke();
                    AudioManager.I.PlaySFX(SFXName.Lose);
                    StopTime();
                    break;
                case GameState.Win:
                    AudioManager.I.PlaySFX(SFXName.Win);
                    //StopTime(); 
                    break;
            }
            if (_logging) Helper.Log(() => State);
        }

        void StopTime() => Time.timeScale = 0;

        void StartTime() => Time.timeScale = 1;

        async void SetupLocaleAsync()
        {
            await LocalizationSettings.InitializationOperation.Task;
            if (_settings.Localization == Localization.EN) { ApplyLocale("en"); return; }
            if (_settings.Localization == Localization.RU) { ApplyLocale("ru"); return; }

            var lang = NormLocale(YG2.lang);
            if (string.IsNullOrEmpty(lang))
            {
                var tcs = new TaskCompletionSource<string>();
                void Handler(string c) => tcs.TrySetResult(c);
                YG2.onCorrectLang += Handler;

                var isLangReady = await Task.WhenAny(tcs.Task, Task.Delay(_LocaleTimeout));
                if (isLangReady == tcs.Task && tcs.Task.IsCompleted)
                    lang = NormLocale(tcs.Task.Result);

                YG2.onCorrectLang -= Handler;
            }
            var locale = lang == "ru" ? "ru" : "en";
            ApplyLocale(locale);
        }

        string NormLocale(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return null;
            s = s.Trim().ToLowerInvariant();
            int i = s.IndexOf('-');
            return i > 0 ? s.Substring(0, i) : s;
        }

        void ApplyLocale(string code)
        {
            var locs = LocalizationSettings.AvailableLocales;
            var target = locs.GetLocale(code) ??
                locs.Locales.Find(l =>
                {
                    var id = l.Identifier.Code?.ToLowerInvariant();
                    var shortId = id?.Split('-')[0];
                    return shortId == code;
                });
            if (target != null && LocalizationSettings.SelectedLocale != target)
                LocalizationSettings.SelectedLocale = target;
        }
    }

    public enum GameState
    {
        Loading,
        Playing,
        Pause,
        Lose,
        Win,
        Ads
    }
}
