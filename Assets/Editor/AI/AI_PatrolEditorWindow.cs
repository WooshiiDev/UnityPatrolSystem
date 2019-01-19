using UnityEngine;
using UnityEditor;

[CanEditMultipleObjects]
public class AI_PatrolEditorWindow : EditorWindow
    {
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

    private string keyDefaults;

    private void Awake()
        {
        //Set min size
        minSize = new Vector2 (400, 100);

        //Load Prefs
        SetEditorPreferencesDefaults ();
        GetEditorPreferences ();
        }

    [MenuItem("Patrol/Patrol Preferences")]
    public static void ShowWindow()
        {
        GetWindow(typeof(AI_PatrolEditorWindow), false, "Patrol");
        }

    private void OnGUI()
        {
        Undo.RecordObject (this, "Opening Window");

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
            SetEditorPreferences ();
        }

    /// <summary>
    /// Call to save all custom editor preferences
    /// </summary>
    private void SetEditorPreferences()
        {
        //Size Data
        EditorPrefs.SetFloat        (keyDefaults + "_PointSize", pointSize);
        EditorPrefs.SetBool         (keyDefaults + "_isPointSizeRelative", isScaleCameraRelative);
        EditorPrefs.SetFloat        (keyDefaults + "_PathThickness", pathThickness);

        //Color Data
        EditorPrefsExt.SaveHexColour (keyDefaults + "_PointColor", pointColour);
        EditorPrefsExt.SaveHexColour (keyDefaults + "_PathColor",  pathColour);
        EditorPrefsExt.SaveHexColour (keyDefaults + "_AreaColor",  areaColour);
        EditorPrefsExt.SaveHexColour (keyDefaults + "_GoalColor", goalColour);

        //Visual
        EditorPrefs.SetBool (keyDefaults + "_PointConnections", togglePointConnections);
        }

    /// <summary>
    /// Load Preferences
    /// </summary>
    private void GetEditorPreferences()
        {
        keyDefaults = Application.productName;
        EditorPrefs.SetString ("Patrol_LocalSaveKey", keyDefaults);

        //Load all values in
        pointSize = EditorPrefs.GetFloat            (keyDefaults + "_PointSize", pointSize);
        isScaleCameraRelative = EditorPrefs.GetBool (keyDefaults + "_isPointSizeRelative", isScaleCameraRelative);
        pathThickness = EditorPrefs.GetFloat        (keyDefaults + "_PathThickness", pathThickness);

        //Colors
        pointColour = EditorPrefsExt.LoadHexColor   (keyDefaults + "_PointColor");
        pathColour = EditorPrefsExt.LoadHexColor    (keyDefaults  + "_PathColor");
        areaColour = EditorPrefsExt.LoadHexColor    (keyDefaults  + "_AreaColor");
        goalColour = EditorPrefsExt.LoadHexColor    (keyDefaults  + "_GoalColor");

        //Visual Features
        togglePointConnections = EditorPrefs.GetBool (keyDefaults + "_PointConnections");

        //EditorPrefsExt.LoadPref ("PointConnections", typeof(bool) );
        }

    /// <summary>
    /// Should only be needed on the first load
    /// </summary>
    private void SetEditorPreferencesDefaults()
        {
        //Only needs to check for the value of one, unless it's been removed manually elsewhere
        if (!EditorPrefs.HasKey("Patrol_LocalSaveKey") )
            {
            keyDefaults = Application.productName;
            EditorPrefs.SetString ("Patrol_LocalSaveKey", keyDefaults);

            //Size Data
            EditorPrefs.SetFloat            (keyDefaults + "_PointSize", 0.2f);
            EditorPrefs.SetBool             (keyDefaults + "_isPointSizeRelative", true);
            EditorPrefs.SetFloat            (keyDefaults + "_PathThickness", 1);

            //Color Data
            //TODO - Store defaults better
            EditorPrefsExt.SaveHexColour    (keyDefaults + "_PointColor", Color.cyan - new Color(0,0,0,0.5f));
            EditorPrefsExt.SaveHexColour    (keyDefaults + "_PathColor",  Color.red);
            EditorPrefsExt.SaveHexColour    (keyDefaults + "_AreaColor",  Color.magenta - new Color (0, 0, 0, 0.5f));
            EditorPrefsExt.SaveHexColour    (keyDefaults + "_GoalColor",  Color.green);

            //Visual
            EditorPrefs.SetBool             (keyDefaults + "_PointConnections", true);
            }
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
