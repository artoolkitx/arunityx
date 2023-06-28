using System;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class ARXUnityEventInt : UnityEvent<int> { };

[Serializable]
public class ARXUnityEventUnityObject : UnityEvent<UnityEngine.Object> { };
