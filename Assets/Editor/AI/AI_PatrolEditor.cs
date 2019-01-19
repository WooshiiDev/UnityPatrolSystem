using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.AI;
using UnityEngine.Experimental.AI;

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
    private string[] patrolOptions;
    public int index;

    //Path Settings
    private bool showDebug;

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
    private PatrolMode patrolMode;

    private GUIStyle centeredStyle;

    //Inspector Draw
    private void Awake()
        {
        //Class
        t = target as AI_Patrol;

        string prefKey = EditorPrefs.GetString ("Patrol_LocalSaveKey");

        //Variables
        patrolOptions = Enum.GetNames (typeof (PatrolMode)); //Get all the enum types
        patrolMode = t.patrolMode; //Get enum type for ref later on
        index = (int)patrolMode;

        //Load all values in
        pointSize = EditorPrefs.GetFloat            (prefKey + "_PointSize", pointSize);
        isScaleCameraRelative = EditorPrefs.GetBool (prefKey + "_isPointSizeRelative", isScaleCameraRelative);
        pathThickness = EditorPrefs.GetFloat        (prefKey + "_PathThickness", pathThickness);

        //Colors
        pointColour = EditorPrefsExt.LoadHexColor   (prefKey + "_PointColor");
        pathColour = EditorPrefsExt.LoadHexColor    (prefKey + "_PathColor");
        areaColour = EditorPrefsExt.LoadHexColor    (prefKey + "_AreaColor");
        goalColour = EditorPrefsExt.LoadHexColor    (prefKey + "_GoalColor");

        //Visual
        showPointConnections = EditorPrefs.GetBool (prefKey + "_PointConnections");
        }
   
    public override void OnInspectorGUI()
        {
        DrawDefaultInspector ();

        EditorGUI.BeginChangeCheck ();

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

                Handles.color = goalColour;
                Handles.ArrowHandleCap (0, point, Quaternion.LookRotation (vec),  GetScaleSize(point, pointSize) , EventType.Repaint);

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

    //Setting Buttons/UI
    private void PatrolSettings()
        {
        //Patrol Mode Selection
        GUILayout.BeginArea (new Rect (10, 10, 250, 70));

        //Show Patrol modes
        GUILayout.Label ("Patrol Modes", EditorStyles.boldLabel);
        int patrolIndex = GUILayout.SelectionGrid (index, patrolOptions, 2);
        
        //Make sure that the GUI has been pressed and update it from there
        if (GUI.changed)
            {
            //Add to undo list
            Undo.RecordObject (t, "Changed point position");

            //Set data to changed
            patrolMode = (PatrolMode)patrolIndex;
            t.patrolMode = patrolMode;
            index = patrolIndex;
            }
          
        GUILayout.EndArea ();

        //State that the settings cannot be changed whilst playing
        if (EditorApplication.isPlaying)
            {
            GUILayout.BeginArea (new Rect (10, 80, 250, 30));
                GUILayout.Label ("Settings are disabled when game is active");
            GUILayout.EndArea ();

            return;
            }

        //Patrol Mode Settings
        GUILayout.BeginArea (new Rect (10, 80, 250, 80));
            GUILayout.Label ("Patrol Quick Settings", EditorStyles.boldLabel);

            //Focus on the current object selected
            if (GUILayout.Button ("Focus on Patrol AI (F)", GUILayout.Width (250)))
                    SceneView.lastActiveSceneView.FrameSelected ();

            //Move to origin point ready for patrol
            switch (patrolMode)
                {
                case PatrolMode.FollowPoints:
                    if (GUILayout.Button ("Move to Origin Point", GUILayout.Width (250)))
                        t.gameObject.transform.position = points[0].point;
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

            //Draggable area
            Handles.color = pointColour;
                Vector3 oldPos = t.patrolPoints[i].point;
                Vector3 newPos = Handles.Slider2D (t.patrolPoints[i].point, Vector3.up, Vector3.right, Vector3.forward, GetScaleSize(oldPos, pointSize), Handles.SphereHandleCap, 1);

            if (EditorGUI.EndChangeCheck())
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
                {
                pathing.Add (path.corners[j]);
                }
            }

        //Store in array
        return pathing.ToArray ();
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
