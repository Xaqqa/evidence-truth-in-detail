using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EvidenceMarker : MonoBehaviour
{

    [SerializeField] string identifier; //Links evidence to the case
    [SerializeField] Transform player;

    public void TriggerEvidence()
    {
        player.GetComponent<PlayerMovement>().collectedEvidence.Add(identifier);
        Destroy(this.gameObject);
    }
}
