using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testCylinderBetweenTwoPoint : MonoBehaviour {
    [SerializeField]
    private Transform cylinderPrefab;

    private GameObject leftSphere;
    private GameObject rightSphere;
    private GameObject cylinder;

    private GameObject camera;
    private Vector3 objPos;
    private Vector3 cameraPos;

    private void Start() {
        leftSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        rightSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        leftSphere.transform.position = new Vector3(-1, 0, 0);
        rightSphere.transform.position = new Vector3(1, 0, 0);
        leftSphere.transform.localScale = Vector3.one * 0.01f;
        rightSphere.transform.localScale = Vector3.one * 0.01f;
        Destroy(leftSphere.GetComponent<SphereCollider>());
        Destroy(rightSphere.GetComponent<SphereCollider>());
        Destroy(leftSphere.GetComponent<MeshRenderer>());
        Destroy(rightSphere.GetComponent<MeshRenderer>());

        InstantiateCylinder(cylinderPrefab, leftSphere.transform.position, rightSphere.transform.position);

        //カメラの位置を取得
        if (variables.isOnXR) {
            camera = GameObject.Find("Leap Rig").transform.Find("Main Camera").gameObject;
        } else {
            camera = GameObject.Find("Main CameraNonVR");
        }
    }

    private void Update() {
        //マウスの座標を取得
        Vector3 mouse = Input.mousePosition;
        mouse.y = -camera.transform.position.y+mouse.y * 2f;
        mouse.z = -Mathf.Abs(camera.transform.position.z);
        objPos = Camera.main.ScreenToWorldPoint(mouse);
        Debug.Log("マウス座標" + objPos);
        //カメラ座標を取得
        //cameraPos = objPos;
        //cameraPos.z = camera.transform.position.z;
        cameraPos = camera.transform.position;

        leftSphere.transform.position = cameraPos;
        rightSphere.transform.position =objPos;

        UpdateCylinderPosition(cylinder, leftSphere.transform.position, rightSphere.transform.position);
    }

    private void InstantiateCylinder(Transform cylinderPrefab, Vector3 beginPoint, Vector3 endPoint) {
        cylinder = Instantiate<GameObject>(cylinderPrefab.gameObject, Vector3.zero, Quaternion.identity);
        //Destroy(cylinder.GetComponent<MeshRenderer>());
        cylinder.AddComponent<fingerFeelCollider>();
        MeshCollider mc= cylinder.AddComponent<MeshCollider>();
        mc.convex = true;
        mc.isTrigger = true;
        cylinder.transform.localScale = Vector3.one * 0.01f;
        UpdateCylinderPosition(cylinder, beginPoint, endPoint);
    }

    private void UpdateCylinderPosition(GameObject cylinder, Vector3 beginPoint, Vector3 endPoint) {
        Vector3 offset = endPoint - beginPoint;
        Vector3 position = beginPoint + ( offset / 2.0f );

        cylinder.transform.position = position;
        cylinder.transform.LookAt(beginPoint);
        Vector3 localScale = cylinder.transform.localScale;
        localScale.z = ( endPoint - beginPoint ).magnitude;
        cylinder.transform.localScale = localScale;
    }
}