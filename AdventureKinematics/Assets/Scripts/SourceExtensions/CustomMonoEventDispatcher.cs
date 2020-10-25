using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SourceExtensions
{
    public class CustomMonoEventDispatcher : MonoBehaviour
    {
        public static event Action earlyUpdate;
        public static event Action networkUpdate;

        void Update()
        {
            networkTimes = Time.deltaTime / networkDeltaTime + extraTime;
            extraTime = networkTimes - (int)networkTimes;
            for (int i = 0; i < (int)networkTimes; i++) networkUpdate?.Invoke();
            
            earlyUpdate?.Invoke();
        }

        float networkTimes = 1f;
        float networkDeltaTime = 0.08f;
        float extraTime = 0f;
    }
}
