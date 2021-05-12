using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SimpleAgent : Agent
{
    private Screenshoter screenshot;
    private NavMeshAgent navMeshAgent;
    private List<List<Vector3>> regions = new List<List<Vector3>>();
    private List<Vector3> corners = new List<Vector3>();
    private double cornerDistance;
    private double distanceTraveled;
    private Vector3 movement;
    private Vector3 startPos;
    private float scStep;
    private Quaternion prevRotation;
    private Quaternion nextRotation;
    private float totalDistance = 1;
    public float elapsedTime;
    private float camTimer;
    private float maxAngle;
    public int REPORT = 0;

    // Update is called once per frame
    void Update()
    {
        if (agentDone == true)
        {
            return;
        }
        REPORT = regions.Count;
        elapsedTime += Time.deltaTime;
        camTimer += Time.deltaTime;
        if(elapsedTime > OL_GLOBAL_INFO.MAX_TIME_BETWEEN_POINTS)
		{
            elapsedTime = 0.0f;
            if(navMeshAgent.destination != Vector3.positiveInfinity)
                navMeshAgent.Warp(navMeshAgent.destination);
            else
			{
                screenshot.ResetCounter();
                agentDone = true;
			}
		}
        //if ( navMeshAgent.enabled && navMeshAgent.remainingDistance < 0.2f) {
        if (corners.Count == 0 || corners.Count == 1) {
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
                    generateCorners(path);
                    /*
                    Debug.Log("position: " + transform.position);
                    Debug.Log("first corner: " + corners[0]);
                    Debug.Log("last corner: " + corners[corners.Count - 1]);
                    Debug.Log("v: " + v);
                    */
                }
            }
        }

        if (transform.position == corners[0])
        {
            movement = interpolateCorners(corners);
            interpolateCornerRotations(movement);
            distanceTraveled = 0;
            //transform.GetChild(0).LookAt(corners[0]);
            transform.LookAt(corners[0]);
        }

        float angleRatio = (float)(distanceTraveled/cornerDistance);
        Quaternion q = Quaternion.Slerp(prevRotation, nextRotation, angleRatio);
        if (isNaN(q))
            transform.GetChild(0).localRotation = nextRotation;
        else
            transform.GetChild(0).localRotation = q;
        Vector3 eulerRotation = transform.GetChild(0).localEulerAngles;
        transform.GetChild(0).localRotation = Quaternion.Euler(eulerRotation.x, eulerRotation.y, 0);
        //transform.position = navMeshAgent.nextPosition;
        transform.position = transform.position + movement;
        distanceTraveled = distanceTraveled + scStep;
        if (distanceTraveled >= cornerDistance)
        {
            transform.position = corners[0];
        }
        screenshot.CaptureScreenshot(Camera.main, OL_GLOBAL_INFO.SCREENSHOT_WIDTH, OL_GLOBAL_INFO.SCREENSHOT_HEIGHT);
        //Debug.Log("screenshot position: " + transform.position.x + " " + transform.position.y + " " +transform.position.z);
        /*
        if (Vector3.Distance(transform.position, startPos) >= scStep)
		{
            startPos = transform.position;
            screenshot.CaptureScreenshot(Camera.main, Screen.width, Screen.height);
        }
        */
    }

    public override void StartAgent(List<(Vector3, Vector3)> bboxlist) {
        agentDone = false;
        scStep = OL_GLOBAL_INFO.DISTANCE_BETWEEN_SCREENSHOTS;
        int totalPoints = OL_GLOBAL_INFO.TOTAL_POINTS;
        screenshot = GetComponent<Screenshoter>();
        navMeshAgent = GetComponent<NavMeshAgent>();

        navMeshAgent.updatePosition = false;
        navMeshAgent.updateRotation = false;
        //make regions
        NavMeshPath path = new NavMeshPath();
        List<Vector3> d = createRandomPoints(bboxlist, totalPoints);
        regions.Clear();
        while (d.Count > 0)
		{
            Vector3 src = d[0];
            List<Vector3> region = new List<Vector3>();
            for (int i=0; i<d.Count; i++)
			{
                NavMesh.CalculatePath(src, d[i], NavMesh.AllAreas, path);
				if (path.status == NavMeshPathStatus.PathComplete || region.Count == 0)
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
        maxAngle = CalculateMaxAngleRatio();
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
            screenshot.ResetCounter();
            agentDone = true;
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
        if (OL_GLOBAL_INFO.SEED == true)
        {
            Random.InitState(5);
        }
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

    public float CalculateMaxAngleRatio()
    {

        //Code to compute max angle rotation neeed for angleRatio
        Quaternion Q1 = Quaternion.Euler(OL_GLOBAL_INFO.MIN_ROTATION_X, OL_GLOBAL_INFO.MIN_ROTATION_Y, 0);
        Quaternion Q2 = Quaternion.Euler(OL_GLOBAL_INFO.MIN_ROTATION_X, OL_GLOBAL_INFO.MAX_ROTATION_Y, 0);
        Quaternion Q3 = Quaternion.Euler(OL_GLOBAL_INFO.MAX_ROTATION_X, OL_GLOBAL_INFO.MIN_ROTATION_Y, 0);
        Quaternion Q4 = Quaternion.Euler(OL_GLOBAL_INFO.MAX_ROTATION_X, OL_GLOBAL_INFO.MAX_ROTATION_Y, 0);
        float[] a = new float[10];
        a[0] = Quaternion.Angle(Q1, Q1);
        a[1] = Quaternion.Angle(Q1, Q2);
        a[2] = Quaternion.Angle(Q1, Q3);
        a[3] = Quaternion.Angle(Q1, Q4);
        a[4] = Quaternion.Angle(Q2, Q2);
        a[5] = Quaternion.Angle(Q2, Q3);
        a[6] = Quaternion.Angle(Q2, Q4);
        a[7] = Quaternion.Angle(Q3, Q3);
        a[8] = Quaternion.Angle(Q3, Q4);
        a[9] = Quaternion.Angle(Q4, Q4);

        float maxAngle = a[0];

        for (int i = 1; i < 10; i++)
        {

            if (a[i] > maxAngle)
                maxAngle = a[i];
        }

        return maxAngle;
    }

    private bool isNaN(Quaternion myQuaternion)
	{
        return (System.Single.IsNaN(myQuaternion.x) || System.Single.IsNaN(myQuaternion.y) || System.Single.IsNaN(myQuaternion.z) || System.Single.IsNaN(myQuaternion.w));
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
            prevRotation = transform.GetChild(0).localRotation;
            nextRotation = Quaternion.Euler(Random.Range(OL_GLOBAL_INFO.MIN_ROTATION_X, OL_GLOBAL_INFO.MAX_ROTATION_X), Random.Range(OL_GLOBAL_INFO.MIN_ROTATION_Y, OL_GLOBAL_INFO.MAX_ROTATION_Y), 0);
            //yield return new WaitForSeconds(OL_GLOBAL_INFO.CAM_ROTATION_FREQUENCY);
            yield return 6;
        }
    }

    void OnDrawGizmos()
    {
        float radius = 0.1f;
        Gizmos.color = Color.red;
        Color[] colors = { Color.red, Color.yellow, Color.green, Color.cyan, Color.blue, Color.green, Color.magenta, Color.black };
        for (int i = 0; i < regions.Count; i++)
        {
            Gizmos.color = colors[i % 8];
            foreach (Vector3 v in regions[i])
                Gizmos.DrawSphere(v, radius);
            Gizmos.color = Color.gray;
            if(navMeshAgent.destination != Vector3.positiveInfinity && navMeshAgent.destination != Vector3.negativeInfinity)
                Gizmos.DrawSphere(navMeshAgent.destination, radius+0.01f);
        }

    }

    void generateCorners(NavMeshPath path)
    {
        corners = new List<Vector3>();
        for (int i=0; i<path.corners.Length; i++)
        {
            corners.Add(new Vector3(path.corners[i].x, path.corners[i].y, path.corners[i].z));
        }
    }

    Vector3 interpolateCorners(List<Vector3> corners)
    {
        if (corners.Count < 2)
        {
            return new Vector3(0, 0, 0);
        }
        else
        {
            Vector3 difference = corners[1] - corners[0];
            cornerDistance = difference.magnitude;
            Vector3 normalized = Vector3.Normalize(difference);
            corners.RemoveAt(0);
            return normalized * scStep;
        }
    }

    void interpolateCornerRotations(Vector3 movement)
    {
        Quaternion nextRotation = Quaternion.LookRotation(movement, Vector3.up);
        int count = 0;
        while (!(transform.GetChild(0).rotation == nextRotation))
        {
            transform.GetChild(0).rotation = Quaternion.RotateTowards(transform.GetChild(0).rotation, nextRotation, 5);
            screenshot.CaptureScreenshot(Camera.main, OL_GLOBAL_INFO.SCREENSHOT_WIDTH, OL_GLOBAL_INFO.SCREENSHOT_HEIGHT);
            count++;
            if (count > 90)
            {
                break;
            }
        }
    }

}
