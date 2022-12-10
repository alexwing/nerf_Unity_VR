using System;
using UnityEngine;

public class InputHandlerHands : MonoBehaviour
{


	[Tooltip("This prefab will be instantiated when the aim visual is awakened, and will be set active when the user is aiming, and deactivated when they are done aiming.")]
	public LineRenderer LaserPrefab;


	private readonly Action _enterAimStateAction;
	private readonly Action _exitAimStateAction;
	private readonly Action<LocomotionTeleport.AimData> _updateAimDataAction;

    [HideInInspector]
	public LineRenderer _lineRenderer;
	private Vector3[] _linePoints;

    public float horizontalOffset = 0.0f;

    public float aimDistance = 200.0f;

    public Transform RightHand;
    private Ray ray = new Ray();

    private BoxCollider boxCollider;
    public static InputHandlerHands instance;

    public static bool aim = true;
	private void Awake()
	{
        instance = this;
		LaserPrefab.gameObject.SetActive(false);
		_lineRenderer = Instantiate(LaserPrefab);
        _lineRenderer.gameObject.name = "Laser";
         boxCollider = _lineRenderer.gameObject.AddComponent<BoxCollider>();
         boxCollider.isTrigger = true;
        

	}
    public void aimClosed()
    {
        aim = false;
        _lineRenderer.gameObject.SetActive(false);
    }
    public void aimOpened()
    {
        aim = true;
        _lineRenderer.gameObject.SetActive(true);
    }

    public void Update(){
        if (!SceneConfig.gameIsPaused)
        {
            aimClosed();
            return;
        }
        if (aim)
        {
            GetAimData(out ray);

            LocomotionTeleport.AimData aimData = new LocomotionTeleport.AimData();
            aimData.Reset();
            aimData.Points.Add(ray.origin);
            aimData.Points.Add(ray.origin + ray.direction * aimDistance);
            aimData.TargetValid = true;
            aimData.Destination = ray.origin + ray.direction * aimDistance;
            aimData.Radius = 0.1f;
            UpdateAimData(aimData);
                
        }
    }

    private void UpdateAimData(LocomotionTeleport.AimData obj)
	{
		_lineRenderer.sharedMaterial.color = obj.TargetValid ? Color.green : Color.red;

		var points = obj.Points;
        _lineRenderer.positionCount = points.Count;

		for (int i = 0; i < points.Count; i++)
		{

			_lineRenderer.SetPosition(i, points[i]);
		}
        Vector3 firstPositions = points[0];
        Vector3 lastPositions = points[points.Count - 1];
        //create box collider
        
      //  boxCollider.center = (firstPositions + lastPositions) / 2;
        boxCollider.size = new Vector3(0.1f,  0.1f, Vector3.Distance(firstPositions, lastPositions));
        boxCollider.transform.position = (firstPositions + lastPositions) / 2;
        boxCollider.transform.rotation = Quaternion.LookRotation(lastPositions - firstPositions);
    
      
	}

    public  void GetAimData(out Ray aimRay)
    {
        Transform t = RightHand;

        Vector3 offset = new Vector3(horizontalOffset, 0, 0);
        Vector3 pos = t.position + offset;
        aimRay = new Ray(pos, t.right);
        
    }
}

