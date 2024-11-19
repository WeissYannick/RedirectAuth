using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HR;
using UnityEngine;

namespace HR_Toolkit
{

    public class Logger : MonoBehaviour
    {
        [HideInInspector] public MovementLogger realHandMovement;

        private RedirectionManager redirectionManager;
        private int _counter;
        private string _logDirectory;
        private int _conditionNr;
        private int _participantNr;

        public static Logger Instance { get; private set; }

        // Stream Writer variables:
        private StreamWriter writer;
        private int frame;
        private string time;
        private Vector3 rHandPos;
        private Quaternion rHandRot;
        private Vector3 vHandPos;
        private Quaternion vHandRot;
        private HandRedirector redirectionTechnique;
        private RedirectionObject target;
        private int targetNr;
        private int inputNr;
        private float redirectionDistance;
        private Vector2 offsetVector;
        private Vector3 targetRealPos;
        private Quaternion targetRealRot;
        private Vector3 targetVirtualPos;
        private Quaternion targetVirtualRot;
        private Vector3 bodyPos;
        private Quaternion bodyRot;

        public bool IsLogging = true;
        public bool RealHandPosition = true;
        public bool RealHandRotation;
        public bool VirtualHandPosition;
        public bool VirtualHandRotation;
        public bool Target;
        public bool TargetNr;
        public bool InputNr;
        public bool RedirectionDistance;
        public bool OffsetVector;
        public bool RedirectionTechnique;
        public bool TargetRealPosition;
        public bool TargetRealRotation;
        public bool TargetVirtualPosition;
        public bool TargetVirtualRotation;
        public bool BodyPosition;
        public bool BodyRotation;
        
        private long _startingTime;
        private string _pin;
        private string _pinInput;
        private long _endTime;


        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            redirectionManager = RedirectionManager.instance;
            _participantNr = redirectionManager.GetParticipantNumber();
            //realHandMovement = gameObject.AddComponent<MovementLogger>();
            //realHandMovement.LoadPath();
            //realHandMovement.StartToLog(RedirectionManager.instance.realHand);
            
            var projectPath = Application.dataPath.Replace("/Assets", ""); 
            _logDirectory = projectPath + "/Logs" + $"/Participant_{_participantNr}";
            Directory.CreateDirectory(_logDirectory);

            writer = new StreamWriter(GetPath());
            WriteHeader();
        }

        private void WriteHeader()
        {
            var variables = new Dictionary<string, bool>
            {
                { "Frame", true },
                { "Timestamp", true },
                { "Real Hand Position", RealHandPosition },
                { "Real Hand Rotation", RealHandRotation },
                { "Virtual Hand Position", VirtualHandPosition },
                { "Virtual Hand Rotation", VirtualHandRotation },
                { "Target", Target },
                { "Target Nr", TargetNr },
                { "Input Nr", InputNr },
                { "Redirection Distance", RedirectionDistance },
                { "Offset Vector", OffsetVector },
                { "Redirection Technique", RedirectionTechnique },
                { "Target Real Position", TargetRealPosition },
                { "Target Real Rotation", TargetRealRotation },
                { "Target Virtual Position", TargetVirtualPosition },
                { "Target Virtual Rotation", TargetVirtualRotation },
                { "Body Position", BodyPosition },
                { "Body Rotation", BodyRotation }
            };

            var header = string.Join(";", variables.Where(v => v.Value).Select(v => v.Key));

            writer.WriteLine(header);
        }

        private void Update()
        {
            if (!IsLogging) return;

            time = DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString();
            rHandPos = redirectionManager.realHand.transform.position;
            rHandRot = redirectionManager.realHand.transform.rotation;
            vHandPos = redirectionManager.virtualHand.transform.position;
            vHandRot = redirectionManager.virtualHand.transform.rotation;

            targetNr = redirectionManager.GetPin()[(_counter - 1) % redirectionManager.PinLength];
            inputNr = redirectionManager.GetLastPinInput();
            redirectionDistance = redirectionManager.GetRedirectionDistance();

            target = redirectionManager.target;
            if (target != null)
            {
                redirectionTechnique = target.redirectionTechnique;
                targetRealPos = target.positions[0].realPosition.transform.position;
                targetRealRot = target.positions[0].realPosition.transform.rotation;
                targetVirtualPos = target.positions[0].virtualPosition.transform.position;
                targetVirtualRot = target.positions[0].virtualPosition.transform.rotation;
                offsetVector = new Vector2(target.GetOffsetVector().x, target.GetOffsetVector().y);
            }

            bodyPos = redirectionManager.body.transform.position;
            bodyRot = redirectionManager.body.transform.rotation;
            if (target != null)
            {
                var logObjects = new List<object>
                {
                    frame, time,
                    RealHandPosition ? rHandPos.ToString("f4") : null,
                    RealHandRotation ? rHandRot : null,
                    VirtualHandPosition ? vHandPos.ToString("f4") : null,
                    VirtualHandRotation ? vHandRot : null,
                    Target ? target : null,
                    TargetNr ? targetNr : null,
                    InputNr ? inputNr : null,
                    RedirectionDistance ? redirectionDistance : null,
                    OffsetVector ? offsetVector.ToString("f4") : null,
                    RedirectionTechnique ? redirectionTechnique.name : null,
                    TargetRealPosition ? targetRealPos.ToString("f4"): null,
                    TargetRealRotation ? targetRealRot : null,
                    TargetVirtualPosition ? targetVirtualPos.ToString("f4") : null,
                    TargetVirtualRotation ? targetVirtualRot : null,
                    BodyPosition ? bodyPos : null,
                    BodyRotation ? bodyRot : null
                };

                var logValues = logObjects.Where(o => o != null).Select(o => o.ToString());
                var log = string.Join(";", logValues);
                writer.WriteLine(log);
            }

            frame++;
        }

        public void CheckPin()
        {
            if (redirectionManager.GetCurrentPinInput().Count < redirectionManager.PinLength) return;
            
            _pin = string.Join("", redirectionManager.GetPin());
            _pinInput = string.Join("", redirectionManager.GetCurrentPinInput());
            _endTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            
            SummarizedPinLog();
        }

        private void OnApplicationQuit()
        {
            writer.Close();
        }

        private string GetPath()
        {
            #if UNITY_EDITOR
            var currentConditionNr = redirectionManager.GetConditionNumber();
            _counter = _conditionNr != currentConditionNr ? 1 : _counter + 1;
            _conditionNr = currentConditionNr;

            return Path.Combine(_logDirectory, $"log_{_participantNr}_{_conditionNr}_{_counter}.csv");
            #endif
        }

        public void Finish()
        {
            writer.Flush();
            writer.Dispose();
            SummarizedLog();
            writer = new StreamWriter(GetPath());
            WriteHeader();
        }
       
        public void ResumeLogging()
        {
            IsLogging = true;
        }

        public void PauseLogging()
        {
            IsLogging = false;
        }

        public void SetStartingTime()
        {
            _startingTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        }

        public void SummarizedLog()
        {
            var isNewCondition = _counter == 1;
            var path = $"summary_{_participantNr}_{_conditionNr}.csv"; 
            var summaryWriter = new StreamWriter(Path.Combine(_logDirectory, path), !isNewCondition);
            
            if (isNewCondition)
            {
                const string header = "Timestamp; TargetNr; InputNr";
                summaryWriter.WriteLine(header);
            }
            
            var log = $"{time}; {targetNr}; {inputNr}";
            summaryWriter.WriteLine(log);
            
            summaryWriter.Flush();
            summaryWriter.Dispose();
        }

        private void SummarizedPinLog()
        {
            if (!IsLogging) return;
                     
            var isNewCondition = _counter <= redirectionManager.PinLength;
            var path = $"summaryPIN_{_participantNr}_{_conditionNr}.csv"; 
            var summaryWriter = new StreamWriter(Path.Combine(_logDirectory, path), !isNewCondition);
            
            if (isNewCondition)
            {
                const string header = "Starting Time; End Time; PIN; Input";
                summaryWriter.WriteLine(header);
            }  
            
            var log = $"{_startingTime}; {_endTime}; {_pin}; {_pinInput}";
            
            summaryWriter.WriteLine(log);
            summaryWriter.Flush();
            summaryWriter.Dispose();
        }
    }
}