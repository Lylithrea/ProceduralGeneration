
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using UnityEditor;
using UnityEngine;

[ExecuteAlways]
public class StreetGenerator : MonoBehaviour
{
    [Range(1f, 15f)]
    public float minStreetLength = 1f;
    [Range(5f, 50f)]
    public float maxStreetLength = 5f;
    [Range(5f, 25f)]
    public float likelyhoodSize = 5f;
    [Range(1f, 15f)]
    public float roadThickness = 1;
    [Range(1f, 25f)]
    public float snapRadius = 4;
    [Range(0, 25)]
    public int angleSize = 10;
    public LayerMask m_LayerMask;

    public GameObject streetParent;
    public GameObject emptyNode;
    public GameObject roadCube;

    private List<GameObject> Nodes = new List<GameObject>();
    private List<GameObject> NewNodes = new List<GameObject>();

    private GameObject firstNode;
    private bool isFirstRun = true;


    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.V))
        {
            RemoveEverything();
        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            GenerateStreets();
        }
        if (Input.GetKeyDown(KeyCode.B))
        {
            GenerateBuildings();
        }

    }

    public void RemoveEverything()
    {
        DestroyRoads();
        GetComponent<BuildingGenerator>().DestroyBuildings();
    }

    public void GenerateStreets()
    {
        firstNode = Instantiate(emptyNode, new Vector3(0, 0, 0), Quaternion.identity, streetParent.transform);
        firstNode.GetComponent<Node>().parentAmount = 0;
        Nodes.Add(firstNode);
        isFirstRun = true;

        while (Nodes.Count != 0)        //Spawn all nodes and roads
        {
            foreach (GameObject currentNode in Nodes)
            {
                if (currentNode.GetComponent<Node>().parentAmount < Random.Range(likelyhoodSize / 2, likelyhoodSize))
                {
                    CreateSpheres(currentNode);
                }
            }
            Nodes = NewNodes;
            NewNodes = new List<GameObject>();
        }
    }

    public void GenerateBuildings()
    {
        GetComponent<BuildingGenerator>().StartLoadSegments();
    }


    void CreateSpheres(GameObject currentNode)
    {
        int splitAmount = Random.Range(0, 3);
        if (isFirstRun)
        {
            splitAmount = 3;
            isFirstRun = false;
        }
        int rot;
        switch (splitAmount)
        {
            case 0:
                rot = Random.Range(0 - angleSize, 0 + angleSize);
                SpawnNode(currentNode, rot);
                break;
            case 1:
                rot = Random.Range(-45 - angleSize, -45 + angleSize);
                SpawnNode(currentNode, rot);
                rot = Random.Range(90 - angleSize, 90 + angleSize);
                SpawnNode(currentNode, rot);
                break;
            case 2:
                rot = Random.Range(0 - angleSize, 0 + angleSize);
                SpawnNode(currentNode, rot);
                rot = Random.Range(90 - angleSize, 90 + angleSize);
                SpawnNode(currentNode, rot);
                rot = Random.Range(180 - angleSize, 180 + angleSize);
                SpawnNode(currentNode, rot);
                break;
            case 3:
                rot = 0;
                SpawnNode(currentNode, rot);
                rot = 90;
                SpawnNode(currentNode, rot);
                rot = 180;
                SpawnNode(currentNode, rot);
                rot = 270;
                SpawnNode(currentNode, rot);
                break;
        }
    }

    private void SpawnNode(GameObject currentNode, int newRotation)
    {
        float roadLength = Random.Range(minStreetLength, maxStreetLength);
        currentNode.transform.rotation *= Quaternion.Euler(0, newRotation, 0);
        Vector3 newPos23 = currentNode.transform.position + currentNode.transform.forward * roadLength;
        
        GameObject newNode = Instantiate(emptyNode, newPos23, currentNode.transform.rotation, streetParent.transform);
        newNode.GetComponent<Node>().parentNode = currentNode;
        ColliderCheck(newNode);
        if (!newNode.GetComponent<Node>().hasMoved)
        {
            newNode.GetComponent<Node>().parentAmount = currentNode.GetComponent<Node>().parentAmount + 1;
            NewNodes.Add(newNode);
        }
        if (newNode.GetComponent<Node>().parentNode != null)
        {
            RoadCreator(newNode.GetComponent<Node>().parentNode, newNode);
        }
    }

    private void ColliderCheck(GameObject node)
    {
        Collider[] hitColliders = Physics.OverlapSphere(node.transform.position, snapRadius, m_LayerMask);
        int i = 0;
        while (i < hitColliders.Length)
        {
            if (hitColliders[i].gameObject != node)
            {
                node.transform.position = hitColliders[i].transform.position;
                node.GetComponent<Node>().hasMoved = true;
            }
            i++;
        }
    }

    private void RoadCreator(GameObject node1, GameObject node2)
    {
        Vector3 roadScale = new Vector3(roadThickness, 0.1f, 0.5f);
        roadScale.z = Vector3.Distance(node1.transform.position, node2.transform.position);
        Vector3 newPos = node1.transform.position;
        node1.transform.LookAt(node2.transform);
        newPos +=  node1.transform.forward * (roadScale.z / 2);
        GameObject newRoad = Instantiate(roadCube, newPos, node1.transform.rotation, streetParent.transform);
        newRoad.transform.localScale = roadScale;
        GetComponent<BuildingGenerator>().BuildingSpawner(node1, node2, roadThickness);
    }


    void DestroyRoads()
    {
        foreach (Transform children in streetParent.transform)
        {
            DestroyImmediate(children.gameObject);
        }
        Nodes.Clear();
        NewNodes.Clear();
    }

}









/*if (Input.GetKeyDown(KeyCode.X))
        {
            pathPoints.Clear();
            destroyRoads();

GameObject centre = Instantiate(sphere, new Vector3(0, 0, 0), Quaternion.identity);
centre.transform.parent = streetParent.transform;
            lastSphere = centre;
            pathPoints.Add(centre.transform);
            int randomRoad = Random.Range(2, 4);

            for(int i = 0; i<randomRoad; i++)
            {
                pathPoints.Clear();
                Vector3 newpos = new Vector3(0, 0, 0);
newpos.x = Random.Range(minStreetLength, maxStreetLength);
                newpos.z = Random.Range(lastSphere.transform.position.z - 120, lastSphere.transform.position.z + 60);
                newpos.x += lastSphere.transform.position.x;

                GameObject newSphere = Instantiate(sphere, newpos, Quaternion.identity);
pathPoints.Add(newSphere.transform);
                newSphere.transform.parent = streetParent.transform;
                lastSphere = newSphere;
                createRoad();
            }
            
        }*/



/*void createRoad()
{
    int currentStreetLength = Random.Range(streetLengthMin, streetLengthMax);
    for (int i = 1; i <= currentStreetLength; i++)
    {
        Vector3 newpos = new Vector3(0, 0, 0);

        newpos.x = Random.Range(minStreetLength, maxStreetLength);
        newpos.z = Random.Range(lastSphere.transform.position.z - streetCurve, lastSphere.transform.position.z + streetCurve);
        newpos.x += lastSphere.transform.position.x;

        GameObject newSphere = Instantiate(sphere, newpos, Quaternion.identity);

        newSphere.transform.parent = streetParent.transform;
        lastSphere = newSphere;
        pathPoints.Add(newSphere.transform);
    }
    lastPoint = lastSphere.transform;
    bezierPath = new BezierPath(pathPoints, false, PathSpace.xyz);
    bezierPath.autoControlLength = streetSmoothness;

    GameObject newRoad = Instantiate(streetObject, new Vector3(0, 0, 0), Quaternion.identity);
    newRoad.transform.parent = streetParent.transform;
    GetComponent<PathCreator>().bezierPath = bezierPath;
    transform.GetComponent<BuildingGenerator>().spawnBuildings();
}*/



/*private void MeshCreator(GameObject node1, GameObject node2)
{
    pathPoints.Clear();
    pathPoints.Add(node1.transform);
    pathPoints.Add(node2.transform);
    bezierPath = new BezierPath(pathPoints, false, PathSpace.xyz);
    bezierPath.autoControlLength = streetSmoothness;
    bezierPath.controlMode = BezierPath.ControlMode.Automatic;

    Vector3 pos = (node1.transform.position + node2.transform.position) / 2;
    GameObject newRoad = Instantiate(streetObject, pos, Quaternion.identity);
    newRoad.transform.parent = streetParent.transform;
    newRoad.GetComponent<PathCreator>().bezierPath = bezierPath;
    bezierPath.controlMode = BezierPath.ControlMode.Automatic;
}*/
