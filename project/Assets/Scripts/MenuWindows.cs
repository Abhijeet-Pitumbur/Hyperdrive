using UnityEngine;

public class MenuWindows : MonoBehaviour
{

    public void OpenWindow(CanvasGroup windowCanvas)
    {
        Menu.ins.windowOpen = true;
        Menu.ins.animating = true;
        if (Menu.ins.postProcessor.profile.TryGetSettings(out Menu.ins.depthOfField))
        { LeanTween.value(3.5f, 1f, Menu.ins.halfSecond).setEaseInOutQuart().setIgnoreTimeScale(true).setOnUpdate((float depth) => { Menu.ins.depthOfField.focusDistance.value = depth; }); }
        Menu.ins.FadeIn(windowCanvas);
        StartCoroutine(Menu.ins.FadeOut(Menu.ins.mainMenuCanvas));
    }

    public void CloseWindow(CanvasGroup windowCanvas)
    {
        Menu.ins.animating = true;
        if (Menu.ins.postProcessor.profile.TryGetSettings(out Menu.ins.depthOfField))
        { LeanTween.value(1f, 3.5f, Menu.ins.halfSecond).setEaseInOutQuart().setIgnoreTimeScale(true).setOnUpdate((float depth) => { Menu.ins.depthOfField.focusDistance.value = depth; }); }
        Menu.ins.FadeIn(Menu.ins.mainMenuCanvas);
        StartCoroutine(Menu.ins.FadeOut(windowCanvas));
        Menu.ins.windowOpen = false;
    }

}