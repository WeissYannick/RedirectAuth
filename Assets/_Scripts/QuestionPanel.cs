using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Leap.Unity;

namespace HR_Toolkit
{


    public class QuestionPanel : MonoBehaviour
    {
        public TMP_Text questionText;
        public Slider Slider;
        
        private void Start()
        {
            Slider.onValueChanged.AddListener(delegate
            {
                Questionnaire.Instance.Entry(questionText.text, (int) Slider.value);
            });
        }

        public void SetQuestionText(string text)
        {
            questionText.text = text;
        }
        
        public void SetSliderValue(int value)
        {
            Slider.value = value;
        }

        public void ResetSliderValue()
        {
            Slider.value = 10;
        }
    }
}