using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadoutDictionary : MonoBehaviour
{

    public static List<string> alphabet = new List<string> { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R","S","T","U","V","W","X","Y","Z"};
    
    public static List<string> primaryWeapons = new List<string> { "FullAutoAR","BurstAR","SingleShotAR" };
    public static List<string> secondaryWeapons = new List<string> { "SingleShotSidearm", "BurstSidearm", "Revolver" };
    public static List<string> perkOnes = new List<string> { "Armor", "BulletBoost", "Restock" };
    public static List<string> perkTwos = new List<string> { "SilentButDeadly", "AlwaysReady", "Recon" };
    public static List<string> lethals = new List<string> { "FragGrenade", "Mine", "StickyGrenade" };

    public static List<List<string>> loadoutSlots = new List<List<string>> { primaryWeapons, secondaryWeapons, perkOnes, perkTwos, lethals, lethals };
}
