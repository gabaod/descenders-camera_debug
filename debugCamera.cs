using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using ModTool.Interface;

public class debugCamera : ModBehaviour
{
    private bool showDebugWindow = false;
    private Rect windowRect = new Rect(20, 20, 800, 1000);
    private Vector2 scrollPosition = Vector2.zero;
    
    // Cached data
    private List<Camera> activeCameras = new List<Camera>();
    private Camera mainCam = null;
    private List<GameObject> allGameObjects = new List<GameObject>();
    private string debugInfo = "";

    void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
        Debug.Log("[CameraDebugMod] Initialized - Press F9 for camera info");
    }
    
    void Start()
    {
        // Initial info gather after Descenders loads
        Invoke("UpdateCameraAndPlayerInfo", 2f);
    }

    void Update()
    {
        // Toggle debug window with F9
        if (Input.GetKeyDown(KeyCode.F9))
        {
            showDebugWindow = !showDebugWindow;
            Debug.Log("[CameraDebugMod] Debug window toggled: " + showDebugWindow);
        }
    }

    void UpdateCameraAndPlayerInfo()
    {
        // Find all cameras
        activeCameras.Clear();
        Camera[] allCameras = FindObjectsOfType<Camera>();
        foreach (Camera cam in allCameras)
        {
            if (cam != null)
            {
                activeCameras.Add(cam);
            }
        }
        
        // Get main camera
        mainCam = Camera.main;

        // Find all GameObjects
        allGameObjects.Clear();
        GameObject[] foundObjects = FindObjectsOfType<GameObject>();
        foreach (GameObject obj in foundObjects)
        {
            if (obj != null)
            {
                allGameObjects.Add(obj);
            }
        }

        // Build debug info string
        BuildDebugInfo();
    }
    
    string GetGameObjectPath(GameObject obj)
    {
        if (obj == null) return "NULL";
        
        string path = obj.name;
        Transform current = obj.transform.parent;
        
        while (current != null)
        {
            path = current.name + "/" + path;
            current = current.parent;
        }
        
        return path;
    }

    void BuildDebugInfo()
    {
        StringBuilder sb = new StringBuilder();
        
        sb.AppendLine("=== DESCENDERS CAMERA DEBUG ===");
        sb.AppendLine("Total Cameras Found: " + activeCameras.Count);
        sb.AppendLine("Total GameObjects Found: " + allGameObjects.Count);
        sb.AppendLine("Time: " + Time.time);
        sb.AppendLine();
        
        if (mainCam != null)
        {
            sb.AppendLine(">>> MAIN CAMERA (Camera.main) <<<");
            sb.AppendLine("GameObject: " + mainCam.gameObject.name);
            sb.AppendLine("Full Path: " + GetGameObjectPath(mainCam.gameObject));
            sb.AppendLine("Tag: " + mainCam.tag);
            sb.AppendLine();
        }
        else
        {
            sb.AppendLine(">>> NO Camera.main FOUND <<<");
            sb.AppendLine();
        }

        sb.AppendLine("=== ALL CAMERAS ===");
        for (int i = 0; i < activeCameras.Count; i++)
        {
            Camera cam = activeCameras[i];
            sb.AppendLine("Camera " + (i + 1) + ":");
            sb.AppendLine("  Name: " + cam.gameObject.name);
            sb.AppendLine("  Path: " + GetGameObjectPath(cam.gameObject));
            sb.AppendLine("  Tag: " + cam.tag);
            sb.AppendLine("  Depth: " + cam.depth);
            sb.AppendLine("  Enabled: " + cam.enabled);
            sb.AppendLine("  Position: " + cam.transform.position);
            
            if (cam.transform.parent != null)
            {
                sb.AppendLine("  Parent: " + cam.transform.parent.name);
            }
            
            if (Camera.main == cam)
            {
                sb.AppendLine("  >> THIS IS MAIN CAMERA <<");
            }
            
            sb.AppendLine();
        }

        sb.AppendLine("=== ALL GAMEOBJECTS ===");
        sb.AppendLine("(Showing root objects and their children)");
        sb.AppendLine();
        
        // Group by root objects to make it more readable
        Dictionary<string, int> rootCounts = new Dictionary<string, int>();
        
        for (int i = 0; i < allGameObjects.Count; i++)
        {
            GameObject obj = allGameObjects[i];
            string path = GetGameObjectPath(obj);
            sb.AppendLine(i + ": " + path);
            sb.AppendLine("    Active: " + obj.activeInHierarchy + " | Tag: " + obj.tag);
            
            // Show if it has a Camera component
            if (obj.GetComponent<Camera>() != null)
            {
                sb.AppendLine("    >> HAS CAMERA COMPONENT <<");
            }
            
            // Count root objects
            string rootName = obj.transform.root.name;
            if (rootCounts.ContainsKey(rootName))
            {
                rootCounts[rootName]++;
            }
            else
            {
                rootCounts[rootName] = 1;
            }
        }
        
        sb.AppendLine();
        sb.AppendLine("=== ROOT OBJECT SUMMARY ===");
        foreach (KeyValuePair<string, int> kvp in rootCounts)
        {
            sb.AppendLine(kvp.Key + ": " + kvp.Value + " objects");
        }

        debugInfo = sb.ToString();
        
        // Log to output_log.txt
        Debug.Log(debugInfo);
    }

    void OnGUI()
    {
        if (showDebugWindow)
        {
            windowRect = GUI.Window(99999, windowRect, DrawDebugWindow, "Camera & GameObject Debug");
        }
    }

    void DrawDebugWindow(int windowID)
    {
        GUILayout.BeginVertical();
        
        GUILayout.Label("F9 to close | Manual refresh only");
        
        if (GUILayout.Button("Refresh Now"))
        {
            UpdateCameraAndPlayerInfo();
            Debug.Log("[CameraDebugMod] Manual refresh triggered");
        }
        
        GUILayout.Space(10);
        
        scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Height(900));
        GUILayout.TextArea(debugInfo, GUILayout.ExpandHeight(true));
        GUILayout.EndScrollView();
        
        GUILayout.EndVertical();
        
        GUI.DragWindow();
    }
}
