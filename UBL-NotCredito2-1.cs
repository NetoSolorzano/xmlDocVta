using System.Xml.Serialization;

namespace xmlDocVta
{
    //[Serializable]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CreditNote-2")]
    [System.Xml.Serialization.XmlRootAttribute("CreditNote", Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CreditNote-2", IsNullable = false)]

    public partial class CreditNote
    {
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

        private IssueDateType issueDateField;
        [XmlElement("IssueDate", Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2")]
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

        private IssueTimeType issueTimeField;
        [XmlElement("IssueTime", Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2")]
        public IssueTimeType IssueTime
        {
            get
            {
                return this.issueTimeField;
            }
            set
            {
                this.issueTimeField = value;
            }
        }

        private NoteType NoteField;
        [XmlElement("Note", Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2")]
        public NoteType Note
        {
            get
            {
                return this.NoteField;
            }
            set
            {
                this.NoteField = value;
            }
        }

        private DocumentCurrencyCodeType DocumentCurrencyCodeField;
        [XmlElement("DocumentCurrencyCode", Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2")]
        public DocumentCurrencyCodeType DocumentCurrencyCode { get { return this.DocumentCurrencyCodeField; } set { this.DocumentCurrencyCodeField = value; } }

        [XmlElement("DiscrepancyResponse", Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2")]
        public DisResp DiscrepancyResponse { get; set; }

        [XmlElement("BillingReference", Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2")]
        public BillRef BillingReference { get; set; }

        private SignatureType[] signatureField;
        [System.Xml.Serialization.XmlElementAttribute("Signature", Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2")]
        public SignatureType[] Signature
        {
            get
            {
                return this.signatureField;
            }
            set
            {
                this.signatureField = value;
            }
        }

        [XmlElement("AccountingSupplierParty", Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2")]
        public ASP AccountingSupplierParty { get; set; }

        [XmlElement("AccountingCustomerParty", Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2")]
        public ACP AccountingCustomerParty { get; set; }
        
        [XmlElement("TaxTotal", Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2")]
        public TaxTotalType TaxTotal { get; set; }

        [XmlElement("LegalMonetaryTotal", Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2")]
        public MonetaryTotalType LegalMonetaryTotal { get; set; }

        [XmlElement("CreditNoteLine", Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2")]
        public CreditNoteLineType[] CreditNoteLine { get; set; }
    }
    public class BillRef            // BillingReference
    {
        [XmlElement("InvoiceDocumentReference", Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2")]
        public InvDocRef InvoiceDocumentReference { get; set; }
    }
    public class InvDocRef
    {
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

        private IssueDateType issueDateField;
        [XmlElement("IssueDate", Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2")]
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

        private DocumentTypeCodeType DocumentTypeCodeField;
        [XmlElement("DocumentTypeCode", Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2")]
        public DocumentTypeCodeType DocumentTypeCode
        {
            get
            {
                return this.DocumentTypeCodeField;
            }
            set
            {
                this.DocumentTypeCodeField = value;
            }
        }
    }
    public class DisResp        // DiscrepancyResponse
    {
        private ReferenceIDType ReferenceIDField;
        [XmlElement("ReferenceID", Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2")]
        public ReferenceIDType ReferenceID { get { return this.ReferenceIDField; } set { this.ReferenceIDField = value; } }

        private ResponseCodeType ResponseCodeField;
        [XmlElement("ResponseCode", Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2")]
        public ResponseCodeType ResponseCode { get { return this.ResponseCodeField; } set { this.ResponseCodeField = value; } }

        private DescriptionType DescriptionField;
        [XmlElement("Description", Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2")]
        public DescriptionType Description
        {
            get
            {
                return this.DescriptionField;
            }
            set
            {
                this.DescriptionField = value;
            }
        }
    }
    public class ASP            // AccountingSupplierParty
    {
        [XmlElement("Party", Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2")]
        public part Party { get; set; }
    }
    public class part
    {
        private WebsiteURIType WebsiteURIField;
        [XmlElement("WebsiteURI", Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2")]
        public WebsiteURIType WebsiteURI
        {
            get
            {
                return this.WebsiteURIField;
            }
            set
            {
                this.WebsiteURIField = value;
            }
        }

        [XmlElement("PartyIdentification", Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2")]
        public ParIden PartyIdentification { get; set; }

        [XmlElement("PartyLegalEntity", Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2")]
        public parLegEnt PartyLegalEntity { get; set; }

        [XmlElement("Contact", Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2")]
        public ContactType Contact
        {
            get; set;
        }
    }
    public class ParIden 
    {
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
    }
    public class parLegEnt
    {
        private RegistrationNameType RegistrationNameField;
        [XmlElement("RegistrationName", Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2")]
        public RegistrationNameType RegistrationName
        {
            get
            {
                return this.RegistrationNameField;
            }
            set
            {
                this.RegistrationNameField = value;
            }
        }

        [XmlElement("RegistrationAddress", Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2")]
        public RegAdd RegistrationAddress { get; set; }
    }
    public class RegAdd         // RegistrationAddress
    {
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

        private AddressTypeCodeType AddressTypeCodeField;
        [XmlElement("AddressTypeCode", Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2")]
        public AddressTypeCodeType AddressTypeCode
        {
            get { return this.AddressTypeCodeField; }
            set { this.AddressTypeCodeField = value; }
        }

        private CityNameType CityNameField;
        [XmlElement("CityName", Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2")]
        public CityNameType CityName
        {
            get { return this.CityNameField; }
            set { this.CityNameField = value; }
        }

        private CountrySubentityType CountrySubentityTypeField;
        [XmlElement("CountrySubentity", Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2")]
        public CountrySubentityType CountrySubentity
        {
            get { return this.CountrySubentityTypeField; }
            set { this.CountrySubentityTypeField = value; }
        }

        private DistrictType DistrictTypeField;
        [XmlElement("District", Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2")]
        public DistrictType District { get { return this.DistrictTypeField; } set { this.DistrictTypeField = value; } }

        [XmlElement("AddressLine", Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2")]
        public AddLin AddressLine { get; set; }

        [XmlElement("Country", Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2")]
        public countr Country { get; set; }
    }
    public class AddLin
    {
        private LineType LineTypeField;
        [XmlElement("Line", Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2")]
        public LineType Line { get { return this.LineTypeField; } set { this.LineTypeField = value; } }
    }
    public class countr
    {
        private IdentificationCodeType identificationCodeField;
        [XmlElement("IdentificationCode", Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2")]
        public IdentificationCodeType IdentificationCode
        {
            get
            {
                return this.identificationCodeField;
            }
            set
            {
                this.identificationCodeField = value;
            }
        }
    }
    public class ACP            // AccountingCustomerParty
    {
        [XmlElement("Party", Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2")]
        public part Party { get; set; }
    }
}
