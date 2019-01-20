// based from https://answers.unity.com/questions/944228/creating-a-smooth-round-flat-circle.html

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircleShapeMesh : MonoBehaviour
{
    [SerializeField]
    private int _innerArc = 360;
    [SerializeField]
    private float _innerRadius = 0.0f;
    [SerializeField]
    private int _outerArc = 360;
    [SerializeField]
    private float _outerRadius = 1.0f;
    [SerializeField]
    private int _segments = 3;

    private void Awake()
    {
        MakeRadialShape(_innerArc, _innerRadius, _outerArc, _outerRadius, _segments);
    }

    private float startAngleStep;
    private float endAngleStep;
    private List<Vector3> vertexList;
    private List<int> triangleList;

    private Quaternion innerQuaternion;
    private Quaternion outerQuaternion;
    private Quaternion offsetQuaternion;

    private void MakeRadialShape(int innerArc, float innerRadius, int outerArc, float outerRadius, int segments)
    {

        startAngleStep = innerArc / (float)segments;
        endAngleStep = outerArc / (float)segments;
        vertexList = new List<Vector3>();
        triangleList = new List<int>();
        innerQuaternion = Quaternion.Euler(0.0f, startAngleStep, 0.0f);

        // there is space between pivot and inner arc (doughnut)
        if (innerRadius > 0)
        {

            // both arcs are the same, normal doughnut shape
            if (innerArc == outerArc)
            {
                MakeDoughnutShape(innerArc, innerRadius, outerArc, outerRadius, segments);
            }
            else
            {
                // end points all lead to 0 (pointy)
                if (outerArc == 0)
                {
                    MakePointyShape(innerArc, innerRadius, outerArc, outerRadius, segments);
                }
                else
                {
                    // base is wider than the tip(but not pointy)
                    if (outerArc < innerArc)
                    {
                        MakeNarrowShape(innerArc, innerRadius, outerArc, outerRadius, segments);
                    }
                    // the tip is wider than the base (fan)
                    else
                    {
                        MakeFanShape(innerArc, innerRadius, outerArc, outerRadius, segments);
                    }
                }
            }
            // move the pivot to inner arc center
            for (int i = 0, ilen = vertexList.Count; i < ilen; i++)
            {
                Vector3 vec3 = vertexList[i];
                vec3.z -= innerRadius;

                vertexList[i] = vec3;
            }
        }
        // pivot is where arc begins (circle)
        else
        {
            MakeCircleShape(innerArc, innerRadius, outerArc, outerRadius, segments);
        }

        // generate UV at some point...
        //List<Vector2> uvList = new List<Vector2>();

        // finally create the mesh
        Mesh mesh = new Mesh();

        mesh.vertices = vertexList.ToArray();
        mesh.triangles = triangleList.ToArray();

        mesh.RecalculateBounds();
        mesh.RecalculateTangents();
        mesh.RecalculateNormals();

        gameObject.GetComponent<MeshFilter>().mesh = mesh;
    }

    private void MakeDoughnutShape(int innerArc, float innerRadius, int outerArc, float outerRadius, int segments)
    {
        outerQuaternion = Quaternion.Euler(0.0f, endAngleStep, 0.0f);
        offsetQuaternion = Quaternion.Euler(0.0f, (-innerArc * 0.5f), 0.0f);


        vertexList.Add(new Vector3(0.0f, 0.0f, innerRadius));
        vertexList.Add(new Vector3(0.0f, 0.0f, outerRadius));

        vertexList[0] = offsetQuaternion * vertexList[0];
        vertexList[1] = offsetQuaternion * vertexList[1];

        vertexList.Add(outerQuaternion * vertexList[1]);
        vertexList.Add(innerQuaternion * vertexList[0]);

        triangleList.Add(0);
        triangleList.Add(1);
        triangleList.Add(2);
        triangleList.Add(0);
        triangleList.Add(2);
        triangleList.Add(3);

        for (int i = 0; i < segments - 1; i++)
        {

            triangleList.Add(vertexList.Count - 1);
            triangleList.Add(vertexList.Count - 2);
            triangleList.Add(vertexList.Count);
            triangleList.Add(vertexList.Count - 1);
            triangleList.Add(vertexList.Count);
            triangleList.Add(vertexList.Count + 1);

            vertexList.Add(outerQuaternion * vertexList[vertexList.Count - 2]);
            vertexList.Add(innerQuaternion * vertexList[vertexList.Count - 2]);

        }
    }

    private void MakePointyShape(int innerArc, float innerRadius, int outerArc, float outerRadius, int segments)
    {
        // get the offset of the center
        offsetQuaternion = Quaternion.Euler(0.0f, (-innerArc * 0.5f), 0.0f);

        vertexList.Add(new Vector3(0.0f, 0.0f, innerRadius));
        // no need to offset the end, it's already at '0'
        vertexList.Add(new Vector3(0.0f, 0.0f, outerRadius));

        // offset the starting vector with the center
        vertexList[0] = offsetQuaternion * vertexList[0];
        // add the next vector in relation to the start vector
        vertexList.Add(innerQuaternion * vertexList[0]);

        triangleList.Add(0);
        triangleList.Add(1);
        triangleList.Add(2);

        for (int i = 0; i < segments - 1; i++)
        {
            triangleList.Add(vertexList.Count - 1);
            triangleList.Add(1);
            triangleList.Add(vertexList.Count);
            vertexList.Add(innerQuaternion * vertexList[vertexList.Count - 1]);
        }
    }

    private void MakeNarrowShape(int innerArc, float innerRadius, int outerArc, float outerRadius, int segments)
    {
        outerQuaternion = Quaternion.Euler(0.0f, endAngleStep, 0.0f);
        vertexList.Add(new Vector3(0.0f, 0.0f, innerRadius));
        vertexList.Add(new Vector3(0.0f, 0.0f, outerRadius));
        vertexList[0] = Quaternion.Euler(0.0f, (-innerArc * 0.5f), 0.0f) * vertexList[0];
        vertexList[1] = Quaternion.Euler(0.0f, (-outerArc * 0.5f), 0.0f) * vertexList[1];

        vertexList.Add(outerQuaternion * vertexList[1]);
        vertexList.Add(innerQuaternion * vertexList[0]);

        triangleList.Add(0);
        triangleList.Add(1);
        triangleList.Add(2);
        triangleList.Add(0);
        triangleList.Add(2);
        triangleList.Add(3);

        for (int i = 0; i < segments - 1; i++)
        {

            triangleList.Add(vertexList.Count - 1);
            triangleList.Add(vertexList.Count - 2);
            triangleList.Add(vertexList.Count);
            triangleList.Add(vertexList.Count - 1);
            triangleList.Add(vertexList.Count);
            triangleList.Add(vertexList.Count + 1);

            vertexList.Add(outerQuaternion * vertexList[vertexList.Count - 2]);
            vertexList.Add(innerQuaternion * vertexList[vertexList.Count - 2]);

        }
    }

    private void MakeFanShape(int innerArc, float innerRadius, int outerArc, float outerRadius, int segments)
    {
        if (innerArc == 0)
        {
            outerQuaternion = Quaternion.Euler(0.0f, endAngleStep, 0.0f);
            offsetQuaternion = Quaternion.Euler(0.0f, (-outerArc * 0.5f), 0.0f);
            vertexList.Add(new Vector3(0.0f, 0.0f, 0.0f));
            vertexList.Add(new Vector3(0.0f, 0.0f, outerRadius));
            vertexList[0] = offsetQuaternion * vertexList[0];

            vertexList.Add(outerQuaternion * vertexList[1]);

            triangleList.Add(0);
            triangleList.Add(1);
            triangleList.Add(2);

            for (int i = 0; i < segments - 1; i++)
            {

                triangleList.Add(0);
                triangleList.Add(vertexList.Count - 1);
                triangleList.Add(vertexList.Count);
                vertexList.Add(outerQuaternion * vertexList[vertexList.Count - 1]);

            }
        }
        else
        {
            outerQuaternion = Quaternion.Euler(0.0f, endAngleStep, 0.0f);

            Vector3 insideStartVector = new Vector3(0.0f, 0.0f, innerRadius);
            Vector3 outsidStartVector = new Vector3(0.0f, 0.0f, outerRadius);

            vertexList.Add(insideStartVector);
            vertexList.Add(outsidStartVector);

            vertexList[0] = Quaternion.Euler(0.0f, (-innerArc * 0.5f), 0.0f) * insideStartVector;
            vertexList[1] = Quaternion.Euler(0.0f, (-outerArc * 0.5f), 0.0f) * outsidStartVector;

            vertexList.Add(outerQuaternion * vertexList[1]);
            vertexList.Add(innerQuaternion * vertexList[0]);

            triangleList.Add(0);
            triangleList.Add(1);
            triangleList.Add(2);
            triangleList.Add(0);
            triangleList.Add(2);
            triangleList.Add(3);

            for (int i = 0; i < segments - 1; i++)
            {

                triangleList.Add(vertexList.Count - 1);
                triangleList.Add(vertexList.Count - 2);
                triangleList.Add(vertexList.Count);
                triangleList.Add(vertexList.Count - 1);
                triangleList.Add(vertexList.Count);
                triangleList.Add(vertexList.Count + 1);

                vertexList.Add(outerQuaternion * vertexList[vertexList.Count - 2]);
                vertexList.Add(innerQuaternion * vertexList[vertexList.Count - 2]);

            }
        }
        
    }

    private void MakeCircleShape(int innerArc, float innerRadius, int outerArc, float outerRadius, int segments)
    {
        outerQuaternion = Quaternion.Euler(0.0f, endAngleStep, 0.0f);
        offsetQuaternion = Quaternion.Euler(0.0f, ((innerArc == 0 ? -outerArc * 0.5f : innerArc)), 0.0f);

        vertexList.Add(new Vector3(0.0f, 0.0f, 0.0f));
        vertexList.Add(new Vector3(0.0f, 0.0f, outerRadius));

        vertexList[1] = offsetQuaternion * vertexList[1];

        vertexList.Add(outerQuaternion * vertexList[1]);

        triangleList.Add(0);
        triangleList.Add(1);
        triangleList.Add(2);

        for (int i = 0; i < segments - 1; i++)
        {
            triangleList.Add(0);
            triangleList.Add(vertexList.Count - 1);
            triangleList.Add(vertexList.Count);
            vertexList.Add(outerQuaternion * vertexList[vertexList.Count - 1]);
        }
    }
}
