using System;
using UnityEngine;

/// <summary>
/// A specific sensor used to trigger the "Fade In" cinematic effect.
/// Wraps the player collision check for this specific layer.
/// </summary>
public class FadeInSensor : MonoBehaviour
{
    [SerializeField] private LayerMask _fadeInSensor;

    



    public bool HasBeenColidedWithFadeInSensor()
    {
        Player player = Player.Instance;

        bool colided = player.isPlayerColidedWithObstacle(_fadeInSensor);

        if (colided)
        {
            Debug.Log("Player collided with FadeInSensor");
            Debug.Log("FadeInSensor Colided: " + colided);
        }
        





        //Debug.Log("Sensor Colided: " + colided);

        return colided;



    }
}