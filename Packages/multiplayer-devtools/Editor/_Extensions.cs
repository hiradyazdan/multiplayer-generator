using System;
using System.Reflection;
using System.Reflection.Emit;

namespace MultiPlayerDevTools
{
    public static class _Extensions
    {
        public static void SetInstanceField<T>(this object instance, string variableName, T value)
        {
            var type = instance.GetType();
            
            var field = type.GetField(variableName, BindingFlags.Instance | BindingFlags.NonPublic);
            var valueType = value.GetType();
            var fieldType = field?.FieldType;
            
            if (fieldType != valueType)
            {
                throw new ArgumentException(
                    $"Argument value should be of type {fieldType}, but {valueType} was passed.", nameof(value)
                );
            }
            
            field.SetValue(instance, value);
        }
    }
}