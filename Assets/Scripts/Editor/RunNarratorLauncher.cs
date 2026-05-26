using UnityEditor;
using UnityEngine;

namespace WitsAndFools.EditorTools
{
    public static class RunNarratorLauncher
    {
        const string PendingKey = "RunNarrator_Pending";

        [MenuItem("Wits and Fools/Narrate Run (Screenshots + Report)")]
        static void Launch()
        {
            if (EditorApplication.isPlaying)
            {
                AttachNarrator();
                return;
            }

            SessionState.SetBool(PendingKey, true);
            Debug.Log("[RunNarrator] Entering Play mode...");
            EditorApplication.isPlaying = true;
        }

        [InitializeOnEnterPlayMode]
        static void OnEnterPlayMode()
        {
            if (!SessionState.GetBool(PendingKey, false)) return;
            SessionState.SetBool(PendingKey, false);
            EditorApplication.delayCall += AttachNarrator;
        }

        static void AttachNarrator()
        {
            if (Object.FindFirstObjectByType<RunNarrator>() != null)
            {
                Debug.LogWarning("[RunNarrator] Already running.");
                return;
            }

            var go = new GameObject("RunNarrator");
            go.AddComponent<RunNarrator>();
            Debug.Log("[RunNarrator] Narrator attached. Run will begin shortly...");
        }
    }
}
