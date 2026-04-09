using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;          // Player
    public float smoothSpeed = 5f;    // 부드러움 정도
    public Vector3 offset;            // 카메라 위치 보정

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPos = target.position + offset;
        desiredPos.z = transform.position.z; // Z 고정

        transform.position = Vector3.Lerp(
            transform.position,
            desiredPos,
            smoothSpeed * Time.deltaTime
        );
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
