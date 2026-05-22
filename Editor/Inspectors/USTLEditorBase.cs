using nadena.dev.ndmf.localization;
using UnityEngine.UIElements;

namespace USTL.FaceTracking.Editor
{
    internal abstract class USTLEditorBase : UnityEditor.Editor
    {
        protected VisualElement Root { get; private set; }

        protected virtual bool ShowLogo => true;

        public sealed override VisualElement CreateInspectorGUI()
        {
            VisualElement root = CreateInspectorRoot();
            Root = root;

            if (ShowLogo)
            {
                AddLogo(root);
            }

            BuildInspectorGUI(root);
            LanguagePrefs.RegisterLanguageChangeCallback(root, _ => ApplyLocalization(root));
            ApplyLocalization(root);
            return root;
        }

        private static void ApplyLocalization(VisualElement root)
        {
            LocalizationUtility.Localize(root);
        }

        protected abstract void BuildInspectorGUI(VisualElement root);

        private static VisualElement CreateInspectorRoot()
        {
            VisualElement root = new()
            {
                style =
                {
                    marginTop = 0,
                    marginBottom = 0,
                },
            };
            return root;
        }

        private static void AddLogo(VisualElement root)
        {
            root.Add(new LogoElement());
        }
    }
}
