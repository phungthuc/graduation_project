using System;
using cowsins;
using UnityEngine;

namespace TheTunnel.GOAP
{
    [RequireComponent(typeof(Collider))]
    public class DungeonEnemyWeapom : MonoBehaviour
    {
        [SerializeField]
        private AudioSource audioSource;

        private bool _canHit = true;
        
        private void OnTriggerEnter(Collider other)
        {
            if (!_canHit)
            {
                return;
            }   
            _canHit = false;
            if(other.CompareTag("Player"))
            {
                other.GetComponent<PlayerStats>().Damage(1, false);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            _canHit = true;
        }
    }
}