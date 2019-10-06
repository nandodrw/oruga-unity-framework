using System;
using System.Collections;
using Oruga.Types.BaseClasses;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Oruga.Services
{
    // Code inspired by http://myriadgamesstudio.com/how-to-use-the-unity-scenemanager/
    public class SceneService : Singleton<SceneService>
    {
        // Loading Progress: private setter, public getter
        private float _loadingProgress;
        public float LoadingProgress => _loadingProgress;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        public IEnumerator LoadSceneAsync(string sceneName,  LoadSceneMode mode)
        {
            var asyncScene = SceneManager.LoadSceneAsync(sceneName, mode);

            // this value stops the scene from displaying when it's finished loading
            asyncScene.allowSceneActivation = false;

            while (!asyncScene.isDone)
            {
                // loading bar progress
                _loadingProgress = Mathf.Clamp01(asyncScene.progress / 0.9f) * 100;

                // scene has loaded as much as possible, the last 10% can't be multi-threaded
                if (asyncScene.progress >= 0.9f)
                {
                    // we finally show the scene
                    asyncScene.allowSceneActivation = true;
                }

                yield return null;
            }
        }
        
        public void LoadSceneSync(string sceneName)
        {
            SceneManager.LoadScene(sceneName);
        }
    }
}

