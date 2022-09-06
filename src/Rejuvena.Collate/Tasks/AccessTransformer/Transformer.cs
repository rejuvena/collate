using System.Collections.Generic;

namespace Rejuvena.Collate.Tasks.AcessTransformer
{
    public class Transformer
    {
        public readonly string ObjectToTransform;
        public readonly AccessorTransformationType AccessorTransformation;
        public readonly ReadonlyTransformationType ReadonlyTransformation;

        public Transformer(
            string objectToTransform,
            AccessorTransformationType? accessorTransformation,
            ReadonlyTransformationType? readonlyTransformation
        ) {
            ObjectToTransform = objectToTransform;
            AccessorTransformation = accessorTransformation ?? AccessorTransformationType.Inherit;
            ReadonlyTransformation = readonlyTransformation ?? ReadonlyTransformationType.Inherit;
        }

        public override string ToString() {
            List<string> values = new();

            if (AccessorTransformation != AccessorTransformationType.Inherit) values.Add(AccessorTransformation.Value);

            if (ReadonlyTransformation != ReadonlyTransformationType.Inherit) values.Add(ReadonlyTransformation.Value);

            values.Add(ObjectToTransform);

            return string.Join(" ", values);
        }

        public static Transformer Parse(string value) {
            string[] elements = value.Split(' ', 3);

            return new Transformer(
                elements[2],
                AccessorTransformationType.Parse(elements[0]),
                ReadonlyTransformationType.Parse(elements[1])
            );
        }
    }
}