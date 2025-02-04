using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Oculus.Interaction;
using UnityEngine.Video;

public class VideoHandler : MonoBehaviour
{
    public Camera sceneCamera;
    private Vector3 targetPosition;
    private Quaternion targetRotation;
    private float step;

    public GameObject anchorPrefab;


    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        //step = 5.0f * Time.deltaTime;
        OVRInput.Update();
        OVRInput.FixedUpdate();
        //Debug.Log(OVRInput.GetLocalControllerPosition(OVRInput.Controller.LHand));

        if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch))
        {
            CreateSpacialAnchor();
        }
    }

    public void CreateSpacialAnchor()
    {
        GameObject prefab = Instantiate(anchorPrefab, OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch), OVRInput.GetLocalControllerRotation(OVRInput.Controller.RTouch));
        prefab.AddComponent<OVRSpatialAnchor>();
    }
}
