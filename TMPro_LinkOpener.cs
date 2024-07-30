using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using System;
using Home.Common.Tooltips;
using Home.Common;
namespace ModPageLib;
public class TMPro_LinkOpener : TooltipTriggerBase, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    private bool _pointerIsOverText = false;
    private bool _clicked = false;
    private Camera camera;

    private TextMeshProUGUI _text;

    private string _currentlyHoveredLinkID = null;

    public override void OnPointerEnter(PointerEventData eventData) => _pointerIsOverText = true;
    public override void OnPointerExit(PointerEventData eventData) => _pointerIsOverText = false;

    public void OnPointerClick(PointerEventData eventData) => _clicked = true;

    public void Start()
    {
        _text = GetComponent<TextMeshProUGUI>();
        foreach (Camera c in FindObjectsOfType<Camera>())
        {
            if (c.name != "UICamera") continue;
            camera = c;
        }
    }
    public void LateUpdate()
    {
        if (!_pointerIsOverText) return;

        int hoveredLinkIndex = TMP_TextUtilities.FindIntersectingLink(_text, Input.mousePosition, camera);
        string hoveredLinkID = hoveredLinkIndex == -1 ? null : _text.textInfo.linkInfo[hoveredLinkIndex].GetLinkID();

        //unhover actions
        bool noLinkIsHovered = hoveredLinkID == null;
        if (noLinkIsHovered)
        {
            if (_currentlyHoveredLinkID != null)
            {
                StopHoverInstantly();
                _currentlyHoveredLinkID = null;
            }
            _clicked = false;
            return;
        }

        //hovered has changed -> hover actions
        bool hoveredLinkHasChanged = hoveredLinkID != _currentlyHoveredLinkID;
        if (hoveredLinkHasChanged)
        {
            if (_currentlyHoveredLinkID != null)
                StopHoverInstantly();

            StartHover(hoveredLinkID);
            _currentlyHoveredLinkID = hoveredLinkID;
        }

        //clicked actions
        if (_clicked)
        {
            Console.WriteLine($"URL clicked: linkInfo[{hoveredLinkIndex}] with id {hoveredLinkID}");
            StopHoverInstantly();
            Application.OpenURL(hoveredLinkID);
        }

        _clicked = false;
    }
    public void StartHover(string text)
    {
        if (TooltipView.Instance(TargetTooltipView) != null)
        {
            TooltipView.Instance(TargetTooltipView).TargetObject = gameObject;
            bool flag = false;
            TooltipView.Instance(TargetTooltipView).Show(text, flag);
        }
    }
}