using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIPopup : UIBase
{
    private string popupName;
    
    protected override void OnEnable()
    {
        base.OnEnable();
    }
    
    protected override void Reset()
    {
        base.Reset();
    }
    
    public override void OnShow(Action<UIBase> finished = null)
    {
        base.OnShow(finished);
    }
    
    public override void OnClose(Action<UIBase> finished = null)
    {   
        base.OnClose(finished);
    }
    
    
}
