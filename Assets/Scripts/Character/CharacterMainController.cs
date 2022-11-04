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
    public AIBrain brain;

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

    public float shootRange, shootDamage;
    [HideInInspector]
    public List<CharacterMainController> shootTargets = new List<CharacterMainController>();
    [HideInInspector]
    public int currentShootTarget;
    public Transform shootPoint;
    public Vector3 shotMissRange;

    public LineRenderer shootLine;
    public float shotRemainTime = .5f;
    private float shotRemainCounter;

    public GameObject shotHitEffect, shotMissEffect;

    public GameObject defendObject;
    public bool isDefending;

    void Start()
    {
        moveTarget = transform.position;

        navAgent.speed = moveSpeed;

        currentHealth = maxHealth;
        UpdateHealthDisplay();

        shootLine.transform.position = Vector3.zero;
        shootLine.transform.rotation = Quaternion.identity;
        shootLine.transform.SetParent(null);
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

        if(shotRemainCounter > 0) 
        {
            shotRemainCounter -= Time.deltaTime;

            if(shotRemainCounter <= 0) 
            {
                shootLine.gameObject.SetActive(false);
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

        //check targets within melee range
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
        if(isDefending == true) 
        {
            damageToTake *= .5f;
        }

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


    public void GetShootTargets() 
    {
        shootTargets.Clear();

        if(isEnemy == false) 
        {
           foreach(CharacterMainController cc in GameManager.instance.enemyTeam) 
            {
                if (Vector3.Distance(transform.position, cc.transform.position) < shootRange) 
                {
                    shootTargets.Add(cc);
                }
            }
        }
        else 
        {
            foreach (CharacterMainController cc in GameManager.instance.playerTeam)
            {
                if (Vector3.Distance(transform.position, cc.transform.position) < shootRange)
                {
                    shootTargets.Add(cc);
                }
            }
        }

        if(currentShootTarget >= shootTargets.Count) 
        {
            currentShootTarget = 0;
        }
    }

    public void FireShot() 
    {
        Vector3 targetPoint = new Vector3( shootTargets[currentShootTarget].transform.position.x, shootTargets[currentShootTarget].shootPoint.position.y, shootTargets[currentShootTarget].transform.position.z);
        
        //random for shooting high or low
        targetPoint.y = Random.Range(targetPoint.y, shootTargets[currentShootTarget].transform.position.y + .25f);

        //potential missed system
        Vector3 targetOffset = new Vector3(Random.Range(-shotMissRange.x, shotMissRange.x),
            Random.Range(-shotMissRange.y, shotMissRange.y),
            Random.Range(-shotMissRange.z, shotMissRange.z));

        //offset will change based on the distance between shooter and target
        targetOffset = targetOffset * ((Vector3.Distance(shootTargets[currentShootTarget].transform.position, transform.position) / shootRange));
        targetPoint += targetOffset;

        Vector3 shootDirection = (targetPoint - shootPoint.position).normalized;

        Debug.DrawRay(shootPoint.position,shootDirection * shootRange, Color.green, 1f);

        RaycastHit hit;
        if(Physics.Raycast(shootPoint.transform.position,shootDirection,out hit, shootRange)) 
        {
            //if we hit a target
            if(hit.collider.gameObject == shootTargets[currentShootTarget].gameObject) 
            {
                shootTargets[currentShootTarget].TakeDamage(shootDamage);

                Instantiate(shotHitEffect, hit.point, Quaternion.identity);
            }
            else 
            {
                //if we hit some obstacles
                PlayerInputMenu.instance.ShowErrorText("Shot Missed!");

                Instantiate(shotMissEffect, hit.point, Quaternion.identity);
            }

            shootLine.SetPosition(0,shootPoint.position);
            shootLine.SetPosition(1, hit.point);
        }        



        //if we don't hit anything
        else 
        {
            PlayerInputMenu.instance.ShowErrorText("Completely Shot Missed!");

            shootLine.SetPosition(0, shootPoint.position);
            shootLine.SetPosition(1, shootPoint.position + (shootDirection*shootRange));
        }

        shootLine.gameObject.SetActive(true);
        shotRemainCounter = shotRemainTime;

    }

    //check shot percentage against current target
    public float CheckShotChance() 
    {
        float shotChance = 0f;

        RaycastHit hit;

        Vector3 targetPoint = new Vector3(shootTargets[currentShootTarget].transform.position.x,
                                          shootTargets[currentShootTarget].shootPoint.position.y, 
                                          shootTargets[currentShootTarget].transform.position.z);

        //if the target's upper body can be shot normally, we gave it a 50% base chance value
        Vector3 shootDirection = (targetPoint - shootPoint.position).normalized;
        Debug.DrawRay(shootPoint.position, shootDirection * shootRange, Color.red, 1f);
        if(Physics.Raycast(shootPoint.position,shootDirection,out hit, shootRange)) 
        {
            if(hit.collider.gameObject == shootTargets[currentShootTarget].gameObject) 
            {
                shotChance += 50f;
            }
        }

        //if this virtual ray can detect the lower part of the target, we add another pecerntage chance
        targetPoint.y = Random.Range(targetPoint.y, shootTargets[currentShootTarget].transform.position.y + .25f);
        shootDirection = (targetPoint - shootPoint.position).normalized;
        Debug.DrawRay(shootPoint.position, shootDirection * shootRange, Color.red, 1f);
        if (Physics.Raycast(shootPoint.position, shootDirection, out hit, shootRange))
        {
            if (hit.collider.gameObject == shootTargets[currentShootTarget].gameObject)
            {
                shotChance += 50f;
            }
        }

        shotChance = shotChance * .95f;
        shotChance *= 1f - (Vector3.Distance(shootTargets[currentShootTarget].transform.position, transform.position) / shootRange);


        return shotChance;
    }

    public void LookAtTarget(Transform target) 
    {
        transform.LookAt(new Vector3(target.position.x, transform.position.y, target.position.z),Vector3.up);
    }

    public void SetDefending(bool shouldDefend) 
    {
        isDefending = shouldDefend;

        defendObject.SetActive(isDefending);
    }

}
