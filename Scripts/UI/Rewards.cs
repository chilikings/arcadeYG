using System.Linq;
using UnityEngine;
using GAME.Audio.SFX;
using GAME.Managers.AD;
using GAME.Managers.UI;
using System.Collections;
using GAME.Extensions.UI;
using GAME.Managers.Game;
using UnityEngine.Events;
using GAME.Managers.Audio;
using GAME.Managers.Level;
using GAME.Managers.Saves;
using GAME.Settings.Rewards;
using UnityEngine.UIElements;
using System.Threading.Tasks;
using UnityEngine.InputSystem;
using UnityEngine.Localization;
using System.Collections.Generic;
using GAME.Settings.Rewards.Items;
using GAME.Settings.Rewards.Skins;
using GAME.Settings.Rewards.Lives;
using UnityEngine.Localization.Settings;
using static UnityEngine.InputSystem.InputAction;
//using GAME.Settings.Rewards.Buffs;

namespace GAME.UI.Rewards
{
    public class RewardController : MonoBehaviour
    {
        [Header("SLOTS")]
        [SerializeField] List<RewardSlot> _winSlots;
        [SerializeField] List<RewardSlot> _loseSlots;
        [SerializeField] List<RewardSlot> _clickSlots;
        [Header("EVENTS")]
        [SerializeField] UnityEvent _onShow;
        [SerializeField] UnityEvent _onHide;
        [SerializeField][Space(4)] UnityEvent<int> _onItemClick;
        [SerializeField] UnityEvent<int> _onSkinClick;
        [SerializeField] UnityEvent<int> _onHeartClick;
        [SerializeField] UnityEvent<int> _onBuffClick;
        [Header("ACTIONS")]
        [SerializeField] InputActionReference _submit;
        [SerializeField] InputActionReference _cancel;
        //[Header("SETTINGS")]
        //[SerializeField] ItemsSettings _items;
        //[SerializeField] SkinsSettings _skins;
        //[SerializeField] LivesSettings _lives;
        //[SerializeField] BuffsSettings _buffs;

        readonly LocalizedAsset<ItemsSettings> _itemsAsset = new() { TableReference = "Rewards", TableEntryReference = "items" };
        readonly LocalizedAsset<SkinsSettings> _skinsAsset = new() { TableReference = "Rewards", TableEntryReference = "skins" };
        readonly LocalizedAsset<LivesSettings> _livesAsset = new() { TableReference = "Rewards", TableEntryReference = "lives" };
        const string _ScreenName = "LevelEndScreen", _TitleLabelName = "winText", _NextButtonName = "startLevelButton", 
                     _ContinueButtonName = "continueButton", _CloseButtonName = "closeButton";
        RewardContext _currentContext;
        GameManager GAME;
        LevelManager LEVEL;
        AudioManager AUDIO;
        SaveManager SAVE;
        //ADManager AD; UIManager UI;
        VisualElement _root, _slotsFrame, _screen;
        Button _continueButton, _nextLevelButton, _closeButton;
        Label _title;
        bool _isVisible;
        bool? _isInited;

        LocalizedString _chooseReward = new LocalizedString("Text", "rewards.choose"), _upgradeChar = new LocalizedString("Text", "rewards.upgradeLabel"),
                        _getReward = new LocalizedString("Text", "rewards.get"), _receivedReward = new LocalizedString("Text", "rewards.received");
        ItemsSettings _items;
        SkinsSettings _skins;
        LivesSettings _lives;
        bool _assetsReady;


        public async void Show(RewardContext context)
        {
            _isInited ??= Initialize();
            if (!_assetsReady) await LoadAssetsAsync();

            var isWin = context == RewardContext.Win;
            _isVisible = true;
            _currentContext = context;
            _screen.Show();
            _nextLevelButton.SetDisplay(isWin);
            _continueButton.SetDisplay(!isWin);
            _closeButton.Hide();
            _title.text = isWin ? _chooseReward.GetLocalizedString() : _upgradeChar.GetLocalizedString();
            Debug.Log($"Locale={LocalizationSettings.SelectedLocale?.Identifier.Code} | items={_items?.name}");
            Debug.Log($"Locale={LocalizationSettings.SelectedLocale?.Identifier.Code} | skins={_skins?.name}");
            GenCards(_root, context);

            _onShow?.Invoke();
        }

        void Awake()
        {
            //_isInited ??= Initialize();
        }

        async void Start()
        {
            _isInited ??= Initialize();
            if (!LocalizationSettings.InitializationOperation.IsDone)
                await LocalizationSettings.InitializationOperation.Task;
            await LoadAssetsAsync();
            //InitManagers();
        }

        bool Initialize()
        {
            _root = GetComponent<UIDocument>().rootVisualElement;
            InitElements(_root);
            InitButtons(_root);
            InitManagers();
            SubControls();
            SubButtons();
            Debug.Log("Initialize");
            return true;
        }

        void InitManagers()
        {
            GAME = GameManager.I;
            LEVEL = LevelManager.I;
            SAVE = SaveManager.I;
            AUDIO = AudioManager.I;
            //AD = ADManager.I; UI = UIManager.I;
        }

        async Task LoadAssetsAsync()
        {
            if (_assetsReady) return;

            _lives = await _livesAsset.LoadAssetAsync().Task;
            _skins = await _skinsAsset.LoadAssetAsync().Task;
            _items = await _itemsAsset.LoadAssetAsync().Task;
            _lives = await _livesAsset.LoadAssetAsync().Task;

            _assetsReady = (_items != null && _skins != null && _lives != null);
        }

        void InitElements(VisualElement root)
        {
            _screen = root.Q<VisualElement>(_ScreenName);
            _title = root.Q<Label>(_TitleLabelName);
        }

        void InitButtons(VisualElement root)
        {
            _nextLevelButton = root.Q<Button>(_NextButtonName);
            _continueButton = root.Q<Button>(_ContinueButtonName);
            _closeButton = root.Q<Button>(_CloseButtonName);
        }

        void SubControls()
        {
            _submit.action.performed += OnSubmit;
            _cancel.action.performed += OnCancel;
            _submit.action.Enable();
            _cancel.action.Enable();
        }

        void SubButtons()
        {
            _nextLevelButton.clicked += NextLevelButtonClicked;
            _continueButton.clicked += ContinueButtonClicked;
            _closeButton.clicked += CloseButtonClicked;
        }

        void Hide(ref bool isVisible)
        {
            _isInited ??= Initialize();
            isVisible = false;
            _screen.Hide();
            _onHide?.Invoke();
        }

        IEnumerator HideDelayed(float delay)
        {
            _isInited ??= Initialize();
            _onHide?.Invoke();
            yield return new WaitForSecondsRealtime(delay);
            _isVisible = false;
            _screen.Hide();
        }

        void NextLevelButtonClicked()
        {
            AUDIO.PlayClick();
            LEVEL.StartNextLevel(); //StartCoroutine(LEVEL.StartNextLevel(0.5f));
            StartCoroutine(HideDelayed(0.1f));
            //Hide(ref _isVisible);
        }

        void ContinueButtonClicked()
        {
            AUDIO.PlayClick();
            LEVEL.Restart();
            StartCoroutine(HideDelayed(0.1f));
            //Hide(ref _isVisible);
        }

        void CloseButtonClicked()
        {
            AUDIO.PlayClick();
            GAME.Play();
            StartCoroutine(HideDelayed(0.1f));
            //Hide(ref _isVisible);
        }

        void GenCards(VisualElement root, RewardContext context)
        {
            _isInited ??= Initialize();
            Debug.Log($"_isInited = {_isInited}");
            _slotsFrame = root.Q<VisualElement>("bonusCardsView");
            _slotsFrame.Clear();
            var cards = GetSlots(context);

            foreach (var card in cards)
            {
                var slot = new Button();
                slot.AddToClassList("bonus-card");

                var cardSettings = GetSettings(card.Type);
                Reward cardReward = null;

                var unusedIDs = GetUnusedIDs(card.Type);
                var cardID = card.IsRandom ? unusedIDs[Random.Range(0, unusedIDs.Count)] : card.ID;
                var image = new Image();
                cardReward = cardSettings.Get(cardID);

                var title = new Label();
                //title.text = _Presets[card.Type];
                title.text = cardReward.Name;
                SetFontSize(title, 45, 40, 35, 12, 8);
                title.AddToClassList("bonus-card-text");
                var labelContainer = new VisualElement();
                labelContainer.AddToClassList("bonus-card-text-container");
                labelContainer.Add(title);
                slot.Add(labelContainer);

                image.sprite = cardReward.Icon;
                image.AddToClassList("bonus-image");
                slot.Add(image);

                var button = new Button();
                button.AddToClassList("bonus-button");
                slot.Add(button);

                var buttonLabel = new Label();
                buttonLabel.text = _getReward.GetLocalizedString();
                buttonLabel.AddToClassList("bonus-text");
                button.Add(buttonLabel);

                var buttonImage = new VisualElement();
                buttonImage.RemoveFromClassList("done-placeholder");
                buttonImage.AddToClassList("watch-ad-placeholder");
                button.Add(buttonImage);

                button.clicked += () => OnRewardClick(button, buttonLabel, buttonImage, card.Type, cardID);
                slot.clicked += () => OnRewardClick(button, buttonLabel, buttonImage, card.Type, cardID);

                _slotsFrame.Add(slot);
            }
            _onShow?.Invoke();
        }

        void OnSubmit(CallbackContext ctx) => HandleSubmit();
        
        void OnCancel(CallbackContext ctx) => HandleCancel();

        void OnRewardClick(Button button, Label label, VisualElement image, RewardType type, int id)
        {
            //Debug.Log($"AudioManager.I = {AudioManager.I}");
            //Debug.Log($"ADManager.I = {ADManager.I}");
            //AUDIO.PlayReward();
            ADManager.I.ShowRewarded(type.ToString(), () => ApplyReward(type, id));
            image.RemoveFromClassList("watch-ad-placeholder");
            image.AddToClassList("done-placeholder");
            button.SetEnabled(false);
            label.text = _receivedReward.GetLocalizedString();
        }

        void HandleCancel()
        {
            if (!_isVisible) return;
            switch (_currentContext)
            {
                case RewardContext.Click: CloseButtonClicked(); break;
                case RewardContext.Win: NextLevelButtonClicked(); break;
                case RewardContext.Lose: ContinueButtonClicked(); break;
            }
        }

        void HandleSubmit()
        {
            if (!_isVisible) return;
            switch (_currentContext)
            {
                case RewardContext.Click: CloseButtonClicked(); break;
                case RewardContext.Win: NextLevelButtonClicked(); break;
                case RewardContext.Lose: ContinueButtonClicked(); break;
            }
        }

        void ApplyReward(RewardType type, int id)
        {
            switch (type)
            {
                case RewardType.Heart:
                    _onHeartClick.Invoke(id);
                    break;
                case RewardType.Buff:
                    _onBuffClick.Invoke(id);
                    break;
                case RewardType.Skin:
                    _onSkinClick.Invoke(id);
                    break;
                case RewardType.Item:
                    _onItemClick.Invoke(id);
                    break;
                default:
                    break;
            }
        }

        void SetFontSize(Label label, int maxSize, int medSize, int minSize, int medLength, int minLength)
        {
            int length = label.text.Length;
            int size = length <= minLength ? maxSize : (length <= medLength ? medSize : minSize);
            label.style.fontSize = size;
        }

        List<RewardSlot> GetSlots(RewardContext context) => context switch
        {
            RewardContext.Win => _winSlots,
            RewardContext.Lose => _loseSlots,
            RewardContext.Click => _clickSlots
        };

        IRewardList GetSettings(RewardType type) => type switch
        {
            RewardType.Item => _items,
            RewardType.Heart => _lives,
            RewardType.Skin => _skins,
            _ => null
        };

        List<int> GetUnusedIDs(RewardType type)
        {
            var settings = GetSettings(type);
            if (settings == null) return new List<int>();
            var allRewards = Enumerable.Range(0, settings.Count()).ToList();
            int skinID = SaveManager.I.SkinID, itemID = SaveManager.I.ItemID;
            //Debug.Log($"SaveManager.I = {SaveManager.I}");
            //Debug.Log($"SAVE = {SAVE}");
            return type switch
            {
                RewardType.Skin => allRewards.Where(id => id != skinID).ToList(),
                RewardType.Item => allRewards.Where(id => id != itemID).ToList(),
                _ => allRewards
            };
        }

    }

}