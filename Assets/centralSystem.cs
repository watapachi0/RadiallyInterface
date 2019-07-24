using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using UnityEngine;

public class centralSystem : MonoBehaviour {

    private int keyNums = 6;
    private int churingNumber = -100;//各ボタンから得られるカーソル位置情報
    [SerializeField]
    private int stage = 0;//インターフェースの処理段階       -100=Error  -2=システムアイコン入力済み  -1=外縁  0=Nutral  1=子音および母音選択  2=子音決定済み母音選択  3=母音決定
    private string InputText = "";
    private string setText = "";
    private int baseNumber;
    private int consonant;  //子音

    /* システムの形決定 */
    protected int poleSum = 5;            //キーの数
    protected float radiusOut = 4f;       //システムの外縁の半径
    protected float radiusIn = 2f;        //ニュートラルエリアの半径
    protected float poleHeight = 2f;      //システムの厚み
    private TextMesh textMesh;

    /* キーを取得済みフラグ */
    private bool isGetKeyObjects = false;
    /* キーオブジェクト */                          //中身はキーを再表示する度に再設定
    [SerializeField]
    private GameObject[] keyObjects;

    /*[親のpointNum,set]*/
    /* set = "見出し","要素数+1",要素1"a",要素2"b", ... 要素n+1,番外"Error" */
    public readonly string[,] textSet = new string[15, 7] { { "k", "ka", "ki", "ku", "ke", "ko", "Error"},
                                                            { "a", "--", "--", "--", "--", "--", "Error"},
                                                            { "s", "sa", "si", "su", "se", "so", "Error"},
                                                            { "t", "ta", "ti", "tu", "te", "to", "Error"},
                                                            { "i", "--", "--", "--", "--", "--", "Error"},
                                                            { "n", "na", "ni", "nu", "ne", "no", "Error"},
                                                            { "h", "ha", "hi", "hu", "he", "ho", "Error"},
                                                            { "u", "--", "--", "--", "--", "--", "Error"},
                                                            { "m", "ma", "mi", "mu", "me", "mo", "Error"},
                                                            { "y", "ya", "--", "yu", "--", "yo", "Error"},
                                                            { "e", "--", "--", "--", "--", "--", "Error"},
                                                            { "r", "ra", "ri", "ru", "re", "ro", "Error"},
                                                            { "w", "wa", "--", "wo", "--", "nn", "Error"},
                                                            { "o", "--", "--", "--", "--", "--", "Error"},
                                                            { "-", "--", "--", "--", "--", "--", "Error"} };

    void Start() {
        textMesh = GameObject.Find("InputText").GetComponent<TextMesh>();
    }

    void Update() {
        textMesh.text = InputText;
        if (!isGetKeyObjects)
            GetKeyObjects();

    }

    public void UpdateChuringNum(int nextNum) {
        churingNumber = nextNum;
        ChuringSystem();
    }

    private void ChuringSystem() {
        if (stage == 0 && 0 < churingNumber && churingNumber <= keyNums) {
            //ニュートラル状態で、入力キー値が1～キー数の間の場合実行
            //最初のキー値を決定
            baseNumber = churingNumber;
            //とりあえず母音を保存
            setText = textSet[( baseNumber - 1 ) * 3 + 1, 0];
            //次の状態へ
            stage = 1;
            //Debug.Log("ちゅ：" + ( churingNumber - 1 ) + " 仮入力：" + setText);
            Debug.Log("stage from 0 to 1");
            if (isGetKeyObjects)
                SetKeytext();
            return;
        } else if (stage == 1 && baseNumber != churingNumber) {
            //子音および母音選択状態で、最初のキー値と入力キー値が違う場合
            //子音が決定するので計算
            consonant = ( ( baseNumber - 1 ) * 3 + 1 ) + ( churingNumber - baseNumber );
            //子音と母音から再計算
            setText = textSet[consonant, churingNumber - 1 + 1];
            //次の状態へ
            stage = 2;
            //Debug.Log("ちゅ：" + ( churingNumber - 1 ) + " 仮入力：" + setText);
            if (isGetKeyObjects)
                SetKeytext();
            return;
        } else if (stage == 2 && 0 < churingNumber && churingNumber <= keyNums) {
            //子音決定済み母音選択状態で、入力キー値が1～キー数の間の場合実行
            setText = textSet[consonant, churingNumber - 1 + 1];
            //Debug.Log("ちゅ：" + ( churingNumber - 1 ) + " 仮入力：" + setText);
            if (isGetKeyObjects)
                SetKeytext();
            return;
        } else if (( stage == 1 || stage == 2 ) && churingNumber == 0) {
            //入力状態で、中心へ戻った場合
            InputText += setText;
            setText = "";
            stage = 0;
            //Debug.Log("ちゅ：" + churingNumber + " 入力：" + InputText);
            if (isGetKeyObjects)
                SetKeytext();
            return;
        }
        Debug.LogWarning("Error. stage = " + stage + " . churingNumber = " + churingNumber + " . baseNumber = "+ baseNumber);
//        Debug.Log("Error. stage = " + stage + " . churingNumber = " + churingNumber + " . ");
    }

    private int SearchWordFromChuring(int SearchWord) {

        return 0;
    }

    //キーオブジェクトの取得
    private void GetKeyObjects() {
        keyObjects = new GameObject[poleSum + 1];
        for (int i = 0; i <= poleSum; i++) {
            if (keyObjects[i] == null) {
                if (GameObject.Find(i.ToString()))
                    keyObjects[i] = GameObject.Find(i.ToString());
                else
                    break;
            }
            if (i == poleSum) {
                isGetKeyObjects = true;
            }
        }
    }

    //キーに文字を割り当てる
    private void SetKeytext() {
        for (int i = 1; i <= poleSum; i++) {
            if (stage == 0) {
                keyObjects[i].GetComponent<TrapezoidPole>().MyText = textSet[i * 3 - 3, 0] + textSet[i * 3 - 2, 0] + textSet[i * 3 - 1, 0];
            } else if (stage == 1) {
                if (churingNumber > 1 && churingNumber < poleSum) {
                    if (churingNumber - 1 <= i && i <= churingNumber + 1)
                        keyObjects[i].GetComponent<TrapezoidPole>().MyText = textSet[i, 0];
                    else
                        keyObjects[i].GetComponent<TrapezoidPole>().MyText = "--";
                } else if (churingNumber == 1) {
                    if (churingNumber - 1 <= i && i <= churingNumber + 1)
                        if (churingNumber - 1 == i)
                            keyObjects[i].GetComponent<TrapezoidPole>().MyText = textSet[poleSum - 1, 0];
                        else
                            keyObjects[i].GetComponent<TrapezoidPole>().MyText = textSet[i, 0];
                    else
                        keyObjects[i].GetComponent<TrapezoidPole>().MyText = "--";
                } else if (churingNumber == poleSum) {
                    if (churingNumber - 1 <= i && i <= churingNumber + 1)
                        if (i == churingNumber + 1)
                            keyObjects[i].GetComponent<TrapezoidPole>().MyText = textSet[0, 0];
                        else
                            keyObjects[i].GetComponent<TrapezoidPole>().MyText = textSet[i, 0];
                    else
                        keyObjects[i].GetComponent<TrapezoidPole>().MyText = "--";
                }
            } else if (stage == 2) {
                keyObjects[i].GetComponent<TrapezoidPole>().MyText = textSet[consonant, i];
            }
        }
    }

    //システムの形などのコールバック用ゲッター
    public int PoleSum {
        get { return poleSum; }
    }

    public float RadiusOut {
        get { return radiusOut; }
    }

    public float RadiusIn {
        get { return radiusIn; }
    }

    public float PoleHeight {
        get { return poleHeight; }
    }

    /**************************************************************************/
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
}
