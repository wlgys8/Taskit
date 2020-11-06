using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace MS.Taskit{
    using MS.EventBus;

    public interface IDependency{

        bool IsActive();

        void OnceActive(Action action);

        string name{
            get;
        }
    }


    public class DefaultDependency : IDependency
    {
        private bool _isActive = false;
        private EventBus _bus = new EventBus(); 

        public DefaultDependency(string name){
            this.name = name;
        }

        public string name {
            get;private set;
        }

        public bool IsActive()
        {
            return _isActive;
        }

        public void SetActive(bool active){
            if(_isActive == active){
                return;
            }
            _isActive = active;
            if(active){
                _bus.Post();
            }
        }

        public void OnceActive(Action action)
        {
            _bus.Once(action);
        }
    }


}

