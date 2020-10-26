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

        protected override void Update()
        {

            /*networkTimes = Time.deltaTime / networkDeltaTime + extraTime;
            extraTime = networkTimes - (int)networkTimes;
            for (int i = 0; i < (int)networkTimes; i++) networkUpdate?.Invoke();*/

            for (int i = 0; i < networkTimes; i++) onNetworkUpdate?.Invoke();
            onEarlyUpdate?.Invoke();

            base.Update();
        }

        public static void InvokeBeforeDisconnect() { onBeforeDisconnect?.Invoke(); }

        int networkTimes = 2;
        float networkDeltaTime = 0.08f;
        float extraTime = 0f;
    }
}
