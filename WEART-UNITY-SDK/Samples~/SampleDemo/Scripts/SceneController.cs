using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneController : MonoBehaviour
{
    [SerializeField]
    GameObject[] _objects;

    [SerializeField]
    Transform[] _resetPosition;

    // Start is called before the first frame update
    void Start()
    {

    }

    public void ResetAllStarterPosition()
    {
        for(int i=0; i< _objects.Length; i++)
        {
                _objects[i].transform.localPosition = _resetPosition[i].localPosition;
                _objects[i].transform.localRotation = _resetPosition[i].localRotation;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
