using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages procedural level generation by spawning pre-designed level parts as the player progresses.
/// Handles object pooling/destruction to maintain performance.
/// </summary>
public class LevelGenerator : MonoBehaviour
{
    private const float PLAYER_DISTANCE_SPAWN_LEVEL_PART = 30f;
    //private const int  LEVEL_PART_1_LIMIT_IN_A_ROW = 2;


    [SerializeField] private Transform levelPartStart;

    [SerializeField] private List<Transform> levelPartsList;
    


    [Header("Spawn Control")]
    [SerializeField] private int maxLevelPartsOnScreen = 40;
    
    // Queue to track active level parts for efficient removal
    private Queue<Transform> lastLevelPartGenerated = new Queue<Transform>();


    private int lastSpawnedIndex = -1;
    //private int levelPart1CountInARow = 0;


    [SerializeField] private Player player;

    private Vector3 lastEndPostion;

    

    private void Awake()
    {
        if (levelPartStart == null)
        {
            Debug.LogError("levelPartStart is NOT assigned!");
        }
        
        // Find the "connection point" of the starting platform
        lastEndPostion = levelPartStart.Find("EndPostion").position;

        
        int startingLevelParts = 1;

        // Spawn initial parts so the world isn't empty
        for (int i = 0; i < startingLevelParts; i++)
        {
            SpawnLevelPart();
        }




    }


    private void Update()
    {
        Vector3 Distance = player.transform.position - lastEndPostion;
        //Debug.Log("Distance to end position: " + Distance.magnitude);


        // Check if player is close enough to the end to trigger new generation
        if (isPlayerNearEndOfLevelPart())
        {
            SpawnLevelPart();
            Debug.Log("Spawned new level part. New lastEndPosition: " + lastEndPostion);
        }
        
    }


    /// <summary>
    /// Selects a random level part and spawns it at the correct position.
    /// Manages the queue of active parts and destroys old ones.
    /// </summary>
    private void SpawnLevelPart()
    {
        int randomIndex = Random.Range(0, levelPartsList.Count);
        
        

        //if(randomIndex == 0 )
        //{
        //    levelPart1CountInARow++;
        //    if(levelPart1CountInARow > LEVEL_PART_1_LIMIT_IN_A_ROW)
        //    {
        //        // pick another part instead
        //        randomIndex = Random.Range(1, levelPartsList.Count);
        //        levelPart1CountInARow = 0;

                
        //    }
        //}
        
        //if (randomIndex == 3 && lastSpawnedIndex != 2)
        //{
        //    // pick another part instead
        //    randomIndex = Random.Range(0, levelPartsList.Count - 1);
        //}

        
        //if (randomIndex == 0 && lastSpawnedIndex == 2)
        //    {
        //    // pick another part instead
        //    randomIndex = Random.Range(1, levelPartsList.Count);
        //}
        
        Transform levelPartTransform = SpawnLevelPart( levelPartsList[randomIndex]  ,  lastEndPostion);
        

        // Update the snap point for the next piece
        lastEndPostion = levelPartTransform.Find("EndPostion").position;
        lastLevelPartGenerated.Enqueue(levelPartTransform);
        lastSpawnedIndex = randomIndex;

        // Cleanup old parts
        if (lastLevelPartGenerated.Count > maxLevelPartsOnScreen)
        {
            Debug.Log(lastLevelPartGenerated.Count + " level parts on screen, exceeding limit of " + maxLevelPartsOnScreen + ". Destroying oldest part.");
            Transform oldParts = lastLevelPartGenerated.Dequeue();
            Destroy(oldParts.gameObject);
            Debug.Log("Destroyed old level part to manage memory.");
        }
    }

    /// <summary>
    /// Instantiates a specific level part prefab at a specific world position.
    /// </summary>
    /// <param name="levelPart">The prefab to spawn.</param>
    /// <param name="spawnPosition">Where to spawn it.</param>
    /// <returns>The transform of the spawned object.</returns>
    private Transform SpawnLevelPart(Transform levelPart , Vector3 spawnPosition)
    {




        Transform levelPartTransform = Instantiate(levelPart, spawnPosition, Quaternion.identity);

        if (levelPartTransform.Find("EndPostion") == null)
        {
            Debug.LogError("Spawned level part is missing EndPosition!");
        }

        if (levelPartStart == null)
        {
            Debug.LogError("levelPartStart is NOT assigned!");
        }
        return levelPartTransform;
    }


    /// <summary>
    /// Checks distance between player and the end of the current known world.
    /// </summary>
    private bool isPlayerNearEndOfLevelPart()
    {
        return Vector3.Distance(player.transform.position, lastEndPostion) < PLAYER_DISTANCE_SPAWN_LEVEL_PART;
    }


    private void DestoryLevelPart(int var)
    {
        Destroy(levelPartsList[var].gameObject);
    }
}