using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class isometricCamera : MonoBehaviour
{
    private gameModeManager modeManager;

    public GameObject selectedPlayer;

    public Transform cameraTarget;
    [SerializeField] public float speed = 5;
    [SerializeField] private bool isCamMoving = false;
    [SerializeField] private int screenWidth;
    [SerializeField] private int screenHeight;
    [SerializeField] public int horizontalBound = 30;
    [SerializeField] public int verticalBound = 30;
    public bool disabledCameraMotion;

    private Transform startTrans;
    Quaternion rotation;


    // Start is called before the first frame update
    void Awake()
    {
        modeManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<gameModeManager>();
        startTrans = this.transform;
        rotation = startTrans.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        //if (hasAuthority)
        //{
        if (cameraTarget == null || modeManager == null)
        {
            InitializeMe();
            Debug.Log("Have auth but No camera target for :" + gameObject.name);
            Debug.Log(gameObject.name + ": Searching for camera with command");
            cameraTarget = transform.parent.gameObject.transform;
                
            return;
        }
        screenWidth = Screen.width;
        screenHeight = Screen.height;
        if (modeManager.currentMode == gameModeManager.Mode.strategy)
        {
            if (!disabledCameraMotion) MoveCam();
        }
        //} else
        //{
        //    Debug.Log("I don't have authority over: " + gameObject.name);
        //}
    }

    public Transform targetTransitionPoint;
    public float camSpeed = 4f;

    private void LateUpdate()
    {
        if (modeManager == null) return;
        if (modeManager.currentMode == gameModeManager.Mode.transitionToThirdPerson)
        {
            if (targetTransitionPoint != selectedPlayer.transform.Find("transitionTarget").transform) targetTransitionPoint = selectedPlayer.transform.Find("transitionTarget").transform;
            //Lerp position
            transform.position = Vector3.Lerp(transform.position, targetTransitionPoint.position, Time.deltaTime * camSpeed);

            Vector3 currentAngle = new Vector3(
                Mathf.LerpAngle(transform.rotation.eulerAngles.x, targetTransitionPoint.transform.rotation.eulerAngles.x, Time.deltaTime * camSpeed),
                Mathf.LerpAngle(transform.rotation.eulerAngles.y, targetTransitionPoint.transform.rotation.eulerAngles.y, Time.deltaTime * camSpeed),
                Mathf.LerpAngle(transform.rotation.eulerAngles.z, targetTransitionPoint.transform.rotation.eulerAngles.z, Time.deltaTime * camSpeed));

            transform.eulerAngles = currentAngle;

            if (Vector3.Distance(transform.position, targetTransitionPoint.position) <= 0.1)
            {
                modeManager.ChangeMode(gameModeManager.Mode.thirdperson);
            }
        }
    }

    private Rect screenRect = new Rect(0, 0, Screen.width, Screen.height);

    void MoveCam()
    {
        if (transform.rotation != rotation) transform.rotation = rotation;
        if (!screenRect.Contains(Input.mousePosition))
            return;
        if (Input.mousePosition.x > screenWidth - horizontalBound)
        {
            isCamMoving = true;
            cameraTarget.transform.Translate(cameraTarget.transform.right * speed * Time.deltaTime, Space.World);
        }
        else if (Input.mousePosition.x < horizontalBound)
        {
            isCamMoving = true;
            cameraTarget.transform.Translate(-cameraTarget.transform.right * speed * Time.deltaTime, Space.World);
        }

        else if (Input.mousePosition.y > screenHeight - verticalBound)
        {
            isCamMoving = true;
            cameraTarget.transform.Translate(cameraTarget.transform.forward * speed * Time.deltaTime, Space.World);
        }
        else if (Input.mousePosition.y < verticalBound)
        {
            isCamMoving = true;
            cameraTarget.transform.Translate(-cameraTarget.transform.forward * speed * Time.deltaTime, Space.World);
        }
        else
        {
            isCamMoving = false;
        }
    }


    private void InitializeMe()
    {
        modeManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<gameModeManager>();
        startTrans = this.transform;
        rotation = startTrans.rotation;
    }
}
