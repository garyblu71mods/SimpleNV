using System;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Linq;
using UnityEngine;

namespace KerbVisionIR.PostProcessing
{
    [Serializable]
    public abstract class PostProcessEffectSettings : ScriptableObject
    {
        public bool active = true;

        public BoolParameter enabled = new BoolParameter { overrideState = true, value = false };

        internal ReadOnlyCollection<ParameterOverride> parameters;

        void OnEnable()
        {
            parameters = GetType()
                .GetFields(BindingFlags.Public | BindingFlags.Instance)
                .Where(t => t.FieldType.IsSubclassOf(typeof(ParameterOverride)))
                .OrderBy(t => t.MetadataToken)
                .Select(t => (ParameterOverride)t.GetValue(this))
                .ToList()
                .AsReadOnly();

            foreach (var parameter in parameters)
                parameter.OnEnable();
        }

        void OnDisable()
        {
            if (parameters == null)
                return;

            foreach (var parameter in parameters)
                parameter.OnDisable();
        }

        public void SetAllOverridesTo(bool state, bool excludeEnabled = true)
        {
            foreach (var prop in parameters)
            {
                if (excludeEnabled && prop == enabled)
                    continue;

                prop.overrideState = state;
            }
        }

        public virtual bool IsEnabledAndSupported(PostProcessRenderContext context)
        {
            return enabled.value;
        }

        public int GetHash()
        {
            unchecked
            {
                int hash = 17;
                foreach (var p in parameters)
                    hash = hash * 23 + p.GetHash();
                return hash;
            }
        }
    }
}
