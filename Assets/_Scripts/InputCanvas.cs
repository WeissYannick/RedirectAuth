using HR_Toolkit;
using TMPro;
using UnityEngine;

public class InputCanvas : MonoBehaviour
{
    public static InputCanvas Instance { get; private set; }
    private TMP_Text _textField;
    private bool _isPinCorrect;
    private bool _isPinIncorrect;

    private void Start()
    {
        _textField = GetComponent<TMP_Text>(); 
        Instance = this;
    }

    private void Update()
    {
        var currentPin = RedirectionManager.instance.GetCurrentPinInput();

        if (_isPinCorrect)
        {
            _textField.text = "✅";
            _isPinCorrect = false;
        }
        else if (_isPinIncorrect)
        {
            _textField.text = "❌";
            _isPinCorrect = false;
            _isPinIncorrect = false;
        }
        else if (currentPin.Count != 0)
        {
            _textField.text = string.Join("", currentPin);
        }
        
    }

    public void PinCorrect()
    {
        _isPinCorrect = true;
    }

    public void PinIncorrect()
    {
        _isPinIncorrect = true;
    }
    
    public void SetText(string text)
    {
        _textField.text = text;
    }
}