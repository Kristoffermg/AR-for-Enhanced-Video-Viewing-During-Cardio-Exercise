using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Video;

public class VideoScript : MonoBehaviour
{
    public AudioSource audio;
    public VideoPlayer video;

    public GameObject canvas;
    public GameObject camera;

    public GameObject centerEye;

    void Start()
    {
        video.Play();
    }

    Vector3 previousVideoPosition = Vector3.zero;
    float previousHeadsetHeight = 0.0f;

    List<string> headPositionData = new List<string>();
    int currentFrame = 0;

    void Update()
    {
        OVRInput.Update();
        OVRInput.FixedUpdate();

        Vector3 cameraPosition = camera.transform.position;
        Vector3 centerEyePosition = centerEye.transform.position;


        canvas.transform.LookAt(camera.transform);
        canvas.transform.Rotate(0, 180, 0);

        Vector3 newVideoPosition = previousVideoPosition;

        if (OVRInput.Get(OVRInput.RawButton.LIndexTrigger))
        {
            Vector3 leftControllerPosition = OVRInput.GetLocalControllerPosition(OVRInput.Controller.LHand);
            leftControllerPosition.y += 0.2f;
            //leftControllerPosition.z += 0.2f;
            newVideoPosition = leftControllerPosition;
        }

        //newVideoPosition.y = centerEyePosition.y - 0.04f;
        float heightDifference = previousHeadsetHeight - centerEyePosition.y;
        newVideoPosition.y -= heightDifference;

        canvas.transform.position = newVideoPosition;

        previousVideoPosition = newVideoPosition;
        previousHeadsetHeight = centerEyePosition.y;


        if (OVRInput.Get(OVRInput.RawButton.RIndexTrigger))
        {
            currentFrame++;
            headPositionData.Add($"{currentFrame},{centerEyePosition.x},{centerEyePosition.y},{centerEyePosition.z}");
        }

        if(OVRInput.Get(OVRInput.RawButton.B))
        {
            using (StreamWriter writer = new StreamWriter("C:/Users/krist/Documents/repos/headPositionData.csv"))
            {
                writer.WriteLine("frame,x,y,z");

                foreach(string row in headPositionData)
                {
                    writer.WriteLine(row);
                }
            }
        }


        //Vector3 cameraAngle = canvas.transform.rotation.eulerAngles;
        //cameraAngle[2] = canvas.transform.rotation.eulerAngles[2];

            //Quaternion canvasRotation = canvas.transform.rotation;
            //canvasRotation.eulerAngles = cameraAngle;

            //canvas.transform.rotation = canvasRotation;
    }
}
