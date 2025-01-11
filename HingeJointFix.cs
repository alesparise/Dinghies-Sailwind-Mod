using UnityEngine;

/// <summary>
/// from https://discussions.unity.com/t/hingejoint-stops-working-properly-after-disable-enable/58645
/// Fixes the HingeJoints going crazy when enabled and disabled
/// </summary>
public class HingeJointFix : MonoBehaviour
{
    private Quaternion initialLocalRotation;
    private Vector3 initialLocalPosition;

    private Quaternion localRotationOnDisable;
    private Vector3 localPositionOnDisable;

    private bool hasDisabled;

    void Awake()
    {
        this.initialLocalRotation = this.transform.localRotation;
        this.initialLocalPosition = this.transform.localPosition;
    }

    void OnDisable()
    {
        this.localRotationOnDisable = this.transform.localRotation;
        this.transform.localRotation = this.initialLocalRotation;

        this.localPositionOnDisable = this.transform.localPosition;
        this.transform.localPosition = this.initialLocalPosition;

        this.hasDisabled = true;
    }

    void Update()
    {
        if (this.hasDisabled)
        {
            this.hasDisabled = false;
            this.transform.localRotation = this.localRotationOnDisable;
            this.transform.localPosition = this.localPositionOnDisable;
        }
    }
}