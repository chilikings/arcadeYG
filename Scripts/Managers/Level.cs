using System;
using UnityEngine;
using GAME.Utils.Core;
using GAME.Managers.AD;
using System.Collections;
using GAME.Enemies.Spawn;
using UnityEngine.Events;
using GAME.Managers.Saves;
using GAME.Settings.Levels;
using GAME.Managers.Singleton;
using GAME.Settings.Level.Images;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace GAME.Managers.Level
{
    public class LevelManager : Singleton<LevelManager, LevelSettings>
    {
        [SerializeField] ImageSettings _images;
        [SerializeField][Range(50, 90)] int _winScore;
        [SerializeField] int _score;
        [SerializeField] UnityEvent _onRestart;
        [SerializeField] UnityEvent _onGameComplete;

        public UnityEvent<LevelInfo> OnLevelStart;
        public UnityEvent<int> OnScoreChanged;
        public UnityEvent OnLevelComplete;

        Sprite _picture, _texture, _background;
        Coroutine _completing, _imagesLoading;
        LevelInfo _level;
        int _levelID;

        public int EnemiesCount { get; private set; }
        public int LevelsCount => _settings.Count;
        public int Score => _score;
        public int LevelID => _levelID;
        public LevelInfo Level => _level;
        public Sprite Picture => _images?.GetSprite(_level.Picture);
        public Sprite Texture => _images?.GetSprite(_level.Texture);
        public Sprite Background => _images?.GetSprite(_level.Background);


        public void StartLevel(int id)
        {
            if (id < 0) return;
            if (id >= _settings.Count)
            {
                _onGameComplete?.Invoke();
                _images.UnloadAll();
                return;
            }

            var nextLevel = _settings.GetLevel(id);
            var prevLevel = _level;
            _levelID = id;
            SaveManager.I.SaveLevel(id);

            if (_imagesLoading != null) StopCoroutine(_imagesLoading);
            var bg = _images.LoadBackground(nextLevel.Background);
            var pic = _images.LoadPicture(nextLevel.Picture);
            var tex = _images.LoadTexture(nextLevel.Texture);
            _imagesLoading = StartCoroutine(LoadingImages(bg, pic, tex, nextLevel, prevLevel));
        }

        //IEnumerator StartingLevel(int id, float delay)
        //{
        //    yield return new WaitForSecondsRealtime(delay);
        //    StartLevel(id);
        //}

        IEnumerator LoadingImages(AsyncOperationHandle<Sprite> bg, AsyncOperationHandle<Sprite> pic, AsyncOperationHandle<Sprite> tex, LevelInfo nextLevel, LevelInfo prevLevel)
        {
            yield return bg;
            yield return pic;
            yield return tex;

            if (bg.Status != AsyncOperationStatus.Succeeded || pic.Status != AsyncOperationStatus.Succeeded || tex.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogWarning("LoadingImages: one or more images failed to load. Keeping previous assets.");
                _imagesLoading = null;
                yield break;
            }
            _level = nextLevel;
            OnLevelStart?.Invoke(nextLevel);
            Restart();

            if (prevLevel != null)
            {
                if (prevLevel.Background != nextLevel.Background) _images.UnloadBackground(prevLevel.Background);
                if (prevLevel.Picture != nextLevel.Picture) _images.UnloadPicture(prevLevel.Picture);
                if (prevLevel.Texture != nextLevel.Texture) _images.UnloadTexture(prevLevel.Texture);
            }
            _imagesLoading = null;
        }

        public void StartNextLevel()
        {
            ADManager.I.ShowInterstitial();
            StartLevel(_levelID + 1);
        }

        public void StartPrevLevel()
        {
            ADManager.I.ShowInterstitial();
            StartLevel(_levelID - 1);
        }

        public IEnumerator StartNextLevel(float delay)
        {
            StartLevel(_levelID + 1);
            yield return new WaitForSecondsRealtime(delay);
            ADManager.I.ShowInterstitial();
        }

        public IEnumerator StartPrevLevel(float delay)
        {
            StartLevel(_levelID - 1);
            yield return new WaitForSecondsRealtime(delay);
            ADManager.I.ShowInterstitial();
        }

        public void Restart()
        {
            Helper.StopCoroutine(ref _completing, this);
            _score = 0;
            EnemiesCount = CountEnemies(_level.Enemies);
            _onRestart?.Invoke();
            OnScoreChanged?.Invoke(_score);
        }

        public bool IsLastLevel() => _levelID == SaveManager.I.LevelID;

        public void CalcScore(float zoneArea, float levelArea)
        {
            _score = Mathf.FloorToInt(100 * Mathf.Clamp01(100 * zoneArea / (levelArea * _winScore)));
            Debug.Log($"Score = {_score}");
            OnScoreChanged?.Invoke(_score);
            if (_score == 100)
                _completing ??= StartCoroutine(Helper.InvokeInSec(OnLevelComplete, _settings.WinDelay));     
        }

        public void KillEnemy()
        {
            if (EnemiesCount > 0) EnemiesCount--;
            if (EnemiesCount == 0) 
                _completing ??= StartCoroutine(Helper.InvokeInSec(OnLevelComplete, 0));
            //Debug.Log($"KillEnemy: {EnemiesCount}");
        }

        int CountEnemies(EnemySpawnInfo[] enemies)
        {
            int count = 0;
            Array.ForEach(enemies, enemy => count += enemy.Count);
            return count;
        }

    }
}