using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    public static GameManager instance;

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

    public CharacterMainController activePlayer;

    public List<CharacterMainController> allChars = new List<CharacterMainController>();
    public List<CharacterMainController> playerTeam = new List<CharacterMainController>();
    public List<CharacterMainController> enemyTeam = new List<CharacterMainController>();

    private int currentChar;

    public int totalTurnPoints = 2;
    [HideInInspector]
    public int turnPointsRemaining;

    public int currentActionCost = 1;

    void Start()
    {
        List<CharacterMainController> tempList = new List<CharacterMainController>();

        //randomly decide the starting unit
        tempList.AddRange(FindObjectsOfType<CharacterMainController>());

        int iterations = tempList.Count + 50;
        while(tempList.Count > 0 && iterations>0) 
        {
            int randomPick = Random.Range(0, tempList.Count);
            allChars.Add(tempList[randomPick]);

            tempList.RemoveAt(randomPick);
            iterations--;
        }

        foreach(CharacterMainController cc in allChars) 
        {
           if(cc.isEnemy == false) 
            {
                playerTeam.Add(cc);
            }
            else 
            {
                enemyTeam.Add(cc);
            }
        }

        //reorder list so enemy will only move after all player moved;
        allChars.Clear();

        //randomly decide unit movement order
        if (Random.value >= 0.5f)
        {
            allChars.AddRange(playerTeam);
            allChars.AddRange(enemyTeam);
        }
        else 
        {
            allChars.AddRange(enemyTeam);
            allChars.AddRange(playerTeam);
        }

        activePlayer = allChars[0]; //default first characer
        CameraController.instance.SetMoveTarget(activePlayer.transform.position);

        currentChar = -1;
        EndTurn();
    }

    
    void Update()
    {
        
    }


    public void FinishMovement() 
    {
        SpendTurnPoints();
    }


    public void SpendTurnPoints() 
    {
        turnPointsRemaining -= currentActionCost;

        if(turnPointsRemaining <= 0) 
        {
            EndTurn();
        }
        else 
        {
            //if there's still remaining points, show valid grid
            if (activePlayer.isEnemy == false)
            {
                //show grid
                //MoveGrid.instance.ShowPointsInRange(activePlayer.moveRange, activePlayer.transform.position);

                PlayerInputMenu.instance.ShowInputMenu();
            }
            else
            {
                PlayerInputMenu.instance.HideMenu();
            }
        }

        PlayerInputMenu.instance.UpdateTurnPointText(turnPointsRemaining);
    }


    public void EndTurn() 
    {
        currentChar++;
        if(currentChar >= allChars.Count) 
        {
            currentChar = 0;
        }

        activePlayer = allChars[currentChar];

        CameraController.instance.SetMoveTarget(activePlayer.transform.position);

        //recover turn points
        turnPointsRemaining = totalTurnPoints;

        //check whether this characer is enemy
        if(activePlayer.isEnemy == false) 
        {
            //show grid
            //MoveGrid.instance.ShowPointsInRange(activePlayer.moveRange, activePlayer.transform.position);

            PlayerInputMenu.instance.ShowInputMenu();
            PlayerInputMenu.instance.turnPointText.gameObject.SetActive(true);
        }
        else 
        {
            PlayerInputMenu.instance.HideMenu();
            PlayerInputMenu.instance.turnPointText.gameObject.SetActive(false);
            
            StartCoroutine(AISkipCo());
        }

        currentActionCost = 1;

        PlayerInputMenu.instance.UpdateTurnPointText(turnPointsRemaining);
    }

    public IEnumerator AISkipCo() 
    {
        yield return new WaitForSeconds(1f);

        EndTurn();
    }
}
