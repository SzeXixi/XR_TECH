using UnityEngine;
using UnityEngine.SceneManagement;
namespace Rokid.UXR.Utility
{
    public class DestoryOnLoad : MonoBehaviour
    {
        private void Awake()
        {
            SceneManager.sceneUnloaded += OnSceneUnLoaded;
        }

        private void OnDestroy()
        {
            SceneManager.sceneUnloaded -= OnSceneUnLoaded;
        }

        private void OnSceneUnLoaded(Scene scene)
        {
            Destroy(this.gameObject);
        }
    }
}


