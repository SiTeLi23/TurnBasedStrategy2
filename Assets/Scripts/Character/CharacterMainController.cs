using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CharacterMainController : MonoBehaviour
{
    public float moveSpeed;
    private Vector3 moveTarget;

    public NavMeshAgent navAgent;
    private bool isMoving;

    public bool isEnemy;

    public float moveRange = 3.5f, runRange = 8f;

    void Start()
    {
        moveTarget = transform.position;

        navAgent.speed = moveSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        //moving to a point
        if (isMoving == true)
        {
            if(GameManager.instance.activePlayer == this) 
            {
                //camera follow
                CameraController.instance.SetMoveTarget(transform.position);

                if (Vector3.Distance(transform.position, moveTarget) < .2f)
                {
                    isMoving = false;
                    GameManager.instance.FinishMovement();
                }
            }
        }
    }

    //click to move
    public void MoveToPoint(Vector3 pointToMoveTo) 
    {
        moveTarget = pointToMoveTo;

        navAgent.SetDestination(moveTarget);
        isMoving = true;
    }


}
