using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMgr : MonoBehaviour
{

    public static CameraMgr instance;

    public Camera cam;
    public bool controlled = false;

    private Vector3 focusPoint;
    private float zoom;
    private Vector2 rot;

    public Vector2 initRot;
    public float minZoom;
    public float maxZoom;
    public float initZoom;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(this);
        }
        else
        {
            instance = this;
        }
    }

        // Start is called before the first frame update
    void Start()
    {
        zoom = initZoom;
        rot = initRot;
        focusPoint = 80 * Vector3.up;
    }

    // Update is called once per frame
    void Update()
    {
        if (!controlled)
        {
            SetCamera();
        }
    }

    public void PanCamera(Vector3 adjustment)
    {
        Vector3 newAdj = new Vector3(adjustment.x * Mathf.Cos(Mathf.Deg2Rad * -rot.y) - adjustment.z * Mathf.Sin(Mathf.Deg2Rad * -rot.y), adjustment.y, adjustment.x * Mathf.Sin(Mathf.Deg2Rad * -rot.y) + adjustment.z * Mathf.Cos(Mathf.Deg2Rad * -rot.y));
        focusPoint += newAdj * Mathf.Sqrt(zoom);
    }

    public void AdjustZoom(float adjustment)
    {
        zoom = Mathf.Clamp(zoom + adjustment, minZoom, maxZoom);
    }

    public void AdjustRotation(Vector2 adjustment)
    {
        rot.x = Mathf.Clamp(rot.x + adjustment.x, 1, 90);
        rot.y = rot.y - adjustment.y;
        rot.y -= rot.y >= 360 ? 360 : 0;
        rot.y += rot.y < 0 ? 360 : 0;
    }

    void SetCamera()
    {
        Vector3 newPos = focusPoint;
        newPos.x += zoom * Mathf.Cos(Mathf.Deg2Rad * rot.x) * Mathf.Sin(Mathf.Deg2Rad * -rot.y);
        newPos.y += zoom * Mathf.Sin(Mathf.Deg2Rad * rot.x);
        newPos.z += -zoom * Mathf.Cos(Mathf.Deg2Rad * rot.x) * Mathf.Cos(Mathf.Deg2Rad * -rot.y);

        cam.transform.position = newPos;
        cam.transform.eulerAngles = rot;
        cam.orthographicSize = zoom;
    }
}
