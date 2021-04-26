/// UI for editing a Detector's mask

using UnityEngine;
using OpenCvSharp;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RawImage))]
[RequireComponent(typeof(AspectRatioFitter))]
public class MaskCanvas : MonoBehaviour, IPointerDownHandler
{
    public SharedWebcam webcam;
    public Detector detector;
    public float brushRadius = 30;

    public RectTransform brushCursor;

    RectTransform rectTransform;
    RawImage rawImage;
    Vector3[] corners;
    const int CV2_FILL_SHAPE = -1;
    Vector2 mousePos;
    AspectRatioFitter aspectRatioFitter;
    bool pointerDownInside = false;

    public Color foregroundColor = Color.white;
    public Color backgroundColor = Color.black;

    // Start is called before the first frame update
    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        rawImage = GetComponent<RawImage>();
        aspectRatioFitter = GetComponent<AspectRatioFitter>();

        webcam.onCameraChange.AddListener(onCameraChange);
        detector.onMaskChange.AddListener(OnMaskChange);
        if (webcam.ready) onCameraChange();
        if (detector.detectorCore.srcMask != null) OnMaskChange();
    }

    void onCameraChange() {
        aspectRatioFitter.aspectRatio = webcam.ratio;
        if (webcam.width != detector.detectorCore.srcMask.Width ||
            webcam.height != detector.detectorCore.srcMask.Height) {
            detector.detectorCore.setMask(new Mat(webcam.height, webcam.width, MatType.CV_8UC3));
            Clear();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (detector.detectorCore.srcMask == null) return;

        var mousePos = GetLocalMousePos();
        
        brushCursor.position = Input.mousePosition;
        brushCursor.sizeDelta = new Vector2(
            brushRadius/detector.detectorCore.srcMask.Width,
            brushRadius/detector.detectorCore.srcMask.Height);
        brushCursor.sizeDelta *= rectTransform.rect.size * 2;

        brushRadius = Mathf.Max(1, brushRadius + Input.mouseScrollDelta.y);

        if (pointerDownInside && !Input.GetMouseButton(0)) pointerDownInside = false;

        if (pointerDownInside) {
            Cv2.Circle(
                detector.detectorCore.srcMask,
                (int)(mousePos.x * detector.detectorCore.srcMask.Width),
                (int)(mousePos.y * detector.detectorCore.srcMask.Height),
                (int)brushRadius,
                new Scalar(foregroundColor.r * 255, foregroundColor.g * 255, foregroundColor.b * 255),
                CV2_FILL_SHAPE
            );
            detector.UpdateMask();
        }
    }

    public void OnPointerDown(PointerEventData evt) {
        pointerDownInside = true;
    }

    public void OnMaskChange() {
        Debug.Log("OnMaskChange");
        rawImage.texture = OpenCvSharp.Unity.MatToTexture(detector.detectorCore.srcMask);
    }

    public void SwapColors() {
        Color tmp = backgroundColor;
        backgroundColor = foregroundColor;
        foregroundColor = tmp;
    }
    public void Clear() {
        detector.detectorCore.srcMask.SetTo(new Scalar(
            backgroundColor.r * 255,
            backgroundColor.g * 255,
            backgroundColor.b * 255));
        detector.UpdateMask();
    }

    Vector2 GetLocalMousePos() {
        var localPosition = (Vector2)transform.InverseTransformPoint(Input.mousePosition);
        localPosition += rectTransform.rect.size / 2f;
        localPosition /= rectTransform.rect.size;
        localPosition.y = 1-localPosition.y;
        return localPosition;
    }
}
