using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisturbanceGenerator : MonoBehaviour
{
    public GameObject arrowPrefab;
    public Transform RemoteTCP;
    public Transform Wing;
    
    public float max_magnitude = 10f;
    public int function = 0;
    public bool ShowArrows = false;
    public bool ApplyDisturbance = false;
    public float visual_scale = 1f;


    public Vector3 Disturbance = new Vector3(0,0,0);

    private delegate Vector3 Func(Vector3 pos);

    private Func[] Functions = new Func[2];

    float rangex = 0.5f;
    float rangey = 0.2f;
    float rangez = 0.1f;
    float resolution = 0.05f;
    int xArrowCount;
    int yArrowCount;
    int zArrowCount;
    int arrowCount = 0;

    GameObject[] Arrows = new GameObject[0];


    private void Start()
    {
        Functions[0] = f0;
        Functions[1] = f1;

        xArrowCount = (int)(0.0001 + rangex / resolution) + 1;
        yArrowCount = (int)(0.0001 + rangey / resolution) + 1;
        zArrowCount = (int)(0.0001 + rangez / resolution) + 1;
        //Debug.Log("Arrow counts (x,y,z): " + xArrowCount.ToString() + ", " + yArrowCount.ToString() + ", " + zArrowCount.ToString());
        arrowCount = xArrowCount * yArrowCount * zArrowCount;

    }

    Vector3 f0(Vector3 pos)
    {
        float a = 0.2f; float b = 6.0f; float c = 7.6f;
        float d = 3.6f; float e = 9.8f; float f = 4.3f;
        float g = 7.4f; float h = 8.9f; float i = 1.5f;

        Vector3 disturbance = new Vector3(Mathf.Sin(pos.x * a + d) * g, Mathf.Sin(pos.y * b + e) * h, Mathf.Sin(pos.z * c + f) * i);
       
        if (disturbance.magnitude > max_magnitude)
        {
            disturbance *= max_magnitude / disturbance.magnitude;
        }
        return disturbance;
    }
    Vector3 f1(Vector3 pos)
    {
        float a = 2.3f; float b = 6.1f; float c = 7.7f;
        float d = 3.6f; float e = 9.8f; float f = 4.3f;
        float g = 1.4f; float h = 1.9f; float i = 1.5f;

        Vector3 disturbance = new Vector3(Mathf.Sin(pos.x * a + d) * g, Mathf.Sin(pos.y * b + e) * h, Mathf.Sin(pos.z * c + f) * i);

        if (disturbance.magnitude > max_magnitude)
        {
            disturbance *= max_magnitude / disturbance.magnitude;
        }
        return disturbance;
    }

    // Update is called once per frame
    void Update()
    {
        Func f = Functions[function]; // Choose one of x functions
        if (ShowArrows)
        {
            destroyArrows();
            generateArrows(f);
        } 
        else
        {
            destroyArrows();
        }
        if (ApplyDisturbance)
            Disturbance = f(RemoteTCP.position);
        else
            Disturbance = new Vector3(0, 0, 0);
    }

    void generateArrows(Func f)
    {
        Vector3 fieldCenter = Wing.position + new Vector3(0, 0.6f, 0); // top of the wing
        Transform arrows = transform.GetChild(0);
        for (int x = 0; x < xArrowCount; x ++)
        {
            for (int y = 0; y < yArrowCount; y++)
            {
                for (int z = 0; z < zArrowCount; z++)
                {
                    GameObject arrow = Instantiate(arrowPrefab);
                    arrow.GetComponentInChildren<Renderer>().material.SetColor("_Color", new Color(0,1,0));
                    int index = x * yArrowCount * zArrowCount + y * zArrowCount + z;
                    Arrows[index] = arrow;

                    Vector3 position = fieldCenter + new Vector3(x, y, z) * resolution - new Vector3(rangex * 0.5f, rangey * 0.5f, rangez * 0.5f);
                    Vector3 disturbance = f(position);
                    arrow.transform.position = position;
                    arrow.transform.rotation = Quaternion.LookRotation(disturbance);
                    arrow.transform.localScale = new Vector3(1, 1, 0.7f) * disturbance.magnitude / 10 * (visual_scale / max_magnitude);

                    arrow.name = "Arrow " + index.ToString();
                    arrow.transform.parent = arrows.transform;
                }
            }
        }
    }

    void destroyArrows()
    {
        if (Arrows.Length > 0)
        {
            foreach (GameObject arrow in Arrows)
            {
                Destroy(arrow);
            }
        }
        Arrows = new GameObject[arrowCount];
    }
}
