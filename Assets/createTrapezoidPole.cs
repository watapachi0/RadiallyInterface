using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class createTrapezoidPole : MonoBehaviour {

    public Material material;
    private int poleSum = 6;
    private float radiusOut = 4f;
    private float radiusIn = 2f;
    private float poleHeight = 2f;

    void Start() {
        GameObject obj;
        for (int i = 0; i < poleSum; i++) {
            obj = new GameObject(i.ToString());
            TrapezoidPole trianglePole = obj.AddComponent<TrapezoidPole>();
            trianglePole.SetPoleNums(i, poleSum, radiusOut, radiusIn, poleHeight);
            trianglePole._material = material;
        }
    }

    void Update() {

    }
}
