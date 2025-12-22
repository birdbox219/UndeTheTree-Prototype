//using System.Collections;
//using Unity.VisualScripting;
//using UnityEngine;
//using UnityEngine.UIElements;

//public class Sensor : MonoBehaviour
//{
//    [SerializeField] private LayerMask sensorLayer;
//    [Header("Fire Speed Scaling")]
//    [SerializeField] private float baseFireSpeed = 2f;

//    [SerializeField] private float minMultiplier = 0.7f;
//    [SerializeField] private float maxMultiplier = 3.5f;

//    [SerializeField] private float heightRamp = 80f;   // units
//    [SerializeField] private float timeRamp = 45f;     // seconds

//    [SerializeField] private AnimationCurve speedCurve;


//    private float startTime;
//    private float startY;

//    private Coroutine reviveCoroutine;
//     private Transform SensorPostion;

//    [SerializeField] private GameObject fadeInSensor;

//    private void Awake()
//    {



//    }

//    private void Start()
//    {
//        startTime = Time.time;
//        startY = transform.position.y;
//    }




//    //We dont use this method but we keep it for future reference
//    private void OnCollisionEnter2D(Collision2D collision)
//    {



//        //if (collision.gameObject.TryGetComponent(out Player player))
//        //{
//        //    Debug.LogError("Sensor hit Player");
//        //}

//        //if (collision.gameObject.TryGetComponent(out Ground obstacle))
//        //{
//        //    Debug.LogError("Sensor hit Obstacle");
//        //}
//    }




//    private void Update()
//    {
//        float multiplier = CalculateFireMultiplier();
//        MoveUp(baseFireSpeed * multiplier);
//    }
//    public bool HasBeenColidedWith()
//    {
//        Player player = Player.Instance;

//        bool colided = player.isPlayerColidedWithObstacle(sensorLayer);

//        if (colided)

//            HandleFireHit();

//        //Debug.Log("Sensor Colided: " + colided);

//        return colided;



//    }


//    public bool HasBeenColidedWithFadeInSensor()
//    {
//        if (fadeInSensor == null)
//        {   Debug.LogError("FadeInSensor component not found!");
//            return false;
//        }

//        return fadeInSensor.GameObject().GetComponent<FadeInSensor>().HasBeenColidedWithFadeInSensor();
//    }



//    private void MoveUp(float speed)
//    {
//        transform.Translate(Vector3.up * speed * Time.deltaTime);
//    }


//    private float CalculateFireMultiplier()
//    {
//        float elapsedTime = Time.time - startTime;
//        float heightDelta = Player.Instance.GetPlayerPositionY() - startY;

//        float timeT = Mathf.Clamp01(elapsedTime / timeRamp);
//        float heightT = Mathf.Clamp01(heightDelta / heightRamp);

//        // Combine both pressures
//        float combinedT = Mathf.Max(timeT, heightT);

//        // Curve smoothing
//        float curvedT = speedCurve.Evaluate(combinedT);

//        return Mathf.Lerp(minMultiplier, maxMultiplier, curvedT);
//    }


//    private void HandleFireHit()
//    {
//        if (Player.Instance.IsSaleemActive)
//        {
//            Player.Instance.SetSaleemAliveStatus(false);
//            //Player.Instance.SetSalmaAliveStatus(true);
//            //Debug.Log("Saleem Died");
//        }
//        else
//        {
//            Player.Instance.SetSalmaAliveStatus(false);
//            //Player.Instance.SetSaleemAliveStatus(true);

//            //Debug.Log("Salma Died");
//        }


//        if (reviveCoroutine != null)
//            StopCoroutine(reviveCoroutine);

//        reviveCoroutine = StartCoroutine(ReviveAfterSwitchLock());
//    }


//    private IEnumerator ReviveAfterSwitchLock()
//    {
//        // Wait for SAME duration as switch lock
//        yield return new WaitForSeconds(Player.Instance.GetSwitchLockDuration());

//        // Revive BOTH safely
//        Player.Instance.SetSaleemAliveStatus(true);
//        Player.Instance.SetSalmaAliveStatus(true);

//        Debug.Log("Revive window ended – characters restored");
//    }

//    public float GetFireSpeed()
//    {
//        return baseFireSpeed;
//    }


//    public float GetSensorPostion()
//    {
//        return this.transform.position.y;
//    }



//}


using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Controls the rising hazard (fire/lava) that chases the player.
/// Calculates dynamic speed based on player progress and time to create pressure.
/// Manages collision detection with the player and triggers game-over/respawn logic.
/// </summary>
public class Sensor : MonoBehaviour
{
    [SerializeField] private LayerMask sensorLayer;

    [Header("Fire Speed Scaling")]
    [SerializeField] private float baseFireSpeed = 2f;

    [SerializeField] private float minMultiplier = 0.8f;
    [SerializeField] private float maxMultiplier = 3.0f;

    [SerializeField] private float heightRamp = 90f;   // slower height pressure
    [SerializeField] private float timeRamp = 40f;     // slower time pressure

    [SerializeField] private AnimationCurve speedCurve;

    [Header("Safety")]
    [SerializeField] private float maxMultiplierIncreasePerSecond = 0.12f;

    [SerializeField] private GameObject fadeInSensor;

    

    private float startTime;
    private float startY;
    private float currentMultiplier;

    private Coroutine reviveCoroutine;

    private void Start()
    {
        startTime = Time.time;
        startY = transform.position.y;
        currentMultiplier = minMultiplier;
    }

    private void Update()
    {
        float targetMultiplier = CalculateFireMultiplier();

        // Smooth acceleration (NO spikes)
        currentMultiplier = Mathf.MoveTowards(
            currentMultiplier,
            targetMultiplier,
            maxMultiplierIncreasePerSecond * Time.deltaTime
        );

        MoveUp(baseFireSpeed * currentMultiplier);
    }

    /// <summary>
    /// Moves the sensor upwards at the calculated speed.
    /// </summary>
    private void MoveUp(float speed)
    {
        transform.Translate(Vector3.up * speed * Time.deltaTime);
    }

    /// <summary>
    /// Calculates the speed multiplier based on how high the player is and how much time has passed.
    /// Uses an exponential curve to ramp up difficulty smoothly but aggressively if the player stalls.
    /// </summary>
    private float CalculateFireMultiplier()
    {
        float elapsedTime = Time.time - startTime;
        float heightDelta = Player.Instance.GetPlayerPositionY() - startY;

        float timeT = Mathf.Clamp01(elapsedTime / timeRamp);
        float heightT = Mathf.Clamp01(heightDelta / heightRamp);

        // Use the stronger pressure (time or height)
        float combinedT = Mathf.Max(timeT, heightT);

        // COMPRESSED exponential (key fix)
        float compressedT = 1f - Mathf.Exp(-combinedT * 1.6f);

        // Curve + hard clamp
        float curvedT = Mathf.Clamp01(speedCurve.Evaluate(compressedT));

        return Mathf.Lerp(minMultiplier, maxMultiplier, curvedT);
    }

    /// <summary>
    /// Checks if the sensor has collided with the player via the Player's own collision logic.
    /// Triggers death logic if true.
    /// </summary>
    public bool HasBeenColidedWith()
    {
        bool collided = Player.Instance.isPlayerColidedWithObstacle(sensorLayer);

        if (collided)
            HandleFireHit();

        return collided;
    }

    /// <summary>
    /// Checks if the fade-in sensor child object has been triggered.
    /// </summary>
    public bool HasBeenColidedWithFadeInSensor()
    {
        if (!fadeInSensor)
        {
            Debug.LogError("FadeInSensor not assigned!");
            return false;
        }

        return fadeInSensor
            .GetComponent<FadeInSensor>()
            .HasBeenColidedWithFadeInSensor();
    }

    /// <summary>
    /// Handles the event when the fire catches the player.
    /// Marks the active character as "dead" and starts the revival timer.
    /// </summary>
    private void HandleFireHit()
    {
        if (Player.Instance.IsSaleemActive)
            Player.Instance.SetSaleemAliveStatus(false);
        else
            Player.Instance.SetSalmaAliveStatus(false);

        if (reviveCoroutine != null)
            StopCoroutine(reviveCoroutine);

        reviveCoroutine = StartCoroutine(ReviveAfterSwitchLock());
    }

    /// <summary>
    /// Restores character life status after the penalty duration.
    /// </summary>
    private IEnumerator ReviveAfterSwitchLock()
    {
        yield return new WaitForSeconds(Player.Instance.GetSwitchLockDuration());

        Player.Instance.SetSaleemAliveStatus(true);
        Player.Instance.SetSalmaAliveStatus(true);
    }

    public float GetFireSpeed()
    {
        return baseFireSpeed * currentMultiplier;
    }

    public float GetSensorPostion()
    {
        return transform.position.y;
    }


    

}