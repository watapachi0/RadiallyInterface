using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Convexを使えない問題が発生したため、
 * 以下にてトリガーイベントを代行する、
 * スクリプトを指先に配置する
 */

public class fingerFeelColider : MonoBehaviour {

    void Start() {

    }

    void Update() {

    }

    public void OnTriggerEnter(Collider other) {
        try {
            other.gameObject.GetComponent<MultipleTrapezoidPoleC>().OnTriggerEnterOwnMade(this.gameObject);
        } catch {
            Debug.Log("指がキーではないオブジェクトに接触しました");
        }
        try {
            other.gameObject.GetComponent<PolygonalPillarC>().OnTriggerEnterOwnMade(this.gameObject);
        } catch {
            Debug.Log("指が多角柱ではないオブジェクトに接触しました");
        }
    }

    public void OnTriggerExit(Collider other) {
        try {
            other.gameObject.GetComponent<MultipleTrapezoidPoleC>().OnTriggerExitOwnMade(this.gameObject);
        } catch {
            Debug.Log("指がキーではないオブジェクトとの接触を終えました");
        }
        try {
            other.gameObject.GetComponent<PolygonalPillarC>().OnTriggerEnterOwnMade(this.gameObject);
        } catch {
            Debug.Log("指が多角柱ではないオブジェクトに接触しました");
        }

    }
}
