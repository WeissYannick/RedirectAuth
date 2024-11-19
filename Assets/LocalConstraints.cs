using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalConstraints : MonoBehaviour
{
    [SerializeField]
    private bool _lockXPosition = false;
    [SerializeField]
    private bool _lockYPosition = false;
    [SerializeField]
    private bool _lockZPosition = false;

    private Vector3 localStartPosition = Vector3.zero;

    void Start()
    {
        localStartPosition = transform.localPosition;
    }

    void Update()
    {
        bool changed = false;
        Vector3 currentLocalPosition = transform.localPosition;
        if (_lockXPosition && currentLocalPosition.x != localStartPosition.x)
        {
            currentLocalPosition.x = localStartPosition.x;
            changed = true;
        }
        if (_lockYPosition && currentLocalPosition.y != localStartPosition.y)
        {
            currentLocalPosition.y = localStartPosition.y;
            changed = true;
        }
        if (_lockZPosition && currentLocalPosition.z != localStartPosition.z)
        {
            currentLocalPosition.z = localStartPosition.z;
            changed = true;
        }
        if (changed)
        {
            transform.localPosition = currentLocalPosition;
        }
    }
}
