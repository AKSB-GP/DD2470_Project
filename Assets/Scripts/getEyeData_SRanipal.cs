using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;
using Varjo.XR;

public class getEyeData_SRanipal : MonoBehaviour
{
    public string saveFolder;
    public GameObject gazePoint;

    private static Matrix4x4 cameraTransformLocalToWorldMatrix;
    private static Vector3 hmdWorldOrigin;
    private static Vector3 hmdWorldForward;
    private static Quaternion hmdWorldQuaternion;

    public KeyCode loggingKey = KeyCode.Return;
    private static bool logging = false;
    private static StreamWriter writer = null;
    private List<VarjoEyeTracking.GazeData> dataSinceLastUpdate;
    private List<VarjoEyeTracking.EyeMeasurements> eyeMeasurementsSinceLastUpdate;
    static readonly string[] ColumnNames = {
        "frameSequence",
        "systemTimestamp(ms)",
        "unityTimestamp(s)",
        "deviceTimestamp(ms)",
        "trialIndex",
        "combinedEyeLocalOrigin_X",
        "combinedEyeLocalOrigin_Y",
        "combinedEyeLocalOrigin_Z",
        "combinedEyeLocalForward_X",
        "combinedEyeLocalForward_Y",
        "combinedEyeLocalForward_Z",
        "isCombinedEyeGazeRayValid",
        "leftEyeLocalOrigin_X",
        "leftEyeLocalOrigin_Y",
        "leftEyeLocalOrigin_Z",
        "leftEyeLocalForward_X",
        "leftEyeLocalForward_Y",
        "leftEyeLocalForward_Z",
        "leftEyePupilDiameter",
        "leftEyePupilPositionInSensorArea_X",
        "leftEyePupilPositionInSensorArea_Y",
        "leftEyePupilPosition_X",
        "leftEyePupilPosition_Y",
        "leftEyeOpenness",
        "leftEyeOpennessReadSuccess",
        "leftEyeFrown",
        "leftEyeSqueeze",
        "leftEyeWide",
        "isLeftEyeGazeRayValid",
        "rightEyeLocalOrigin_X",
        "rightEyeLocalOrigin_Y",
        "rightEyeLocalOrigin_Z",
        "rightEyeLocalForward_X",
        "rightEyeLocalForward_Y",
        "rightEyeLocalForward_Z",
        "rightEyePupilDiameter",
        "rightEyePupilPositionInSensorArea_X",
        "rightEyePupilPositionInSensorArea_Y",
        "rightEyePupilPosition_X",
        "rightEyePupilPosition_Y",
        "rightEyeOpenness",
        "rightEyeOpennessReadSuccess",
        "rightEyeFrown",
        "rightEyeSqueeze",
        "rightEyeWide",
        "isRightEyeGazeRayValid",
        "hmdWorldOrigin_X",
        "hmdWorldOrigin_Y",
        "hmdWorldOrigin_Z",
        "hmdWorldForward_X",
        "hmdWorldForward_Y",
        "hmdWorldForward_Z",
        "hmdWorldQuaternion_W",
        "hmdWorldQuaternion_X",
        "hmdWorldQuaternion_Y",
        "hmdWorldQuaternion_Z",
        "hmdL2W_m00",
        "hmdL2W_m01",
        "hmdL2W_m02",
        "hmdL2W_m03",
        "hmdL2W_m10",
        "hmdL2W_m11",
        "hmdL2W_m12",
        "hmdL2W_m13",
        "hmdL2W_m20",
        "hmdL2W_m21",
        "hmdL2W_m22",
        "hmdL2W_m23",
        "hmdL2W_m30",
        "hmdL2W_m31",
        "hmdL2W_m32",
        "hmdL2W_m33",
        "headVel_Az",
        "headVel_Pol",
        "stimLocalPos_X",
        "stimLocalPos_Y",
        "stimLocalPos_Z"
    };

    Camera cam;
    float[] headAzs = new float[5];
    float[] headPols = new float[5];
    float dt = 1 / 90.0f;
    public static float headVel_Az;
    public static float headVel_Pol;

    private static long startTime;
    private static float unityTime;

    void Start()
    {
        cam = Camera.main;
        startTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
    }

    void Update()
    {
        dt = Time.deltaTime;
        unityTime = Time.time;

        cameraTransformLocalToWorldMatrix = cam.transform.localToWorldMatrix;
        hmdWorldOrigin = cam.transform.position;
        hmdWorldForward = cam.transform.forward;
        hmdWorldQuaternion = cam.transform.rotation;

        Vector2 headFicks = vec2ficks(cam.transform.forward.x, cam.transform.forward.y, cam.transform.forward.z);
        for (int i = 0; i < 4; i++)
        {
            headAzs[i] = headAzs[i + 1];
            headPols[i] = headPols[i + 1];
        }
        headAzs[4] = headFicks.x;
        headPols[4] = headFicks.y;
        headVel_Az = (-headAzs[0] - headAzs[1] + headAzs[3] + headAzs[4]) / (6 * dt);
        headVel_Pol = (-headPols[0] - headPols[1] + headPols[3] + headPols[4]) / (6 * dt);

        if (Input.GetKeyDown(loggingKey))
        {
            if (logging) StopLogging(); else StartLogging();
        }

        int gazeCount = VarjoEyeTracking.GetGazeList(out dataSinceLastUpdate, out eyeMeasurementsSinceLastUpdate);

        if (dataSinceLastUpdate != null && dataSinceLastUpdate.Count > 0)
        {
            VarjoEyeTracking.GazeData latestGaze = dataSinceLastUpdate[dataSinceLastUpdate.Count - 1];
            if (latestGaze.status != VarjoEyeTracking.GazeStatus.Invalid)
            {
                gazePoint.transform.position = cam.transform.TransformPoint(latestGaze.gaze.forward * 0.75f);
            }

            if (logging)
            {
                foreach (var data in dataSinceLastUpdate)
                {
                    CaptureAndLog(data);
                }
            }
        }
    }

    void CaptureAndLog(VarjoEyeTracking.GazeData data)
    {
        string[] logData = new string[ColumnNames.Length];

        logData[Array.IndexOf(ColumnNames, "frameSequence")] = data.frameNumber.ToString();
        logData[Array.IndexOf(ColumnNames, "systemTimestamp(ms)")] = (DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond - startTime).ToString();
        logData[Array.IndexOf(ColumnNames, "unityTimestamp(s)")] = unityTime.ToString();
        logData[Array.IndexOf(ColumnNames, "deviceTimestamp(ms)")] = (data.captureTime / 1000000).ToString();
        logData[Array.IndexOf(ColumnNames, "trialIndex")] = gameStates.trialIndex.ToString();

        logData[Array.IndexOf(ColumnNames, "combinedEyeLocalOrigin_X")] = data.gaze.origin.x.ToString();
        logData[Array.IndexOf(ColumnNames, "combinedEyeLocalOrigin_Y")] = data.gaze.origin.y.ToString();
        logData[Array.IndexOf(ColumnNames, "combinedEyeLocalOrigin_Z")] = data.gaze.origin.z.ToString();
        logData[Array.IndexOf(ColumnNames, "combinedEyeLocalForward_X")] = data.gaze.forward.x.ToString();
        logData[Array.IndexOf(ColumnNames, "combinedEyeLocalForward_Y")] = data.gaze.forward.y.ToString();
        logData[Array.IndexOf(ColumnNames, "combinedEyeLocalForward_Z")] = data.gaze.forward.z.ToString();
        logData[Array.IndexOf(ColumnNames, "isCombinedEyeGazeRayValid")] = (data.status != VarjoEyeTracking.GazeStatus.Invalid).ToString();

        logData[Array.IndexOf(ColumnNames, "leftEyeLocalOrigin_X")] = data.left.origin.x.ToString();
        logData[Array.IndexOf(ColumnNames, "leftEyeLocalOrigin_Y")] = data.left.origin.y.ToString();
        logData[Array.IndexOf(ColumnNames, "leftEyeLocalOrigin_Z")] = data.left.origin.z.ToString();
        logData[Array.IndexOf(ColumnNames, "leftEyeLocalForward_X")] = data.left.forward.x.ToString();
        logData[Array.IndexOf(ColumnNames, "leftEyeLocalForward_Y")] = data.left.forward.y.ToString();
        logData[Array.IndexOf(ColumnNames, "leftEyeLocalForward_Z")] = data.left.forward.z.ToString();
        logData[Array.IndexOf(ColumnNames, "leftEyePupilDiameter")] = data.leftPupilSize.ToString();
        logData[Array.IndexOf(ColumnNames, "leftEyePupilPositionInSensorArea_X")] = "0";
        logData[Array.IndexOf(ColumnNames, "leftEyePupilPositionInSensorArea_Y")] = "0";
        logData[Array.IndexOf(ColumnNames, "leftEyePupilPosition_X")] = "0";
        logData[Array.IndexOf(ColumnNames, "leftEyePupilPosition_Y")] = "0";
        logData[Array.IndexOf(ColumnNames, "leftEyeOpenness")] = (data.leftStatus == VarjoEyeTracking.GazeEyeStatus.Tracked ? "1" : "0");
        logData[Array.IndexOf(ColumnNames, "leftEyeOpennessReadSuccess")] = (data.leftStatus != VarjoEyeTracking.GazeEyeStatus.Invalid).ToString();
        logData[Array.IndexOf(ColumnNames, "leftEyeFrown")] = "0";
        logData[Array.IndexOf(ColumnNames, "leftEyeSqueeze")] = "0";
        logData[Array.IndexOf(ColumnNames, "leftEyeWide")] = "0";
        logData[Array.IndexOf(ColumnNames, "isLeftEyeGazeRayValid")] = (data.leftStatus != VarjoEyeTracking.GazeEyeStatus.Invalid).ToString();

        logData[Array.IndexOf(ColumnNames, "rightEyeLocalOrigin_X")] = data.right.origin.x.ToString();
        logData[Array.IndexOf(ColumnNames, "rightEyeLocalOrigin_Y")] = data.right.origin.y.ToString();
        logData[Array.IndexOf(ColumnNames, "rightEyeLocalOrigin_Z")] = data.right.origin.z.ToString();
        logData[Array.IndexOf(ColumnNames, "rightEyeLocalForward_X")] = data.right.forward.x.ToString();
        logData[Array.IndexOf(ColumnNames, "rightEyeLocalForward_Y")] = data.right.forward.y.ToString();
        logData[Array.IndexOf(ColumnNames, "rightEyeLocalForward_Z")] = data.right.forward.z.ToString();
        logData[Array.IndexOf(ColumnNames, "rightEyePupilDiameter")] = data.rightPupilSize.ToString();
        logData[Array.IndexOf(ColumnNames, "rightEyePupilPositionInSensorArea_X")] = "0";
        logData[Array.IndexOf(ColumnNames, "rightEyePupilPositionInSensorArea_Y")] = "0";
        logData[Array.IndexOf(ColumnNames, "rightEyePupilPosition_X")] = "0";
        logData[Array.IndexOf(ColumnNames, "rightEyePupilPosition_Y")] = "0";
        logData[Array.IndexOf(ColumnNames, "rightEyeOpenness")] = (data.rightStatus == VarjoEyeTracking.GazeEyeStatus.Tracked ? "1" : "0");
        logData[Array.IndexOf(ColumnNames, "rightEyeOpennessReadSuccess")] = (data.rightStatus != VarjoEyeTracking.GazeEyeStatus.Invalid).ToString();
        logData[Array.IndexOf(ColumnNames, "rightEyeFrown")] = "0";
        logData[Array.IndexOf(ColumnNames, "rightEyeSqueeze")] = "0";
        logData[Array.IndexOf(ColumnNames, "rightEyeWide")] = "0";
        logData[Array.IndexOf(ColumnNames, "isRightEyeGazeRayValid")] = (data.rightStatus != VarjoEyeTracking.GazeEyeStatus.Invalid).ToString();

        logData[Array.IndexOf(ColumnNames, "hmdWorldOrigin_X")] = hmdWorldOrigin.x.ToString();
        logData[Array.IndexOf(ColumnNames, "hmdWorldOrigin_Y")] = hmdWorldOrigin.y.ToString();
        logData[Array.IndexOf(ColumnNames, "hmdWorldOrigin_Z")] = hmdWorldOrigin.z.ToString();
        logData[Array.IndexOf(ColumnNames, "hmdWorldForward_X")] = hmdWorldForward.x.ToString();
        logData[Array.IndexOf(ColumnNames, "hmdWorldForward_Y")] = hmdWorldForward.y.ToString();
        logData[Array.IndexOf(ColumnNames, "hmdWorldForward_Z")] = hmdWorldForward.z.ToString();
        logData[Array.IndexOf(ColumnNames, "hmdWorldQuaternion_W")] = hmdWorldQuaternion.w.ToString();
        logData[Array.IndexOf(ColumnNames, "hmdWorldQuaternion_X")] = hmdWorldQuaternion.x.ToString();
        logData[Array.IndexOf(ColumnNames, "hmdWorldQuaternion_Y")] = hmdWorldQuaternion.y.ToString();
        logData[Array.IndexOf(ColumnNames, "hmdWorldQuaternion_Z")] = hmdWorldQuaternion.z.ToString();

        logData[Array.IndexOf(ColumnNames, "hmdL2W_m00")] = cameraTransformLocalToWorldMatrix.m00.ToString();
        logData[Array.IndexOf(ColumnNames, "hmdL2W_m01")] = cameraTransformLocalToWorldMatrix.m01.ToString();
        logData[Array.IndexOf(ColumnNames, "hmdL2W_m02")] = cameraTransformLocalToWorldMatrix.m02.ToString();
        logData[Array.IndexOf(ColumnNames, "hmdL2W_m03")] = cameraTransformLocalToWorldMatrix.m03.ToString();
        logData[Array.IndexOf(ColumnNames, "hmdL2W_m10")] = cameraTransformLocalToWorldMatrix.m10.ToString();
        logData[Array.IndexOf(ColumnNames, "hmdL2W_m11")] = cameraTransformLocalToWorldMatrix.m11.ToString();
        logData[Array.IndexOf(ColumnNames, "hmdL2W_m12")] = cameraTransformLocalToWorldMatrix.m12.ToString();
        logData[Array.IndexOf(ColumnNames, "hmdL2W_m13")] = cameraTransformLocalToWorldMatrix.m13.ToString();
        logData[Array.IndexOf(ColumnNames, "hmdL2W_m20")] = cameraTransformLocalToWorldMatrix.m20.ToString();
        logData[Array.IndexOf(ColumnNames, "hmdL2W_m21")] = cameraTransformLocalToWorldMatrix.m21.ToString();
        logData[Array.IndexOf(ColumnNames, "hmdL2W_m22")] = cameraTransformLocalToWorldMatrix.m22.ToString();
        logData[Array.IndexOf(ColumnNames, "hmdL2W_m23")] = cameraTransformLocalToWorldMatrix.m23.ToString();
        logData[Array.IndexOf(ColumnNames, "hmdL2W_m30")] = cameraTransformLocalToWorldMatrix.m30.ToString();
        logData[Array.IndexOf(ColumnNames, "hmdL2W_m31")] = cameraTransformLocalToWorldMatrix.m31.ToString();
        logData[Array.IndexOf(ColumnNames, "hmdL2W_m32")] = cameraTransformLocalToWorldMatrix.m32.ToString();
        logData[Array.IndexOf(ColumnNames, "hmdL2W_m33")] = cameraTransformLocalToWorldMatrix.m33.ToString();

        logData[Array.IndexOf(ColumnNames, "headVel_Az")] = headVel_Az.ToString();
        logData[Array.IndexOf(ColumnNames, "headVel_Pol")] = headVel_Pol.ToString();
        logData[Array.IndexOf(ColumnNames, "stimLocalPos_X")] = gameStates.holeLocalPos_X.ToString();
        logData[Array.IndexOf(ColumnNames, "stimLocalPos_Y")] = gameStates.holeLocalPos_Y.ToString();
        logData[Array.IndexOf(ColumnNames, "stimLocalPos_Z")] = gameStates.holeLocalPos_Z.ToString();

        Log(logData);
    }

    static void Log(string[] values)
    {
        if (!logging || writer == null) return;
        string line = string.Join(";", values);
        writer.WriteLine(line);
    }

    public void StartLogging()
    {
        if (logging) return;
        logging = true;
        string logPath = "D:/houb3/eyeTrackerEvaluation/data/" + saveFolder + "/";
        Directory.CreateDirectory(logPath);
        DateTime now = DateTime.Now;
        string fileName = string.Format("dataLog_{0}-{1:00}-{2:00}-{3:00}-{4:00}", now.Year, now.Month, now.Day, now.Hour, now.Minute);
        string path = logPath + fileName + ".csv";
        writer = new StreamWriter(path);
        Log(ColumnNames);
        Debug.Log("Log file started at: " + path);
    }

    void StopLogging()
    {
        if (!logging) return;
        if (writer != null)
        {
            writer.Flush();
            writer.Close();
            writer = null;
        }
        logging = false;
        Debug.Log("Logging ended");
    }

    private void OnDisable() => StopLogging();
    private void OnApplicationQuit() => StopLogging();

    Vector2 vec2ficks(float x, float y, float z)
    {
        float r = Mathf.Sqrt(x * x + y * y + z * z);
        float az = Mathf.Atan2(x, z);
        float pol = Mathf.Acos(y / r);
        return new Vector2(rad2deg(az), 90 - rad2deg(pol));
    }

    float rad2deg(float rad) => (180 / Mathf.PI) * rad;
}