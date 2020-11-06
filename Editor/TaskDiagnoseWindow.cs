using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEditor;

namespace MS.Taskit.Diagnose.Editor{
    using MS.Taskit.Editor;

    internal class TaskDiganoseWindow : UnityEditor.EditorWindow
    {
        [MenuItem("Window/Taskit/DiganoseWindow")]
        private static void Open(){
            EditorWindow.GetWindow<TaskDiganoseWindow>().Show();
        }

        private TaskManagerDebugInfo[] managers;
        private int _currentManagerId = -1;

        void OnEnable(){
            this.Refresh();
            TaskDiagnose.onTaskDependenciesUpdate += OnTaskDependenciesUpdate;
            TaskDiagnose.onManagerRegistered += Refresh;
        }

        void OnDisable(){
            TaskDiagnose.onTaskDependenciesUpdate -= OnTaskDependenciesUpdate;
            TaskDiagnose.onManagerRegistered -= Refresh;
        }

        private void OnTaskDependenciesUpdate(Task task){
            this.Repaint();
        }

        private const float DependencyItemHeight = 20;

        private float GetTaskItemHeight(int managerId,int taskId){
            var dependencies = TaskDiagnose.GetDependencyDebugInfos(managerId,taskId);
            var height = EditorGUI.GetPropertyHeight(SerializedPropertyType.String,GUIContent.none);
            return Mathf.Max(height,dependencies.Count * DependencyItemHeight);
        }

        private void DrawTaskStatus(int managerId,int taskId,Rect rect){
            var taskDebugInfo = TaskDiagnose.GetTaskDebugInfo(managerId,taskId);
            GUIContent icon = GUIContent.none;
            switch(taskDebugInfo.status){
                case TaskStatus.Idle:
                icon = GUIHelper.iconIdle;
                break;
                case TaskStatus.Pending:
                icon = GUIHelper.iconRunning;
                break;
                case TaskStatus.Active:
                icon = GUIHelper.iconSuccess;
                break;
                case TaskStatus.Completed:
                case TaskStatus.Archived:
                icon = GUIHelper.iconFail;
                break;
            }
            GUI.Label(rect,icon);
        }
        private void DrawTask(int managerId,int taskId){
            var height = GetTaskItemHeight(managerId,taskId);
            var rect = GUILayoutUtility.GetRect(this.position.width,height);
            var x = rect.x;
            var y = rect.y;

            var splitedSize = headerSplitState.realSizes;

            var h = EditorGUI.GetPropertyHeight(SerializedPropertyType.String,new GUIContent(taskId.ToString()));

            EditorGUI.LabelField(new Rect(x,y,splitedSize[0],h),taskId.ToString());
            x += splitedSize[0];
            //draw status

            h = EditorGUI.GetPropertyHeight(SerializedPropertyType.String,GUIContent.none);
            DrawTaskStatus(managerId,taskId,new Rect(x,y,splitedSize[1],h));
            x += splitedSize[1];
            //draw dependencies
            
            var dependencies = TaskDiagnose.GetDependencyDebugInfos(managerId,taskId);
            foreach(var d in dependencies){
                GUIContent icon = GUIContent.none;
                if(d.isActive){
                    icon = GUIHelper.iconSuccess;
                }else{
                    icon = GUIHelper.iconIdle;
                }
                GUI.Label(new Rect(x,y,20,20),icon);
                EditorGUI.LabelField(new Rect(x + 20,y,splitedSize[2] - 20,DependencyItemHeight),d.name);
                y += DependencyItemHeight;
            }
        }

        private void Refresh(){
            managers = TaskDiagnose.GetTaskManagers().ToArray();
            if(managers.Length == 0){
                _currentManagerId = -1;
                return;
            }
            var managerId = managers[0].referenceId;
            _currentManagerId = managerId;
        }

        private GUIHelper.SplitterStateProxy _headerSplitState;
        private GUIHelper.SplitterStateProxy headerSplitState{
            get{
                if(_headerSplitState == null){
                    _headerSplitState = new GUIHelper.SplitterStateProxy(new float[]{20,20,60},new int[]{100,100,100},null);
                }
                return _headerSplitState;
            }
        }


        private void OnGUI() {
            GUIHelper.BeginHorizontalSplit(headerSplitState);
            GUILayout.Label("TaskId");
            GUILayout.Label("Status");
            GUILayout.Label("Dependencies");
            GUIHelper.EndHorizontalSplit();

            var taskIds = TaskDiagnose.GetUnarchivedTaskIds(_currentManagerId);
            foreach(var taskId in taskIds){
                DrawTask(_currentManagerId, taskId);
            }

            DrawArchivedTaskIds();
        }

        private bool _showArchivedTasks = true;
        private Vector2 _progressOfArchivedTask;

        private void DrawArchivedTaskIds(){
            _showArchivedTasks = EditorGUILayout.Foldout(_showArchivedTasks,"Archived Tasks");
            if(_showArchivedTasks){

                var archivedTaskIds = TaskDiagnose.GetArchivedTaskIds(_currentManagerId);
                var width = this.position.width;
                var itemWidth = 50f;
                var lineHeight = 30f;
                var countPerLine = Mathf.FloorToInt(width / itemWidth);
                var lineCount = Mathf.CeilToInt(archivedTaskIds.Count * 1f / countPerLine);
                var height = lineCount * lineHeight;

                _progressOfArchivedTask = EditorGUILayout.BeginScrollView(_progressOfArchivedTask);
                var rect = GUILayoutUtility.GetRect(width,height);

                for(var i = 0; i < archivedTaskIds.Count; i ++){
                    var tid = archivedTaskIds[archivedTaskIds.Count - 1 - i];
                    var line = Mathf.FloorToInt(i / countPerLine);
                    var offset = i % countPerLine;
                    var r = new Rect(rect.x + offset * itemWidth, rect.y + line * lineHeight,itemWidth,lineHeight);
                    GUI.Label(r,tid.ToString());
                }

                EditorGUILayout.EndScrollView();
            }
        }
    }
}
