using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
public class ProceduralWallScript : MonoBehaviour
{
    [SerializeField] Transform[] centers;
    [SerializeField] float radius;
    [SerializeField] int pointsOnCircle;
    [SerializeField] List<Vector3> points = new List<Vector3>(); //add a doubly linked list later // this is less optimal // repeat the process for now, later restart the process
    Mesh wallMesh;
    // convert this into a mesh
    List<Vector3> wallVertices = new List<Vector3>();
    List<List<Vector3>> holes = new List<List<Vector3>>();

    //just to make it visible move it later
    [SerializeField] List<int> triangles = new List<int>();

    //for test purposes only
    Vector3 lastCenter;
    // triangle count is (vertices - 2) * 3
    //REMEMBER TO RECALCULATE THE NORMALS
    // Start is called before the first frame update
    // Add combining meshes
    // save the mesh and add on to it, change only what needs to be changed for optimization, just add polygons

    //Make a display for each point and debug, YAY

    //Make deformable mesh + voronoi for shattering

    //rework everything so that the destroyable meshes work in an orientation
    void Start()
    {
        //wallMesh = new Mesh();
        //GetComponent<MeshFilter>().mesh = wallMesh;
        //GetComponent<MeshCollider>().sharedMesh = wallMesh;
        //Vector3[] vertices = new Vector3[4];
        //Vector2[] uv = new Vector2[4];
        //int[] triangles = new int[6];
        //vertices[0] = new Vector3(-5, -5, 0);
        //vertices[1] = new Vector3(-5, 5, 0);
        //vertices[2] = new Vector3(5, 5, 0);
        //vertices[3] = new Vector3(5, -5, 0);
        //uv[0] = new Vector2(0, 0);
        //uv[1] = new Vector2(0, 1);
        //uv[2] = new Vector2(1, 1);
        //uv[3] = new Vector2(1, 0);
        //triangles[0] = 0;
        //triangles[1] = 1;
        //triangles[2] = 2;
        //triangles[3] = 0;
        //triangles[4] = 2;
        //triangles[5] = 3;
        //wallMesh.vertices = vertices;
        //wallMesh.triangles = triangles;
        //wallMesh.uv = uv;
        //wallMesh.RecalculateNormals();

        //Use ToArray
        // wallMesh will be a pregenerated mesh
        // Add all meshes that overlap with the wall to the points directly, combine the meshes into one mesh
        wallMesh = new Mesh();
        GetComponent<MeshFilter>().mesh = wallMesh;
        //GetComponent<MeshCollider>().sharedMesh = wallMesh; // change this into a different Mesh as this will cause it to be too laggy
        //wallVertices.Add(new Vector3(-5, -5, 0));
        //wallVertices.Add(new Vector3(-5, 5, 0));
        //wallVertices.Add(new Vector3(5, 5, 0));
        //wallVertices.Add(new Vector3(5, -5, 0));

        //// This is so unoptimized, maybe chunk everything differently for better processing times
        ////Make a list of all the points and then make everything based on the/
        //for (int i = 0; i < centers.Length; ++i) // add the wall stuff for when you are shooting the gun // rework destruction on a large scale
        //{
        //    List<Vector3> holeVertices = new List<Vector3>();
        //    for (int j = 0; j < pointsOnCircle; ++j)
        //    {
        //        float angle = 360f / pointsOnCircle * (j + 1f / 2f) / 180 * Mathf.PI;
        //        Vector3 point = new Vector3(centers[i].transform.position.x + radius * Mathf.Cos(angle), centers[i].transform.position.y + radius * Mathf.Sin(angle), 0);
        //        holeVertices.Add(point);
        //    }
        //    if (holeVertices.Count == 0)
        //        continue;
        //    bool isColliding = false;
        //    bool isCollidingWall = false;
        //    List<(Vector2, int, int)> intersections = new List<(Vector2, int, int)>();//get the furthest points // 0 = point, 1 = start / end index shape 1 collision, 2 = start / end shape 2 collision
        //    //List<(Vector2, int, int, int, int)> intersections = new List<(Vector2, int, int, int, int)>();// 0 = point, 1, 2 = start / end index shape 1 collision, 3, 4 = start / end shape 2 collision
        //    //only include the furthest points for now, we only need normal shapes
        //    for (int j = 0; j < wallVertices.Count; ++j) // optimize this later // you can just not iterate through the times that it is intersection // you don't need to check sides that have already been checked
        //    {
        //        Vector2 next = wallVertices[(j + 1) % wallVertices.Count];
        //        float x1 = wallVertices[j].x, y1 = wallVertices[j].y;
        //        float x2 = next.x, y2 = next.y;
        //        for (int k = 0; k < holeVertices.Count; ++k)
        //        {
        //            next = holeVertices[(k + 1) % holeVertices.Count];
        //            float x3 = holeVertices[k].x, y3 = holeVertices[k].y;
        //            float x4 = next.x, y4 = next.y;
        //            //just make a function for this // (I'm lazy)
        //            float uA = ((x4 - x3) * (y1 - y3) - (y4 - y3) * (x1 - x3)) / ((y4 - y3) * (x2 - x1) - (x4 - x3) * (y2 - y1));
        //            float uB = ((x2 - x1) * (y1 - y3) - (y2 - y1) * (x1 - x3)) / ((y4 - y3) * (x2 - x1) - (x4 - x3) * (y2 - y1));
        //            if (uA > 0 && uA < 1 && uB > 0 && uB < 1) // exclude rare cases (points are identical / deal with this later)
        //            {
        //                isColliding = true;
        //                isCollidingWall = true;
        //                float intersectionX = x1 + (uA * (x2 - x1));
        //                float intersectionY = y1 + (uA * (y2 - y1));
        //                Vector2 intersect = new Vector2(intersectionX, intersectionY);
        //                if (intersections.Count == 0)
        //                    intersections.Add((intersect, j, k));
        //                else if (intersections.Count == 1)
        //                    intersections.Add((intersect, j, k));
        //                else if (Vector2.Distance(intersect, intersections[0].Item1) > Vector2.Distance(intersections[0].Item1, intersections[1].Item1) &&
        //                         Vector2.Distance(intersect, intersections[0].Item1) > Vector2.Distance(intersect, intersections[1].Item1))
        //                    intersections[1] = (intersect, j, k);
        //                else if (Vector2.Distance(intersect, intersections[1].Item1) > Vector2.Distance(intersections[0].Item1, intersections[1].Item1))// Add this if necessary:  && Vector2.Distance(intersect, intersections[1].Item1) > Vector2.Distance(intersect, intersections[0].Item1)
        //                    intersections[0] = (intersect, j, k);
        //            }
        //        }
        //    }
        //    if (!isColliding)
        //        holes.Add(holeVertices); // this currently regenerates the object, make it so that it moves the points later
        //    else if (isColliding)
        //    {
        //        if (isCollidingWall)
        //        {
        //            if (intersections.Count == 1)
        //            {
        //                //add this interaction later
        //            }
        //            else if (intersections.Count == 2) //account for when intersections and points are the same + 1 intersection
        //            {
        //                int hole1Index = intersections[0].Item3;
        //                int hole2Index = (intersections[1].Item3 + 1) % holeVertices.Count;
        //                int wall1Index = intersections[0].Item2;
        //                int wall2Index = (intersections[1].Item2 + 1) % wallVertices.Count;
        //                Vector2 inter1 = intersections[0].Item1;
        //                Vector2 inter2 = intersections[1].Item1;
        //                if ((intersections[1].Item2 < intersections[0].Item2) || (intersections[1].Item2 == wallVertices.Count - 1 && intersections[0].Item2 == 0) ||
        //                     Vector2.Distance(intersections[1].Item1, wallVertices[intersections[0].Item2]) < Vector2.Distance(intersections[0].Item1, wallVertices[intersections[0].Item2])
        //                     )
        //                {
        //                    hole1Index = intersections[1].Item3;
        //                    hole2Index = (intersections[0].Item3 + 1) % holeVertices.Count;
        //                    wall1Index = intersections[1].Item2;
        //                    wall2Index = (intersections[0].Item2 + 1) % wallVertices.Count;
        //                    inter1 = intersections[1].Item1;
        //                    inter2 = intersections[0].Item1;
        //                }
        //                //Debug.Log("hole1Index is: " + hole1Index);
        //                //Debug.Log("hole2Index is: " + hole2Index);
        //                //Debug.Log("wall1Index is: " + wall1Index);
        //                //Debug.Log("wall2Index is: " + wall2Index);
        //                int wallIters = 0 > wall2Index - wall1Index ? wallVertices.Count - wall1Index + wall2Index : wall2Index - wall1Index;
        //                List<Vector2> tempVertices = new List<Vector2>();
        //                for (int j = 1; j < wallIters; ++j)
        //                {
        //                    tempVertices.Add(wallVertices[(wall1Index + j) % wallVertices.Count]);
        //                    Debug.Log("indices are: " + (wall1Index + j) % wallVertices.Count);
        //                }

        //                for (int j = 0; j < tempVertices.Count; ++j)
        //                    wallVertices.Remove(tempVertices[j]);
        //                int intersectIndex = Mathf.Min(wall1Index + 1, wallVertices.Count);
        //                wallVertices.Insert(intersectIndex, inter1);
        //                wallVertices.Insert(intersectIndex + 1, inter2);
        //                int count = 0;
        //                for (int j = (hole1Index + 1) % holeVertices.Count; j != hole2Index; j = (j + 1) % holeVertices.Count)
        //                {
        //                    wallVertices.Insert(intersectIndex + 1 + count, holeVertices[j]);
        //                    ++count;
        //                }
        //            }
        //        }
        //    }
        //}
        //for (int i = 0; i < wallVertices.Count; ++i)
        //    points.Add(wallVertices[i]);
        //List<(Vector2, Vector2, int)> holeConnection = new List<(Vector2, Vector2, int)>(); // 1 = connectionPoint, 2 = leftMostPoint, 3 = hole index
        //for (int i = 0; i < holes.Count; ++i) // write the simplest case right now // check for other points in the triangles for connecting later
        //{
        //    //greatest x value
        //    int maxIndex = 0;
        //    for (int j = 1; j < holes[i].Count; ++j)
        //    {
        //        if (Mathf.Round(holes[i][maxIndex].x * 100) / 100 < Mathf.Round(holes[i][j].x * 100) / 100)
        //            maxIndex = j;
        //    }
        //    float x1 = holes[i][maxIndex].x, y1 = holes[i][maxIndex].y;

        //    for (int j = 0; j < points.Count; ++j)
        //    {
        //        float x3 = points[j].x, y3 = points[j].y;
        //        float x4 = points[(j + 1) % points.Count].x, y4 = points[(j + 1) % points.Count].y;
        //        float x2 = (x3 > x4 ? x3 : x4) + 1, y2 = holes[i][maxIndex].y;
        //        float uA = ((x4 - x3) * (y1 - y3) - (y4 - y3) * (x1 - x3)) / ((y4 - y3) * (x2 - x1) - (x4 - x3) * (y2 - y1));
        //        float uB = ((x2 - x1) * (y1 - y3) - (y2 - y1) * (x1 - x3)) / ((y4 - y3) * (x2 - x1) - (x4 - x3) * (y2 - y1));
        //        if (uA >= 0 && uA <= 1 && uB >= 0 && uB <= 1)
        //        {
        //            float intersectionX = x1 + (uA * (x2 - x1));
        //            float intersectionY = y1 + (uA * (y2 - y1));
        //            Vector2 intersect = new Vector2(intersectionX, intersectionY);
        //            int indexOffset = Vector2.Distance(new Vector2(x3, y3), intersect) < Vector2.Distance(new Vector2(x4, y4), intersect) ? 1 : 2;
        //            for (int k = 0; k < holes[i].Count; ++k)
        //            {
        //                points.Insert(j + indexOffset + k, holes[i][(maxIndex + k) % holes[i].Count]);
        //            }
        //            points.Insert(j + indexOffset + holes[i].Count, holes[i][maxIndex]);
        //            points.Insert(j + indexOffset + 1 + holes[i].Count, points[j + indexOffset - 1]);
        //            break;
        //        }
        //    }
        //}
        //// MAKE SURE THAT THE ORDERING IS MORE ROBUST, WHOEVER GOES LAST WILL BE FIRST, CHECK FOR OTHER INTERSECTIONS
        ////Optimize later // identify all ears
        //List<int> indices = new List<int>();
        //for (int i = 0; i < points.Count; ++i)
        //    indices.Add(i);
        //for (int i = 0; i < points.Count; ++i)
        //{
        //    for (int j = 0; j < points.Count - i; ++j)
        //    {
        //        Vector3 last = points[indices[(j - 1) < 0 ? indices.Count - 1 : (j - 1)]];
        //        Vector3 vertex = points[indices[j]];
        //        Vector3 next = points[indices[(j + 1) % indices.Count]];
        //        // figure out a better angle solution                
        //        if (IsTriangleOrientedClockwise(last, vertex, next)) // figure out if it is already reflex, so it does not need to be recalculated
        //        {
        //            bool isEar = true;
        //            for (int k = 0; k < points.Count - i; ++k)
        //            {
        //                if (k == j || k == ((j - 1) < 0 ? indices.Count - 1 : (j - 1)) || k == (j + 1) % indices.Count)
        //                    continue;
        //                if (IsPointInTriangle(last, vertex, next, points[indices[k]]))
        //                {
        //                    isEar = false;
        //                    break;
        //                }
        //            }
        //            if (isEar)
        //            {
        //                triangles.Add(indices[(j - 1) < 0 ? indices.Count - 1 : (j - 1)]);
        //                triangles.Add(indices[j]);
        //                triangles.Add(indices[(j + 1) % indices.Count]);
        //                indices.RemoveAt(j);
        //                break;
        //            }

        //        }
        //    }
        //    if (indices.Count == 3)
        //    {
        //        triangles.Add(indices[0]);
        //        triangles.Add(indices[1]);
        //        triangles.Add(indices[2]);
        //        break;
        //    }
        //}
        ////for (int i = 0; i < points.Count; ++i)
        ////{
        ////    Vector3 last = points[i - 1 < 0 ? points.Count - 1 : i - 1];
        ////    Vector3 vertex = points[i];
        ////    Vector3 next = points[(i + 1) % points.Count];
        ////    bool clockwise = IsTriangleOrientedClockwise(last, vertex,  next);
        ////    Debug.Log("Point " + i + " clockwise is: " + clockwise);
        ////}
        //wallMesh.vertices = points.ToArray();
        //wallMesh.triangles = triangles.ToArray();
        //wallMesh.RecalculateNormals();
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
    // Update is called once per frame
    void Update()
    {
        triangles.Clear();
        points.Clear();
        holes.Clear();
        wallVertices.Clear();
        wallVertices.Add(new Vector3(-5, -5, 0));
        wallVertices.Add(new Vector3(-5, 5, 0));
        wallVertices.Add(new Vector3(5, 5, 0));
        wallVertices.Add(new Vector3(5, -5, 0));

        // This is so unoptimized, maybe chunk everything differently for better processing times
        //Make a list of all the points and then make everything based on the/
        //for (int i = 0; i < centers.Length; ++i) // add the wall stuff for when you are shooting the gun // rework destruction on a large scale
        //{
        //    List<Vector3> holeVertices = new List<Vector3>();
        //    for (int j = 0; j < pointsOnCircle; ++j)
        //    {
        //        float angle = 360f / pointsOnCircle * (j + 1f / 2f) / 180 * Mathf.PI;
        //        Vector3 point = new Vector3(centers[i].transform.position.x + radius * Mathf.Cos(angle), centers[i].transform.position.y + radius * Mathf.Sin(angle), 0);
        //        holeVertices.Add(point);
        //    }
        //    bool isColliding = false; //I am being an idiot
        //    bool isWallColliding = false;
        //    List<Vector3> newWallVertices = new List<Vector3>();
        //    List<(Vector3, int, int)> intersects = new List<(Vector3, int, int)>(); //int = hole segment, int = wall segment number
        //    // optimize this later // you can just not iterate through the times that it is intersection // you don't need to check sides that have already been checked
        //    //reverse the order of this holes then walls 
        //    for (int j = 0; j < holeVertices.Count; ++j) 
        //    {
        //        Vector2 next = holeVertices[(j + 1) % holeVertices.Count];
        //        float x1 = holeVertices[j].x, y1 = holeVertices[j].y;
        //        float x2 = next.x, y2 = next.y;
        //        List<(Vector3, int, int)> tempIntersects = new List<(Vector3, int, int)>();
        //        for (int k = 0; k < wallVertices.Count; ++k)
        //        {
        //            next = wallVertices[(k + 1) % wallVertices.Count];
        //            float x3 = wallVertices[k].x, y3 = wallVertices[k].y;
        //            float x4 = next.x, y4 = next.y;
        //            float uA = ((x4 - x3) * (y1 - y3) - (y4 - y3) * (x1 - x3)) / ((y4 - y3) * (x2 - x1) - (x4 - x3) * (y2 - y1));
        //            float uB = ((x2 - x1) * (y1 - y3) - (y2 - y1) * (x1 - x3)) / ((y4 - y3) * (x2 - x1) - (x4 - x3) * (y2 - y1));

        //            if (uA > 0 && uA < 1 && uB > 0 && uB < 1) // exclude rare cases (points are identical / deal with this later)
        //            {
        //                isColliding = true;
        //                isWallColliding = true;
        //                float intersectionX = x1 + (uA * (x2 - x1));
        //                float intersectionY = y1 + (uA * (y2 - y1));
        //                Vector2 intersect = new Vector2(intersectionX, intersectionY); // organize intersections
        //                tempIntersects.Add((intersect, j, k));
        //            }
        //        }
        //        if (tempIntersects.Count >= 2 &&
        //            Vector3.Distance(holeVertices[j], tempIntersects[0].Item1) < Vector3.Distance(holeVertices[j], tempIntersects[tempIntersects.Count - 1].Item1))
        //        {
        //            tempIntersects.Reverse();
        //            intersects.InsertRange(intersects.Count, tempIntersects);
        //        } 
        //        else
        //            intersects.InsertRange(intersects.Count, tempIntersects);
        //    }
        //    if (!isColliding)
        //    {

        //        holes.Add(holeVertices); // this currently regenerates the object, make it so that it moves the points later // if it splits the object, make a new object // do this later
        //    }
        //    else if (isWallColliding)
        //    {
        //        if (intersects.Count == 1)
        //        {

        //        }
        //        else
        //        {

        //            for (int j = 0; j < intersects.Count; j += 2)
        //            {
        //                // make something for 3 to 0 // make everything based off of item3, not 2
        //                List<Vector3> segment = new List<Vector3>();
        //                bool counterTurn = (intersects[j + 1].Item2 < intersects[j].Item2 ? holeVertices.Count - intersects[j].Item2 + intersects[j + 1].Item2 : intersects[j + 1].Item2 - intersects[j].Item2) > 
        //                                   (intersects[j].Item2 < intersects[j + 1].Item2 ? holeVertices.Count - intersects[j + 1].Item2 + intersects[j].Item2 : intersects[j].Item2 - intersects[j + 1].Item2);
        //                int holeIters = intersects[j].Item3 > intersects[j + 1].Item3 || counterTurn ? holeVertices.Count - intersects[j + 1].Item2 + intersects[j].Item2 : intersects[j + 1].Item2 - intersects[j].Item2;
        //                int wallIters = intersects[j].Item3 > intersects[j + 1].Item3 || counterTurn ? intersects[j + 1].Item3 - intersects[j].Item3 : holeVertices.Count - intersects[j + 1].Item3 + intersects[j].Item3;
        //                Debug.Log("holeIters is: " + holeIters);
        //                Debug.Log("wallIters is: " + wallIters);
        //                segment.Add(intersects[j].Item1);
        //                //Debug.Log("itersection 1 is: " + intersects[j]);
        //                //Debug.Log("itersection 2 is: " + intersects[j + 1]); // check if over after // lazy
        //                if (intersects[j].Item3 > intersects[j + 1].Item3 || counterTurn ||
        //                    (intersects[j].Item3 == intersects[j + 1].Item3 && Vector2.Distance(intersects[j].Item1, wallVertices[intersects[j].Item3]) > Vector2.Distance(intersects[j + 1].Item1, wallVertices[intersects[j].Item3])))
        //                {
        //                    for (int k = 0; k < wallIters; ++k)
        //                        segment.Insert(segment.Count, wallVertices[(intersects[j].Item3 + k + 1) % wallVertices.Count]);
        //                    segment.Insert(segment.Count, intersects[j + 1].Item1);
        //                    for (int k = 0; k < holeIters; ++k)
        //                        segment.Insert(segment.Count, holeVertices[(intersects[j + 1].Item2 + k + 1) % holeVertices.Count]);

        //                }
        //                else
        //                {

        //                    for (int k = 0; k < holeIters; ++k)
        //                        segment.Insert(segment.Count, holeVertices[(intersects[j].Item2 + k + 1) % holeVertices.Count]);
        //                    segment.Insert(segment.Count, intersects[j + 1].Item1);
        //                    for (int k = 0; k < wallIters; ++k)
        //                        segment.Insert(segment.Count, wallVertices[(intersects[j + 1].Item3 + k + 1) % wallVertices.Count]);
        //                }
        //                newWallVertices.InsertRange(newWallVertices.Count, segment);
        //            }
        //        }
        //        wallVertices = newWallVertices;
        //    }

        //}
        for (int i = 0; i < centers.Length; ++i) // add the wall stuff for when you are shooting the gun // rework destruction on a large scale
        {
            List<Vector3> holeVertices = new List<Vector3>();
            for (int j = 0; j < pointsOnCircle; ++j)
            {
                float angle = 360f / pointsOnCircle * (j + 1f / 2f) / 180 * Mathf.PI;
                Vector3 point = new Vector3(centers[i].transform.position.x + radius * Mathf.Cos(angle), centers[i].transform.position.y + radius * Mathf.Sin(angle), 0);
                holeVertices.Add(point);
            }
            if (holeVertices.Count == 0)
                continue;
            bool isColliding = false;
            bool isCollidingWall = false;
            List<(Vector2, int, int)> intersections = new List<(Vector2, int, int)>();//get the furthest points // 0 = point, 1 = start / end index shape 1 collision, 2 = start / end shape 2 collision
            //List<(Vector2, int, int, int, int)> intersections = new List<(Vector2, int, int, int, int)>();// 0 = point, 1, 2 = start / end index shape 1 collision, 3, 4 = start / end shape 2 collision
            //only include the furthest points for now, we only need normal shapes
            for (int j = 0; j < wallVertices.Count; ++j) // optimize this later // you can just not iterate through the times that it is intersection // you don't need to check sides that have already been checked
            {
                Vector2 next = wallVertices[(j + 1) % wallVertices.Count];
                float x1 = wallVertices[j].x, y1 = wallVertices[j].y;
                float x2 = next.x, y2 = next.y;
                for (int k = 0; k < holeVertices.Count; ++k)
                {
                    next = holeVertices[(k + 1) % holeVertices.Count];
                    float x3 = holeVertices[k].x, y3 = holeVertices[k].y;
                    float x4 = next.x, y4 = next.y;
                    //just make a function for this // (I'm lazy)
                    float uA = ((x4 - x3) * (y1 - y3) - (y4 - y3) * (x1 - x3)) / ((y4 - y3) * (x2 - x1) - (x4 - x3) * (y2 - y1));
                    float uB = ((x2 - x1) * (y1 - y3) - (y2 - y1) * (x1 - x3)) / ((y4 - y3) * (x2 - x1) - (x4 - x3) * (y2 - y1));
                    if (uA > 0 && uA < 1 && uB > 0 && uB < 1) // exclude rare cases (points are identical / deal with this later)
                    {
                        isColliding = true;
                        isCollidingWall = true;
                        float intersectionX = x1 + (uA * (x2 - x1));
                        float intersectionY = y1 + (uA * (y2 - y1));
                        Vector2 intersect = new Vector2(intersectionX, intersectionY);
                        if (intersections.Count == 0)
                            intersections.Add((intersect, j, k));
                        else if (intersections.Count == 1)
                            intersections.Add((intersect, j, k));
                        else if (Vector2.Distance(intersect, intersections[0].Item1) > Vector2.Distance(intersections[0].Item1, intersections[1].Item1) &&
                                 Vector2.Distance(intersect, intersections[0].Item1) > Vector2.Distance(intersect, intersections[1].Item1))
                            intersections[1] = (intersect, j, k);
                        else if (Vector2.Distance(intersect, intersections[1].Item1) > Vector2.Distance(intersections[0].Item1, intersections[1].Item1))// Add this if necessary:  && Vector2.Distance(intersect, intersections[1].Item1) > Vector2.Distance(intersect, intersections[0].Item1)
                            intersections[0] = (intersect, j, k);
                    }
                }
            }
            if (!isColliding)
                holes.Add(holeVertices); // this currently regenerates the object, make it so that it moves the points later
            else if (isColliding)
            {
                if (isCollidingWall)
                {
                    if (intersections.Count == 1)
                    {
                        //add this interaction later
                    }
                    else if (intersections.Count == 2) //account for when intersections and points are the same + 1 intersection
                    {
                        int hole1Index = intersections[0].Item3;
                        int hole2Index = (intersections[1].Item3 + 1) % holeVertices.Count;
                        int wall1Index = intersections[0].Item2;
                        int wall2Index = (intersections[1].Item2 + 1) % wallVertices.Count;
                        Vector2 inter1 = intersections[0].Item1;
                        Vector2 inter2 = intersections[1].Item1;
                        if ((intersections[1].Item2 < intersections[0].Item2) || (intersections[1].Item2 == wallVertices.Count - 1 && intersections[0].Item2 == 0) ||
                             Vector2.Distance(intersections[1].Item1, wallVertices[intersections[0].Item2]) < Vector2.Distance(intersections[0].Item1, wallVertices[intersections[0].Item2])
                             )
                        {
                            hole1Index = intersections[1].Item3;
                            hole2Index = (intersections[0].Item3 + 1) % holeVertices.Count;
                            wall1Index = intersections[1].Item2;
                            wall2Index = (intersections[0].Item2 + 1) % wallVertices.Count;
                            inter1 = intersections[1].Item1;
                            inter2 = intersections[0].Item1;
                        }
                        //Debug.Log("hole1Index is: " + hole1Index);
                        //Debug.Log("hole2Index is: " + hole2Index);
                        //Debug.Log("wall1Index is: " + wall1Index);
                        //Debug.Log("wall2Index is: " + wall2Index);
                        int wallIters = 0 > wall2Index - wall1Index ? wallVertices.Count - wall1Index + wall2Index : wall2Index - wall1Index;
                        List<Vector2> tempVertices = new List<Vector2>();
                        for (int j = 1; j < wallIters; ++j)
                        {
                            tempVertices.Add(wallVertices[(wall1Index + j) % wallVertices.Count]);
                            Debug.Log("indices are: " + (wall1Index + j) % wallVertices.Count);
                        }

                        for (int j = 0; j < tempVertices.Count; ++j)
                            wallVertices.Remove(tempVertices[j]);
                        int intersectIndex = Mathf.Min(wall1Index + 1, wallVertices.Count);
                        wallVertices.Insert(intersectIndex, inter1);
                        wallVertices.Insert(intersectIndex + 1, inter2);
                        int count = 0;
                        for (int j = (hole1Index + 1) % holeVertices.Count; j != hole2Index; j = (j + 1) % holeVertices.Count)
                        {
                            wallVertices.Insert(intersectIndex + 1 + count, holeVertices[j]);
                            ++count;
                        }
                    }
                }
            }
        }
        for (int i = 0; i < wallVertices.Count; ++i)
            points.Add(wallVertices[i]);
        List<(Vector2, Vector2, int)> holeConnection = new List<(Vector2, Vector2, int)>(); // 1 = connectionPoint, 2 = leftMostPoint, 3 = hole index
        for (int i = 0; i < holes.Count; ++i) // write the simplest case right now // check for other points in the triangles for connecting later // add all these to the wall later so that you do not need to re render everything // saves processing
        {
            //greatest x value
            int maxIndex = 0;
            for (int j = 1; j < holes[i].Count; ++j)
            {
                if (Mathf.Round(holes[i][maxIndex].x * 100) / 100 < Mathf.Round(holes[i][j].x * 100) / 100)
                    maxIndex = j;
            }
            float x1 = holes[i][maxIndex].x, y1 = holes[i][maxIndex].y;
            Vector2 intersect = new Vector2();
            bool hasIntersected = false;
            int intersectionCount = 0;
            int intersectionIndex = 0;
            float x3 = 0, y3 = 0;
            float x4 = 0, y4 = 0;
            for (int j = 0; j < points.Count; ++j)
            {
                x3 = points[j].x; 
                y3 = points[j].y;
                x4 = points[(j + 1) % points.Count].x;
                y4 = points[(j + 1) % points.Count].y;
                float x2 = (x3 > x4 ? x3 : x4) + 1, y2 = holes[i][maxIndex].y;
                float uA = ((x4 - x3) * (y1 - y3) - (y4 - y3) * (x1 - x3)) / ((y4 - y3) * (x2 - x1) - (x4 - x3) * (y2 - y1));
                float uB = ((x2 - x1) * (y1 - y3) - (y2 - y1) * (x1 - x3)) / ((y4 - y3) * (x2 - x1) - (x4 - x3) * (y2 - y1));
                float intersectionX = x1 + (uA * (x2 - x1));
                float intersectionY = y1 + (uA * (y2 - y1));
                intersect = new Vector2(intersectionX, intersectionY);
                if (uA >= 0 && uA <= 1 && uB >= 0 && uB <= 1)
                {
                    hasIntersected = true;
                    ++intersectionCount;
                    intersectionIndex = j;
                }
            }
            if (hasIntersected && intersectionCount % 2 == 1)
            {
                int indexOffset = Vector2.Distance(new Vector2(x3, y3), intersect) < Vector2.Distance(new Vector2(x4, y4), intersect) ? 1 : 2;
                for (int k = 0; k < holes[i].Count; ++k)
                {
                    points.Insert(intersectionIndex + indexOffset + k, holes[i][(maxIndex + k) % holes[i].Count]);
                }
                points.Insert(intersectionIndex + indexOffset + holes[i].Count, holes[i][maxIndex]);
                points.Insert(intersectionIndex + indexOffset + 1 + holes[i].Count, points[intersectionIndex + indexOffset - 1]);
            }
        }
        // MAKE SURE THAT THE ORDERING IS MORE ROBUST, WHOEVER GOES LAST WILL BE FIRST, CHECK FOR OTHER INTERSECTIONS
        //Optimize later // identify all ears
        List<int> indices = new List<int>();
        for (int i = 0; i < points.Count; ++i)
            indices.Add(i);
        for (int i = 0; i < points.Count; ++i)
        {
            for (int j = 0; j < points.Count - i; ++j)
            {
                Vector3 last = points[indices[(j - 1) < 0 ? indices.Count - 1 : (j - 1)]];
                Vector3 vertex = points[indices[j]];
                Vector3 next = points[indices[(j + 1) % indices.Count]];
                // figure out a better angle solution                
                if (IsTriangleOrientedClockwise(last, vertex, next)) // figure out if it is already reflex, so it does not need to be recalculated
                {
                    bool isEar = true;
                    for (int k = 0; k < points.Count - i; ++k)
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
        //for (int i = 0; i < points.Count; ++i)
        //{
        //    Vector3 last = points[i - 1 < 0 ? points.Count - 1 : i - 1];
        //    Vector3 vertex = points[i];
        //    Vector3 next = points[(i + 1) % points.Count];
        //    bool clockwise = IsTriangleOrientedClockwise(last, vertex,  next);
        //    Debug.Log("Point " + i + " clockwise is: " + clockwise);
        //}
        wallMesh.Clear();
        wallMesh.vertices = points.ToArray();
        wallMesh.triangles = triangles.ToArray();
        wallMesh.RecalculateNormals();
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        for (int i = 0; i < points.Count; ++i)
            Gizmos.DrawSphere(points[i], 0.1f);
    }
}

