using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Rejuvena.Collate.Cecil.AT
{
    internal readonly record struct ReadonlyTransformationType(string Value)
    {
        /// <summary>
        ///     Doesn't perform any modification to the readonly state.
        /// </summary>
        public static readonly ReadonlyTransformationType Inherit = new("=");

        /// <summary>
        ///     Transforms the readonly state to be true.
        /// </summary>
        public static readonly ReadonlyTransformationType Readonly = new("+r");

        /// <summary>
        ///     Transforms the readonly state to be false.
        /// </summary>
        public static readonly ReadonlyTransformationType ReadWrite = new("-r");

        public static readonly ReadOnlyCollection<ReadonlyTransformationType> Types =
            new List<ReadonlyTransformationType>
            {
                Readonly,
                ReadWrite
            }.AsReadOnly();

        public static ReadonlyTransformationType Parse(string value) {
            // Add *some* leniency, I guess.
            value = value.ToLower();

            ReadonlyTransformationType? type = Types.FirstOrDefault(x => x.Value.Equals(value));

            return type ?? Inherit;
        }
    }
}