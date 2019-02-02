using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEditor;
using UnityEngine.AI;
using UnityEngine.Experimental.AI;
using UnityEngine.Events;

public enum PointMovement {Y, XZ, XYZ}

[CanEditMultipleObjects]
[CustomEditor(typeof(AI_Patrol) )]
public class AI_PatrolEditor : Editor
    {
    //Array data for easier reference
    private PointData[] points;
    private Vector3[] areaBounds;

    private Vector3[] pathPoints;

    private bool calculatePath; //For generating a linear path using the nav mesh 

    //Settings
    private PatrolMode patrolMode;
    private string[] patrolOptions;
    public int patrolOptionsIndex;

    private PointMovement pointMovement;
    private string[] pointMoveOptions;
    public int pointOptionsIndex;

    //Color
    private Color pointColour;
    private Color pathColour;
    private Color areaColour;
    private Color goalColour;

    //Resizing
    private bool isScaleCameraRelative;
    private float pointSize;
    private float pathThickness;

    NavMeshQuery NavMeshQuery;
    NavMeshWorld NavMeshWorld;

    //Visual
    private bool showPointConnections;

    //Script/enum references
    private AI_Patrol t;
    private GUIStyle centeredStyle;
    private AI_PatrolSettings settings;

    //Inspector Draw
    private void Awake()
        {
        //Class
        t = target as AI_Patrol;

        if (EditorPrefs.HasKey("PatrolSettingsPath"))
            {
            settings = (AI_PatrolSettings) AssetDatabase.LoadAssetAtPath (EditorPrefs.GetString ("PatrolSettingsPath"), typeof (AI_PatrolSettings));
            SetData ();
            }

        //Variables
        patrolOptions = Enum.GetNames (typeof (PatrolMode)); //Get all the enum types
        pointMoveOptions = Enum.GetNames (typeof (PointMovement));

        patrolMode = t.patrolMode; //Get enum type for ref later on
        patrolOptionsIndex = (int)patrolMode;

        pointMovement = PointMovement.XZ; //Get enum type for ref later on
        pointOptionsIndex = (int)pointMovement;
        }
   
    public void SetData()
        {
        //---SCRIPTABLE OBJECT STUFFS---
        //Handle Values
        pointSize = settings.pointSize;
        isScaleCameraRelative = settings.isScaleCameraRelative;
        pathThickness = settings.pathThickness;

        //Colors
        pointColour = settings.pointColour;
        pathColour = settings.pathColour;
        areaColour = settings.areaColour;
        goalColour = settings.goalColour;

        //Visual Features
        showPointConnections = settings.togglePointConnections;
        //---SCRIPTABLE OBJECT STUFFS--- END
        }

    bool cakeIsgREAT;

    public override void OnInspectorGUI()
        {
        DrawDefaultInspector ();

        EditorGUI.BeginChangeCheck ();

        if (!cakeIsgREAT)
            if (GUILayout.Button ("Add Point Manually"))
                cakeIsgREAT = true;

        if (GUILayout.Button ("Add Point (Automatically)"))
            {
            Vector3 pointOne = points[0].point;
            Vector3 pointTwo = points[points.Length - 1].point;

            Vector3 origin = (pointOne + pointTwo) / 2;
            PointData point = new PointData ("", origin, 10);

            //Undo recording for reverting
            Undo.RecordObject (t, "Added Point");

            //Add point and update array
            t.patrolPoints.Add (point);
            points = t.patrolPoints.ToArray();
            }



        for (int i = 0; i < t.patrolPoints.Count; i++)
            {
            PointData point = t.patrolPoints[i];
            point.name = "Point " + i;
            }

        EditorGUI.EndChangeCheck ();
        }

    //Everything under this method is for the Scene GUI ONLY
    private void OnSceneGUI()
        {
        //Return checks
        if (t == null || t.patrolPoints == null)
            {
            Handles.BeginGUI ();
                GUILayout.Label ("No Data to specify");
            Handles.EndGUI ();

            return;
            }

        //Set style defaults
        centeredStyle = GUI.skin.GetStyle ("Label");
        centeredStyle.alignment = TextAnchor.UpperCenter;

        //Make sure it's only happening on the repaint event
        if (Event.current.type == EventType.Repaint)
            DisplayPatrolData ();

        //GUI Settings
        Handles.BeginGUI ();
           PatrolSettings ();
        Handles.EndGUI ();

        //Draggable Points
        if (patrolMode != PatrolMode.RandomAreaPoint)
            DragPoints ();

        if (cakeIsgREAT)
            if (Event.current.type == EventType.MouseUp)
                {
                Ray worldRay = HandleUtility.GUIPointToWorldRay (Event.current.mousePosition);
                RaycastHit hitInfo;

                if (Physics.Raycast (worldRay, out hitInfo))
                    {
                    Undo.RecordObject (t, "Added Point");

                    t.patrolPoints.Add (new PointData ("point", hitInfo.point, 1));
                    cakeIsgREAT = false;
                    }
                }
        }

    private void DisplayPatrolData()
        {
        //Temps
        Vector3 minArea = t.minAreaPoint;
        Vector3 maxArea = t.maxAreaPoint;

        //Setup variables
        points = t.patrolPoints.ToArray();
        areaBounds = new Vector3[] //Tedious but does what it needs to, and updates with changes
            {
            new Vector3(minArea.x, minArea.y, minArea.z),
            new Vector3(maxArea.x, minArea.y, minArea.z),
            new Vector3(maxArea.x, maxArea.y, maxArea.z),
            new Vector3(minArea.x, minArea.y, maxArea.z)
            };

        //Switch Editor Handles
        switch (patrolMode)
            {
            case PatrolMode.FollowPoints:
                Mode_NextPoint ();
                break;

            case PatrolMode.RandomPoint:
                Mode_NextPoint (false);
                break;

            case PatrolMode.RandomAreaPoint:
                Mode_RandomAreaPoint ();
                break;
            }

        DrawPatrolPath ();
        }

    //Patrol Mode Handles
    private void Mode_NextPoint(bool isConnected = true)
        {
        //Get point and store
        for (int i = 0; i < points.Length; i++)
            {
            //Get point
            PointData pointData = points[i];

            //Get current and next point
            Vector3 point = pointData.point;
            Vector3 pointNext = (i != points.Length - 1) ? points[i + 1].point : points[0].point;

            //Show info
            Vector3 textPoint = point + Vector3.up*5;

            //Make sure the text appears on top of stuff
            Handles.Label (textPoint, "Point " + i  
                + "\n" + point 
                + "\n" + pointData.moveDelay + "s movement delay", centeredStyle);

            //Connect up points if needed
            if (isConnected)
                {
                //Show arrow showing path dir
                Vector3 vec = pointNext - point;

                if (vec == Vector3.zero)
                    vec = Camera.main.transform.forward;

               // Handles.color = goalColour;
               // Handles.ArrowHandleCap (0, point, Quaternion.LookRotation (vec),  GetScaleSize(point, pointSize) , EventType.Repaint);

                //Show dotted line to next point
                if (showPointConnections)
                    {
                    Handles.color = pathColour;
                    Handles.DrawDottedLine (point, pointNext, 4.0f);
                    }
                }
            }
        }

    private void Mode_RandomAreaPoint()
        {
        //Draw area
        Handles.DrawSolidRectangleWithOutline (areaBounds, areaColour, Color.black);

        //Draw text of each max point
        Handles.Label (t.minAreaPoint + Camera.main.transform.forward, "Min\n" + t.minAreaPoint.ToString (), centeredStyle);
        Handles.Label (t.maxAreaPoint + Camera.main.transform.forward, "Max\n" + t.maxAreaPoint.ToString (), centeredStyle);

        if (EditorApplication.isPlaying)
            {
            //Draw goal location
            Handles.color = goalColour;
            Handles.DrawSolidDisc (t.Agent.destination, Vector3.up, 2);
            Handles.Label (t.Agent.destination, "Agent Goal", centeredStyle);

            //Draw path that the agent is following
            Handles.color = pathColour;
            Handles.DrawPolyLine (t.Agent.path.corners);
            }
        }

    int currentPoint = 0;

    Vector3 DraggableDir = new Vector3 ();
    bool xLock;
    bool yLock;
    bool zLock;

    //Setting Buttons/UI
    private void PatrolSettings()
        {
        //Patrol Mode Selection
        GUILayout.BeginArea (new Rect (10, 10, 250, 160));

        //Show Patrol modes
        GUILayout.Label ("Patrol Modes", EditorStyles.boldLabel);
        int patrolIndex = GUILayout.SelectionGrid (patrolOptionsIndex, patrolOptions, 2);

        GUILayout.Label ("Point Movement Mode", EditorStyles.boldLabel);

            GUILayout.BeginHorizontal ();
                GUILayout.BeginVertical ();
                    xLock = GUILayout.Toggle (xLock, "X");
                    yLock = GUILayout.Toggle (yLock, "Y");
                    zLock = GUILayout.Toggle (zLock, "Z");
                GUILayout.EndVertical ();

            if (GUILayout.Button ("Lock Points") )
                    {
                    xLock = false;
                    yLock = false;
                    zLock = false;
                    }
            GUILayout.EndHorizontal ();

        DraggableDir.x = (xLock) ? 1 : 0;
        DraggableDir.y = (yLock) ? 1 : 0;
        DraggableDir.z = (zLock) ? 1 : 0;


        // GUILayout.Label ("Point Movement Mode", EditorStyles.boldLabel);
        // int pointIndex = GUILayout.SelectionGrid (pointOptionsIndex, pointMoveOptions, 2);

        //Make sure that the GUI has been pressed and update it from there
        if (GUI.changed)
            {
            //Add to undo list
            Undo.RecordObject (t, "Changed point position");

            //Set data to changed
            patrolMode = (PatrolMode)patrolIndex;
            t.patrolMode = patrolMode;
            patrolOptionsIndex = patrolIndex;

            //pointMovement = (PointMovement)pointIndex;
            //pointOptionsIndex = pointIndex;
            }
          
        GUILayout.EndArea ();

        //State that the settings cannot be changed whilst playing
        if (EditorApplication.isPlaying)
            {
            GUILayout.BeginArea (new Rect (10, 180, 250, 30));
                GUILayout.Label ("Settings are disabled when game is active");
            GUILayout.EndArea ();

            return;
            }

        //Patrol Mode Settings
        GUILayout.BeginArea (new Rect (10, 150, 250, 80));
            GUILayout.Label ("Patrol Quick Settings", EditorStyles.boldLabel);

            //Focus on the current object selected
            //HandleExt.CreateButton ("Change to a Top-view of path", GUILayout.Width (250), SceneView.lastActiveSceneView.FrameSelected ());

            //Move to origin point ready for patrol
            switch (patrolMode)
                {
                case PatrolMode.FollowPoints:
                    if (GUILayout.Button ("Move to Origin Point", GUILayout.Width (250)))
                        {
                        t.gameObject.transform.position = points[0].point;
                        currentPoint = 0;
                        }

                    GUILayout.BeginHorizontal ();
                    
                    if (GUILayout.Button ("Next Point") )
                        {
                        currentPoint = (currentPoint == points.Length - 1 ? 0 : currentPoint + 1);
                        t.gameObject.transform.position = points[currentPoint].point;
                        }

                    if (GUILayout.Button ("Previous Point") )
                        {
                        currentPoint = (currentPoint == 0 ? points.Length-1 : currentPoint - 1);
                        t.gameObject.transform.position = points[currentPoint].point;
                        }

                    GUILayout.EndHorizontal ();
                    break;

                case PatrolMode.RandomPoint:
                    if (GUILayout.Button ("Move to Random Point", GUILayout.Width (250)))
                        t.gameObject.transform.position = t.GetNextPoint ();
                    break;
                }

        GUILayout.EndArea ();

        //NavMesh Generation
        GUILayout.BeginArea (new Rect (300, 10, 350, 100));
            GUILayout.Label ("NavMesh Path Generation", EditorStyles.boldLabel);

            GUILayout.BeginHorizontal ();
                bool calculatePath = GUILayout.Button ("Generate Path", GUILayout.Width (124));
                bool clearPath = GUILayout.Button ("Clear Path", GUILayout.Width (124));
            GUILayout.EndHorizontal ();

            GUILayout.Label ("Will clear path on deselect", EditorStyles.miniBoldLabel);
            GUILayout.Label ("Not accurate to agents - Points are equal to NavMesh corners", EditorStyles.miniBoldLabel);


        //Generate the path
        if (calculatePath)
            pathPoints = CalculateNavMeshPath ();

        //Clear it since it's not needed anymore
        if (clearPath)
            pathPoints = null;


        GUILayout.EndArea ();
        }

    //Draggable Points
    private void DragPoints()
        {
        if (points == null)
            return;

        //Update before changing due to decreasing the list size 
        //that will cause index errors
        points = t.patrolPoints.ToArray ();

        for (int i = 0; i < points.Length; i++)
            {
            //Create draggable point for moving the patrol locations around easily
            EditorGUI.BeginChangeCheck ();
            
            Vector3 oldPos = t.patrolPoints[i].point;
            Vector3 newPos = oldPos;

            Handles.color = pointColour;

            float size = GetScaleSize (oldPos, pointSize);

            int id = GUIUtility.GetControlID (FocusType.Keyboard);

            newPos = Handles.FreeMoveHandle (id, oldPos, Quaternion.identity, size, Vector3.one / 2, Handles.SphereHandleCap);
            newPos.x = (DraggableDir.x == 0) ? oldPos.x : newPos.x;
            newPos.y = (DraggableDir.y == 0) ? oldPos.y : newPos.y;
            newPos.z = (DraggableDir.z == 0) ? oldPos.z : newPos.z;

            /*
            switch (pointMovement)
                {
                case PointMovement.Y:
                    newPos = Handles.Slider (t.patrolPoints[i].point, Vector3.up, size, Handles.SphereHandleCap, 0.5f);
                    break;

                case PointMovement.XZ:
                    newPos = Handles.Slider2D (id, oldPos, Vector3.up, Vector3.forward, Vector3.right, size, Handles.SphereHandleCap, Vector2.one/2);
                    break;

                case PointMovement.XYZ:
                    newPos = Handles.FreeMoveHandle (id, oldPos, Quaternion.identity, size, Vector3.one / 2, Handles.SphereHandleCap);
                    break;
                }*/


            if (EditorGUI.EndChangeCheck ())
                {
                Undo.RecordObject (t, "Changed point position"); //Add to undo list
                t.patrolPoints[i].point = newPos; //Return to points
                }
            }
        }

    //NavMesh path
    private void DrawPatrolPath()
        {
        //Show dotted line to next point
        Handles.color = pathColour;
        Handles.DrawAAPolyLine (pathThickness, pathPoints);
        }

    //Calculate a nav mesh path from the patrol points set
    private Vector3[] CalculateNavMeshPath()
        {
        if (points.Length == 0)
            {
            Debug.Log ("No path to generate!");
            return null;
            }

        if (patrolMode == PatrolMode.RandomAreaPoint)
            {
            Debug.Log ("Path Generation not supported with this mode!");
            return null;
            }

        NavMeshPath path = new NavMeshPath ();
        List<Vector3> pathing = new List<Vector3> ();

        //Loop all points and calculate movement path to next point
        for (int i = 1; i <= points.Length; i++)
            {
            if (i < points.Length)
                {
                //Get two neighbouring points and ^^^ 
                Vector3 previousPoint = points[i - 1].point;
                Vector3 currentPoint = points[i].point;

                NavMesh.CalculatePath (previousPoint, currentPoint, 1, path);
                }
            else
                {
                //Link final and first points up
                NavMesh.CalculatePath (points[i - 1].point, points[0].point, 1, path);
                }

            //Add to list
            for (int j = 0; j < path.corners.Length; j++)
                pathing.Add (path.corners[j]);
            }

        //Store in array
        return pathing.ToArray ();
        }

    //Raycast to ground
    private Vector3 RaycastToGround(Vector3 point)
        {
        RaycastHit hit;

        if (Physics.Raycast (point, Vector3.down, out hit, Mathf.Infinity))
            return hit.point;

        return point;
        }

    /// <summary>
    /// Use when scaling handle size with camera distance
    /// </summary>
    /// <param name="point">Point to calculate distance</param>
    /// <param name="multiplier">Multiplier for scale</param>
    /// <returns></returns>
    private float GetScaleSize(Vector3 point, float multiplier = 1)
        {
        if (isScaleCameraRelative)
            return HandleUtility.GetHandleSize (point) * multiplier;
        else
            return multiplier;
        }
    }

public static class EditorExt
    {
    public static bool CreateButton(string buttonText, GUILayoutOption style)
        {
        bool isClicked = GUILayout.Button (buttonText, style);

        return isClicked;
        }

    public static bool CreateButton(string buttonText, GUILayoutOption style, UnityAction action)
        {
        bool isClicked = GUILayout.Button (buttonText, style);

        if (isClicked)
            action.Invoke ();

        return isClicked;
        }

    public static Vector3 Position2DHandle(Vector3 point, Vector3 xAxis, Vector3 zAxis, float scale, float snap)
        {
        Handles.color = Handles.xAxisColor;
            point.x = Handles.Slider (point, xAxis, scale, Handles.ArrowHandleCap, snap).x;

        Handles.color = Handles.zAxisColor;
            point.z = Handles.Slider (point, zAxis, scale, Handles.ArrowHandleCap, snap).z;

        return point;
        }

    public static int IntField(int defaultValue)
        {
        string text = GUILayout.TextField (defaultValue.ToString ());
        text = Regex.Replace (text, @"^\d$", "0");

        return Convert.ToInt32 (text);
        }
    }
