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
        this.gameObject.AddComponent<MeshFilter>();
        mesh_filter = GetComponent<MeshFilter>();
        //メッシュアタッチ
        mesh_filter.mesh = mesh;
        //レンダラー追加 + マテリアルアタッチ
        this.gameObject.AddComponent<MeshRenderer>();
        this.gameObject.GetComponent<MeshRenderer>().material = variables.material_PolygonalPillar;
        //コライダーアタッチ
        this.gameObject.AddComponent<MeshCollider>();
        this.gameObject.GetComponent<MeshCollider>().sharedMesh = mesh;

        //NormalMapの再計算
        mesh_filter.mesh.RecalculateNormals();

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

    //何個のオブジェクト中の何番目のオブジェクトか
    private void SetFace() {
        face = new int[variables.poleSum * 12];
        for (int i = 0; i < variables.poleSum; i++) {
            face[i * 12 + 0] = 8 * i + 2;
            face[i * 12 + 1] = 8 * i + 1;
            face[i * 12 + 2] = 8 * i + 0;
            face[i * 12 + 3] = 8 * i + 1;
            face[i * 12 + 4] = 8 * i + 2;
            face[i * 12 + 5] = 8 * i + 3;
            face[i * 12 + 6] = 8 * i + 4;
            face[i * 12 + 7] = 8 * i + 5;
            face[i * 12 + 8] = 8 * variables.poleSum + i + 1 - 1;
            face[i * 12 + 9] = 8 * i + 7;
            face[i * 12 + 10] = 8 * i + 6;
            face[i * 12 + 11] = 9 * variables.poleSum + i + 1 - 1;
        }
    }

    private void OnTouchPointer() {
        systemScript.UpdateChuringNum(int.Parse(gameObject.name));
    }
}
