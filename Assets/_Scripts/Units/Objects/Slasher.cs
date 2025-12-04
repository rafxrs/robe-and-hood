using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slasher : MonoBehaviour
{
    public float maxAngleDeflection = 30.0f;
    public float speedOfPendulum = 1.0f;
    public float baseAngle = 90f;

    // Update is called once per frame
    void Update()
    {
        float angle = maxAngleDeflection * Mathf.Sin( Time.time * speedOfPendulum);
        transform.localRotation = Quaternion.Euler( 0, 0, angle+baseAngle);
    }
}
