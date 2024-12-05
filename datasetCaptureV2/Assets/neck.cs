using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class neck : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        transform.rotation = Quaternion.Euler(0, Mathf.Sin(Time.time) * 90f + 160f, 0);
    }
}
