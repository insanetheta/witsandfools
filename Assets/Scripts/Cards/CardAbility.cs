using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WitsAndFools.Cards
{
    /// <summary>
    /// Base class for all card abilities
    /// </summary>
    public abstract class CardAbility : MonoBehaviour
    {
        [Header("Ability Info")]
        public string abilityName;
        public string description;
        public Core.CardAbilityType abilityType;
        
        [Header("Activation Settings")]
        public bool canActivateOnPlay = true;
        public bool canActivateOnDefense = false;
        public bool isPassive = false;
        
        // Events
        public System.Action<CardAbility> OnAbilityActivated;
        public System.Action<CardAbility> OnAbilityResolved;
        
        /// <summary>
        /// Check if this ability can be activated in current context
        /// </summary>
        /// <returns>True if ability can be activated</returns>
        public abstract bool CanActivate();
        
        /// <summary>
        /// Activate the ability
        /// </summary>
        /// <returns>True if activation was successful</returns>
        public abstract bool Activate();
        
        /// <summary>
        /// Get the ability description with current context
        /// </summary>
        /// <returns>Formatted description string</returns>
        public virtual string GetDescription()
        {
            return description;
        }
        
        /// <summary>
        /// Called when ability is triggered
        /// </summary>
        protected virtual void OnAbilityTriggered()
        {
            UnityEngine.Debug.Log($"Ability '{abilityName}' activated");
            OnAbilityActivated?.Invoke(this);
        }
        
        /// <summary>
        /// Called when ability effect is complete
        /// </summary>
        protected virtual void OnAbilityComplete()
        {
            UnityEngine.Debug.Log($"Ability '{abilityName}' resolved");
            OnAbilityResolved?.Invoke(this);
        }
    }
}