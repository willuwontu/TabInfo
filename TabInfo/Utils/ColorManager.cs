using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace TabInfo.Utils
{
    public class ColorManager
    {
        public static float RelativeLuminance(Color color)
        {
            float ColorPartValue(float part)
            {
                return part <= 0.03928f ? part / 12.92f : Mathf.Pow((part + 0.055f) / 1.055f, 2.4f);
            }
            var r = ColorPartValue(color.r);
            var g = ColorPartValue(color.g);
            var b = ColorPartValue(color.b);

            var l = 0.2126f * r + 0.7152f * g + 0.0722f * b;
            return l;
        }

        public static float ColorContrast(Color a, Color b)
        {
            float result = 0f;
            var La = RelativeLuminance(a) + 0.05f;
            var Lb = RelativeLuminance(b) + 0.05f;

            result = Mathf.Max(La, Lb) / Mathf.Min(La, Lb);

            return result;
        }

        /// <summary>
        /// Checks to see if a pair of colors contrast enough to be readable and returns a modified set if not.
        /// </summary>
        /// <param name="backgroundColor">The background color for the text to go on.</param>
        /// <param name="textColor">The intended text color.</param>
        /// <returns>A pair of contrasting colors, the lightest as the first color.</returns>
        public static Color[] GetContrastingColors(Color backgroundColor, Color textColor, float ratio)
        {
            Color[] colors = new Color[2];

            var backL = RelativeLuminance(backgroundColor);
            var textL = RelativeLuminance(textColor);

            if (textL > backL)
            {
                colors[0] = textColor;
                colors[1] = backgroundColor;
            }
            else
            {
                colors[1] = textColor;
                colors[0] = backgroundColor;
            }

            // See if we have good enough contrast already
            if (!(ColorContrast(backgroundColor, textColor) < ratio))
            {
                return colors;
            }

            Color.RGBToHSV(colors[0], out var lightH, out var lightS, out var lightV);
            Color.RGBToHSV(colors[1], out var darkH, out var darkS, out var darkV);

            // If the darkest color can be darkened enough to have enough contrast after brightening the color.
            if (ColorContrast(Color.HSVToRGB(darkH, darkS, 0f), Color.HSVToRGB(lightH, lightS, 1f)) >= ratio)
            {
                var lightDiff = 1f - lightV;
                var darkDiff = darkV;

                var steps = new float[] { 0.12f, 0.1f, 0.08f, 0.05f, 0.04f, 0.03f, 0.02f, 0.01f, 0.005f };
                var step = 0;

                var lightRatio = (lightDiff / (lightDiff + darkDiff));
                var darkRatio = (darkDiff / (lightDiff + darkDiff));

                while (ColorContrast(Color.HSVToRGB(lightH, lightS, lightV), Color.HSVToRGB(darkH, darkS, darkV)) < ratio)
                {
                    while (ColorContrast(Color.HSVToRGB(lightH, lightS, lightV + lightRatio * steps[step]), Color.HSVToRGB(darkH, darkS, darkV - darkRatio * steps[step])) > ratio && step < steps.Length - 1)
                    {
                        step++;
                    }
                    lightV += lightRatio * steps[step];
                    darkV -= darkRatio * steps[step];
                }

                colors[0] = Color.HSVToRGB(lightH, lightS, lightV);
                colors[1] = Color.HSVToRGB(darkH, darkS, darkV);
            }
            // Fall back to using white.
            else
            {
                colors[0] = Color.white;

                while (ColorContrast(Color.white, Color.HSVToRGB(darkH, darkS, darkV)) < ratio)
                {
                    darkV -= 0.01f;
                }

                colors[1] = Color.HSVToRGB(darkH, darkS, darkV);
            }

            return colors;
        }
    }
}
