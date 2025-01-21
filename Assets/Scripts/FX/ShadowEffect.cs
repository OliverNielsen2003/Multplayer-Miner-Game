using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShadowEffect : MonoBehaviour
{
    public Transform pos;
    // Start is called before the first frame update

    // Update is called once per frame
    void Update()
    {
        transform.position = pos.position;
    }
}