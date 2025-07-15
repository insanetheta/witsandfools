using UnityEngine;

namespace WitsAndFools.Core
{
    /// <summary>
    /// Cleans up duplicate attack/defense areas that may have been created in previous sessions
    /// </summary>
    public class DuplicateAreaCleanup : MonoBehaviour
    {
        void Start()
        {
            CleanupDuplicateAreas();
        }
        
        void CleanupDuplicateAreas()
        {
            // Find all GameObjects with "AttackCardArea" name (duplicates)
            GameObject[] attackCardAreas = GameObject.FindGameObjectsWithTag("Untagged");
            foreach (GameObject obj in attackCardAreas)
            {
                if (obj.name == "AttackCardArea")
                {
                    Debug.Log($"Removing duplicate AttackCardArea: {obj.name}");
                    Destroy(obj);
                }
            }
            
            // Find all GameObjects with "DefenseCardArea" name (duplicates)
            foreach (GameObject obj in attackCardAreas)
            {
                if (obj.name == "DefenseCardArea")
                {
                    Debug.Log($"Removing duplicate DefenseCardArea: {obj.name}");
                    Destroy(obj);
                }
            }
            
            Debug.Log("Duplicate area cleanup complete");
        }
    }
}
