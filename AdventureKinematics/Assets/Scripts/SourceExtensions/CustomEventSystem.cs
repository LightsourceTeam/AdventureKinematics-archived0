using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SourceExtensions
{
    public class CustomEventSystem : EventSystem
    {
        public static event Action onEarlyUpdate;
        public static event Action onNetworkUpdate;
        public static event Action onBeforeDisconnect;
        public static event Action onAfterUdpInvolved;
        public static event Action onBeforeConnect;
        public static bool ConnectedToServer = false;



        protected override void Update()
        {
            if(ConnectedToServer) onNetworkUpdate?.Invoke();

            onEarlyUpdate?.Invoke();

            base.Update();
        }

        public static void NotifyAboutDisconnect() { ConnectedToServer = true; onBeforeDisconnect?.Invoke(); }

        public static void NotifyAboutInvolvingUdp() => onAfterUdpInvolved?.Invoke();

        public static void NotifyAboutConnect() { ConnectedToServer = true;  onBeforeConnect?.Invoke(); }

    }
}
