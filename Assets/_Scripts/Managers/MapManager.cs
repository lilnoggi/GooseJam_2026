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
        AudioManager.Instance.PlayBGM(BGMType.MainTheme);
        
        // As soon as the map scene loads, update the visuals based on the player's progress
        RefreshMapState();
    }

    private void RefreshMapState()
    {
        // Get the variable tracking the players progress
        int currentIndex = _sessionData.CurrentLevelIndex;

        // Loop through every single node on the map
        foreach (MapNode node in _mapNodes)
        {
            bool isUnlocked = node.NodeIndex <= currentIndex;

            bool isCurrentLevel = node.NodeIndex == currentIndex;

            // Tell the MapNode to update its sprite and button
            node.SetState(isUnlocked, isCurrentLevel);
        }

        // TODO: Call path-drawing coroutine here later
    }
}
