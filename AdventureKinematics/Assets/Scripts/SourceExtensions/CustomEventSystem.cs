using System;
using Networking.Client;
using UnityEngine;

namespace SourceExtensions
{
    public class CustomEventSystem : MonoBehaviour
    {
        public static CustomEventSystem current = null;

        public static event Action onEarlyUpdate;
        public static event Action onNetworkUpdate;
        public static event Action onBeforeConnect;
        public static event Action onAfterUdpInvolved;
        public static event Action onBeforeDisconnect;
        public static event Action onAfterForceDisonnect;

        protected void Awake()
        {
            if (current == null) current = this;
            else
            {
                Logging.LogWarning("Two event systems in one scene!");
                Destroy(gameObject);
            }
        }

        protected void Update()
        {
            if (current != this) return;

            onNetworkUpdate?.Invoke();

            onEarlyUpdate?.Invoke();
        }

        protected void OnDestroy()
        {
            current = null;
        }

        public static void NotifyAboutConnect() { onBeforeConnect?.Invoke(); }

        public static void NotifyAboutInvolvingUdp() => onAfterUdpInvolved?.Invoke();

        public static void NotifyAboutDisconnect() { onBeforeDisconnect?.Invoke(); }

        public static void NotifyAboutForceDisconnect() { onAfterForceDisonnect?.Invoke(); }
    }

    public class NoCustomEventSystemException : Exception 
    {
        public NoCustomEventSystemException(string message) : base(message) { }
    }
}
