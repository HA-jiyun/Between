using UnityEngine;

public class Billboard : MonoBehaviour
{
    private Transform cam;
    private float baseScale = 0.07f;

    private void Start()
    {
        if(Camera.main != null)
            cam = Camera.main.transform;
    }

    private void LateUpdate()
    {
        if (cam == null) return;

        transform.LookAt(transform.position + cam.rotation * Vector3.forward, cam.rotation * Vector3.up);

        float distance = Vector3.Distance(transform.position, cam.position);
        transform.localScale = baseScale * distance * Vector3.one;
    }
}
