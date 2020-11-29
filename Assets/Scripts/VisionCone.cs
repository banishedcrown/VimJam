using CodeMonkey.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisionCone : MonoBehaviour
{
    // Start is called before the first frame update
    private Mesh mesh;

    [SerializeField] LayerMask layermask;
    public float fov = 90;
    Vector3 origin = Vector3.zero;

    float startingAngle = 0f;
    public float viewDistance = 4f;

    void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        
        

        origin = Vector3.zero;
    }

    void LateUpdate()
    {
        origin = transform.position;
        int raycount = 50;
        
        float angle = startingAngle + 2;
        float angleIncrease = fov / raycount;
        float distance = viewDistance + .05f;

        Vector3[] vertices = new Vector3[raycount + 1 + 1];
        Vector2[] uv = new Vector2[vertices.Length];
        int[] triangles = new int[raycount*3];


        vertices[0] = Vector3.zero;

        int vertexIndex = 1;
        int triangleIndex = 0;

        for(int i = 0; i <= raycount; i++)
        {
            Vector3 vertex;
            Vector3 lookAngle = UtilsClass.GetVectorFromAngle((int)angle);
            RaycastHit2D hit = Physics2D.Raycast(origin, lookAngle, distance, layermask);

            if(hit.collider == null)
            {
                vertex = lookAngle * distance;
            }
            else
            {
                vertex = hit.point - (Vector2)(origin);
            }

            vertices[vertexIndex] = vertex;
            uv[vertexIndex] = vertex;

            if (i > 0)
            {
                triangles[triangleIndex + 0] = 0;
                triangles[triangleIndex + 1] = vertexIndex - 1;
                triangles[triangleIndex + 2] = vertexIndex;

                triangleIndex += 3;
            }

            vertexIndex++;
            angle -= angleIncrease;
        }

        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;
        mesh.bounds = new Bounds(origin, Vector3.one * 1000f);
    }

    public void setOrigin(Vector3 origin)
    {
        this.origin = origin;
    }
    public void setAimDirection( Vector3 dir) 
    { 
        startingAngle = UtilsClass.GetAngleFromVectorFloat(dir) + fov/2f; 
    }

}
