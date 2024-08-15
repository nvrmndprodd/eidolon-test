using System;

namespace EventService
{
    [Serializable]
    public struct EventDTO
    {
        public string type;
        public string data;

        public EventDTO(string type, string data)
        {
            this.type = type;
            this.data = data;
        }
    }
}