using UnityEngine;
using System.Collections.Generic;


[CreateAssetMenu(
    fileName = "CardDatabase",
    menuName = "Cheating Geese/Cards/Card Database")]

public class CardDatabase : ScriptableObject
{
    
    [SerializeField] private List<CardData> _cards = new(); // all avaliable normal cards

    public IReadOnlyList<CardData> Cards => _cards; //read only access for other scripts


    #if UNITY_EDITOR

    //only the editor can replace the databases card list
    public void EditorSetCards(List<CardData> cards)
    {
        _cards = cards;
    }

    #endif
}
