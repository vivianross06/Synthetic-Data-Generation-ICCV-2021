using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SimpleAgent : Agent
{
    public Screenshoter screenshot;
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
    private bool isRotating = false;
    // Update is called once per frame
    void Update()
    {
        if (!isRotating)
        {
            if (agentDone == true)
            {
                return;
            }
            elapsedTime += Time.deltaTime;
            camTimer += Time.deltaTime;
            if (elapsedTime > OL_GLOBAL_INFO.MAX_TIME_BETWEEN_POINTS)
            {
                elapsedTime = 0.0f;
                if (navMeshAgent.destination != Vector3.positiveInfinity)
                    navMeshAgent.Warp(navMeshAgent.destination);
                else
                {
                    agentDone = true;
                }
            }
            if (corners.Count == 0 || corners.Count == 1)
            {
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
                isRotating = true;
                navMeshAgent.enabled = false;
                StartCoroutine(interpolateCornerRotations(movement));
                distanceTraveled = 0;
            }
            else
			{
                Vector3 eulerRotation = Camera.main.gameObject.transform.parent.localEulerAngles;
                Camera.main.gameObject.transform.parent.localRotation = Quaternion.Euler(eulerRotation.x, eulerRotation.y, 0);
                transform.position = transform.position + movement;
                navMeshAgent.nextPosition = transform.position;
                distanceTraveled = distanceTraveled + scStep;
                if (distanceTraveled >= cornerDistance)
                {
                    transform.position = corners[0];
                }
                screenshot.CaptureScreenshot(Camera.main, OL_GLOBAL_INFO.SCREENSHOT_WIDTH, OL_GLOBAL_INFO.SCREENSHOT_HEIGHT);
            }
            /*float angleRatio = (float)(distanceTraveled / cornerDistance);
            Quaternion q = Quaternion.Slerp(prevRotation, nextRotation, angleRatio);
            if (isNaN(q))
            {
                Camera.main.gameObject.transform.parent.localRotation = nextRotation;

            }
            else
			{
                Camera.main.gameObject.transform.parent.localRotation = q;
            }*/

        }
    }

    public override void StartAgent(List<(Vector3, Vector3)> bboxlist)
    {
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
            for (int i = 0; i < d.Count; i++)
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
        gameObject.SetActive(true);
        Vector3 localPos = Camera.main.gameObject.transform.localPosition;
        transform.position = regions[0][0];
        startPos = transform.position;
        elapsedTime = 0.0f;
        navMeshAgent.enabled = true;
	    GameObject prev = GameObject.Find("rotFix");
        if (prev != null)
            localPos = prev.transform.localPosition;
        GameObject rotFix = new GameObject("rotFix");
        rotFix.transform.parent = this.transform;
        Camera.main.gameObject.transform.parent = rotFix.transform;
        rotFix.transform.position = transform.position;
        rotFix.transform.localPosition = localPos;
        Camera.main.gameObject.transform.localPosition = Vector3.zero;
        Camera.main.transform.localEulerAngles = new Vector3(OL_GLOBAL_INFO.PARALLAX_ANGLE[1], OL_GLOBAL_INFO.PARALLAX_ANGLE[0], 0);
	Destroy(prev);

        //StartCoroutine(SetCameraLookAngle());

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
            agentDone = true;
            navMeshAgent.enabled = false;
        }
    }

    private Vector3 getRandomPoint()
    {
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
                    //Point Not Found
                }

            }
        }
        return randomPoints;
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
        //Unused, randomness is not good.
        for (; ; )
        {
            camTimer = 0.0f;
            prevRotation = Camera.main.gameObject.transform.parent.localRotation;
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
            if (navMeshAgent.destination != Vector3.positiveInfinity && navMeshAgent.destination != Vector3.negativeInfinity)
                Gizmos.DrawSphere(navMeshAgent.destination, radius + 0.01f);
        }

    }

    void generateCorners(NavMeshPath path)
    {
        corners = new List<Vector3>();
        for (int i = 0; i < path.corners.Length; i++)
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

    IEnumerator interpolateCornerRotations(Vector3 movement)
    {
        Quaternion nextRotation = (Vector3.Cross(movement, Vector3.up) == Vector3.zero ? Camera.main.gameObject.transform.parent.rotation : Quaternion.LookRotation(movement, Vector3.up));
        while (Quaternion.Angle(Camera.main.gameObject.transform.parent.rotation, nextRotation) > 0.5)
        {
            isRotating = true;
            Quaternion q = Quaternion.RotateTowards(Camera.main.gameObject.transform.parent.rotation, nextRotation, OL_GLOBAL_INFO.ROTATION_INCREMENT_DEGREES);
            q.eulerAngles = new Vector3(q.eulerAngles.x, q.eulerAngles.y, 0);
            Camera.main.gameObject.transform.parent.rotation = q;
            screenshot.CaptureScreenshot(Camera.main, OL_GLOBAL_INFO.SCREENSHOT_WIDTH, OL_GLOBAL_INFO.SCREENSHOT_HEIGHT);
            yield return 0;
        }
        navMeshAgent.enabled = true;
        isRotating = false;
    }

}
