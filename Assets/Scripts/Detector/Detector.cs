// Detects the horizontal position of a bright(er) object in a webcam input

using UnityEngine;
using OpenCvSharp;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;
using System;

public class Detector : MonoBehaviour
{
    // Inspector settings
    public Texture2D mask;
    public SharedWebcam webcam;

    // output
    [NonSerialized]
    public float position = 0;

    // Textures
    Mat cvImage; // openCV image, where we'll write the camera before each detector-frame, then pass to the detector
    Mat maskMat; // Mask texture (/matrix)
    Mat pixelCounts; // unmasked pixel count for each pixel column in the captured image
    Mat debugImg; // debug Image, for viewing whats going on inside the detector
    [NonSerialized]
    public Texture debugTexture; // In order to access the debug image from other components, it
                                 // needs to be in the 'Texture' format we'll do the conversion
                                 // after the detector is done with a frame and before starting the
                                 // next one

    // Loop & thread management
    bool cameraHasNewFrame;
    Thread thread;
    bool threadWorking; // This is set to true when the detector starts working on a frame and false
                        // once it's ready to start a new one

    // Profiling
    [NonSerialized]
    public float framerate = 0;

    // Start is called before the first frame update
    void Start()
    {
        thread = new Thread(detectorLoop);
        thread.Start();
        webcam.onCameraChange.AddListener(updateInput);
    }

    // Update is called once per frame
    void Update()
    {
        if (shouldUpdateMask()) updateMask();

        if (webcam.didUpdateThisFrame) cameraHasNewFrame = true;
        if (!threadWorking && enabled) {
            if (debugImg != null) debugTexture = OpenCvSharp.Unity.MatToTexture(debugImg);
            if (cameraHasNewFrame) {
                // prepare new detector frame (Here we do what can't be done in the thread, eg. accessing unity textures)
                cvImage = OpenCvSharp.Unity.TextureToMat(webcam.capture).Clone();
                cameraHasNewFrame = false;
                // Start new detector frame
                threadWorking = true;
            }
        }
    }

    bool shouldUpdateMask() {
        if (!webcam.ready) return false;
        if (mask) {
            return maskMat == null || maskMat.Width != webcam.width || maskMat.Height != webcam.height;
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
            if (webcam.width != maskMat.Width || webcam.height != maskMat.Height) {
                Cv2.Resize(maskMat, maskMat, new Size(webcam.width, webcam.height));
                UnityEngine.Debug.LogWarningFormat(
                    "The mask shape ({0}, {1}) does not match the camera shape ({2}, {3}). The mask was scaled to fit",
                    mask.width, mask.height, webcam.width, webcam.height);
            }

            // Count unmasked pixels in each column
            pixelCounts = new Mat(maskMat.Width, maskMat.Height, maskMat.Type());
            Cv2.Reduce(maskMat, pixelCounts, ReduceDimension.Row, ReduceTypes.Sum, MatType.CV_32FC1);
            pixelCounts = pixelCounts/255;
        }
    }

    void updateInput() {
        debugImg = new Mat(1, webcam.capture.width, MatType.CV_8UC3);
        cvImage = OpenCvSharp.Unity.TextureToMat(webcam.capture);
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
            threadWorking = false;
        }
    }
}
