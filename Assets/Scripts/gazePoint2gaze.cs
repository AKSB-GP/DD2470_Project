using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Varjo.XR;
using UnityEngine.XR;
public class gazePoint2gaze : MonoBehaviour
{
    public GameObject gazePoint;
    private Vector3 combinedEyeLocalForward;
    private Vector3 combinedEyeLocalOrigin;
    private bool isCombinedEyeGazeRayValid;
    private float cursorDist;
    Camera cam;
    // Start is called before the first frame update
    void Start()
    {
        cam = Camera.main;
        cursorDist = 0.75f;
    }

    // Update is called once per frame
    void Update()
    {
        VarjoEyeTracking.GazeData gazeData = VarjoEyeTracking.GetGaze();
        //Get curretn-frame gaze ray
        //isCombinedEyeGazeRayValid = SRanipal_Eye_v2.GetGazeRay ((GazeIndex) 2, out combinedEyeLocalOrigin, out combinedEyeLocalForward);
        isCombinedEyeGazeRayValid = gazeData.status != VarjoEyeTracking.GazeStatus.Invalid;
        //if gucci then create gazepoint
       if (isCombinedEyeGazeRayValid){
            combinedEyeLocalOrigin = gazeData.gaze.origin;
            combinedEyeLocalForward = gazeData.gaze.forward;
            //Debug.Log(isCombinedEyeGazeRayValid);
            gazePoint.transform.position = cam.transform.TransformPoint (combinedEyeLocalForward*cursorDist);
        }
    }
}
