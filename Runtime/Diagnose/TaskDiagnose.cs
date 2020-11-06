using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Diagnostics;

namespace MS.Taskit.Diagnose{

    /// <summary>
    /// Only work in UnityEditor
    /// </summary>
    public class TaskDiagnose
    {

        public static event System.Action onManagerRegistered;
        public static event System.Action<Task> onTaskDependenciesUpdate;

        private static Dictionary<int,System.WeakReference<TaskManager>> _managerDict = new Dictionary<int, System.WeakReference<TaskManager>>();


        [Conditional("UNITY_EDITOR")]
        internal static void RegisterManager(TaskManager manager){
            _managerDict.Add(manager.GetHashCode(),new System.WeakReference<TaskManager>(manager));
            if(onManagerRegistered != null){
                onManagerRegistered();
            }
        }

        [Conditional("UNITY_EDITOR")]
        internal static void FireDependenciesUpdate(Task task){
            if(onTaskDependenciesUpdate != null){
                onTaskDependenciesUpdate(task);
            }
        }

        public static List<TaskManagerDebugInfo> GetTaskManagers(){
            List<TaskManagerDebugInfo> result = new List<TaskManagerDebugInfo>();
            foreach(var kv in _managerDict){
                var value = kv.Value;
                TaskManager manager;
                if(value.TryGetTarget(out manager)){
                    result.Add(new TaskManagerDebugInfo(){
                        name = manager.name,
                        referenceId = kv.Key
                    });
                }
            }
            return result;
        }

        private static TaskManager GetManager(int managerId){
            System.WeakReference<TaskManager> managerRef;
            if(_managerDict.TryGetValue(managerId,out managerRef)){
                TaskManager manager;
                if(managerRef.TryGetTarget(out manager)){
                    return manager;
                }
            }
            return null;
        }

        public static TaskDebugInfo GetTaskDebugInfo(int managerId,int taskId){
            TaskManager manager = GetManager(managerId);
            if(manager != null){
                var task = manager.GetTask(taskId);
                return new TaskDebugInfo(){
                    status = task.status
                };
            }
            return default(TaskDebugInfo);
        }

        public static List<TaskDependencyDebugInfo> GetDependencyDebugInfos(int managerId,int taskId){

            List<TaskDependencyDebugInfo> result = new List<TaskDependencyDebugInfo>();
            TaskManager manager = GetManager(managerId);
            if(manager != null){
                var task = manager.GetTask(taskId);
                if(task.status == TaskStatus.Archived || task.status == TaskStatus.Completed){
                    //如果已经完成或者归档了，就不需要再监控依赖了
                    return result;
                }
                var dependencies = task.GetDependencies();
                foreach(var d in dependencies){
                    result.Add(new TaskDependencyDebugInfo(){
                        name = d.name,
                        isActive = d.IsActive(),
                    });
                }
            }
            return result;
        }

        public static List<int> GetUnarchivedTaskIds(int managerId){
            TaskManager manager = GetManager(managerId);
            if(manager != null){
                return manager.GetTaskIds().Where((tid)=>{
                    return !manager.IsTaskArchived(tid);
                }).ToList();
            }else{
                return new List<int>();
            }
        }

        public static List<int> GetArchivedTaskIds(int managerId){
            TaskManager manager = GetManager(managerId);
            if(manager == null){
                return new List<int>();
            }
            return manager.GetTaskIds().Where((tid)=>{
                    return manager.IsTaskArchived(tid);
                }).ToList();
        }

    }



    public struct TaskManagerDebugInfo{

        public string name;
        public int referenceId;
    }

    public struct TaskDebugInfo{

        public TaskStatus status;
    }

    public struct TaskDependencyDebugInfo{

        public string name;
        public bool isActive;
    }


}
