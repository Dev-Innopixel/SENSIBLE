using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Globalization;
using System.Text;
using System.IO;
using System;

public class DataCollector : MonoBehaviour
{
    public Transform TCP_Master;
    public Transform TCP_Remote;
    public TCP_ROS TCP_ROS;
    public Slider Slider;
    public TaskTrack TaskTrack;
    public DisturbanceGenerator DisturbanceGenerator;
    public Plot Plot;


    public string csvFileName = "Experiment_3_";
    private string dataFolderPath;
    private string csvFilePath;
    private StringBuilder csvStringBuilder;

    public bool saveToCSV = true;

    private float t0 = 0f;

    bool Tracking = false;

    // Start is called before the first frame update
    void Start()
    {
        MakeExperimentRecordingFile();
    }

    // Update is called once per frame
    void Update()
    {
        float t = Time.time;

        if (Tracking)
        {
            WriteDataToCSV(t - t0, TCP_Remote.position, TCP_Master.position, TaskTrack.x_a_d, TaskTrack.x_d_d, TCP_ROS.Force.magnitude, TCP_ROS.Force.magnitude - Plot.desiredForce, DisturbanceGenerator.Disturbance, TaskTrack.TrackingError, Slider.stiffness, TCP_ROS.d_prod);
        }
    }

    public void StartCollect()
    {
        t0 = Time.time;
        Tracking = true;
    }

    public void EndCollect()
    {
        Tracking = false;
    }

    string F(float inp) 
    {
        return inp.ToString(CultureInfo.InvariantCulture);
    }

    string Fvec(Vector3 inp)
    {
        return $"{F(inp.x)},{F(inp.y)},{F(inp.z)}";
    }

    void WriteDataToCSV(float time, Vector3 actual_position, Vector3 desired_position, Vector3 actual_velocity, Vector3 desired_velocity, float force, float force_error, Vector3 disturbance, float tracking_error, float stiffness, float dotproduct) // int activePath, int activeDisturbance, bool noFeedback, bool VisualFeedback, bool hapticFeedback, bool mixedFeedback, bool noStiffness
    {

        // Append new data to the StringBuilder
        csvStringBuilder.AppendLine($"{F(time)},{Fvec(actual_position)},{Fvec(desired_position)},{Fvec(actual_velocity)},{Fvec(desired_velocity)},{Fvec(disturbance)},{F(force)},{F(force_error)},{F(tracking_error)},{F(stiffness)},{F(dotproduct)}");
        // {activePath},{activeDisturbance},{noFeedback},{VisualFeedback},{hapticFeedback},{mixedFeedback},{noStiffness}

        // Write the updated StringBuilder to the file
        File.WriteAllText(csvFilePath, csvStringBuilder.ToString());
    }

    void MakeExperimentRecordingFile()
    {
        if (saveToCSV == true)
        {
            dataFolderPath = Application.dataPath + $"/Data/{csvFileName}/";
            if (!Directory.Exists(dataFolderPath))
            {
                Directory.CreateDirectory(dataFolderPath);
            }


            string fileName = csvFileName + "-Data-" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".csv";
            csvFilePath = Path.Combine(dataFolderPath, fileName);

            // Create the CSV file and write header
            csvStringBuilder = new StringBuilder();
            //csvStringBuilder.AppendLine($"{timeString},{trackingerror},{stylys.transform.position.x},{stylys.transform.position.y},
            //{stylys.transform.position.z},{disturbance.transform.position.x},{disturbance.transform.position.y},{disturbance.transform.position.z},
            //{force},{targetforce},{dotproduct}, {errorString}, {activePath},{activeDisturbance},{noFeedback},{VisualFeedback},{hapticFeedback},{mixedFeedback},{noStiffness}");

            csvStringBuilder.AppendLine(
                "time," +
                "x_actual_x,x_actual_y,x_actual_z," +
                "x_desired_x,x_desired_y,x_desired_z," +
                "x_d_actual_x,x_d_actual_y,x_d_actual_z," +
                "x_d_desired_x,x_d_desired_y,x_d_desired_z," +
                "disturbance_x,disturbance_y,disturbance_z," +
                "force," +
                "force_error," +
                "tracking_error," +
                "stiffness," +
                "dotproduct"); 
            // Active Path, Active Disturbance, No Feedback, Visual Feedback, Haptic Feedback, Mixed Feedback, No stiffness Control

            // Save the header to the file
            File.WriteAllText(csvFilePath, csvStringBuilder.ToString());

            Debug.Log("CSV file created: " + csvFilePath);
        }
    }
}
