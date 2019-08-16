using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SystemKeyPole : MonoBehaviour {

    private int myNum;

    private createTrapezoidPole createSorce;
    private centralSystem systemScript;

    //表示関係
    MeshRenderer meshRenderer;

    //文字のゲームオブジェクト
    TextMesh TmeshC;
    private void Awake() {
        createSorce = GameObject.Find("central").GetComponent<createTrapezoidPole>();
        systemScript = GameObject.Find("central").GetComponent<centralSystem>();
    }
    void Start() {
        //systemKey(9文字)を頭から削除したものが自分の名前
        myNum = int.Parse(transform.name.Substring(9));
        transform.position = new Vector3(variables.systemCommandRadius * Mathf.Sin((float)myNum / variables.systemCommandNum * Mathf.PI * 2),
                                         variables.systemCommandRadius * Mathf.Cos((float)myNum / variables.systemCommandNum * Mathf.PI * 2),
                                         variables.poleHeight / 2)
                                         + variables.createSourcePosition;
        transform.localEulerAngles = new Vector3(270, 0, 0);
        transform.localScale = new Vector3(( variables.radiusOut - variables.radiusIn ), variables.poleHeight / 2, ( variables.radiusOut - variables.radiusIn ));

        meshRenderer = GetComponent<MeshRenderer>();
        //影を発生させない
        meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        //影の影響を受けない
        meshRenderer.receiveShadows = false;

        CapsuleCollider capsuleCollider = this.gameObject.GetComponent<CapsuleCollider>();
        capsuleCollider.isTrigger = true;

        if (!variables.isOnXR) {
            //暫定当たり判定用Event Trigger
            //イベントトリガーのアタッチと初期化
            EventTrigger currentTrigger = this.gameObject.AddComponent<EventTrigger>();
            currentTrigger.triggers = new List<EventTrigger.Entry>();
            //イベントトリガーのトリガーイベント作成
            //EventTrigger.Entry entry = new EventTrigger.Entry();
            //entry.eventID = EventTriggerType.PointerEnter;
            //entry.callback.AddListener((x) => OnTouchPointer());  //ラムダ式の右側は追加するメソッド
            //トリガーイベントのアタッチ
            //currentTrigger.triggers.Add(entry);

            /* 侵入イベント用 */
            //侵入時に色を変える
            EventTrigger.Entry entry2 = new EventTrigger.Entry();
            entry2.eventID = EventTriggerType.PointerEnter;
            entry2.callback.AddListener((x) => OnMouseEnter());
            currentTrigger.triggers.Add(entry2);
            //侵入終了時に色を戻す
            EventTrigger.Entry entry3 = new EventTrigger.Entry();
            entry3.eventID = EventTriggerType.PointerExit;
            entry3.callback.AddListener((x) => OnMouseExit());
            currentTrigger.triggers.Add(entry3);
        }
        //テキスト表示
        make3Dtext();

        //表示するか
        meshRenderer.enabled = variables.displaySystemCommand[myNum];
        //使える状態風に表示するか
        if (variables.useSystemCommand[myNum])
            meshRenderer.material = variables.material_TrapezoidPole_Normal;
        else
            meshRenderer.material = variables.material_TrapezoidPole_Touch;

        //クリエイト元を親にする
        transform.parent = createSorce.gameObject.transform;
    }

    void Update() {
        //テキストの更新
        TmeshC.text = variables.systemCommandName[myNum];
    }

    //private void OnTouchPointer() {
    //    if (variables.useSystemCommand[myNum] && variables.displaySystemCommand[myNum])
    //        systemScript.UpdateChuringNum(-myNum);
    //}

    private void OnMouseEnter() {
        if (variables.useSystemCommand[myNum] && variables.displaySystemCommand[myNum]) {
            systemScript.UpdateChuringNum(-myNum);
            meshRenderer.material = variables.material_TrapezoidPole_Touch;
        }
    }

    private void OnMouseExit() {
        if (variables.useSystemCommand[myNum] && variables.displaySystemCommand[myNum])
            meshRenderer.material = variables.material_TrapezoidPole_Normal;
    }

    private void OnTriggerEnter(Collider other) {
        if (other.gameObject.name.Substring(2) == "index_endPointer") {
            systemScript.UpdateChuringNum(-myNum);
            meshRenderer.material = variables.material_TrapezoidPole_Touch;
        }
    }

    private void OnTriggerExit(Collider other) {
        if (other.gameObject.name.Substring(2) == "index_endPointer")
            meshRenderer.material = variables.material_TrapezoidPole_Normal;
    }

    private void make3Dtext() {
        //中心のUI表示
        GameObject textCentor = new GameObject("text");
        MeshRenderer MRC = textCentor.AddComponent<MeshRenderer>();
        TmeshC = textCentor.AddComponent<TextMesh>();
        //文字サイズ
        TmeshC.fontSize = variables.systemTextFontSize;
        //文字色
        TmeshC.color = variables.material_SystemText.color;
        //アンカー位置を中心に
        TmeshC.anchor = TextAnchor.MiddleCenter;
        //真ん中寄せ
        TmeshC.alignment = TextAlignment.Center;
        //表示文字(更新されればError表示は消えるはず)
        TmeshC.text = "Error";
        //位置調整（台形の中心に設定）
        TmeshC.transform.position = transform.position;
        //大きさ
        textCentor.transform.localScale = new Vector3(0.001f, 0.001f, 0.001f);
        //子オブジェクトに設定
        textCentor.transform.parent = this.transform;
    }
}
