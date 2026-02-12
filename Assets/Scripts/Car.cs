using System;
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
    public float wheelSize = 0.53f; // radius of the wheel
    public float maxTorque = 450f; // maximum engine torque
    public float wheelGrip = 12f; // how strongly it risists sideways slip, higher is grippier
    public float maxGrip = 12f; // maximum grip before the wheel starts to slip, higher is less likely to slip
    public float frictionCoWheel = 0.022f; // rolling friction

    [Header("Suspension")]
    public float suspensionForce = 90f; //spring constant
    public float dampAmount = 2.5f; // damping constant
    public float suspensionForceClamp = 200f; // cap on total suspension force to prevent instability

    [Header("Car Mass")]
    public float massInKg = 200f; // (not strcitly used, but you might incorporate it)

    //These are updated each frame 
    [HideInInspector] public Vector2 input = Vector2.zero; // horizontal = steering input, vertical = gas/brake input
    [HideInInspector] public bool Forwards = false;

    private Rigidbody rb;

    void Start()
    {
        //Grab or add a Rigidbody component to the car
        rb = GetComponent<Rigidbody>();
        if (!rb) rb = gameObject.AddComponent<Rigidbody>();

        //Slight tweak to inertia if dersied
        rb.inertiaTensor = 1.0f * rb.inertiaTensor;
        
        //Create each wheel
        if (wheels != null)
        {
            for (int i = 0; i < wheels.Length; i++)
            {
                WheelProperties w = wheels[i];

                //convert local position to world position for the wheel
                Vector3 parentRelativePosition = transform.InverseTransformPoint(transform.TransformPoint(w.LocalPosition));

                //intiantiate visual wheel
                w.wheelObject = Instantiate(wheelPrefab, transform);
                w.wheelObject.transform.localPosition = w.LocalPosition;
                w.wheelObject.transform.eulerAngles = transform.eulerAngles;
                w.wheelObject.transform.localScale = 2f * new Vector3(wheelSize, wheelSize, wheelSize);

                //calculate the wheel circumference for the rotation logic
                w.wheelCircumference = 2f * Mathf.PI * wheelSize;
                //store reference to the car's rigidbody for later use
                w.parentRigidbody = rb;
            }
        }
    }

    void update()
    {
        input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
    }

    void FixedUpdate()
    {
        if (wheels == null || wheels.Length == 0) return;

        foreach (var wheel in wheels)
        {
            if (!wheel.wheelObject) continue;

            //for easy refference
            Transform wheelObj = wheel.wheelObject.transform;
            Transform wheelVisual = wheelObj.GetChild(0); // assuming the visual model is the first child of the wheel object

            //calculate steer angle if wheelState ==1
            if (wheel.wheelState == 1)
            {
                float targetAngle = wheel.turnAngle * input.x;
                Quaternion targetRotation = Quaternion.Euler(0f, targetAngle, 0f);
                // terp to the new steer angle
                wheelObj.localRotation = Quaternion.Lerp(
                    wheelObj.localRotation,
                    targetRotation,
                    Time.fixedDeltaTime * 5f
                );
            }
            else if (wheel.wheelState == 0 && rb.velocity.magnitude > 0.04f)
            {
                //For free wheels, optionally align them in direction of motion
                RaycastHit tmphit;
                if (Physics.Raycast(transform.TransfromPoint(wheel.LocalPosition),
                                    -transform.up,
                                    out tmphit,
                                    wheelSize *2f))
                {
                    Quaternion aim = Quaternion.LookRotation(rb.GetPointVector(tmphit.point), transform.up); wheelObj.rotation = Quaternion.Lerp(wheelObj.rotation, aim, Time.fixedDeltaTime * 5f);
                    wheelObj.rotation = Quaternion.Lerp(wheelObj.rotation, aim, Time.fixedDeltaTime * 100f);
                }
            }
        }
    }
}