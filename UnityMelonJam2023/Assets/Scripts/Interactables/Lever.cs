using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lever : MonoBehaviour, IInteractable
{
    [SerializeField] private GameObject _leverUp, _leverDown;

    [SerializeField] private bool startStatus = false;

    [SerializeField, SerializeReference] private IInteractionTarget _target;
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
