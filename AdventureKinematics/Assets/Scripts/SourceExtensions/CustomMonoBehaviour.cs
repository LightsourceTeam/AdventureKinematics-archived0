using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace SourceExtensions
{
    public class CustomMonoBehaviour : MonoBehaviour
    {
        protected virtual void OnEnable() { CustomEventSystem.onEarlyUpdate += EarlyUpdate; CustomEventSystem.onNetworkUpdate += NetworkUpdate; CustomEventSystem.onBeforeDisconnect += BeforeDisconnect; }

        protected virtual void OnDisable() { CustomEventSystem.onEarlyUpdate -= EarlyUpdate; CustomEventSystem.onNetworkUpdate -= NetworkUpdate; CustomEventSystem.onBeforeDisconnect -= BeforeDisconnect; }

        // gets called every time an instruction from server gets executed
        protected virtual void NetworkUpdate()
        {

        }

        // gets called in the beginning of the frame before other functions
        protected virtual void EarlyUpdate() { }

        // gets called several unfixed amount of times per frame with fixed interval, used for physics simulations 
        protected virtual void FixedUpdate() { }

        // heart of the cycle that gets called once per frame
        protected virtual void Update() { }

        // gets called in the end of the frame
        protected virtual void LateUpdate() { }

        // gets called before disconnection completes
        protected virtual void BeforeDisconnect() { }


    }
}