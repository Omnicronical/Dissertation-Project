using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbitCameraScript : MonoBehaviour
{
    public float sensitivity = 10f;
    public Transform target;

    public float rotationSpeed = 30f;
    public float orbitRadius = 5f;

    private float initialY;
    private float orbitProgress = 0f;
    public bool clockwiseOrbit = true;

    // Smooth zooming variables
    public float smoothTime = 0.3f;
    private float zoomVelocity = 0.0f;

    // Smooth orbiting variables
    public float orbitSmoothTime = 0.1f;
    private Vector3 orbitVelocity = Vector3.zero;

    void Update()
    {
        transform.LookAt(target);

        // Smooth zooming
        var zoomInput = Input.GetAxis("Mouse ScrollWheel") * sensitivity;
        float targetZoom = orbitRadius - zoomInput * sensitivity;
        orbitRadius = Mathf.SmoothDamp(orbitRadius, targetZoom, ref zoomVelocity, smoothTime);

        Orbit();
    }

    private void Start()
    {
        initialY = transform.position.y;
    }

    void Orbit()
    {
        // Smooth orbiting
        float targetAngle = orbitProgress + (clockwiseOrbit ? 1f : -1f) * rotationSpeed * Time.deltaTime;
        float smoothedAngle = Mathf.SmoothDampAngle(orbitProgress, targetAngle, ref orbitVelocity.y, orbitSmoothTime);
        orbitProgress = smoothedAngle;

        // Calculate the orbit position
        float orbitX = Mathf.Cos(orbitProgress * Mathf.Deg2Rad) * orbitRadius;
        float orbitZ = Mathf.Sin(orbitProgress * Mathf.Deg2Rad) * orbitRadius;
        Vector3 orbitPosition = new Vector3(orbitX, initialY, orbitZ) + target.position;

        // Move towards the orbit position
        transform.position = orbitPosition;

        // Correct the Y coordinate to the initial value
        Vector3 newPosition = transform.position;
        newPosition.y = initialY;
        transform.position = newPosition;
    }
}
