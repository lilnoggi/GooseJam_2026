using UnityEngine;
using System.Collections.Generic;

public class MapManager : MonoBehaviour
{
    [Header("Run Data")]
    [SerializeField] private SessionData _sessionData;

    [Header("Map Nodes")]
    [SerializeField] private List<MapNode> _mapNodes;

    private void Start()
    {
        // As soon as the map scene loads, update the visuals based on the player's progress
        RefreshMapState();
    }

    private void RefreshMapState()
    {
        // Get the variable tracking the players progress
        int currentIndex = _sessionData.CurrentLevelIndex;

        // Loop through every single node on the map
        for (int i = 0; i < _mapNodes.Count; i++)
        {
            // A node is UNLOCKED if its index is less than or equal to current progress
            bool isUnlocked = i <= currentIndex;

            // A node is the CURRENT LEVEL if its index exactly matches progess
            bool isCurrentLevel = i == currentIndex;

            // Tell the MapNode to update its sprite and button
            _mapNodes[i].SetState(isUnlocked, isCurrentLevel);
        }

        // TODO: Call path-drawing coroutine here later
    }
}
