using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Character
{ 
    void Update()
    { 
        if(isAlive)
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
            if (Input.GetKeyDown(KeyCode.Space) && isOnFloor)
            {
                rigidBody.AddForce(new Vector2(0, JumpForce), ForceMode2D.Impulse);
            }

            transform.position += MovementAmount;
        }
    }
}
