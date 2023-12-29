using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TeleportToNextScene : MonoBehaviour
{
    [SerializeField] private GameManager.SceneSelection _nextScene;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            GameManager.Instance?.LoadScene(_nextScene);
        }
    }
}
