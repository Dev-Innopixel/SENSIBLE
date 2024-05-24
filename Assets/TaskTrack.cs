using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Text;
using System.Globalization;


public class TaskTrack : MonoBehaviour
{
    public Transform TCP_Master;
    public Transform TCP_Remote;
    public Transform Wing;
    public Transform DesiredPositonVisualizer;
    public DataCollector DataCollector;
    public float ToolRadius = 0.015f;


    float[] line1   = {-0.414678f, -0.800f, 0, 1.551754f};
    float[] circle1 = {0f, 0.0f, 0.6f };
    float[] circle2 = {0f, 0.4f, 0.2f };
    float[] circle3 = { -4.5370250f, -0.8f, 5.20701639f };


    float[] intersectline1 = { -0.176326f, 0.4f };
    float[] intersectline2 = { 0.176326f, 0.0f };

    public bool FlipX = false;

    public Vector3 localTCP;
    public Vector3 desiredPos;
    public float TrackingError;

    Vector3 StartPosLocal;
    Vector3 EndPosLocal;
    GameObject startSphere;
    GameObject endSphere;
    private Color green = new Color(0, 0.7f, 0.2f);
    private Color blue = new Color(0.2f, 0, 0.7f);

    float TrackTriggerDistance = 0.05f;
    bool started = false;
    bool ended = true;

    // for generating TCP velocities
    public Vector3 x_d_d;
    public Vector3 x_a_d;
    Vector3 x_d_last = new Vector3(0, 0, 0);
    Vector3 x_a_last = new Vector3(0, 0, 0);
    float last_time = 0;

    public bool Tracking = false;

    public float TimeTaken = 0;
    //public List<string> TrackingErrorArrayAndTime = new List<string>();


    // Start is called before the first frame update
    void Start()
    {
        float startx = -0.25f;
        float starty = 0.4f;
        float endx = 0.6f;
        float endy = 0.4f;

        int mult = FlipX ? 1 : -1;
        StartPosLocal = getProjectedPositionLine(line1, new Vector3(startx * mult, starty), ToolRadius);
        EndPosLocal = getProjectedPositionCircle(circle1, new Vector3(endx * mult, endy), ToolRadius);

        GenerateStartAndEndPoints();

    }

    GameObject SpawnPrimitiveAsChild(PrimitiveType type, Vector3 position, string name, Color color, float scale)
    {
        GameObject obj = GameObject.CreatePrimitive(type);
        obj.transform.localScale = new Vector3(scale, scale, scale);
        obj.transform.parent = transform;
        obj.transform.localPosition = position;
        obj.name = name;
        obj.GetComponentInChildren<Renderer>().material.SetColor("_Color", color);
        return obj;
    }

    void GenerateStartAndEndPoints() 
    {
        startSphere = SpawnPrimitiveAsChild(PrimitiveType.Sphere, StartPosLocal, "StartPoint", green, TrackTriggerDistance/2);
        endSphere = SpawnPrimitiveAsChild(PrimitiveType.Sphere, EndPosLocal, "EndPoint", blue, TrackTriggerDistance/2);
    }

    bool aboveLine(float[] line, Vector3 TCP)
    {
        int mult = FlipX ? 1 : -1;
        return TCP.y - line[0] * TCP.x * mult - line[1] > 0;
    }

    Vector3 othogonalUnitVector(Vector3 Vec) // 90 degrees rotation around z (-90 degrees in unity)
    {
        int mult = FlipX ? 1 : -1;
        Vector3 Ortho = new Vector3(-Vec.y, Vec.x, 0) / Mathf.Sqrt(Vec.y * Vec.y + Vec.x * Vec.x) * mult;
        return Ortho;
    }

    Vector3 getProjectedPositionCircle(float[] circle, Vector3 Point, float toolRadius)
    {
        int mult = FlipX ? 1 : -1;
        Vector3 circlePos = new Vector3(mult*circle[0], circle[1], 0);
        Vector3 relativeVector = Point - circlePos;
        Vector3 projectedPoint = circlePos + (relativeVector / relativeVector.magnitude) * (circle[2] + toolRadius);
        return projectedPoint;
    }
    Vector3 getProjectedPositionLine(float[] line, Vector3 Point, float toolRadius)
    {

        int mult = FlipX ? 1 : -1;
        Vector3 Linep1 = new Vector3(mult*line[0], line[1], 0);
        Vector3 Linep2 = new Vector3(mult*line[2], line[3], 0);
        Vector3 LineVec = Linep2 - Linep1;
        Vector3 p1TCP = Point - Linep1;
        Vector3 p1TCPproj = LineVec * Vector3.Dot(LineVec, p1TCP) / Vector3.Dot(LineVec, LineVec);
        Vector3 ToolRadiusOffset = othogonalUnitVector(LineVec) * toolRadius;
        Vector3 ProjectedPoint = Linep1 + ToolRadiusOffset + p1TCPproj;
        return ProjectedPoint;
    }

    void CheckTrackingTask()
    {
        bool had_started = started;
        bool had_ended = ended;


        // check if close to start and end
        if (Vector3.Distance(StartPosLocal, localTCP) < TrackTriggerDistance)
            started = true;
        if (Vector3.Distance(EndPosLocal, localTCP) < TrackTriggerDistance)
            ended = true;

        if (!had_started && started)
        {
            ended = false;

            // tell data collector to start collecting
            DataCollector.StartCollect();
            // visually indicate task under way
            setColor(startSphere, scaleColor(green, 1.5f));
            setColor(endSphere, scaleColor(blue, 2f));
        }
        else if (!had_ended && ended)
        {
            started = false;
            // stop indicating task under way
            setColor(startSphere, green);
            setColor(endSphere, blue);
            // tell data collector to start collecting
            DataCollector.EndCollect();
        }

    }


    //void writeDataLine(float err, float currentTime)
    //{
    //    TrackingErrorArrayAndTime.Add(
    //        (currentTime - StartTime).ToString().Replace(',', '.') + 
    //        ", " +
    //        err.ToString().Replace(',', '.'));
    //}

    //void saveTrackingDataToFile()
    //{
    //    string directory = "C://Users/Fusk/Desktop/CollectedData/";
    //    int fCount = Directory.GetFiles(directory, "*", SearchOption.TopDirectoryOnly).Length;
    //    string filename = "TrackingError_" + fCount.ToString().PadLeft(3, '0') + "_" + DateTime.Now.ToString("dd-MM-yyyy_hh-mm") + ".txt";
    //    StreamWriter sw = new StreamWriter(directory + filename);
    //    sw.WriteLine("Time taken");
    //    sw.WriteLine(TimeTaken.ToString());
    //    sw.WriteLine("Tracking error");

    //    foreach (string value in TrackingErrorArrayAndTime)
    //    {
    //        sw.WriteLine(value);
    //        sw.Flush();
    //    }
    //    sw.Close();
    //}

    Color scaleColor(Color color, float scale)
    {
        return new Color(color.r*scale, color.g * scale, color.b * scale);
    }

    void setColor (GameObject obj, Color color)
    {
        obj.GetComponentInChildren<Renderer>().material.SetColor("_Color", color);
    }

    // Update is called once per frame
    void Update()
    {
        
        float t = Time.realtimeSinceStartup;
        float delta_t = t - last_time;
        Vector3 x_d = TCP_Master.position;
        Vector3 x_a = TCP_Remote.position;

        x_d_d = (x_d - x_d_last) / delta_t;
        x_a_d = (x_a - x_a_last) / delta_t;

        last_time = t;
        x_d_last = TCP_Master.position;
        x_a_last = TCP_Remote.position;

        
        localTCP = TCP_Remote.position - Wing.position;
        Vector3 localProjectedTCP = new Vector3(localTCP.x,localTCP.y,0);
        // decide which feature to calculate desired position from
        int mult = FlipX ? 1 : -1;
        if (mult * localTCP.x > 0) // split down the middle 
        {
            if (aboveLine(intersectline2, localTCP))
            {
                desiredPos = getProjectedPositionCircle(circle1, localProjectedTCP, ToolRadius);
            }
            else
            {
                desiredPos = getProjectedPositionCircle(circle3, localProjectedTCP, ToolRadius);
            }
        }
        else
        {
            if (aboveLine(intersectline1, localTCP))
            {
                desiredPos = getProjectedPositionCircle(circle2, localProjectedTCP, ToolRadius);
            }
            else
            {
                desiredPos = getProjectedPositionLine(line1, localProjectedTCP, ToolRadius);
            }
        }
        
        DesiredPositonVisualizer.position = Wing.position + desiredPos;

        TrackingError = Vector3.Distance(localTCP, desiredPos);

        CheckTrackingTask();
    }
}
