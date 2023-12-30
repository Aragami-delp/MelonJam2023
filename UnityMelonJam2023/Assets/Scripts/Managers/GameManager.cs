using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using static UnityEngine.InputSystem.DefaultInputActions;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private List<SceneLevel> _levelNames;

    // Start is called before the first frame update
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(this);
    }

    public void LoadScene(LEVEL _nextLevel)
    {
        foreach (SceneLevel sceneLevel in _levelNames)
        {
            if (sceneLevel.Level == _nextLevel)
            {
                try
                {
                    SceneManager.LoadScene(sceneLevel.SceneName);
                }
                catch
                {
                    Debug.LogError("Couldn't load level");
                }
            }
        }
    }

    public void CloseGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
