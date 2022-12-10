
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CubeBeatWar
{

    public class MenuItemController : MonoBehaviour
    {

        [HideInInspector]
        public MenuItem menuItem;

        //raycast to detect if the LineRenderer is over the menu item

        private float activationTime = 0.5f;

        [HideInInspector]
        public float activationTimer = 0;

        public GameObject selectedBox;

        private float localScaleSelectedBox = 0.0f;

        [SerializeField] private TMPro.TextMeshPro title;


        public void Start()
        {
            title.text = menuItem.name;
            activationTime = SceneConfig.activationTime;
            //scale x to 0
            localScaleSelectedBox = selectedBox.transform.localScale.x;
            selectedBox.transform.localScale = new Vector3(0, selectedBox.transform.localScale.y, selectedBox.transform.localScale.z);
           
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.name == "Laser")
            {
                
                activationTimer = activationTime;
                selectedBox.SetActive(true);
                GameMenu.instance.setDescription(menuItem.description);
                StartCoroutine(Activation());
            }
        }

        void OnTriggerExit(Collider other)
        {
            if (other.gameObject.name == "Laser")
            {              
                activationTimer = activationTime;   
                GameMenu.instance.setDescription("");
                selectedBox.SetActive(false);
                StopAllCoroutines();
            }

        }


        private IEnumerator Activation()
        {
            activationTimer = activationTime;
            while (activationTimer > 0)
            {
                activationTimer -= Time.deltaTime;
                selectedBox.transform.localScale = new Vector3(localScaleSelectedBox * activationTimer / activationTime, selectedBox.transform.localScale.y, selectedBox.transform.localScale.z);
                yield return null;
            }
                    selectedBox.transform.localScale = new Vector3(0, selectedBox.transform.localScale.y, selectedBox.transform.localScale.z);
                    menuItem.onSelected.Invoke();
 

        }


        public void clearSelection()
        {
            activationTimer = 0;
            selectedBox.SetActive(false);
        }

    }

}