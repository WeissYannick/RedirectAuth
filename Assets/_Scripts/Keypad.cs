using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Leap.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HR_Toolkit
{
    public class Keypad : MonoBehaviour
    {
        public Button[] buttons;
        public TMP_Text prompt;
        public TMP_Text pinTMP;
        public GameObject indicator;

        private List<int> _pin;

        private void Update()
        {
            HighlightPinDigit();
        }

        public float GetMaxDistance()
        {
            return buttons
                .SelectMany(_ => buttons, (button, otherButton) => 
                    Vector3.Distance(button.transform.position, otherButton.transform.position))
                .Max();
        }

        public float GetDiagonalDistance()
        {
            return (float) buttons.SelectMany(_ => buttons, (button, otherButton) => 
                    Math.Round(Vector3.Distance(button.transform.position, otherButton.transform.position), 4))
                .Distinct()
                .OrderBy(x => x)
                .Skip(2)
                .FirstOrDefault();
        }
        
        public TMP_Text GetPinTmp()
        {
            return pinTMP;
        }
        
        public void SetPinTmpActive(bool active)
        {
            pinTMP.gameObject.SetActive(active);
        }
        
        public void SetPin(List<int> digits)
        {
            _pin = digits;
            
            pinTMP.GetComponent<TextMeshProUGUI>().text =  string.Join("", digits);
        }

        
        public void SetPrompt(string promptMessage)
        {
            prompt.GetComponent<TextMeshProUGUI>().text = promptMessage;
        }

        public void SetIndicatorColor(Color color)
        {
            indicator.GetComponent<Image>().color = color;
        }
        
        public void ActivateAllButtons(bool active)
        {
             buttons.ForEach(button => button.interactable = active); 
        }

        public void HighlightPinDigit()
        {
            var pin = string.Join("", _pin);
            var pinUgui = pinTMP.GetComponent<TextMeshProUGUI>();
            var currentPinInputCount = RedirectionManager.instance.GetCurrentPinInput().Count;
            
            if (currentPinInputCount >= pin.Length) return;
            var nextDigit = pin[currentPinInputCount].ToString();
            pinUgui.text = pin
                .Remove(currentPinInputCount, 1)
                .Insert(currentPinInputCount, $"<color=#5555ff>{nextDigit}</color>");
        }
    }
}