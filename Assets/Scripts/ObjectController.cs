using UnityEngine;

public class ObjectController : MonoBehaviour
{
    [Header("Hands")]
    [SerializeField] private Transform _leftHand;
    [SerializeField] private Transform _rightHand;

    [SerializeField] private Transform _RotationGestoureAnchor;
    [SerializeField] private Transform _MoveGestoureAnchor;

    [Header("Player")]
    public float rotationSpeed = 5f;       // Rotation speed.


    [Header("Movement")]
    [SerializeField]
    [Tooltip("The speed of the player.")]
    public float mainSpeed = 70f;
    [SerializeField]
    [Tooltip("The maximum speed of the player.")]
    public float maxSpeed = 10f;
    [SerializeField]
    [Tooltip("The force magnitude of the player.")]
    public float forceMagnitude = 2f;

    [Header("Zoom")]

    [SerializeField]
    [Tooltip("The minimum distance between the hands for zooming.")]
    public float minZoomDistance = 0.1f;

    [SerializeField]
    [Tooltip("The maximum distance between the hands for zooming.")]
    public float maxZoomDistance = 3f;


    [Header("Rigidbody")]
    [SerializeField]
    [Tooltip("The rigidbody to control.")]
    public Rigidbody rb;

    public Quaternion currentRotation;

    private float _gestureStartTime = 0f;
    private Vector3 _gestureStartPosition = Vector3.zero;

    public float maxRotationDistance = 0.1f;
    private Vector3 _prevRightHandPos;

    public void Start()
    {
        currentRotation = rb.transform.rotation;
    }
    private bool handStateRight;
    public bool HandStateRight
    {
        get { return handStateRight; }
        set { handStateRight = value; }
    }

    private bool handStateRightZoom;
    public bool HandStateRightZoom
    {
        get { return handStateRightZoom; }
        set { handStateRightZoom = value; }
    }

    private bool handStateLeft;
    public bool HandStateLeft
    {
        get { return handStateLeft; }
        set { handStateLeft = value; }
    }
    private void Update()
    {
        if (HandStateRight) ObjectMovement();
        if (HandStateLeft) GestureRotation();
        if (HandStateRightZoom) ObjectZoom();
    }

    private void ObjectZoom()
    {
        // TODO: escalar el rigibody segun lo cerca que este _MoveGestoureAnchor de _rightHand solo en profundidad
        // tiene que ser proporcional a el _minZoomDistance y al  _maxZoomDistance

// Calculate the direction between the right hand and the movement anchor
Vector3 direction = _rightHand.position - _MoveGestoureAnchor.position;

// Use the "z" component of the direction vector as the distance in the frontal direction
float distance = direction.z;

// Calculate the scale factor based on the distance between the hands
float scaleFactor = Mathf.InverseLerp(minZoomDistance, maxZoomDistance, distance);

Debug.Log("scaleFactor: " + scaleFactor);

// Scale the rigidbody
rb.transform.localScale = new Vector3(scaleFactor, scaleFactor, scaleFactor);



    }

    private void GestureRotation()
    {

        // Calculate the distance between the left hand and the rotation anchor
        float distance = Vector3.Distance(_leftHand.position, _RotationGestoureAnchor.position);

        // Scale the force based on the Zoom strength (distance between the hands)
        float forceScale = Mathf.Clamp01(distance / maxRotationDistance);

        // Calculate the direction of the rotation force using the cross product
        Vector3 direction = Vector3.Cross(_leftHand.position, _RotationGestoureAnchor.position);

        // Apply the rotation force to the object
        Vector3 torque = direction * rotationSpeed * forceScale * forceMagnitude;
        rb.AddTorque(torque);

    }

    private void ObjectMovement()
    {
        // Calculate the direction between the right hand and the movement anchor
        Vector3 direction = _rightHand.position - _MoveGestoureAnchor.position;

        // Calculate the force to apply to the object
        Vector3 force = direction * mainSpeed * forceMagnitude;

        // Clamp the force magnitude to the maximum speed
        force = Vector3.ClampMagnitude(force, maxSpeed);

        // Apply the force to the object
        rb.AddForce(force);
    }
}