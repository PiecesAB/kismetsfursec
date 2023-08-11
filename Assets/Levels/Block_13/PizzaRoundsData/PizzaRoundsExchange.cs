using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PizzaRoundsExchange : MonoBehaviour, IChoiceUIResponse
{
    private Dictionary<string, int> responseToId = new Dictionary<string, int>()
    {
        {"supreme", 601}, {"sausage", 600}
    };

    public GameObject ChoiceResponse(string message)
    {
        bool succ = InventoryItemsNew.DeleteItemById(301);
        if (!succ) { throw new System.Exception("This shouldn't be possible!"); }
        InventoryItemsNew.AddItem(responseToId[message], true);
        return null;
    }
}
