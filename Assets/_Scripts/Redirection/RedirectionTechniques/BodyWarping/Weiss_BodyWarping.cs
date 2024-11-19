using UnityEngine;

namespace HR_Toolkit
{
    public class Weiss_BodyWarping : BodyWarping
    {
        private Vector3 _realTarget;
        private Vector3 _virtualTarget;
        
        private Vector3 _retargetingVector = Vector3.zero;

        public AnimationCurve retargetingCurve;
        
        public override void Init(RedirectionObject redirectionObject, Transform head, Transform warpOrigin,
            Transform realHandPos, RedirectionObject target)
        {
            _realTarget = target.GetRealTargetPos();
            _virtualTarget = target.GetVirtualTargetPos();
        } 
        
        public override void ApplyRedirection(Transform realHandPos, Transform virtualHandPos, Transform warpOrigin, RedirectionObject target,
            Transform bodyTransform)
        {
            var targetHandDistance = _realTarget.z - realHandPos.position.z;
            var targetWarpDistance = _realTarget.z - warpOrigin.position.z;
            
            var progress = 1 - (targetHandDistance / targetWarpDistance);
            
            Vector3 newRetargetingVector = Vector3.Lerp(Vector3.zero, _virtualTarget - _realTarget, 
                retargetingCurve.Evaluate(progress));
            
        
            if (newRetargetingVector.magnitude >= _retargetingVector.magnitude) {
                _retargetingVector = newRetargetingVector;
            }
            else
            {
                _retargetingVector /= (_retargetingVector.magnitude / newRetargetingVector.magnitude);
            }

            var newPosition = realHandPos.position + _retargetingVector;
            virtualHandPos.position = newPosition;
        }
        
    }
}
