using System.Collections;
using System.Collections.Generic;
using SoundStone.SoundSystem;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;
using SoundStone;

[RequireComponent(typeof(PlayerToolkit))]
public class ToolkitUI : UIElement
{
    public SteamVR_Action_Boolean openUIAction = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("OpenUI");
    public Transform handTransform;

    protected PlayerToolkit playerToolkitInstance;
    protected Hand hand;

}
