using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveGrid : MonoBehaviour
{

    public static MoveGrid instance;


    public MovePoint startPoint;

    public Vector2Int spawnRange;

    public LayerMask whatIsGround,whatIsObstacle;

    public float obstacleCheckRange;

    public List<MovePoint> allMovePoints = new List<MovePoint>();

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

        GenerateMoveGrid();

        HideMovePoint();
    }
    void Start()
    {
 
    }


    void Update()
    {
        
    }

    //Generate moveable grid
    public void GenerateMoveGrid() 
    {
       for(int x = -spawnRange.x; x <= spawnRange.x; x++) 
        {
            for(int y = -spawnRange.y; y <= spawnRange.y; y++) 
            {
                RaycastHit hit;
                if (Physics.Raycast(transform.position + new Vector3(x, 10f, y), Vector3.down, out hit, 20f, whatIsGround)) 
                {
                    //check obstacle
                    if (Physics.OverlapSphere(hit.point, obstacleCheckRange, whatIsObstacle).Length == 0)
                    {
                        MovePoint newPoint = Instantiate(startPoint, transform.position + hit.point, transform.rotation);
                        newPoint.transform.SetParent(transform);

                        allMovePoints.Add(newPoint);
                    }
                }


            }
        }

        startPoint.gameObject.SetActive(false);
    }

    //Hide unnecessary grid
    public void HideMovePoint() 
    {
       foreach(MovePoint mp in allMovePoints) 
        {
            mp.gameObject.SetActive(false);
        }
    }

    //Show all valid move point which is in this character's moving range
    public void ShowPointsInRange(float moveRange, Vector3 centerPoint) 
    {
        HideMovePoint();

        foreach(MovePoint mp in allMovePoints) 
        {
           if(Vector3.Distance(centerPoint, mp.transform.position) <= moveRange) 
            {
                mp.gameObject.SetActive(true);

                //find the current point which already has a unit standing there
                //disactive characer's current standing moving points.
                foreach (CharacterMainController cc in GameManager.instance.allChars) 
                {
                   if(Vector3.Distance(cc.transform.position, mp.transform.position) < .5f) 
                    {
                        mp.gameObject.SetActive(false);
                    }
                }
            }
        }
    }

    //get all the moveable list
    public List<MovePoint> GetMovePointsInRange(float moveRange, Vector3 centerPoint) 
    {
        List<MovePoint> foundPoints = new List<MovePoint>();

        foreach (MovePoint mp in allMovePoints)
        {
            if (Vector3.Distance(centerPoint, mp.transform.position) <= moveRange)
            {
                bool shouldAdd = true;

                //find the current point which already has a unit standing there
                foreach (CharacterMainController cc in GameManager.instance.allChars)
                {
                    if (Vector3.Distance(cc.transform.position, mp.transform.position) < .5f)
                    {
                        shouldAdd = false;
                    }
                }

                if(shouldAdd == true) 
                {
                    foundPoints.Add(mp);
                }
            }
        }


        return foundPoints;
    }
}
