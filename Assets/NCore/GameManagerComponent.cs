using UnityEngine;
using NCore.Managers;
using NCore.Settings;

public class GameManagerComponent : MonoBehaviour
{
    [Header("Load the scene at index by default if > 0")]
    public int defaultScene = 0;

    private bool showNotice = false;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        EventManager.UpdateSettings += UpdateSettings;
    }

    private void Start()
    {
        if (IO.SettingsExists())
        {
            GameManager.Start(defaultScene);
        }
    }

    public void NoticeReadBeginGame()
    {
        GameManager.Start(defaultScene);
        
    }

    //Default event subscribers to prevent null reference exceptions.
    public void UpdateSettings()
    {

    }
}
