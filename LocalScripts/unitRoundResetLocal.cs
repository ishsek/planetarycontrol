using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class unitRoundResetLocal : MonoBehaviour
{
    private gameModeManager modeManager;
    private moveUnitLocal myMoveUnit;
    private PlayerControllerLocal myPlayerController;
    // Start is called before the first frame update
    void Start()
    {
        modeManager = GameObject.FindWithTag("GameManager").GetComponent<gameModeManager>();
        myMoveUnit = GetComponent<moveUnitLocal>();
        myPlayerController = GetComponent<PlayerControllerLocal>();
    }

    // Update is called once per frame
    void Update()
    {
        modeManager = GameObject.FindWithTag("GameManager").GetComponent<gameModeManager>();
        myMoveUnit = GetComponent<moveUnitLocal>();
        myPlayerController = GetComponent<PlayerControllerLocal>();
    }

    // Reset stuff for this unit between rounds
    public void ResetMe()
    {
        myPlayerController.currentActionPoints = myPlayerController.fullActionPoints;
        myPlayerController.previewActionPoints = myPlayerController.currentActionPoints;
        myMoveUnit.attackedThisTurn = false;
        myMoveUnit.selected = false;
        myMoveUnit.myOutline.OutlineWidth = 0f;
        myMoveUnit.myOutline.OutlineColor = myMoveUnit.outlineHoverColor;
        myMoveUnit.GetComponent<NavMeshAgent>().ResetPath();
        myMoveUnit.myPointer.transform.position = new Vector3(0, -20, 0);
    }
}
