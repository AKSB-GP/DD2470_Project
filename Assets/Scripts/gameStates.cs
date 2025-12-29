using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class gameStates : MonoBehaviour {
    public static int frameOfCentering;
    public static bool isEyeOnTargetSpatialHys;
    public static bool isEyeOnTarget;
    public static bool isFixation;
    public static bool isVorTwd;
    public static bool isVorAway;
    public static bool isOnset;
    public static bool isToNext;
    public static bool isResetHead;
    public static bool isFixationCandidate;
    public static bool isCoroutineStarted;
    public static bool isFailure;
    public static int trialIndex;
    public static int eventType;


    //thresholds and other values
    public static float cdGain;
    public static Vector3 cursorWorldPos;
    public static Vector3 cursorInHeadPos;


    public static float ballPosPx_X; //x position of the ball in pixels, (0,0) is the center hole
    public static float ballPosPx_Y; //y position of the ball in pixels, (0,0) is the center hole
    public static float holeWorldPos_X;
    public static float holeWorldPos_Y;
    public static float holeWorldPos_Z;
    public static float holeLocalPos_X; //relative to the hmd
    public static float holeLocalPos_Y; //relative to the hmd
    public static float holeLocalPos_Z; //relative to the hmd
    public static float pinpointError;

    public static Color outterCircleColor = new Color (24 / 255f, 61 / 255f, 97 / 255f, 0.2f);
    public static Color innerCircleColor = new Color (78 / 255f, 105 / 255f, 26 / 255f, 0.2f);
    public static Color innerCircleColorFixated = new Color (78 / 255f, 105 / 255f, 26 / 255f, 0.2f);
    public static Color outterCircleColorFixated = new Color (24 / 255f, 61 / 255f, 97 / 255f, 1);
    public static Color innerCircleColorHeadGesture = new Color (240 / 255f, 70 / 255f, 115 / 255f, 1);
    public static Color golfBallColor = new Color (1, 1, 1, 1);
    public static Color holeColor = new Color (0, 0, 0, 1);
    public static float outterCircleRadius = 0.5f; //px value
    public static float innerCircleRadius = 0.224f; //px value 
    public static float spatialHysterisis = 0.1f; //how much error is allowed for ball-to-hole position to be holed
    public static float ballVelThreshHole = 0.5f; //how slow must ball be to be holed
    public static float timeThreshHole = 0.6f; //dwell time of ball in the hole's spatial hysterisis to be considered holed
    public static float golfBallRadius = 0.0707f; //px value
    public static float holeRadius = 0.0707f; //px value
    public static float vorAngleSensitivity = 45; //HMD vel dir. and cued VOR dir must be winthin this threshold, for the ball to move
    public static float pixPerDeg = 0.5f / 2.86f; //there is 0.5px in every 2.86deg of visual angle, on the raw image.
    public static float D = Mathf.Sqrt (0.2f); //px disatance that the ball has to travel from its inital position to the center hole.
    public static float vorFailTimeThreshold = 5; //how many seconds to wait for the ball to be holed before labelling a VOR failure

    public static float dwellTimeThreshold = 0.1f; //how many seconds that gaze and head must be steady to activate ball and hole (VOR) visuals
    public static float headVelThreshold = 5.0f; //1.0f; // (deg/s) head must be below this threshold to start dwellTimeThreshold to activate ball and hole (VOR) visuals

    //for limit pilot test
    public static int cdGainInd = 0;
    public static Vector3? targetCenterPos = null;
    public static bool isCam0TransformTransformSet = false;
    public static Vector3 cam0TransformPos;
    public static Quaternion cam0TransformRot;

    //for headBoost ii, Head-Gaze in all directions
    //starting target sequence index
    public static int seqIndex = 0; 
    //maximum target sequence index (number of sequences to run = maxSeqIndex+1)
    public static int maxSeqIndex = 1;
    //whether to automatically start logging(recording) data on scene load
    public static bool loggingOnStart = false;
    //overwrite the  customizeSeqIndex input in expoControl_collectAllHeadGazeDirection.cs, so that the next time the data collection scene loads, it uses gameStates.seqIndex.
    public static bool overWriteSeqIndexToContinue = false;

    // Start is called before the first frame update
    void Start () {
        frameOfCentering = 0;
        isEyeOnTargetSpatialHys = false;
        isEyeOnTarget = false;
        isFixation = false;
        isVorAway = false;
        isVorTwd = false;
        isOnset = false;
        isToNext = false;
        isFixationCandidate = false; //is dwell counting up
        isFailure = false;
        isCoroutineStarted = false;
        trialIndex = 0;
        eventType = 69;

        //threoulds and otehr values
        cdGain = Single.NaN;
        ballPosPx_X = Single.NaN;
        ballPosPx_Y = Single.NaN;
        holeWorldPos_X = Single.NaN;
        holeWorldPos_Y = Single.NaN;
        holeWorldPos_Z = Single.NaN;
        holeLocalPos_X = Single.NaN;
        holeLocalPos_Y = Single.NaN;
        holeLocalPos_Z = Single.NaN;
        pinpointError = Single.NaN;

        //initialize rawimage
        //pixPerDeg = 0.5f / 2.86f; //there is 0.5px in every 2.86deg of visual angle, on the raw image.
        //outterCircleColor = new Color (24 / 255f, 61 / 255f, 97 / 255f, 1);
        //innerCircleColor = new Color (78 / 255f, 105 / 255f, 26 / 255f, 0.2f);
        //innerCircleColorFixated = new Color (78 / 255f, 105 / 255f, 26 / 255f, 1);
        //golfBallColor = new Color (1, 1, 1, 1);
        //holeColor = new Color (0, 0, 0, 1);
        //outterCircleRadius = 0.5f;
        //innerCircleRadius = 0.224f;

        //golfBallRadius = 0.0707f;
        //holeRadius = 0.0707f;
        //D = Mathf.Sqrt (0.2f); //px disatance that the ball has to travel from its inital position to the center hole. or 2.56deg in visual angles. 

        //thresholds
        //spatialHysterisis = 0.1f; //how much error is allowed for ball-to-hole position to be holed
        //ballVelThreshHole = 0.5f; //how slow must ball be to be holed
        //vorAngleSensitivity = 45; //HMD vel dir. and cued VOR dir must be winthin this threshold, for the ball to move
    }

    // Update is called once per frame
    void Update () {

    }
}