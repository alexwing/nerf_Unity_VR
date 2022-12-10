
using UnityEngine;

public class KameHameHa : MonoBehaviour
{

    [SerializeField] private int FrameRate = 1;
    public float Distance = 20;
    public float Size = 1;
    public float Velocity = 1;

    private int currentTime = 0;
    [Tooltip("life time of kamehameha")]
    public int lifeTime = 10;

    private int bounceCount = 0;
    [Tooltip("number of bounce")]
    public int bounceLimit = 3;
    void Start()
    {
        InvokeRepeating("CheckDistance", 0, 1 / FrameRate);
        InvokeRepeating("CheckLifeTime", 0, 1);
    }

    private void CheckDistance()
    {
        float cameraDistance = Vector3.Distance(SceneConfig.MainCamera.position, transform.position);
        if (cameraDistance > Distance)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        bounceCount++;
        if (bounceCount > bounceLimit)
        {
            Destroy(gameObject);
        }
    }


    private void CheckLifeTime()
    {
        currentTime++;
        if (currentTime > lifeTime)
        {
            Destroy(gameObject);
        }
    }

    void OnDisable()
    {
        CancelInvoke("CheckDistance");
        CancelInvoke("CheckLifeTime");
    }
    private void OnDestroy()
    {
        CancelInvoke("CheckDistance");
        CancelInvoke("CheckLifeTime");
    }
}




