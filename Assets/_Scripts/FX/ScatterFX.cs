using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScatterFX : MonoBehaviour
{
    public float minForce = 10f; // Minimum force magnitude
    public float maxForce = 20f; // Maximum force magnitude
    public float scatterAngleRange = 30f; // Angle range in degrees

    void Start()
    {
        Scatter();
    }

    void Scatter()
    {
        // Calculate a random direction within the specified range
        float randomAngle = Random.Range(-scatterAngleRange, scatterAngleRange);
        Vector2 scatterDirection = Quaternion.Euler(0, 0, randomAngle) * Vector2.up;

        // Calculate a random force magnitude within the specified range
        float randomForceMagnitude = Random.Range(minForce, maxForce);

        // Apply force to the rigidbody in the calculated direction
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        rb.AddForce(scatterDirection * randomForceMagnitude, ForceMode2D.Impulse);
    }
}
