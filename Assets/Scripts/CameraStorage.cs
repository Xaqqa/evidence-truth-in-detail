using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraStorage : MonoBehaviour
{
    public static List<Texture2D> cameraList = new List<Texture2D>();
    [SerializeField] GameObject developedPhotoPrefab;
    int oldestPhotoRef;
    [SerializeField] int numberOfPhotoSlots;


    [SerializeField] Transform player;
    public static List<string> analysedEvidence = new List<string>();

    public void DevelopPhotos()
    {
        int developedPhotos = 0;
        List<Transform> sparePhotoSlots = new List<Transform>(); //List of all free photo slots when photo developing
        List<Texture2D> photosDeveloped = new List<Texture2D>(); //Photos to remove once developed
        foreach (Transform child in this.transform) if (child.childCount == 0) sparePhotoSlots.Add(child); //Adds free slots to the list above

        foreach (Texture2D photo in cameraList)
        {
            if (sparePhotoSlots.Count != 0)
            {
                var developedPhoto = Instantiate(developedPhotoPrefab, sparePhotoSlots[0]); //Instantiates new developed photo prefab on the correct transform (which are children of this object)
                sparePhotoSlots.RemoveAt(0); //Removes free slot from List
                developedPhoto.GetComponent<Renderer>().material.mainTexture = cameraList[developedPhotos]; //Sets photo as texture of the prefab
                photosDeveloped.Add(cameraList[developedPhotos]); //Adds photo to list of photos to remove from cameraList
                developedPhotos++; //Updates reference for how many photos have been developed
            }
            else
            {
                Debug.Log("No free slots for new photos, oldest photos have been overwritten.");
                Destroy(transform.GetChild(oldestPhotoRef).GetChild(0).gameObject); //Destroys Oldest Photo
                if(oldestPhotoRef == numberOfPhotoSlots - 1) oldestPhotoRef = 0; //Resets Oldest Photo Ref
                else oldestPhotoRef++; //Updates Oldest Photo Ref
            }
        }
        foreach (string collectedEvidence in player.GetComponent<PlayerMovement>().collectedEvidence) analysedEvidence.Add(collectedEvidence); //Allows collected evidence to be used within investigations

        foreach (Texture2D photo in photosDeveloped) cameraList.Remove(photo);

        Debug.Log("Developed Photos: " + developedPhotos);
    }
}
