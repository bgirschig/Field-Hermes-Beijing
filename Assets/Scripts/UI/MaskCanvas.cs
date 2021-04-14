/// UI for editing a Detector's mask

using UnityEngine;
using OpenCvSharp;
using UnityEngine.UI;
using System.IO;

[RequireComponent(typeof(RawImage))]
[RequireComponent(typeof(AspectRatioFitter))]
public class MaskCanvas : MonoBehaviour
{
    public SharedWebcam webcam;
    public Detector detector;
    public float brushRadius = 30;

    public RectTransform brushCursor;

    RectTransform rectTransform;
    Mat maskMat;
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
        
        // This doesn't work properly yet
        // LoadMask();

        webcam.onCameraChange.AddListener(onCameraChange);
    }

    void onCameraChange() {
        maskMat = new Mat(webcam.height, webcam.width, MatType.CV_8UC1, new Scalar(0,0,0));
        UpdateTexture();
        aspectRatioFitter.aspectRatio = webcam.ratio;
    }

    // Update is called once per frame
    void Update()
    {
        if (maskMat == null) return;

        var mousePos = GetLocalMousePos();
        
        brushCursor.position = Input.mousePosition;
        brushCursor.sizeDelta = new Vector2(brushRadius/maskMat.Width, brushRadius/maskMat.Height);
        brushCursor.sizeDelta *= rectTransform.rect.size * 2;

        brushRadius = Mathf.Max(1, brushRadius + Input.mouseScrollDelta.y);

        if (Input.GetMouseButton(0)) {
            Cv2.Circle(
                maskMat,
                (int)(mousePos.x * maskMat.Width),
                (int)(mousePos.y * maskMat.Height),
                (int)brushRadius,
                new Scalar(foregroundColor.r * 255, foregroundColor.g * 255, foregroundColor.b * 255),
                CV2_FILL_SHAPE
            );

            UpdateTexture();
        }
    }

    public void SwapColors() {
        Color tmp = backgroundColor;
        backgroundColor = foregroundColor;
        foregroundColor = tmp;
    }
    public void Clear() {
        maskMat.SetTo(new Scalar(
            backgroundColor.r * 255,
            backgroundColor.g * 255,
            backgroundColor.b * 255));
        UpdateTexture();
    }

    void UpdateTexture() {
        Texture2D texture = OpenCvSharp.Unity.MatToTexture(maskMat);
        rawImage.texture = texture;
        detector.mask = texture;
        detector.maskNeedsUpdate = true;
    }

    public void SaveMask() {
        Texture2D texture = OpenCvSharp.Unity.MatToTexture(maskMat);
        var pngBytes = texture.EncodeToPNG();
        string destination = Path.Combine(Application.persistentDataPath, "mask.png");
        File.WriteAllBytes(destination, pngBytes);
    }

    void LoadMask() {
        string maskPath = Path.Combine(Application.persistentDataPath, "mask.png");
        if (File.Exists(maskPath)) {
            print($"loading {maskPath}");
            var pngBytes = File.ReadAllBytes(maskPath);
            Texture2D texture = new Texture2D(2,2);
            texture.LoadImage(pngBytes);
            maskMat = OpenCvSharp.Unity.TextureToMat(texture);
            aspectRatioFitter.aspectRatio = (float)maskMat.Width / maskMat.Height;
            UpdateTexture();
        }
    }

    Vector2 GetLocalMousePos() {
        var localPosition = (Vector2)transform.InverseTransformPoint(Input.mousePosition);
        localPosition += rectTransform.rect.size / 2f;
        localPosition /= rectTransform.rect.size;
        localPosition.y = 1-localPosition.y;
        return localPosition;
    }
}
