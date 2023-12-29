using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] private float _interactionCricleRadius = 5f;
    [SerializeField] private LayerMask _interactionLayers;
    public void UseInteract()
    {
        Collider2D collision = Physics2D.OverlapCircle(this.transform.position, _interactionCricleRadius, _interactionLayers); // Closest collision
        if (collision != null)
        {
            try
            {
                collision.GetComponent<IInteractable>().Interact();
            }
            catch
            {
                Debug.LogWarning("Interaction failed");
            }
        }
    }
}
