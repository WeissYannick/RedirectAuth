using System;
using Leap;
using Leap.Unity;
using UnityEngine;

namespace HR_Toolkit
{

    public class LeapMotionHandProjector : PostProcessProvider
    {

        [Header("Projection")]
        public Transform headTransform;

        public Vector3 testPos;

        public override void ProcessFrame(ref Frame inputFrame)
        {
            if (headTransform == null) { headTransform = Camera.main.transform; }

            foreach (var hand in inputFrame.Hands)
            {
                if(RedirectionManager.instance == null) return; 
                hand.SetTransform(RedirectionManager.instance.virtualHand.transform.position, hand.Rotation);
                // hand.SetTransform(testPos, hand.Rotation);
            }
        }
    }
}

