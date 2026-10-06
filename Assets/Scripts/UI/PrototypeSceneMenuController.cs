using UnityEngine;
using UnityEngine.SceneManagement;

namespace Muki.UI
{
    public sealed class PrototypeSceneMenuController : MonoBehaviour
    {
        [SerializeField] private string prototypeScene = "EscenaPrueba";

        public void OpenPrototype()
        {
            if (Application.CanStreamedLevelBeLoaded(prototypeScene))
                SceneManager.LoadScene(prototypeScene);
            else
                Debug.LogError($"No se encontró la escena '{prototypeScene}'. Añádela a Build Settings.");
        }
    }
}
