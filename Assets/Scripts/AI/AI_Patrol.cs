using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System.Linq;

public enum PatrolMode { FollowPoints, RandomPoint, RandomAreaPoint }

[System.Serializable]
public class PointData
    {
    [HideInInspector]
    public string name;

    public Vector3 point;
    public float moveDelay;


    public PointData(string _name, Vector3 _point, float _moveDelay)
        {
        name = _name;

        point = _point;
        moveDelay = _moveDelay;
        }
    }

[RequireComponent(typeof(NavMeshAgent) )]
public class AI_Patrol : MonoBehaviour
    {
    public PatrolMode patrolMode;

    public float movementSpeed;
    private float delayTimer;
    [SerializeField]

    [Header ("Point Based Patrol")]
    public List<PointData> patrolPoints;
    private Vector3 currentPoint;
    private int pointIndex;

    [Header ("Area Patrol")]
    public Vector3 minAreaPoint;
    public Vector3 maxAreaPoint;

    //Components
    public NavMeshAgent Agent { get; private set; }

    // Use this for initialization
    void Awake()
        {
        //Setup NavMesh Agent
        Agent = GetComponent<NavMeshAgent> ();
        Agent.speed = movementSpeed;

        Agent.destination = GetNextPoint ();
        }

    public Vector3 GetNextPoint()
        {
        Vector3 targetPoint = new Vector3 ();

        switch (patrolMode)
            {
            //Get next point
            case PatrolMode.FollowPoints:
                targetPoint = patrolPoints[pointIndex].point;

                pointIndex = (pointIndex == patrolPoints.Count - 1) ? 0 : pointIndex + 1;
                break;

            case PatrolMode.RandomPoint:
                //Randomise
                System.Random rand = new System.Random ();
                pointIndex = rand.Next (0, patrolPoints.Count);

                targetPoint = patrolPoints[pointIndex].point;
                break;

            case PatrolMode.RandomAreaPoint:
                //Get area size by getting vector diff
                Vector3 areaSize = maxAreaPoint - minAreaPoint;

                //Get random unit size and get a positive descale from area size
                Vector2 randomPos = Random.insideUnitCircle;
                areaSize.x *= Mathf.Abs (randomPos.x);
                areaSize.z *= Mathf.Abs (randomPos.y);

                //Apply to target
                targetPoint = maxAreaPoint - areaSize;
                break;
            }

        return targetPoint;
        }

    // Update is called once per frame
    void Update()
        {
        if (!Agent.pathPending && Agent.remainingDistance <= 0)
            {
            //Get the movement delay from the struct in the list
            float movementDelay = patrolPoints[pointIndex].moveDelay;

            //Increase time until the time has passed, so the GameObject can move again
            delayTimer += Time.deltaTime;

            if (delayTimer >= movementDelay)
                {
                Agent.SetDestination(GetNextPoint ());
                delayTimer = 0;
                }
            }
        }
    }
