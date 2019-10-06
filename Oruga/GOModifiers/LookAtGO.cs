using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Oruga.GOModifiers
{
    public class LookAtGO : MonoBehaviour
    {
        
        public Transform target;
        public float speed = 10f;
        
//        void Update()
//        {
//            if(target != null)
//            {
//                transform.LookAt(target, Vector3.up);
//            }
//        }
        
        void Update ()
        {
            transform.Rotate(Vector3.up, speed * Time.deltaTime);
        }
    }
    
}
