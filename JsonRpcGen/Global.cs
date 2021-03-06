using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using JsonRpcGen.TypeHandler;
using Microsoft.CSharp;
using Newtonsoft.Json.Linq;

namespace JsonRpcGen
{
    internal static class Global
    {
        public static string BaseNamespace = "XBMCRPC";

        public static DirectoryInfo TargetDir { get; set; }

        public static Dictionary<string, MultiBaseType> MultiBaseTypes = new Dictionary<string, MultiBaseType>();

        public static TypeMap TypeMap { get; set; }

        static Global()
        {
            TypeMap= new TypeMap();
        }

        public static string GetRefName(string refName, Dictionary<string, string> replacements)
        {
            if (replacements.ContainsKey(refName))
            {
                return replacements[refName];
            }
            switch (refName)
            {
                case "Array.String":
                    return "string[]";
                case "Array.Integer":
                    return "int[]";
                case "Optional.Boolean":
                    return "bool?";
                case "Optional.Integer":
                    return "int?";
                case "Optional.Number":
                    return "double?";
                case "Optional.String":
                    return "string";
                case "Global.String.NotEmpty":
                    return "string";
                default:
                    return BaseNamespace + "." + refName;
            }
        }

        public static bool IsValueType(JToken arg)
        {
            if (arg["type"] == null)
                return false;
            var type = arg["type"].ToString();
            switch (type)
            {
                case "boolean":
                case "integer":
                case "number":
                case "string":
                    return true;
                default:
                    return false;
            }
        }


        public static string MakeFirstUpper(string name)
        {
            var chars = name.ToCharArray();
            chars[0] = char.ToUpperInvariant(chars[0]);
            name = new string(chars);
            return name;
        }


        public static bool IsReservedName(string name)
        {
            CSharpCodeProvider cs = new CSharpCodeProvider();
            return !cs.IsValidIdentifier(name);
        }
    }
}