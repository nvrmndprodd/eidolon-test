using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace EventService
{
    public class EventService : MonoBehaviour
    {
        private const string EVENTS_KEY = "events";
        private const float REQUEST_COOLDOWN = 2f;
        private const int REQUEST_TIMEOUT = 5;
        private const long RESPONSE_CODE_OK = 200;

        private readonly List<EventDTO> _bufferToWrite = new(100);
        private List<EventDTO> _bufferToSend = new(100);
        
        private string _serverUrl = "localhost:3000/api/events";
        private float _requestCooldown;
        private bool _performingRequest;
        
        public void Construct(string serverUrl)
        {
            _serverUrl = serverUrl;
        }

        private void Start()
        {
            TrySendEventsAfterUnpause();
        }

        private void Update()
        {
            if (_bufferToSend.Count == 0 && _bufferToWrite.Count == 0)
            {
                return;
            }
            
            _requestCooldown -= Time.deltaTime;

            if (_requestCooldown > 0 || _performingRequest == true)
            {
                return;
            }
            
            Debug.Log("Timer expired. Preparing to send events.");
            _bufferToSend.AddRange(_bufferToWrite);
            _bufferToWrite.Clear();
                
            TrySendEvents().Forget();
        }

#if UNITY_ANDROID
        // OnApplicationQuit not working properly as far as I know
        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus == true)
            {
                SaveEventsForLater();
            }
            else
            {
                TrySendEventsAfterUnpause();
            }
        }
#elif UNITY_WEBGL
        // call from JS code that is subscribed to tab switch/close browser events
        public void SaveEvents()
        {
            SaveEventsForLater();
        }
#endif

        private void OnApplicationQuit()
        {
            SaveEventsForLater();
        }

        public void TrackEvent(string type, string data)
        {
            if (_bufferToWrite.Count == 0)
            {
                Debug.Log("Cooldown started");
                _requestCooldown = REQUEST_COOLDOWN;
            }
            
            Debug.Log($"Add type: {type} data: {data}");
            _bufferToWrite.Add(new EventDTO(type, data));
        }

        private void TrySendEventsAfterUnpause()
        {
            if (!PlayerPrefs.HasKey(EVENTS_KEY))
            {
                Debug.Log("No events after unpause");
                return;
            }
            
            var json = PlayerPrefs.GetString(EVENTS_KEY);
            PlayerPrefs.DeleteKey(EVENTS_KEY);
            _bufferToSend = JsonConvert.DeserializeObject<List<EventDTO>>(json);
            TrySendEvents().Forget();
        }

        private async UniTask TrySendEvents()
        {
            _performingRequest = true;
            var json = JsonConvert.SerializeObject(_bufferToSend);
            Debug.Log($"Trying to send {_bufferToSend.Count} events. Message: {json}");

            var webRequest = UnityWebRequest.Post(_serverUrl, json);
            webRequest.timeout = REQUEST_TIMEOUT;

            try
            {
                await webRequest.SendWebRequest();
            }
            catch (Exception e)
            {
                Debug.Log(e);
                _performingRequest = false;
            }
            finally
            {
                Debug.Log($"responseCode: {webRequest.responseCode}");
                
                if (webRequest.responseCode == RESPONSE_CODE_OK || true)
                {
                    _bufferToSend.Clear();
                    _performingRequest = false;
                    Debug.Log("Web request successful");
                }
                
                webRequest.Dispose();
            }
        }

        private void SaveEventsForLater()
        {
            // better to add some kind of message id/hash when performing request
            // so server can handle if we send the same message twice or so
            // it can happen if we quit before getting code 200
            
            if (_bufferToSend.Count == 0 && _bufferToWrite.Count == 0)
            {
                Debug.Log("No events to save for later");
                return;
            }

            _bufferToSend.AddRange(_bufferToWrite);
            
            var json = JsonConvert.SerializeObject(_bufferToSend);
            PlayerPrefs.SetString(EVENTS_KEY, json);
            
            _bufferToSend.Clear();
            _bufferToWrite.Clear();
            Debug.Log($"Saved {_bufferToSend.Count} events. Json: {json}");
        }
    }
}