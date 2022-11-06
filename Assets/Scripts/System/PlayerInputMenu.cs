using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerInputMenu : MonoBehaviour
{
    public static PlayerInputMenu instance;
    private void Awake()
    {
        #region Singleton
        if (instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
        }
        #endregion
    }


    public GameObject InputMenu, moveMenu, meleeMenu, shootMenu;
    public TMP_Text turnPointText, errorText;

    public float errorDisplayTime = 2f;
    private float errorCounter;

    public TMP_Text hitChanceText;

    public TMP_Text resultText;
    public GameObject endBattleButton;

    public void HideMenu() 
    {
        InputMenu.SetActive(false);
        moveMenu.SetActive(false);
        meleeMenu.SetActive(false);
        shootMenu.SetActive(false);
    }

    public void ShowInputMenu() 
    {
        InputMenu.SetActive(true);    
    }

    #region Movement Action
    public void ShowMove() 
    {
       if(GameManager.instance.turnPointsRemaining >= 1) 
        {
            MoveGrid.instance.ShowPointsInRange(GameManager.instance.activePlayer.moveRange,GameManager.instance.activePlayer.transform.position);
            GameManager.instance.currentActionCost = 1;
        }

        SFXManager.instance.UISelect.Play();
    }

    public void ShowRun()
    {
        if (GameManager.instance.turnPointsRemaining >= 2)
        {
            MoveGrid.instance.ShowPointsInRange(GameManager.instance.activePlayer.runRange, GameManager.instance.activePlayer.transform.position);
            GameManager.instance.currentActionCost = 2;
        }

        SFXManager.instance.UISelect.Play();
    }

    public void ShowMoveMenu() 
    {
        HideMenu();
        moveMenu.SetActive(true);

        ShowMove();
        SFXManager.instance.UISelect.Play();
    }

    public void HideMoveMenu() 
    {
        HideMenu();
        MoveGrid.instance.HideMovePoint();
        ShowInputMenu();

        SFXManager.instance.UICancel.Play();
    }

    #endregion


    #region Melee Action

    public void ShowMeleeMenu() 
    {
        HideMenu();
        meleeMenu.SetActive(true);
        SFXManager.instance.UISelect.Play();
    }

    public void HideMeleeMenu() 
    {
        HideMenu();
        ShowInputMenu();

        GameManager.instance.targetDisplay.SetActive(false);

        SFXManager.instance.UICancel.Play();
    }

    public void CheckMelee() 
    {
        GameManager.instance.activePlayer.GetMeleeTarget();

        if (GameManager.instance.activePlayer.meleeTargets.Count > 0) 
        {
            ShowMeleeMenu();

            GameManager.instance.targetDisplay.SetActive(true);
            GameManager.instance.targetDisplay.transform.position = GameManager.instance.activePlayer.meleeTargets[GameManager.instance.activePlayer.currentMeleeTarget].transform.position;

            //look at target
            GameManager.instance.activePlayer.LookAtTarget(GameManager.instance.activePlayer.meleeTargets[GameManager.instance.activePlayer.currentMeleeTarget].transform);
        }
        else 
        {
            ShowErrorText("There is no enemy in melee range");

            SFXManager.instance.UICancel.Play();
        }
    }

    public void MeleeHit() 
    {
        GameManager.instance.activePlayer.DoMelee();
        GameManager.instance.currentActionCost = 1;

        HideMenu();

        GameManager.instance.targetDisplay.SetActive(false);
        StartCoroutine(WaitToEndActionCo(1f));

        SFXManager.instance.UISelect.Play();
    }

    public void NextMeleeTarget() 
    {
        GameManager.instance.activePlayer.currentMeleeTarget++;
        if(GameManager.instance.activePlayer.currentMeleeTarget >= GameManager.instance.activePlayer.meleeTargets.Count) 
        {
            GameManager.instance.activePlayer.currentMeleeTarget = 0;
        }
        GameManager.instance.targetDisplay.transform.position = GameManager.instance.activePlayer.meleeTargets[GameManager.instance.activePlayer.currentMeleeTarget].transform.position;

        //look at target
        GameManager.instance.activePlayer.LookAtTarget(GameManager.instance.activePlayer.meleeTargets[GameManager.instance.activePlayer.currentMeleeTarget].transform);

        SFXManager.instance.UISelect.Play();
    }

    #endregion


    #region Shoot Action

    public void ShowShootMenu()
    {
        HideMenu();
        shootMenu.SetActive(true);

        UpdateHitChance();
        SFXManager.instance.UISelect.Play();
    }

    public void HideShootMenu()
    {
        HideMenu();
        ShowInputMenu();

        GameManager.instance.targetDisplay.SetActive(false);

        CameraController.instance.SetMoveTarget(GameManager.instance.activePlayer.transform.position);

        SFXManager.instance.UICancel.Play();
    }

    public void CheckShoot() 
    {
        GameManager.instance.activePlayer.GetShootTargets();

        if(GameManager.instance.activePlayer.shootTargets.Count > 0) 
        {
            ShowShootMenu();

            //handle selected indicator
            GameManager.instance.targetDisplay.SetActive(true);
            GameManager.instance.targetDisplay.transform.position = GameManager.instance.activePlayer.shootTargets[GameManager.instance.activePlayer.currentShootTarget].transform.position;

            //look at target
            GameManager.instance.activePlayer.LookAtTarget(GameManager.instance.activePlayer.shootTargets[GameManager.instance.activePlayer.currentShootTarget].transform);

            //camera rotate to target
            CameraController.instance.SetFireView();
        }
        else 
        {
            ShowErrorText("There is no enemy in shoot range");

            SFXManager.instance.UICancel.Play();
        }
    }

    public void NextShootTarget() 
    {
        GameManager.instance.activePlayer.currentShootTarget++;

        if (GameManager.instance.activePlayer.currentShootTarget >= GameManager.instance.activePlayer.shootTargets.Count)
        {
            GameManager.instance.activePlayer.currentShootTarget = 0;
        }

        //handle selected indicator
        GameManager.instance.targetDisplay.transform.position = GameManager.instance.activePlayer.shootTargets[GameManager.instance.activePlayer.currentShootTarget].transform.position;

        UpdateHitChance();
        
        //look at target
        GameManager.instance.activePlayer.LookAtTarget(GameManager.instance.activePlayer.shootTargets[GameManager.instance.activePlayer.currentShootTarget].transform);

        //camera rotate to target
        CameraController.instance.SetFireView();

        SFXManager.instance.UISelect.Play();
    }

    public void FireShot() 
    {
        GameManager.instance.activePlayer.FireShot();

        GameManager.instance.currentActionCost = 1;
        HideMenu();

        GameManager.instance.targetDisplay.SetActive(false);

        StartCoroutine(WaitToEndActionCo(1f));

        SFXManager.instance.UISelect.Play();
    }

    public void UpdateHitChance() 
    {
        float hitChance = Random.Range(50f, 95f);

        hitChanceText.text = "Chance To Hit: " + GameManager.instance.activePlayer.CheckShotChance().ToString("F1") + "%";
        
    }



    #endregion


    #region Defending Action

    public void Defend() 
    {
        GameManager.instance.activePlayer.SetDefending(true);
        GameManager.instance.EndTurn();

        SFXManager.instance.UISelect.Play();
    }

    #endregion



    public void UpdateTurnPointText(int turnPoints) 
    {
        turnPointText.text = "Tunr Points Remaining: "+ turnPoints;
    }

    public void SkipTurn() 
    {
        GameManager.instance.EndTurn();
        SFXManager.instance.UISelect.Play();
    }

    public IEnumerator WaitToEndActionCo(float timeToWait) 
    {
        yield return new WaitForSeconds(timeToWait);

        GameManager.instance.SpendTurnPoints();

        //set back to normal camera view
        CameraController.instance.SetMoveTarget(GameManager.instance.activePlayer.transform.position);
    }

    public void ShowErrorText(string messageToShow) 
    {
        errorText.text = messageToShow;
        errorText.gameObject.SetActive(true);

        errorCounter = errorDisplayTime;

    }

    private void Update()
    {
        if (errorCounter > 0) 
        {
            errorCounter -= Time.deltaTime;

            if(errorCounter <= 0) 
            {
                errorText.gameObject.SetActive(false);
            }
        }
    }

    public void LeaveBattle() 
    {
        GameManager.instance.LeaveBattle();
    }



}
