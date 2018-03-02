using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerIKController : MonoBehaviour
{
    private Vector3 LeftHandTarget = Vector3.zero;
    private Vector3 RightHandTarget = Vector3.zero;
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }
    void OnAnimatorIK()
    {
        if (animator != null)
        {
            animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1);
            animator.SetIKPosition(AvatarIKGoal.RightHand, RightHandTarget);
            animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1);
            animator.SetIKPosition(AvatarIKGoal.LeftHand, LeftHandTarget);
        }
    }

    public void SetHandTargetPositions(Vector3 RightHandPos, Vector3 LeftHandPos)
    {
        RightHandTarget = RightHandPos;
        LeftHandTarget = LeftHandPos;
    }
}
