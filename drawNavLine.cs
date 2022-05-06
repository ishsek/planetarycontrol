using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Networking;

public class drawNavLine : NetworkBehaviour
{
    public gameModeManager modeManager;
    public NavMeshAgent agentToDraw;
    public LineRenderer lineR;
    public moveUnit myMoveUnit;
    public float lineHeight;
    public float pathLength;

    // Start is called before the first frame update
    public override void OnStartAuthority()
    {
        modeManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<gameModeManager>();
        agentToDraw = GetComponent<NavMeshAgent>();
        lineR = GetComponent<LineRenderer>();
        myMoveUnit = GetComponent<moveUnit>();
    }

    // Update is called once per frame
    void Update()
    {
        //if (hasAuthority)
        //{
        if (modeManager == null) { InitializeMe(); return; }
        if (modeManager.currentMode == gameModeManager.Mode.strategy)
        {
            if (!myMoveUnit.selected && lineR.enabled == true) { lineR.enabled = false; return; }
            if (agentToDraw.hasPath)
            {
                lineR.positionCount = agentToDraw.path.corners.Length;
                Vector3[] positionsToSet = agentToDraw.path.corners;
                for (int i = 0; i < positionsToSet.Length; i++)
                {
                    positionsToSet[i].y += lineHeight;
                }
                Vector3 current = positionsToSet[0];
                pathLength = 0;
                for (int i = 1; i < positionsToSet.Length; i++)
                {
                    pathLength += (Vector3.Distance(current, positionsToSet[i]));
                    current = positionsToSet[i];
                }
                lineR.SetPositions(positionsToSet);
                lineR.enabled = true;
            }
            else
            {
                pathLength = 0;
            }
        }
        else
        {
            lineR.enabled = false;
        }
        //}
    }

    private void InitializeMe()
    {
        modeManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<gameModeManager>();
        agentToDraw = GetComponent<NavMeshAgent>();
        lineR = GetComponent<LineRenderer>();
        myMoveUnit = GetComponent<moveUnit>();
    }
}
