using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnitySUNCG;

public class SampleRoom : MonoBehaviour
{
    public House house;
    public GameObject houseObject;
    public string fullId;
    // Start is called before the first frame update
    void Start()
    {
        //string fullId = "35c7af9c459e7c96920f81ae1b16f3aa_0_0";
        //string fullId = "00a13f6179b2ed9a1bc10eef9d5b3bf4_0_1";
        houseObject = Scene.GetRoomHouseObjectById(fullId);
        HouseLoader houseLoader = houseObject.GetComponent<HouseLoader>();
        houseLoader.Load(true);
    }

    // Update is called once per framed
    void Update()
    {
        
    }
}
