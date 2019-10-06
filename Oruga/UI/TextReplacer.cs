 using System;
using UnityEngine;
using UnityEngine.UI;
using Oruga.Types;
using UnityEngine.Serialization;

namespace Oruga.UI
{
    public class TextReplacer : MonoBehaviour
    {
        public Text text;
        public InputField inputField;

        public StringVariable variableString;
        public IntVariable variableInt;
        public FloatVariable variableFloat;
        public BoolVariable variableBool;

        public bool alwaysUpdate;

        private void OnEnable()
        {
            if (text == null && inputField == null) return;

            string value = null;
            
            if (variableString != null)
            {
                value = variableString.ToString();
            }

            if (variableInt != null)
            {
                value = variableInt.ToString();
            }

            if (variableFloat != null)
            {
                value = variableFloat.ToString();
            }

            if (variableBool != null)
            {
                value = variableBool.ToString();
            }

            if (text != null)
                text.text = value;

            if (inputField != null)
                inputField.text = value;
        }

        private void Update()
        {
            if (alwaysUpdate)
            {
                string value = null;
                
                if (variableString != null)
                {
                    value = variableString.ToString();
                }

                if (variableInt != null)
                {
                    value = variableInt.ToString();
                }

                if (variableFloat != null)
                {
                    value = variableFloat.ToString();
                }

                if (variableBool != null)
                {
                    value = variableBool.ToString();
                }
                
                if (text != null)
                    text.text = value;

                if (inputField != null)
                    inputField.text = value;
            }
        }

        public void BindValueFromInput(string newValue)
        {
            if (variableString != null)
            {
                variableString.SetValue(newValue);
            }

            if (variableInt != null)
            {
                if (newValue != "")
                {
                    int x = 0;
                    if (Int32.TryParse(newValue, out x))
                    {
                        variableInt.SetValue(x);
                    }
                }
                else
                {
                    variableInt.SetValue(null);
                }
            }
        }
        
        public void BindValueFromInput(float newValue)
        {
            if (variableFloat != null)
            {
                variableFloat.SetValue(newValue);
            }
        }
    }
}