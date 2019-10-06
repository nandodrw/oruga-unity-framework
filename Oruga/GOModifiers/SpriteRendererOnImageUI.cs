using UnityEngine;
using UnityEngine.UI;

// Took from: https://boiagames.blogspot.com/2015/08/reusing-your-unity-in-game-animations.html
namespace Oruga.GOModifiers
{
    public class SpriteRendererOnImageUI : MonoBehaviour
    {
        
        // set the controller you want to use in the inspector
        public RuntimeAnimatorController controller;
        
        // the UI/Image component
        Image imageCanvas;
        // the fake SpriteRenderer
        SpriteRenderer fakeRenderer;
        // the Animator
        Animator animator;
        
        
        void Start ()
        {
            imageCanvas = GetComponent<Image>();
            fakeRenderer = gameObject.AddComponent<SpriteRenderer>();
            // avoid the SpriteRenderer to be rendered
            fakeRenderer.enabled = false;
            animator = gameObject.AddComponent<Animator>();
            
            // set the controller
            animator.runtimeAnimatorController = controller;
        }
        
        void Update () {
            // if a controller is running, set the sprite
            if (animator.runtimeAnimatorController) {
                imageCanvas.sprite = fakeRenderer.sprite;
            }
        }
        
    }
}
