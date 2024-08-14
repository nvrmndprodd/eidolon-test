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
        private const long RESPONSE_CODE_OK = 200;
        
        private readonly List<EventDTO> _bufferToWrite = new(100);
        private readonly List<EventDTO> _bufferToSend = new(100);
        
        private string _serverUrl = "localhost:3000/api/events";
        private bool _performingRequest;

        private void Start()
        {
            if (PlayerPrefs.HasKey(EVENTS_KEY))
            {
                var json = PlayerPrefs.GetString(EVENTS_KEY);
                PlayerPrefs.DeleteKey(EVENTS_KEY);
                // deserialize directly into buffer to send why not imo
            }
        }

        private void Update()
        {
            // just ticking timer and send request if previous request already finished
            if (timer && not sending request rn)
            {
                _bufferToSend.AddRange(_bufferToWrite);
                _bufferToWrite.Clear();
                
                _performingRequest = true;
                
                TrySendEvents().Forget();
            }
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
                TrySendEventsAfterUnpause().Forget();
            }
        }
#elif UNITY_WEBGL
        // call from JS code that is subscribed to tab switch/close browser events
        public void SaveEvents()
        {
            SaveEventsForLater();
        }
#endif

        public void Init(string serverUrl)
        {
            _serverUrl = serverUrl;
        }

        private async UniTask TrySendEvents()
        {
            var json = JsonConvert.SerializeObject(_bufferToSend);

            var webRequest = UnityWebRequest.Post(_serverUrl, json);

            await webRequest.SendWebRequest();

            if (webRequest.responseCode == RESPONSE_CODE_OK)
            {
                _bufferToSend.Clear();
                _performingRequest = false;
            }
        }

        private void SaveEventsForLater()
        {
            if (_bufferToSend.Count == 0 && _bufferToWrite.Count == 0)
            {
                PlayerPrefs.DeleteKey(EVENTS_KEY);
            }
            
            _bufferToSend.AddRange(_bufferToWrite);
            var json = JsonConvert.SerializeObject(_bufferToSend);
            
        }
    }
}