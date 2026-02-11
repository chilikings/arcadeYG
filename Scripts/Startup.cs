using UnityEngine;
using UnityEngine.AddressableAssets;

public static class Startup
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Initialize()
    {
        Addressables.InitializeAsync();
    }
}
