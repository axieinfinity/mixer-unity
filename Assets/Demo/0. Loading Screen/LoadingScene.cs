using UnityEngine;
using UnityEngine.SceneManagement;

namespace SkyMavis.AxieMixer.Unity.Demo
{
    public class LoadingScene : MonoBehaviour
    {
        public void OnButtonClicked(int idx)
        {
            if (idx == 0)
            {
                SceneManager.LoadScene("DemoMixer");
            }
            else
            {
                SceneManager.LoadScene("FlappyAxie");
            }
        }
    }
}
