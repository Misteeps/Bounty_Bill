using System;
using UnityEngine;

namespace Game
{
    public class PickupDetection : MonoBehaviour
    {
        public event Action TouchedAmmo;
        [SerializeField] private LayerMask inactiveBulletLayer;

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if ((inactiveBulletLayer.value & (1 << collision.transform.gameObject.layer)) > 0)
            {
                Debug.Log("touched inactive bullet", collision.gameObject);
                TouchedAmmo?.Invoke();
            }
        }
    }
}
