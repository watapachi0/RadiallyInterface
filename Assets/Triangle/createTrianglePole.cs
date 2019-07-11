using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class createTrianglePole : MonoBehaviour {

    public Material material;
    private int poleSum = 6;
    private float radius = 2f;
    private float poleHeight = 2f;

    void Start() {
        GameObject obj;
        for (int i = 0; i < poleSum; i++) {
            obj = new GameObject(i.ToString());
            TrianglePole trianglePole = obj.AddComponent<TrianglePole>();
            trianglePole.SetPoleNums(i, poleSum, radius, poleHeight);
            trianglePole._material = material;
        }
    }

    void Update() {

    }
}
