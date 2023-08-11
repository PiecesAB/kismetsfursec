using UnityEngine;
using System.Collections;

public class temp : MonoBehaviour {

	// Use this for initialization
	void Start () {
        string hi = "";
        string[] lol = new string[20]
        {
            "Croc","Wind","House","Lizard","Snake",
            "Death", "Deer","Rabbit","Water","Dog",
            "Monkey","Grass","Reed","Jaguar","Eagle",
            "Vult","Quake","Flint","Rain","Flower"
        };

        string[] succ = new string[12]
        {
            "Jan","Feb","Mar","Apr","May","Jun",
            "Jul","Aug","Sep","Oct","Nov","Dec"
        };

        int[] lopp = new int[12]
        {
            31,
            28,
            31,
            30,
            31,
            30,
            31,
            31,
            30,
            31,
            30,
            40 //lol
        };
        int junk = 0;
        int throp = 0;
        
        for (int i = 0; i < 365; i++)
        {
            int j = i % 20;
            int k = i % 13;
            throp++;
            if (throp > lopp[junk])
            {
                junk++;
                throp = 1;
            }

            hi = hi + succ[junk] + " "+ (throp) + " = " + (k+1) + " " + lol[j] + "\n";
        }
        print(hi);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
