using System.Xml.Serialization;

namespace xmlDocVta
{
    //[Serializable]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "urn:sunat:names:specification:ubl:peru:schema:xsd:VoidedDocuments-1")]
    [System.Xml.Serialization.XmlRootAttribute("VoidedDocuments", Namespace = "urn:sunat:names:specification:ubl:peru:schema:xsd:VoidedDocuments-1", IsNullable = false)]
   
    public partial class VoidedDocuments
    {
        //[XmlElement("firma", Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2")]
        //public UBLExtensionType firma { get; set; }

        //[XmlElement("UBLExtensions", Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2")]
        //public UBLExtensionType[] UBLExtensions { get; set; }

        private UBLExtensionType[] uBLExtensionsField;
        [System.Xml.Serialization.XmlArrayAttribute(Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2")]
        [System.Xml.Serialization.XmlArrayItemAttribute("UBLExtension", IsNullable = false)]
        public UBLExtensionType[] UBLExtensions
        {
            get
            {
                return this.uBLExtensionsField;
            }
            set
            {
                this.uBLExtensionsField = value;
            }
        }

        private UBLVersionIDType UBLVersionIDField;
        [XmlElement("UBLVersionID", Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2")]
        public UBLVersionIDType UBLVersionID { get { return this.UBLVersionIDField; } set { this.UBLVersionIDField = value; } }

        private CustomizationIDType CustomizationIDField;
        [XmlElement("CustomizationID", Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2")]
        public CustomizationIDType CustomizationID { get { return this.CustomizationIDField; } set { this.CustomizationIDField = value; } }

        private IDType idField;
        [XmlElement("ID", Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2")]
        public IDType ID
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }

        [XmlElement("ReferenceDate", Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2")]
        public string ReferenceDate { get; set; }

        private IssueDateType issueDateField;
        [XmlElement("IssueDate", Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2")]
        //public string IssueDate { get; set; }
        public IssueDateType IssueDate
        {
            get
            {
                return this.issueDateField;
            }
            set
            {
                this.issueDateField = value;
            }
        }

        [XmlElement("Signature", Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2")]
        public SignatureType[] Signature { get; set; }

        [XmlElement("AccountingSupplierParty", Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2")]
        public asp AccountingSupplierParty { get; set; }
        
        [XmlElement("VoidedDocumentsLine", Namespace = "urn:sunat:names:specification:ubl:peru:schema:xsd:SunatAggregateComponents-1")]
        public vdl VoidedDocumentsLine { get; set; }
    }
    public class asp
    {
        [XmlElement("CustomerAssignedAccountID", Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2")]
        public string CustomerAssignedAccountID { get; set; }
        [XmlElement("AdditionalAccountID", Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2")]
        public string AdditionalAccountID { get; set; }
        public par Party { get; set; }
    }
    public class par
    {
        public ple PartyLegalEntity { get; set; }
    }
    public class ple 
    {
        private RegistrationNameType registrationNameField;
        [XmlElement("RegistrationName", Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2")]
        public RegistrationNameType RegistrationName
        {
            get
            {
                return this.registrationNameField;
            }
            set
            {
                this.registrationNameField = value;
            }
        }
    }
    public class vdl
    {
        [XmlElement("LineID", Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2")]
        public string LineID { get; set; }

        private DocumentTypeCodeType DocumentTypeCodeField;
        [XmlElement("DocumentTypeCode", Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2")]
        public DocumentTypeCodeType DocumentTypeCode { get {return this.DocumentTypeCodeField; } set {this.DocumentTypeCodeField = value; } }

        public string DocumentSerialID { get; set; }
        public string DocumentNumberID { get; set; }
        public string VoidReasonDescription { get; set; }
    }
    public class signature     // Signature
    {
        [XmlElement("IDs0", Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2")]
        public string IDs0 { get; set; }

        [XmlElement("SignatoryParty", Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2")]
        public sigPar SignatoryParty { get; set; }

        [XmlElement("DigitalSignatureAttachment", Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2")]
        public digSigAt DigitalSignatureAttachment { get; set; }
    }
    public class sigPar     // SignatoryParty
    {
        [XmlElement("PartyIdentification", Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2")]
        public parIden PartyIdentification { get; set; }

        [XmlElement("PartyName", Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2")]
        public parNam PartyName { get; set; }
    }
    public class parIden        // PartyIdentification
    {
        [XmlElement("IDs1", Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2")]
        public string IDs1 { get; set; }
    }
    public class parNam         // PartyName
    {
        [XmlElement("Name0", Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2")]
        public string Name0 { get; set; }
    }
    public class digSigAt       // DigitalSignatureAttachment
    {
        [XmlElement("ExternalReference", Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2")]
        public extRef ExternalReference { get; set; }
    }
    public class extRef         // ExternalReference
    {
        [XmlElement("URI", Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2")]
        public string URI { get; set; }
    }
}
