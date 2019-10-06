﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* 指の接触を検知し、ピンチとして動作する
 * ピンチ時には、
 * ピンチ側の手の、人差し指の指先の座標にシステムを配置する
 * ピンチ側の手の、ピンチ解除、及び存在が消えたときは座標決定
 * ピンチと反対側の手の、上下左右前後への相対移動で、ローカルオイラー回転させる
 * ピンチと反対側の手の、存在しない時は回転は一切しない（途中で表示されてもしない。途中で消えたら回転は止めたまま。）
 */

public class ObjTransRota : MonoBehaviour {

    private GameObject LThumb;
    private GameObject RThumb;
    private GameObject LIndex;
    private GameObject RIndex;

    //ピンチ判定用の相対座標の閾値
    [SerializeField]
    private float pinchDistance;

    //ピンチで操作する対象オブジェクト
    public GameObject targetObject;

    void Start() {

    }

    void Update() {
    }

    public void SetThumbAndIndex(GameObject LThumbObj, GameObject RThumbObj, GameObject LIndexObj, GameObject RIndexObj) {
        LThumb = LThumbObj;
        RThumb = RThumbObj;
        LIndex = LIndexObj;
        RIndex = RIndexObj;
    }
}