using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace GestureRecognizer
{
    [System.Serializable]
    public class GestoureController : MonoBehaviour
    {
        public float threshold = 0.1f;
        public bool debugMode = true;
        public OVRCustomSkeleton skeleton;
        public OVRInput.Controller controller;
        public List<Gesture> gestures = new List<Gesture>();
        private List<OVRBone> fingerBones;

        private enum GestureInvokeState
        {
            START,
            END
        }


        [HideInInspector]
        public float activationTime = 0.5f;

        [HideInInspector]
        public float activationTimer = 0;


        private Gesture previousGesture;


        void Start()
        {
            fingerBones = new List<OVRBone>(skeleton.Bones);
            previousGesture = new Gesture();
        }


        public void SetActiveHands(bool active)
        {
            SkinnedMeshRenderer pathToHand = skeleton.GetComponentInChildren<SkinnedMeshRenderer>();            
            if (pathToHand)
            {
                pathToHand.enabled = active;
            }
 
        }

        // Update is called once per frame
        void Update()
        {
            //is valid position
            if (!OVRInput.GetControllerOrientationTracked(controller))
            {
                //_GestureScoreText.text = controller + " Disabled";
                SetActiveHands(false);
                return;
            }
            SetActiveHands(true);
            if (debugMode && Input.GetKeyDown(KeyCode.Space))
            {
                save();
            }

            //get current gesture
            Gesture currentGesture = Reconized();
            bool hasRecognized = !currentGesture.Equals(new Gesture());
            //check gesture status
            if (hasRecognized && !currentGesture.name.Equals(previousGesture.name))
            {
                //Debug.Log("New gesture found: " + currentGesture.name);
                if (!previousGesture.Equals(new Gesture()))
                {
                    if (previousGesture.onRecognitionEnded != null)
                    {
                        runActivation(previousGesture, GestureInvokeState.END);
                      
                    }
                }
                previousGesture = currentGesture;
                if (currentGesture.onRecognized != null)
                {
                    runActivation(currentGesture, GestureInvokeState.START);                    
                }
            }
            if (!hasRecognized && !previousGesture.Equals(new Gesture()))
            {
                if (previousGesture.onRecognitionEnded != null)
                {
                    runActivation(previousGesture, GestureInvokeState.END);                    
                }
                previousGesture = new Gesture();
            }

        }

        void save()
        {
            fingerBones = new List<OVRBone>(skeleton.Bones);
            Gesture g = new Gesture();
            g.name = "new gesture";
            List<Vector3> data = new List<Vector3>();
            foreach (OVRBone bone in fingerBones)
            {
                //  Debug.Log("bone: " + bone.Id);
                data.Add(skeleton.transform.InverseTransformPoint(bone.Transform.position));
            }
            g.fingerDatas = data;
            gestures.Add(g);

        }
        Gesture Reconized()
        {
            fingerBones = new List<OVRBone>(skeleton.Bones);
            Gesture currentGesture = new Gesture();
            float currentMin = float.MaxValue;
            foreach (Gesture gesture in gestures)
            {
                if (!gesture.active)
                {
                    continue;
                }
                float sumDistance = 0;
                bool isdiscarded = false;
                for (int i = 0; i < fingerBones.Count; i++)
                {
                    Vector3 currentData = skeleton.transform.InverseTransformPoint(fingerBones[i].Transform.position);
                    float distance = Vector3.Distance(currentData, gesture.fingerDatas[i]);
                    if (distance > threshold)
                    {
                        // Debug.Log("bone: " + fingerBones[i].Id + " distance: " + distance);
                        isdiscarded = true;
                        break;
                    }
                    sumDistance += distance;


                }
                if (!isdiscarded && sumDistance < currentMin)
                {
                    //currentMin = sumDistance;
                    currentGesture = gesture;
                   // Debug.Log("New gesture found2: " + currentGesture.name);
                }

            }
            return currentGesture;
        }

        private void runActivation(Gesture gesture, GestureInvokeState state)
        {
            activationTimer = 0;
            activationTime = gesture.activationTime;
            StopAllCoroutines();
            if (gesture.active){
                UnityEvent action = null;
                switch (state)
                {
                    case GestureInvokeState.START:
                        action = gesture.onRecognized;
                        break;
                    case GestureInvokeState.END:
                        action = gesture.onRecognitionEnded;
                        break;
                }
                if (action != null )
                {
                    if (activationTime == 0){
                        action.Invoke();
                    }else{
                        StartCoroutine(Activation(action));
                    }
                }
                                
            }
        }

            
            


        
        private IEnumerator Activation(UnityEvent action)
        {
            activationTimer = activationTime;
            while (activationTimer > 0)
            {
                activationTimer -= Time.deltaTime;
                yield return null;
            }
            action.Invoke();
        }        
    }
}