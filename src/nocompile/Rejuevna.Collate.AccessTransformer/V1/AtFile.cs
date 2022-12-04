using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Mono.Cecil;

namespace Rejuevna.Collate.AccessTransformer.V1
{
    public class AtFile : IAtFile
    {
        public const int VERSION = 1;

        public int Version => VERSION;

        private readonly AtItem[] Items;

        public AtFile(AtItem[] items) {
            Items = items;
        }

        #region Transformer

        public bool Transform(ModuleDefinition module) {
            bool modified = false;

            foreach (AtItem transformer in Items) {
                List<TypeDefinition> types = new();
                foreach (TypeDefinition type in module.Types) CollectAllNested(type, types);

                foreach (TypeDefinition type in types) {
                    if (transformer.ObjectToTransform == type.FullName) {
                        modified = true;
                        type.IsSealed = ReadonlyState(transformer, type.IsSealed);
                        if (type.IsSealed && type.IsAbstract) type.IsSealed = false; // We don't want to ever make types static.
                    }
                    else {
                        IEnumerable<MethodDefinition> methods = type.Methods.Where(method => transformer.ObjectToTransform == method.FullName);
                        foreach (MethodDefinition method in methods) {
                            modified = true;
                            method.IsFinal = ReadonlyState(transformer, type.IsSealed);

                            if (!StaticSafeOperation(transformer, method.IsStatic)) continue;
                            switch (transformer.AccessorTarget) {
                                case AccessorType.Inherit:
                                    continue;
                                case AccessorType.Internal:
                                    method.IsAssembly = true;
                                    break;
                                case AccessorType.Private:
                                    method.IsPrivate = true;
                                    break;
                                case AccessorType.Protected:
                                    method.IsFamily = true;
                                    break;
                                case AccessorType.Public:
                                    method.IsPublic = true;
                                    break;
                                case AccessorType.PrivateProtected:
                                    method.IsFamilyAndAssembly = true;
                                    break;
                                case AccessorType.ProtectedInternal:
                                    method.IsFamilyOrAssembly = true;
                                    break;
                            }
                        }

                        IEnumerable<FieldDefinition> fields = type.Fields.Where(field => transformer.ObjectToTransform == field.FullName);
                        foreach (FieldDefinition field in fields) {
                            modified = true;
                            field.IsInitOnly = ReadonlyState(transformer, field.IsStatic);

                            if (!StaticSafeOperation(transformer, field.IsStatic)) continue;
                            switch (transformer.AccessorTarget) {
                                case AccessorType.Inherit:
                                    continue;
                                case AccessorType.Internal:
                                    field.IsAssembly = true;
                                    break;
                                case AccessorType.Private:
                                    field.IsPrivate = true;
                                    break;
                                case AccessorType.Protected:
                                    field.IsFamily = true;
                                    break;
                                case AccessorType.Public:
                                    field.IsPublic = true;
                                    break;
                                case AccessorType.PrivateProtected:
                                    field.IsFamilyAndAssembly = true;
                                    break;
                                case AccessorType.ProtectedInternal:
                                    field.IsFamilyOrAssembly = true;
                                    break;
                            }
                        }
                    }
                }
            }

            return modified;
        }

        private static void CollectAllNested(TypeDefinition type, ICollection<TypeDefinition> types) {
            types.Add(type);
            if (!type.HasNestedTypes) return;
            foreach (TypeDefinition nested in type.NestedTypes) CollectAllNested(nested, types);
        }

        private static bool StaticSafeOperation(AtItem transformer, bool @static) {
            return transformer.AccessorTarget.AllowsStatic() || !@static;
        }

        private static bool ReadonlyState(AtItem transformer, bool @readonly) {
            return transformer.ReadonlyTarget == ReadonlyType.Inherit ? @readonly : transformer.ReadonlyTarget == ReadonlyType.Readonly;
        }

        #endregion

        public void Write(StreamWriter writer) {
            writer.Write(ToString());
        }

        public void WriteFile(string path) {
            File.WriteAllText(path, ToString());
        }

        public override string ToString() {
            StringBuilder sb = new();
            sb.AppendLine($"v{VERSION}");
            foreach (AtItem item in Items) sb.AppendLine(item.ToString());
            return sb.ToString();
        }

        public static AtFile Read(string path) {
            string[] lines = File.ReadAllLines(path).Where(x => !string.IsNullOrWhiteSpace(x) && !x.StartsWith("#")).Select(x => x.Trim()).ToArray();
            string versionLine = lines[0];

            if (!versionLine.StartsWith("v")) throw new InvalidOperationException("Tried to parse file without specified version.");
            if (int.Parse(versionLine.Substring(1)) != VERSION) throw new InvalidOperationException("Tried to parse file with invalid version.");

            return new AtFile(
                lines.Where(line => !string.IsNullOrWhiteSpace(line) && !line.StartsWith("v")).Select(AtItem.Parse).ToArray()
            );
        }
    }
}