using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace USTL.FaceTracking.Editor
{
    internal sealed class LogoElement : VisualElement
    {
        private const string LogoGuid = "5fcd88df613914a19aa49da0ed427b5e";

        private const int LogoSizeRefreshMaxTries = 10;
        private const float FallbackLogoSize = 45f;
        private static Texture2D _logoTexture;

        private readonly Image _image;

        internal LogoElement()
        {
            style.flexDirection = FlexDirection.Row;
            style.alignItems = Align.Center;
            style.justifyContent = Justify.Center;
            style.paddingTop = 4;
            style.paddingBottom = 10;

            _image = new Image
            {
                image = LogoTexture,
                scaleMode = ScaleMode.ScaleToFit,
                style =
                {
                    flexShrink = 0,
                },
            };

            SetLogoImageSize();

            Add(_image);
        }

        private static Texture2D LogoTexture
        {
            get
            {
                if (!_logoTexture)
                {
                    _logoTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(LogoGuid));
                }

                return _logoTexture;
            }
        }

        private void SetLogoImageSize(int maxTries = LogoSizeRefreshMaxTries)
        {
            float height = GetTargetLogoHeight();
            if (height <= 0f)
            {
                if (maxTries <= 0)
                {
                    return;
                }

                EditorApplication.delayCall += () => SetLogoImageSize(maxTries - 1);
                height = FallbackLogoSize;
            }

            _image.style.width = new Length(CalculateLogoWidth(height), LengthUnit.Pixel);
            _image.style.height = new Length(height, LengthUnit.Pixel);
        }

        private static float GetTargetLogoHeight()
        {
            try
            {
                return (EditorStyles.label?.lineHeight ?? 0f) * 3f;
            }
            catch (NullReferenceException)
            {
                return 0f;
            }
        }

        private static float CalculateLogoWidth(float height)
        {
            if (!LogoTexture)
            {
                return FallbackLogoSize;
            }

            if (LogoTexture.height <= 0)
            {
                return height;
            }

            return height / LogoTexture.height * LogoTexture.width;
        }
    }
}
