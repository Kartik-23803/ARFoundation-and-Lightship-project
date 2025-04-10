using Niantic.Lightship.AR.Semantics;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.InputSystem;

public class SemanticQuerying : MonoBehaviour
{
    public ARCameraManager _cameraMan;
    public ARSemanticSegmentationManager _semanticMan;

    public TMP_Text _text;
    public RawImage _image;
    public Material _material;

    private string _channel = "ground";
    private float _timer = 0.0f;

    void Start()
    {
        if (_text != null)
        {
            _text.text = "Tap to detect surface";
        }
    }

    void OnEnable()
    {
        if (_cameraMan != null)
        {
            _cameraMan.frameReceived += OnCameraFrameUpdate;
        }
    }

    private void OnDisable()
    {
        if (_cameraMan != null)
        {
            _cameraMan.frameReceived -= OnCameraFrameUpdate;
        }
    }

    private void OnCameraFrameUpdate(ARCameraFrameEventArgs args)
    {
        if (!_semanticMan.subsystem.running)
        {
            return;
        }

        Matrix4x4 mat = Matrix4x4.identity;
        var texture = _semanticMan.GetSemanticChannelTexture(_channel, out mat);

        if (texture)
        {
            _image.material = _material;
            _image.material.SetTexture("_SemanticTex", texture);
            _image.material.SetMatrix("_SemanticMat", mat);
        }
    }

    void Update()
    {
        if (!_semanticMan.subsystem.running)
        {
            return;
        }

        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
        {
            Vector2 touchPosition = Touchscreen.current.primaryTouch.position.ReadValue();
            
            if (touchPosition.x > 0 && touchPosition.x < Screen.width &&
                touchPosition.y > 0 && touchPosition.y < Screen.height)
            {
                _timer += Time.deltaTime;
                if (_timer > 0.05f)
                {
                    var list = _semanticMan.GetChannelNamesAt((int)touchPosition.x, (int)touchPosition.y);

                    if (list.Count > 0)
                    {
                        _channel = list[0];
                        _text.text = _channel;
                        Debug.Log($"Surface detected: {_channel}");
                    }
                    else
                    {
                        _text.text = "?";
                        Debug.Log("No surface detected");
                    }

                    _timer = 0.0f;
                }
            }
        }

        #if UNITY_EDITOR
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 mousePosition = Mouse.current.position.ReadValue();
            
            if (mousePosition.x > 0 && mousePosition.x < Screen.width &&
                mousePosition.y > 0 && mousePosition.y < Screen.height)
            {
                var list = _semanticMan.GetChannelNamesAt((int)mousePosition.x, (int)mousePosition.y);

                if (list.Count > 0)
                {
                    _channel = list[0];
                    _text.text = _channel;
                    Debug.Log($"Surface detected: {_channel}");
                }
                else
                {
                    _text.text = "?";
                    Debug.Log("No surface detected");
                }
            }
        }
        #endif
    }
}