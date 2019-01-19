# UnityPatrolSystem - Read Me

This is the source code for a point-based patrol system with multiple modes and custom Editor additions for easier setup and use.

This read me will be updated when necessary. 

[Imgur](https://i.imgur.com/zyK3DPn.gifv)

---

#### Patrol Modes

<a href="https://imgur.com/GgfaFj3"><img src="https://i.imgur.com/GgfaFj3.png" title="source: imgur.com" /></a>


**Follow Points** - Agent will follow points in order of list    
**Random Point** - Agent selects random point from list to go to   
**RandomAreaPoint** - Agent will select a random point within the area and *attempt* to move to it

For all of the above, agents will stop going to points if it is impossible to go any further.
After that, the time delay will run before going to the next one. 

The list for the points is very simple showing the position and the delay time before moving again.

<a href="https://imgur.com/hWEi8KO"><img src="https://i.imgur.com/hWEi8KO.png" title="source: imgur.com" /></a>

#### Draggable Patrol Points

[Imgur](https://i.imgur.com/eUWmtHx.gifv)

As seen above, the points are editable from the draggable Handles within the Scene. This makes things much easier when it comes to setting points

---

#### Extra GUI Options

<img src="https://i.imgur.com/eaaBYeK.png" title="source: imgur.com" />

**Path Modes**

Selecting one of these will change the path mode for the GameObject which will also display the different Handles for the Scene.

**Patrol Quick Settings**

***Note:*** These settings will not appear when the game is running. 

*Focus on Patrol AI (F)* - Focuses on the object selected in the Hierarchy. States that F is also the hotkey for this (as default).
*Move to Origin Point* - Moves the GameObject to the first point within the List.

Others not shown above:

*Move to Random Point* - Select random point from the list and move the GameObject there. **[Area Point Mode]**

**NavMesh Generation**

The two buttons shown above are pretty straight forward in which the *Generate Path* button will calculate a **NavMesh Path** and draw a path within the Scene view.

The other button *Clear Path* removes it from view. This is here just for the sake of accessibility to keep things clean when needed.

***Note*** - As stated within the image, the path will clear when the GameObject is deselected.

---

#### Additions not currently added

Currently there is no support for Raycasting below to drop the points on top of meshes for accurate navigation. This will be one of the first added features to come.

When the **Experimental NavMesh API** is added with full support, there will be an update to the Generate Path preview. Until then, it'll only show a path connecting to NavMesh vertices.

More GUI additions - anything that can be useful or help make things easier.

**ScriptableObjects** may be used to store the data for:

- **Patrol Points/Area** - This is due to the fact that adding more in the future can be tedious, and it'll be easier and more dynamic with ScriptableObjects with custom exposure within the Inspector.

- **Replacing EditorPrefs** - Reasoning behind this is for projects that have multiple people working on them (i.e. Unity Collaborate) so then people can just customize their own styles. Either that or make the EditorPreference Keys more Unique to the local machine.

#### Currently known issues

The scene handles do not refresh automatically without a select/deselect of the GameObject(s) selected.

Colors will be defaulted to black when it's a new project. 
