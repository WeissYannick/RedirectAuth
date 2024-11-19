using System.Collections.Generic;
using System.Linq;
using HR_Toolkit.Redirection;
using Leap.Unity;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

namespace HR_Toolkit
{
    public class RedirectionObject : MonoBehaviour
    {
        public List<VirtualToRealConnection> positions;

        [Space]
        [Header("Optional Settings:")]
        [Tooltip(
            "Optional - If no redirection technique is selected, the default technique that is set in the Redirection Manager will be used. Otherwise the selected technique will only be used on this object.")]
        public HandRedirector redirectionTechnique;

        [Tooltip(
            "Optional - Can be used if you want to use a specif warp origin just for this redirection object that differs from the default warp origin")]
        public GameObject warpOrigin;

        public RedirectionObject resetPosition;
        public bool useResetPosition;
        public bool thisIsAResetPosition;

        [Space] [Header("Redirection Events:")]
        public UnityEvent onRedirectionActivated;

        public UnityEvent onRedirectionDeactivated;

        private Color _startColor; 
        private Vector3 _initialPosition;
        private Vector3 _offset;
        
        private void Start()
        {
            if (redirectionTechnique == null)
                redirectionTechnique = RedirectionManager.instance.GetDefaultRedirectionTechnique();

            if (warpOrigin == null) warpOrigin = RedirectionManager.instance.GetDefaultWarpOrigin();

            tag = "virtualTarget";
            gameObject.layer = LayerMask.NameToLayer("Virtual/Object");

            foreach (var prefabCorrespondent in gameObject.GetComponentsInChildren<VirtualToRealConnection>())
            {
                if (positions.Contains(prefabCorrespondent)) continue;

                positions.Add(prefabCorrespondent);
            }
            _initialPosition = transform.position;
        }

        public void Redirect()
        {
            redirectionTechnique.ApplyRedirection(RedirectionManager.instance.realHand.transform,
                RedirectionManager.instance.virtualHand.transform, RedirectionManager.instance.warpOrigin.transform,
                this, RedirectionManager.instance.body.transform);
        }

        public void StartRedirection()
        {
            onRedirectionActivated.Invoke();
            redirectionTechnique.Init(this, RedirectionManager.instance.body.transform,
                RedirectionManager.instance.warpOrigin.transform, RedirectionManager.instance.realHand.transform, this);
            HighlightOn();
        }

        public void EndRedirection()
        {
            onRedirectionDeactivated.Invoke();
            HighlightOff();
            redirectionTechnique.EndRedirection();
            if (useResetPosition) resetPosition.HighlightOff();
        }


        #region Private helpers

        private void OnHandEnter()
        {
            if (!useResetPosition || RedirectionManager.instance.target != this) return;

            HighlightOff();
            resetPosition.HighlightOn();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("virtualHand") || thisIsAResetPosition) return;
            OnHandEnter();
        }

        private void HighlightOn()
        {
            if (!RedirectionManager.instance.isHighlightActive) return;
            // _startColor = GetComponent<Renderer>().material.color;
            // GetComponent<Renderer>().material.color = Color.yellow;
            _startColor = GetComponentsInChildren<Renderer>()
                .FirstOrDefault(r => r.material != null)!.material.color;
            GetComponentsInChildren<Renderer>().ForEach( r => r.material.color = Color.yellow);
        }

        private void HighlightOff()
        {
            if (!RedirectionManager.instance.isHighlightActive) return;
            // GetComponent<Renderer>().material.color = _startColor;
            GetComponentsInChildren<Renderer>().ForEach( r => r.material.color = _startColor);
        }

        #endregion

        #region Getter & Setter

        public HandRedirector GetRedirectionTechnique()
        {
            return redirectionTechnique;
        }

        public GameObject GetWarpOrigin()
        {
            return warpOrigin;
        }

        public Vector3 GetVirtualTargetPos()
        {
            if (positions[0] != null) return positions[0].virtualPosition.position;
            if (positions[0] != null)
                Debug.LogWarning(
                    "The RedirectionObject " + gameObject.name +
                    "is missing a VirtualToRealConnection. Make sure one is placed as a child object and it is assigned in the positions array!",
                    transform);
            Debug.Log("virualTarget: " + positions[0].virtualPosition.name);
            return positions[0].virtualPosition.position;
        }

        public Vector3 GetRealTargetPos()
        {
            if (positions[0] == null)
                Debug.LogWarning(
                    "The RedirectionObject " + gameObject.name +
                    "is missing a VirtualToRealConnection. Make sure one is placed as a child object and it is assigned in the positions array!",
                    transform);

            Vector3 realTargetPos;

            if (RedirectionManager.instance.isRandomTarget)
            {
                if (RedirectionManager.instance.isRandomVector) 
                {
                    var randomVector = RedirectionManager.instance.randomVector;
                    
                    _offset = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0).normalized;
                    var maxRandomVectorLength = Mathf.Tan(RedirectionManager.instance.maxRedirectionAngle * Mathf.Deg2Rad) 
                                                * RedirectionManager.instance.threshold;
                    // _offset *= Random.Range(0f, maxRandomVectorLength);
                    _offset *= maxRandomVectorLength;
                    
                    randomVector.transform.position = transform.position + _offset;
                    realTargetPos = randomVector.transform.position;
                }
                else
                {
                    var realTarget = positions[Random.Range(-1, positions.Count)];
                    realTargetPos = realTarget.realPosition.position;
                    Debug.Log("Random Target: " + realTarget.name);
                }
            }
            else
            {
                realTargetPos = positions[0].realPosition.position;
            }

            return realTargetPos;
        }

        public Quaternion GetVirtualRot()
        {
            return positions[0].virtualPosition.rotation;
        }

        public Quaternion GetRealRot()
        {
            return positions[0].realPosition.rotation;
        }

        public List<VirtualToRealConnection> GetAllPositions()
        {
            return positions;
        }

        public Vector3 GetRealTargetForwardVector()
        {
            return positions[0].realPosition.forward;
        }

        public Vector3 GetVirtualTargetForwardVector()
        {
            return positions[0].virtualPosition.forward;
        }

        public bool UseResetPosition()
        {
            return useResetPosition;
        }

        public RedirectionObject GetResetPosition()
        {
            return resetPosition;
        }

        public GameObject GetRealTargetObject()
        {
            return positions[0].realPosition.gameObject;
        }

        public GameObject GetVirtualTargetObject()
        {
            return positions[0].virtualPosition.gameObject;
        }

        public void DeactivateTarget()
        {
            RedirectionManager.instance.DeactivateTarget();
        }
        
        public Vector3 GetInitialPosition()
        {
            return _initialPosition;
        }

        public float GetRedirectionDistance()
        {
            return _offset.magnitude;
        }
        
        public Vector3 GetOffsetVector()
        {
            return _offset;
        }

        #endregion
    }
}