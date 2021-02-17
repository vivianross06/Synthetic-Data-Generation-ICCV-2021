using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace UnitySUNCG
{
    [System.Serializable]
    public class House
    {
        public string id;
        public float[] front;
        public float[] up;
        public float scaleToMeters;
        public Level[] levels;
    }

    [System.Serializable]
    public class Level
    {
        public string id;
        public BBox bbox;
        public Node[] nodes;
        public List<Node> rooms;

    }

    [System.Serializable]
    public class Node
    {
        public string id;
        public string type;
        public bool valid;
        public string modelId;
        public Material[] materials;
        public float[] dimensions;
        public float[] transform;
        public float isMirrored;
        public string[] roomTypes;
        public bool hideCeiling;
        public bool hideFloor;
        public bool hideWalls;
        public int[] nodeIndices;
        public int state;
        public string roomId;
    }

    [System.Serializable]
    public class BBox
    {
        public float[] min;
        public float[] max;
    }

    [System.Serializable]
    public class Material
    {
        public string name;
        public string texture;
        public string diffuse;
    }
}
