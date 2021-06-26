using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace CEStructGenerator
{
    /*
     * <Elements>
  <Element Offset="64" Vartype="Pointer" Bytesize="4" Description="hitObjectManager" DisplayMethod="Unsigned Integer">
    <Structure Name="osu.GameplayElements.HitObjectManagerOsu" AutoFill="0" AutoCreate="1" DefaultHex="0" AutoDestroy="0" DoNotSaveLocal="0" RLECompression="1" AutoCreateStructsize="4096">
      <Elements>
        <Element Offset="0" Vartype="Pointer" Bytesize="4" Description="Vtable" DisplayMethod="Unsigned Integer"/>
        <Element Offset="4" Vartype="Pointer" Bytesize="4" Description="binder" DisplayMethod="Unsigned Integer"/>
        <Element Offset="8" Vartype="Double" Bytesize="8" Description="SliderScoringPointDistance" DisplayMethod="Unsigned Integer"/>
        <Element Offset="16" Vartype="Double" Bytesize="8" Description="SpinnerRotationRatio" DisplayMethod="Unsigned Integer"/>
        <Element Offset="24" Vartype="Float" Bytesize="4" Description="HitObjectRadius" DisplayMethod="Unsigned Integer"/>
        <Element Offset="28" Vartype="4 Bytes" Bytesize="4" Description="PreEmpt" DisplayMethod="Signed Integer"/>
        <Element Offset="32" Vartype="4 Bytes" Bytesize="4" Description="HitWindow50" DisplayMethod="Signed Integer"/>
        <Element Offset="36" Vartype="4 Bytes" Bytesize="4" Description="HitWindow100" DisplayMethod="Signed Integer"/>
        <Element Offset="40" Vartype="4 Bytes" Bytesize="4" Description="HitWindow300" DisplayMethod="Signed Integer"/>
        <Element Offset="44" Vartype="Float" Bytesize="4" Description="StackOffset" DisplayMethod="Unsigned Integer"/>
        <Element Offset="48" Vartype="Pointer" Bytesize="4" Description="Beatmap" DisplayMethod="Unsigned Integer"/>
        <Element Offset="52" Vartype="Pointer" Bytesize="4" Description="ActiveMods" DisplayMethod="Unsigned Integer"/>
        <Element Offset="56" Vartype="Pointer" Bytesize="4" Description="Bookmarks" DisplayMethod="Unsigned Integer"/>
        <Element Offset="60" Vartype="Pointer" Bytesize="4" Description="ComboColours" DisplayMethod="Unsigned Integer"/>
        <Element Offset="64" Vartype="Pointer" Bytesize="4" Description="eventManager" DisplayMethod="Unsigned Integer"/>
        <Element Offset="68" Vartype="Pointer" Bytesize="4" Description="hitFactory" DisplayMethod="Unsigned Integer"/>
        <Element Offset="72" Vartype="Pointer" Bytesize="4" Description="hitObjects" DisplayMethod="Unsigned Integer"/>
        <Element Offset="76" Vartype="Pointer" Bytesize="4" Description="hitObjectsMinimal" DisplayMethod="Unsigned Integer"/>
        <Element Offset="80" Vartype="Pointer" Bytesize="4" Description="hitObjectsReplaced" DisplayMethod="Unsigned Integer"/>
        <Element Offset="84" Vartype="Pointer" Bytesize="4" Description="lastHitObject" DisplayMethod="Unsigned Integer"/>
        <Element Offset="88" Vartype="Pointer" Bytesize="4" Description="spriteManager" DisplayMethod="Unsigned Integer"/>
        <Element Offset="92" Vartype="Pointer" Bytesize="4" Description="Variables" DisplayMethod="Unsigned Integer"/>
        <Element Offset="96" Vartype="Pointer" Bytesize="4" Description="ManiaStage" DisplayMethod="Unsigned Integer"/>
        <Element Offset="100" Vartype="Pointer" Bytesize="4" Description="followPoints" DisplayMethod="Unsigned Integer"/>
        <Element Offset="104" Vartype="Pointer" Bytesize="4" Description="ForcedHit" DisplayMethod="Unsigned Integer"/>
        <Element Offset="108" Vartype="Pointer" Bytesize="4" Description="lastAddedObject" DisplayMethod="Unsigned Integer"/>
        <Element Offset="112" Vartype="4 Bytes" Bytesize="4" Description="LongestObject" DisplayMethod="Signed Integer"/>
        <Element Offset="116" Vartype="4 Bytes" Bytesize="4" Description="LongestObjectCount" DisplayMethod="Signed Integer"/>
        <Element Offset="120" Vartype="Float" Bytesize="4" Description="SpriteRatio" DisplayMethod="Unsigned Integer"/>
        <Element Offset="124" Vartype="Float" Bytesize="4" Description="SpriteDisplaySize" DisplayMethod="Unsigned Integer"/>
        <Element Offset="128" Vartype="4 Bytes" Bytesize="4" Description="PreEmptSliderComplete" DisplayMethod="Signed Integer"/>
        <Element Offset="132" Vartype="4 Bytes" Bytesize="4" Description="CurrentComboBad" DisplayMethod="Signed Integer"/>
        <Element Offset="136" Vartype="4 Bytes" Bytesize="4" Description="CurrentComboKatu" DisplayMethod="Signed Integer"/>
        <Element Offset="140" Vartype="4 Bytes" Bytesize="4" Description="currentHitObjectIndex" DisplayMethod="Signed Integer"/>
        <Element Offset="144" Vartype="4 Bytes" Bytesize="4" Description="hitObjectsCount" DisplayMethod="Signed Integer"/>
        <Element Offset="148" Vartype="Byte" Bytesize="1" Description="ReadFromOsb" DisplayMethod="Unsigned Integer"/>
        <Element Offset="149" Vartype="Byte" Bytesize="1" Description="BookmarksDontDelete" DisplayMethod="Unsigned Integer"/>
      </Elements>
    </Structure>
  </Element>
</Elements>
    */
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
                        cppType = "float_t";
                        break;
                    case "Double":
                        cppType = "double_t";
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
