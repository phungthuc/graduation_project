using UnityEngine;

namespace TheTunnel.Enemy
{
    public class EnemyHealth : cowsins.EnemyHealth
    {
        public void SetHealth(float value)
        {
           maxHealth = value;
           ResetHealth();
        }
        
        public void ResetHealth()
        {
            isDead = false;
            health = maxHealth;
            if (healthSlider != null)
            {
                healthSlider.value = health;
            }
        }
    }
}