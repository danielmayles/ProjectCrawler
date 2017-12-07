using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllerablePlayer : Player
{
    private Vector3 OldPos;

    void Update()
    {
        isAlive = true;
        if (isAlive)
        {
            Vector3 MovementAmount = new Vector3();
            if (Input.GetKey(KeyCode.A))
            {
                MovementAmount.x -= Speed * Time.deltaTime;
            }
            if (Input.GetKey(KeyCode.D))
            {
                MovementAmount.x += Speed * Time.deltaTime;
            }
            if(Input.GetKeyDown(KeyCode.Space))
            {
                if (isRagdolling)
                {
                    StopRagdoll();
                }
                else
                {
                    Ragdoll();
                }       
            }

            transform.position += MovementAmount;
            CharacterModel.transform.forward = MovementAmount.normalized + (Vector3.forward * -1);
            if (transform.position != OldPos)
            {
                NetworkPacketSender.SendPlayerPosition(GetPlayerConnectionID(), transform.position);
                OldPos = transform.position;
            }
        }
    }
}
