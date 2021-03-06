/*
 * Copyright (c) 2006-2016, openmetaverse.co
 * All rights reserved.
 *
 * - Redistribution and use in source and binary forms, with or without
 *   modification, are permitted provided that the following conditions are met:
 *
 * - Redistributions of source code must retain the above copyright notice, this
 *   list of conditions and the following disclaimer.
 * - Neither the name of the openmetaverse.co nor the names
 *   of its contributors may be used to endorse or promote products derived from
 *   this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
 * AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
 * ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE
 * LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
 * CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
 * SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
 * INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
 * CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
 * POSSIBILITY OF SUCH DAMAGE.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using System.Text;

namespace OpenMetaverse.StructuredData
{
    /// <summary>
    /// 
    /// </summary>
    public static partial class OSDParser
    {
        private static XmlSchema XmlSchema;
        private static XmlTextReader XmlTextReader;
        private static string LastXmlErrors = string.Empty;
        private static object XmlValidationLock = new object();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xmlData"></param>
        /// <returns></returns>
        public static OSD DeserializeLLSDXml(byte[] xmlData)
        {
            using(XmlTextReader xrd =  new XmlTextReader(new MemoryStream(xmlData, false)))
                return DeserializeLLSDXml(xrd);
        }

        public static OSD DeserializeLLSDXml(Stream xmlStream)
        {
            using(XmlTextReader xrd = new XmlTextReader(xmlStream))
                return DeserializeLLSDXml(xrd);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xmlData"></param>
        /// <returns></returns>
        public static OSD DeserializeLLSDXml(string xmlData)
        {
            byte[] bytes = Utils.StringToBytes(xmlData);
            using(XmlTextReader xrd = new XmlTextReader(new MemoryStream(bytes, false)))
                return DeserializeLLSDXml(xrd);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xmlData"></param>
        /// <returns></returns>
        public static OSD DeserializeLLSDXml(XmlTextReader xmlData)
        {
            try
            {
                xmlData.Read();
                SkipWhitespace(xmlData);

                xmlData.Read();
                OSD ret = ParseLLSDXmlElement(xmlData);

                return ret;
            }
            catch
            {
                return new OSD();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static byte[] SerializeLLSDXmlBytes(OSD data)
        {
            return Encoding.UTF8.GetBytes(SerializeLLSDXmlString(data));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string SerializeLLSDXmlString(OSD data)
        {
            StringWriter sw = new StringWriter();
            using(XmlTextWriter writer = new XmlTextWriter(sw))
            {
                writer.Formatting = Formatting.None;

                writer.WriteStartElement(string.Empty, "llsd", string.Empty);
                SerializeLLSDXmlElement(writer, data);
                writer.WriteEndElement();

                return sw.ToString();
            }
        }

        public static string SerializeLLSDInnerXmlString(OSD data)
        {
            StringWriter sw = new StringWriter();
            using (XmlTextWriter writer = new XmlTextWriter(sw))
            {
                writer.Formatting = Formatting.None;

                SerializeLLSDXmlElement(writer, data);

                return sw.ToString();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="data"></param>
        public static void SerializeLLSDXmlElement(XmlTextWriter writer, OSD data)
        {
            switch (data.Type)
            {
                case OSDType.Unknown:
                    writer.WriteStartElement(string.Empty, "undef", string.Empty);
                    writer.WriteEndElement();
                    break;
                case OSDType.Boolean:
                    writer.WriteStartElement(string.Empty, "boolean", string.Empty);
                    writer.WriteString(data.AsString());
                    writer.WriteEndElement();
                    break;
                case OSDType.Integer:
                    writer.WriteStartElement(string.Empty, "integer", string.Empty);
                    writer.WriteString(data.AsString());
                    writer.WriteEndElement();
                    break;
                case OSDType.Real:
                    writer.WriteStartElement(string.Empty, "real", string.Empty);
                    writer.WriteString(data.AsString());
                    writer.WriteEndElement();
                    break;
                case OSDType.String:
                    writer.WriteStartElement(string.Empty, "string", string.Empty);
                    writer.WriteString(data.AsString());
                    writer.WriteEndElement();
                    break;
                case OSDType.UUID:
                    writer.WriteStartElement(string.Empty, "uuid", string.Empty);
                    writer.WriteString(data.AsString());
                    writer.WriteEndElement();
                    break;
                case OSDType.Date:
                    writer.WriteStartElement(string.Empty, "date", string.Empty);
                    writer.WriteString(data.AsString());
                    writer.WriteEndElement();
                    break;
                case OSDType.URI:
                    writer.WriteStartElement(string.Empty, "uri", string.Empty);
                    writer.WriteString(data.AsString());
                    writer.WriteEndElement();
                    break;
                case OSDType.Binary:
                    writer.WriteStartElement(string.Empty, "binary", string.Empty);
                        writer.WriteStartAttribute(string.Empty, "encoding", string.Empty);
                        writer.WriteString("base64");
                        writer.WriteEndAttribute();
                    writer.WriteString(data.AsString());
                    writer.WriteEndElement();
                    break;
                case OSDType.Map:
                    OSDMap map = (OSDMap)data;
                    writer.WriteStartElement(string.Empty, "map", string.Empty);
                    foreach (KeyValuePair<string, OSD> kvp in map)
                    {
                        writer.WriteStartElement(string.Empty, "key", string.Empty);
                        writer.WriteString(kvp.Key);
                        writer.WriteEndElement();

                        SerializeLLSDXmlElement(writer, kvp.Value);
                    }
                    writer.WriteEndElement();
                    break;
                case OSDType.Array:
                    OSDArray array = (OSDArray)data;
                    writer.WriteStartElement(string.Empty, "array", string.Empty);
                    foreach (var element in array)
                    {
                        SerializeLLSDXmlElement(writer, element);
                    }
                    writer.WriteEndElement();
                    break;
                case OSDType.LlsdXml:
                    writer.WriteRaw(data.AsString());
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xmlData"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        public static bool TryValidateLLSDXml(XmlTextReader xmlData, out string error)
        {
            lock (XmlValidationLock)
            {
                LastXmlErrors = string.Empty;
                XmlTextReader = xmlData;

                CreateLLSDXmlSchema();

                XmlReaderSettings readerSettings = new XmlReaderSettings();
                readerSettings.ValidationType = ValidationType.Schema;
                readerSettings.Schemas.Add(XmlSchema);
                readerSettings.ValidationEventHandler += new ValidationEventHandler(LLSDXmlSchemaValidationHandler);

                using(XmlReader reader = XmlReader.Create(xmlData, readerSettings))
                {

                    try
                    {
                        while (reader.Read()) { }
                    }
                    catch (XmlException)
                    {
                        error = LastXmlErrors;
                        return false;
                    }

                    if (LastXmlErrors == string.Empty)
                    {
                        error = null;
                        return true;
                    }
                    else
                    {
                        error = LastXmlErrors;
                        return false;
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        private static OSD ParseLLSDXmlElement(XmlTextReader reader)
        {
            SkipWhitespace(reader);

            if (reader.NodeType != XmlNodeType.Element)
                throw new OSDException("Expected an element");

            string type = reader.LocalName;
            OSD ret;

            switch (type)
            {
                case "undef":
                    if (reader.IsEmptyElement)
                    {
                        reader.Read();
                        return new OSD();
                    }

                    reader.Read();
                    SkipWhitespace(reader);
                    ret = new OSD();
                    break;
                case "boolean":
                    if (reader.IsEmptyElement)
                    {
                        reader.Read();
                        return OSD.FromBoolean(false);
                    }

                    if (reader.Read())
                    {
                        string s = reader.ReadString().Trim();

                        if (!string.IsNullOrEmpty(s) && (s == "true" || s == "1"))
                        {
                            ret = OSD.FromBoolean(true);
                            break;
                        }
                    }

                    ret = OSD.FromBoolean(false);
                    break;
                case "integer":
                    if (reader.IsEmptyElement)
                    {
                        reader.Read();
                        return OSD.FromInteger(0);
                    }

                    if (reader.Read())
                    {
                        int.TryParse(reader.ReadString().Trim(), out var value);
                        ret = OSD.FromInteger(value);
                        break;
                    }

                    ret = OSD.FromInteger(0);
                    break;
                case "real":
                    if (reader.IsEmptyElement)
                    {
                        reader.Read();
                        return OSD.FromReal(0d);
                    }

                    if (reader.Read())
                    {
                        double value = 0d;
                        string str = reader.ReadString().Trim().ToLower();

                        if (str == "nan")
                            value = double.NaN;
                        else
                            Utils.TryParseDouble(str, out value);

                        ret = OSD.FromReal(value);
                        break;
                    }

                    ret = OSD.FromReal(0d);
                    break;
                case "uuid":
                    if (reader.IsEmptyElement)
                    {
                        reader.Read();
                        return OSD.FromUUID(UUID.Zero);
                    }

                    if (reader.Read())
                    {
                        UUID.TryParse(reader.ReadString().Trim(), out var value);
                        ret = OSD.FromUUID(value);
                        break;
                    }

                    ret = OSD.FromUUID(UUID.Zero);
                    break;
                case "date":
                    if (reader.IsEmptyElement)
                    {
                        reader.Read();
                        return OSD.FromDate(Utils.Epoch);
                    }

                    if (reader.Read())
                    {
                        DateTime.TryParse(reader.ReadString().Trim(), out var value);
                        ret = OSD.FromDate(value);
                        break;
                    }

                    ret = OSD.FromDate(Utils.Epoch);
                    break;
                case "string":
                    if (reader.IsEmptyElement)
                    {
                        reader.Read();
                        return OSD.FromString(string.Empty);
                    }

                    if (reader.Read())
                    {
                        ret = OSD.FromString(reader.ReadString());
                        break;
                    }

                    ret = OSD.FromString(string.Empty);
                    break;
                case "binary":
                    if (reader.IsEmptyElement)
                    {
                        reader.Read();
                        return OSD.FromBinary(Utils.EmptyBytes);
                    }

                    if (reader.GetAttribute("encoding") != null && reader.GetAttribute("encoding") != "base64")
                        throw new OSDException("Unsupported binary encoding: " + reader.GetAttribute("encoding"));

                    if (reader.Read())
                    {
                        try
                        {
                            ret = OSD.FromBinary(Convert.FromBase64String(reader.ReadString().Trim()));
                            break;
                        }
                        catch (FormatException ex)
                        {
                            throw new OSDException("Binary decoding exception: " + ex.Message);
                        }
                    }

                    ret = OSD.FromBinary(Utils.EmptyBytes);
                    break;
                case "uri":
                    if (reader.IsEmptyElement)
                    {
                        reader.Read();
                        return OSD.FromUri(new Uri(string.Empty, UriKind.RelativeOrAbsolute));
                    }

                    if (reader.Read())
                    {
                        ret = OSD.FromUri(new Uri(reader.ReadString(), UriKind.RelativeOrAbsolute));
                        break;
                    }

                    ret = OSD.FromUri(new Uri(string.Empty, UriKind.RelativeOrAbsolute));
                    break;
                case "map":
                    return ParseLLSDXmlMap(reader);
                case "array":
                    return ParseLLSDXmlArray(reader);
                default:
                    reader.Read();
                    ret = null;
                    break;
            }

            if (reader.NodeType != XmlNodeType.EndElement || reader.LocalName != type)
            {
                throw new OSDException("Expected </" + type + ">");
            }

            reader.Read();
            return ret;
        }

        private static OSDMap ParseLLSDXmlMap(XmlTextReader reader)
        {
            if (reader.NodeType != XmlNodeType.Element || reader.LocalName != "map")
                throw new NotImplementedException("Expected <map>");

            OSDMap map = new OSDMap();

            if (reader.IsEmptyElement)
            {
                reader.Read();
                return map;
            }

            if (reader.Read())
            {
                while (true)
                {
                    SkipWhitespace(reader);

                    if (reader.NodeType == XmlNodeType.EndElement && reader.LocalName == "map")
                    {
                        reader.Read();
                        break;
                    }

                    if (reader.NodeType != XmlNodeType.Element || reader.LocalName != "key")
                        throw new OSDException("Expected <key>");

                    string key = reader.ReadString();

                    if (reader.NodeType != XmlNodeType.EndElement || reader.LocalName != "key")
                        throw new OSDException("Expected </key>");

                    if (reader.Read())
                        map[key] = ParseLLSDXmlElement(reader);
                    else
                        throw new OSDException("Failed to parse a value for key " + key);
                }
            }

            return map;
        }

        private static OSDArray ParseLLSDXmlArray(XmlTextReader reader)
        {
            if (reader.NodeType != XmlNodeType.Element || reader.LocalName != "array")
                throw new OSDException("Expected <array>");

            OSDArray array = new OSDArray();

            if (reader.IsEmptyElement)
            {
                reader.Read();
                return array;
            }

            if (reader.Read())
            {
                while (true)
                {
                    SkipWhitespace(reader);

                    if (reader.NodeType == XmlNodeType.EndElement && reader.LocalName == "array")
                    {
                        reader.Read();
                        break;
                    }

                    array.Add(ParseLLSDXmlElement(reader));
                }
            }

            return array;
        }        

        private static void SkipWhitespace(XmlTextReader reader)
        {
            while (
                reader.NodeType == XmlNodeType.Comment ||
                reader.NodeType == XmlNodeType.Whitespace ||
                reader.NodeType == XmlNodeType.SignificantWhitespace ||
                reader.NodeType == XmlNodeType.XmlDeclaration)
            {
                reader.Read();
            }
        }

        private static void CreateLLSDXmlSchema()
        {
            if (XmlSchema == null)
            {
                #region XSD
                const string schemaText = @"
<?xml version=""1.0"" encoding=""utf-8""?>
<xs:schema elementFormDefault=""qualified"" xmlns:xs=""http://www.w3.org/2001/XMLSchema"">
  <xs:import schemaLocation=""xml.xsd"" namespace=""http://www.w3.org/XML/1998/namespace"" />
  <xs:element name=""uri"" type=""xs:string"" />
  <xs:element name=""uuid"" type=""xs:string"" />
  <xs:element name=""KEYDATA"">
    <xs:complexType>
      <xs:sequence>
        <xs:element ref=""key"" />
        <xs:element ref=""DATA"" />
      </xs:sequence>
    </xs:complexType>
  </xs:element>
  <xs:element name=""date"" type=""xs:string"" />
  <xs:element name=""key"" type=""xs:string"" />
  <xs:element name=""boolean"" type=""xs:string"" />
  <xs:element name=""undef"">
    <xs:complexType>
      <xs:sequence>
        <xs:element ref=""EMPTY"" />
      </xs:sequence>
    </xs:complexType>
  </xs:element>
  <xs:element name=""map"">
    <xs:complexType>
      <xs:sequence>
        <xs:element minOccurs=""0"" maxOccurs=""unbounded"" ref=""KEYDATA"" />
      </xs:sequence>
    </xs:complexType>
  </xs:element>
  <xs:element name=""real"" type=""xs:string"" />
  <xs:element name=""ATOMIC"">
    <xs:complexType>
      <xs:choice>
        <xs:element ref=""undef"" />
        <xs:element ref=""boolean"" />
        <xs:element ref=""integer"" />
        <xs:element ref=""real"" />
        <xs:element ref=""uuid"" />
        <xs:element ref=""string"" />
        <xs:element ref=""date"" />
        <xs:element ref=""uri"" />
        <xs:element ref=""binary"" />
      </xs:choice>
    </xs:complexType>
  </xs:element>
  <xs:element name=""DATA"">
    <xs:complexType>
      <xs:choice>
        <xs:element ref=""ATOMIC"" />
        <xs:element ref=""map"" />
        <xs:element ref=""array"" />
      </xs:choice>
    </xs:complexType>
  </xs:element>
  <xs:element name=""llsd"">
    <xs:complexType>
      <xs:sequence>
        <xs:element ref=""DATA"" />
      </xs:sequence>
    </xs:complexType>
  </xs:element>
  <xs:element name=""binary"">
    <xs:complexType>
      <xs:simpleContent>
        <xs:extension base=""xs:string"">
          <xs:attribute default=""base64"" name=""encoding"" type=""xs:string"" />
        </xs:extension>
      </xs:simpleContent>
    </xs:complexType>
  </xs:element>
  <xs:element name=""array"">
    <xs:complexType>
      <xs:sequence>
        <xs:element minOccurs=""0"" maxOccurs=""unbounded"" ref=""DATA"" />
      </xs:sequence>
    </xs:complexType>
  </xs:element>
  <xs:element name=""integer"" type=""xs:string"" />
  <xs:element name=""string"">
    <xs:complexType>
      <xs:simpleContent>
        <xs:extension base=""xs:string"">
          <xs:attribute ref=""xml:space"" />
        </xs:extension>
      </xs:simpleContent>
    </xs:complexType>
  </xs:element>
</xs:schema>
";
                #endregion XSD

                MemoryStream stream = new MemoryStream(Encoding.ASCII.GetBytes(schemaText));

                XmlSchema = new XmlSchema();
                XmlSchema = XmlSchema.Read(stream, new ValidationEventHandler(LLSDXmlSchemaValidationHandler));
            }
        }

        private static void LLSDXmlSchemaValidationHandler(object sender, ValidationEventArgs args)
        {
            string error =
                $"Line: {XmlTextReader.LineNumber} - Position: {XmlTextReader.LinePosition} - {args.Message}";

            if (LastXmlErrors == string.Empty)
                LastXmlErrors = error;
            else
                LastXmlErrors += Environment.NewLine + error;
        }
    }
}
