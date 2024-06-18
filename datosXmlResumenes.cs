using System;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Serialization;

namespace xmlDocVta
{
    [Serializable]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
    //[System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlRootAttribute("SummaryDocuments", Namespace = "urn:sunat:names:specification:ubl:peru:schema:xsd:SummaryDocuments-1", IsNullable = false)]
    //[System.Xml.Serialization.XmlAnyElement("sdl", Namespace = "urn:sunat:names:specification:ubl:peru:schema:xsd:SummaryDocuments-1")]
    public class parR
    {
        public PartyLegalEntityType PartyLegalEntity { get; set; }
    }
    public class aspR
    {
        [XmlElement("CustomerAssignedAccountID", Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2")]
        public string CustomerAssignedAccountID { get; set; }
        [XmlElement("AdditionalAccountID", Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2")]
        public string AdditionalAccountID { get; set; }
        public parR Party { get; set; }
    }
    public class BillPays
    {
        public PaidAmountType PaidAmount { get; set; }
        public InstructionIDType InstructionID { get; set; }
    };
    public class sdl
    {
        [XmlElement("LineID", Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2")]
        public string LineID { get; set; }

        private DocumentTypeCodeType DocumentTypeCodeField;
        [XmlElement("DocumentTypeCode", Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2")]
        public DocumentTypeCodeType DocumentTypeCode { get { return this.DocumentTypeCodeField; } set { this.DocumentTypeCodeField = value; } }

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
        
        [XmlElement("AccountingCustomerParty", Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2")]
        public aspR AccountingCustomerParty { get; set; }

        [XmlElement("Status", Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2")]
        public StatusType Status { get; set; }

        [XmlAnyElement("TotalAmount", Namespace = "urn:sunat:names:specification:ubl:peru:schema:xsd:SunatAggregateComponents-1")]
        public TotalAmountType TotalAmount { get; set; }

        [XmlElement("BillingPayment", Namespace = "urn:sunat:names:specification:ubl:peru:schema:xsd:SunatAggregateComponents-1")]
        public BillPays[] BillingPayment { get; set; }

        [XmlElement("AllowanceCharge", Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2")]
        public AllowanceChargeType AllowanceCharge { get; set; }

        [XmlElement("TaxTotal", Namespace ="")]
        public TaxTotalType TaxTotal { get; set; }
        
    }

    public partial class SummaryDocuments  //datosXmlResumenes
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

        [XmlElement("ReferenceDate", Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2")]
        public string ReferenceDate { get; set; }

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

        [XmlElement("Signature", Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2")]
        public SignatureType[] Signature { get; set; }

        [XmlElement("AccountingSupplierParty", Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2")]
        public aspR AccountingSupplierParty { get; set; }

        public sdl SummaryDocumentsLine { get; set; }
        
    }
}
