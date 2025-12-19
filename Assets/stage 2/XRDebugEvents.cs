using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class XRDebugEvents : MonoBehaviour
{
    public void OnHoverEnter(HoverEnterEventArgs args)
    {
        Debug.Log($"[HOVER ENTER] interactor={name} target={args.interactableObject.transform.name}");
    }

    public void OnHoverExit(HoverExitEventArgs args)
    {
        Debug.Log($"[HOVER EXIT] interactor={name} target={args.interactableObject.transform.name}");
    }

    public void OnSelectEnter(SelectEnterEventArgs args)
    {
        Debug.Log($"[SELECT ENTER] interactor={name} target={args.interactableObject.transform.name}");
    }

    public void OnSelectExit(SelectExitEventArgs args)
    {
        Debug.Log($"[SELECT EXIT] interactor={name} target={args.interactableObject.transform.name}");
    }
}
