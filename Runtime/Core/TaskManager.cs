using System.Collections.Generic;

namespace  MS.Taskit
{
    using Diagnose;
    using MS.EventBus;

    public class TaskManager
    {

        private Dictionary<int,Task> _tasks = new Dictionary<int, Task>();
        private string _name;
        private ITaskStatusDatabase _statusDB;

        private EventBus<TaskStatus> _taskStatusEvents = new EventBus<TaskStatus>();

        public TaskManager(string name,ITaskStatusDatabase statusDB){
            _name = name;
            _statusDB = statusDB;
            TaskDiagnose.RegisterManager(this);
        }

        public TaskManager(string name):this(name,new MemoryTaskStatusDB()){

        }

        internal int[] GetTaskIds(){
            return _statusDB.GetTaskIds();
        }

        internal Task GetTask(int taskId){
            return _tasks[taskId];
        }

        internal Task TryGetTask(int taskId){
            Task task = null;
            if(_tasks.TryGetValue(taskId,out task)){
                return task;
            }
            return null;
        }

        private ITaskStatusDatabase statusDB{
            get{
                return _statusDB;
            }
        }

        public TaskStatus GetTaskStatus(int taskId){
            return _statusDB.GetStatus(taskId);
        }

        internal void InternalSetTaskStatus(int taskId,TaskStatus status){
            if(GetTaskStatus(taskId) != status){
                _statusDB.SetStatus(taskId,status);
                try{
                    _taskStatusEvents.Post(status,taskId);
                }finally{
                    if(status == TaskStatus.Archived){
                        _tasks.Remove(taskId);
                    }
                }
            }
        }

        public void On(TaskStatus status,System.Action<int> onTaskChangedTo){
            _taskStatusEvents.On<int>(status,onTaskChangedTo);
        }

        public void Once(TaskStatus status,System.Action<int> onTaskChangedTo){
            _taskStatusEvents.Once<int>(status,onTaskChangedTo);
        }

        public void Off(TaskStatus status,System.Action<int> onTaskChangedTo){
            _taskStatusEvents.Off<int>(status,onTaskChangedTo);
        }

        /// <summary>
        /// 对指定的task进行归档, Task对象会从manager中被移除(如果创建过的话)，无法再被访问，也无法被重新创建
        /// </summary>
        public void ArchiveTask(int taskId){
            var task = TryGetTask(taskId);
            if(task != null){
                task.Archive();
            }else{
                this.InternalSetTaskStatus(taskId,TaskStatus.Archived);
            }
        }

        public bool IsTaskArchived(int taskId){
            return GetTaskStatus(taskId) == TaskStatus.Archived;
        }

        public string name{
            get{
                return _name;
            }
        }

        private void AssertNotArchived(int taskId){
            if(IsTaskArchived(taskId)){
                throw new System.InvalidOperationException($"task {taskId} has been archived");
            }
        }

        /// <summary>
        /// 如果Task已经Archived，则会抛出异常
        /// </summary>
        public Task EnsureTask(int taskId){
            AssertNotArchived(taskId);
            Task task = TryGetTask(taskId);
            if(task == null){
                task = new Task(this,taskId);
                _tasks.Add(taskId,task);
            }
            return task;
        }
    }











}
