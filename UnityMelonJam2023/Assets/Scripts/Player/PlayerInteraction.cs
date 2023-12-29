using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] private float _interactionCricleRadius = 5f;
    [SerializeField] private LayerMask _interactionLayers;
    [SerializeField] private LayerMask _interaction2Layers;
    public void UseInteract(int interactionId = 0)
    {
        Collider2D collision = Physics2D.OverlapCircle(
            transform.position, 
            _interactionCricleRadius, 
            interactionId == 0 ? _interactionLayers : _interaction2Layers); 
        // Closest collision
        if (collision != null)
        {
            try
            {
                collision.GetComponent<IInteractable>()?.Interact();
            }
            catch(UnityException e)
            {
                Debug.LogWarning(e);
            }
        }
    }
}
