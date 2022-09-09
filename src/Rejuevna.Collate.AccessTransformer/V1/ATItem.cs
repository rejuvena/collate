namespace Rejuevna.Collate.AccessTransformer.V1
{
    public readonly record struct ATItem(string ObjectToTransform, AccessorType AccessorTarget, ReadonlyType ReadonlyTarget)
    {
        public override string ToString() {
            return $"{AccessorTypeUtils.ToString(AccessorTarget)} {ReadonlyTypeUtils.ToString(ReadonlyTarget)} {ObjectToTransform}";
        }

        public static ATItem Parse(string value) {
            string[] elems = value.Split(' ', 3);
            return new ATItem(elems[2], AccessorTypeUtils.Parse(elems[0]), ReadonlyTypeUtils.Parse(elems[1]));
        }
    }
}