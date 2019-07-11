using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class pointSystem : MonoBehaviour {

    private int stage = 0;
    public GameObject CirclePrefab;
    private GameObject CircleClone;
    private string textSet = null;
    private centoralSystem system;
    private string myText;
    private bool active = true;

    void Start() {
        system = GameObject.Find("SystemObject").gameObject.GetComponent<centoralSystem>();
    }

    void Update() {

    }

    //マウスが上に乗った時の処理
    public void OnGetMouseInEvent() {
        if (active) {
            if (stage == 1) {
                //ミニサークル表示
                CircleClone = Instantiate(CirclePrefab, transform.position, CirclePrefab.transform.rotation);
                CircleClone.transform.position = transform.position;
                //ミニサークルに親の数字を教える
                CircleClone.GetComponent<circleSystem>().SetParentNum(int.Parse(transform.name));
                //ミニサークルに第二段階と教える
                CircleClone.GetComponent<circleSystem>().SetSystemPass(2);
                //本オブジェクトを薄い色にして無効化する
                //システムにstage2がいることを伝える
                //システムに本オブジェクトの親のcircleを伝えて丸ごと動作を止める
                system.StopAllPointFromCircleChildren(transform.parent.gameObject);
            } else if (stage == 2) {
                //入力処理  parentNum  int.Parse(transform.name)
                //Debug.Log(system.textSet[transform.parent.GetComponent<circleSystem>().GetParentNum(), int.Parse(transform.name) + 2]);
                system.SetDimensionNum(transform.parent.GetComponent<circleSystem>().GetParentNum(), int.Parse(transform.name) + 2);
                //システムに本オブジェクトの親のcircleを伝えて丸ごと動作を止める
                system.StopAllPointFromCircleChildren(transform.parent.gameObject);
            } else {
                //Error
            }
            Debug.Log(myText);
        }
    }

    //ニュートラルポジションにポインタが入ったとき用　もしくは　第二段階でキャンセルされた際用
    public void OnGetNutoralEvent() {
        //本オブジェクトを有効化する
    }

    //他オブジェクトからの操作時に使用
    public void SetSystemPass(int todo) {
        //一段階目か二段階目か
        if (todo == 1) {
            //本オブジェクトは1段階目
            stage = 1;
        } else if (todo == 2) {
            //本オブジェクトは2段階目
            stage = 2;
        } else {
            //Error
        }
    }

    private void defineMyText() {
        myText = system.getSringData(stage, int.Parse(transform.name)/*,int setNum大文字小文字など*/);
    }

    //このpointオブジェクトの動作を止めたり動かしたり
    public void SetActive(bool flg) {
        active = flg;
    }
}
