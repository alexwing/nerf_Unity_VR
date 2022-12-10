using System.Collections;
using UnityEngine;

public class Target : MonoBehaviour
{
    [SerializeField] private MeshRenderer _meshRenderer;
    // [SerializeField] private Material _original;
    //  [SerializeField] private Material _hit;
    [Tooltip("Detonation prefab")]
    public GameObject currentDetonator;
    [Tooltip("explosion effect Time")]
    [Range(0, 60)]
    public float explosionLife = 10;
    [Tooltip("Return to anchor time")]
    [Range(0, 60)]
    public int _restoreTime = 15;
    public float detailLevel = 1.0f;
    private bool newHit = false;

    private float currentTime = 0;
    [Tooltip("Ramdom explosion particles system")]
    [Range(0f, 1f)]
    public float ramdomExplosion = 0.22f;

    [SerializeField] private int FrameRate = 1;

    [Header("Sound Destrucion")]
    public AudioClip clip;
    [Tooltip("Width of terrain destruction")]
    [Range(0, 1500)]
    public int DistanceSoundLimit = 500;

    public float delayDestruction = 0.25f;

    private CubeBeatWar.EnemyController enemyController;

    void Start()
    {
        InvokeRepeating("CheckDistance", 0, 1 / FrameRate);
        enemyController = GetComponent<CubeBeatWar.EnemyController>();
    }
    private void CheckDistance()
    {
        if (newHit)
        {
            currentTime += 1;

            if (currentTime > _restoreTime)
            {
                newHit = false;
                Debug.Log("currentTime: " + currentTime + ">" + _restoreTime);
                //    _meshRenderer.material = _original;
                //gameObject.GetComponent<TargetSnap>().interactable = false;

            }
        }

    }
    private void OnTriggerEnter(Collider other)
    {
        currentTime = 0;
        //gameObject.GetComponent<TargetSnap>().interactable = true;
        newHit = true;
        //      _meshRenderer.material = _hit;

        if (other.gameObject.name == "kamehameha")
        {
            //delay detonation
            StartCoroutine(detonation(other));

            Magic._hitCount++;


        }
    }



    IEnumerator detonation(Collider collision)
    {
                //float ExplosionVelocity = collision.GetComponent<KameHameHa>().Velocity;
        // Debug.Log("Size" + collision.GetComponent<KameHameHa>().Size + " -- " + explosionSize + " -- " + ExplosionVelocity);
        Debug.Log("velocity: " + collision.GetComponent<KameHameHa>().Velocity);
        float normalizedValue = Mathf.InverseLerp(0, 1, (int)collision.GetComponent<KameHameHa>().Velocity);
       // float normalizedValue = Mathf.InverseLerp(0, 100, (int)collision.GetComponent<KameHameHa>().Size);
        int explosionSize = (int)Mathf.Lerp(1, 10, normalizedValue);
        Destroy(Instantiate(currentDetonator, collision.transform.position, Quaternion.identity), explosionLife);

        yield return new WaitForSeconds(delayDestruction);;

        for (int i = 0; i < explosionSize; i++)
        {
            Destroy(Instantiate(currentDetonator, Utils.RandomNearPosition(this.transform, ramdomExplosion, 0f, ramdomExplosion).position, Quaternion.identity), explosionLife);
        }
        Utils.PlaySound(clip, this.transform, Camera.main.transform, DistanceSoundLimit);
        enemyController.Hit();
        Destroy(this.gameObject);


    }



}
