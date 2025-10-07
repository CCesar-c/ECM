using UnityEngine;
using UnityEngine.EventSystems;
#if UNITY_ANDROID
public class CameraTouchLook : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    public float sensitivity = 2f;

    private bool isDragging = false;
    private Vector2 lastPos;
    private float xRotation = 0f;

    public void OnPointerDown(PointerEventData eventData)
    {
        isDragging = true;
        lastPos = eventData.position;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isDragging = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging) return;

        Vector2 delta = eventData.position - lastPos;
        lastPos = eventData.position;

        // rotación horizontal del cuerpo (Y)
        moviem.instance.transform.Rotate(Vector3.up * delta.x * sensitivity * Time.deltaTime);

        // rotación vertical de la cámara (X)
        xRotation -= delta.y * sensitivity * Time.deltaTime;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        moviem.instance.playerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }
}
#endif