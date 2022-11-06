using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SFXManager : MonoBehaviour
{
    public static SFXManager instance;

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

    public AudioSource deathHuman, deathRobot, impact, meleeHit, takeDamage, UICancel, UISelect;

    public AudioSource[] shootSound;

    public void PlayShoot() 
    {
        shootSound[Random.Range(0, shootSound.Length)].Play();
    }
}
