using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WandProjectile : MonoBehaviour
{
    public GameObject player;
    public GameObject canvas;

    [SerializeField] Material deadNPC;
    [SerializeField] Material witnessNPC;
    [SerializeField] Sprite perpetratorKilled;
    private void OnCollisionEnter(Collision collision)
    {
        GameObject temp_hitObject = collision.gameObject;
        if(temp_hitObject.tag == "NPC")
        {
            if (temp_hitObject.GetComponent<DialogueManager>().npc.guilty) player.GetComponent<PlayerMovement>().UpdateMorality(0.1f);
            else player.GetComponent<PlayerMovement>().UpdateMorality(-0.1f);

            temp_hitObject.GetComponent<MeshRenderer>().material = deadNPC;
            temp_hitObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
            temp_hitObject.GetComponent<DialogueManager>().npc.dead = true;
            temp_hitObject.GetComponent<Rigidbody>().AddExplosionForce(1000f, transform.position, 100f);
            canvas.GetComponent<HUDManager>().AddMedalToMedalFeed("Perpetrator Incapacitated", perpetratorKilled);
        }
        Debug.Log("Completed Projectile Collision");
        Destroy(this.gameObject);
    }
}
