using System.Collections.Generic;
using UnityEngine.UIElements;

namespace USTL.FaceTracking.Editor
{
    public class RangeFloatField : FloatField
    {
        private float _maxValue = float.PositiveInfinity;
        private float _minValue = float.NegativeInfinity;

        public RangeFloatField()
        {
            this.RegisterValueChangedCallback(OnValueChanged);
        }

        public float minValue
        {
            get => _minValue;
            set
            {
                _minValue = value;
                SetValueWithoutNotify(this.value);
            }
        }

        public float maxValue
        {
            get => _maxValue;
            set
            {
                _maxValue = value;
                SetValueWithoutNotify(this.value);
            }
        }

        public string bindPath
        {
            get => bindingPath;
            set => bindingPath = value;
        }

        public override float value
        {
            get => base.value;
            set => base.value = Clamp(value);
        }

        public void SetRange(float minValue, float maxValue)
        {
            _minValue = minValue;
            _maxValue = maxValue;
            SetValueWithoutNotify(value);
        }

        public override void SetValueWithoutNotify(float newValue)
        {
            base.SetValueWithoutNotify(Clamp(newValue));
        }

        private void OnValueChanged(ChangeEvent<float> evt)
        {
            float clampedValue = Clamp(evt.newValue);
            if (EqualityComparer<float>.Default.Equals(evt.newValue, clampedValue))
            {
                return;
            }

            evt.StopImmediatePropagation();
            SetValueWithoutNotify(clampedValue);

            if (EqualityComparer<float>.Default.Equals(evt.previousValue, clampedValue))
            {
                return;
            }

            using (ChangeEvent<float> changeEvent = ChangeEvent<float>.GetPooled(evt.previousValue, clampedValue))
            {
                changeEvent.target = this;
                SendEvent(changeEvent);
            }
        }

        private float Clamp(float value)
        {
            float min = _minValue <= _maxValue ? _minValue : _maxValue;
            float max = _minValue <= _maxValue ? _maxValue : _minValue;

            if (value < min)
            {
                return min;
            }

            return value > max ? max : value;
        }

        public new class UxmlFactory : UxmlFactory<RangeFloatField, UxmlTraits>
        {
        }

        public new class UxmlTraits : FloatField.UxmlTraits
        {
            private readonly UxmlFloatAttributeDescription _maxValue = new()
            {
                name = "max-value",
                defaultValue = float.PositiveInfinity,
            };

            private readonly UxmlFloatAttributeDescription _minValue = new()
            {
                name = "min-value",
                defaultValue = float.NegativeInfinity,
            };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);

                if (ve is RangeFloatField field)
                {
                    field.SetRange(_minValue.GetValueFromBag(bag, cc), _maxValue.GetValueFromBag(bag, cc));
                }
            }
        }
    }
}
