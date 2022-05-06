using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ThirdPersonCamera : MonoBehaviour {

    private gameModeManager modeManager;

    public GameObject selectedPlayer;

    public bool lockCursor;
    public float mouseSensitivity = 10;
    public Transform target;
    public float dstFromTarget = 2;
    public Vector2 pitchMinMax = new Vector2(-40, 85);

    public float rotationSmoothTime = 0.1f;
    Vector3 rotationSmoothVelocity;
    Vector3 currentRotation;

    float yaw;
    float pitch;

    private void Awake()
    {
        InitializeMe();
    }

    private void Update()
    {
        if (modeManager == null) { InitializeMe(); return; }
        if (modeManager.currentMode == gameModeManager.Mode.thirdperson)
        {
            if (lockCursor && Cursor.lockState != CursorLockMode.Locked)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            } 
        } else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public Transform targetTransitionPoint;
    public float camSpeed = 4f;

    // Update is called once per frame
    void LateUpdate () {
        //if (hasAuthority)
        //{
        if (modeManager == null) return;
            if (modeManager.currentMode == gameModeManager.Mode.thirdperson)
            {
                if (target != selectedPlayer.transform.Find("ybotTarget").transform) target = selectedPlayer.transform.Find("ybotTarget").transform;
                yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
                pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;
                pitch = Mathf.Clamp(pitch, pitchMinMax.x, pitchMinMax.y);

                currentRotation = Vector3.SmoothDamp(currentRotation, new Vector3(pitch, yaw), ref rotationSmoothVelocity, rotationSmoothTime);
                transform.eulerAngles = currentRotation;


                transform.position = target.position - transform.forward * dstFromTarget;
            }
            else if (modeManager.currentMode == gameModeManager.Mode.transitionToStrategy)
            {
                //Lerp position
                transform.position = Vector3.Lerp(transform.position, targetTransitionPoint.position, Time.deltaTime * camSpeed);

                Vector3 currentAngle = new Vector3(
                    Mathf.LerpAngle(transform.rotation.eulerAngles.x, targetTransitionPoint.transform.rotation.eulerAngles.x + 45, Time.deltaTime * camSpeed),
                    Mathf.LerpAngle(transform.rotation.eulerAngles.y, targetTransitionPoint.transform.rotation.eulerAngles.y, Time.deltaTime * camSpeed),
                    Mathf.LerpAngle(transform.rotation.eulerAngles.z, targetTransitionPoint.transform.rotation.eulerAngles.z, Time.deltaTime * camSpeed));

                transform.eulerAngles = currentAngle;
                if (Vector3.Distance(transform.position, targetTransitionPoint.position) <= 0.01)
                {
                    modeManager.ChangeMode(gameModeManager.Mode.strategy);
                }
        }
            
        //}
    }

    private void InitializeMe()
    {
        modeManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<gameModeManager>();
    }
}
