using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ShowKeyboard : MonoBehaviour
{
    public GameObject vrKeyboardcanvas;
    private VirtualKeyboard virtualKeyboard;
    public VirtualTextInputBox textbox;
    
    private TMP_InputField inputField;
    void Start()
    {
        inputField = GetComponent<TMP_InputField>();
        virtualKeyboard = vrKeyboardcanvas.GetComponent<VirtualKeyboard>();
    }

    public void OpenKeyboard()
    {
        vrKeyboardcanvas.SetActive(true);
        virtualKeyboard.inputField = inputField;
        //NonNativeKeyboard.Instance.InputField = inputField;
        textbox.TextField= inputField.text;
        
        //NonNativeKeyboard.Instance.PresentKeyboard(inputField.text);
 
    }
}
