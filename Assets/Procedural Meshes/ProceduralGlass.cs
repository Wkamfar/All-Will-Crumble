using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProceduralGlass : MonoBehaviour
{
    Mesh glassMesh;
    [SerializeField] List<List<Vector3>> cuttingShapes = new List<List<Vector3>>();
    [SerializeField] List<GameObject> shards = new List<GameObject>();
    [SerializeField] float width;
    [SerializeField] float height;
    [SerializeField] float thickness;
    [SerializeField] List<Vector3> glassPoints = new List<Vector3>();
    [SerializeField] GameObject shardPrefab;
    // Start is called before the first frame update
    void Start()
    {
        glassMesh = new Mesh();
        GetComponent<MeshFilter>().mesh = glassMesh;
        GetComponent<MeshCollider>().sharedMesh = glassMesh;
        List<int> triangles = new List<int>();
        List<Vector3> points = new List<Vector3>();
        glassPoints.Add(new Vector3(-width / 2, -height / 2, -thickness / 2));
        glassPoints.Add(new Vector3(-width / 2, height / 2, -thickness / 2));
        glassPoints.Add(new Vector3(width / 2, height / 2, -thickness / 2));
        glassPoints.Add(new Vector3(width / 2, -height / 2, -thickness / 2));
        for (int i = glassPoints.Count - 1; i >= 0; --i)
            glassPoints.Insert(glassPoints.Count, new Vector3(glassPoints[i].x, glassPoints[i].y, thickness / 2));
        for (int i = 0; i < glassPoints.Count; ++i)
            points.Add(glassPoints[i]);

        // creating a glass shard
        List<Vector3> cuttingShape1 = new List<Vector3>();
        cuttingShape1.Add(new Vector2(-6, 0));
        cuttingShape1.Add(new Vector2(-6, 6));
        cuttingShape1.Add(new Vector2(0, 6));
        //cuttingShape1.Add(new Vector2(0, 0));
        //cuttingShape1.Add(new Vector2(-3, 0));
        //cuttingShape1.Add(new Vector2(-3, 6));
        //cuttingShape1.Add(new Vector2(0, 6));
        //cuttingShape1.Add(new Vector2(0, 0));
        cuttingShapes.Add(cuttingShape1);
        //List<Vector3> cuttingShape2 = new List<Vector3>();
        //cuttingShape2.Add(new Vector2(-3, 0));
        //cuttingShape2.Add(new Vector2(0, -3));
        //cuttingShape2.Add(new Vector2(3, 0));
        //cuttingShapes.Add(cuttingShape2);
        //
        List<int> inside = new List<int>();
        List<bool> corner = new List<bool>();
        List<int> notIntersecting = new List<int>();
        for (int i = 0; i < cuttingShapes.Count; ++i)
        {
            for (int j = 0; j < cuttingShapes[i].Count; ++j)
            {
                int interCount = 0;
                for (int k = 0; k < glassPoints.Count / 2; ++k)
                {
                    Vector2 next = glassPoints[(k + 1) % (glassPoints.Count / 2)];
                    float xp = cuttingShapes[i][j].x, yp = cuttingShapes[i][j].y;
                    float x1 = glassPoints[k].x, y1 = glassPoints[k].y;
                    float x2 = next.x, y2 = next.y;
                    if ((yp < y1) != (yp < y2) && xp < x1 + ((yp - y1) / (y2 - y1)) * (x2 - x1))
                        ++interCount;
                    
                }
                if (interCount % 2 == 1)
                {
                    inside.Add(i);
                    break;
                }
            }
            bool hasCorner = false;
            for (int j = 0; j < glassPoints.Count / 2; ++j)
            {
                int interCount = 0;
                for (int k = 0; k < cuttingShapes[i].Count; ++k)
                {
                    Vector2 next = cuttingShapes[i][(k + 1) % (cuttingShapes[i].Count)];
                    float xp = glassPoints[j].x, yp = glassPoints[j].y;
                    float x1 = cuttingShapes[i][k].x, y1 = cuttingShapes[i][k].y;
                    float x2 = next.x, y2 = next.y;
                    if ((yp < y1) != (yp < y2) && xp < x1 + ((yp - y1) / (y2 - y1)) * (x2 - x1))
                        ++interCount;
                }
                if (interCount % 2 == 1)
                {
                    if (inside.Count == 0 || inside[inside.Count - 1] != i)
                        inside.Add(i);
                    corner.Add(true);
                    hasCorner = true;
                    break;
                }
            }
            if (!hasCorner)
                corner.Add(false);
        }
        for (int i = 0; i < inside.Count; ++i)
        {
            bool intersecting = false;
            List<Vector3> shardPoints = new List<Vector3>();
            List<(Vector3, int, int)> intersections = new List<(Vector3, int, int)>(); // point, shard inter, glass inter
            for (int j = 0; j < cuttingShapes[inside[i]].Count; ++j)// makes the intersecting shapes
            {
                Vector2 current = cuttingShapes[inside[i]][j];
                Vector2 next = cuttingShapes[inside[i]][(j + 1) % cuttingShapes[inside[i]].Count];
                float x1 = current.x, y1 = current.y;
                float x2 = next.x, y2 = next.y;
                for (int k = 0; k < glassPoints.Count / 2; ++k)
                {
                    next = glassPoints[(k + 1) % (glassPoints.Count / 2)];
                    float x3 = glassPoints[k].x, y3 = glassPoints[k].y;
                    float x4 = next.x, y4 = next.y;
                    float uA = ((x4 - x3) * (y1 - y3) - (y4 - y3) * (x1 - x3)) / ((y4 - y3) * (x2 - x1) - (x4 - x3) * (y2 - y1));
                    float uB = ((x2 - x1) * (y1 - y3) - (y2 - y1) * (x1 - x3)) / ((y4 - y3) * (x2 - x1) - (x4 - x3) * (y2 - y1));
                    float intersectionX = x1 + (uA * (x2 - x1));
                    float intersectionY = y1 + (uA * (y2 - y1));
                    Vector2 intersect = new Vector2(intersectionX, intersectionY);
                    if (uA >= 0 && uA <= 1 && uB >= 0 && uB <= 1)
                    {
                        intersections.Add((intersect, j, k));
                        if (intersecting) // make this an iterative process // reorder afterwards
                        {
                            if (intersections[0].Item3 > intersections[1].Item3 && (intersections[1].Item3 != 0 || intersections[0].Item3 != glassPoints.Count - 1) ||
                            (intersections[0].Item3 == 0 && intersections[1].Item3 == glassPoints.Count - 1))
                            {
                                (Vector3, int, int) temp = (intersections[1].Item1, intersections[1].Item2, intersections[1].Item3);
                                intersections[1] = intersections[0];
                                intersections[0] = temp;
                            }
                            else if (intersections[0].Item3 == intersections[1].Item3 && Vector3.Distance(glassPoints[intersections[0].Item3], intersections[0].Item1) > Vector3.Distance(glassPoints[intersections[0].Item3], intersections[1].Item1))
                            {
                                (Vector3, int, int) temp = (intersections[1].Item1, intersections[1].Item2, intersections[1].Item3);
                                intersections[1] = intersections[0];
                                intersections[0] = temp;
                            }
                        } 
                        intersecting = true;
                    }
                }
            }
            if (!intersecting)
                notIntersecting.Add(inside[i]);
            else
            {
                if (corner[i]) // just check if corner
                {
                    int glassIndex = 0;
                    if (intersections[1].Item3 > intersections[0].Item3 && (intersections[0].Item3 != 0 || intersections[0].Item3 != glassPoints.Count - 1))
                        glassIndex = intersections[1].Item3;
                    shardPoints.Add(intersections[0].Item1);
                    shardPoints.Add(glassPoints[glassIndex]);
                    shardPoints.Add(intersections[1].Item1);
                    int cutIter = cuttingShapes[inside[i]].Count - (intersections[1].Item2 > intersections[0].Item2 ? intersections[1].Item2 - intersections[0].Item2 : cuttingShapes[inside[i]].Count - intersections[0].Item2 + intersections[1].Item2);
                    for (int j = 0; j < cutIter; ++j)
                        shardPoints.Add(cuttingShapes[inside[i]][(intersections[0].Item2 - j) % cuttingShapes[inside[i]].Count]);
                    
                }
                else if (intersections.Count > 2)
                {

                }
                else
                {
                    shardPoints.Add(intersections[0].Item1);
                    shardPoints.Add(intersections[1].Item1);
                    int cutIter = cuttingShapes[inside[i]].Count - (intersections[1].Item2 > intersections[0].Item2 ? intersections[1].Item2 - intersections[0].Item2 : cuttingShapes[inside[i]].Count - intersections[0].Item2 + intersections[1].Item2);
                    for (int j = 0; j < cutIter; ++j)
                        shardPoints.Add(cuttingShapes[inside[i]][(intersections[1].Item2 + 1 + j) % cuttingShapes[inside[i]].Count]);
                }
                shards.Add(CreateShard(shardPoints));
            }                
        }
        for (int i = 0; i < notIntersecting.Count; ++i)
            shards.Add(CreateShard(cuttingShapes[notIntersecting[i]]));
        // draw with depth
        List<int> indices = new List<int>();
        for (int i = 0; i < points.Count / 2; ++i)
            indices.Add(i);
        int pointsCount = indices.Count;

        for (int i = 0; i < pointsCount; ++i)
        {
            for (int j = 0; j < pointsCount - i; ++j)
            {
                Vector3 last = points[indices[(j - 1) < 0 ? indices.Count - 1 : (j - 1)]];
                Vector3 vertex = points[indices[j]];
                Vector3 next = points[indices[(j + 1) % indices.Count]];
                // figure out a better angle solution                
                if (IsTriangleOrientedClockwise(last, vertex, next)) // figure out if it is already reflex, so it does not need to be recalculated
                {
                    bool isEar = true;
                    for (int k = 0; k < pointsCount - i; ++k)
                    {
                        if (k == j || k == ((j - 1) < 0 ? indices.Count - 1 : (j - 1)) || k == (j + 1) % indices.Count)
                            continue;
                        if (IsPointInTriangle(last, vertex, next, points[indices[k]]))
                        {
                            isEar = false;
                            break;
                        }
                    }
                    if (isEar)
                    {
                        triangles.Add(indices[(j - 1) < 0 ? indices.Count - 1 : (j - 1)]);
                        triangles.Add(indices[j]);
                        triangles.Add(indices[(j + 1) % indices.Count]);
                        indices.RemoveAt(j);
                        break;
                    }

                }
            }
            if (indices.Count == 3)
            {
                triangles.Add(indices[0]);
                triangles.Add(indices[1]);
                triangles.Add(indices[2]);
                break;
            }
        }
        int triCount = triangles.Count;
        for (int i = 0; i < triCount; ++i)
            triangles.Insert(triangles.Count, triangles[i] + points.Count / 2);
        for (int i = 0; i < points.Count / 2; ++i)
        {
            List<int> face = new List<int>();
            face.Add(i);
            face.Add(points.Count - 1 - i);
            face.Add(points.Count - 2 - i < points.Count /2 ? points.Count  - 1 : points.Count - 2 - i);
            face.Add((i + 1) % (points.Count / 2));
            triangles.Add(face[0]);
            triangles.Add(face[1]);
            triangles.Add(face[2]);
            triangles.Add(face[2]);
            triangles.Add(face[3]);
            triangles.Add(face[0]);
        }
        glassMesh.Clear();
        glassMesh.vertices = points.ToArray();
        glassMesh.triangles = triangles.ToArray();
        glassMesh.RecalculateNormals();

    }

    // Update is called once per frame
    void Update()
    {
        //for (int x = 0; x < faces.Count; ++x)
        //{
        //    List<int> indices = faces[x];
        //    int pointsCount = indices.Count;

        //    for (int i = 0; i < pointsCount; ++i)
        //    {
        //        for (int j = 0; j < pointsCount - i; ++j)
        //        {
        //            Vector3 last = points[indices[(j - 1) < 0 ? indices.Count - 1 : (j - 1)]];
        //            Vector3 vertex = points[indices[j]];
        //            Vector3 next = points[indices[(j + 1) % indices.Count]];
        //            // figure out a better angle solution                
        //            if (IsTriangleOrientedClockwise(last, vertex, next)) // figure out if it is already reflex, so it does not need to be recalculated
        //            {
        //                bool isEar = true;
        //                for (int k = 0; k < pointsCount - i; ++k)
        //                {
        //                    if (k == j || k == ((j - 1) < 0 ? indices.Count - 1 : (j - 1)) || k == (j + 1) % indices.Count)
        //                        continue;
        //                    if (IsPointInTriangle(last, vertex, next, points[indices[k]]))
        //                    {
        //                        isEar = false;
        //                        break;
        //                    }
        //                }
        //                if (isEar)
        //                {
        //                    triangles.Add(indices[(j - 1) < 0 ? indices.Count - 1 : (j - 1)]);
        //                    triangles.Add(indices[j]);
        //                    triangles.Add(indices[(j + 1) % indices.Count]);
        //                    indices.RemoveAt(j);
        //                    break;
        //                }

        //            }
        //        }
        //        if (indices.Count == 3)
        //        {
        //            triangles.Add(indices[0]);
        //            triangles.Add(indices[1]);
        //            triangles.Add(indices[2]);
        //            break;
        //        }
        //    }
        //}
    }
    GameObject CreateShard(List<Vector3> shardPoints)//create the shards first, then assign where they are placed // get the point relative location, do this all later
    {
        //object location + rotation
        Vector3 center = new Vector3();
        List<int> triangles = new List<int>();
        for (int i = 0; i < shardPoints.Count; ++i)
            center = new Vector2(center.x + shardPoints[i].x, center.y + shardPoints[i].y);
        center /= shardPoints.Count;
        List<Vector3> points = new List<Vector3>();
        for (int i = 0; i < shardPoints.Count; ++i)
            Debug.Log("shardPoint " + i + " is: " + shardPoints[i]);
        for (int i = 0; i < shardPoints.Count; ++i)
            points.Add(new Vector3(shardPoints[i].x - center.x, shardPoints[i].y - center.y, -thickness / 2));
        for (int i = points.Count - 1; i >= 0; --i)
            points.Insert(points.Count, new Vector3(points[i].x, points[i].y, thickness / 2));
        List<int> indices = new List<int>();
        for (int i = 0; i < points.Count / 2; ++i)
            indices.Add(i);
        int pointsCount = indices.Count;

        for (int i = 0; i < pointsCount; ++i)
        {
            for (int j = 0; j < pointsCount - i; ++j)
            {
                Vector3 last = points[indices[(j - 1) < 0 ? indices.Count - 1 : (j - 1)]];
                Vector3 vertex = points[indices[j]];
                Vector3 next = points[indices[(j + 1) % indices.Count]];
                // figure out a better angle solution                
                if (IsTriangleOrientedClockwise(last, vertex, next)) // figure out if it is already reflex, so it does not need to be recalculated
                {
                    bool isEar = true;
                    for (int k = 0; k < pointsCount - i; ++k)
                    {
                        if (k == j || k == ((j - 1) < 0 ? indices.Count - 1 : (j - 1)) || k == (j + 1) % indices.Count)
                            continue;
                        if (IsPointInTriangle(last, vertex, next, points[indices[k]]))
                        {
                            isEar = false;
                            break;
                        }
                    }
                    if (isEar)
                    {
                        triangles.Add(indices[(j - 1) < 0 ? indices.Count - 1 : (j - 1)]);
                        triangles.Add(indices[j]);
                        triangles.Add(indices[(j + 1) % indices.Count]);
                        indices.RemoveAt(j);
                        break;
                    }

                }
            }
            if (indices.Count == 3)
            {
                triangles.Add(indices[0]);
                triangles.Add(indices[1]);
                triangles.Add(indices[2]);
                break;
            }
        }
        int triCount = triangles.Count;
        for (int i = 0; i < triCount; ++i)
            triangles.Insert(triangles.Count, triangles[i] + points.Count / 2);
        for (int i = 0; i < points.Count / 2; ++i)
        {
            List<int> face = new List<int>();
            face.Add(i);
            face.Add(points.Count - 1 - i);
            face.Add(points.Count - 2 - i < points.Count / 2 ? points.Count - 1 : points.Count - 2 - i);
            face.Add((i + 1) % (points.Count / 2));
            //Debug.Log("for triangle: " + i + " the points are: " + face[0] + ", " + face[1] + ", " + face[2] + ", " + face[3]);
            triangles.Add(face[0]);
            triangles.Add(face[1]);
            triangles.Add(face[2]);
            triangles.Add(face[2]);
            triangles.Add(face[3]);
            triangles.Add(face[0]);
        }
        GameObject newShard = Instantiate(shardPrefab, transform, false);
        newShard.transform.localPosition = center;
        newShard.transform.localScale = new Vector3(0.95f, 0.95f, 1);
        shards.Add(newShard);
        Mesh shardMesh = new Mesh();
        newShard.GetComponent<MeshFilter>().mesh = shardMesh;
        newShard.GetComponent<MeshCollider>().sharedMesh = shardMesh;
        shardMesh.vertices = points.ToArray();
        shardMesh.triangles = triangles.ToArray();
        shardMesh.RecalculateNormals();
        return newShard;
    }
    bool IsTriangleOrientedClockwise(Vector2 p1, Vector2 p2, Vector2 p3)
    {
        bool isClockWise = true;

        float determinant = p1.x * p2.y + p3.x * p1.y + p2.x * p3.y - p1.x * p3.y - p3.x * p2.y - p2.x * p1.y;

        if (determinant > 0f)
        {
            isClockWise = false;
        }

        return isClockWise;
    }
    public bool IsPointInTriangle(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p)
    {
        bool isWithinTriangle = false;
        float denominator = ((p2.y - p3.y) * (p1.x - p3.x) + (p3.x - p2.x) * (p1.y - p3.y));
        float a = ((p2.y - p3.y) * (p.x - p3.x) + (p3.x - p2.x) * (p.y - p3.y)) / denominator;
        float b = ((p3.y - p1.y) * (p.x - p3.x) + (p1.x - p3.x) * (p.y - p3.y)) / denominator;
        float c = 1 - a - b;
        if (a > 0f && a < 1f && b > 0f && b < 1f && c > 0f && c < 1f)
        {
            isWithinTriangle = true;
        }
        return isWithinTriangle;
    }
    private void OnDrawGizmos()
    {
        //Gizmos.color = Color.red;
        //for (int i = 0; i < glassPoints.Count; ++i)
        //    Gizmos.DrawSphere(glassPoints[i], 0.1f);
        //Gizmos.color = Color.blue;
        //for (int i = 0; i < cuttingShapes.Count; ++i)
        //    for (int j = 0; j < cuttingShapes[i].Count; ++j)
        //        Gizmos.DrawSphere(cuttingShapes[i][j], 0.1f);
    }

}

// MIGHT USE LATER
//    else
//{
//    shardPoints.Clear();
//    shardPoints.Add(intersections[1].Item1);
//    shardPoints.Add(glassPoints[glassIndex]);
//    shardPoints.Add(intersections[2].Item1);
//    int cutIter = intersections[0].Item2 > intersections[1].Item2 ? intersections[0].Item2 - intersections[1].Item2 : cuttingShapes[inside[i]].Count - intersections[1].Item2 + intersections[0].Item2;
//    for (int j = 0; j<cutIter; ++j)
//        shardPoints.Add(cuttingShapes[inside[i]][intersections[1].Item2 + 1 + j]);
//}
