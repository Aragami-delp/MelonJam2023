using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using static UnityEngine.InputSystem.DefaultInputActions;

public class GameManager : MonoBehaviour
{
    public enum SceneSelection
    {
        NONE = 0,
        MAINMENU = 10,
        ARATEST = 100,
        FUCHSTEST = 101,
        ARATEST2 = 102,

    }
    public static GameManager Instance { get; private set; }

    [SerializeField] private SceneSelection _firstLevel;

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

    public void LoadScene(SceneSelection _nextScene)
    {
        switch (_nextScene)
        {
            case SceneSelection.MAINMENU:
                SceneManager.LoadScene(0);
                break;
            case SceneSelection.ARATEST:
                SceneManager.LoadScene("AraTestScene");
                break;
            case SceneSelection.FUCHSTEST:
                SceneManager.LoadScene("Fuchs Scene");
                break;
            case SceneSelection.ARATEST2:
                SceneManager.LoadScene("AraTestScene Level2");
                break;
            case SceneSelection.NONE:
            default:
                break;
        }
    }

    public void LoadFirstLevel()
    {
        LoadScene(_firstLevel);
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
