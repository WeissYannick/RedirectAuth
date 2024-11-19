using System;
using Leap.Unity;
using Leap.Unity.HandsModule;
using Leap.Unity.InputModule;
using UnityEngine;

namespace HR_Toolkit
{
    public class Handedness : MonoBehaviour
    {
        public Chirality DominantHand;
        
        public GameObject RealLeftHand;
        public GameObject RealRightHand;
        
        public HandBinder VirtualLeftHand;
        public HandBinder VirtualRightHand;
        
        public PointerElement LeftPointer;
        public PointerElement RightPointer;
        
        private void Start()
        {
            switch (DominantHand)
            {
                case Chirality.Left:
                    ActivateLeftHand();
                    DeactivateRightHand();
                    break;
                case Chirality.Right:
                    ActivateRightHand();
                    DeactivateLeftHand();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void switchHand(Chirality handedness)
        {
            switch(handedness)
            {
                case Chirality.Left:
                    DeactivateRightHand();
                    ActivateLeftHand();
                    break;
                case Chirality.Right:
                    DeactivateLeftHand();
                    ActivateRightHand();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        private void ActivateRightHand()
        {
            RealRightHand.SetActive(true);
            RightPointer.gameObject.SetActive(true);
            DeactivateLeftHand();
            GetComponent<MovementController>().trackedHand = RealRightHand;
            VirtualRightHand.GetComponent<HandEnableDisable>().FreezeHandState = false;
        }
        
        private void DeactivateRightHand()
        {
            RealRightHand.SetActive(false);
            VirtualRightHand.gameObject.SetActive(false);
            RightPointer.gameObject.SetActive(false);
            VirtualRightHand.GetComponent<HandEnableDisable>().FreezeHandState = true;
        }
        
        private void ActivateLeftHand()
        {
            RealLeftHand.SetActive(true);
            LeftPointer.gameObject.SetActive(true);
            DeactivateRightHand();
            GetComponent<MovementController>().trackedHand = RealLeftHand;
            VirtualLeftHand.GetComponent<HandEnableDisable>().FreezeHandState = false;
        }
        
        private void DeactivateLeftHand()
        {
            RealLeftHand.SetActive(false);
            VirtualLeftHand.gameObject.SetActive(false);
            LeftPointer.gameObject.SetActive(false);
            VirtualLeftHand.GetComponent<HandEnableDisable>().FreezeHandState = true;
        }

    }
}