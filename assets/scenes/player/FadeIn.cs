using Godot;
using System;

public partial class FadeIn : ColorRect
{
    bool fading = false;
    int fadeDirection = 1;

    const float fadeSeconds = 2f;
    float fadeSpeed = 1 / fadeSeconds;

    public override void _Ready()
    {

    }

    public override void _Process(double delta)
    {
        if (fading) {
            Color currentColor = Color;
            currentColor.A += fadeDirection * fadeSpeed * (float)delta;
            Color = currentColor;

            if (fadeDirection == -1 && currentColor.A <= 0) {
                fading = false;
            }
        }
    }

    public void DoFadeIn()
    {
        Color currentColor = Color;
        currentColor.A = 1;
        Color = currentColor;

        fadeDirection = -1;
        fading = true;
    }

    public void DoFadeOut()
    {
    }
}
