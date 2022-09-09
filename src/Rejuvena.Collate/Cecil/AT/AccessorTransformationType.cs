using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Rejuvena.Collate.Cecil.AT
{
    internal readonly record struct AccessorTransformationType(string Value, bool NoStatic = false)
    {
        /// <summary>
        ///     Doesn't perform an operation on an accessor.
        /// </summary>
        public static readonly AccessorTransformationType Inherit = new("=");

        /// <summary>
        ///     Transforms an accessor to be public.
        /// </summary>
        public static readonly AccessorTransformationType Public = new("public");

        /// <summary>
        ///     Transforms an accessor to be static.
        /// </summary>
        public static readonly AccessorTransformationType Private = new("private");

        /// <summary>
        ///     Transforms an accessor to be internal.
        /// </summary>
        public static readonly AccessorTransformationType Internal = new("internal");

        /// <summary>
        ///     Transforms an accessor to be protected.
        /// </summary>
        public static readonly AccessorTransformationType Protected = new("protected", true);

        /// <summary>
        ///     Transforms an accessor to be protected internal.
        /// </summary>
        public static readonly AccessorTransformationType ProtectedInternal = new("protected-internal", true);

        /// <summary>
        ///     Transforms an accessor to be private protected.
        /// </summary>
        public static readonly AccessorTransformationType PrivateProtected = new("private-protected", true);

        public static readonly ReadOnlyCollection<AccessorTransformationType> Types =
            new List<AccessorTransformationType>
            {
                Public,
                Private,
                Internal,
                Private,
                Protected,
                ProtectedInternal,
                PrivateProtected
            }.AsReadOnly();

        public static AccessorTransformationType Parse(string value) {
            // Add *some* leniency, I guess.
            value = value.ToLower().Replace(' ', '-');

            AccessorTransformationType? type = Types.FirstOrDefault(x => x.Value.Equals(value));

            return type ?? Inherit;
        }
    }
}