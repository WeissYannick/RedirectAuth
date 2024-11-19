using System.IO;
using UnityEngine;
using System.Linq;
using HR_Toolkit;
using UnityEngine.Windows.WebCam;

public class WebcamCapture : MonoBehaviour
{
    private VideoCapture _videoCapture;
    
    private int _counter;
    private int _conditionNr = -1;
    private string _logDirectory;

    private void Start()
    {
        var projectPath = Application.dataPath.Replace("/Assets", "");
        _logDirectory = projectPath + "/Logs" 
                                    + $"/Participant_{RedirectionManager.instance.GetParticipantNumber()}"
                                    + "/Videos";
        Directory.CreateDirectory(_logDirectory);
    }

    private void Update()
    {
        if (Input.GetKeyDown("space"))
        {
            StartNewVideo();
        }
    }

    private void OnApplicationQuit()
    {
        FinishRecording();
    }

    public void SetupCamera()
    {
        var devices = WebCamTexture.devices;
        foreach (var device in devices)
        {
            Debug.Log(device.name);            
            
        }

        var cameraResolution = VideoCapture.SupportedResolutions.OrderByDescending((res) => res.width * res.height).First();
;
        Debug.Log(cameraResolution);
        var cameraFramerate = VideoCapture.GetSupportedFrameRatesForResolution(cameraResolution).OrderByDescending((fps) => fps).First();
        Debug.Log(cameraFramerate);

        VideoCapture.CreateAsync(false, videoCapture => 
        {
            if (videoCapture != null)
            {
                _videoCapture = videoCapture;
                Debug.Log("Created VideoCapture Instance!");

                var cameraParameters = new CameraParameters
                {
                    hologramOpacity = 0.0f,
                    frameRate = cameraFramerate,
                    cameraResolutionWidth = cameraResolution.width,
                    cameraResolutionHeight = cameraResolution.height,
                    pixelFormat = CapturePixelFormat.BGRA32
                };

                _videoCapture.StartVideoModeAsync(cameraParameters,
                    VideoCapture.AudioState.None, _ => StartRecording());
            }
            else
            {
                Debug.LogError("Failed to create VideoCapture Instance!");
            }
        });
    }
    public void StartRecording()
    {
        var participantNr = RedirectionManager.instance.GetParticipantNumber();
        var currentConditionNr = RedirectionManager.instance.GetConditionNumber();
        _counter = _conditionNr != currentConditionNr ? 1 : _counter + 1;
        _conditionNr = currentConditionNr;
        var filename = $"Video_{participantNr}_{_conditionNr}_{_counter}.mp4";
        var filepath = Path.Combine(_logDirectory, filename);
        filepath = filepath.Replace("/", @"\");
        
        _videoCapture.StartRecordingAsync(filepath, _ => { });
        Debug.Log("Start Recording!");
    }

    public void StopRecording()
    {
        _videoCapture.StopRecordingAsync(_ => { });
        Debug.Log("Pause Recording!");
    }

    public void StartNewVideo()
    {
        if (_videoCapture == null) return;
        
        if (_videoCapture.IsRecording)
        {
            _videoCapture.StopRecordingAsync(_ => StartRecording());
        }
        else 
        {
            StartRecording();
        }
        
        Debug.Log("Start New Video!");
    }

    public void FinishRecording()
    {
        if (!_videoCapture.IsRecording)
            _videoCapture.StopVideoModeAsync(_ => { });
        else
            _videoCapture.StopRecordingAsync(_ => FinishRecording());
        Debug.Log("Finish Recording!");
    } 
}
