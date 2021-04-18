using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class ExistingSceneLoader : Loader
{
    public List<GameObject> parentsOfValidGeometry;
    private GameObject navAgent;
    private NavMeshSurface navMeshSurface;
    private NavMeshBuildSettings agentSettings;

    public override GameObject Load()
    {

        Bounds bounds = parentsOfValidGeometry[0].GetComponentsInChildren<Renderer>()[0].bounds;
        foreach (GameObject g in parentsOfValidGeometry)
        {
            OL_GLOBAL_INFO.setLayerOfAll(g, 8);
            foreach (Renderer renderer in g.GetComponentsInChildren<Renderer>())
            {
                bounds.Encapsulate(renderer.bounds);
            }
        }
        GameObject nmh = new GameObject("NavMesh Holder");
        navMeshSurface = nmh.AddComponent<NavMeshSurface>();
        navMeshSurface.layerMask = LayerMask.GetMask("NavMeshLayer");
        agentSettings = NavMesh.CreateSettings();
        agentSettings.agentHeight = 1.5f;
        agentSettings.agentRadius = 0.1f;
        //navMeshSurface.BuildNavMeshWithSettings(agentSettings);
        navAgent = OL_GLOBAL_INFO.AGENT;
        navAgent.GetComponent<NavMeshAgent>().agentTypeID = agentSettings.agentTypeID;
        navAgent.GetComponent<NavMeshAgent>().angularSpeed = 80.0f;

        List<(Vector3, Vector3)> bbl = new List<(Vector3, Vector3)>();
        (Vector3, Vector3) bb;
        bb.Item1 = bounds.min;
        bb.Item2 = bounds.max;
        bbl.Add(bb);
        OL_GLOBAL_INFO.BBOX_LIST = bbl;
        return new GameObject("toBeDeleted");
    }

}
