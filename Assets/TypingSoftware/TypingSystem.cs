using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class TypingSystem : MonoBehaviour {

    public TextMesh InputTextObject;//入力を表示する欄
    public TextMesh TaskTextObject;//タスクを表示する欄
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

    void Start() {
        taskReady();
        decideTaskIndexQueue();
    }

    void Update() {
        if (currentTaskClear) {
            inputText = "";
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
            displayInputText();
            displayTaskText();
        }
    }

    //一覧のテキストファイルから読み込んで変数に格納
    void taskReady() {
        //wordファイルの場所
        string filePath = Application.dataPath + "/word.txt";
        //stream reader 初期化
        StreamReader streamReader = new StreamReader(filePath, true);
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
        if (data == "SystemCommand:BackSpace") {
            if (0 < inputText.Length) {
                inputText = inputText.Substring(0, inputText.Length - 1);
            }
        } else {
            inputText += data;
        }
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
        TaskTextObject.text = "<color=#0000ff>";
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
        TaskTextObject.text += "<color=#ff0000>";
        string TextError = "";
        //できるている範囲から、キューの当該単語長の中で
        for (int i = TextClear.Length; i < taskIndexQueue[currentTaskNum].Length; i++) {
            //入力済み単語長を超えていればbreak
            if (i >= inputText.Length)
                break;
            //なければそのままスルー
            TextError += taskIndexQueue[currentTaskNum][i];
        }
        //タグの締め
        TaskTextObject.text += TextError + "</color>";

        //そこから先は白く
        //白色
        TaskTextObject.text += "<color=#ffffff>";
        string TextOther = "";
        //できるている+できていない範囲から、キューの当該単語長の中で
        for (int i = TextClear.Length + TextError.Length; i < taskIndexQueue[currentTaskNum].Length; i++) {
            //残りの文字を入れていく
            TextOther += taskIndexQueue[currentTaskNum][i];
        }
        //タグの締め
        TaskTextObject.text += TextOther + "</color>";
    }

    //インプット情報を保存する→そのうちタスクテキストの縁ありで表示する仕様にする
    void displayInputText() {
        InputTextObject.text = inputText;
    }
}
