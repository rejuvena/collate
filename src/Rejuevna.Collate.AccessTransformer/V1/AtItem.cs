﻿namespace Rejuevna.Collate.AccessTransformer.V1
{
    public readonly record struct AtItem(string ObjectToTransform, AccessorType AccessorTarget, ReadonlyType ReadonlyTarget)
    {
        public string ObjectToTransform { get; } = ObjectToTransform;

        public AccessorType AccessorTarget { get; } = AccessorTarget;

        public ReadonlyType ReadonlyTarget { get; } = ReadonlyTarget;

        public override string ToString() {
            return $"{AccessorTypeUtils.ToString(AccessorTarget)} {ReadonlyTypeUtils.ToString(ReadonlyTarget)} {ObjectToTransform}";
        }

        public static AtItem Parse(string value) {
            string[] elems = value.Split(new[] {' '}, 3);
            return new AtItem(elems[2], AccessorTypeUtils.Parse(elems[0]), ReadonlyTypeUtils.Parse(elems[1]));
        }
    }
}