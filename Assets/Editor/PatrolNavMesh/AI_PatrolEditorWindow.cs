using UnityEngine;
using UnityEditor;

[CanEditMultipleObjects]
public class AI_PatrolEditorWindow : EditorWindow
    {
    public AI_PatrolSettings settings;

    //Scale/Size
    private bool isScaleCameraRelative;

    private float pointSize = 1;
    private float pathThickness = 1;

    //Col Data
    private Color pointColour = Color.cyan;
    private Color pathColour = Color.red;
    private Color areaColour = Color.magenta;
    private Color goalColour = Color.green;

    //Visual Features
    private bool togglePointConnections;

    [MenuItem("Window/NavMesh Patrol/Scene Settings")]
    static void Init()
        {
        GetWindow (typeof (AI_PatrolEditorWindow));
        }

    private void OnEnable()
        {
        //Set min size
        minSize = new Vector2 (400, 100);

        if (EditorPrefs.HasKey ("PatrolSettingsPath"))
            {
            string objectPath = EditorPrefs.GetString ("PatrolSettingsPath");
            settings = AssetDatabase.LoadAssetAtPath (objectPath, typeof (AI_PatrolSettings)) as AI_PatrolSettings;

            LoadSettings ();
            }
        }

    private void OnFocus()
        {
        Repaint ();
        }

    [MenuItem("Patrol/Patrol Preferences")]
    public static void ShowWindow()
        {
        GetWindow(typeof(AI_PatrolEditorWindow), false, "Patrol");
        }

    private void OnGUI()
        {
        InitSettings ();

        if (settings == null)
            return;

        //Handle Scale/Size
        EditorGUILayout.LabelField ("Resizing", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal ();
            pointSize = EditorGUILayout.Slider ("Point Arrow Size", pointSize, 0.1f, 5);
            isScaleCameraRelative = EditorGUILayout.Toggle ("Scale with Camera", isScaleCameraRelative);
        EditorGUILayout.EndHorizontal ();

        pathThickness = EditorGUILayout.Slider ("NavMesh Path Thickness", pathThickness, 1, 5);

        EditorGUILayout.Space ();

        //Colour of Handles
        EditorGUILayout.LabelField ("Handle Colors", EditorStyles.boldLabel);
            pointColour = EditorGUILayout.ColorField ("Point Color", pointColour);
            pathColour = EditorGUILayout.ColorField ("Path Color", pathColour);
            areaColour = EditorGUILayout.ColorField ("Area Color", areaColour);
            goalColour = EditorGUILayout.ColorField ("Goal Color", goalColour);

        EditorGUILayout.Space ();

        EditorGUILayout.LabelField ("Visual Features", EditorStyles.boldLabel);
            togglePointConnections = EditorGUILayout.Toggle ("Show Point Connections", togglePointConnections);

        //Save and apply undo
        if (GUI.changed)
            SaveSettings ();
        }

    public void InitSettings()
        {
        //Taken from https://unity3d.com/learn/tutorials/modules/beginner/live-training-archive/scriptable-objects
        //then edited for now
        //Will be modified in the future for better needs
        if (settings != null)
            {
            GUILayout.Space (12);

            if (GUILayout.Button ("Show Settings in Project Window", GUILayout.Height(50)) )
                {
                EditorUtility.FocusProjectWindow ();
                Selection.activeObject = settings;
                }

            GUILayout.Space (12);
            }
        else
            {
            GUILayout.Label ("No current settings found!", EditorStyles.boldLabel);

            GUILayout.Space (12);

            if (GUILayout.Button ("Create Settings File"))
                {
                GUILayout.Label ("Creating file...", EditorStyles.boldLabel);
                AI_PatrolSettings settingsAsset = CreateInstance<AI_PatrolSettings> ();

                AssetDatabase.CreateAsset (settingsAsset, "Assets/Editor/PatrolSceneSettings.asset");
                AssetDatabase.SaveAssets ();

                settings = settingsAsset;

                if (settings)
                    {
                    LoadSettings ();
                    string assetPath = AssetDatabase.GetAssetPath (settings);
                    EditorPrefs.SetString ("PatrolSettingsPath", assetPath); 
                    }
                }
            }
        }

    /// <summary>
    /// Save values back into ScriptableObject
    /// </summary>
    private void SaveSettings()
        {
        settings.pointSize = pointSize;
        settings.isScaleCameraRelative = isScaleCameraRelative;
        settings.pathThickness = pathThickness;

        //Colors
        settings.pointColour = pointColour;
        settings.pathColour = pathColour;
        settings.areaColour = areaColour;
        settings.goalColour = goalColour;

        //Visual Features
        settings.togglePointConnections = togglePointConnections;
        }

    /// <summary>
    /// Loaded  from ScriptableObject
    /// </summary>
    private void LoadSettings()
        {
        //Basic
        pointSize = settings.pointSize;
        isScaleCameraRelative = settings.isScaleCameraRelative;
        pathThickness = settings.pathThickness;

        //Colors
        pointColour = settings.pointColour;
        pathColour = settings.pathColour;
        areaColour = settings.areaColour;
        goalColour = settings.goalColour;

        //Visual Features
        togglePointConnections = settings.togglePointConnections;
        }
    }

/// <summary>
/// Extra methods to extend the EditorPrefs for easy Get/Set situations
/// </summary>
public static class EditorPrefsExt
    {
    /// <summary>
    /// Converts Color to Hexadecmial for EditorPref string saving
    /// </summary>
    /// <param name="key">EditorPref Key</param>
    /// <param name="col">Color to convert</param>
    public static void SaveHexColour(string key, Color col)
        {
        string strCol = "#" + ColorUtility.ToHtmlStringRGBA (col);
        EditorPrefs.SetString (key, strCol);
        }

    /// <summary>
    /// Converts Hexadecimal string value in EditorPrefs to Color (RGBA)
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public static Color LoadHexColor(string key)
        {
        if (!EditorPrefs.HasKey (key))
            return Color.black;
        else
            {
            string strCol = EditorPrefs.GetString (key);

            Color col;
            ColorUtility.TryParseHtmlString (strCol, out col);

            return col;
            }
        }

    /// <summary>
    /// Checks for key and returns value if unknown type
    /// Rarely used
    /// </summary>
    /// <typeparam name="T">Key Value Type</typeparam>
    /// <param name="key">Key String</param>
    /// <param name="type">The literal key return type in the form typeof()</param>
    /// <returns></returns>
    public static dynamic LoadPref<T>(string key, T type)
        {
        if (EditorPrefs.HasKey(key))
            {
            Debug.Log(type.ToString());

            switch (type.ToString())
                {
                case "System.Boolean":
                    return EditorPrefs.GetBool (key);

                case "System.Single":
                    return EditorPrefs.GetBool (key);

                case "System.Int32":
                    return EditorPrefs.GetBool (key);
                }
            }

        return default(T);
        }
    }
