using UnityEngine;
using System.Collections;

public class DemoDamage : MonoBehaviour 
{
    //A simple class to cause damage to the SimplePlayer instance. 
    //just for demonstrating purposes, not meant to be actually used in your own project

    public int DamageAmount = 10;
    public SimplePlayer Player;

    void OnGUI()
    {
        GUI.Label(new Rect(50, 80, 300, 20), "Press Space to take " + DamageAmount + " HP damage");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Player.Damage(DamageAmount);
        }
    }
}
