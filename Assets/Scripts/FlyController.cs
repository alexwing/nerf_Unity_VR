using UnityEngine;

public class FlyController : MonoBehaviour
{

    [Header("Hands")]
    [SerializeField] private Transform _leftHand;
    [SerializeField] private Transform _rightHand;

    [SerializeField] private Transform _RotationGestoureAnchor;
    [SerializeField] private Transform _MoveGestoureAnchor;


    [Header("Score")]
    [SerializeField] private TMPro.TextMeshPro _MoveScoreText;

    [Header("Player")]
    public float rotationSpeed = 5f;       // Rotation speed.
    public static bool HandStateRight = false;
    public static bool HandStateLeft = false;

    public float mainSpeed = 70f;
    public float maxSpeed = 10f;
    public float forceMagnitude = 2f;


    public void HandRightClosed()
    {
        HandStateRight = false;

    }
    public void HandRightOpened()
    {
        HandStateRight = true;
    }

    public void HandLeftClosed()
    {
        HandStateLeft = false;

    }
    public void HandLeftOpened()
    {
        HandStateLeft = true;
    }

    private void Update()
    {
        if (HandStateRight) PlayerMovement();
        if (HandStateLeft) GestureRotation();

    }
    private void GestureRotation()
    {
        //float distance = Vector3.Distance(_leftHand.position, _RotationGestoureAnchor.position);
         float distance = Vector3.Distance(new Vector3(_leftHand.localPosition.x,0,0), new Vector3(_RotationGestoureAnchor.localPosition.x,0,0));
        float rotateInfluence;

        if (!Utils.IsFrontAtObject(_RotationGestoureAnchor, _leftHand))
        {
            _MoveScoreText.text = "Rotation left: " + distance.ToString("F2");
            rotateInfluence = -distance * Time.deltaTime * rotationSpeed;
        }
        else
        {
             _MoveScoreText.text = "Rotation right: " + distance.ToString("F2");
            rotateInfluence = +distance * Time.deltaTime * rotationSpeed;
        }

        GetComponent<Rigidbody>().AddTorque(new Vector3(0, rotateInfluence, 0));
    }

    private void PlayerMovement()
    {

        //float distance = Vector3.Distance(_rightHand.position, _MoveGestoureAnchor.position);
        float distance = Vector3.Distance(new Vector3(0,0,_rightHand.localPosition.z), new Vector3(0,0,_MoveGestoureAnchor.localPosition.z));
        float moveInfluence;

        if (Utils.IsFrontAtObject(_MoveGestoureAnchor, _rightHand))
        {
            _MoveScoreText.text = "Move fordward: " + distance.ToString("F2");
            moveInfluence = distance;
        }
        else
        {
            _MoveScoreText.text = "Move backward: " + distance.ToString("F2");
            moveInfluence = -distance;
        }

        float CurrentSpeed = Mathf.Min(mainSpeed * moveInfluence * (forceMagnitude + Time.deltaTime), maxSpeed);

        Vector3 movement = Camera.main.transform.forward * CurrentSpeed;
        GetComponent<Rigidbody>().AddForce(movement);
      //  GetComponent<OVRPlayerController>().Jump();



    }
}