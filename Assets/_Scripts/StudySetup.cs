using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.TextCore.Text;

namespace HR_Toolkit
{
    public enum KeypadSize
    {
        Small,
        Medium,
        Large
    }

    public enum Curves
    {
        None,
        Linear,
        EaseIn,
        EaseOut,
        Shift
    }

    public class StudySetup : MonoBehaviour 
    {
        public int participantNumber;
        public int step;
        public int pinThreshold = 1;
        public float timerInSec = 30f;
        public int pinAttemptsBeforeTimer = 20;
        public TMP_Text timerText;

        private List<Tuple<KeypadSize, Curves>> _conditions;
        private int _conditionNumber;
        private bool _isTimerStarted;
        private float _remainingTime;

        private readonly int[,] _balancedLatinSquare =
        {
            { 15, 0, 1, 6, 10, 12, 5, 4, 13, 7, 9, 14, 3, 11, 8, 2 },
            { 15, 11, 2, 14, 8, 7, 3, 4, 9, 12, 13, 6, 5, 0, 10, 1 },
            { 15, 10, 5, 1, 13, 0, 9, 6, 3, 12, 8, 4, 2, 7, 11, 14 },
            { 15, 7, 14, 4, 11, 12, 2, 6, 8, 0, 3, 1, 9, 10, 13, 5 },
            { 15, 13, 9, 5, 3, 10, 8, 1, 2, 0, 11, 6, 14, 12, 7, 4 },
            { 15, 12, 4, 6, 7, 0, 14, 1, 11, 10, 2, 5, 8, 13, 3, 9 },
            { 15, 3, 8, 9, 2, 13, 11, 5, 14, 10, 7, 1, 4, 0, 12, 6 },
            { 15, 0, 6, 1, 12, 10, 4, 5, 7, 13, 14, 9, 11, 3, 2, 8 },
            { 15, 2, 11, 8, 14, 3, 7, 9, 4, 13, 12, 5, 6, 10, 0, 1 },
            { 15, 10, 1, 5, 0, 13, 6, 9, 12, 3, 4, 8, 7, 2, 14, 11 },
            { 15, 14, 7, 11, 4, 2, 12, 8, 6, 3, 0, 9, 1, 13, 10, 5 },
            { 15, 13, 5, 9, 10, 3, 1, 8, 0, 2, 6, 11, 12, 14, 4, 7 },
            { 15, 4, 12, 7, 6, 14, 0, 11, 1, 2, 10, 8, 5, 3, 13, 9 },
            { 15, 3, 9, 8, 13, 2, 5, 11, 10, 14, 1, 7, 0, 4, 6, 12 },
            { 15, 6, 0, 12, 1, 4, 10, 7, 5, 14, 13, 11, 9, 2, 3, 8 },
            { 15, 2, 8, 11, 3, 14, 9, 7, 13, 4, 5, 12, 10, 6, 1, 0 },
            { 15, 1, 10, 0, 5, 6, 13, 12, 9, 4, 3, 7, 8, 14, 2, 11 },
            { 15, 14, 11, 7, 2, 4, 8, 12, 3, 6, 9, 0, 13, 1, 5, 10 },
            { 15, 5, 13, 10, 9, 1, 3, 0, 8, 6, 2, 12, 11, 4, 14, 7 },
            { 15, 4, 7, 12, 14, 6, 11, 0, 2, 1, 8, 10, 3, 5, 9, 13 },
            { 15, 9, 3, 13, 8, 5, 2, 10, 11, 1, 14, 0, 7, 6, 4, 12 },
            { 15, 6, 12, 0, 4, 1, 7, 10, 14, 5, 11, 13, 2, 9, 8, 3 },
            { 15, 8, 2, 3, 11, 9, 14, 13, 7, 5, 4, 10, 12, 1, 6, 0 },
            { 15, 1, 0, 10, 6, 5, 12, 13, 4, 9, 7, 3, 14, 8, 11, 2 },
            { 15, 11, 14, 2, 7, 8, 4, 3, 12, 9, 6, 13, 0, 5, 1, 10 },
            { 15, 5, 10, 13, 1, 9, 0, 3, 6, 8, 12, 2, 4, 11, 7, 14 },
            { 15, 7, 4, 14, 12, 11, 6, 2, 0, 8, 1, 3, 10, 9, 5, 13 },
            { 15, 9, 13, 3, 5, 8, 10, 2, 1, 11, 0, 14, 6, 7, 12, 4 },
            { 15, 12, 6, 4, 0, 7, 1, 14, 10, 11, 5, 2, 13, 8, 9, 3 },
            { 15, 8, 3, 2, 9, 11, 13, 14, 5, 7, 10, 4, 1, 12, 0, 6 },
        };
        
        private RedirectionManager _redirectionManager;

        private void Awake()
        {
            _conditions = Conditions();
            _redirectionManager = RedirectionManager.instance;
            _conditionNumber = _balancedLatinSquare[participantNumber, step];
            _redirectionManager.SetPinThreshold(pinThreshold);
        }

        private void Update()
        {
            if (!_isTimerStarted) return;
            _redirectionManager.keypad.gameObject.SetActive(false);
            
            _remainingTime -= Time.deltaTime;

            var minutes = Mathf.FloorToInt(_remainingTime / 60.0f);
            var seconds = Mathf.FloorToInt(_remainingTime % 60.0f);

            if (_remainingTime > 0)
                timerText.text = $"Break\n{minutes:00}:{seconds:00}";

            if (_remainingTime <= 0)
            {
                StopTimer();
            }
        }

        public void StartTimer()
        {
            if (_redirectionManager.GetIsRecord())
            {
                _redirectionManager.GetWebcam().StopRecording();
                _redirectionManager.SetIsRecord(false);
            }
            _isTimerStarted = true;
            timerText.gameObject.SetActive(true);
            _remainingTime = timerInSec;
            Logger.Instance.PauseLogging();
        }

        public void StopTimer()
        {
            if (_redirectionManager.IsQuestionnaireActive()) return;
             
            if (!_redirectionManager.GetIsRecord())
            {
                _redirectionManager.GetWebcam().StartRecording();
                _redirectionManager.SetIsRecord(true);
            }
            _isTimerStarted = false;
            timerText.gameObject.SetActive(false);
            _redirectionManager.keypad.gameObject.SetActive(true);
            _redirectionManager.keypad.transform.position = _redirectionManager.GetInitialKeypadPosition();
            Logger.Instance.ResumeLogging();
            Logger.Instance.SetStartingTime();
        }

        public void NextCondition()
        {
            if (step < _conditions.Count)
            {
                step++;
            }
            else
            {
                Finish();                
            }
            
            _redirectionManager.keypad.transform.position = _redirectionManager.GetInitialKeypadPosition(); 
            SetupCondition();
        }

        private List<Tuple<KeypadSize, Curves>> Conditions()
        {
            var conditions = new List<Tuple<KeypadSize, Curves>>();
            
            foreach (KeypadSize keyboardSize in Enum.GetValues(typeof(KeypadSize)))
            {
                foreach (Curves curves in Enum.GetValues(typeof(Curves)))
                {
                    conditions.Add(new Tuple<KeypadSize, Curves>(keyboardSize, curves));         
                }
            }
            // Last Training Condition
            conditions.Add(new Tuple<KeypadSize, Curves>(KeypadSize.Medium, Curves.None)); 
            return conditions;
        }
        
        private void SetupParameters(KeypadSize keypadSize, Curves curve)
        {
            var redirectionTechnique = (Weiss_BodyWarping) _redirectionManager.GetDefaultRedirectionTechnique();
            
            _redirectionManager.isRandomVector = true;
            _redirectionManager.isRandomTarget = true;
            _redirectionManager.isShiftKeypad = false;
            
            switch (keypadSize)
            {
                case KeypadSize.Small:
                    _redirectionManager.ScaleKeypadToMaxAngle(4f);
                    break;
                case KeypadSize.Medium:
                    _redirectionManager.ScaleKeypadToMaxAngle(8f);
                    break;
                case KeypadSize.Large:
                    _redirectionManager.ScaleKeypadToMaxAngle(16f);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(keypadSize), keypadSize, null);
            }

            switch (curve)
            {
                case Curves.None:
                    redirectionTechnique.retargetingCurve = AnimationCurve.Constant(0, 1, 0);
                    break;
                case Curves.Linear:
                    redirectionTechnique.retargetingCurve = AnimationCurve.Linear(0, 0, 1, 1);
                    break;
                case Curves.EaseIn:
                    redirectionTechnique.retargetingCurve = AnimationCurve.EaseInOut(0, 0, 2, 2);
                    break;
                case Curves.EaseOut:
                    redirectionTechnique.retargetingCurve = AnimationCurve.EaseInOut(-1, -1, 1, 1);
                    break;
                case Curves.Shift:
                    _redirectionManager.isRandomVector = false;
                    _redirectionManager.isRandomTarget = false;
                    _redirectionManager.isShiftKeypad = true;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(curve), curve, null);
            }
        }

        public void SetupCondition()
        { 
            _conditionNumber = _balancedLatinSquare[participantNumber, step - 1];
            var condition = _conditions[_conditionNumber];
            SetupParameters(condition.Item1, condition.Item2);
            Debug.Log("Setup Parameters: " + condition.Item1 + " " + condition.Item2 + " Current Step: " + (step - 1));
        }
        
        private void Finish()
        {
            _redirectionManager.keypad.gameObject.SetActive(false); 
            Debug.Log("Study Finished!");
        }
        
        public int GetConditionNumber()
        { 
            return _conditionNumber;
        }
        
        public int GetPinAttemptsBeforeTimer()
        {
            return pinAttemptsBeforeTimer;
        }

        public bool GetIsTimerStarted()
        {
            return _isTimerStarted;
        }
        
    }
}