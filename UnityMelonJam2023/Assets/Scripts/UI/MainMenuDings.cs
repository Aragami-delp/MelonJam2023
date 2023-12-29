using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuDings : MonoBehaviour
{
    public void StartGame()
    {
        GameManager.Instance?.LoadFirstLevel();
    }

    public void CloseGame()
    {
        GameManager.Instance?.CloseGame();
    }
}
