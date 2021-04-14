/// UI for editing a Detector's mask

using UnityEngine;
using OpenCvSharp;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage))]
[RequireComponent(typeof(AspectRatioFitter))]
public class MaskCanvas : MonoBehaviour
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

    public Color foregroundColor = Color.white;
    public Color backgroundColor = Color.black;

    // Start is called before the first frame update
    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        rawImage = GetComponent<RawImage>();
        aspectRatioFitter = GetComponent<AspectRatioFitter>();

        webcam.onCameraChange.AddListener(onCameraChange);
        detector.onMaskChange.AddListener(updateMask);
    }

    void onCameraChange() {
        aspectRatioFitter.aspectRatio = webcam.ratio;
    }

    // Update is called once per frame
    void Update()
    {
        if (detector.originalMaskMat == null) return;

        var mousePos = GetLocalMousePos();
        
        brushCursor.position = Input.mousePosition;
        brushCursor.sizeDelta = new Vector2(
            brushRadius/detector.originalMaskMat.Width,
            brushRadius/detector.originalMaskMat.Height);
        brushCursor.sizeDelta *= rectTransform.rect.size * 2;

        brushRadius = Mathf.Max(1, brushRadius + Input.mouseScrollDelta.y);

        if (Input.GetMouseButton(0)) {
            Cv2.Circle(
                detector.originalMaskMat,
                (int)(mousePos.x * detector.originalMaskMat.Width),
                (int)(mousePos.y * detector.originalMaskMat.Height),
                (int)brushRadius,
                new Scalar(foregroundColor.r * 255, foregroundColor.g * 255, foregroundColor.b * 255),
                CV2_FILL_SHAPE
            );
            detector.maskNeedsUpdate = true;
        }
    }

    public void updateMask() {
        rawImage.texture = OpenCvSharp.Unity.MatToTexture(detector.originalMaskMat);
    }

    public void SwapColors() {
        Color tmp = backgroundColor;
        backgroundColor = foregroundColor;
        foregroundColor = tmp;
    }
    public void Clear() {
        detector.originalMaskMat.SetTo(new Scalar(
            backgroundColor.r * 255,
            backgroundColor.g * 255,
            backgroundColor.b * 255));
        detector.maskNeedsUpdate = true;
    }

    Vector2 GetLocalMousePos() {
        var localPosition = (Vector2)transform.InverseTransformPoint(Input.mousePosition);
        localPosition += rectTransform.rect.size / 2f;
        localPosition /= rectTransform.rect.size;
        localPosition.y = 1-localPosition.y;
        return localPosition;
    }
}
