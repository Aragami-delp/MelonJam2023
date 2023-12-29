using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleEnemyFOV : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 30f; // Degrees per second
    [SerializeField] private float changeInterval = 2f; // Time in seconds between direction changes
    [SerializeField] private FieldOfView fieldOfView;

    private Rigidbody2D rb;
    private float nextChangeTime = 0f;
    private float currentRotationSpeed;

    private SpriteRenderer sr;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();

        if (rb == null)
        {
            Debug.LogError("RandomRotateRigidbody: No Rigidbody2D found on the GameObject.");
            return;
        }

        SetRandomRotationSpeed();

        //fieldOfView.transform.position = -this.transform.position;
    }

    void FixedUpdate()
    {
        // Check if it's time to change the rotation direction
        if (Time.fixedTime >= nextChangeTime)
        {
            SetRandomRotationSpeed();
        }

        // Apply rotation
        rb.MoveRotation(rb.rotation + currentRotationSpeed * Time.fixedDeltaTime);

        fieldOfView.SetOrigin(transform.position);
        fieldOfView.SetAimDirection(transform.right);
    }

    void SetRandomRotationSpeed()
    {
        // Randomize rotation speed and direction
        currentRotationSpeed = Random.Range(-rotationSpeed, rotationSpeed);

        // Set the next change time
        nextChangeTime = Time.fixedTime + changeInterval;
    }

    public void DisableRenderer()
    {
        sr.enabled = false;
        fieldOfView.ShowFOV = false;
    }

    public void EnableRenderer()
    {
        sr.enabled = true;
        fieldOfView.ShowFOV = true;
    }
}
