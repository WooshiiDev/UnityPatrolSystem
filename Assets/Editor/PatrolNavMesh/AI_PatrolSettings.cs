using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
[CreateAssetMenu (fileName = "Patrol Settings", menuName = "NavMesh Patrol/Settings", order = 1)]
public class AI_PatrolSettings : ScriptableObject
    {
    //Scale/Size
    public bool isScaleCameraRelative;

    public float pointSize = 1;
    public float pathThickness = 1;

    //Col Data
    public Color pointColour = Color.cyan;
    public Color pathColour = Color.red;
    public Color areaColour = Color.magenta;
    public Color goalColour = Color.green;

    //Visual Features
    public bool togglePointConnections;

    //Data saving stuff
    public int currentSetPoint;
    }
