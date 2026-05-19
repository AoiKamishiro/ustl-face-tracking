using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace USTL.FaceTracking.Editor
{
    internal abstract class FaceTrackingEditorBase : UnityEditor.Editor
    {
        private const string DefaultLogoGuid = "5fcd88df613914a19aa49da0ed427b5e";
        private const int LogoSizeRefreshMaxTries = 10;
        private const float FallbackLogoHeight = 45f;

        private static readonly Dictionary<string, Texture2D> LogoTextures = new();

        protected virtual string LogoGuid => DefaultLogoGuid;

        protected virtual bool ShowLogo => true;

        public sealed override VisualElement CreateInspectorGUI()
        {
            VisualElement root = CreateInspectorRoot();

            if (ShowLogo)
            {
                AddLogo(root);
            }

            BuildInspectorGUI(root);
            return root;
        }

        protected abstract void BuildInspectorGUI(VisualElement root);

        private static VisualElement CreateInspectorRoot()
        {
            VisualElement root = new();
            FaceTrackingInspectorStyles.ApplyInspectorRoot(root);
            return root;
        }

        private void AddLogo(VisualElement root)
        {
            Texture2D logoTexture = LoadLogoTexture(LogoGuid);
            if (logoTexture == null)
            {
                return;
            }

            VisualElement container = new();
            FaceTrackingInspectorStyles.ApplyLogoContainer(container);

            Image image = new()
            {
                image = logoTexture,
                scaleMode = ScaleMode.ScaleToFit,
            };
            FaceTrackingInspectorStyles.ApplyLogoImage(image);
            SetLogoImageSize(image, logoTexture);

            container.Add(image);
            root.Add(container);
        }

        private static Texture2D LoadLogoTexture(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                return null;
            }

            if (LogoTextures.TryGetValue(guid, out Texture2D cachedTexture))
            {
                return cachedTexture;
            }

            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            Texture2D texture = string.IsNullOrEmpty(assetPath) ? null : AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
            LogoTextures[guid] = texture;
            return texture;
        }

        private static void SetLogoImageSize(Image image, Texture2D logoTexture, int maxTries = LogoSizeRefreshMaxTries)
        {
            float height = GetTargetLogoHeight();
            if (height <= 0f)
            {
                if (maxTries <= 0)
                {
                    return;
                }

                EditorApplication.delayCall += () => SetLogoImageSize(image, logoTexture, maxTries - 1);
                height = FallbackLogoHeight;
            }

            image.style.width = new Length(CalculateLogoWidth(logoTexture, height), LengthUnit.Pixel);
            image.style.height = new Length(height, LengthUnit.Pixel);
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

        private static float CalculateLogoWidth(Texture2D logoTexture, float height)
        {
            if (logoTexture.height <= 0)
            {
                return height;
            }

            return height / logoTexture.height * logoTexture.width;
        }
    }
}
