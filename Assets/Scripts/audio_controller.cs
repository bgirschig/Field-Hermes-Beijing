// Controller for audio-sync test scene

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class audio_controller : MonoBehaviour
{
    public GameObject toggleGroup;
    public GameObject togglePrefab;
    public GameObject bpmIndicator;
    public int trackCount = 3;
    public AudioClip audioClip;

    public float bpm = 54;
    private float beat_interval;
    private GameObject beat_indicator;

    // Start is called before the first frame update
    void Start()
    {
        beat_interval = 60 / bpm;
        beat_indicator = bpmIndicator.transform.GetChild(0).gameObject;

        for (int i = 0; i < trackCount; i++)
        {
            var audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 0;
            audioSource.clip = audioClip;
            audioSource.playOnAwake = false;

            var toggleObj = Instantiate(togglePrefab, toggleGroup.transform);
            var toggle = toggleObj.GetComponent<Toggle>();
            toggleObj.GetComponentInChildren<Text>().text = string.Format("track {0}", i+1);
            toggle.onValueChanged.AddListener((bool value) => {
                if (value) {
                    double nextBeat = Math.Ceiling(AudioSettings.dspTime / beat_interval) * beat_interval;
                    audioSource.time = 0;
                    audioSource.PlayScheduled(nextBeat);
                } else {
                    audioSource.Stop();
                }
            });
        }
    }

    void Update() {
        double positionInBeat = (AudioSettings.dspTime % beat_interval) / beat_interval;
        double distance_to_beat = Math.Min(positionInBeat, 1-positionInBeat);
        bool is_beat = distance_to_beat < 0.05;

        bpmIndicator.transform.localRotation = Quaternion.Euler(0,0,-(float)positionInBeat*360);
        if (is_beat) {
            // Debug.Log("beat");
            beat_indicator.SetActive(true);
            // bpmIndicatorHandle.sizeDelta = new Vector2(10, 50);
        } else {
            beat_indicator.SetActive(false);
            // bpmIndicatorHandle.sizeDelta = new Vector2(1.6f, 50);
            // bpmIndicator.transform.localScale = new Vector3(1, 1, 1);
        }
    }
}
