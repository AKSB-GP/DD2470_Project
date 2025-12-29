using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using james_utils;


public class expo_control : MonoBehaviour
{
    
    public RawImage stim;
    public string stimPosCsvName;
    private StreamReader reader;
    private List<Vector3> stimInHmdPosList = new List<Vector3> ();
    private int s;
    private Camera cam;

    private static System.Random rng = new System.Random();  

    private float timeStart;

    private bool toStart = false;

    public GameObject debugger;
    private List<GameObject> debugList = new List<GameObject> ();
    private bool debugStimPos = false;

    // Start is called before the first frame update
    void Start()
    {
        //initialize rawimage
        stim.material.SetColor ("_ColorOutterCircle", gameStates.outterCircleColorFixated);
        stim.material.SetColor ("_ColorInnerCircle", gameStates.innerCircleColorFixated);
        stim.material.SetColor ("_ColorHole", gameStates.holeColor);
        stim.material.SetFloat ("_RadiusOutterCircle", gameStates.outterCircleRadius);
        stim.material.SetFloat ("_RadiusInnerCircle", gameStates.innerCircleRadius);
        stim.material.SetFloat ("_RadiusHole", gameStates.holeRadius);
        stim.material.SetInt ("_ShallMakeHole", 1);

        stim.gameObject.SetActive (true);

        //read stim-in-HMD Positions
        var tarPosPath = Path.Combine (Application.dataPath, "Resources", stimPosCsvName+".csv");
        StreamReader reader = new StreamReader (@tarPosPath);
        stimInHmdPosList = csv_utils.CSVRead2Vector3List (stimInHmdPosList, reader);

        s=0;

        cam=Camera.main;

        //stimInHmdPosList = ShuffleListVector3(stimInHmdPosList);
        IListExtensions.Shuffle(stimInHmdPosList);

        timeStart = Time.time;

        if (debugStimPos){
            for (int i=0; i<stimInHmdPosList.Count; i++){
            var g = Instantiate(debugger, new Vector3(0,0,0), Quaternion.identity);
            g.SetActive(true);
            
            debugList.Add(g);
            }
        }
        

    }

    // Update is called once per frame
    void Update()
    {
        if (debugStimPos){
            for (int i=0; i<debugList.Count; i++){
                var g = debugList[i];
                g.transform.position = cam.transform.TransformPoint (stimInHmdPosList[i] + new Vector3(0, -0.0f, -0.0f));
                g.transform.LookAt (cam.transform);
                g.transform.Rotate (0, 180, 0, Space.Self);
            }
        }
        

        if (Input.GetKeyDown(KeyCode.Space)){
            toStart=true;
            timeStart = Time.time;
        }

        if (toStart==false){
            return;
        }

        if (s>=49){
            stim.gameObject.SetActive(false);
        }

        gameStates.trialIndex = s;
        stim.transform.position = cam.transform.TransformPoint (stimInHmdPosList[s] + new Vector3(0, -0.0f, -0.0f));
        stim.transform.LookAt (cam.transform);
        stim.transform.Rotate (0, 180, 0, Space.Self);

        gameStates.holeLocalPos_X = cam.transform.InverseTransformPoint(stim.transform.position).x;
        gameStates.holeLocalPos_Y = cam.transform.InverseTransformPoint(stim.transform.position).y;
        gameStates.holeLocalPos_Z = cam.transform.InverseTransformPoint(stim.transform.position).z;
        

        
        if (Time.time - timeStart > 2){
            //Debug.Log("s: " + s.ToString());
            //Debug.Log(stimInHmdPosList[s]);
            
            stim.gameObject.SetActive (true);
            s=s+1;

            timeStart = Time.time;
        }

        



    }

    // List<Vector3> CSVRead2Vector3List (List<Vector3> stimSP, StreamReader reader) {
    //     using (reader) {
    //         while (!reader.EndOfStream) {
    //             var line = reader.ReadLine ();
    //             var values = line.Split (',');

    //             stimSP.Add (new Vector3 (
    //                 float.Parse (values[0], CultureInfo.InvariantCulture),
    //                 float.Parse (values[1], CultureInfo.InvariantCulture),
    //                 float.Parse (values[2], CultureInfo.InvariantCulture)));
    //         }
    //     }
    //     return stimSP;
    // }

    
    // List<Vector3> ShuffleListVector3( List<Vector3> list)  
    // {  
    //     int n = list.Count;  
    //     while (n > 1) {  
    //         n--;  
    //         int k = rng.Next(n + 1);  
    //         Vector3 value = list[k];  
    //         list[k] = list[n];  
    //         list[n] = value;  
    //     }  

    //     return list;
    // }
}
