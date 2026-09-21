using UnityEngine;

public class FramerateLimiter : MonoBehaviour
{
    [SerializeField, Min(30)] int _value = 60;
    [SerializeField] bool _disableVSync = true;

    private void Awake()
    {
        if (_disableVSync)
            QualitySettings.vSyncCount = 0;

        Application.targetFrameRate = Mathf.Max(30, _value);
    }
}
