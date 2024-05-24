using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeArt.Components;

public class ButtonInteraction : MonoBehaviour
{
    [SerializeField]
    private Animator _animator;

    [SerializeField]
    private SceneController _sceneController;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private bool TryGetComponentTouchableObjec(Collider other, out WeArtHapticObject hapticObject)
    {
        hapticObject = other.GetComponent<WeArtHapticObject>();
        if(hapticObject != null)
        {
            return true;
        }

        return false;
    }

    private void OnTriggerEnter(Collider other)
    {
        WeArtHapticObject hapticObject;
        if(TryGetComponentTouchableObjec(other, out hapticObject)) {
            _animator.SetInteger("Pressing", 2);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        WeArtHapticObject hapticObject;
        if (TryGetComponentTouchableObjec(other, out hapticObject)) {
            _animator.SetInteger("Pressing", 1);
            ResetAll();
        }
    }

    public void ResetAll()
    {
        GameObject[] lineRenderer = GameObject.FindGameObjectsWithTag("LineDraw");
        foreach (GameObject line in lineRenderer) Destroy(line);

        _sceneController.ResetAllStarterPosition();
    }
}
