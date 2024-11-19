using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Leap.Unity;
using Leap.Unity.InputModule;
using UnityEngine;
using Random = UnityEngine.Random;

namespace HR_Toolkit
{
    public class RedirectionManager : MonoBehaviour
    {
        /// <summary>
        ///  With a static instance of this object, all other objects can get the data
        /// </summary>
        public static RedirectionManager instance;
        
        /// <summary>
        /// The virtual world parent game object. Needs to be set to the object that will be rotated
        /// with the World Warping Redirection Techniques
        /// </summary>
        public GameObject virtualWorld;
        /// <summary>
        /// The game object of the physically tracked hand
        /// </summary>
        public GameObject realHand;
        /// <summary>
        /// The game object of the virtual hand
        /// </summary>
        public GameObject virtualHand;
        /// <summary>
        /// The warp origin that is used by all redirection techniques. If it is set to NONE, it will
        /// be set to the hand's real position on the start of each redirection
        /// </summary>
        public GameObject warpOrigin;
        /// <summary>
        /// Reset Position is set in each RedirectionTechnique.
        /// The ResetPosition is used between two redirections. Instead of redirecting from one target to another target,
        /// the user will be redirected to the reset position first. This prevents to huge redirections. 
        /// </summary>
        private RedirectionObject resetPosition;

        /// <summary>
        /// The disance threshold to align the real and virtual hand
        /// </summary>
        public const float handAlignmentDistance = 0.01f;
        /// <summary>
        /// Check if the next target is the reset position 
        /// </summary>
        private bool useResetPosition;
        /// <summary>
        /// The physically tracked head position
        /// </summary>
        public GameObject body;

        public bool showQuestionnaire;
        public bool isRandomTarget;
        public bool isRandomVector;
        public bool isShiftKeypad;
        public bool isHighlightActive;
        public bool isStudySetup;
        public bool isScaleKeypad;
        public GameObject randomVector;
        public float maxRedirectionAngle = 10f;
        public WebcamCapture webcam;
        
        private bool _isTargetSelected;
        private bool _isWarpOriginSet;
        public Keypad keypad;
        public Questionnaire questionnaire;
        public float threshold = 0.2f; 
        private List<int> _pin = new();
        private List<int> _currentPinInput = new();
        public  int PinLength = 4;
        private int _pinInputCount;
        private int _pinThreshold = 1;
        private StudySetup _studySetup;
        private Vector3 _initialKeypadPosition;
        private Handedness _handedness;
        private bool _loggerStarted;
        private bool _isHandBehindKeypad;
        private bool _isRecord;
        
        private bool _isRedirectionActive;
        /// <summary>
        /// The movement controller is automatically added to the Redirection Manager, it tracks the actual
        /// movement options and it's parameters
        /// </summary>
        public MovementController movementController;
        /// <summary>
        /// A list which holds all redirected prefabs. On default it serves as a new target selection but
        /// can be edited manually
        /// </summary>
        public List<RedirectionObject> allRedirectedPrefabs;
        /// <summary>
        /// The Redirection Technique in the Redirection Manager serves as the default Redirection
        /// Technique, which is used by all Redirected Prefabs, that do not specify another technique 
        /// </summary>
        public HandRedirector redirectionTechnique;

        /// <summary>
        /// The active redirected prefab. It's redirection technique 'ApplyRedirection()' Method is
        /// called in the Update()
        /// </summary>
        public RedirectionObject target;
        /// <summary>
        /// The last redirected target
        /// </summary>
        public RedirectionObject lastTarget;

        /// <summary>
        /// The speed of the hand movement when the hand is controlled by the mouse
        /// </summary>
        [HideInInspector]
        public float speed;
        /// <summary>
        /// The speed of the height hand movement when the hand is controlled by the mouse
        /// </summary>
        [HideInInspector]
        public float mouseWheelSpeed;
        /// <summary>
        /// The selected movement option.
        /// </summary>
        [HideInInspector]
        public MovementController.Movement movement;

        private LineRenderer lineRenderer;

        /// <summary>
        /// On Awake we set the static instance of the Redirection Manager, so that all
        /// other objects can access it's data. The in editor selected Redirection Technique
        /// becomes the default Redirection Technique for all objects.
        /// </summary>
        private void Awake()
        {
            instance = this;
            instance.redirectionTechnique = redirectionTechnique;
            _studySetup = GetComponent<StudySetup>();
            
            if (isStudySetup)
            {
                questionnaire.gameObject.SetActive(false);
            }
        }


        /// <summary>
        /// On Start we set the virtual hand position to the real hand position,
        /// Initialize the MovementController with the real hand,
        /// Set the mesh of the real and virtual hand to the virtual/physical camera layers
        /// to render them only on the needed cameras
        /// </summary>
        private void Start()
        {
            SetVirtualHandToRealHand();
            movementController.Init(realHand);
            //SetLayerRecursively(realHand, "Physical/Hand");
            //SetLayerRecursively(virtualHand, "Virtual/Hand");
            
           _initialKeypadPosition = keypad.transform.position; 

           webcam.SetupCamera();
           GenerateNewPin();
           _isRecord = true;
           _handedness = GetComponent<Handedness>();
        }

        /// <summary>
        /// In the Update we do:
        ///   - Move the hand with the movement controller
        ///   - Check for an 'space'-key input to change the target
        ///     - if it is changed, the warp origin will be set to the actual real hand pos,
        ///     - the last target is the to the actual target
        ///     - set a new target with GetNextTarget()
        ///     - update the highlighted objects
        ///   - call the Redirect() Method on the target (redirected prefab)
        /// </summary>
        private void Update()
        {
            movementController.MoveHand();
            movementController.MoveBody();
           
            var keypadDistance = keypad.transform.position.z - realHand.transform.position.z;
            if (!_isTargetSelected && keypadDistance > threshold)
            {
                if (isRandomTarget)
                {
                    CheckForNewTarget();
                    _isRedirectionActive = false;
                }
                else if (isShiftKeypad)
                {
                    ShiftKeypad();
                }
                
                UpdatePrompt(true);
                _isTargetSelected = true;
                _isHandBehindKeypad = false; 
                
                CheckPin();
                
                if (_loggerStarted)
                    Logger.Instance.Finish();
                _loggerStarted = true;
                
            }
            else if (_isTargetSelected && keypadDistance < threshold && !_isWarpOriginSet) 
            {
                warpOrigin.transform.position = realHand.transform.position;
                _isWarpOriginSet = true;
                if (isRandomTarget)
                {
                    _isRedirectionActive = true;
                }
            }
            
            Vector3 indexFinger = Vector3.zero;
            if (_handedness.DominantHand == Chirality.Left)
            {
                var leapHand = _handedness.VirtualLeftHand.GetLeapHand();
                if (leapHand != null)
                {
                    indexFinger = leapHand.GetIndex().TipPosition;
                }
            }
            else
            {
                var leapHand = _handedness.VirtualRightHand.GetLeapHand();
                if (leapHand != null)
                {
                    indexFinger = leapHand.GetIndex().TipPosition;
                }
            }
            
            if (indexFinger.z > keypad.transform.position.z + 0.02 && !_isHandBehindKeypad 
                                                                   && !_studySetup.GetIsTimerStarted()
                                                                   && indexFinger.z < keypad.transform.position.z + 0.1)
            {
                _isHandBehindKeypad = true;
                
                UpdatePrompt(false);
                SetIsShiftKeypad(false);
                DeactivateTarget();
                AddUserInput(-2);

                
                var audioSource = gameObject.AddComponent<AudioSource>(); 
                var pressOnAudio = Resources.Load<AudioClip>("PressOn");
                var pressOffAudio = Resources.Load<AudioClip>("PressOff"); 
                
                audioSource.PlayOneShot(pressOnAudio);
                audioSource.clip = pressOffAudio;
                audioSource.PlayDelayed(0.25f);
            }
            
            
            if (isScaleKeypad)
            {
                ScaleKeypadToMaxAngle(maxRedirectionAngle);
            }
            
            // apply redirection
            if (_isRedirectionActive)
            {
                target.Redirect();
            }
            else
            {
                SetVirtualHandToRealHand();
            }

        }

        public void UpdatePrompt(bool isHandRetracted)
        {
            if (isHandRetracted)
            {
                keypad.SetPrompt("Enter Passcode");
                keypad.SetIndicatorColor(Color.green);
                keypad.ActivateAllButtons(true);
            }
            else
            {
                keypad.SetPrompt("Retract Hand fully");
                keypad.SetIndicatorColor(Color.red);
                keypad.ActivateAllButtons(false);
            }
        }

        private void CheckPin()
        {
            if (_currentPinInput.Count < PinLength) return;
            if (keypad.transform.position.z - realHand.transform.position.z < threshold) return;

            Logger.Instance.CheckPin();
            GenerateNewPin();
            _currentPinInput.Clear();
        }

        private void GenerateNewPin()
        {
            keypad.SetPinTmpActive(true);
            _pin.Clear();
            for (var i = 0; i < PinLength; i++)
            {
                _pin.Add(Random.Range(0, allRedirectedPrefabs.Count));  
            }

            keypad.SetPin(_pin);

            if (isStudySetup)
            {
                if (_pinInputCount % _studySetup.GetPinAttemptsBeforeTimer() == 0)
                {
                    if (_pinInputCount > 0)
                    {
                        keypad.gameObject.SetActive(false);
                        _studySetup.StartTimer();
                    } 
                }
                
                if (_pinInputCount % _pinThreshold == 0)
                    if (_pinInputCount > 0 && showQuestionnaire)
                    {
                        keypad.gameObject.SetActive(false);
                        questionnaire.Show();
                        isRandomTarget = false; //Deactivate the Redirection 
                    }
                    else
                    {
                        _studySetup.NextCondition();
                    }
            }

            if (_isRecord)
            {
               webcam.StartNewVideo(); 
            }
            Logger.Instance.SetStartingTime();
            _pinInputCount++;
        }
        public void ScaleKeypadToMaxAngle(float angle)
        {
            if (angle <= 0) return;
            
            maxRedirectionAngle = angle;
            var maxRandomVectorLength = Mathf.Tan(angle * Mathf.Deg2Rad) * threshold;
            var maxDistance = keypad.GetDiagonalDistance();
            var maxRandomVectorLengthInKeypad = maxRandomVectorLength / maxDistance;
            
            keypad.transform.localScale *= maxRandomVectorLengthInKeypad; 
        }
        
        #region Called in Update
        private void CheckForNewTarget()
        {
            // if (virtualHand.transform.position.Equals(warpOrigin.transform.position)) return;  
            
            lastTarget = target;
            if (lastTarget != null)
            {
                lastTarget.EndRedirection();
            }

            if (isRandomTarget)
            {
                target = GetRandomTarget();
                _isTargetSelected = true;
            }
            else
            {
                target = GetNextTarget();
            }

            
            UpdateWarpOrigin();
            if (target != null)
            {
                target.StartRedirection();
            }
        }

        private void UpdateWarpOrigin()
        {
           // TODO: implement UpdateWarpOrigin()  
        }

        private RedirectionObject GetNextTarget()
        {
            if (allRedirectedPrefabs.Count == 0)
            {
                throw new Exception("There are no redirected prefabs that could be targeted");
            }
            
            // there was no previous target selected, we need to set it on first call
            if (target == null && lastTarget == null)
            {
                return allRedirectedPrefabs[0];
            }
            
            if (allRedirectedPrefabs.Count == 1)
            {
                Debug.Log("There is only one target, can't choose another target");
                return allRedirectedPrefabs[0];
            }

            var index = allRedirectedPrefabs.IndexOf(lastTarget);
            var newIndex = (index + 1) % allRedirectedPrefabs.Count;

            return allRedirectedPrefabs[newIndex];        
        }

        private RedirectionObject GetRandomTarget()
        {
            if (!allRedirectedPrefabs.Any()) return null;
            var index = Random.Range(0, allRedirectedPrefabs.Count);
            return allRedirectedPrefabs[index];
        }

        private void ShiftKeypad() 
        {
            if (!isShiftKeypad) return;
            
            var offset = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0).normalized;                        
            var maxRandomVectorLength = Mathf.Tan(maxRedirectionAngle * Mathf.Deg2Rad) * threshold;
            offset *= maxRandomVectorLength;
            
            isShiftKeypad = false;

            keypad.transform.position =  _initialKeypadPosition + offset;
        }


        #endregion
        
        public void SetVirtualHandToRealHand()
        {
            virtualHand.transform.position = realHand.transform.position;
            virtualHand.transform.rotation = realHand.transform.rotation;
            virtualHand.transform.localScale = realHand.transform.localScale;
        }
        
        public void AddUserInput(int input)
        {
            _currentPinInput.Add(input);
        }
        

        #region Render Hands on Layer
        /// <summary>
        /// Is used to set all game objects and its children to a specific layer, is used here to set
        /// the hand meshes from the SteamVR asset to a new layer, since they spawn after 
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="newLayer"></param>
        void SetLayerRecursively(GameObject obj, string newLayer)
        {
            if (null == obj)
            {
                return;
            }
           
            obj.layer = LayerMask.NameToLayer(newLayer);
           
            foreach (Transform child in obj.transform)
            {
                if (null == child)
                {
                    continue;
                }
                SetLayerRecursively(child.gameObject, newLayer);
            }
        }
        
        private static void SetHandWithChildsToLayer_(GameObject obj, string name)
        {
            obj.layer = LayerMask.NameToLayer(name);
            foreach (var child in obj.GetComponentsInChildren<Transform>(true))  
            {
                //child.gameObject.layer = LayerMask.NameToLayer (name); 
                SetHandWithChildsToLayer_(child.gameObject, name);
                Debug.Log("Changed LAyer");
            }
        }

        #endregion

        #region Getter
        public LineRenderer GetLineRenderer()
        {
            return lineRenderer;
        }
        
        /// <summary>
        /// Returns the redirection technique which was set in the inspector on the
        /// Redirection Manager object in the inspector
        /// </summary>
        /// <returns></returns>
        public HandRedirector GetDefaultRedirectionTechnique()
        {
            return redirectionTechnique;
        }
        public GameObject GetDefaultWarpOrigin()
        {
            return warpOrigin;
        }
        
        /// <summary>
        /// Checks, if the virtual hand and the real hand are aligned. Displays the result on the overview screen
        /// </summary>
        public bool HandsAreAligned()
        {
            var handDistance = Vector3.Distance(virtualHand.transform.position, realHand.transform.position);
         
            if (handDistance < handAlignmentDistance)
            {
                return true;
            }
         
            return false;
        }

        public RedirectionObject GetActiveTarget()
        {
            return target;
        }

        public void SetWarpOrigin(Vector3 newOrigin)
        {
            warpOrigin.transform.position = newOrigin;
        }
        
        public void DeactivateTarget()
        {
            _isTargetSelected = false;
            _isWarpOriginSet = false;
        }
        
        public List<int> GetCurrentPinInput()
        {
            return _currentPinInput;
        }
        
        public void SetIsShiftKeypad(bool value)
        {
            isShiftKeypad = value;
        }

        public void NextStudyCondition()
        {
            _studySetup.NextCondition();
        }
        
        public int GetParticipantNumber()
        {
            return _studySetup.participantNumber;
        }
        
        public int GetConditionNumber()
        {
            return _studySetup.GetConditionNumber();
        }

        public List<int> GetPin()
        {
            return _pin;
        }

        public float GetRedirectionDistance()
        {
            if (target == null) return 0;
            
            return target.GetRedirectionDistance();
        }
        
        public int GetLastPinInput()
        {
            if (_currentPinInput.Count == 0) return -1;
            return _currentPinInput[^1];
        }
        
        public void SetPinThreshold(int n)
        {
            _pinThreshold = n;
        }
        
        public Vector3 GetInitialKeypadPosition()
        {
            return _initialKeypadPosition;
        }

        public void SetIsBehindKeypad(bool value)
        {
            _isHandBehindKeypad = value;
        }

        public WebcamCapture GetWebcam()
        {
            return webcam;
        }

        public void SetIsRecord(bool value)
        {
            _isRecord = value;
        }
        
        public bool GetIsRecord()
        {
            return _isRecord;
        }

        public bool IsQuestionnaireActive()
        {
            return questionnaire.GetIsActive();
        }
        
        public void SwitchHandednessToDominantHand()
        {
            _handedness.switchHand(_handedness.DominantHand == Chirality.Left ? Chirality.Left : Chirality.Right);
        }
        
        public void SwitchHandednessToWeakHand()
        {
            var weakHand = _handedness.DominantHand == Chirality.Left ? Chirality.Right : Chirality.Left;
            if (weakHand == Chirality.Left)
            {
                _handedness.LeftPointer.forceDisable = false;
                _handedness.RightPointer.forceDisable = true;
            }
            else
            {
                _handedness.LeftPointer.forceDisable = true;
                _handedness.RightPointer.forceDisable = false;
            }
            
            _handedness.switchHand(weakHand);
        }

        public void SetupStudyCondition()
        {
            _studySetup.SetupCondition();
        }

          #endregion
    }
    
}