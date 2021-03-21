// Detects the horizontal position of a bright(er) object in a webcam input

using UnityEngine;
using UnityEngine.UI;
using OpenCvSharp;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;
public class Detector : MonoBehaviour
{
    public Texture2D mask;
    public ViewOption view;

    WebCamTexture capture; // The webcam capture
    RawImage image; // target image for cvImage

    Mat cvImage; // openCV image, where we'll write the camera before each detector-frame, then pass to the detector
    Mat debugImg; // debug Image, for viewing whats going on inside the detector
    Mat maskMat;
    Mat pixelCounts; // unmasked pixel count for each pixel column in the captured image

    Thread thread;
    bool threadWorking; // This is set to true when the detector starts working on a frame and false once it's ready to start a new one
    bool cameraHasNewFrame;

    public byte max = 0;
    public int maxIndex = 0;

    // profiling
    float framerate = 0;

    // Start is called before the first frame update
    void Start()
    {
        image = GetComponent<RawImage>();
        capture = new WebCamTexture(WebCamTexture.devices[0].name);
        capture.Play();

        debugImg = new Mat(1, capture.width, MatType.CV_8UC3);
        cvImage = OpenCvSharp.Unity.TextureToMat(capture);

        thread = new Thread(detectorLoop);
        thread.Start();
    }

    // Update is called once per frame
    void Update()
    {
        if (shouldUpdateMask()) updateMask();
        if (capture.didUpdateThisFrame) cameraHasNewFrame = true;
        if (!threadWorking && enabled) {
            updateViewer();

            if (cameraHasNewFrame) {
                // prepare new detector frame (Here we do what can't be done in the thread, eg. accessing unity textures)
                cvImage = OpenCvSharp.Unity.TextureToMat(capture);
                cameraHasNewFrame = false;
                // Start new detector frame
                threadWorking = true;
            }
        }
    }

    bool shouldUpdateMask() {
        if (mask) {
            return maskMat == null || maskMat.Width != capture.width || maskMat.Height != capture.height;
        } else {
            return maskMat != null || pixelCounts != null;
        }
    }

    void updateMask() {
        if (mask == null) {
            maskMat = null;
            pixelCounts = null;
        } else {
            // Load mask into an openCV image
            maskMat = OpenCvSharp.Unity.TextureToMat(mask);
            Cv2.CvtColor(maskMat, maskMat, ColorConversionCodes.BGR2GRAY);

            // Resize mask to fit camera
            if (capture.width != maskMat.Width || capture.height != maskMat.Height) {
                Cv2.Resize(maskMat, maskMat, new Size(capture.width, capture.height));
                UnityEngine.Debug.LogWarningFormat(
                    "The mask shape ({0}, {1}) does not match the camera shape ({2}, {3}). The mask was scaled to fit",
                    mask.width, mask.height, capture.width, capture.height);
            }

            // Count unmasked pixels in each column
            pixelCounts = new Mat(maskMat.Width, maskMat.Height, maskMat.Type());
            Cv2.Reduce(maskMat, pixelCounts, ReduceDimension.Row, ReduceTypes.Sum, MatType.CV_32FC1);
            pixelCounts = pixelCounts/255;
        }
    }

    void updateViewer() {
        switch (view)
        {
            case ViewOption.CAPTURE:
                image.texture = capture;
                image.rectTransform.sizeDelta = new Vector2(image.texture.width, image.texture.height);
            break;
            case ViewOption.CV_IMAGE:
                image.texture = OpenCvSharp.Unity.MatToTexture(debugImg);
                image.rectTransform.sizeDelta = new Vector2(image.texture.width, Mathf.Max(image.texture.height, 200));
            break;
            default:
            break;
        }
    }

    void OnGUI()
    {
        var style = new GUIStyle();
        style.fontSize = Screen.width/20;
        style.normal.textColor = new Color(255,255,255);
        GUI.TextArea(
            new UnityEngine.Rect(10, 10, 200, 50),
            string.Format("Detector framerate: {0:0.0}", framerate),
            style
        );
    }

    private void detectorLoop() {
        // settings
        int min_peak_width = 10;

        // state
        float prevAverage = 0;
        float prevMax = 0;
        Stopwatch stopWatch = new Stopwatch();

        while (true) {
            // Thread control
            if (!threadWorking) continue;
            if (cvImage == null) continue;
            stopWatch.Restart();

            // Transform input image into a clean 1D array, ready for processing
            Cv2.CvtColor(cvImage, cvImage, ColorConversionCodes.BGR2GRAY);
            if (maskMat != null) {
                Cv2.BitwiseAnd(cvImage, maskMat, cvImage);
                Cv2.Reduce(cvImage, cvImage, ReduceDimension.Row, ReduceTypes.Sum, MatType.CV_32FC1);
                Cv2.Divide(cvImage, pixelCounts, cvImage);
            } else {
                Cv2.Reduce(cvImage, cvImage, ReduceDimension.Row, ReduceTypes.Avg, MatType.CV_32FC1);
            }
            cvImage.ConvertTo(cvImage, MatType.CV_8UC1);

            var peaks = new List<(int, float)>();
            float valuesSum = 0;
            float max = 0;
            int? peak_start = null;
            float peak_max = 0;

            var indexer = new Mat.Indexer<byte>(cvImage);
            int adaptativeThreshold = (int)((prevMax + prevAverage) / 2);

            for (int x = 0; x < cvImage.Width; x++)
            {
                // This is costly. Do it as little as possible
                byte val = indexer[0,x];

                // Compute the average and max pixel values. peaks will be defined relative to those baselines
                valuesSum += val;
                if (val > max) max = val;

                if (val > adaptativeThreshold) {
                    // Peak start
                    if (peak_start == null) peak_start = x;
                    // Peak "sustain"
                    if (val > peak_max) peak_max = val;
                } else {
                    // Peak end. Save the peaks that match our criteria
                    if (peak_start != null && x - peak_start > min_peak_width) {
                        peaks.Add((
                            (int)(peak_start+x)/2,
                            peak_max
                        ));
                    }
                    peak_start = null;
                    peak_max = 0;
                }
            }

            // Update the values that will be used as a baseline for next frame
            prevAverage = valuesSum / cvImage.Width;
            prevMax = max;

            // Update debug image
            Cv2.CvtColor(cvImage, debugImg, ColorConversionCodes.GRAY2BGR);
            Cv2.Resize(debugImg, debugImg, new Size(cvImage.Width, debugImg.Height));
            for (int idx=0; idx<peaks.Count; idx++) {
                Cv2.Line(debugImg, peaks[idx].Item1, 0, peaks[idx].Item1, 50, new Scalar(0,peaks[idx].Item2*255,0), 2);
            }

            framerate = (float)(1000/stopWatch.Elapsed.TotalMilliseconds);
            threadWorking = false;
        }
    }

    public enum ViewOption { CAPTURE, CV_IMAGE }
}
