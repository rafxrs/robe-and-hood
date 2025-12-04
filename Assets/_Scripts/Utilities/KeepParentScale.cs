using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeepParentScale : MonoBehaviour
{
    void Update()
    {
        transform.localScale = transform.parent.localScale;
    }
}
