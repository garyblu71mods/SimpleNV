using System;
using UnityEngine;

namespace KerbVisionIR.PostProcessing
{
    /// <summary>
    /// The base abstract class for all parameter override types.
    /// </summary>
    public abstract class ParameterOverride
    {
        /// <summary>
        /// The override state of this parameter.
        /// </summary>
        public bool overrideState;

        internal abstract void Interp(ParameterOverride from, ParameterOverride to, float t);

        public abstract int GetHash();

        public T GetValue<T>()
        {
            return ((ParameterOverride<T>) this).value;
        }

        protected internal virtual void OnEnable()
        {
        }

        protected internal virtual void OnDisable()
        {
        }

        internal abstract void SetValue(ParameterOverride parameter);
    }

    [Serializable]
    public class ParameterOverride<T> : ParameterOverride
    {
        public T value;

        public ParameterOverride()
            : this(default(T), false)
        {
        }

        public ParameterOverride(T value)
            : this(value, false)
        {
        }

        public ParameterOverride(T value, bool overrideState)
        {
            this.value = value;
            this.overrideState = overrideState;
        }

        internal override void Interp(ParameterOverride from, ParameterOverride to, float t)
        {
            Interp(from.GetValue<T>(), to.GetValue<T>(), t);
        }

        public virtual void Interp(T from, T to, float t)
        {
            value = t > 0f ? to : from;
        }

        public void Override(T x)
        {
            overrideState = true;
            value = x;
        }

        internal override void SetValue(ParameterOverride parameter)
        {
            value = parameter.GetValue<T>();
        }

        public override int GetHash()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + overrideState.GetHashCode();
                hash = hash * 23 + value.GetHashCode();
                return hash;
            }
        }

        public static implicit operator T(ParameterOverride<T> prop)
        {
            return prop.value;
        }
    }

    [Serializable]
    public sealed class FloatParameter : ParameterOverride<float>
    {
        public override void Interp(float from, float to, float t)
        {
            value = from + (to - from) * t;
        }
    }

    [Serializable]
    public sealed class IntParameter : ParameterOverride<int>
    {
        public override void Interp(int from, int to, float t)
        {
            value = (int)(from + (to - from) * t);
        }
    }

    [Serializable]
    public sealed class BoolParameter : ParameterOverride<bool> {}

    [Serializable]
    public sealed class ColorParameter : ParameterOverride<Color>
    {
        public override void Interp(Color from, Color to, float t)
        {
            value.r = from.r + (to.r - from.r) * t;
            value.g = from.g + (to.g - from.g) * t;
            value.b = from.b + (to.b - from.b) * t;
            value.a = from.a + (to.a - from.a) * t;
        }

        public static implicit operator Vector4(ColorParameter prop)
        {
            return prop.value;
        }
    }

    [Serializable]
    public sealed class Vector2Parameter : ParameterOverride<Vector2>
    {
        public override void Interp(Vector2 from, Vector2 to, float t)
        {
            value.x = from.x + (to.x - from.x) * t;
            value.y = from.y + (to.y - from.y) * t;
        }
    }

    [Serializable]
    public sealed class Vector3Parameter : ParameterOverride<Vector3>
    {
        public override void Interp(Vector3 from, Vector3 to, float t)
        {
            value.x = from.x + (to.x - from.x) * t;
            value.y = from.y + (to.y - from.y) * t;
            value.z = from.z + (to.z - from.z) * t;
        }
    }

    [Serializable]
    public sealed class Vector4Parameter : ParameterOverride<Vector4>
    {
        public override void Interp(Vector4 from, Vector4 to, float t)
        {
            value.x = from.x + (to.x - from.x) * t;
            value.y = from.y + (to.y - from.y) * t;
            value.z = from.z + (to.z - from.z) * t;
            value.w = from.w + (to.w - from.w) * t;
        }
    }
}
