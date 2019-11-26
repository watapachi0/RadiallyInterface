using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateCubeKey : MonoBehaviour {

    centralSystem systemScript;

    void Start() {
        systemScript = GetComponent<centralSystem>();
        GameObject obj;
        for (int m = 0; m < systemScript.textSet.GetLength(0); m++) {
            for(int n = 0; n < systemScript.textSet.GetLength(1); n++) {
                obj = new GameObject(m.ToString() + n.ToString("D2"));
                CubeKey CK = obj.AddComponent<CubeKey>();
                CK.myPosition = new Vector3(keyPosX1(n), keyPosY1(m), 0);
            }
        }
    }

    void Update() {

    }

    private float keyPosX1(int n) {
        float x = variables.cubeWidth;
        float dim1 = systemScript.textSet.GetLength(1);
        float d = variables.cubesIntervalX;
        return -( x * dim1 + d * ( dim1 - 1 ) ) / 2f + ( x + d ) * n;
    }

    private float keyPosX2(int n) {
        float x = variables.cubeWidth;
        float dim1 = systemScript.textSet.GetLength(1);
        float d = variables.cubesIntervalX;
        return ( x + d ) * ( -( dim1 - 1 ) / 2f + n ) - x / 2f;
    }

    private float keyPosY1(int m) {
        float y = variables.cubeVertical;
        float dim0 = systemScript.textSet.GetLength(0);
        float d = variables.cubesIntervalY;
        return ( y * ( dim0 - 1 ) + d * ( dim0 - 1 ) ) / 2f - ( y + d ) * m;
    }

    private float keyPosY2(int m) {
        float y = variables.cubeVertical;
        float dim0 = systemScript.textSet.GetLength(0);
        float d = variables.cubesIntervalY;
        return ( y + d ) * ( ( dim0 - 1 ) / 2f - m );
    }
}
