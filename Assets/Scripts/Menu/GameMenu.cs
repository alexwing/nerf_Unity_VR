
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CubeBeatWar
{
    [System.Serializable]
    public class GameMenu : MonoBehaviour
    {


        public GameObject menuContainer;
        public GameObject scoreContainer;

        [Tooltip("The hall of fame")]        
        public GameObject hallOfFamePrefab;
        public GameObject menuItemPrefab; 

        [SerializeField] private TMPro.TextMeshPro description;
        public Transform menuAnchor;

        public float menuItemDistance = 1f;

        public List<MenuItem> menuItems = new List<MenuItem>();

        public static GameMenu instance;

        

        public void Awake()
        {
            instance = this;
        }

        public void Start()
        {   
            SceneConfig.gameIsPaused = true;
            menuContainer.SetActive(true);
            hallOfFamePrefab.SetActive(true);
            scoreContainer.SetActive(false);
            createMenu();
        }

        public void disableSelectedMenus()
        {    
            menuItems.ForEach(item => item.gameObject.GetComponent<MenuItemController>().clearSelection());
        }

        private void createMenu(){


            //add menus to the menu container
            foreach (MenuItem menuItem in menuItems)
            {
                
                menuItem.gameObject = Instantiate(menuItemPrefab, menuAnchor);
                menuItem.gameObject.name = menuItem.name;
                menuItem.gameObject.transform.localPosition = new Vector3(0, menuItemDistance * menuItems.IndexOf(menuItem), 0);
                menuItem.gameObject.GetComponent<MenuItemController>().menuItem = menuItem;
            }       

        }

        public void hideMenu(){
            disableSelectedMenus();
            menuContainer.SetActive(false);
            hallOfFamePrefab.SetActive(false);
            scoreContainer.SetActive(true);
        }

        public void showMenu(){
            if (!isActiveMenu())
            {
                SceneConfig.gameIsPaused = true;
                menuContainer.SetActive(true);
                hallOfFamePrefab.SetActive(true);
                scoreContainer.SetActive(false);        
            }else{
                SceneConfig.gameIsPaused = false;
                hideMenu();
                foreach (MenuItem menuItem in menuItems)
                {
                    menuItem.gameObject.GetComponent<MenuItemController>().selectedBox.SetActive(false);
                }
            }
        }


        public bool isActiveMenu(){
            return menuContainer.activeSelf;
        }


        public void Quit()
        {
            Debug.Log("Quit");
            Application.Quit();

        }

        public void setDescription(string description){
            this.description.text = description;
        }


    }
}
