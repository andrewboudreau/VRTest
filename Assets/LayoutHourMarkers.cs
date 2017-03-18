using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LayoutHourMarkers : MonoBehaviour
{

    public float radius = 4.5f;

    public GameObject hourMarkerPrefab;

    void Start()
    {
        if (hourMarkerPrefab != null)
        {
            for (int i = 0; i < 12; i++)
            {
                float angle = i * Mathf.PI * 2 / 12;
                Vector3 pos = new Vector3(transform.position.x, transform.position.y + Mathf.Cos(angle) * radius, transform.position.z + Mathf.Sin(angle) * radius);
                var p = (GameObject)Instantiate(hourMarkerPrefab, pos, Quaternion.identity);
                p.transform.parent = transform;

                if (i == 0)
                {
                    p.GetComponent<Renderer>().material.color = new Color(1f, 0, 0);
                }
            }
        }
    }
}
