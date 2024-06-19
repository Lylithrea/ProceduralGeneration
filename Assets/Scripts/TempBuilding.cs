using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempBuilding
{
    public GameObject obj { get; set; }
    public Vector3 pos { get; set; }
    public Quaternion rot { get; set; }
    public int buildHeight;

    public TempBuilding(GameObject obj, Vector3 pos, Quaternion rot, int buildHeight = 0) 
    {
        this.obj = obj;  
        this.pos = pos;  
        this.rot =  rot;
        this.buildHeight = buildHeight;
    }

}
