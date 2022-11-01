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


    public GameObject InputMenu, moveMenu;
    public TMP_Text turnPointText;

    public void HideMenu() 
    {
        InputMenu.SetActive(false);
        moveMenu.SetActive(false);
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
    }

    public void ShowRun()
    {
        if (GameManager.instance.turnPointsRemaining >= 2)
        {
            MoveGrid.instance.ShowPointsInRange(GameManager.instance.activePlayer.runRange, GameManager.instance.activePlayer.transform.position);
            GameManager.instance.currentActionCost = 2;
        }
    }

    public void ShowMoveMenu() 
    {
        HideMenu();
        moveMenu.SetActive(true);

        ShowMove();
    }

    public void HideMoveMenu() 
    {
        HideMenu();
        MoveGrid.instance.HideMovePoint();
        ShowInputMenu();
    }

    #endregion

    public void UpdateTurnPointText(int turnPoints) 
    {
        turnPointText.text = "Tunr Points Remaining: "+ turnPoints;
    }

    public void SkipTurn() 
    {
        GameManager.instance.EndTurn();
    }

}
