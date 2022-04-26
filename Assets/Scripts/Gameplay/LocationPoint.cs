using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LocationPoint : MonoBehaviour
{
    [SerializeField] string locationName;


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))  
        {
            var hudCanvas = other.transform.parent.GetChild(2).gameObject; //Gets HUD (Canvas) of whoever walks into the zone [MIGHT BE A BIT HEAVY FOR THE GAME]
            hudCanvas.GetComponent<HUDManager>().DefineLocationOnHUD(locationName); //Sets location name
        }
    }
    /*/
    private void OnTriggerExit(Collider other)
    {
        var hudCanvas = other.transform.parent.GetChild(2).gameObject; //Gets HUD (Canvas) of whoever walks into the zone [MIGHT BE A BIT HEAVY FOR THE GAME]
        hudCanvas.GetComponent<HUDManager>().DefineLocationOnHUD(null); //Sets location name
    }
    /*/
}
