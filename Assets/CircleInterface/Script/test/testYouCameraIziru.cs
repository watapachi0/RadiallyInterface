using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testYouCameraIziru : MonoBehaviour {

    public GameObject mainCamera;
    public GameObject leapCamera;
    public Vector3 mainPosition;
    public Vector3 leapPosition;
    public Vector3 leapRotation;
    private int flg = 0;
    void Start() {
        mainCamera.GetComponent<Camera>().targetDisplay = 0;
        leapCamera.GetComponent<Camera>().targetDisplay = 1;
        leapCamera.GetComponent<AudioListener>().enabled = false;
        leapCamera.transform.eulerAngles = leapRotation;
    }

    void Update() {
        if (flg==0) {
            mainCamera.GetComponent<Camera>().targetDisplay = 2;
            leapCamera.GetComponent<Camera>().targetDisplay = 0;
            flg = 1;
        } else if(flg==1) {
            leapCamera.GetComponent<Camera>().targetDisplay = 1;
            mainCamera.GetComponent<Camera>().targetDisplay = 0;
            flg = 2;
        }
        mainCamera.transform.position = mainPosition;
        leapCamera.transform.position = leapPosition;
    }
}
