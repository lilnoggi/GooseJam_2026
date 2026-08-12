#if UNITY_EDITOR

using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEditor;
using Unity.VisualScripting;

//this class will only exist inside of the unity editor, it will create every combination of 4 suites x 13 ranks = 52 cards

public static class PrototypeCardGenerator 
{

    private const string CardFolder = "Assets/_Data/Cards"; 
    private const string DatabasePath = "Assets/_Data/CardDatabase.asset"; //the database asset that contains refrences to all of the generated cards


    [MenuItem("Cheating Geese/Generate Prototype Cards")]
    public static void GenerateCards()
    {

        if (!AssetDatabase.IsValidFolder(CardFolder)) //saftey check for if the folder has been created
        {
            Debug.LogError("Could not find Assets/_Data/Cards. Please create the _Data folder and Cards folder first. Or if moved PLZ MOVE BACK PLZ PLZ PLZ!!!");
            return;
        }

        List<CardData> generatedCards = new(); //this list will contain every generated card


        foreach (CardSuit suit in Enum.GetValues(typeof(CardSuit))) //loop through all 4 suites
        {
            foreach (CardRank rank in Enum.GetValues(typeof(CardRank))) //loop through all 13 ranks
            {

                string cardPath = $"{CardFolder}/Card_{suit}_{rank}.asset"; //Example Assets/_Data/Cards/Card_Blood_Ace.asset
                CardData card = AssetDatabase.LoadAssetAtPath<CardData>(cardPath); //check weather the card exists

                if (card == null) //if it doesn't exist create a new CardData asset
                {
                    card = ScriptableObject.CreateInstance<CardData>();

                    AssetDatabase.CreateAsset(card, cardPath);

                }
             
                card.EditorSetData (suit,rank);//give card correct rank and suit
                EditorUtility.SetDirty(card);//tell Unity that a asset has changed
                generatedCards.Add(card);//add this card to the database list

            }
        }

        CardDatabase database = AssetDatabase.LoadAssetAtPath<CardDatabase>(DatabasePath); //tries to find an existing CardDatabase asset

        if(database == null) //if one doesnt exist yet then create one
        {
            database = ScriptableObject.CreateInstance<CardDatabase>();

            AssetDatabase.CreateAsset(database, DatabasePath );
        }


        database.EditorSetCards(generatedCards);//give datavase refrences to all 52 pf the cards
        EditorUtility.SetDirty(database);
        AssetDatabase.SaveAssets(); //save all generated assets
        AssetDatabase.Refresh(); //refresh Unities project window
        Selection.activeObject = database;// selects the database when the generation is finished
        EditorGUIUtility.PingObject(database);

        Debug.Log($"Successfully generated {generatedCards.Count} cards");


    }
}


#endif