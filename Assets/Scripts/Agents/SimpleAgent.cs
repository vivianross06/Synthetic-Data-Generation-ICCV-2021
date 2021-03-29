using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SimpleAgent : Agent
{
    private Screenshoter screenshot;
    private NavMeshAgent navMeshAgent;
    private List<List<Vector3>> regions = new List<List<Vector3>>();
    private Vector3 startPos;
    private float scStep;
    private Quaternion prevRotation;
    private Quaternion nextRotation;
    private float totalDistance = 1;
    public float elapsedTime;
    private float camTimer;


    // Update is called once per frame
    void Update()
    {
        elapsedTime += Time.deltaTime;
        camTimer += Time.deltaTime;
        if(elapsedTime > OL_GLOBAL_INFO.MAX_TIME_BETWEEN_POINTS)
		{
            elapsedTime = 0.0f;
            navMeshAgent.Warp(navMeshAgent.destination);
		}
        if ( navMeshAgent.enabled && navMeshAgent.remainingDistance < 0.2f) {
            elapsedTime = 0.0f;
            if (regions.Count > 0)
            {
                Vector3 v = getRandomPoint();
                if (navMeshAgent.enabled)
                {
                    navMeshAgent.SetDestination(v);
                    NavMeshPath path = new NavMeshPath();
                    NavMesh.CalculatePath(transform.position, v, NavMesh.AllAreas, path);
                    totalDistance = PathLength(path);
                }
            }
        }
        if (OL_GLOBAL_INFO.FLYTHROUGH_MODE == ModeEnum.Human)
        {
            float angleRatio = 1 - (Quaternion.Angle(prevRotation, nextRotation) / (OL_GLOBAL_INFO.MAX_ROTATION_X + OL_GLOBAL_INFO.MAX_ROTATION_Y));
            transform.GetChild(0).rotation = Quaternion.Slerp(prevRotation, nextRotation, camTimer * angleRatio / OL_GLOBAL_INFO.CAM_ROTATION_DURATION);
            Vector3 eulerRotation = transform.GetChild(0).eulerAngles;
            transform.GetChild(0).rotation = Quaternion.Euler(eulerRotation.x, eulerRotation.y, 0);
        }
        if (Vector3.Distance(transform.position, startPos) >= scStep)
		{
            startPos = transform.position;
            screenshot.CaptureScreenshot(Camera.main, Screen.width, Screen.height);
        }
    }

    public override void StartAgent(List<(Vector3, Vector3)> bboxlist) {
        scStep = OL_GLOBAL_INFO.DISTANCE_BETWEEN_SCREENSHOTS;
        int totalPoints = OL_GLOBAL_INFO.TOTAL_POINTS;
        screenshot = GetComponent<Screenshoter>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        //make regions
        NavMeshPath path = new NavMeshPath();
        List<Vector3> d = createRandomPoints(bboxlist, totalPoints);
        while (d.Count > 0)
		{
            Vector3 src = d[0];
            List<Vector3> region = new List<Vector3>();
            for (int i=0; i<d.Count; i++)
			{
                NavMesh.CalculatePath(src, d[i], NavMesh.AllAreas, path);
				if (path.status == NavMeshPathStatus.PathComplete || i==0)
                {
                    //Then d[0] and d[i] share the same region.
                    region.Add(d[i]);
                    d.RemoveAt(i);
                    i--;
				}
            }
            regions.Add(region);
		}
        transform.position = regions[0][0];
        startPos = transform.position;
        elapsedTime = 0.0f;
        navMeshAgent.enabled = true;
        gameObject.SetActive(true);
        StartCoroutine(SetCameraLookAngle());
    }

    public void ResetAgent(List<List<Vector3>> regions)
    {
        if (regions.Count > 0)
        {
            transform.position = regions[0][0];
            startPos = transform.position;
            navMeshAgent.enabled = true;
        }
        else
        {
            navMeshAgent.enabled = false;
        }
    }

    private Vector3 getRandomPoint()
	{
        /*
        int x = Random.Range(0, destLen);
        for (int i = 0; i < regions.Count; i++)
        {
            int l = regions[i].Count;
            if (x >= l)
            {
                x -= l;
            }
            else
            {
                return regions[i][x];
            }
        }
        return regions[0][0];
        */
        if (regions[0].Count > 0)
        {
            int x = Random.Range(0, regions[0].Count);
            Vector3 point = regions[0][x];
            regions[0].RemoveAt(x);
            return point;
        }
        else
        {
            navMeshAgent.enabled = false;
            regions.RemoveAt(0);
            ResetAgent(regions);
            return transform.position;
        }
    }

    private List<Vector3> createRandomPoints(List<(Vector3, Vector3)> bboxlist, int totalPoints)
    {
        List<Vector3> randomPoints = new List<Vector3>();
        int pointsPerLevel = totalPoints / bboxlist.Count;
        for (int l = 0; l < bboxlist.Count; l++)
        {
            //generate totalPoints random points on NavMesh!
            if (l == bboxlist.Count - 1)
            {
                pointsPerLevel += totalPoints - (pointsPerLevel * bboxlist.Count);
            }
            for (int i = 0; i < pointsPerLevel; i += 0)
            {
                float rx = UnityEngine.Random.Range(bboxlist[l].Item1.x, bboxlist[l].Item2.x);
                float ry = UnityEngine.Random.Range(bboxlist[l].Item1.y, bboxlist[l].Item2.y);
                float rz = UnityEngine.Random.Range(bboxlist[l].Item1.z, bboxlist[l].Item2.z);
                Vector3 randomPoint = new Vector3(rx, ry, rz);
                NavMeshHit hit;
                Vector3 result = new Vector3(0, 0, 0); //All this code is doing is finding a random point within the bounding box of the level we are looking at,
                                                       //and then finding the closest point on the NavMesh.
                if (NavMesh.SamplePosition(randomPoint, out hit, Vector3.Distance(bboxlist[l].Item1, bboxlist[l].Item2), NavMesh.AllAreas))
                {
                    result = hit.position;
                    randomPoints.Add(result);
                    i++;
                }
                else
                {
                    //Debug.Log("Point not found.");
                    //Debug.Log(randomPoint);
                }

            }
        }
        return randomPoints;
    }

    void OnDrawGizmos()
    {
        float radius = 0.1f;
        Gizmos.color = Color.red;
        Color[] colors = { Color.red, Color.yellow, Color.green, Color.cyan, Color.blue, Color.green, Color.magenta, Color.black };
        for (int i = 0; i < regions.Count; i++)
        {
            Gizmos.color = colors[i%8];
            foreach (Vector3 v in regions[i])
                Gizmos.DrawSphere(v, radius);
        }

    }

    private float PathLength(NavMeshPath path)
    {
        if (path.corners.Length < 2)
            return 0;

        Vector3 previousCorner = path.corners[0];
        float lengthSoFar = 0.0F;
        int i = 1;
        while (i < path.corners.Length)
        {
            Vector3 currentCorner = path.corners[i];
            lengthSoFar += Vector3.Distance(previousCorner, currentCorner);
            previousCorner = currentCorner;
            i++;
        }
        return lengthSoFar;
    }

    IEnumerator SetCameraLookAngle()
    {
        for (; ; )
        {
            camTimer = 0.0f;
            prevRotation = transform.GetChild(0).rotation;
            nextRotation = Quaternion.Euler(Random.Range(-OL_GLOBAL_INFO.MAX_ROTATION_X, OL_GLOBAL_INFO.MAX_ROTATION_X), Random.Range(-OL_GLOBAL_INFO.MAX_ROTATION_Y, OL_GLOBAL_INFO.MAX_ROTATION_Y), 0);
            yield return new WaitForSeconds(OL_GLOBAL_INFO.CAM_ROTATION_FREQUENCY);
        }
    }
}
