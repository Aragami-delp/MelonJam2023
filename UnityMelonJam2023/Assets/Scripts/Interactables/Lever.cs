using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(10)]
public class Lever : MonoBehaviour, IInteractable
{
    [SerializeField] private GameObject _leverUp, _leverDown;

    [SerializeField] private bool startStatus = false;

    [SerializeField] private MonoInteractionTarget _target;
    private bool _status;
    public bool Status
    {
        get
        {
            return _status;
        }
        set
        {
            if (value)
            {
                _leverUp.SetActive(true); _leverDown.SetActive(false);
            }
            else
            {
                _leverUp.SetActive(false); _leverDown.SetActive(true);
            }
            _status = value;
            _target.DoStuff(value);
        }
    }

    private void Start()
    {
        Status = startStatus;
    }

    public void Interact()
    {
        Debug.Log("Lever used");
        Status = !Status;
    }
}
