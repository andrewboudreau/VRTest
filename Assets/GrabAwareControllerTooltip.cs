using UnityEngine;
using VRTK;

/// <summary>
/// Doesn't show tooltips if the controller is grabbing something. 
/// </summary>
public class GrabAwareControllerTooltip : VRTK_ControllerTooltips
{

    [Tooltip("Show tooltips on start even when headset aware, but only until the first glance exit")]
    public bool showTipsOnStart = true;

    private bool starting = true;

    private VRTK_InteractGrab interactGrab;
    
    protected override void Awake()
    {
        base.Awake();

        interactGrab = GetComponentInParent<VRTK_InteractGrab>();
        if (interactGrab)
        {
            interactGrab.ControllerGrabInteractableObject += new ObjectInteractEventHandler(DoGrab);
            interactGrab.ControllerUngrabInteractableObject += new ObjectInteractEventHandler(DoUngrab);
        }
    }

    protected override void Update()
    {
        base.Update();

        if (showTipsOnStart && starting)
        {
            starting = false;
            ToggleTips(true);
        }
    }

    void DoGrab(object sender, ObjectInteractEventArgs e)
    {
        ToggleTips(false);
        enabled = false;
    }

    void DoUngrab(object sender, ObjectInteractEventArgs e)
    {
        enabled = true;
    }
}
