using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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

    public GameObject targetDisplay;

    public bool shouldSpawnAtRandomPoints;
    public List<Transform> playerSpawnPoints = new List<Transform>();
    public List<Transform> enemySpawnPoints = new List<Transform>();

    public bool matchEnded;

    public string levelToLoad;

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

        //randomly spawn player if its random spawn mode
        if (shouldSpawnAtRandomPoints) 
        {
           foreach(CharacterMainController cc in playerTeam) 
            {
               if(playerSpawnPoints.Count > 0) 
                {
                    int pos = Random.Range(0, playerSpawnPoints.Count);

                    cc.transform.position = playerSpawnPoints[pos].position;
                    //removed taken position
                    playerSpawnPoints.RemoveAt(pos);
                }
            }

            foreach (CharacterMainController cc in enemyTeam)
            {
                if (enemySpawnPoints.Count > 0)
                {
                    int pos = Random.Range(0, enemySpawnPoints.Count);

                    cc.transform.position = enemySpawnPoints[pos].position;
                    //removed taken position
                    enemySpawnPoints.RemoveAt(pos);
                }
            }
        }


        //camera movement
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

        CheckForVictory();

        if (matchEnded == false)
        {

            if (turnPointsRemaining <= 0)
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

                    activePlayer.brain.ChooseAction();
                }
            }
 
        }
        
        PlayerInputMenu.instance.UpdateTurnPointText(turnPointsRemaining);
    }


    public void EndTurn() 
    {
        CheckForVictory();

        if (matchEnded == false)
        {
            currentChar++;
            if (currentChar >= allChars.Count)
            {
                currentChar = 0;
            }

            activePlayer = allChars[currentChar];

            CameraController.instance.SetMoveTarget(activePlayer.transform.position);

            //recover turn points
            turnPointsRemaining = totalTurnPoints;

            //check whether this characer is enemy
            if (activePlayer.isEnemy == false)
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

                activePlayer.brain.ChooseAction();
            }

            currentActionCost = 1;

            PlayerInputMenu.instance.UpdateTurnPointText(turnPointsRemaining);

            activePlayer.SetDefending(false);
        }
    }

    public IEnumerator AISkipCo() 
    {
        yield return new WaitForSeconds(1f);

        EndTurn();
    }

    public void CheckForVictory() 
    {
        bool allDead = true;

        foreach(CharacterMainController cc in playerTeam) 
        {
            //player survived
            if(cc.currentHealth > 0) 
            {
                allDead = false;
            }
        }

        if(allDead == true) 
        {
            //player all dead
            PlayerLoses();
        }
        else 
        {
            allDead = true;
            foreach (CharacterMainController cc in enemyTeam)
            {
                if (cc.currentHealth > 0)
                {
                    //enemy survived
                    allDead = false;
                }
            }
            if (allDead)
            {
                //enemy all dead
                PlayerWins();
            }
        }

    }

    public void PlayerWins() 
    {
        Debug.Log("Player wins");

        matchEnded = true;

        PlayerInputMenu.instance.resultText.text = "You Wins!";
        PlayerInputMenu.instance.resultText.gameObject.SetActive(true);

        PlayerInputMenu.instance.endBattleButton.SetActive(true);
        PlayerInputMenu.instance.turnPointText.gameObject.SetActive(false);

    }

    public void PlayerLoses() 
    {
        Debug.Log("Player loses");

        matchEnded = true;

        PlayerInputMenu.instance.resultText.text = "You Lost!";
        PlayerInputMenu.instance.resultText.gameObject.SetActive(true);

        PlayerInputMenu.instance.endBattleButton.SetActive(true);
        PlayerInputMenu.instance.turnPointText.gameObject.SetActive(false);
    }

    public void LeaveBattle() 
    {
        SceneManager.LoadScene(levelToLoad);
    }
}
