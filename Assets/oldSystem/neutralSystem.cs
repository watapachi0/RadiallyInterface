using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class neutralSystem : MonoBehaviour {

    private centoralSystem system;

    void Start() {
        system = GameObject.Find("SystemObject").gameObject.GetComponent<centoralSystem>();
    }

    void Update() {

    }

    //システムへイベントを送る
    public void OnGetMouseInEvent() {
        system.GetNeutralEvent();
    }
}
