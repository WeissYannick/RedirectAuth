using System.Collections.Generic;
using System.IO;
using HR_Toolkit;
using Leap.Unity;
using TMPro.EditorUtilities;
using UnityEngine;
using Logger = UnityEngine.Logger;

namespace HR_Toolkit
{
    public class Questionnaire : MonoBehaviour
    {
        public static Questionnaire Instance { get; private set; }

        private readonly string[] _questions =
        {
            "How mentally demanding was the task?",
            "How physically demanding was the task?",
            "How hurried or rushed was the pace of the task?",
            "How successful were you in accomplishing what you were asked to do?",
            "How hard did you have to work to accomplish your level of performance?",
            "How insecure, discouraged, irritated, stressed, and annoyed were you?",
            "In a shared space, how concerned would you be that bystanders can guess your input?"
        };

        private int _questionsPerPage;
        private int _nextPage;
        private StreamWriter _writer;
        private RedirectionManager _redirectionManager;
        private bool _isActive { get;  set; }

        private readonly SortedDictionary<string, int> _answers = new();

        public QuestionPanel[] questionPanels;

        private void Start()
        {
            Instance = this;
            _redirectionManager = RedirectionManager.instance;
        }

        private void Begin()
        {
            _questionsPerPage = questionPanels.Length;
            _writer = new StreamWriter(GetPath());
            WriteHeader();
        }

        public void NextPage()
        {
            if (_nextPage * _questionsPerPage >= _questions.Length)
            {
                Log();
                Finish();
                return;
            }

            questionPanels.ForEach(panel => panel.gameObject.SetActive(false));
            var currentPageQuestionCount =
                Mathf.Min(_questions.Length - _nextPage * _questionsPerPage, _questionsPerPage);
            for (int i = 0; i < currentPageQuestionCount; i++)
            {
                var questionsIndex = i + _nextPage * _questionsPerPage;

                questionPanels[i].gameObject.SetActive(true);
                questionPanels[i].SetQuestionText(_questions[questionsIndex]);
            }

            _nextPage++;
            questionPanels.ForEach(panel => panel.ResetSliderValue());
            questionPanels.ForEach(panel =>
            {
                if (_answers.TryGetValue(panel.questionText.text, out var answer))
                    panel.SetSliderValue(answer);
            });
        }

        public void PreviousPage()
        {
            var previousPage = _nextPage - 2;
            if (previousPage < 0) return;

            questionPanels.ForEach(panel => panel.gameObject.SetActive(true));
            for (var i = 0; i < _questionsPerPage; i++)
            {
                var questionsIndex = i + previousPage * _questionsPerPage;
                questionPanels[i].SetQuestionText(_questions[questionsIndex]);
            }

            _nextPage--;
            questionPanels.ForEach(panel =>
            {
                if (_answers.TryGetValue(panel.questionText.text, out var answer))
                    panel.SetSliderValue(_answers[panel.questionText.text]);
            });
        }

        public void Entry(string question, int answer)
        {
            _answers[question] = answer;
        }

        public void Show()
        {
            Start();
            Begin();
            _isActive = true;
            Logger.Instance.PauseLogging();
            gameObject.SetActive(true);
            _answers.Clear();
            _nextPage = 0;
            NextPage();
            _redirectionManager.SwitchHandednessToWeakHand();

            RedirectionManager.instance.NextStudyCondition();
            if (_redirectionManager.GetIsRecord())
            {
                _redirectionManager.GetWebcam().StopRecording();
                _redirectionManager.SetIsRecord(false);
            }
        }

        #region Logger

        private void Log()
        {
            foreach (var answer in _answers)
            {
                _writer.WriteLine($"{answer.Key}; {answer.Value}");
            }
        }

        private string GetPath()
        {
            #if UNITY_EDITOR
            var participantNumber = _redirectionManager.GetParticipantNumber();
            var conditionNumber = _redirectionManager.GetConditionNumber();
            var projectPath = Application.dataPath.Replace("/Assets", "");
            var dataPath = projectPath + "/Logs" + $"/Participant_{participantNumber}";
            Directory.CreateDirectory(dataPath);
            return Path.Combine(dataPath, $"questionnaire_{participantNumber}_{conditionNumber}.csv");
            #endif
        }

        private void WriteHeader()
        {
            _writer.WriteLine("Question; Answer");
        }

        public void Finish()
        {
            _writer.Flush();
            _writer.Dispose();

            gameObject.SetActive(false);
            _redirectionManager.keypad.gameObject.SetActive(true);
            _redirectionManager.keypad.transform.position = _redirectionManager.GetInitialKeypadPosition();
            _redirectionManager.SwitchHandednessToDominantHand();
            
            _redirectionManager.SetupStudyCondition();
            
            if (!_redirectionManager.GetIsRecord())
            {
                _redirectionManager.GetWebcam().StartRecording();
                _redirectionManager.SetIsRecord(true);
            }
            Logger.Instance.ResumeLogging();
            Logger.Instance.SetStartingTime();
            _isActive = false;
        }

        #endregion
        
        public bool GetIsActive()
        {
            return _isActive;
        }
    }
}