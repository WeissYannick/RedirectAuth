﻿
using System.Numerics;
using HR_Toolkit.Redirection;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace HR_Toolkit
{
    /// <summary>
    /// Cheng et al. extended Azmandian et al.’s work (see Azmandian_BodyWarping.cs). They created a so called Sparse
    /// Haptic Proxy which is a set of geometric primitives that simulate touch feedback for various objects in a virtual
    /// reality scenario. The hand gets retargeted by a body warp.   
    ///
    /// We use their redirection approach in our toolkit.
    /// To define the warping ratio, Cheng computes the distance vector T between the real and virtual target. Then he gradually
    /// adds this offset to th the virtual hand position, depending on the distance between the real hand and the real target.
    /// This is the shift ratio a, which ranges between 0 and 1 from the beginning of the motion to a full offset T when the
    /// virtual hand touches the virtual target and the real hand the real target.
    ///
    /// They also implemented a zero warp distance. The general retargeting should be reduced to zero whenever the users
    /// retract their hand close to the body. So the hand retargeting starts only when the user's hand crosses a given distance D.
    ///
    /// More information:
    /// Lung-Pan Cheng et al.,  Sparse Haptic Proxy: Touch Feedback in Virtual EnvironmentsUsing a General Passive Prop.
    /// InProceedings of the 2017 CHI Conference on Human Factors in Computing Systems (CHI ’17). ACM, New York, NY, USA,3718–3728.
    /// DOI: http://dx.doi.org/10.1145/3025453.3025753
    /// 
    /// </summary>
    public class Cheng_BodyWarping : BodyWarping
    {
        /// <summary>
        /// Distance vector between the virtual and physical target
        /// </summary>
        private Vector3 _t; 
        /// <summary>
        /// the null vector
        /// </summary>
        private Vector3 _t0;
        /// <summary>
        /// Checks if the real hand stayed in the Zero Warp Zone
        /// </summary>
        private bool _isInZeroWarpZone = false;
        /// <summary>
        /// distance in m from player position where no warping is applied
        /// </summary>
        public float zeroWarpDistance = 0;

        private Vector3 _realTargetPos;

        public override void ApplyRedirection(Transform realHandPos, Transform virtualHandPos, 
            Transform warpOrigin, RedirectionObject target, Transform playerTransform)
        {
            var ds = 0f;
            // compute the distance between the real hand position and the users body position
            var dist = Vector3.Distance(realHandPos.position, playerTransform.position);
            // TODO update comments
            // check whether the hand is below the zero warp distance or not. If so, set ds to zero,
            if (dist < zeroWarpDistance)
            {
                ds = 0;
                _isInZeroWarpZone = true;
            }
            // else set ds to the length between the physical hand position Hp and the warping origin H0
            else
            {
                // the hand left the zero warp zone, set this point to the warp origin
                if (_isInZeroWarpZone)
                {
                    RedirectionManager.instance.SetWarpOrigin(realHandPos.position);
                }
                ds = (realHandPos.position - warpOrigin.position).magnitude;
                _isInZeroWarpZone = false;
            }
            // compute the length between the physical hand and the physical target
            var dp = (realHandPos.position - _realTargetPos).magnitude;
            // compute the shift ratio, it ranges between 0 and 1
            var a = ds / (ds + dp);
            // compute the warp 
            var w = a * _t + (1 - a) * _t0;
            // applay the warp to the virtual hand
            virtualHandPos.position = realHandPos.position + w;
        }

        public override void Init(RedirectionObject redirectionObject, Transform head, Transform warpOrigin1,
            Transform realHandPos, RedirectionObject target)
        {

            _realTargetPos = target.GetRealTargetPos();

            // compute the distance vector between the virtual and physical target
            // _t = redirectionObject.GetVirtualTargetPos() - redirectionObject.GetRealTargetPos();
            _t = target.GetVirtualTargetPos() - _realTargetPos;
            
        }
    }
}
