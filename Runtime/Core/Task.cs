using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MS.Taskit{

    using Diagnose;
    using MS.EventBus;

    public enum TaskStatus{

        ///任务还未开始
        Idle,
        //表示任务在等待激活，前置条件还没有完全满足
        Pending,
        //Active表示任务已经激活，其前置要求都已经得到满足
        Active,
        //Completed表示任务已经完成
        Completed,
        //Archived表示任务归档（例如领取了奖励）
        Archived,
    }



    public class Task{

        private int _taskId;
        private List<IDependency> _dependencies = new List<IDependency>();
        private EventBus<TaskStatus> _eventbus = new EventBus<TaskStatus>();
        private TaskManager _manager;
        internal Task(TaskManager manager,int taskId){
            _manager = manager;
            _taskId = taskId;
        }

        public void On(TaskStatus status,System.Action<Task> action){
            _eventbus.On(status,action);
        }

        public void Off(TaskStatus status,System.Action<Task> action){
            _eventbus.Off(status,action);
        }

        public void Once(TaskStatus status,System.Action<Task> action){
            _eventbus.Once(status,action);
        }

        public void AddDependency(IDependency dep){
            _dependencies.Add(dep);
        }

        public bool RemoveDependency(IDependency dep){
            return _dependencies.Remove(dep);
        }

        public bool ContainsDependencyWithName(string name){
            foreach(var d in _dependencies){
                if(d.name == name){
                    return true;
                }
            }
            return false;
        }

        internal IDependency[] GetDependencies(){
            return _dependencies.ToArray();
        }

        /// <summary>
        /// 开启任务(只有Idle状态的任务可以开启)，状态变为Pending
        /// </summary>
        /// <returns>表示任务是否成功开启</returns>
        public bool Start(){
            if(this.status != TaskStatus.Idle){
                Debug.LogWarning("only idle task can be started");
                return false;
            }
            this.status = TaskStatus.Pending;
            _eventbus.Post(TaskStatus.Pending,this);

            this.CheckDependencies();
            return true;
        }

        /// <summary>
        /// 当任务的所有依赖条件都激活，就会调用次方法，将状态切换为Actived
        /// </summary>
        private void Active(){
            if(this.status!= TaskStatus.Pending){
                Debug.LogWarning("only pending task can be actived");
                return;
            }
            this.status = TaskStatus.Active;
            _eventbus.Post(TaskStatus.Active,this);
        }

        /// <summary>
        /// 调用此方法，可以将Actived状态的任务标记为Completed
        /// </summary>
        public bool Complete(){
            if(this.status != TaskStatus.Active){
                Debug.LogWarning($"only actived task can be completed,current status is {status}, taskId = {_taskId}");
                return false;
            }
            this.status = TaskStatus.Completed;
            _eventbus.Post(TaskStatus.Completed,this);
            return true;
        }


        /// <summary>
        /// 调用Archive来归档一个任务。 任何状态的任务，都可以被直接归档。归档后将无法再被访问
        /// </summary>
        public bool Archive(){
            if(this.status == TaskStatus.Archived){
                return false;
            }
            this.status = TaskStatus.Archived;
            _eventbus.Post(TaskStatus.Archived,this);
            return true;
        }   

        /// <summary>
        /// 检查并刷新依赖条件的状态。如果全部依赖条件都满足，那么切换为任务为Actived状态
        /// </summary>
        private void CheckDependencies(){
            try{
                if(this.status == TaskStatus.Pending){
                    foreach(var d in _dependencies){
                        if(!d.IsActive()){
                            d.OnceActive(this.CheckDependencies);
                            return;
                        }
                    }
                    //all dependencies were actived
                    this.Active();
                }
            }finally{
                TaskDiagnose.FireDependenciesUpdate(this);
            }
        }
        public TaskStatus status{
            get{
                return _manager.GetTaskStatus(_taskId);
            }private set{
                _manager.InternalSetTaskStatus(_taskId,value);
            }
        }
        public int taskId{
            get{
                return _taskId;
            }
        }

        public override string ToString(){
            return _taskId.ToString();
        }
    }
}
