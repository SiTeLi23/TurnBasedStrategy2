using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static CameraController instance;

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

        moveTarget = transform.position;
    }

    public float moveSpeed, manualMoveSpeed = 5f;
    private Vector3 moveTarget;

    private Vector2 moveInput;

    private float targetRot;
    public float rotateSpeed;
    private int currentAngle; //rotation snapping angle unit

    public Transform theCam;

    public float fireCamViewAngle = 30f;
    private float targetCamViewAngle;
    private bool isFireView;

    void Start()
    {
        targetCamViewAngle = 45f;
    }

    
    void Update()
    {
        if(moveTarget != transform.position) 
        {
            transform.position = Vector3.MoveTowards(transform.position, moveTarget, moveSpeed * Time.deltaTime);
        }

        moveInput.x = Input.GetAxis("Horizontal");
        moveInput.y = Input.GetAxis("Vertical");
        moveInput.Normalize();

        //camera movement
        if(moveInput != Vector2.zero) 
        {
            transform.position += ((transform.forward * (moveInput.y * manualMoveSpeed)) + (transform.right * (moveInput.x * manualMoveSpeed))) * Time.deltaTime;

            moveTarget = transform.position;
        }

        //snap back to player
        if (Input.GetKeyDown(KeyCode.Tab)) 
        {
            SetMoveTarget(GameManager.instance.activePlayer.transform.position);
        }

        #region Camera Rotation
        //rotate camera
        if (Input.GetKeyDown(KeyCode.E)) 
        {
            currentAngle++;

            if(currentAngle >= 4) 
            {
                currentAngle = 0;
            }
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            currentAngle--;

            if (currentAngle < 0 )
            {
                currentAngle = 3;
            }
        }

        if (isFireView == false)
        {
            targetRot = (90f * currentAngle) + 45f; //45f is the start rotation of the camera, current angle  can not bigger than 4(4X90 = 360 degree)
        }
        
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0f, targetRot, 0f), rotateSpeed * Time.deltaTime);

        //raise up rotation toward the selected target if in shoot mode
        theCam.localRotation = Quaternion.Slerp(theCam.localRotation, Quaternion.Euler(targetCamViewAngle, 0f, 0f), rotateSpeed * Time.deltaTime);
            
        #endregion
    }

    public void SetMoveTarget(Vector3 newTarget) 
    {
        moveTarget = newTarget;

        //return to a normal view
        targetCamViewAngle = 45f;
        isFireView = false;
    }

    public void SetFireView() 
    {
        //whenever we start to shoot, move camera to the current selected unit
        moveTarget = GameManager.instance.activePlayer.transform.position;

        //move camera toward current selected target
        targetRot = GameManager.instance.activePlayer.transform.eulerAngles.y;

        targetCamViewAngle = fireCamViewAngle;

        isFireView = true;
    }
}
