using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TeleportToNextScene : MonoBehaviour, IHideOutOfView
{
    [SerializeField] private LEVEL _nextLevel;
    [SerializeField] private bool _hideWhenNotInView = true;
    private SpriteRenderer _spriteRenderer;

    private void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        if (_hideWhenNotInView)
        {
            DisableRenderer();
        }
    }

    public bool AllowHide => _hideWhenNotInView;

    public void DisableRenderer()
    {
        _spriteRenderer.enabled = false;
    }

    public void EnableRenderer()
    {
        _spriteRenderer.enabled = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            GameManager.Instance?.LoadScene(_nextLevel);
        }
    }
}
