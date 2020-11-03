using System;
using Networking.Client;
using UnityEngine.EventSystems;

namespace SourceExtensions
{
    public class CustomEventSystem : EventSystem
    {
        public static CustomEventSystem instance = null;

        public static event Action onEarlyUpdate;
        public static event Action onNetworkUpdate;
        public static event Action onBeforeConnect;
        public static event Action onAfterUdpInvolved;
        public static event Action onBeforeDisconnect;
        public static event Action onAfterForceDisonnect;

        protected override void Awake()
        {
            if (instance == null) instance = this;
            else Logging.LogWarning("Two event systems in one scene!");
        }

        protected override void Update()
        {
            if (instance != this) return;

            onNetworkUpdate?.Invoke();

            onEarlyUpdate?.Invoke();

            base.Update();
        }

        public static void NotifyAboutConnect() { onBeforeConnect?.Invoke(); }

        public static void NotifyAboutInvolvingUdp() => onAfterUdpInvolved?.Invoke();

        public static void NotifyAboutDisconnect() { onBeforeDisconnect?.Invoke(); }

        public static void NotifyAboutForceDisconnect() { onAfterForceDisonnect?.Invoke(); }
    }
}
