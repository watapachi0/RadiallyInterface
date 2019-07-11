using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using UnityEngine;

public class centoralSystem : MonoBehaviour {

    /*[親のpointNum,set]*/
    /* set = "stage1用の見出し","要素数+1",要素1"a",要素2"b", ... 要素n+1"Cancel",番外"Error" */
    public readonly string[,] textSet = new string[8, 8] { { "abc",  "4", "a", "b", "c", "Cancel", "Error",  "Error" },
                                                           { "def",  "4", "d", "e", "f", "Cancel", "Error",  "Error" },
                                                           { "ghi",  "4", "g", "h", "i", "Cancel", "Error",  "Error" },
                                                           { "jkl",  "4", "j", "k", "l", "Cancel", "Error",  "Error" },
                                                           { "mno",  "4", "m", "n", "o", "Cancel", "Error",  "Error" },
                                                           { "pqrs", "5", "p", "q", "r", "s",      "Cancel", "Error" },
                                                           { "tuv",  "4", "t", "u", "v", "Cancel", "Error",  "Error" },
                                                           { "wxyz", "5", "w", "x", "y", "z",      "Cancel", "Error" } };
    //イベント座標保持用 暫定的に100でnull扱い
    private int dimension1 = 100;
    private int dimension2 = 100;
    private GameObject stage2object;//削除用

    void Start() {

    }

    void Update() {

    }

    //pointからのセット内容返信用
    public string getSringData(int stage, int pointNum/*,int setNum大文字小文字など*/) {
        if (textSet.GetLength(0) > stage && textSet.GetLength(1) > pointNum) {
            return textSet[stage, pointNum];
        } else {
            //debug
            return "debug text";
        }
    }

    //Circleオブジェクトからすべてのpointを取得・停止する
    public void StopAllPointFromCircleChildren(GameObject parentCircle) {
        int i = 0;
        GameObject childPoint = parentCircle.transform.Find(i.ToString()).gameObject;
        while (childPoint/*オブジェクトの存在を確認*/) {
            childPoint.GetComponent<pointSystem>().SetActive(false);

            i++;
            if (parentCircle.transform.Find(i.ToString()) == null)
                break;
            else
                childPoint = parentCircle.transform.Find(i.ToString()).gameObject;
        }
    }

    public void SetDimensionNum(int d1, int d2) {
        dimension1 = d1;
        dimension2 = d2;
    }

    //ニュートラルエリアにポインタが侵入した際のイベント
    public void GetNeutralEvent() {
        if (dimension1 <= textSet.GetLongLength(0) && dimension2 <= textSet.GetLongLength(1)) {
            Debug.Log(textSet[dimension1, dimension2]);
            if (textSet[dimension1, dimension2].Length == 1) {
                SendKeys.Send(textSet[dimension1, dimension2]);
            }
            Destroy(stage2object);
            GameObject tmpGO = GameObject.Find("CircleCentor");
            for (int i = 0; i < textSet.GetLength(0); i++) {
                tmpGO.transform.Find(i.ToString()).GetComponent<pointSystem>().SetActive(true);
            }
        } else {
            Debug.LogWarning("dimension error");
        }
        dimension1 = 100;
        dimension2 = 100;
    }

    public void SetStage2Object(GameObject obj) {
        stage2object = obj;
    }
}
