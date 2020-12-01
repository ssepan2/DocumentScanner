using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Linq;
//using System.Xml.Serialization;
using System.Text;
using Ssepan.Utility;

namespace DocumentScannerCommon
{
    /// <summary>
    /// xml-based list of document type objects
    /// </summary>
    public class DocumentType
    {
        #region Declarations
        public const String DATA_FILE_TYPE = "xml";
        public const String DATA_FILE_NAME = "documenttypes";
        #endregion Declarations


        public DocumentType(String name)
        { 
            Name = name;
        }

        #region Persisted Properties
        private String _Name = default(String);
        public String Name
        {
            get { return _Name; }
            set { _Name = value; }
        }
        #endregion Persisted Properties

        #region static methods
        /// <summary>
        /// Loads the current object with data from the specified file.
        /// </summary>
        /// <param name="xml"></param>
        public static List<DocumentType> LoadString(String xml)
        {
            List<DocumentType> returnValue = default(List<DocumentType>);
            XDocument xmlDocument = default(XDocument);

            try
            {
                xmlDocument = new XDocument();
                xmlDocument = XDocument.Parse(xml, LoadOptions.None);
                returnValue =
                    (from xElement in xmlDocument.Root.Elements("DocumentType")
                    select new DocumentType(xElement.Element("Name").Value)).ToList<DocumentType>();

                return returnValue;
            }
            catch (Exception ex)
            {
                Log.Write(ex, MethodBase.GetCurrentMethod(), EventLogEntryType.Error);
                throw;
            }
        }

        /// <summary>
        /// Return a List(Of DocumentType) as defined by DocumentTypes.xml.
        /// </summary>
        /// <returns></returns>
        public static List<DocumentType> GetDocumentTypes()
        {
            List<DocumentType> returnValue = default(List<DocumentType>);
            String xml = String.Empty;

            try
            {
                //get list of document types defined in XML
                xml = DocumentScannerCommon.Properties.Resources.DocumentTypes;
                returnValue = DocumentType.LoadString(xml);
            }
            catch (Exception ex)
            {
                Log.Write(ex, MethodBase.GetCurrentMethod(), EventLogEntryType.Error);
            }
            return returnValue;
        }
        #endregion static methods

        #region Protected Members
        #endregion Protected Members
    }
}
