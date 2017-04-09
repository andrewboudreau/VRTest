using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Orbit : MonoBehaviour
{
    Vector3 spin = new Vector3(.08f, .05f, 0.1f);
    
    // orbit parameters
    float theta = 0;  // angle that will be increased each loop
    float h = 12;      // x coordinate of circle center
    float k = 10;      // y coordinate of circle center
    float step = 0.0015f;  // amount to add to theta each time (degrees)
    float r = 20;

    void Update()
    {
        // spin
        transform.rotation *= Quaternion.Euler(spin);

        // orbit
        transform.position = new Vector3(h + r * Mathf.Cos(theta), k + r * Mathf.Sin(theta), k + r * Mathf.Sin(theta));
        if (theta > 3600)
        {
            theta -= 3600;
        }

        theta += step;
    }
}
