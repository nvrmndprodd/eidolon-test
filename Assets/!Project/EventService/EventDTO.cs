using System;

namespace EventService
{
    [Serializable]
    public struct EventDTO
    {
        public string type;
        public string data;
    }
}