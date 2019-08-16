using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * それぞれの指にポインタ―を付与する
 */

public class createPointer : MonoBehaviour {

    //private GameObject[,,] fingers;
    //2cm角の球体
    private Vector3 pointerScale = new Vector3(0.02f, 0.02f, 0.02f);

    /* LoPoly Rigged Hand 用 */
    private string[] LandR = new string[2] { "L", "R" };
    private string[] fingerName = new string[6] { "Palm", "index", "middle", "pinky", "ring", "thumb" };
    private string[] fingerJoint = new string[5] { "meta", "a", "b", "c", "end" };
    /* LoPoly Rigged Hand 用 終 */

    private void Awake() {
        GameObject[,,] fingers = new GameObject[LandR.Length, fingerName.Length, fingerJoint.Length];
        for (int LR = 0; LR < LandR.Length; LR++) {
            GameObject hand;
            //リグを取得
            hand = GameObject.Find("Leap Rig");
            //リグの下の手を取得
            hand = hand.transform.Find("Hand Models").gameObject;
            if (LR == 0) {
                hand = hand.transform.Find("LoPoly Rigged Hand Left").gameObject;
            } else {
                hand = hand.transform.Find("LoPoly Rigged Hand Right").gameObject;
            }
            //ローポリハンドのの下の腕を取得
            hand = hand.transform.Find(LandR[LR] + "_Wrist").gameObject;
            //腕の下の手のひらを取得
            hand = hand.transform.Find(LandR[LR] + "_Palm").gameObject;

            /* 以下一行バージョン */
            //if (LR == 0)
            //    hand = GameObject.Find("Leap Rig").transform.Find("Hand Models").transform.Find("LoPoly Rigged Hand Left").transform.Find(LandR[LR] + "_Wrist").transform.Find(LandR[LR] + "_Palm").gameObject;
            //else
            //    hand = GameObject.Find("Leap Rig").transform.Find("Hand Models").transform.Find("LoPoly Rigged Hand Right").transform.Find(LandR[LR] + "_Wrist").transform.Find(LandR[LR] + "_Palm").gameObject;
            /* 以下一行バージョン 終 */
            
            //行列に保存
            fingers[LR, 0, 0] = hand;

            for (int Name = 1; Name < fingerName.Length; Name++) {
                GameObject fingerParent = fingers[LR, 0, 0];
                for (int Joint = 0; Joint < fingerJoint.Length; Joint++) {
                    //L_thumb_cとR_thumb_cは存在しないので飛ばす
                    if (fingerName[Name] == "thumb" && fingerJoint[Joint] == "c")
                        Joint++;

                    //目的の部分を見つける
                    GameObject target = fingerParent.transform.Find(LandR[LR] + "_" + fingerName[Name] + "_" + fingerJoint[Joint]).gameObject;
                    //マーカー用球体を作る
                    GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    //大きさ
                    sphere.transform.localScale = pointerScale;
                    //場所は目的部分のところ
                    sphere.transform.position = target.transform.position;
                    //部分を親に
                    sphere.transform.parent = target.transform;
                    //名前は親にちなむ
                    sphere.name = target.name + "Pointer";
                    //当たり判定用に
                    Rigidbody rigidbody = sphere.AddComponent<Rigidbody>();
                    //重力は消す
                    rigidbody.useGravity = false;
                    //回転や移動もなし
                    rigidbody.constraints = RigidbodyConstraints.FreezeAll;
                    //今の目的部分を次の親にする
                    fingerParent = target;
                }
            }
        }
    }

    void Start() {
        //１フレーム目で本スクリプトを削除
        Destroy(this);
    }

    void Update() {

    }
}
