using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class HUDManager : MonoBehaviour
{
    [SerializeField] Transform eventFeedPanel;
    [SerializeField] GameObject eventPanel;

    [SerializeField] Transform medalFeedPanel;
    [SerializeField] GameObject medalPanel;

    [SerializeField] TextMeshProUGUI currentLocation;
    public bool triggerEvent;

    [Header("Equipment Wheel")]
    [SerializeField] Transform equipmentWheelPanel;
    [SerializeField] Transform equipmentObjectsAnchor;
    public static bool equipmentWheelTriggered;
    public static int equipmentIndex = -1; //used for reference to the equipmentObjects list

    [Header("Crosshair")]
    public static bool crosshairEnabled = true;
    public static bool defaultCrosshairEnabled = true;
    [SerializeField] GameObject crosshairPanel;

    [Header("Morality Indicator")]
    [SerializeField] Transform moralityBar;
    [SerializeField] Transform moralityBarIndicator;

    // Update is called once per frame
    void Update()
    {
        if (triggerEvent)
        {
            triggerEvent = false;
            AddEventToEventFeed("TheKryton", "Melee", "Xaqqa");
            AddMedalToMedalFeed("Headshot", null);
        }
    }

    IEnumerator Start()
    {
        foreach (Transform child in equipmentWheelPanel)
        {
            if (child.GetComponent<Image>()) child.GetComponent<Image>().alphaHitTestMinimumThreshold = .5f;
        }
        yield return null;
    } //Set Clickable Area of buttons

    public void EquipmentWheelFlipFlop()
    {
        if(!equipmentWheelTriggered) StartCoroutine(nameof(_EquipmentWheelFlipFlop));
    }

    IEnumerator _EquipmentWheelFlipFlop()
    {
        equipmentWheelTriggered = true;
        if (equipmentWheelPanel.gameObject.activeInHierarchy)
        {
            equipmentWheelPanel.gameObject.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            equipmentWheelPanel.gameObject.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        DisableCrosshairFlipFlop();
        Debug.Log("EquipmentWheelFlipFlop");

        yield return new WaitForSeconds(.1f);
        equipmentWheelTriggered = false;
    }


    public void EquipmentButtonPressed()
    {
        if(equipmentIndex >= 0) equipmentObjectsAnchor.GetChild(equipmentIndex).gameObject.SetActive(false);

        equipmentIndex = EventSystem.current.currentSelectedGameObject.transform.GetSiblingIndex();
        PlayerMovement.equipmentBeingUsed = false;
        equipmentObjectsAnchor.GetChild(equipmentIndex).gameObject.SetActive(true);
    }


    public void DisableCrosshairFlipFlop()
    {
        if (crosshairEnabled)
        {
            crosshairPanel.SetActive(false);
            crosshairEnabled = false;
        }
        else
        {
            crosshairPanel.SetActive(true);
            crosshairEnabled = true;
        }
    }

    public void SwitchCrosshairFlipFlop()
    {
        if (defaultCrosshairEnabled)
        {
            crosshairPanel.transform.GetChild(0).gameObject.SetActive(false);
            crosshairPanel.transform.GetChild(1).gameObject.SetActive(true);
            defaultCrosshairEnabled = false;
        }
        else
        {
            crosshairPanel.transform.GetChild(0).gameObject.SetActive(true);
            crosshairPanel.transform.GetChild(1).gameObject.SetActive(false);
            defaultCrosshairEnabled = true;
        }
    }

    public void AddMedalToMedalFeed(string MedalName, Sprite MedalIcon)
    {
        var thisMedal = Instantiate(medalPanel, medalFeedPanel); //Spawns Medal as Child of Medal Panel
        thisMedal.transform.GetChild(0).GetComponent<Image>().sprite = MedalIcon;
        thisMedal.GetComponentInChildren<TextMeshProUGUI>().text = MedalName;

        if (medalFeedPanel.childCount > 3) Destroy(medalFeedPanel.GetChild(0).gameObject); //Remove the oldest medal if more than 3 are visible after triggering new one

        StartCoroutine(RemoveMedalFromMedalFeed(thisMedal)); //Removes Medal after 2s
    }

    IEnumerator RemoveMedalFromMedalFeed(GameObject MedalToRemove)
    {

        yield return new WaitForSeconds(2);
        if (MedalToRemove.activeInHierarchy) Destroy(MedalToRemove);
    }

    void AddEventToEventFeed(string CommitterName, string Method, string VictimName)
    {
        var thisEvent = Instantiate(eventPanel, eventFeedPanel);
        thisEvent.GetComponentInChildren<TextMeshProUGUI>().text = "<b>" + CommitterName + "</b> [" + Method + "] <b>" + VictimName + "</b>";

        if(eventFeedPanel.childCount > 5) Destroy(eventFeedPanel.GetChild(0).gameObject);
    }

    public void DefineLocationOnHUD(string locationName)
    {
        currentLocation.text = locationName;
    }

    public void MoveMoralityIndicator()
    {
        Vector2 temp_moralityIndicatorPosition = moralityBarIndicator.localPosition;
        if (PlayerMovement.moralityLevel >= -1f || PlayerMovement.moralityLevel <= 1f)
        {
            temp_moralityIndicatorPosition.Set(Mathf.Clamp(PlayerMovement.moralityLevel * 256f,-256f,256f), 0f);

            moralityBarIndicator.localPosition = temp_moralityIndicatorPosition;
        }
    }
}
