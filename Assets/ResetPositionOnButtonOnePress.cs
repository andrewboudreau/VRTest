using System.Collections;
using UnityEngine;
using VRTK;

public class ResetPositionOnButtonOnePress : MonoBehaviour
{
    public GameObject objectToReset;

    private void Start()
    {
        if (GetComponent<VRTK_ControllerEvents>() == null)
        {
            Debug.LogError("VRTK_ControllerEvents_ListenerExample is required to be attached to a Controller that has the VRTK_ControllerEvents script attached to it");
            return;
        }

        if (objectToReset == null)
        {
            Debug.LogError("ObjectToReset must be set for the script to work");
            return;
        }

        GetComponent<VRTK_ControllerEvents>().TouchpadPressed += new ControllerInteractionEventHandler(DoResetButtonPressed);
    }

    private void DoResetButtonPressed(object sender, ControllerInteractionEventArgs e)
    {
        var clock = objectToReset.GetComponent<ClockGrab>();
        if (clock)
        {
            clock.ReturnToHome();
        }
    }
}
