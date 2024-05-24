using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MathNet.Numerics.LinearAlgebra;

public class AxisCalculator : MonoBehaviour
{

    public FingerPointContainer FingerPointContainer;
    public GameObject EllipsoidCenter;
    private Matrix<float> Points;
    public Vector3 principleAxes;
    public Vector3[] projectedPoints = new Vector3[3];

    public Vector3[] RelativeFingertips = new Vector3[3];


    bool report = true; // Debuggin

    public double axis_min = 0.01; 
    public double axis_max = 0.25;

    // Start is called before the first frame update
    void Start()
    {
        Points = Matrix<float>.Build.Dense(3, 3);
        principleAxes[0] = 0.2f;
        principleAxes[1] = 0.2f;
        principleAxes[2] = 0.2f;
    }

    // Update is called once per frame
    void Update()
    {
        if (FingerPointContainer.PointsValid())
        {
            SetPrincipleAxes();
            SetProjectedPoints();
        }
    }

    public bool PointsValid()
    {
        return FingerPointContainer.PointsValid();
    }

    Vector3 GetLocalPosition(Vector3 globalPos) 
    {
        Vector3 res = new Vector3();
        for (int i = 0; i < 3; i++)
        {
            res[i] = globalPos[i] - EllipsoidCenter.transform.localPosition[i];
        }
        res = Matrix4x4.Rotate(EllipsoidCenter.transform.rotation).MultiplyPoint3x4(res);

        return res;
    }


    void SetPrincipleAxes()
    {
        if (!SetSquaredPointMatrix())
            return;



        // SVD
        //Vector<float> y = Vector<float>.Build.Dense(3, 1);
        //Vector<float> x;
        //var svd = Points.Svd(true);
        //x = svd.Solve(y); 


        // DLS
        //Vector<float> y = Vector<float>.Build.Dense(3, 1);
        //Vector<float> x;
        //float lambda = 0.0005f * 0.0005f;
        //Matrix<float> I = Matrix<float>.Build.DenseIdentity(3);
        //Matrix<float> PointsDLSInverse = (Points.Transpose() * Points + lambda * I).Inverse() * Points.Transpose(); // calculate psudo-inverse of the Points matrix using DLS
        //x = PointsDLSInverse * y;


        // QP solver
        Matrix<float> y1 = Matrix<float>.Build.Dense(3, 1, 1);
        Matrix<float> p = Points.Transpose()*Points;
        Matrix<float> qT = - y1.Transpose()*Points;
        Vector<float> x = GetAxesQPSolution(p, qT);


        for (int i = 0; i < 3; i++)
        {
            if (x[i] <= 0 || double.IsNaN(x[i]))
            {
                if (report)
                {
                    Debug.Log("Invalid Ellipsoid configuration");
                    report = false;
                }
                return;
            }
        }
        report = true;

        

        float delta = 0.2f * Time.deltaTime;
        for (int i = 0; i < 3; i++)
        {
            float desiredAxis = Mathf.Sqrt(1 / x[i]) * 2;
            principleAxes[i] += Mathf.Sign(desiredAxis - principleAxes[i]) * Mathf.Min(Mathf.Abs(principleAxes[i] - desiredAxis), delta);
        }

    }

    void SetProjectedPoints()
    {
        float a = principleAxes[0] / 2; // get the semi-axes
        float b = principleAxes[1] / 2;
        float c = principleAxes[2] / 2;

        
        for (int i = 0; i < 3; i++)
        {
            Vector3 point = FingerPointContainer.fingerPoints[i].position;

            float t = a * b * c / Mathf.Sqrt(Mathf.Pow(a * b * point[2], 2) + Mathf.Pow(a * c * point[1], 2) + Mathf.Pow(b * c * point[0], 2));

            projectedPoints[i] = new Vector3(point[0]*t, point[1] * t, point[2] * t);
        }

    }

    bool SetSquaredPointMatrix() {

        bool allZero = true;
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                if (FingerPointContainer.fingerPoints[i].position[j] != 0)
                    allZero = false;
            }
        }
        if (allZero)
            return false;

        for (int i = 0; i < 3; i++)
        {
            Vector3 TempVector = GetLocalPosition(FingerPointContainer.fingerPoints[i].position);

            RelativeFingertips[i] = TempVector; // also save it for projection


            for (int j = 0; j < 3; j++)
            {
                Points[i, j] = TempVector[j] * TempVector[j];
            }
        }
        return true;
    }


    Vector<float> GetAxesQPSolution(Matrix<float> p, Matrix<float> qT)
    {
        int n = 3;
        double[,] a_in = new double[n, n];
        double[] b_in = new double[n];
        for (int i = 0; i < n; i++)
        {
            b_in[i] = qT[0, i];
            for (int j = 0; j < n; j++)
            {
                a_in[i, j] = p[i, j];
            }
        }

        
        double[] x_out = qpsolver(n, a_in, b_in, axis_min, axis_max);


        Vector<float> x = Vector<float>.Build.Dense(3, 0);
        for (int i = 0; i < 3; i++)
        {
            x[i] = (float)x_out[i];
        }
        return x;
    }


    double[] qpsolver(int n, double[,] a, double[] b, double min, double max)
    {

        // define limites

        double[,] c = new double[2*n,n+1]; // { { 1, 0, 0, 1 / (min*min)}, { 1, 0, 0, 1 / (max * max) }, { 0, 1, 0, 1 / (min * min) }, { 0, 1, 0, 1 / (max * max) }, { 0, 0, 1, 1 / (min * min) }, { 0, 0, 1, 1 / (max * max) } }
        int[] ct = new int[2*n]; // { -1, 1, -1, 1, -1, 1 }
        
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                if (i == j)
                {
                    c[2 * i, j] = 1;
                    c[2 * i+1, j] = 1;
                }
                else
                {
                    c[2 * i, j] = 0;
                    c[2 * i+1, j] = 0;
                }
            }
            c[2 * i, n] = 1 / (min * min);
            c[2 * i + 1, n] = 1 / (max * max);

            ct[2 * i] = -1;
            ct[2 * i + 1] = 1;

        }



        double[] x;
        alglib.minqpstate state;
        alglib.minqpreport rep;

        // create solver, set quadratic/linear terms
        alglib.minqpcreate(n, out state);
        alglib.minqpsetquadraticterm(state, a);
        alglib.minqpsetlinearterm(state, b);
        alglib.minqpsetlc(state, c, ct);

        // set scale
        double[] s = new double[n];
        for (int i = 0; i < n; i++)
            s[i] = 1;

        alglib.minqpsetscale(state, s);

        // Solve problem with BLEIC-based QP solver.
        // Default stopping criteria are used.

        alglib.minqpsetalgobleic(state, 0.0, 0.0, 0.0, 100);
        alglib.minqpoptimize(state);
        alglib.minqpresults(state, out x, out rep);
        
        return x;
    }

}