using UnityEngine;
using UnityEngine.UI;

public class GazePointer : MonoBehaviour
{
    #region Inspector
    [SerializeField] private Image circleProgressBar;
    #endregion

    #region Monobehaviour
    private void OnEnable()
    {
        XRUIInputModuleWithGaze.OnGazeUpdate += HandleGazeUpdate;
    }

    private void OnDisable()
    {
        XRUIInputModuleWithGaze.OnGazeUpdate -= HandleGazeUpdate;
    }
    
    #endregion

    #region Methods
    private void HandleGazeUpdate(float percentage, bool gazeEnabled)
    {
        circleProgressBar.gameObject.SetActive(gazeEnabled);
        circleProgressBar.fillAmount = percentage;
    }
    #endregion
}
