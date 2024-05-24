using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeArt.Components;
using WeArt.Core;

public class SpherePulseInteraction : MonoBehaviour
{
    WeArtTouchableObject _touchableObject;
    Animator _animator;


    private bool _stillTriggered = false;

    // Start is called before the first frame update
    void Start()
    {
        _animator = GetComponent<Animator>();
        _touchableObject = GetComponent<WeArtTouchableObject>();    
    }

    // Update is called once per frame
    void Update()
    {
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.name == "ColliderHot" || other.name == "ColliderCold")
        {
            _animator.SetInteger("Pulse", 1);
        }

    }

    private void OnTriggerExit(Collider other)
    {
        if (other.name == "ColliderHot" || other.name == "ColliderCold")
        {
            _animator.SetInteger("Pulse", 0);
        }
    }
}
