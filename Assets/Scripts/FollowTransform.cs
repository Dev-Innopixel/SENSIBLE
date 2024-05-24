using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowTransform : MonoBehaviour
{
    public bool IgnoreRotation = false;

    public GameObject ToFollow;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = ToFollow.transform.position;
        if (!IgnoreRotation)
        {
            transform.rotation = ToFollow.transform.rotation;
        }
    }
}
