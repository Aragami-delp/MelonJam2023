using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuDings : MonoBehaviour
{
    public void StartGame()
    {
        GameManager.Instance?.LoadScene(LEVEL.LEVEL_1);
    }

    public void CloseGame()
    {
        GameManager.Instance?.CloseGame();
    }
}
