using UnityEngine;

public sealed class GameRuntimeServices : MonoBehaviour
{
    private const float AutoSaveInterval = 5f;
    private float _saveTimer;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Create()
    {
        if (FindObjectOfType<GameRuntimeServices>())
            return;

        GameObject instance = new GameObject("[Game Runtime Services]");
        DontDestroyOnLoad(instance);
        instance.AddComponent<GameRuntimeServices>();
    }

    private void Update()
    {
        _saveTimer += Time.unscaledDeltaTime;
        if (_saveTimer < AutoSaveInterval)
            return;

        _saveTimer = 0f;
        SaveGameStore.Flush();
    }

    private void OnApplicationPause(bool paused)
    {
        if (paused)
            SaveGameStore.Flush();
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
            SaveGameStore.Flush();
    }

    private void OnApplicationQuit()
    {
        SaveGameStore.Flush();
    }
}
