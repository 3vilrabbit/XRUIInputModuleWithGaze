using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.UI;

public class XRUIInputModuleWithGaze : XRUIInputModule
{
    #region Inspector
    [Header("Gaze")]
    [SerializeField] private bool useGaze = false;
    [SerializeField] private float gazeTime = 3f;
    [SerializeField, Tooltip("Time after repeating the input, 0 to disable repeating.")] private float repeatGazeTime = 0f;
    #endregion
    
    #region Events
    public static event Action<float, bool> OnGazeUpdate;
    #endregion
    
    #region Properties
    
    public bool UseGaze { set => useGaze = value; }
    
    #endregion
    
    #region Fields
    private PointerEventData _lookData;
    private GameObject _currentLook;
    private GameObject _currentPressed;
    private GameObject _lastActiveButton;
    private float _currentStartTime;
    #endregion

    #region Unity Methods
    public override void Process()
    {
        base.Process();
        if (useGaze) { HandleGaze(); }
    }
    #endregion
    
    #region Methods
    private void HandleGaze()
    {
        SendUpdateEventToSelectedObject();
        
        PointerEventData lookData = GetLookPointerEventData();
        _currentLook = lookData.pointerCurrentRaycast.gameObject;
        if (_currentLook == null)
        {
            ClearSelection();
            UpdateGaze(0, false);
        }
        
        HandlePointerExitAndEnter(lookData, _currentLook);

        if ((_currentLook != null))
        {
            bool clickable = false;
            if (_currentLook.transform.gameObject.GetComponent<Button>() != null) clickable = _currentLook.transform.gameObject.GetComponent<Button>().interactable;
            if (_currentLook.transform.parent != null) {
                if (_currentLook.transform.parent.gameObject.GetComponent<Button>() != null) clickable = _currentLook.transform.parent.gameObject.GetComponent<Button>().interactable;
                if (_currentLook.transform.parent.gameObject.GetComponent<Toggle>() != null) clickable = true;
                if (_currentLook.transform.parent.gameObject.GetComponent<Slider>() != null) clickable = true;
                if (_currentLook.transform.parent.parent != null)
                {
                    if (_currentLook.transform.parent.parent.gameObject.GetComponent<Slider>() != null) {
                        if (_currentLook.name != "Handle") clickable = true;
                    }
                    if (_currentLook.transform.parent.parent.gameObject.GetComponent<Toggle>() != null) clickable = true;
                }
            }
            
            if (clickable)
            {
                if (_lastActiveButton == _currentLook)
                {
                    UpdateGaze((Time.realtimeSinceStartup - _currentStartTime) / gazeTime, true);
                    
                    if (Time.realtimeSinceStartup - _currentStartTime > gazeTime)
                    {
                        UpdateGaze(0, false);
                        ExecutePress();
                        _currentStartTime = Time.realtimeSinceStartup + (repeatGazeTime >= 0.1f? repeatGazeTime : float.MaxValue);
                    }
                }
                else
                {
                    _lastActiveButton = _currentLook;
                    _currentStartTime = Time.realtimeSinceStartup;
                    UpdateGaze(0f, false);
                }
            }
            else
            {
                _lastActiveButton = null;
                UpdateGaze(0f, false);
                ClearSelection();
            }
        }
    }
    
    private void ExecutePress()
    {
        ClearSelection();

        _lookData.pressPosition = _lookData.position;
        _lookData.pointerPressRaycast = _lookData.pointerCurrentRaycast;
        _lookData.pointerPress = null;

        if (_currentLook != null)
        {
            _currentPressed = _currentLook;
            GameObject newPressed = ExecuteEvents.ExecuteHierarchy(_currentPressed, _lookData, ExecuteEvents.pointerDownHandler);

            if (newPressed == null)
            {
                newPressed = ExecuteEvents.ExecuteHierarchy(_currentPressed, _lookData, ExecuteEvents.pointerClickHandler);
                if (newPressed != null) { _currentPressed = newPressed; }
            }
            else
            {
                _currentPressed = newPressed;
                ExecuteEvents.Execute(newPressed, _lookData, ExecuteEvents.pointerClickHandler);
            }

            ExecuteEvents.ExecuteHierarchy(_currentPressed, _lookData, ExecuteEvents.pointerUpHandler);
        }
    }
    
    private PointerEventData GetLookPointerEventData()
    {
        _lookData ??= new PointerEventData(eventSystem);
        _lookData.Reset();
        _lookData.delta = Vector2.zero;
        _lookData.position = GetLookPosition();
        _lookData.scrollDelta = Vector2.zero;
        eventSystem.RaycastAll(_lookData, m_RaycastResultCache);
        _lookData.pointerCurrentRaycast = FindFirstRaycast(m_RaycastResultCache);
        m_RaycastResultCache.Clear();
        return _lookData;
    }
    
    private Vector2 GetLookPosition()
    {
        Vector2 lookPosition;
        if (UnityEngine.XR.XRSettings.enabled)
        {
            lookPosition = new Vector2 (UnityEngine.XR.XRSettings.eyeTextureWidth / 2f, UnityEngine.XR.XRSettings.eyeTextureHeight / 2f);
        }
        else
        {
            lookPosition = new Vector2 (Screen.width / 2f, Screen.height / 2f);
        }

        return lookPosition;
    }
   
    private void ClearSelection()
    {
        if (eventSystem.currentSelectedGameObject) { eventSystem.SetSelectedGameObject(null); }
    }

    protected virtual void UpdateGaze(float percentage, bool gazeEnabled)
    {
        OnGazeUpdate?.Invoke(percentage, gazeEnabled);
    }
    
    #endregion
}
