using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Security.Cryptography;
using UnityEngine;

/*[System.Serializable] public struct Node {
    public Vector3 position;
    public Quaternion rotation;
    public float time;

    public Node(Vector3 p, Quaternion r, float t)
    {
        this.position = p;
        this.rotation = r;
        this.time = t;
    }
    
}*/


public class CameraScript : MonoBehaviour
{
    public GameObject ListHead;
    private List<(Transform, float)> Nodes = new List<(Transform, float)>();
    private int currentNode = 0;
    private float timeElapsed = 0.0f;
    public bool repeat = false;
    // Start is called before the first frame update
    void Start()
    {
        Transform startPos = new GameObject("StartPos").transform;
        startPos.position = transform.position;
        startPos.rotation = transform.rotation;
        Nodes.Add((startPos, 1));
        for (int i = 0; i < ListHead.transform.childCount; i++)
        {
            //Nodes.Add((ListHead.transform.GetChild(i), ListHead.transform.GetChild(i).gameObject.GetComponent<TimeToReach>().time));
        }

    }

    // Update is called once per frame
    void Update()
    {
        timeElapsed += Time.deltaTime;
        if (currentNode + 1 < Nodes.Count)
        {
            float targetTime = Nodes[currentNode + 1].Item2;
            float ratioDone = Mathf.Clamp((timeElapsed / targetTime), 0, 1);
            transform.position = Vector3.Lerp(Nodes[currentNode].Item1.position, Nodes[currentNode + 1].Item1.position, ratioDone);
            transform.rotation = Quaternion.Slerp(Nodes[currentNode].Item1.rotation, Nodes[currentNode + 1].Item1.rotation, ratioDone);
            if (ratioDone == 1)
            {
                timeElapsed = 0;
                currentNode++;
            }
        }
        else if (repeat)
        {
            timeElapsed = 0;
            currentNode = 0;
            Nodes[0].Item1.position = transform.position;
            Nodes[0].Item1.rotation = transform.rotation;
            //Remove the lines below this comment if you're not going to change time values in inspector.
            Nodes.RemoveRange(1, Nodes.Count - 1);
            for (int i = 0; i < ListHead.transform.childCount; i++)
            {
                //Nodes.Add((ListHead.transform.GetChild(i), ListHead.transform.GetChild(i).gameObject.GetComponent<TimeToReach>().time));
            }
        }
    }
}
