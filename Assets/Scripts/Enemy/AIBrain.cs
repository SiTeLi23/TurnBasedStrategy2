using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIBrain : MonoBehaviour
{
    public CharacterMainController charCon;

    public float waitBeforeActing = 1f, waitAfterActing = 1f, waitBeforeShooting = .5f;

    public float moveChance = 60f, defendChance = 25f, skipChance = 15f;

    [Range(0f, 100f)]
    public float ignoreShootChance = 20f, moveRandomChance = 50f;

    public void ChooseAction() 
    {
        StartCoroutine(ChooseCo());
    }

    public IEnumerator ChooseCo() 
    {
        Debug.Log(name + " is choosing an action");

        yield return new WaitForSeconds(waitBeforeActing);

        bool actionTaken = false;

        #region AI Melee

        charCon.GetMeleeTarget();

        if(charCon.meleeTargets.Count > 0) 
        {
            Debug.Log("Is Meleeing");

            charCon.currentMeleeTarget = Random.Range(0, charCon.meleeTargets.Count);

            GameManager.instance.currentActionCost = 1;

            StartCoroutine(WaitToEndAction(waitAfterActing));

            charCon.DoMelee();

            actionTaken = true;

        }

        #endregion


        #region AI Shoot

        charCon.GetShootTargets();

        if (actionTaken == false && charCon.shootTargets.Count > 0)
        {
            //random shoot or not behavior
            if (Random.Range(0f, 100f) > ignoreShootChance)
            {
                List<float> hitChances = new List<float>();

                //get all the potential shoot targets 
                for (int i = 0; i < charCon.shootTargets.Count; i++)
                {
                    //since this system is using shooting ray to detect shot chance, we need to rotate character toward target first, 
                    //that way, if the target is behind the shooter, the ray won't be blocked by the shooter itself

                    charCon.currentShootTarget = i;
                    charCon.LookAtTarget(charCon.shootTargets[i].transform);
                    hitChances.Add(charCon.CheckShotChance());
                }

                //cycle through the list to get the highest chance for target to shoot
                float highestChance = 0f;
                for (int i = 0; i < hitChances.Count; i++)
                {
                    if (hitChances[i] > highestChance)
                    {
                        highestChance = hitChances[i];
                        charCon.currentShootTarget = i;
                    }
                    //if we have same shooting percentage among 2 enemies, we randomly pick one as the highest value
                    else if (hitChances[i] == highestChance)
                    {
                        if (Random.value > .5f)
                        {
                            charCon.currentShootTarget = i;
                        }
                    }
                }

                //if there's at least one character the enemy can shoot
                if (highestChance > 0f)
                {
                    charCon.LookAtTarget(charCon.shootTargets[charCon.currentShootTarget].transform);

                    //camera track to targets
                    CameraController.instance.SetFireView();

                    actionTaken = true;

                    StartCoroutine(WaitToShoot());

                    Debug.Log(name + " shot at " + charCon.shootTargets[charCon.currentShootTarget].name);
                }

            }
        }


        #endregion


        #region AI movement

        if (actionTaken == false) 
        {
            //the chance of random decision
            float actionDecision = Random.Range(0f, moveChance + defendChance + skipChance);
            if (actionDecision < moveChance)
            {

                //the chance of moving randomly to a point
                float moveRandom = Random.Range(0f, 100f);

                List<MovePoint> potentialMovePoints = new List<MovePoint>();
                int selectedPoint = 0;

                if (moveRandom > moveRandomChance)
                {

                    //find the nearest target player
                    int nearestPlayer = 0;

                    for (int i = 1; i < GameManager.instance.playerTeam.Count; i++)
                    {
                        //calculating the nearest player
                        if (Vector3.Distance(transform.position,
                            GameManager.instance.playerTeam[nearestPlayer].transform.position)
                          > Vector3.Distance(transform.position, GameManager.instance.playerTeam[i].transform.position))
                        {
                            nearestPlayer = i;

                        }
                    }



                    //if the target player is outside the walking range of the AI and remaining point is enough,
                    //the AI should be able to choose to run.
                    //otherwise, the AI will only choose to walk.

                    if (Vector3.Distance(transform.position, GameManager.instance.playerTeam[nearestPlayer].transform.position) > charCon.moveRange
                        && GameManager.instance.turnPointsRemaining >= 2)
                    {

                        //get all the moveable points for this AI 
                        potentialMovePoints = MoveGrid.instance.GetMovePointsInRange(charCon.runRange, transform.position);

                        //find the cloest point toward the target player
                        float closestDistance = 1000f;
                        for (int i = 0; i < potentialMovePoints.Count; i++)
                        {
                            if (Vector3.Distance(GameManager.instance.playerTeam[nearestPlayer].transform.position,
                                potentialMovePoints[i].transform.position) < closestDistance)
                            {
                                closestDistance = Vector3.Distance(GameManager.instance.playerTeam[nearestPlayer].transform.position,
                               potentialMovePoints[i].transform.position);

                                selectedPoint = i;
                            }
                        }

                        //run to the cloeset point which near the cloeset target player
                        GameManager.instance.currentActionCost = 2;

                        Debug.Log(name + " is running to " + GameManager.instance.playerTeam[nearestPlayer].name);

                    }
                    else
                    {
                        //get all the moveable points for this AI 
                        potentialMovePoints = MoveGrid.instance.GetMovePointsInRange(charCon.moveRange, transform.position);

                        //find the cloest point toward the target player
                        float closestDistance = 1000f;
                        for (int i = 0; i < potentialMovePoints.Count; i++)
                        {
                            if (Vector3.Distance(GameManager.instance.playerTeam[nearestPlayer].transform.position,
                                potentialMovePoints[i].transform.position) < closestDistance)
                            {
                                closestDistance = Vector3.Distance(GameManager.instance.playerTeam[nearestPlayer].transform.position,
                               potentialMovePoints[i].transform.position);

                                selectedPoint = i;
                            }
                        }

                        //walk to the cloeset point which near the cloeset target player
                        GameManager.instance.currentActionCost = 1;
                        Debug.Log(name + " is walking to " + GameManager.instance.playerTeam[nearestPlayer].name);

                    }

                }
                //randomly walk to a point 
                else
                {
                    potentialMovePoints = MoveGrid.instance.GetMovePointsInRange(charCon.moveRange, transform.position);

                    selectedPoint = Random.Range(0, potentialMovePoints.Count);

                    GameManager.instance.currentActionCost = 1;

                    Debug.Log(name + " is walking to a random spot");
                }

                charCon.MoveToPoint(potentialMovePoints[selectedPoint].transform.position);
            }

            else if (actionDecision < moveChance + defendChance)
            {
                //defending decision

                Debug.Log(name + "is defending");

                charCon.SetDefending(true);

                GameManager.instance.currentActionCost = GameManager.instance.turnPointsRemaining;

                StartCoroutine(WaitToEndAction(waitAfterActing));

            }

            else
            {
                //skip decision

                    Debug.Log(name + "skip turn");
                    GameManager.instance.EndTurn();
            }
        }

        #endregion
    }

    IEnumerator WaitToEndAction(float timeToWait) 
    {
        Debug.Log("waiting To End Action");

        yield return new WaitForSeconds(timeToWait);

        GameManager.instance.SpendTurnPoints();
    }

    IEnumerator WaitToShoot() 
    {
        yield return new WaitForSeconds(waitBeforeShooting);

        charCon.FireShot();

        GameManager.instance.currentActionCost = 1;

        StartCoroutine(WaitToEndAction(waitAfterActing));
    }


}
