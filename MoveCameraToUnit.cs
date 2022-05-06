using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MoveCameraToUnit : MonoBehaviour
{
    //units related
    public int unit_num;
    GameObject[] units;
    
    //camera offsets
    Camera cam;
    GameObject camLoc;
    GameObject unit;
    
    private Plane plane = new Plane(Vector3.right, Vector3.zero);
    private Vector3 v3Center = new Vector3(0.5f,0.5f,0);
    Vector3 camSmoothDampV;
    
    // Start is called before the first frame update
    void Start()
    {
        //find all units object
		units = GameObject.FindGameObjectsWithTag("Player");
        unit = units[unit_num-1];
        
        //camera
        camLoc = GameObject.Find("CameraLoc");
        cam = GameObject.Find("myCamera").GetComponent<Camera>();
    }
    

    
    // Calulate screen position and move camera to the target
    public void moveCamera()
	{
        Ray ray = cam.ViewportPointToRay(v3Center);
        float dist;
        if(plane.Raycast(ray, out dist)){
            Vector3 intersection = ray.GetPoint(dist);
            Debug.Log(intersection);
            
            Vector3 worldPos = units[unit_num-1].transform.position; 
            Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);
            Vector3 target = screenPos - intersection;
            
            // Move the camera smoothly to the target position
            camLoc.transform.position = target;
        }


    }
    
}
