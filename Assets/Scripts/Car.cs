using System;
using System.Numerics;
using UnityEngine;

[Serializable]

public class WheelProperties
{
    public int wheelState = 1;  // 1 = steerable wheel, 0 = free wheel
    [HideInInspector] public float biDirectional = 0; // optional advanced usage
    public Vector3 LocalPosition; // wheel anchor point in the car's local space
    public float turnAngle = 30f; // maximum steering angle for steerable wheels

    [HideInInspector] public float lastSuspensionLength = 0.0f;
    [HideInInspector] public Vector3 localSlipDirection;
    [HideInInspector] public Vector3 worldSlipDirection;
    [HideInInspector] public Vector3 suspensionForceDirection;
    [HideInInspector] public Vector3 wheelworldPosition;
    [HideInInspector] public float wheelCircumference;
    [HideInInspector] public float torque = 0.0f;
    [HideInInspector] public Rigidbody parentRigidbody;
    [HideInInspector] public GameObject wheelObject;
    [HideInInspector] public float hitPointForce;
    [HideInInspector] public Vector3  localVelocity;
}

public class car : MonoBehaviour
{
    [Header("Wheel Setup")]
    public GameObject wheelPrefab;
    public WheelProperties[] wheels;
    public float wheelSize = 0.53f; // Diameter of the wheel
}