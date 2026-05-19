using Gameplay;
using UnityEngine;

namespace Coocking
{
    //TODO: delete 18.11.2025
    public class DroppableGeneral_SimpleController : MonoBehaviour
    {
        public Dropable_General _dropable;

        private void Awake()
        {
            if (_dropable == null) { _dropable = GetComponent<Dropable_General>(); }
        }
    }
}