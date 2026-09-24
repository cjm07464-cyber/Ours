using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;          // Player
    public float smoothSpeed = 5f;    // 부드러움 정도
    public Vector3 offset;            // 카메라 위치 보정
    public bool instantFollow = true;
    public bool followX = true;
    public bool followY = true;
    public float catchUpThreshold = 0.05f;

    private bool switchToInstantWhenClose;
    private float fixedX;
    private float fixedY;

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPos = GetDesiredPosition();

        if (instantFollow)
        {
            transform.position = desiredPos;
            return;
        }

        transform.position = Vector3.Lerp(
            transform.position,
            desiredPos,
            smoothSpeed * Time.deltaTime
        );

        if (switchToInstantWhenClose && IsCloseEnoughOnXY(desiredPos))
        {
            instantFollow = true;
            switchToInstantWhenClose = false;
            transform.position = desiredPos;
        }
    }

    public void SnapToTarget()
    {
        if (target == null) return;

        Vector3 targetPosition = GetDesiredPosition();
        targetPosition.z = transform.position.z;
        transform.position = targetPosition;
    }

    public void StartSmoothCatchUp(Transform followTarget)
    {
        target = followTarget;
        instantFollow = false;
        switchToInstantWhenClose = true;
    }

    public void SetFixedPosition(float x, float y)
    {
        fixedX = x;
        fixedY = y;
    }

    private Vector3 GetDesiredPosition()
    {
        Vector3 targetPosition = target.position + offset;
        Vector3 desiredPos = transform.position;
        desiredPos.x = followX ? targetPosition.x : fixedX;
        desiredPos.y = followY ? targetPosition.y : fixedY;
        desiredPos.z = transform.position.z; // Z 고정
        return desiredPos;
    }

    private bool IsCloseEnoughOnXY(Vector3 desiredPos)
    {
        float distance = 0f;

        if (followX)
        {
            distance += Mathf.Pow(transform.position.x - desiredPos.x, 2f);
        }

        if (followY)
        {
            distance += Mathf.Pow(transform.position.y - desiredPos.y, 2f);
        }

        return Mathf.Sqrt(distance) <= Mathf.Max(0f, catchUpThreshold);
    }

    /*  void LateUpdate()
      {
          if (target == null) return;

          transform.position = new Vector3(
              target.position.x,
              target.position.y,
              transform.position.z
          );
      }*/
}
