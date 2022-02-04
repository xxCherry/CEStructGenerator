using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace CEStructGenerator
{
    public class Program
    {
        private static string _structContent;
        private static string _structName;
        private static StringBuilder _stringBuilder = new StringBuilder();
        private static List<Element> _elements = new List<Element>();

        public static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                return;
            }

            if (File.Exists(args[0])) // Probably bad way to check if arg is path, but i don't care
            {
                _structContent = File.ReadAllText(args[0]);
            }
            else
            {
                _structContent = args[0];
            }

            ParseXml();

            var result = BuildCode();

            Console.WriteLine(result);

            Console.ReadKey();
        }

        public static void ParseXml()
        {
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(_structContent));

            using (var reader = XmlReader.Create(new StreamReader(stream)))
            {
                reader.ReadToFollowing("Structure");
                reader.MoveToAttribute("Name");

                _structName = reader.Value;

                reader.ReadToFollowing("Element");

                // read first element
                var type = reader.GetAttribute("Vartype");
                var name = reader.GetAttribute("Description");

                _elements.Add(new Element(type, name));

                while (reader.Read() && reader.NodeType != XmlNodeType.EndElement && reader.Name != "Elements")
                {
                    if (reader.NodeType == XmlNodeType.Element && reader.Name == "Element")
                    {
                        type = reader.GetAttribute("Vartype");
                        name = reader.GetAttribute("Description");

                        _elements.Add(new Element(type, name));
                    }
                }
            }
        }

        public static string BuildCode()
        {
            _stringBuilder.AppendLine($"public struct {_structName} " + "{");
            foreach (var element in _elements)
            {
                var cppType = "";

                switch (element.Type)
                {
                    case "Pointer":
                        cppType = "uintptr_t";
                        break;
                    case "Byte":
                        cppType = "int8_t";
                        break;
                    case "2 Bytes":
                        cppType = "int16_t";
                        break;
                    case "4 Bytes":
                        cppType = "int32_t";
                        break;
                    case "8 Bytes":
                        cppType = "int64_t";
                        break;
                    case "Float":
                        cppType = "float";
                        break;
                    case "Double":
                        cppType = "double";
                        break;
                    case "Array of byte":
                        cppType = "int8_t[]";
                        break;
                    case "String":
                        cppType = "char*";
                        break;
                    case "Unicode String":
                        cppType = "wchar_t*";
                        break;
                }

                _stringBuilder.AppendLine($"    {cppType} {element.Name};");
            }
            _stringBuilder.AppendLine("}");

            return _stringBuilder.ToString();
        }
    }
}
