using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using TMPro;
using UnityEngine.UI;

public class CharacterMainController : MonoBehaviour
{
    public float moveSpeed;
    private Vector3 moveTarget;

    public NavMeshAgent navAgent;
    private bool isMoving;

    public bool isEnemy;

    public float moveRange = 3.5f, runRange = 8f;

    public float meleeRange = 2f;
    
    [HideInInspector]
    public List<CharacterMainController> meleeTargets = new List<CharacterMainController>();
    [HideInInspector]
    public int currentMeleeTarget;
    public float meleeDamage = 5f;

    public float maxHealth = 10f;
    [HideInInspector]
    public float currentHealth;
    public TMP_Text healthText;
    public  Slider healthSlider;

    void Start()
    {
        moveTarget = transform.position;

        navAgent.speed = moveSpeed;

        currentHealth = maxHealth;
        UpdateHealthDisplay();
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

    //movement action
    public void MoveToPoint(Vector3 pointToMoveTo) 
    {
        moveTarget = pointToMoveTo;

        navAgent.SetDestination(moveTarget);
        isMoving = true;
    }

    //get melee target
    public void GetMeleeTarget() 
    {
        meleeTargets.Clear();

        //check enemies in melee range
        if(isEnemy == false) 
        {
          foreach(CharacterMainController cc in GameManager.instance.enemyTeam) 
            {
               if(Vector3.Distance(transform.position,cc.transform.position) < meleeRange) 
                {
                    meleeTargets.Add(cc);
                }
            }
        }
        else 
        {
            foreach (CharacterMainController cc in GameManager.instance.playerTeam)
            {
                if (Vector3.Distance(transform.position, cc.transform.position) < meleeRange)
                {
                    meleeTargets.Add(cc);
                }
            }
        }

        //make sure it remember the last selected melee target
        if(currentMeleeTarget >= meleeTargets.Count) 
        {
            currentMeleeTarget = 0;
        }
    }

    public void DoMelee() 
    {
        meleeTargets[currentMeleeTarget].TakeDamage(meleeDamage);
    }

    public void TakeDamage(float damageToTake) 
    {
        currentHealth -= damageToTake;

        if(currentHealth <= 0) 
        {
            currentHealth = 0;

            navAgent.enabled = false;

            transform.rotation = Quaternion.Euler(-70f, transform.rotation.eulerAngles.y, 0f);

            GameManager.instance.allChars.Remove(this);
            if (GameManager.instance.playerTeam.Contains(this)) 
            {
                GameManager.instance.playerTeam.Remove(this);
            }
            if (GameManager.instance.enemyTeam.Contains(this))
            {
                GameManager.instance.enemyTeam.Remove(this);
            }

        }

        UpdateHealthDisplay();
    }

    public void UpdateHealthDisplay() 
    {
        healthText.text = "HP: " + currentHealth + "/" + maxHealth;

        healthSlider.maxValue = maxHealth;
        healthSlider.value = currentHealth;

    }

}
