using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WeArt.Components
{

    public class DrawLineRender : MonoBehaviour
    {
        [SerializeField]
        private LineRenderer prefabLineRenderer;

        private GameObject currentLineRenderer;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public void DrawLine(RaycastHit hit)
        {
            if (currentLineRenderer == null)
            {
                currentLineRenderer = Instantiate(currentLineRenderer, hit.point, Quaternion.identity);
                LineRenderer lineRenderer = currentLineRenderer.GetComponent<LineRenderer>();
                lineRenderer.SetPosition(0, hit.point);
            }
            else
            {
                LineRenderer lineRenderer = currentLineRenderer.GetComponent<LineRenderer>();
                lineRenderer.positionCount++;
                lineRenderer.SetPosition(lineRenderer.positionCount - 1, hit.point);
            }
        }

        public void ReleaseLine()
        {
            currentLineRenderer = null;
        }
    }

}