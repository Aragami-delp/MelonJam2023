using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(10)]
public class Lever : MonoBehaviour, IInteractable, IHideOutOfView
{
    [SerializeField] private GameObject _leverUp, _leverDown;

    [SerializeField] private bool startStatus = false;

    [SerializeField] private MonoInteractionTarget _target;

    [SerializeField] private bool _hideWhenNotInView;
    private bool _status;
    public bool Status
    {
        get
        {
            return _status;
        }
        set
        {
            if (!_hideWhenNotInView)
            {
                if (value)
                {
                    _leverUp.SetActive(true); _leverDown.SetActive(false);
                }
                else
                {
                    _leverUp.SetActive(false); _leverDown.SetActive(true);
                }
            }
            else
            {
                DisableRenderer();
            }
            _status = value;
            _target?.DoStuff(value);
        }
    }

    public bool AllowHide => _hideWhenNotInView;

    private void Start()
    {
        Status = startStatus;
    }

    public void Interact()
    {
        Status = !Status;
    }

    public void DisableRenderer()
    {
        _leverUp.SetActive(false); _leverDown.SetActive(false);
    }

    public void EnableRenderer()
    {
        if (_status)
        {
            _leverUp.SetActive(true); _leverDown.SetActive(false);
        }
        else
        {
            _leverUp.SetActive(false); _leverDown.SetActive(true);
        }
    }
}
