using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllerablePlayer : Player
{
    private Vector3 OldPos;
    private Vector3 OldRot;

    void Update()
    {
        isAlive = true;
        if (isAlive)
        {
            Vector3 MovementAmount = new Vector3();
            if (!isRagdolling)
            {
                if (Input.GetKey(KeyCode.A))
                {
                    MovementAmount.x -= Speed * Time.deltaTime;
                }
                if (Input.GetKey(KeyCode.D))
                {
                    MovementAmount.x += Speed * Time.deltaTime;
                }
            }
            if(Input.GetKeyDown(KeyCode.Space))
            {
                if (isRagdolling)
                {
                    StopRagdoll(HipRigidBody.transform.position);
                }
                else
                {
                    Jump(MovementAmount.normalized);
                }       
            }

            transform.position += MovementAmount;
            CharacterModel.transform.forward = MovementAmount.normalized + (Vector3.forward * -1);
            if (transform.position != OldPos || transform.eulerAngles != OldRot)
            {
                NetworkPacketSender.SendPlayerTransform(GetPlayerConnectionID(), transform);
                OldPos = transform.position;
                OldRot = transform.eulerAngles;
            }
        }
    }

    public override void Jump(Vector3 Direction)
    {
        base.Jump(Direction);
        NetworkPacketSender.SendPlayerJump(GetPlayerConnectionID(), Direction);
    }

    public override void Ragdoll()
    {
        base.Ragdoll();
        NetworkPacketSender.SendRagdollPlayer(GetPlayerConnectionID());
    }

    public override void StopRagdoll(Vector3 RagdollPosition)
    {
        base.StopRagdoll(RagdollPosition);
        NetworkPacketSender.SendStopRagdollPlayer(GetPlayerConnectionID(), RagdollPosition);
    }
}
