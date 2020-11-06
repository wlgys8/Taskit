using System.Collections.Generic;
using System.Linq;

namespace MS.Taskit{

    /// <summary>
    /// 使用TaskStatusDatabase统一存储和管理任务的状态信息
    /// </summary>
    public interface ITaskStatusDatabase
    {

        TaskStatus GetStatus(int taskId);

        void SetStatus(int taskId,TaskStatus status);

        int[] GetTaskIds();
    }



    public class MemoryTaskStatusDB : ITaskStatusDatabase
    {

        private Dictionary<int,TaskStatus> _statusDict = new Dictionary<int, TaskStatus>();

        public TaskStatus GetStatus(int taskId)
        {
            TaskStatus s = TaskStatus.Idle;
            if(_statusDict.TryGetValue(taskId,out s)){
                return s;
            }
            return TaskStatus.Idle;
        }

        public int[] GetTaskIds()
        {
            return _statusDict.Keys.ToArray();
        }

        public void SetStatus(int taskId, TaskStatus status)
        {
            if(_statusDict.ContainsKey(taskId)){
                _statusDict[taskId] = status;
            }else{
                _statusDict.Add(taskId,status);
            }
        }
    }
}
