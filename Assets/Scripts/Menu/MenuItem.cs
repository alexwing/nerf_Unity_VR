
using UnityEngine;
using UnityEngine.Events;
namespace CubeBeatWar
{
    [System.Serializable]
    public class MenuItem
    {
        public string name;
        public string description;

        [HideInInspector]
        public GameObject gameObject;
        
        public bool isActive;        
        public UnityEvent onSelected;
    

    }
}