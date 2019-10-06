using System;
using System.Collections;
using UnityEngine;
using Oruga.Types;
using UnityEngine.Events;

namespace Oruga.Providers
{
    public class GeoLocationProvider : MonoBehaviour
    {

        public BoolVariable debugModeFlag;
        public FloatVariable accuracy;

        public FloatVariable latitude;
        public FloatVariable longitude;
        public FloatVariable horizontalAccuracy;

        public FloatVariable northHeading;
        public FloatVariable northHeadingAccuracy;

        public UnityEvent locationDataStart;
        public UnityEvent locationDataReceived;
        
        
        public FloatVariable minimumGpsAccuracyThreshold;
        public UnityEvent gpsGotIntoAccuracyThreshold;
        public UnityEvent gpsGotOverAccuracyThreshold;
        private bool _gpsAccuracyThresholdReached = false;
        
        private bool _locationProviderInitialized = false;

        // Start is called before the first frame update
        private void Awake()
        {
            StartCoroutine(StartService());
        }

        IEnumerator StartService()
        {
#if UNITY_EDITOR
            yield return new WaitForSeconds(5);
#endif

            // First, check if user has location service enabled
            if (!Input.location.isEnabledByUser)
            {
                if (debugModeFlag != null && debugModeFlag.Value)
                {
                    Debug.LogError("GPS acces was denied");
                    Console.WriteLine("GPS acces was denied");
                }

                yield break;
            }

            // Start location service
            if (accuracy != null)
            {
                if (debugModeFlag != null && debugModeFlag.Value)
                {
                    Debug.Log($"Initializing GeoLocation provider, Accuracy: {accuracy.Value}");
                    Console.WriteLine("Initializing GeoLocation provider, Accuracy: {0}" + accuracy.Value);
                }
                Input.location.Start(accuracy.Value);
                Input.compass.enabled = true;
            }

            else
            {
                Input.location.Start();
            }

            StartCoroutine(ListenLocation());
        }

        public void StopService()
        {
            StopCoroutine(ListenLocation());

            Input.location.Stop();
            Input.compass.enabled = false;

            _locationProviderInitialized = false;
            if (debugModeFlag != null && debugModeFlag.Value)
            {
                Debug.Log("GeoLocation provider stopped");
                Console.WriteLine("GeoLocation provider stopped");
            }
        }

        IEnumerator ListenLocation()
        {
            bool loopEnabled = true;

            while (loopEnabled)
            {
                if (debugModeFlag != null && debugModeFlag.Value)
                {
                    Debug.Log("Looping on Listening GeoLocation");
                }

                // Delay on loop execution otherwise code will break
                yield return new WaitForSeconds(0.25f);

                while (Input.location.status == LocationServiceStatus.Initializing)
                {
                    if (debugModeFlag != null && debugModeFlag.Value)
                    {
                        Debug.Log("GeoLocation is initializing");
                        Console.WriteLine("GeoLocation is initializing");
                    }

                    yield return new WaitForSeconds(1.5f);
                }

                if (Input.location.status == LocationServiceStatus.Failed)
                {
                    if (debugModeFlag != null && debugModeFlag.Value)
                    {
                        Debug.LogError("Unable to determine device location");
                        Console.WriteLine("Unable to determine device location");
                    }

                    loopEnabled = false;
                    yield break;
                }

                if (Input.location.status == LocationServiceStatus.Running
                    && !_locationProviderInitialized)
                {
                    locationDataStart.Invoke();
                    _locationProviderInitialized = true;

                    if (debugModeFlag != null && debugModeFlag.Value)
                    {
                        Debug.Log("GeoLocation initialized");
                        Console.WriteLine("GeoLocation initialized");
                    }
                }

                if (Input.location.status == LocationServiceStatus.Running &&
                    _locationProviderInitialized)
                {
                    if (debugModeFlag != null && debugModeFlag.Value)
                    {
                        Debug.Log("Reading Location data");
                        Console.WriteLine("Reading Location data");
                    }

                    // Emiting Location data
                    locationDataReceived.Invoke();

                    // Update data holders
                    if (latitude != null)
                        latitude.SetValue(Input.location.lastData.latitude);

                    if (longitude != null)
                        longitude.SetValue(Input.location.lastData.longitude);

                    if (horizontalAccuracy != null)
                        horizontalAccuracy.SetValue(Input.location.lastData.horizontalAccuracy);

                    if (northHeading != null)
                        northHeading.SetValue(Input.compass.trueHeading);

                    if (northHeadingAccuracy != null)
                        northHeadingAccuracy.SetValue(Input.compass.headingAccuracy);

                    if (!_gpsAccuracyThresholdReached && horizontalAccuracy.Value <= minimumGpsAccuracyThreshold.Value)
                    {
                        gpsGotIntoAccuracyThreshold.Invoke();
                        _gpsAccuracyThresholdReached = true;
                    }

                    if (_gpsAccuracyThresholdReached && horizontalAccuracy.Value > minimumGpsAccuracyThreshold.Value)
                    {
                        gpsGotOverAccuracyThreshold.Invoke();
                        _gpsAccuracyThresholdReached = false;
                    }
                }
            }
        }

        private void OnDestroy()
        {
            Input.location.Stop();
            Input.compass.enabled = false;
        }

        private void OnDisable()
        {
            Input.location.Stop();
            Input.compass.enabled = false;
        }

    }
}