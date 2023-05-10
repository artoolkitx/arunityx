using System;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class ARUnityEventInt : UnityEvent<int> { };

[Serializable]
public class ARUnityEventUnityObject : UnityEvent<UnityEngine.Object> { };
