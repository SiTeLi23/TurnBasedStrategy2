using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIBrain : MonoBehaviour
{
    public CharacterMainController charCon;

    public float waitBeforeActing = 1f, waitAfterActing = 1f, waitBeforeShooting = .5f;

    [Range(0f,100f)]
    public float ignoreShootChance = 20f;

    public void ChooseAction() 
    {
        StartCoroutine(ChooseCo());
    }

    public IEnumerator ChooseCo() 
    {
        Debug.Log(name + "is choosing an action");

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


        if (actionTaken == false) 
        {
            Debug.Log(name + "skip turn");
            GameManager.instance.EndTurn();   
        }
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
