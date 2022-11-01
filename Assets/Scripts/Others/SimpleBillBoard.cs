using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleBillBoard : MonoBehaviour
{

    void LateUpdate()
    {
        transform.rotation = CameraController.instance.transform.rotation;
    }
}
