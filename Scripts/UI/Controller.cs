using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GAME.Extensions.Sprites;
using GAME.Extensions.UI;
using GAME.Managers.AD;
using GAME.Managers.Audio;
using GAME.Managers.Game;
using GAME.Managers.Level;
using GAME.Managers.Saves;
using GAME.Managers.UI;
using GAME.Settings.Rewards;
using GAME.UI.Rewards;
using GAME.Utils.Core;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UIElements;
using static UnityEngine.InputSystem.InputAction;


namespace GAME.UI.Controller
{
    public class UIController : MonoBehaviour
    {
        [Header("VISUALS")]
        [SerializeField] Gradient _topbarColor;
        [SerializeField][Range(50, 100)][Space(4)] float _pictureHeight;
        [Header("ANIMATION")]
        [SerializeField][Range(0.1f, 1.0f)] float _pulseInterval;
        [SerializeField][Space] Vector2 _labelPulseRange = new(44, 45);
        [SerializeField][Range(0, 3)][Space(2)] float _pressLabelPulseDelay = 2;
        [SerializeField][Space(5)] Vector2 _buttonPulseRange = new(1f, 1.1f);
        [SerializeField][Range(0, 3)][Space(4)] float _startScreenFadeDuration = 1;
        [SerializeField][Range(0, 3)][Space(2)] float _finalScreenFadeDuration = 1;
        [Header("EVENTS")]
        [SerializeField] InputActionReference _submit;
        [SerializeField] InputActionReference _cancel;
        [SerializeField][Space(4)] UnityEvent _onScreenResize;
        [SerializeField][Space(2)] UnityEvent _onHeartClick;
        [SerializeField][Space] bool _logging;


        const string Topbar = "topBar", ProgressBar = "ProgressBar", HeartBar = "heartBar", HeartItem = "heart",
                     PauseButton = "pauseButton", RestartButton = "restartButton", MusicButton = "musicButton", SoundButton = "soundButton", SkipButton = "skipButton", InfoButton = "infoButton", StartGameButton = "StartGameButton", RestartGameButton = "NewGameButton",
                     StartButton = "startLevelButton", PrevButton = "prevButton", HeartButton = "heartPlusButton", ContinueButton = "continueButton", CloseButton = "closeButton", СloseButtonContainer = "closeButtonContainer",
                     PicturePlaceholder = "FinalImagePlaceholder", InfoScreenButton = "infoScreenButton",
                     MusicIcon = "musicIcon", SoundIcon = "soundIcon", PauseIcon = "pauseIcon", SkipIcon = "skipIcon", HeartIcon = "heartIcon", DebugPanel = "DebugPanel",
                     WinLabel = "winText", ContinueLabel = "continueText", ContinueLabelClick = "continueTextClick", NextLevelLabel = "nextLevelText", ClickToContinueLabel = "ClickToContinueText", StartScreen = "WelcomeScreen",
                     LevelEndScreen = "LevelEndScreen", PauseScreen = "PauseScreen", PictureScreen = "FinalImageScreen", InfoScreen = "InfoScreen", FinalScreen = "CompleteScreen",
                     ProgressFill = "unity-progress-bar__progress", ResetButton = "ResetButton", UnlockButton = "UnlockButton";

        Dictionary<string, Button> _buttons = new() { { PauseButton, null }, { RestartButton, null }, { MusicButton, null }, { SoundButton, null },{ SkipButton, null }, { PictureScreen, null},
                                                      { StartGameButton, null}, { HeartButton, null }, { ContinueButton, null }, { CloseButton, null }, { StartButton, null }, { PrevButton, null },
                                                      { InfoButton, null }, { StartScreen, null }, { InfoScreenButton, null },
                                                      { PicturePlaceholder, null }, { RestartGameButton, null }, { ResetButton, null }, { UnlockButton, null }};
        Dictionary<string, VisualElement> _elements = new() {  { Topbar, null }, { LevelEndScreen, null }, { PauseScreen, null }, {СloseButtonContainer, null },
                                                               { HeartItem, null }, { HeartBar, null }, { InfoScreen, null }, { FinalScreen, null },
                                                               { MusicIcon, null }, { SoundIcon, null }, { PauseIcon, null }, { SkipIcon, null } , { DebugPanel, null }};
        Dictionary<string, Label> _labels = new() { { WinLabel, null }, { ContinueLabel, null }, { ContinueLabelClick, null }, { NextLevelLabel, null }, { ClickToContinueLabel, null } };
        VisualElement[] _hearts = new VisualElement[Helper.MaxLivesCount];
        GameManager GAME;
        LevelManager LEVEL;
        AudioManager AUDIO;
        SaveManager SAVE;
        ADManager AD;
        UIManager UI;
        RewardController _rewardController;
        VisualElement _root, _progressFill;
        ProgressBar _progressBar;
        //TextField _numberInput; //IntegerField _numberInput;
        Coroutine _labelPulsation;
        Coroutine _buttonPulsation;
        const string _ZeroNumber = "000";
        const int _Second = 1000;
        string _numberText;
        int _livesCount;
        bool _isPictureScreenVisible = false, pulsingUp = true, _isTutorVisible = false;
        bool? _isInited;

        LocalizedString _ls = new LocalizedString("Text", "hud.level"), _pauseLs = new("Text", "ui.pause"),
                        _continueLs = new("Text", "rewards.continueButton"), _clickToContLs = new("Text", "ui.clickToContinue");

        private readonly LocalizedAsset<Sprite> _startSprite = new() { TableReference = "Assets", TableEntryReference = "startGame" },
                                                _endSprite = new() { TableReference = "Assets", TableEntryReference = "endGame" },
                                                _learnSprite = new() { TableReference = "Assets", TableEntryReference = "learning" };


        public void OnLevelComplete()
        {
            ShowPictureScreen(_pictureHeight);
        }

        public void OnGameStart()
        {
            _isInited ??= Initialize();
            ShowStartScreen();
            //HideTopPanel();
        }

        public void OnGameComplete()
        {
            ShowFinalScreen();
        }

        public void OnLevelStart()
        {
            //_rewardController.Hide();
            _progressBar.title = $"{_ls.GetLocalizedString()} {LevelManager.I.LevelID + 1}";
            _elements[SkipIcon].EnableInClassList("ads", LevelManager.I.IsLastLevel());

            AUDIO.SetMusicMute(SAVE.IsMusicMuted);
            AUDIO.SetSoundMute(SAVE.IsSoundMuted);
            UpdateMusicIcon(SAVE.IsMusicMuted);
            UpdateSoundIcon(SAVE.IsSoundMuted);
        }

        public void ClearHeartBar()
        {
            if (_logging) Debug.Log("Clear HeartBar");
            _rewardController.Show(RewardContext.Lose);
            //AD.ShowInterstitial();
        }

        public void UpdateProgressbar(int score)
        {
            if (_logging) Debug.Log($"Update Progressbar = {score}");
            _progressBar.value = score;
            _progressFill.SetDisplay(score > 0);
        }

        public void UpdateHeartBar(int count)
        {
            if (_logging) Debug.Log($"Update HeartBar = {count}");
            _livesCount = count;
            for (int i = 0; i < Helper.MaxLivesCount; i++)
                _hearts[i].SetDisplay(count > i ? true : false);
        }

        //public void OnSubmit(InputValue value)
        //{
        //    Debug.Log("OnSubmit");
        //    if (!value.isPressed) return;
        //    HandleSubmit();
        //}

        //public void OnCancel(InputValue value)
        //{
        //    Debug.Log("OnCancel");
        //    if (!value.isPressed) return;
        //    HandleCancel();
        //}

        public void ShowTopPanel()
        {
            _elements[Topbar].Show();
            _progressBar.Show();
        }

        public void HideTopPanel()
        {
            _elements[Topbar].Hide();
            _progressBar.Hide();
        }

        async void Start()
        {
            if (!LocalizationSettings.InitializationOperation.IsDone)
                await LocalizationSettings.InitializationOperation.Task;

            await Task.Yield();

            await ApplyStaticLocalizedTexts();
            await ApplyLocalizedImages();
        }

        void Awake()
        {
            _isInited ??= Initialize();
        }

        bool Initialize()
        {
            InitManagers();
            _root = GetComponent<UIDocument>().rootVisualElement;
            InitAllElements(_root);
            _rewardController = GetComponent<RewardController>();
            _elements[DebugPanel].visible = UI.IsDebug;
            _elements[Topbar].SetColor(_topbarColor);


            return true;
        }

        void OnEnable()
        {
            SubControls();
            SubElements();
        }

        void InitManagers()
        {
            GAME = GameManager.I;
            LEVEL = LevelManager.I;
            SAVE = SaveManager.I;
            AUDIO = AudioManager.I;
            AD = ADManager.I;
            UI = UIManager.I;
        }

        void SubElements()
        {
            //Debug.Log("SubElements");
            SubMainButtons();
            SubAudioButtons();
            SubHeartButton();
            SubInfoButton();
            //SubPictureScreen();
            SubRestartGameButton();
            SubStartGameButton();
            SubDebugButtons();
            SubOnResize();
        }

        void SubControls()
        {
            _submit.action.performed += OnSubmit;
            _cancel.action.performed += OnCancel;
            _submit.action.Enable();
            _cancel.action.Enable();
        }

        void InitAllElements(VisualElement root)
        {
            InitElements(root);
            InitHearts(root);
            InitButtons(root);
            InitLabels(root);
            InitFields(root);
        }

        void InitElements(VisualElement root) { foreach (var key in _elements.Keys.ToList()) _elements[key] = root.Q<VisualElement>(key); }

        void InitHearts(VisualElement root)
        {
            for (int i = 0; i < _hearts.Length; i++)
            {
                var name = $"{HeartIcon}{i + 1}";
                var heart = root.Q<VisualElement>(name);
                _elements[name] = heart;
                _hearts[i] = heart;
            }
        }

        void InitButtons(VisualElement root)
        {
            foreach (var key in _buttons.Keys.ToList())
            {
                _buttons[key] = root.Q<Button>(key);
                _buttons[key].focusable = false;
            }
        }

        void InitLabels(VisualElement root) { foreach (var key in _labels.Keys.ToList()) _labels[key] = root.Q<Label>(key); }

        void InitFields(VisualElement root)
        {
            //_numberInput = root.Q<IntegerField>(PlayerNumber);
            //_numberInput = root.Q<TextField>(PlayerNumber);
            _progressBar = root.Q<ProgressBar>(ProgressBar);
            _progressFill = _progressBar.Q<VisualElement>(className: ProgressFill);
        }

        void StartFirstLevel()
        {
            LEVEL.StartLevel(0);
            _buttons[InfoScreenButton].clicked -= StartFirstLevel;
        }

        async Task ApplyStaticLocalizedTexts()
        {
            await SetLabelTextAsync("PauseText", _pauseLs);
            await SetLabelTextAsync("nextLevelText", _continueLs);
            await SetLabelTextAsync("continueText", _continueLs);
            await SetLabelTextAsync("ClickToContinueText", _clickToContLs);
        }
        async System.Threading.Tasks.Task ApplyLocalizedImages()
        {
            await SetBgSpriteAsync("WelcomeScreen", _startSprite);
            await SetBgSpriteAsync("CompleteScreen", _endSprite);
            await SetBgSpriteAsync("infoScreenButton", _learnSprite);
        }

        async Task SetLabelTextAsync(string name, LocalizedString ls)
        {
            if (!_labels.TryGetValue(name, out var label) || label == null)
                label = _root?.Q<Label>(name);
            if (label == null) return;

            AsyncOperationHandle<string> h = ls.GetLocalizedStringAsync();
            await h.Task;

            label.text = h.Result;
            _labels[name] = label;
        }

        async System.Threading.Tasks.Task SetBgSpriteAsync(string elementName, LocalizedAsset<Sprite> asset)
        {
            var ve = _root?.Q<VisualElement>(elementName);
            if (ve == null) return;

            AsyncOperationHandle<Sprite> h = asset.LoadAssetAsync();
            await h.Task;

            var sprite = h.Result;
            if (sprite != null)
                ve.style.backgroundImage = new StyleBackground(sprite);
        }

        IEnumerator LabelPulsating()
        {
            float duration = _pulseInterval;
            float t = 1f;
            bool pulsingUp = false;

            while (true)
            {
                if (pulsingUp)
                {
                    t += Time.deltaTime / duration;
                    if (t >= 1f)
                    {
                        t = 1f;
                        pulsingUp = false;
                    }
                }
                else
                {
                    t -= Time.deltaTime / duration;
                    if (t <= 0f)
                    {
                        t = 0f;
                        pulsingUp = true;
                    }
                }

                float fontSize = Mathf.Lerp(_labelPulseRange.x, _labelPulseRange.y, t);
                _labels[ClickToContinueLabel].style.fontSize = fontSize;

                yield return null;
            }
        }

        IEnumerator ButtonPulsating(Button button)
        {
            float duration = _pulseInterval, t = 1f;
            var pulsingUp = false;

            while (true)
            {
                if (pulsingUp)
                {
                    t += Time.deltaTime / duration;
                    if (t >= 1f)
                    {
                        t = 1f;
                        pulsingUp = false;
                    }
                }
                else
                {
                    t -= Time.deltaTime / duration;
                    if (t <= 0f)
                    {
                        t = 0f;
                        pulsingUp = true;
                    }
                }

                var scale = Mathf.Lerp(_buttonPulseRange.x, _buttonPulseRange.y, t);
                var styleScale = new StyleScale(new Vector3(scale, scale, 1f));
                button.style.scale = styleScale;
                yield return null;
            }
        }

        void HideFinalImageScreen()
        {
            _buttons[PictureScreen].Hide();
            _buttons[PicturePlaceholder].RemoveFromClassList("final-image-screen-image--expanded");
            _buttons[PictureScreen].RemoveFromClassList("final-image-screen--expanded");
            Helper.StopCoroutine(ref _labelPulsation, this);
        }

        void OnSubmit(CallbackContext ctx)
        {
            HandleSubmit();
        }

        void OnCancel(CallbackContext ctx)
        {
            HandleCancel();
        }

        void HandleCancel()
        {
            if (_isPictureScreenVisible) HidePictureScreen();
            if (_isTutorVisible) HideInfoScreen();
        }

        void HandleSubmit()
        {
            if (_isPictureScreenVisible) HidePictureScreen();
            if (_isTutorVisible) HideInfoScreen();
        }

        void SubMainButtons()
        {
            SubPauseButton();

            _buttons[RestartButton].clicked += () =>
            {
                LEVEL.Restart();
                AUDIO.PlayClick();
                DisablePause();
            };

            _buttons[SkipButton].clicked += () =>
            {
                AUDIO.PlayToggle();
                if (LEVEL.IsLastLevel()) AD.ShowRewarded("SkipLevel", LEVEL.OnLevelComplete.Invoke);
                else
                {
                    //LEVEL.StartNextLevel();
                    StartCoroutine(LEVEL.StartNextLevel(0.5f));
                }
                //LEVEL.OnLevelComplete.Invoke(); 
                DisablePause();
            };
            _buttons[PrevButton].clicked += () =>
            {
                AUDIO.PlayToggle();
                //LEVEL.StartPrevLevel();
                StartCoroutine(LEVEL.StartPrevLevel(0.5f));
            };
        }

        void EnablePause()
        {
            GAME.Pause();
            _elements[PauseScreen].Show();
            _elements[PauseIcon].AddToClassList("play");
        }

        void DisablePause()
        {
            GAME.Play();
            _elements[PauseScreen].Hide();
            _elements[PauseIcon].RemoveFromClassList("play");
        }

        void SubStartGameButton()
        {
            _buttons[StartGameButton].clicked += () => StartCoroutine(HideStartScreenDelayed(0.5f));
            //_buttons[StartGameButton].clicked += HideStartScreen;
        }

        void SubRestartGameButton()
        {
            _buttons[RestartGameButton].clicked += () =>
            {
                //AUDIO.PlayClick();
                HideFinalScreen();
                SAVE.ResetLevel();
                StartFirstLevel();
            };
        }

        void ShowStartScreen()
        {
            _buttons[StartScreen].Show();
            _buttons[StartScreen].EnableClass("hidden", false);
            _buttonPulsation = StartCoroutine(ButtonPulsating(_buttons[StartGameButton]));
        }

        void ShowFinalScreen()
        {
            _elements[FinalScreen].Show();
            //_elements[FinalScreen].EnableClass("hidden", false);
            _elements[FinalScreen].EnableClass("visible", true);
            _buttonPulsation = StartCoroutine(ButtonPulsating(_buttons[RestartGameButton]));
        }

        void ShowPictureScreen(float pictureHeight)
        {
            _buttons[PictureScreen].Show();
            _isPictureScreenVisible = true;
            _buttons[PictureScreen].schedule.Execute(() => { _buttons[PicturePlaceholder].AddToClassList("final-image-screen-image--expanded"); }).StartingIn(0);
            _buttons[PictureScreen].schedule.Execute(() => { _buttons[PictureScreen].AddToClassList("final-image-screen--expanded"); }).StartingIn(0);
            _labels[ClickToContinueLabel].visible = false;
            _labels[ClickToContinueLabel].schedule.Execute(() =>
            {
                _labels[ClickToContinueLabel].schedule.Execute(() =>
                {
                    _labels[ClickToContinueLabel].AddToClassList("final-image-screen-image-label--expanded");
                    _labelPulsation = StartCoroutine(LabelPulsating());
                    _labels[ClickToContinueLabel].visible = true;
                    SubPictureScreen();
                }).StartingIn((long)(_pressLabelPulseDelay * _Second));
            }).StartingIn(0);
            var picture = LEVEL.Picture;
            float aspectRatio = picture.GetAspectRatio(), width = pictureHeight * aspectRatio * Screen.height / Screen.width;
            _buttons[PicturePlaceholder].style.backgroundImage = new StyleBackground(picture);
            _buttons[PicturePlaceholder].SetSize(width, pictureHeight, LengthUnit.Percent);
        }

        IEnumerator HideStartScreenDelayed(float delay)
        {
            if (SaveManager.I.LoadNumber() == 0)
            {
                //_elements[PlayerNumberScreen].Show();
                _numberText = Random.Range(0, 999).ToString("D3");
                _numberText = "456";
                GAME.SetPlayerNumber(_numberText);
                SAVE.SaveNumber(int.Parse(_numberText));

                ShowInfoScreen();
                _buttons[InfoScreenButton].clicked += StartFirstLevel;
            }
            else
            {
                var levelID = SaveManager.I.LoadLevel();
                var heartsCount = SaveManager.I.LivesCount;
                //ShowTopPanel();
            }
            yield return new WaitForSecondsRealtime(delay);
            _buttons[StartScreen].EnableClass("hidden", true);
            _buttons[StartScreen].schedule.Execute(() =>
            {
                _buttons[StartScreen].Hide();
            }).StartingIn((long)(_startScreenFadeDuration * _Second));
            Helper.StopCoroutine(ref _buttonPulsation, this);
        }

        void HideFinalScreen()
        {
            _elements[FinalScreen].EnableClass("visible", false);
            //_elements[FinalScreen].EnableClass("hidden", true);
            _elements[FinalScreen].schedule.Execute(() => _elements[FinalScreen].Hide()).StartingIn((long)(_finalScreenFadeDuration * _Second));
            Helper.StopCoroutine(ref _buttonPulsation, this);
        }

        void PressResetButton()
        {
            AUDIO.PlayClick();
            SAVE.ResetAll();
            GAME.Restart();
        }

        void PressUnlockButton()
        {
            AUDIO.PlayClick();
            SAVE.UnlockLevels();
            GAME.Restart();
        }

        void SubPauseButton()
        {
            _buttons[PauseButton].clicked += () =>
            {
                AUDIO.PlayClick();
                if (GAME.State == GameState.Playing) EnablePause();
                else DisablePause();
            };
        }

        void SubHeartButton()
        {
            _buttons[HeartButton].clicked += () =>
            {
                if (_livesCount == Helper.MaxLivesCount) return;
                AUDIO.PlayClick();
                ADManager.I.ShowRewarded(RewardType.Heart.ToString(), () => _onHeartClick?.Invoke());
                //_onHeartClick?.Invoke();
            };
        }

        void SubInfoButton()
        {
            _buttons[InfoButton].clicked += ShowInfoScreen;
            _buttons[InfoScreenButton].clicked += () => StartCoroutine(HideInfoScreenDelayed(0.5f));
            //_buttons[InfoScreenButton].clicked += HideInfoScreen;
        }

        void ShowInfoScreen()
        {
            AUDIO.PlayClick();
            //_elements[LevelEndScreen].Hide();
            _elements[InfoScreen].Show();
            _isTutorVisible = true;
            GAME.Pause();
        }

        void HideInfoScreen()
        {
            _elements[InfoScreen].Hide();
            _isTutorVisible = false;
            GAME.SetPrevState();
        }

        IEnumerator HideInfoScreenDelayed(float delay)
        {
            //GAME.SetPrevState();
            yield return new WaitForSecondsRealtime(delay);
            _elements[InfoScreen].Hide();
            _isTutorVisible = false;
            GAME.Play(); //GAME.SetPrevState();
        }

        void SubAudioButtons()
        {
            _buttons[MusicButton].clicked += ToggleMusic;
            _buttons[SoundButton].clicked += ToggleSound;
        }

        void SubPictureScreen()
        {
            _buttons[PictureScreen].clicked += HidePictureScreen;
            _buttons[PicturePlaceholder].clicked += HidePictureScreen;
        }

        void UnsubPictureScreen()
        {
            _buttons[PictureScreen].clicked -= HidePictureScreen;
            _buttons[PicturePlaceholder].clicked -= HidePictureScreen;
        }

        void HidePictureScreen()
        {
            //Debug.Log("FinalImageScreenClicked");
            AUDIO.PlayClick();
            HideFinalImageScreen();
            _rewardController.Show(RewardContext.Win);
            //AD.ShowInterstitial();
            _isPictureScreenVisible = false;
            UnsubPictureScreen();
        }

        void ToggleSound()
        {
            AUDIO.PlayToggle();
            AUDIO.SwitchSFX();
            SAVE.SaveSoundMute(AUDIO.IsSoundMuted);
            UpdateSoundIcon(AUDIO.IsSoundMuted);
        }

        void ToggleMusic()
        {
            AUDIO.PlayToggle();
            AUDIO.SwitchMusic();
            SAVE.SaveMusicMute(AUDIO.IsMusicMuted);
            UpdateMusicIcon(AUDIO.IsMusicMuted);
        }

        void UpdateMusicIcon(bool mute) => _elements[MusicIcon]?.EnableInClassList("mute", mute);

        void UpdateSoundIcon(bool mute) => _elements[SoundIcon]?.EnableInClassList("mute", mute);

        void SubOnResize()
        {
            _root.RegisterCallback<GeometryChangedEvent>(_ => _onScreenResize?.Invoke());
            _buttons[HeartButton].SetSquareOnResize();
            _buttons[PrevButton].SetSquareOnResize();
            _buttons[RestartButton].SetSquareOnResize();
            _buttons[PauseButton].SetSquareOnResize();
            _buttons[SkipButton].SetSquareOnResize();
            _buttons[InfoButton].SetSquareOnResize();
            _buttons[SoundButton].SetSquareOnResize();
            _buttons[MusicButton].SetSquareOnResize();
            _buttons[StartGameButton].SetSquareOnResize();
            _buttons[RestartGameButton].SetSquareOnResize();
            foreach (var heart in _hearts) heart.SetSquareOnResize();
        }

        void SubDebugButtons()
        {
            _buttons[ResetButton].clicked += PressResetButton;
            _buttons[UnlockButton].clicked += PressUnlockButton;
        }

        void OnDestroy()
        {
            _submit.action.performed -= OnSubmit;
            _cancel.action.performed -= OnCancel;
        }

    }

}
