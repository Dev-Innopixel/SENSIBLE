using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TabletController : MonoBehaviour
{
    public FingerPointContainer FingerPointContainer;

    public Transform YZ_ellipse;
    public Transform XY_ellipse;
    public BoxCollider InterfacingRegionYZ;
    public BoxCollider InterfacingRegionXY;

    private Vector3 thumb;
    private Vector3 index;

    public Vector3 principleAxes;

    public float x_axis = 0.1f;
    public float y_axis = 0.1f;
    public float z_axis = 0.1f;
    
    public float x_rot = 0;
    public float z_rot = 0;

    private float localScale_y;
    // Start is called before the first frame update
    void Start()
    {
        localScale_y = YZ_ellipse.localScale.y;
    }

    // Update is called once per frame
    void Update()
    {
        if(FingerPointContainer.PointsValid())
        {
            thumb = FingerPointContainer.fingerPoints[0].position;
            index = FingerPointContainer.fingerPoints[1].position;

            int region = fingersInTabletRegion();

            if (region == 1) // sets y_axis, z_axis, and x_rot
            {
                Vector3 localThumb = ProjectedLocalPosition(thumb, InterfacingRegionYZ);
                Vector3 localIndex = ProjectedLocalPosition(index, InterfacingRegionYZ);

                Axis12AndAngle(localThumb, localIndex, ref y_axis, ref z_axis, ref x_rot);
                x_rot = -x_rot; // flip sign or it is incorrect in termes of the wanted effect

            }
            else if (region == 2) // sets x_axis, y_axis, and z_rot
            {
                Vector3 localThumb = ProjectedLocalPosition(thumb, InterfacingRegionXY);
                Vector3 localIndex = ProjectedLocalPosition(index, InterfacingRegionXY);

                Axis12AndAngle(localThumb, localIndex, ref y_axis, ref x_axis, ref z_rot);
            }

            principleAxes = new Vector3(x_axis, y_axis, z_axis);

            ScaleAndRotateEllipses();

        }
    }

    int fingersInTabletRegion()
    {
        if (InterfacingRegionYZ.bounds.Contains(thumb) && 
            InterfacingRegionYZ.bounds.Contains(index))
        { 
            return 1;
        }
        else if (
            InterfacingRegionXY.bounds.Contains(thumb) && 
            InterfacingRegionXY.bounds.Contains(index))
        {
            return 2;
        }
        return 0;
    }

    Vector3 ProjectedLocalPosition(Vector3 point, BoxCollider region)
    {
        Vector3 res = region.transform.worldToLocalMatrix.MultiplyPoint(point);
        res.y = 0;
        return res;
    }

    void Axis12AndAngle(Vector3 thumb, Vector3 index, ref float axis1, ref float axis2, ref float angle)
    {
        // in local frame -x is 'x' and -z is 'y' in terms of a usual XY-plane
        Vector2 thu = new Vector2(-thumb.x, -thumb.z);
        Vector2 inx = new Vector2(-index.x, -index.z);

        Vector2 circMean = (thu / thu.magnitude + inx / inx.magnitude) * 1 / 2;
        angle = Mathf.Atan2(circMean.y, circMean.x) - Mathf.PI/4;

        Vector2 v1 = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        Vector2 v2 = new Vector2(Mathf.Cos(angle + Mathf.PI/2), Mathf.Sin(angle + Mathf.PI / 2));

        axis1 = (Vector2.Dot(thu, v1) * v1).magnitude * 0.5f * 1.0f / transform.localScale[0];
        axis2 = (Vector2.Dot(inx, v2) * v2).magnitude * 0.5f * 1.0f / transform.localScale[0];
    }

    void ScaleAndRotateEllipses()
    {
        YZ_ellipse.localScale = new Vector3(y_axis, localScale_y, z_axis); // This doesn't really make sense unless you know what's going on in the paper:
        XY_ellipse.localScale = new Vector3(y_axis, localScale_y, x_axis); // "Independantly Commanding Size, Shape and orientation of Robot Endpoint Stiffness in Tele-Impedance by Virtual Ellipsoid Interface"

        YZ_ellipse.transform.localRotation = Quaternion.Euler(0, -(-x_rot) * 180 / Mathf.PI, 0); // regular angle of x_rot is correct locally, so flip the direction of the angle again for displaying on the ellipse
        XY_ellipse.transform.localRotation = Quaternion.Euler(0, (-z_rot) * 180 / Mathf.PI, 0);
    }
}
