using Networking.Client;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace SourceExtensions
{
    public class CustomMonoBehaviour : MonoBehaviour
    {
        //--------------------------------------------------
        #region INSTRUCTIONS



        public static Dictionary<short, MethodInfo> instructions { get; private set; }  // contains all the methods marked with Instruction attribute


        public static void RegisterAllInstructions()    // makes all instructions ready for usage 
        {
            if (instructions == null)
                instructions = (from type in Assembly.GetExecutingAssembly().GetTypes()
                                from method in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                                where method.GetCustomAttribute(typeof(Client.InstructionAttribute)) != null
                                select method).ToDictionary(x => (x.GetCustomAttribute(typeof(Client.InstructionAttribute)) as Client.InstructionAttribute).id);
        }



        #endregion
        //--------------------------------------------------
        #region EVENTS



        public void Start()
        {
            if (CustomEventSystem.current == null) throw new NoCustomEventSystemException("No currently active custcom event system has been found!");
        }

        protected virtual void OnEnable() 
        {
            CustomEventSystem.onEarlyUpdate += EarlyUpdate;
            CustomEventSystem.onNetworkUpdate += NetworkUpdate; 
            CustomEventSystem.onBeforeDisconnect += BeforeDisconnect;
            CustomEventSystem.onAfterUdpInvolved += AfterUdpInvolved;
            CustomEventSystem.onBeforeConnect += BeforeConnect;
        }

        protected virtual void OnDisable() { 
            CustomEventSystem.onEarlyUpdate -= EarlyUpdate; 
            CustomEventSystem.onNetworkUpdate -= NetworkUpdate;
            CustomEventSystem.onBeforeDisconnect -= BeforeDisconnect; 
            CustomEventSystem.onAfterUdpInvolved -= AfterUdpInvolved;
            CustomEventSystem.onBeforeConnect -= BeforeConnect;
        }

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

        // gets called before connection completes
        protected virtual void BeforeConnect() { }

        // gets called before disconnection completes
        protected virtual void BeforeDisconnect() { }

        // gets called after udp connection is established
        protected virtual void AfterUdpInvolved() { }

        protected virtual void OnAppQuit() { }

        protected virtual void OnAppFinalQuit() { }



        #endregion
        //--------------------------------------------------
    }
}