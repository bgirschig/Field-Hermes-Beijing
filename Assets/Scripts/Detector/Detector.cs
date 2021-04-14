// Detects the horizontal position of a bright(er) object in a webcam input

using UnityEngine;
using OpenCvSharp;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine.Events;
using System.Diagnostics;

public class Detector : MonoBehaviour
{
    [NonSerialized]
    public DetectorCore detectorCore;

    // Inspector settings
    public SharedWebcam webcam;

    // output
    [NonSerialized]
    public float position = 0;
    [NonSerialized]
    public float speed = 0;
    [NonSerialized]
    public UnityEvent onMaskChange = new UnityEvent();

    // Textures
    [NonSerialized]
    public Texture debugTexture; // In order to access the debug image from other components, it
                                 // needs to be in the 'Texture' format we'll do the conversion
                                 // after the detector is done with a frame and before starting the
                                 // next one

    float prevPosition = 0;
    float prevDetectorTime = 0;


    // Start is called before the first frame update
    void Start()
    {
        detectorCore = new DetectorCore();

        webcam.onCameraChange.AddListener(OnCameraChange);
        if (webcam.ready) OnCameraChange();

        LoadMaskFromDisk();
    }

    // Update is called once per frame
    void Update()
    {
        if (!enabled) return;
        
        if (detectorCore.debugImg != null) {
            debugTexture = OpenCvSharp.Unity.MatToTexture(detectorCore.debugImg);
        }
        if (webcam.didUpdateThisFrame) {
            float time = Time.time;
            position = detectorCore.detect(OpenCvSharp.Unity.TextureToMat(webcam.capture));

            speed = (position - prevPosition) / (time - prevDetectorTime);

            prevDetectorTime = time;
            prevPosition = position;
        }
    }

    void OnCameraChange() {
        detectorCore.SetSize(webcam.capture.width, webcam.capture.height);
        onMaskChange.Invoke();
    }

    public void UpdateMask() {
        detectorCore.UpdateMask();
        onMaskChange.Invoke();
    }

    public void SaveMask() {
        Texture2D texture = OpenCvSharp.Unity.MatToTexture(detectorCore.srcMask);
        var pngBytes = texture.EncodeToPNG();
        string destination = Path.Combine(Application.persistentDataPath, "mask.png");
        File.WriteAllBytes(destination, pngBytes);
    }

    void LoadMaskFromDisk() {
        string maskPath = Path.Combine(Application.persistentDataPath, "mask.png");
        if (File.Exists(maskPath)) {            
            var pngBytes = File.ReadAllBytes(maskPath);
            var loaderTexture = new Texture2D(2,2);
            loaderTexture.LoadImage(pngBytes);
            
            detectorCore.setMask(OpenCvSharp.Unity.TextureToMat(loaderTexture));
            onMaskChange.Invoke();
        }
    }
}

public class DetectorCore {
    // settings
    public int min_peak_width = 10;

    // state
    float prevAverage = 0;
    float prevMax = 0;
    float position = 0;
    public float framerate;
    // 
    int width = 0;
    int height = 0;

    public Mat srcMask;
    public Mat debugImg;
    Mat maskMat;
    Mat pixelCounts;

    Stopwatch stopWatch = new Stopwatch();

    public void setMask(Mat srcMask) {
        this.srcMask = srcMask;
        UpdateMask();
    }

    public void UpdateMask() {
        if (srcMask == null) {
            maskMat = null;
            pixelCounts = null;
            return;
        }
        if (width == 0 || height == 0) {
            return;
        }
        
        maskMat = srcMask.Clone();

        UnityEngine.Debug.Assert(maskMat.Width>0 && maskMat.Height>0, "invalid mask size: 0");
        UnityEngine.Debug.Assert(width>0 && height>0, "invalid target mask size: 0");

        Cv2.CvtColor(maskMat, maskMat, ColorConversionCodes.BGR2GRAY);

        // Resize mask to fit camera
        if (width != maskMat.Width || height != maskMat.Height) {
            UnityEngine.Debug.LogWarning(
                $"The mask shape ({maskMat.Width}, {maskMat.Height}) does not match the camera " +
                $"shape ({width}, {height}). The mask was scaled to fit");
            Cv2.Resize(maskMat, maskMat, new Size(width, height));
        }

        // Count unmasked pixels in each column
        pixelCounts = new Mat(maskMat.Width, maskMat.Height, maskMat.Type());
        Cv2.Reduce(maskMat, pixelCounts, ReduceDimension.Row, ReduceTypes.Sum, MatType.CV_32FC1);
        pixelCounts = pixelCounts/255;
    }

    public void SetSize(int width, int height) {
        this.width = width;
        this.height = height;
        debugImg = new Mat(1, width, MatType.CV_8UC3);
        UpdateMask();
    }

    public float detect(Mat cvImage) {
        if (cvImage == null) return position;
        if (width == 0 || height == 0) return position;

        stopWatch.Restart();

        // Transform input image into a clean 1D array, ready for processing
        int channels = cvImage.Channels();
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

        int? best_peak_idx = null;
        float best_peak_brightness = 0;

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
                    if (peak_max > best_peak_brightness) {
                        best_peak_brightness = peak_max;
                        best_peak_idx = peaks.Count - 1;
                    }
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

        // Update the final detector value
        if (best_peak_idx != null) {
            position = (float)peaks[(int)best_peak_idx].Item1 / cvImage.Width;
        }

        framerate = (float)(1000/stopWatch.Elapsed.TotalMilliseconds);
        return position;
    }
}