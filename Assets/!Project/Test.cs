using UnityEditor;
using UnityEngine;

namespace DefaultNamespace
{
    public class Test : MonoBehaviour
    {
        public EventService.EventService service;
        
        private void Update()
        {
            if (Input.GetKey(KeyCode.A))
            {
                var type = GUID.Generate().ToString();
                var data = GUID.Generate().ToString();
                service.TrackEvent(type, data);
            }
        }

        private void First()
        {
            var hit = Physics2D.Raycast(Vector2.zero, Vector2.zero);
        }
    }
}