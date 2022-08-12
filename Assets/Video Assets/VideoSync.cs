using UnityEngine;
using UnityEngine.Video;

public class VideoSync : MonoBehaviour {

    private VideoPlayer video;
// Use this for initialization
    void Start () {
        video = GetComponent<UnityEngine.Video.VideoPlayer>();
    }

// Update is called once per frame
    void Update()
    {
        if (video.isActiveAndEnabled)
        {
            video.Pause();
            video.StepForward();
        }
    }
}