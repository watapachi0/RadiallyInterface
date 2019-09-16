using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class testInputField : MonoBehaviour {

    public InputField inputField;
    public TypingSystem typingSystem;

    void Start() {
        //inputField = GetComponent<InputField>();
    }

    void Update() {

    }

    public void BackSpaceButton() {
        typingSystem.listenKeyEvent("SystemCommand:BackSpace");
    }

    public void OnEndEditEvent() {
        typingSystem.listenKeyEvent(inputField.text);
        inputField.text = "";
    }
}
