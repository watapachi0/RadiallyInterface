using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class circleSystem : MonoBehaviour {

    private int cubeNum = 12;    //配置するキューブの数
    private float radius = 5;   //配置する円の半径
    private Vector3 wasCubePoint = Vector3.zero;    //配置オブジェクトの左側下角座標
    private Vector3 willCubePoint = Vector3.zero;   //配置オブジェクトの右側下角座標
    private bool isStartObjectPointTop = false;     //初期のオブジェクトの配置を真上真ん中にするか？でなければゼロ回転から計算する
    private float centorCornar = 0;     //中心角
    private float sumCentorCornar = 0;  //計算のために必要なこれまでの角度
    private float cubeKatamuki = 0;     //Cubeに傾向いてほしい角度
    private float cubeScaleWidth = 2;   //Cubeの幅
    private float cubeScaleHeight = 1;  //Cubeの高さ
    public GameObject point;
    private int Stage = 0;
    private centoralSystem system;
    private int parentNum = 0;

    void Start() {
        if (Stage == 0)
            SetSystemPass(1);
        centorCornar = 360f / cubeNum;
        if (/*!isStartObjectPointTop*/true) {
            //キューブの初期位置
            wasCubePoint = new Vector3(transform.position.x,
                                       transform.position.y + radius,
                                       transform.position.z);
        }
        for (int i = 0; i < cubeNum; i++) {
            //Cubeの右下の位置の計算
            willCubePoint = new Vector3(radius * Mathf.Sin(( sumCentorCornar + centorCornar ) / 360f * Mathf.PI * 2) + transform.position.x,
                                        radius * Mathf.Cos(( sumCentorCornar + centorCornar ) / 360f * Mathf.PI * 2) + transform.position.y,
                                        transform.position.z);
            //Cubeの中心座標の計算
            Vector3 distance = new Vector3(Mathf.Sin(( sumCentorCornar / 360 + centorCornar / 360 / 2 ) * Mathf.PI * 2) * ( radius * Mathf.Cos(centorCornar / 360 / 2 * Mathf.PI * 2) + cubeScaleHeight / 2 ),
                                           Mathf.Cos(( sumCentorCornar / 360 + centorCornar / 360 / 2 ) * Mathf.PI * 2) * ( radius * Mathf.Cos(centorCornar / 360 / 2 * Mathf.PI * 2) + cubeScaleHeight / 2 ),
                                           transform.position.z);
            //Cube幅確定
            cubeScaleWidth = Vector3.Distance(willCubePoint, wasCubePoint);
            //Cubeの傾き[rad]
            cubeKatamuki = Mathf.Atan2(wasCubePoint.y - willCubePoint.y, wasCubePoint.x - willCubePoint.x);
            //Cube生成
            GameObject cube = Instantiate(point, distance + transform.position, transform.rotation);
            //Cubeを傾ける
            cube.transform.eulerAngles = new Vector3(0, 0, cubeKatamuki * 180 / Mathf.PI);
            //Cubeのサイズ変更
            cube.transform.localScale = new Vector3(cubeScaleWidth, cubeScaleHeight, 1);
            //リネーム
            cube.name = i.ToString();
            //自分の子にする
            cube.transform.parent = this.gameObject.transform;
            //段階を与える
            if (Stage == 1) {
                cube.GetComponent<pointSystem>().SetSystemPass(1);
            } else if (Stage == 2) {
                cube.GetComponent<pointSystem>().SetSystemPass(2);
            }
            //後処理
            wasCubePoint = willCubePoint;
            sumCentorCornar += centorCornar;

        }
    }

    void Update() {

    }

    //他オブジェクトからの操作時に使用
    public void SetSystemPass(int todo) {
        if (todo == 1) {
            Stage = 1;      //一番内側
            system = GameObject.Find("SystemObject").gameObject.GetComponent<centoralSystem>();
            cubeNum = system.textSet.GetLength(0);
        } else if (todo == 2) {
            Stage = 2;      //次のやつ
            system = GameObject.Find("SystemObject").gameObject.GetComponent<centoralSystem>();
            cubeNum = int.Parse(system.textSet[parentNum, 1]);
            system.SetStage2Object(this.gameObject);
        }
    }

    //あ～お、か～こ、a~eなどの大まかなセットを行う
    public void SetKeysetNum() {

    }

    //Stage2用各pointにkey効果を割り当てる
    public void SetPointEffect() {

    }

    public void SetParentNum(int Num) {
        parentNum = Num;
    }
    public int GetParentNum() {
        return parentNum;
    }
}
