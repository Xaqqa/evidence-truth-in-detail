using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueManager : MonoBehaviour
{

    public NPC npc;
    [SerializeField] GameObject player;
    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] TextMeshProUGUI dialogueText;

    [SerializeField] TextMeshProUGUI playerResponseHappy;
    [SerializeField] TextMeshProUGUI playerResponseNeutral;
    [SerializeField] TextMeshProUGUI playerResponseAggressive;
    [SerializeField] TextMeshProUGUI playerResponseAnalytical;

    [SerializeField] GameObject dialogueUI;

    public void StartConversation()
    {
        player.GetComponent<PlayerMovement>().talkingNPC = npc;
        dialogueUI.SetActive(true);
        nameText.text = npc.name;
        dialogueText.text = npc.dialogue[0];
        playerResponseHappy.text = npc.playerDialogue[0];
        playerResponseNeutral.text = npc.playerDialogue[1];
        playerResponseAggressive.text = npc.playerDialogue[2];
        

        if (CameraStorage.analysedEvidence.Contains(npc.evidenceIdentifier))
        {
            playerResponseAnalytical.GetComponentInParent<Button>().interactable = true;
            playerResponseAnalytical.text = npc.playerDialogue[3];
        }
        else
        {
            playerResponseAnalytical.text = "[Find evidence to unlock this dialogue option]";
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void DisplayResponse(int responseValue)
    {
        dialogueText.text = npc.dialogue[responseValue + 1];
        playerResponseHappy.transform.parent.gameObject.SetActive(false);
        playerResponseNeutral.transform.parent.gameObject.SetActive(false);
        playerResponseAggressive.transform.parent.gameObject.SetActive(false);
        playerResponseAnalytical.transform.parent.gameObject.SetActive(false);
    }

    public void StopConversation()
    {
        dialogueUI.SetActive(false);
        playerResponseHappy.transform.parent.gameObject.SetActive(true);
        playerResponseNeutral.transform.parent.gameObject.SetActive(true);
        playerResponseAggressive.transform.parent.gameObject.SetActive(true);
        playerResponseAnalytical.transform.parent.gameObject.SetActive(true);
        playerResponseAnalytical.transform.parent.GetComponent<Button>().interactable = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
