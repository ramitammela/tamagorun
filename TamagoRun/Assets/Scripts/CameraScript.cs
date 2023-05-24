using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour
{
    private Camera camera;
    public float zoom;

    [Header("Follow")]
    public Transform target;
    public bool followX;
    public bool followY;

    public float offsetX;
    public float offsetY;

    [Header("Camera Shake")]
    public float shakeDuration;
    public float shakeMagnitude;

    [Header("Hit Color")]
    public Color startColor;
    public Color hitColor;

    private float originalX;
    private float originalY;
    private static CameraScript _instance;


    public static CameraScript Instance
    {
        get
        {
            if (_instance == null)
                Debug.Log("CameraScript is null");

            return _instance;
        }
    }

    private void Awake()
    {
        _instance = this;
    }

    void Start()
    {
        camera = transform.GetComponent<Camera>();
        startColor = camera.backgroundColor;


        originalX = transform.position.x;
        originalY = transform.position.y;

    }

    void Update()
    {
        if (!target)
            return;

        if (followX && !followY)
        {
            transform.position = new Vector3(target.position.x + offsetX, transform.position.y, -10f);
        }
        else if (followY && !followX)
        {
            transform.position = new Vector3(transform.position.x, target.position.y + offsetY, -10f);
        }
        else if (followX & followY)
        {
            transform.position = new Vector3(target.position.x + offsetX, target.position.y + offsetY, -10f);
        }

        camera.orthographicSize = zoom;
    }


    // ->   CameraScript.Instance.StartShake();
    public void StartShake()        
    {
        StartCoroutine(Shake(shakeDuration, shakeMagnitude));       //0.075f, 0.075f
    }

    public IEnumerator Shake(float duration, float magnitude)
    {
        Vector3 orignalPosition = transform.position;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            float x = Random.Range(-0.5f, 0.5f) * magnitude;
            float y = Random.Range(-0.5f, 0.5f) * magnitude;

            transform.position = new Vector3(transform.position.x + x, transform.position.y + y, -10f);
            elapsed += Time.deltaTime;
            yield return 0;
        }

        transform.position = orignalPosition;
    }


    public void StartColorHit()
    {
        StartCoroutine(HitColorLerp(0.08f));
    }
    public IEnumerator HitColorLerp(float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            camera.backgroundColor = Color.Lerp(startColor, hitColor, Mathf.PingPong(Time.time, 1));

            elapsed += Time.deltaTime;
            yield return 0;
        }

        camera.backgroundColor = startColor;
    }

    //  WIP, add lerp's
    public void ResetX()
    {   transform.position = new Vector2(originalX, transform.position.y);   }
    public void ResetY()
    {   transform.position = new Vector2(transform.position.x, originalY);   }
    public void ResetZoom()
    {   StartCoroutine(ZoomLerp(12)); }

    public void ChangeZoom(float z)
    {   StartCoroutine(ZoomLerp(z));    }



    float lerpDuration = 1.5f; 
    IEnumerator ZoomLerp(float zoom123)
    {
        float timeElapsed = 0;
        while (timeElapsed < lerpDuration)
        {
            zoom = Mathf.Lerp(zoom, zoom123, timeElapsed / lerpDuration);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        zoom = zoom123;
    }


}