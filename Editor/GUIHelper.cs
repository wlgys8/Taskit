using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System;
using System.Linq;

namespace MS.Taskit.Editor{
    internal static class GUIHelper
    {
        public static GUIContent iconRunning{
            get{
                var index = Mathf.FloorToInt((float)(EditorApplication.timeSinceStartup % 12)).ToString("00");
                return EditorGUIUtility.IconContent("d_WaitSpin" + index);
            }
        }

        public static GUIContent iconIdle{
            get{
                return EditorGUIUtility.IconContent("d_winbtn_mac_inact");
            }
        }
        public static GUIContent iconFail{
            get{
                return EditorGUIUtility.IconContent("d_winbtn_mac_close");
            }
        }

        public static GUIContent iconSuccess{
            get{
                return EditorGUIUtility.IconContent("d_winbtn_mac_max");
            }
        }


        public class SplitterStateProxy{
            private static Type _SplitterStateType;
            internal static Type SplitterStateType{
                get{
                    if(_SplitterStateType == null){
                        _SplitterStateType = typeof(EditorWindow).Assembly.GetTypes().First(x => x.FullName == "UnityEditor.SplitterState");
                    }
                    return _SplitterStateType;
                }
            }

            private static ConstructorInfo _SplitterStateTypeCtor;
            internal static ConstructorInfo SplitterStateTypeCtor{
                get{
                    if(_SplitterStateTypeCtor == null){
                        _SplitterStateTypeCtor = SplitterStateType.GetConstructor(flags, null, new Type[] { typeof(float[]), typeof(int[]), typeof(int[]) }, null);
                    }
                    return _SplitterStateTypeCtor;
                }
            }

            private object _raw;

            public SplitterStateProxy(float[] relativeSizes, int[] minSizes, int[] maxSizes){
                _raw =  SplitterStateTypeCtor.Invoke(new object[] { relativeSizes, minSizes, maxSizes });
            }

            internal object raw{
                get{
                    return _raw;
                }
            }

            public int currentActiveSplitter{
                get{
                    return (int)SplitterStateType.GetField("currentActiveSplitter",flags).GetValue(_raw);
                }
            }

            public float[] realSizes{
                get{
                    #if UNITY_2020_1_OR_NEWER
                        return (float[])SplitterStateType.GetField("realSizes",flags).GetValue(_raw);
                    #else
                        var intRealSizes = (int[])SplitterStateType.GetField("realSizes",flags).GetValue(_raw);
                        float[] floatRealSizes = new float[intRealSizes.Length];
                        for(var i = 0; i < intRealSizes.Length;i++){
                            floatRealSizes[i] = intRealSizes[i];
                        }
                        return floatRealSizes;
                    #endif
                }
            }
            public int splitSize{
                get{
                    return (int)SplitterStateType.GetField("splitSize",flags).GetValue(_raw);
                }set{
                    SplitterStateType.GetField("splitSize",flags).SetValue(_raw,value);
                }
            }

        }

        static BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;


        private static Type _SplitterGUILayoutType;
        private static Type SplitterGUILayoutType{
            get{
                if(_SplitterGUILayoutType == null){
                    _SplitterGUILayoutType = typeof(EditorWindow).Assembly.GetTypes().First(x => x.FullName == "UnityEditor.SplitterGUILayout");
                }
                return _SplitterGUILayoutType;
            }
        }

        private static MethodInfo _BeginVerticalSplitMethod = null;

        private static MethodInfo BeginVerticalSplitMethod{
            get{
                if(_BeginVerticalSplitMethod == null){
                    _BeginVerticalSplitMethod = SplitterGUILayoutType.GetMethod("BeginVerticalSplit", flags, null, new Type[] { SplitterStateProxy.SplitterStateType, typeof(GUILayoutOption[]) }, null);
                }
                return _BeginVerticalSplitMethod;
            }
        }

        private static MethodInfo _EndVerticalSplitMethod = null;

        private static MethodInfo EndVerticalSplitMethod{
            get{
                if(_EndVerticalSplitMethod == null){
                    _EndVerticalSplitMethod = SplitterGUILayoutType.GetMethod("EndVerticalSplit", flags, null, Type.EmptyTypes, null);
                }
                return _EndVerticalSplitMethod;
            }
        }

        private static MethodInfo _BeginHorizontalSplitMethod = null;

        private static MethodInfo BeginHorizontalSplitMethod{
            get{
                if(_BeginHorizontalSplitMethod == null){
                    _BeginHorizontalSplitMethod = SplitterGUILayoutType.GetMethod("BeginHorizontalSplit", flags, null, new Type[] { SplitterStateProxy.SplitterStateType, typeof(GUILayoutOption[]) }, null);
                }
                return _BeginHorizontalSplitMethod;
            }
        }

        private static MethodInfo _EndHorizontalSplitMethod = null;

        private static MethodInfo EndHorizontalSplitMethod{
            get{
                if(_EndHorizontalSplitMethod == null){
                    _EndHorizontalSplitMethod = SplitterGUILayoutType.GetMethod("EndHorizontalSplit", flags, null, Type.EmptyTypes, null);
                }
                return _EndHorizontalSplitMethod;
            }
        }



        public static void BeginVerticalSplit(SplitterStateProxy splitterState, params GUILayoutOption[] options)
        {
            BeginVerticalSplitMethod.Invoke(null, new object[] { splitterState.raw, options });
        }

        public static void EndVerticalSplit()
        {
            EndVerticalSplitMethod.Invoke(null, Type.EmptyTypes);
        }
        public static void BeginHorizontalSplit(SplitterStateProxy splitterState, params GUILayoutOption[] options)
        {
            BeginHorizontalSplitMethod.Invoke(null, new object[] { splitterState.raw, options });
        }

        public static void EndHorizontalSplit()
        {
            EndHorizontalSplitMethod.Invoke(null, Type.EmptyTypes);
        }
    }
}
