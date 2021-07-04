using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEditor;

public enum FormatEnum { RGB, DepthMap };
public enum LoadModeEnum { CompleteDirectory, PartialDirectory, SingleScene };

[System.Serializable]
public struct ScreenShotType
{
    public Shader shader;
    public string directoryName;
    public FormatEnum formatType;
}

public class OmniLoader : MonoBehaviour
{
    [HideInInspector] public MonoScript AgentScript;
    [HideInInspector] public MonoScript LoaderScript; //Loader is responsible for NavMesh generation
    [HideInInspector] public LoadModeEnum loadMode = LoadModeEnum.CompleteDirectory;
    [HideInInspector] public string loadOption = "";
    [HideInInspector] public int rotationDegrees = 5;
    [HideInInspector] public MonoScript ScreenshotScript;
    [HideInInspector] public List<ScreenShotType> scs = new List<ScreenShotType>(0);
    [HideInInspector] public int screenshotWidth = 320;
    [HideInInspector] public int screenshotHeight = 256;
    [HideInInspector] public uint agentWaypoints = 40;
    [HideInInspector] public float stepDistance = 1.0f;
    [HideInInspector] public Vector2 horizontalAngleRange = Vector2.zero;
    [HideInInspector] public Vector2 verticalAngleRange = Vector2.zero;
    [HideInInspector] public Vector2 parallaxAngle = Vector2.zero;
    [HideInInspector] public bool seedFlythroughs = false;
    [HideInInspector] public string flythroughName = "";
    [HideInInspector] public float agentHeight = 1.5f;
    private List<string> sceneIDs;
    private GameObject currentScene;
    private Loader sceneLoader;
    private Agent sceneAgent;
    private NavMeshAgent nma;
    private Screenshoter screenshotRef;

    // Start is called before the first frame update
    void Start()
    {
        GameObject agentObj = new GameObject("Agent");
        agentObj.SetActive(false);
        OL_GLOBAL_INFO.AGENT = agentObj;
        OL_GLOBAL_INFO.ROTATION_INCREMENT_DEGREES = rotationDegrees;
        OL_GLOBAL_INFO.SCREENSHOT_PROPERTIES = scs;
        OL_GLOBAL_INFO.SCREENSHOT_WIDTH = screenshotWidth;
        OL_GLOBAL_INFO.SCREENSHOT_HEIGHT = screenshotHeight;
        OL_GLOBAL_INFO.TOTAL_POINTS = Convert.ToInt32(agentWaypoints);
        OL_GLOBAL_INFO.DISTANCE_BETWEEN_SCREENSHOTS = stepDistance;
        /*OL_GLOBAL_INFO.MIN_ROTATION_Y = horizontalAngleRange[0];
        OL_GLOBAL_INFO.MAX_ROTATION_Y = horizontalAngleRange[1];
        OL_GLOBAL_INFO.MIN_ROTATION_X = verticalAngleRange[0];
        OL_GLOBAL_INFO.MAX_ROTATION_X = verticalAngleRange[1];*/
        OL_GLOBAL_INFO.PARALLAX_ANGLE = parallaxAngle;
        OL_GLOBAL_INFO.SEED = seedFlythroughs;
        OL_GLOBAL_INFO.FTNAME = flythroughName;

        if (AgentScript != null)
            agentObj.AddComponent(AgentScript.GetClass());
        if (ScreenshotScript != null)
            screenshotRef = (Screenshoter)(agentObj.AddComponent(ScreenshotScript.GetClass()));
        nma = agentObj.AddComponent<NavMeshAgent>();
        nma.enabled = false;
        //modify desired values of NavMeshAgent component here.
        //example: nma.speed = 3.5f;
        nma.speed = 0.9f;
        nma.acceleration = 1.2f;
        if (AgentScript != null)
        {
            GameObject camera = Camera.main.gameObject;
            camera.transform.SetParent(agentObj.transform);
            camera.transform.localPosition = new Vector3(0, agentHeight, 0);
            camera.transform.localRotation = Quaternion.identity;
        }

        sceneIDs = generateIDs();

        if (LoaderScript != null)
        {
            sceneLoader = (Loader)GetComponent(LoaderScript.GetClass());
            OL_GLOBAL_INFO.SCENE_NAME = sceneIDs[0];
            sceneLoader.SetNextScene(sceneIDs[0]);
            sceneIDs.RemoveAt(0);
            currentScene = sceneLoader.Load();
        }
        if (AgentScript != null)
        {
            sceneAgent = (Agent)agentObj.GetComponent(AgentScript.GetClass());
            sceneAgent.StartAgent(OL_GLOBAL_INFO.BBOX_LIST);
        }

    }

    // Update is called once per frame
    void Update()
    {
        if (loadMode != LoadModeEnum.SingleScene && sceneIDs.Count > 0)
        {
            bool isDone = false;
            if (AgentScript != null)
            {
                isDone = sceneAgent.agentDone;
            }

            if (isDone)
            {
                OL_GLOBAL_INFO.SCENE_NAME = sceneIDs[0];
                sceneLoader.SetNextScene(sceneIDs[0]);
                sceneIDs.RemoveAt(0);
                foreach (Renderer rend in currentScene.GetComponentsInChildren<Renderer>())
                {
                    Texture.DestroyImmediate(rend.material.mainTexture, true);
                }
                DestroyImmediate(currentScene);
                EditorUtility.UnloadUnusedAssetsImmediate(true);
                currentScene = sceneLoader.Load();
                screenshotRef.ResetCounter();
                sceneAgent.StartAgent(OL_GLOBAL_INFO.BBOX_LIST);
            }

        }
        if (sceneAgent.agentDone == true)
        {
            foreach (Renderer rend in currentScene.GetComponentsInChildren<Renderer>())
            {
                Texture.DestroyImmediate(rend.material.mainTexture, true);
            }
            DestroyImmediate(currentScene);
            EditorUtility.UnloadUnusedAssetsImmediate(true);
            Debug.Log("quitting");
            UnityEditor.EditorApplication.isPlaying = false;
        }
    }

    private List<string> generateIDs()
    {
        List<string> ids = new List<string>();
        switch (loadMode)
        {
            case LoadModeEnum.CompleteDirectory:
                string[] dir = Directory.GetDirectories(((Loader)GetComponent(LoaderScript.GetClass())).GetDatasetDirectory());
                for (int i = 0; i < dir.Length; i++)
                {
                    string[] folders = dir[i].Split('/');
                    ids.Add(folders[folders.Length - 1]);
                }
                ids.Sort();
                break;
            case LoadModeEnum.PartialDirectory:
                ids = new List<string>(File.ReadAllLines(loadOption));
                break;
            case LoadModeEnum.SingleScene:
                ids.Add(loadOption);
                break;
        }
        return ids;
    }
}

public class Loader : MonoBehaviour
{
    public virtual GameObject Load()
    { return new GameObject("default"); }

    public virtual void SetNextScene(string sceneID)
    { }

    public virtual string GetDatasetDirectory()
    { return ""; }

    public virtual void GenerateIDs()
    { }
}

public class Agent : MonoBehaviour
{
    public bool agentDone;

    public virtual void StartAgent(List<(Vector3, Vector3)> bboxlist)
    { }
}

public class Screenshoter : MonoBehaviour
{
    public virtual void CaptureScreenshot(Camera cam, int width, int height)
    { }
    public virtual void ResetCounter()
    { }
}

public static class OL_GLOBAL_INFO
{
    public static GameObject AGENT;
    public static string DATASET;
    public static string SCENE_ID;
    public static List<ScreenShotType> SCREENSHOT_PROPERTIES;
    public static int SCREENSHOT_WIDTH = 320;
    public static int SCREENSHOT_HEIGHT = 256;
    public static string SCREENSHOT_FILENAME = "";
    public static int TOTAL_POINTS = 40;
    public static float DISTANCE_BETWEEN_SCREENSHOTS = 0.1f;
    public static float MAX_TIME_BETWEEN_POINTS = 60.0f;
    public static float MIN_ROTATION_X = 0f;
    public static float MAX_ROTATION_X = 0f;
    public static float MIN_ROTATION_Y = 0f;
    public static float MAX_ROTATION_Y = 0f;
    public static Vector2 PARALLAX_ANGLE = Vector2.zero;
    public static float CAM_ROTATION_DURATION = 0.5f;
    public static float CAM_ROTATION_FREQUENCY = 0.5f;
    public static List<(Vector3, Vector3)> BBOX_LIST;
    public static string SCENE_NAME;
    public static bool SEED;
    public static string FTNAME;
    public static int ROTATION_INCREMENT_DEGREES = 5;

    public static void setLayerOfAll(GameObject root, int layer)
    {
        root.layer = layer;
        foreach (Transform child in root.transform)
        {
            setLayerOfAll(child.gameObject, layer);
        }
    }
}
