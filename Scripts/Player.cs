using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{

    [SerializeField] LayerMask layerMask;

    // Update is called once per frame
    void Update()
    {
        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.down), out RaycastHit hitInfo, 1.01f, layerMask))
        {
        }
        else
        {
            transform.position = transform.position + new Vector3(0, -9.8f * Time.deltaTime, 0);
        }
    }
}
