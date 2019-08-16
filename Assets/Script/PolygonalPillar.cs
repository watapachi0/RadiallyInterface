using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/*
 * 
 * オブジェクトの描画と各種コンポーネントのアタッチのみ行う
 * 多角柱
 * 
 * 
 */

public class PolygonalPillar : MonoBehaviour {

    private centralSystem systemScript;

    //面情報
    int[] face;

    private void Awake() {
        systemScript = GameObject.Find("central").GetComponent<centralSystem>();
    }

    void Start() {
        //メッシュ作成
        Mesh mesh = new Mesh();
        //メッシュリセット
        mesh.Clear();
        //メッシュへの頂点情報の追加
        mesh.vertices = variables.polygonalPillarVertex;
        //メッシュへの面情報の追加
        SetFace();
        mesh.triangles = face;

        //メッシュフィルター追加
        MeshFilter mesh_filter = new MeshFilter();
        mesh_filter = this.gameObject.AddComponent<MeshFilter>();
        //メッシュアタッチ
        mesh_filter.mesh = mesh;
        //レンダラー追加 + マテリアルアタッチ
        MeshRenderer meshRenderer = this.gameObject.AddComponent<MeshRenderer>();
        meshRenderer.material = variables.material_PolygonalPillar;
        //コライダーアタッチ
        MeshCollider meshCollider = this.gameObject.AddComponent<MeshCollider>();
        meshCollider.sharedMesh = mesh;
        meshCollider.convex = true;
        meshCollider.isTrigger = true;

        //NormalMapの再計算
        mesh_filter.mesh.RecalculateNormals();

        if (!variables.isOnXR) {
            //当たり判定用 Event Trigger
            //イベントトリガーのアタッチと初期化
            EventTrigger currentTrigger = this.gameObject.AddComponent<EventTrigger>();
            currentTrigger.triggers = new List<EventTrigger.Entry>();
            //イベントトリガーのトリガーイベント作成
            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerEnter;
            entry.callback.AddListener((x) => OnTouchPointer());  //ラムダ式の右側は追加するメソッド
                                                                  //トリガーイベントのアタッチ
            currentTrigger.triggers.Add(entry);
        }

        //クリエイト元を親にする
        transform.parent = systemScript.gameObject.transform;
    }

    //何個のオブジェクト中の何番目のオブジェクトか
    private void SetFace() {
        //底面それぞれ3頂点+(ポリゴン二枚なので)側面6頂点 * 角数 * 分割数
        face = new int[12 * variables.poleSum * variables.trapezoidDivisionNum];
        for (int i = 0; i < variables.poleSum * variables.trapezoidDivisionNum; i++) {
            //以下で、poleSum分割の三角柱ができる
            face[i * 12 + 0] = 8 * i + 2;  //側面1
            face[i * 12 + 1] = 8 * i + 1;  //
            face[i * 12 + 2] = 8 * i + 0;  //
            face[i * 12 + 3] = 8 * i + 1;  //側面2
            face[i * 12 + 4] = 8 * i + 2;  //
            face[i * 12 + 5] = 8 * i + 3;  //
            face[i * 12 + 6] = 8 * i + 4;  //底面1
            face[i * 12 + 7] = 8 * i + 5;  //
            face[i * 12 + 8] = 8 * variables.poleSum * variables.trapezoidDivisionNum + i + 1 - 1;   //底面1の角部分
            face[i * 12 + 9] = 8 * i + 7;   //底面2
            face[i * 12 + 10] = 8 * i + 6;  //
            face[i * 12 + 11] = 9 * variables.poleSum * variables.trapezoidDivisionNum + i + 1 - 1;  //底面2の角部分
        }
    }

    private void OnTouchPointer() {
        systemScript.UpdateChuringNum(int.Parse(gameObject.name));
    }

    private void OnTriggerEnter(Collider other) {
        if (other.gameObject.name.Substring(2) == "index_endPointer")
            systemScript.UpdateChuringNum(int.Parse(gameObject.name));
    }
}
