using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.Collections.LowLevel.Unsafe;
using System.Threading;

[ExecuteAlways]
public class BuildingGenerator : MonoBehaviour
{
    public bool spawnInstantHouses = true;
    public bool spawnAllHouses = true;
    public int spawnChance = 10;
    public int houseHeight = 5;
    [Range(0f, 10f)]
    public float padding = 5;
    [Range(1f, 50f)]
    public float maxBuildingHeight = 5;
    [Range(100f, 5000f)]
    public float cityCentre = 5000;

    public bool staticHeight = false;

    public GameObject buildingParent;
    public LayerMask m_LayerMask;

    public List<Building> buildGen = new List<Building>();
    public List<TempBuilding> Segments = new List<TempBuilding>();

    public float spacing = 3;

    // Start is called before the first frame update
    void Start()
    {
    }

   

    public void BuildingSpawner(GameObject node1, GameObject node2, float roadThickness)
    {
        float currentSpawnPos = 0;
        Vector3 buildingPos = node1.transform.position;

        node1.transform.LookAt(node2.transform);

        float normalX = node2.transform.position.x - node1.transform.position.x;
        float normalZ = node2.transform.position.z - node1.transform.position.z;

        float tempHeight = 0.875f * (Mathf.Pow((float)System.Math.E, (-Mathf.Pow(node1.GetComponent<Node>().parentAmount, 4)) / cityCentre)) + 0.125f;
        tempHeight = tempHeight * maxBuildingHeight;

        Vector3 tempVec = new Vector3(-normalZ, 0, normalX);
        Vector3 tempVec2 = new Vector3(normalZ, 0, -normalX);

        Quaternion buildingRot = node1.transform.rotation;
        Quaternion buildingRot2 = node1.transform.rotation * Quaternion.Euler(0, 180, 0);



        while (currentSpawnPos < Vector3.Distance(node1.transform.position, node2.transform.position))
        {
            int tempDecide = Random.Range(0, buildGen.Count);
            Vector3 oPos = buildingPos - node1.transform.position;
            if (buildGen[tempDecide].buildingBase[0].GetComponentInChildren<Renderer>().bounds.size.x > Vector3.Distance(node1.transform.position, node2.transform.position) - oPos.magnitude)
            {
                tempDecide = 0;
            }
            float width = buildGen[tempDecide].buildingBase[0].GetComponentInChildren<Renderer>().bounds.size.x;

            buildingPos += node1.transform.forward * (width / 2);

            Vector3 newPos = buildingPos + Vector3.Normalize(tempVec) * ((roadThickness + width + 0.5f) / 2);

            float buildingHeight = UnityEngine.Random.Range(tempHeight / 2, tempHeight + 1);
            if (staticHeight)
            {
                buildingHeight = maxBuildingHeight;
            }
            buildGen[tempDecide].Spawner(newPos, buildingRot, (int)buildingHeight, buildingParent, m_LayerMask, this);
            buildingPos += node1.transform.forward * (width / 2);
            currentSpawnPos += oPos.magnitude;
        }

        currentSpawnPos = 0;
        buildingPos = node1.transform.position;

        while (currentSpawnPos < Vector3.Distance(node1.transform.position, node2.transform.position))
        {
            int tempDecide = Random.Range(0, buildGen.Count);
            Vector3 oPos = buildingPos - node1.transform.position;
            if (buildGen[tempDecide].buildingBase[0].GetComponentInChildren<Renderer>().bounds.size.x > Vector3.Distance(node1.transform.position, node2.transform.position) - oPos.magnitude)
            {
                tempDecide = 0;
            }
            float width = buildGen[tempDecide].buildingBase[0].GetComponentInChildren<Renderer>().bounds.size.x;

            buildingPos += node1.transform.forward * (width / 2);
            Vector3 newPos2 = buildingPos + Vector3.Normalize(tempVec2) * ((roadThickness + width + 0.5f) / 2);     //calculate distance for position of building, including the roadthickness. ((roadThickness + 2) / 2
            float buildingHeight2 = UnityEngine.Random.Range(tempHeight / 2, tempHeight + 1);
            if (staticHeight)
            {
                buildingHeight2 = maxBuildingHeight;
            }
            buildGen[tempDecide].Spawner(newPos2, buildingRot2, (int)buildingHeight2, buildingParent, m_LayerMask, this);
            buildingPos += node1.transform.forward * (width / 2);
            currentSpawnPos += oPos.magnitude;
        }
        //Debug.Log("segment: " + Segments.Count);
        //StartCoroutine(LoadSegments(buildingParent));
    }
    public void StartLoadSegments()
    {
        StartCoroutine(LoadSegments(buildingParent));
    }

    public IEnumerator LoadSegments(GameObject buildingParent)
    {
        for (int i = 0; i < Segments.Count; i++)
        {
            if (Segments[i].buildHeight != 0 && spawnInstantHouses)
            {
                GameObject SpawnedSegment = Instantiate(Segments[i].obj);
                SpawnedSegment.layer = 7;
                SpawnedSegment.transform.parent = buildingParent.transform;
                SpawnedSegment.transform.position = Segments[i].pos;
                SpawnedSegment.transform.rotation = Segments[i].rot;
                Debug.Log(SpawnedSegment.GetComponentInChildren<Renderer>().bounds.extents);
                Collider[] hitColliders = Physics.OverlapBox(SpawnedSegment.transform.position, SpawnedSegment.GetComponentInChildren<Renderer>().bounds.size / 4, Quaternion.identity, m_LayerMask);
                if (hitColliders.Count() > 0)
                {
                    DestroyImmediate(SpawnedSegment);
                    i += Segments[i].buildHeight;
                }
                else
                {
                    for (int j = 1; j < Segments[i].buildHeight + 1; j++)
                    {
                        int k = i + j;
                        GameObject SpawnedSegmentK = Instantiate(Segments[k].obj);
                        SpawnedSegmentK.layer = 7;
                        SpawnedSegmentK.transform.parent = buildingParent.transform;
                        SpawnedSegmentK.transform.position = Segments[k].pos;
                        SpawnedSegmentK.transform.rotation = Segments[k].rot;
                    }
                }
            }

            if (!spawnInstantHouses)
            {
                GameObject SpawnedSegment = Instantiate(Segments[i].obj);
                SpawnedSegment.layer = 7;
                SpawnedSegment.transform.parent = buildingParent.transform;
                SpawnedSegment.transform.position = Segments[i].pos;
                SpawnedSegment.transform.rotation = Segments[i].rot;
                if (Segments[i].buildHeight != 0)
                {
                    Collider[] hitColliders = Physics.OverlapBox(SpawnedSegment.transform.position, SpawnedSegment.GetComponentInChildren<Renderer>().bounds.size / 4, Quaternion.identity, m_LayerMask);
                    if (hitColliders.Count() > 0)
                    {
                        DestroyImmediate(SpawnedSegment);
                        i += Segments[i].buildHeight;
                    }
                }
            }
            if (!spawnAllHouses)
            {
                yield return new WaitForEndOfFrame();
            }
        }
        yield return new WaitForEndOfFrame();

    }
    void Update()
    {

        if (Input.GetKeyDown(KeyCode.N))
        {
            DestroyBuildings();
        }

    }
    public void DestroyBuildings()
    {
        foreach (Transform children in buildingParent.transform)
        {
            DestroyImmediate(children.gameObject);
        }
        Segments.Clear();
    }
}

#region backup
/*public void BuildingDecider(Vector3 buildingPosition, Quaternion buildingRotation, int buildingHeight)
{
    int tempDecide = Random.Range(0, 2);
    switch (tempDecide)
    {
        case 0:
            break;
        case 1:
            break;
    }
}

void GenerateBuilding(Vector3 buildingPosition, Quaternion buildingRotation, int buildingHeight)
{
    GameObject baseBuilding = Instantiate(buildingBase[Random.Range(0, buildingBase.Count)], buildingPosition, buildingRotation);
    baseBuilding.transform.parent = buildingParent.transform;

    Collider[] hitColliders = Physics.OverlapBox(baseBuilding.transform.position, baseBuilding.GetComponentInChildren<Renderer>().bounds.extents / 2, Quaternion.identity, m_LayerMask);
    if (hitColliders.Count() != 1)
    {
        Destroy(baseBuilding);
        return;
    }
    buildingPosition.y += baseBuilding.GetComponentInChildren<Renderer>().bounds.max.y - buildingPosition.y;

    for (int i = 0; i <= buildingHeight; i++)
    {
        buildingRotation *= Quaternion.Euler(0, randomRotation(), 0);
        GameObject middleBuilding = Instantiate(buildingMiddle[Random.Range(0, buildingMiddle.Count)], buildingPosition, buildingRotation);
        middleBuilding.transform.parent = buildingParent.transform;
        buildingPosition.y += (middleBuilding.GetComponentInChildren<Renderer>().bounds.max.y - buildingPosition.y);
    }

    GameObject roofBuilding = Instantiate(buildingRoof[Random.Range(0, buildingRoof.Count)], buildingPosition, buildingRotation);
    roofBuilding.transform.parent = buildingParent.transform;

}

float randomRotation()
{
    int randomGen = Random.Range(0, 5);
    switch (randomGen)
    {
        case 0:
            return 90;
        case 1:
            return 180;
        case 2:
            return 270;
        case 3:
            return 0;
    }
    return 0;
}
void generateAttachments(GameObject segment, Vector3 segmentPosition)
{
    int attachmentNumber = Random.Range(-1, attachments.Count);
    if(attachmentNumber >= 0)
    {
        int number = Random.Range(0,6);
        for (int i = 0; i < number; i++)
        {
            int side = Random.Range(0, 5);
            setVariables(segment, attachmentNumber);

            switch (side)
            {
                case 1:     //front
                    Vector3 position1 = new Vector3(0, 0, 0);
                    position1.x = segmentPosition.x + borderMax.x;
                    position1.y = Random.Range(borderMin.y + attachmentMax.y, borderMax.y - attachmentMax.y);
                    position1.z = Random.Range(borderMin.z + attachmentMax.z, borderMax.z - attachmentMax.z);
                    GameObject fan1 = Instantiate(attachments[attachmentNumber], position1, Quaternion.Euler(0, 0, 0));
                    fan1.transform.localScale *= Random.Range(0.5f * fan1.transform.localScale.x, 1.5f * fan1.transform.localScale.x);
                    fan1.transform.parent = buildingParent.transform;
                    break;
                case 2:     //left
                    Vector3 position2 = new Vector3(0, 0, 0);
                    position2.x = Random.Range(borderMin.x + attachmentMax.x, borderMax.x - attachmentMax.x);
                    position2.y = Random.Range(borderMin.y + attachmentMax.y, borderMax.y - attachmentMax.y);
                    position2.z = segment.GetComponentInChildren<Renderer>().bounds.max.z;
                    GameObject fan2 = Instantiate(attachments[attachmentNumber], position2, Quaternion.Euler(0, 270, 0));
                    fan2.transform.localScale *= Random.Range(0.5f * fan2.transform.localScale.x, 1.5f * fan2.transform.localScale.x);
                    fan2.transform.parent = buildingParent.transform;
                    break;
                case 3:     //back
                    Vector3 position3 = new Vector3(0, 0, 0);
                    position3.x = segmentPosition.x - segment.GetComponentInChildren<Renderer>().bounds.max.x;
                    position3.y = Random.Range(borderMin.y + attachmentMax.y, borderMax.y - attachmentMax.y);
                    position3.z = Random.Range(borderMin.z + attachmentMax.z, borderMax.z - attachmentMax.z);
                    GameObject fan3 = Instantiate(attachments[attachmentNumber], position3, Quaternion.Euler(0, 180, 0));
                    fan3.transform.localScale *= Random.Range(0.5f * fan3.transform.localScale.x, 1.5f * fan3.transform.localScale.x);
                    fan3.transform.parent = buildingParent.transform;
                    break;
                case 4:     //right
                    Vector3 position4 = new Vector3(0, 0, 0);
                    position4.x = Random.Range(borderMin.x + attachmentMax.x, borderMax.x - attachmentMax.x);
                    position4.y = Random.Range(borderMin.y + attachmentMax.y, borderMax.y - attachmentMax.y);
                    position4.z = segmentPosition.z - segment.GetComponentInChildren<Renderer>().bounds.max.z;
                    position4.z += segmentPosition.z;
                    GameObject fan4 = Instantiate(attachments[attachmentNumber], position4, Quaternion.Euler(0, 90, 0));
                    fan4.transform.localScale *= Random.Range(0.5f * fan4.transform.localScale.x, 1.5f * fan4.transform.localScale.x);
                    fan4.transform.parent = buildingParent.transform;
                    break;
            }
        }
    }
}
void setVariables(GameObject segment, int attachmentNumber)
{

    borderMin.x = segment.GetComponentInChildren<Renderer>().bounds.min.x;
    borderMin.y = segment.GetComponentInChildren<Renderer>().bounds.min.y;
    borderMin.z = segment.GetComponentInChildren<Renderer>().bounds.min.z;

    borderMax.x = segment.GetComponentInChildren<Renderer>().bounds.max.x;
    borderMax.y = segment.GetComponentInChildren<Renderer>().bounds.max.y;
    borderMax.z = segment.GetComponentInChildren<Renderer>().bounds.max.z;

    attachmentMin.x = attachments[attachmentNumber].GetComponentInChildren<Renderer>().bounds.min.x;
    attachmentMin.y = attachments[attachmentNumber].GetComponentInChildren<Renderer>().bounds.min.y;
    attachmentMin.z = attachments[attachmentNumber].GetComponentInChildren<Renderer>().bounds.min.z;

    attachmentMax.x = attachments[attachmentNumber].GetComponentInChildren<Renderer>().bounds.max.x;
    attachmentMax.y = attachments[attachmentNumber].GetComponentInChildren<Renderer>().bounds.max.y;
    attachmentMax.z = attachments[attachmentNumber].GetComponentInChildren<Renderer>().bounds.max.z;
}

}



/*void generateBuilding()
{
Vector3 buildingPosition = new Vector3(0, 0, 3 * j);
GameObject baseBuilding = Instantiate(buildingBase[Random.Range(0, buildingBase.Count)], buildingPosition, Quaternion.identity);
baseBuilding.transform.parent = buildingParent.transform;
buildingPosition.y += baseBuilding.GetComponentInChildren<Renderer>().bounds.max.y;

int buildingHeight = Random.Range(0, houseHeight);
for (int i = 0; i <= buildingHeight; i++)
{
    GameObject middleBuilding = Instantiate(buildingMiddle[Random.Range(0, buildingMiddle.Count)], buildingPosition, Quaternion.identity);
    middleBuilding.transform.parent = buildingParent.transform;
    buildingPosition.y += (middleBuilding.GetComponentInChildren<Renderer>().bounds.max.y - buildingPosition.y);
    generateAttachments(middleBuilding, buildingPosition);

}

GameObject roofBuilding = Instantiate(buildingRoof[Random.Range(0, buildingRoof.Count)], buildingPosition, Quaternion.identity);
roofBuilding.transform.parent = buildingParent.transform;
}*/


/*public void spawnBuildings()
   {
       VertexPath path = pathCreator.path;

       spacing = Mathf.Max(minSpacing, spacing);
       float dst = startHousePadding;
       destroyBuildings();

       while (dst < path.length - endHousePadding)
       {
           int spawner = Random.Range(0, spawnChance);
           if (spawner != 0)
           {
               Vector3 point = path.GetPointAtDistance(dst);
               Quaternion rot = path.GetRotationAtDistance(dst);
               point.z += meshCreator.roadWidth + Random.Range(padding * 0.9f, padding * 1.1f);
               rot *= Quaternion.Euler(0, 0, 90);
               generateBuilding(point, rot);
           }
               dst += spacing;
       }
       float dst2 = startHousePadding;
       while (dst2 < path.length - endHousePadding)
       {
           int spawner = Random.Range(0, spawnChance);
           if (spawner != 0)
           {
               Vector3 point = path.GetPointAtDistance(dst2);
               Quaternion rot = path.GetRotationAtDistance(dst2);
               point.z -= meshCreator.roadWidth + Random.Range(padding, padding * 2f);
               rot *= Quaternion.Euler(180, 0, 90);
               generateBuilding(point, rot);
           }
               dst2 += spacing;
       }
   }*/

#endregion