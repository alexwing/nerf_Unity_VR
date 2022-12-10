using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TargetSnap : MonoBehaviour
{
    private Vector3 snapToPos;
    private Quaternion snapToRot;
    private Rigidbody body;
    public float snapTime = 2;

    private float dropTimer;
    public bool interactable = true;

    private void awake()
    {

    }

    private void Start()
    {
        snapToPos = new Vector3(transform.position.x, transform.position.y, transform.position.z);
        snapToRot = new Quaternion(transform.rotation.x, transform.rotation.y, transform.rotation.z, transform.rotation.w);
        body = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {

        if (interactable)
        {
            //  body.isKinematic = false;
            dropTimer = -1;
        }
        else
        {
            dropTimer += Time.deltaTime / (snapTime / 2);

            //  body.isKinematic = dropTimer > 1;

            if (dropTimer > 1)
            {
                body.velocity = new Vector3(0f, 0f, 0f);
                body.angularVelocity = new Vector3(0f, 0f, 0f);
                //transform.parent = snapTo;
                transform.position = snapToPos;
                transform.rotation = snapToRot;

                interactable = true;
            }
            else
            {
                float t = Mathf.Pow(35, dropTimer);

                body.velocity = Vector3.Lerp(body.velocity, Vector3.zero, Time.fixedDeltaTime * 4);
                if (body.useGravity)
                    body.AddForce(-Physics.gravity);

                transform.position = Vector3.Lerp(transform.position, snapToPos, Time.fixedDeltaTime * t * 3);
                transform.rotation = Quaternion.Slerp(transform.rotation, snapToRot, Time.fixedDeltaTime * t * 2);

            }
        }
    }

}