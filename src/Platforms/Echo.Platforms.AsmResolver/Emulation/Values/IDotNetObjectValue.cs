using AsmResolver.DotNet;
using Echo.Concrete.Values;

namespace Echo.Platforms.AsmResolver.Emulation.Values
{
    /// <summary>
    /// Represents a compound object consisting of fields.
    /// </summary>
    public interface IDotNetObjectValue : IDotNetValue
    {
        /// <summary>
        /// Gets the value of a field stored in the object.
        /// </summary>
        /// <param name="field">The field to get the value from.</param>
        /// <returns>The field value.</returns>
        IConcreteValue GetFieldValue(IFieldDescriptor field);

        /// <summary>
        /// Sets the value of a field stored in the object.
        /// </summary>
        /// <param name="field">The field to set the value for.</param>
        /// <param name="value">The new value.</param>
        void SetFieldValue(IFieldDescriptor field, IConcreteValue value);
    }
}