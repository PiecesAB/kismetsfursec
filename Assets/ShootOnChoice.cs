using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootOnChoice : MonoBehaviour, IChoiceUIResponse
{
    [System.Serializable]
    public struct ChoiceAndShooters
    {
        public string choice;
        public GameObject[] shooters;
    }

    public string currChoice;
    public GameObject[] currShooters;
    public ChoiceAndShooters[] shotList;
    public WaypointPerson stopWaypointOnNonemptyChoice;

    public GameObject ChoiceResponse(string message)
    {
        ChangeChoice(message);
        return null;
    }

    private void Start()
    {
        ChangeChoice("");
    }

    private void DestroyCurrShooters()
    {
        for (int i = 0; i < currShooters.Length; ++i)
        {
            Destroy(currShooters[i].gameObject);
        }
    }

    private void CloneToCurrShooters(ref GameObject[] makers)
    {
        currShooters = new GameObject[makers.Length];
        for (int i = 0; i < makers.Length; ++i)
        {
            currShooters[i] = Instantiate(makers[i], makers[i].transform.position, makers[i].transform.rotation, makers[i].transform.parent);
            currShooters[i].SetActive(true);
        }
    }

    private void ChangeChoice(string c)
    {
        if (c == "")
        {
            DestroyCurrShooters();
        }
        else
        {
            if (stopWaypointOnNonemptyChoice)
            {
                Destroy(stopWaypointOnNonemptyChoice);
            }
        }
        for (int i = 0; i < shotList.Length; ++i)
        {
            if (shotList[i].choice == c && c != currChoice)
            {
                DestroyCurrShooters();
                CloneToCurrShooters(ref shotList[i].shooters);
                break;
            }
        }
        currChoice = c;
    }
}
