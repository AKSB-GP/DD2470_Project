using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity;
using UnityEngine;
using UnityEngine.XR;
using Varjo.XR;
public class startCalibration : MonoBehaviour
{
    //if to start calibration at the start of the scene
    public bool doCalibrationAtStart = false;
    // key to manually restart calibration
    private List<InputDevice> devices = new List<InputDevice>();
    private InputDevice device;

    public KeyCode calibrationKey = KeyCode.C;
    [Header("Gaze calibration settings")]
    public VarjoEyeTracking.GazeCalibrationMode gazeCalibrationMode = VarjoEyeTracking.GazeCalibrationMode.Fast;
    //TODO SWAP VIVEVR TO VARJO XR EYEBALICBRATION
    // Start is called before the first frame update
    void Start()
    {
        //start calibration
        if (doCalibrationAtStart)
        {
            //ViveSR.anipal.Eye.SRanipal_Eye_API.LaunchEyeCalibration (IntPtr.Zero);
            VarjoEyeTracking.RequestGazeCalibration(gazeCalibrationMode);

        }
    }

    // Update is called once per frame
    void Update()
    {
        //manual restart of calibration
        if (Input.GetKey(calibrationKey))
        {
            //ViveSR.anipal.Eye.SRanipal_Eye_API.LaunchEyeCalibration(IntPtr.Zero);
            VarjoEyeTracking.RequestGazeCalibration(gazeCalibrationMode);

        }
    }
    //from varjo
    void GetDevice()
    {
        InputDevices.GetDevicesAtXRNode(XRNode.CenterEye, devices);
        device = devices.FirstOrDefault();
    }

    void OnEnable()
    {
        if (!device.isValid)
        {
            GetDevice();
        }
    }
}

