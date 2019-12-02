//using System.Collections;
//using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class TypingSystem : MonoBehaviour {

    public TextMesh InputTextObject;//入力を表示する欄
    public TextMesh TaskTextObject;//タスクを表示する欄
    public centralSystem centralSystem;

    [SerializeField]
    private string[] taskIndex;//txtファイルから取り出したタスク配列
    private int taskNum;//全タスク数
    [SerializeField]
    private string[] taskIndexQueue;//タスクのキュー
    [SerializeField]
    private int currentTaskNum = 0;//現在のキューの実行状況
    //private int currentCharPosition;//カラオケ表記用
    private string inputText = "";
    private bool currentTaskClear = false;  //タスク内容をクリアしたか

    private string clear;
    private string error;
    private string other;

    void Start() {
        taskReady();
        decideTaskIndexQueue();
        //カラーコード生成
        clear = TransMaterialToColorCode(variables.material_Typing_Clear);
        error = TransMaterialToColorCode(variables.material_Typing_Error);
        other = TransMaterialToColorCode(variables.material_Typing_Other);
    }

    void Update() {
        if (currentTaskClear) {
            //inputText = "";
            //編集メソッドから中身をゼロにする
            this.centralSystem.EditInputText("");
            currentTaskNum++;
            currentTaskClear = false;
        }

        /* とりあえず　一周したら止める */
        if (currentTaskNum >= taskNum) {
            Debug.Log("task clear.");
            TaskTextObject.text = "task clear.";
            currentTaskNum = 0;
            Destroy(this);
        }
        /* とりあえず　終わり */
        else {
            displayTaskText();
            displayInputText();
        }
    }

    //一覧のテキストファイルから読み込んで変数に格納
    void taskReady() {
        //wordファイルの場所
# if UNITY_EDITOR
        string filePath = Application.dataPath + "/word.txt";
#elif UNITY_STANDALONE_WIN
        string filePath = Application.dataPath + "/../word.txt";
# endif
        FileInfo fi = new FileInfo(filePath);
        //stream reader 初期化
        StreamReader streamReader = new StreamReader(fi.OpenRead(), true);
        //ファイルの行数取得
        for (taskNum = 0; !streamReader.EndOfStream; taskNum++)
            streamReader.ReadLine();

        //得た行数で一覧用の配列を初期化
        taskIndex = new string[taskNum];
        //stream reader 初期化
        StreamReader streamReader2 = new StreamReader(filePath, true);
        //配列に格納
        for (int i = 0; i < taskNum; i++)
            taskIndex[i] = streamReader2.ReadLine();

        //キューの初期化
        taskIndexQueue = new string[taskNum];
        for (int i = 0; i < taskNum; i++)
            taskIndexQueue[i] = "";
    }

    //キーイベントを得て保存する
    public void listenKeyEvent(string data) {
        /*if (data == "SystemCommand:BackSpace") {
            if (0 < inputText.Length) {
                inputText = inputText.Substring(0, inputText.Length - 1);
            }
        } else {
            inputText += data;
        }*/
        inputText = data;
    }

    //タスクのキューを作成
    void decideTaskIndexQueue() {
        for (int i = 0; i < taskNum; i++) {
            int randomIndex;
            //とりあえず乱数出す
            do {
                randomIndex = Random.Range(0, taskNum);
            } while (taskIndexQueue[randomIndex] != "");    //乱数を参照値にして、キューが空ならbreak
            //空の中身にタスクを入れる
            taskIndexQueue[randomIndex] = taskIndex[i];
        }
    }

    //タスクテキストを表示する
    void displayTaskText() {
        //できているところは青く
        //青色
        //TaskTextObject.text = "<color=#0000ff>";
        TaskTextObject.text = clear;
        string TextClear = "";
        //キューの当該単語長の中で
        for (int i = 0; i < taskIndexQueue[currentTaskNum].Length; i++) {
            //入力済み単語長を超えていればbreak
            if (i >= inputText.Length)
                break;
            //当該単語と入力単語の差異があればbreak
            if (taskIndexQueue[currentTaskNum][i] != inputText[i])
                break;
            //なければそのままスルー
            TextClear += taskIndexQueue[currentTaskNum][i];
            //タスク内容をクリアしたか
            if (i + 1 == taskIndexQueue[currentTaskNum].Length)
                currentTaskClear = true;
        }
        //タグの締め
        TaskTextObject.text += TextClear + "</color>";

        //できてないことろは赤く
        //赤色
        //TaskTextObject.text += "<color=#ff0000>";
        TaskTextObject.text += error;
        string TextError = "";
        //できるている範囲から、キューの当該単語長の中で
        for (int i = TextClear.Length; i < taskIndexQueue[currentTaskNum].Length; i++) {

            //エラー文字を1文字に保つ
            if (false) {
                //間違えた文字をそのまま表示するならtrue
                //BSなしで、間違えた文字を次の文字で上書きするならfalse
            } else if (i + 1 == taskIndexQueue[currentTaskNum].Length && i == TextClear.Length && inputText.Length > i + 1) {
                //最後の一文字で＆＆そこまで正解（最後の一文字ミス）＆＆タスクより長い文字列
                inputText = inputText.Substring(0, i) + inputText[i + 1];
                centralSystem.EditInputText(inputText);
                displayTaskText();
                variables.logInstance.LogSaving("delete",true);
                variables.logInstance.LogSaving("now string\t" + inputText, false);
                return;
            } else if (i > TextClear.Length && i < inputText.Length) {
                //エラー文字２文字目について
                inputText = inputText.Substring(0, i - 1) + inputText[i];
                centralSystem.EditInputText(inputText);
                displayTaskText();
                variables.logInstance.LogSaving("delete", true);
                variables.logInstance.LogSaving("now string\t" + inputText, false);
                return;
            } else if (i == TextClear.Length && i + 1 < inputText.Length) {
                //エラー文字1文字目について
                if (taskIndexQueue[currentTaskNum][i] == inputText[i + 1]) {
                    //入力のテキストの次文字、とタスク文字とが同じなら
                    inputText = inputText.Substring(0, i) + inputText[i + 1];
                    centralSystem.EditInputText(inputText);
                    displayTaskText();
                    variables.logInstance.LogSaving("delete", true);
                    variables.logInstance.LogSaving("now string\t" + inputText, false);
                    return;
                }
            }

            //入力済み単語長を超えていればbreak
            if (inputText.Length <= i)
                break;

            //なければそのままスルー
            TextError += taskIndexQueue[currentTaskNum][i];
        }
        //タグの締め
        TaskTextObject.text += TextError + "</color>";

        //そこから先は白く
        //白色
        //TaskTextObject.text += "<color=#ffffff>";
        TaskTextObject.text += other;
        string TextOther = "";
        //できるている+できていない範囲から、キューの当該単語長の中で
        for (int i = TextClear.Length + TextError.Length; i < taskIndexQueue[currentTaskNum].Length; i++) {
            //残りの文字を入れていく
            TextOther += taskIndexQueue[currentTaskNum][i];
        }
        //タグの締め
        TaskTextObject.text += TextOther + "</color>";
    }

    //インプット情報を保存する→タスクテキストの縁ありで表示
    void displayInputText() {
        InputTextObject.text = inputText;
    }

    //マテリアル情報をカラーコード情報に返還
    private string TransMaterialToColorCode(Material material) {
        string colorCode = "<color=#";
        //カラーを取り出したうえで、16進数に変換、格納
        colorCode += ( (int)( material.color.r * 255 ) ).ToString("x2");
        colorCode += ( (int)( material.color.g * 255 ) ).ToString("x2");
        colorCode += ( (int)( material.color.b * 255 ) ).ToString("x2");
        colorCode += ">";
        //Debug.Log(colorCode);
        return colorCode;
    }
}
