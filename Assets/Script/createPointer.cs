using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * それぞれの指にポインタ―を付与する
 * LeapMotionの描画など
 */

public class createPointer : MonoBehaviour {

    //球体を描画する
    [SerializeField]
    private bool doRenderSphere = true;
    //LeapMotionの手を描画する
    [SerializeField]
    private bool doRenderLeapHands = true;
    private SkinnedMeshRenderer[] handRenderer = new SkinnedMeshRenderer[2];

    //private GameObject[,,] fingers;
    //2cm角の球体
    private Vector3 pointerScale = new Vector3(0.02f, 0.02f, 0.02f);
    //当たり判定
    private float pointerColRad = 0.05f;

    /* LoPoly Rigged Hand 用 */
    private string[] LandR = new string[2] { "L", "R" };
    private string[] fingerName = new string[6] { "Palm", "index", "middle", "pinky", "ring", "thumb" };
    private string[] fingerJoint = new string[5] { "meta", "a", "b", "c", "end" };
    /* LoPoly Rigged Hand 用 終 */

    private GameObject[,,] fingers;
    private MeshRenderer[,,] fingersRenderer;
    //再計算が必要
    private bool needReCulc = false;

    void Start() {
        fingers = new GameObject[LandR.Length, fingerName.Length, fingerJoint.Length];
        fingersRenderer = new MeshRenderer[LandR.Length, fingerName.Length, fingerJoint.Length];
        for (int LR = 0; LR < LandR.Length; LR++) {
            GameObject hand;
            //リグを取得
            hand = GameObject.Find("Leap Rig");
            //リグの下の手を取得
            hand = hand.transform.Find("Hand Models").gameObject;
            if (LR == 0) {
                hand = hand.transform.Find("LoPoly Rigged Hand Left").gameObject;
                //手の描画の可否
                handRenderer[LR] = hand.transform.Find("LoPoly_Hand_Mesh_Left").GetComponent<SkinnedMeshRenderer>();
                handRenderer[LR].enabled = doRenderLeapHands;
            } else {
                hand = hand.transform.Find("LoPoly Rigged Hand Right").gameObject;
                //手の描画の可否
                handRenderer[LR] = hand.transform.Find("LoPoly_Hand_Mesh_Right").GetComponent<SkinnedMeshRenderer>();
                handRenderer[LR].enabled = doRenderLeapHands;
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
                    //当たり判定
                    sphere.GetComponent<SphereCollider>().radius = pointerColRad;
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
                    //配列に保存
                    fingers[LR, Name, Joint] = target;
                    fingersRenderer[LR, Name, Joint] = sphere.GetComponent<MeshRenderer>();
                    fingersRenderer[LR, Name, Joint].enabled = doRenderSphere;
                }
            }
            //指先に当たり判定スクリプトをアタッチ

            //人差指先にアタッチ
            //GameObject indexPointer = fingers[LR, 1, 4].transform.Find(fingers[LR, 1, 4].name + "Pointer").gameObject;
            //indexPointer.AddComponent<fingerFeelCollider>();
            //indexPointer.GetComponent<SphereCollider>().isTrigger = true;
        }

        //ピンチ用スクリプトの初期化
        GetComponent<ObjTransRota>().SetThumbAndIndex(fingers[0, 5, 4],
                                                      fingers[1, 5, 4],
                                                      fingers[0, 1, 4],
                                                      fingers[1, 1, 4]);
        /* 左手親指先
         * 右手親指先
         * 左手人差指先
         * 右手人差指先
         */
        variables.fingers[0] = fingers[0, 1, 4];
        variables.fingers[1] = fingers[1, 1, 4];
    }

    void Update() {
        //１フレーム目で本スクリプトを削除
        //Destroy(this);
        //再計算をする必要があるか
        if (needReCulc)
            RenderReCulc();
    }

    //値変更時に中身を再度計算
    private void OnValidate() {
        needReCulc = true;
    }

    private void RenderReCulc() {
        //エラー対策
        if (fingers == null)
            return;

        for (int LR = 0; LR < LandR.Length; LR++) {
            handRenderer[LR].enabled = doRenderLeapHands;
            for (int Name = 1; Name < fingerName.Length; Name++) {
                for (int Joint = 0; Joint < fingerJoint.Length; Joint++) {
                    //エラー対策
                    try {
                        fingersRenderer[LR, Name, Joint].enabled = doRenderSphere;
                    } catch {
                        //エラーが出たらなにもしない
                    }
                }
            }
        }
        //再計算の必要性なし
        needReCulc = false;
    }
}