using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FomtAddMaterials : MonoBehaviour {

    public Material[] matsToAdd;


	// Use this for initialization
	void Start () {

        List<Material> mod = new List<Material>(GetComponent<Renderer>().materials);

        foreach (Material m in matsToAdd)
        {
            mod.Add(m);
        }

        GetComponent<Renderer>().materials = mod.ToArray();

    }
	
	// Update is called once per frame
	void Update () {
	
	}
}
