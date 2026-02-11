using UnityEngine;
using System.Collections;
using GAME.Settings.Level.Images;
using UnityEngine.SceneManagement;
using UnityEngine.AddressableAssets;

public class Bootstrap : MonoBehaviour
{
    [SerializeField] string _mainSceneName = "MainScene";

    IEnumerator Start()
    {
        var init = Addressables.InitializeAsync();
        while (!init.IsDone) yield return null;

        while (ImageSettings.Instance == null) yield return null;
        while (!ImageSettings.Ready.IsCompleted) yield return null;

        yield return SceneManager.LoadSceneAsync(_mainSceneName, LoadSceneMode.Single);
    }
}
