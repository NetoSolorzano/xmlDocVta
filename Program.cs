using System;
using System.IO;
using System.Data.SQLite;
using System.Text;
using System.Xml.Serialization;
using System.Xml;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;

namespace xmlDocVta
{
    class Program
    {
        //static string rutaAct = @"C:\TRANSC_pruebas\TransCarga.db";
        static string rutaAct = Directory.GetCurrentDirectory() + @"\TransCarga.db";    // la base de datos siempre debe llamarse Transcarga.db
        public static string CadenaConexion = $"Data Source={rutaAct}";       // este app debe estar dentro del directorio del sistema Ej. c:/transcarga/xmlDV
        static int Main(string[] args)
        {
            var ruta = args[0];                         // ruta donde se grabará el xml
            var ruce = args[1];                         // ruc del emisor del comprobante
            var docv = args[2];                         // comprobante en formato <serie>-<numero>
            var ifir = args[3].ToLower();               // indicador si se debe firmar | true = si firmar, false = no firmar
            var cert = args[4];                         // ruta y nombre del certificado .pfx
            var clav = args[5];                         // clave del certificado
            var tipg = args[6];                         // tipo de comprobante de venta

            int retorna = 0;   // 0=fallo
            if (ruta != "" && ruce != "" && docv != "")
            {
                Console.WriteLine("Ruta xml: " + ruta);
                Console.WriteLine("Ruc: " + ruce);
                Console.WriteLine("Comprob: " + docv);
                Console.WriteLine("Ruta .db: " + rutaAct);
                Console.WriteLine("Firma?: " + ifir);
                if (docv.Substring(0, 2) == "RA") if (DatosUBLBaja(ruta, ruce, docv, ifir, cert, clav, tipg) == "Exito") retorna = 1;
                if (docv.Substring(0, 2) == "RC") if (DatosUBLResumen(ruta, ruce, docv, ifir, cert, clav, tipg) == "Exito") retorna = 1;
                if (docv.Substring(0, 2) == "FC" || docv.Substring(0, 2) == "BC") if (DatosUBLNota(ruta, ruce, docv, ifir, cert, clav, tipg) == "Exito") retorna = 1;
                if (docv.Substring(0, 2) == "F0" || docv.Substring(0, 2) == "B0") if (DatosUBLDespach(ruta, ruce, docv, ifir, cert, clav, tipg) == "Exito") retorna = 1;
            }
            return retorna;
        }
        private static string DatosUBLNota(string Pruta, string PrucEmi, string PdocuDV, string IndFir, string RAcert, string Clacert, string tipg)
        {
            string retorna = "";
            using (SQLiteConnection cnx = new SQLiteConnection(CadenaConexion))
            {
                cnx.Open();
                string condet = "";
                string consulta = "";
                consulta = "select * from dt_cabnc where NumNotC=@docNC";
                condet = "select * from dt_detnc where NumNotC=@docNC";
                String[,] detalle = new string[9, 12] {
                    { "", "", "", "", "", "", "", "", "", "", "", "" },
                    { "", "", "", "", "", "", "", "", "", "", "", "" },
                    { "", "", "", "", "", "", "", "", "", "", "", "" },
                    { "", "", "", "", "", "", "", "", "", "", "", "" },
                    { "", "", "", "", "", "", "", "", "", "", "", "" },
                    { "", "", "", "", "", "", "", "", "", "", "", "" },
                    { "", "", "", "", "", "", "", "", "", "", "", "" },
                    { "", "", "", "", "", "", "", "", "", "", "", "" },
                    { "", "", "", "", "", "", "", "", "", "", "", "" }
                };
                using (SQLiteCommand micon = new SQLiteCommand(condet, cnx))
                {
                    int cta = 0;
                    micon.Parameters.AddWithValue("@docNC", PdocuDV);
                    using (SQLiteDataReader lite = micon.ExecuteReader())
                    {
                        while (lite.Read())
                        {
                            double ddd = double.Parse(lite["ValUnit"].ToString());
                            double bbb = double.Parse(lite["PreUnit"].ToString());
                            double aaa = double.Parse(lite["Totfila"].ToString());
                            detalle[cta, 0] = (cta + 1).ToString();
                            detalle[cta, 1] = lite["Cantprd"].ToString();        // cantidad de la fila
                            detalle[cta, 2] = lite["DesDet1"].ToString();        // descripción 1
                            detalle[cta, 3] = lite["DesDet2"].ToString();        // descripción 2
                            detalle[cta, 4] = lite["CodIntr"].ToString();        // codigo de servicio ZZ
                            detalle[cta, 5] = lite["ValPeso"].ToString();       // peso
                            detalle[cta, 6] = lite["UniMedS"].ToString();       // codigo unidad medida sunat
                            detalle[cta, 7] = ddd.ToString("#0.00");            // valor unitario
                            detalle[cta, 8] = bbb.ToString("#0.00");            // Precio unitario
                            detalle[cta, 9] = aaa.ToString("#0.00");            // Total fila
                            detalle[cta, 10] = lite["CodPrec"].ToString();      // Código del Tipo de precio SUNAT
                            cta += 1;
                        }
                    }
                }
                using (SQLiteCommand micon = new SQLiteCommand(consulta, cnx))
                {
                    micon.Parameters.AddWithValue("@docNC", PdocuDV);
                    SQLiteDataReader lite = micon.ExecuteReader();
                    if (lite.HasRows == true)
                    {
                        if (lite.Read())
                        {
                            if (tipg == "07")   // notas credito
                            {
                                retorna = UsoUBLnotCred(Pruta, IndFir, RAcert, Clacert,
                                lite["EmisRuc"].ToString(),lite["EmisNom"].ToString(),lite["EmisCom"].ToString(),lite["CodLocA"].ToString(),lite["EmisUbi"].ToString(),
                                lite["EmisDir"].ToString(),lite["EmisDep"].ToString(),lite["EmisPro"].ToString(),lite["EmisDis"].ToString(),lite["EmisUrb"].ToString(),
                                lite["EmisPai"].ToString(),lite["EmisCor"].ToString(),lite["EmisTel"].ToString(),lite["EmisTDoc"].ToString(),lite["EmisWeb"].ToString(),
                                //
                                lite["SeriNot"].ToString(),lite["NumeNot"].ToString(),lite["TipoNot"].ToString(),lite["NumNotC"].ToString(),
                                lite["IdenNot"].ToString(),lite["FecEmis"].ToString(),lite["HorEmis"].ToString(),lite["TipDocu"].ToString(),
                                lite["CodLey1"].ToString(),lite["MonLetr"].ToString(),lite["CodMonS"].ToString(),lite["NtipNot"].ToString(),
                                // datos del cliente
                                lite["DstTipdoc"].ToString(),lite["DstNumdoc"].ToString(),lite["DstNombre"].ToString(),lite["DstDirecc"].ToString(),
                                lite["DstDepart"].ToString(),lite["DstProvin"].ToString(),lite["DstDistri"].ToString(),lite["DstUrbani"].ToString(),
                                lite["DstUbigeo"].ToString(),lite["DstCorre"].ToString(),lite["DstTelef"].ToString(),
                                // Información de importes 
                                "1000",
                                lite["ImpTotImp"].ToString(),lite["ImpOpeGra"].ToString(),lite["ImpIgvTot"].ToString(),lite["ImpOtrosT"].ToString(),
                                lite["TotValVta"].ToString(),lite["TotPreVta"].ToString(),lite["TotDestos"].ToString(),lite["TotOtrCar"].ToString(),
                                lite["TotaVenta"].ToString(),int.Parse(lite["CanFilDet"].ToString()),lite["CondPago"].ToString(),decimal.Parse(lite["TipoCamb"].ToString()),
                                // varios
                                lite["nipfe"].ToString(),lite["restexto"].ToString(),lite["autoriOP"].ToString(),lite["webose"].ToString(),
                                lite["userCrea"].ToString(),lite["nomLocC"].ToString(),lite["desped0"].ToString(),lite["motivoA"].ToString(),
                                lite["modTrans"].ToString(),lite["motiTras"].ToString(),lite["tipoComp"].ToString(),lite["fecEComp"].ToString(),
                                lite["Comprob"].ToString(),lite["rutLogo"].ToString(),lite["rutNomQR"].ToString(),lite["rutNoPdf"].ToString(),
                                lite["porcIgv"].ToString(), lite["codAfIgv"].ToString(), lite["codTrib"].ToString(),detalle);
                            }
                        }
                    }
                    else
                    {
                        // no hay datos
                        //
                    }
                }
            };
            return retorna;
        }
        private static string DatosUBLBaja(string Pruta, string PrucEmi, string DocBaja, string IndFir, string RAcert, string Clacert, string tipg)
        {
            string retorna = "";
            using (SQLiteConnection cnx = new SQLiteConnection(CadenaConexion))
            {
                cnx.Open();
                string consulta = "select * from dt_anula where docBaja=@dbaj";
                using (SQLiteCommand micon = new SQLiteCommand(consulta, cnx))
                {
                    micon.Parameters.AddWithValue("@dbaj", DocBaja);
                    SQLiteDataReader lite = micon.ExecuteReader();
                    if (lite.HasRows == true)
                    {
                        if (lite.Read())
                        {
                            var datDet = new vdl
                            {
                                LineID = lite["Numline"].ToString(),                // id de la linea 
                                DocumentTypeCode = new DocumentTypeCodeType { Value = lite["CodComp"].ToString() },       // tipo de documento a ser anulado
                                DocumentSerialID = lite["SerComp"].ToString(),      // serie del documento a ser anulado
                                DocumentNumberID = lite["NumComp"].ToString(),      // numero del documento a ser anulado
                                VoidReasonDescription = lite["motivoA"].ToString()  // descripción
                            };
                            var legCle = new ple
                            {
                                RegistrationName = new RegistrationNameType { Value = lite["EmisNom"].ToString() }
                            };
                            var parClte = new par
                            {
                                PartyLegalEntity = legCle
                            };
                            var datClte = new asp
                            {
                                CustomerAssignedAccountID = lite["EmisRuc"].ToString(),  // ruc del emisor
                                AdditionalAccountID = lite["EmisTDoc"].ToString(),       // tipo de documento del emisor
                                Party = parClte
                            };
                            var datXml = new VoidedDocuments
                            {
                                UBLVersionID = new UBLVersionIDType { Value = "2.0" },
                                CustomizationID = new CustomizationIDType { Value = "1.0" },
                                ID = new IDType { Value = lite["docBaja"].ToString() },              // identificador de la anulación
                                ReferenceDate = lite["FecDoc"].ToString(),    // fecha del doc. anulado
                                IssueDate = new IssueDateType { Value = DateTime.Parse(lite["FecEmis"].ToString()) },       // fecha de emision de la baja
                                AccountingSupplierParty = datClte,
                                VoidedDocumentsLine = datDet
                            };
                            retorna = UsoUBLBaja(datXml, RAcert, Clacert,
                                Pruta, PrucEmi, lite["EmisNom"].ToString(), DocBaja);
                        }
                    }
                }
            }
            return retorna;
        }
        private static string DatosUBLResumen(string Pruta, string PrucEmi, string DocBaja, string IndFir, string RAcert, string Clacert, string tipg)
        {
            string retorna = "";
            using (SQLiteConnection cnx = new SQLiteConnection(CadenaConexion))
            {
                cnx.Open();
                string consulta = "select dt_cabres.*,dt_detres.* from dt_cabres inner join dt_detres on dt_cabres.docBaja=dt_detres.docBaja where dt_cabres.docBaja=@dbaj";
                using (SQLiteCommand micon = new SQLiteCommand(consulta, cnx))
                {
                    micon.Parameters.AddWithValue("@dbaj", DocBaja);
                    SQLiteDataReader lite = micon.ExecuteReader();
                    if (lite.HasRows == true)
                    {
                        if (lite.Read())
                        {
                            var datClt = new aspR
                            {
                                CustomerAssignedAccountID = lite["NumDCli"].ToString()
                            };
                            var datDet = new sdl
                            {
                                LineID = lite["Numline"].ToString(),                        // id de la linea 
                                ID = new IDType { Value = lite["NumComp"].ToString() },     // numero de la boleta
                                DocumentTypeCode = new DocumentTypeCodeType { Value = lite["CodComp"].ToString() },       // tipo de documento a ser anulado
                                
                                AccountingCustomerParty = datClt,
                                Status = new StatusType { ConditionCode = new ConditionCodeType { Value = lite["EstadoI"].ToString() } },
                                TotalAmount = new TotalAmountType { currencyID = lite["CodMonS"].ToString(), Value = decimal.Parse(lite["ImpTDoc"].ToString()) },
                                BillingPayment = new BillPays[]
                                {
                                    new BillPays { 
                                        PaidAmount = new PaidAmountType { currencyID = lite["CodMonS"].ToString(), Value = decimal.Parse(lite["ImpOpGr"].ToString()) },
                                        InstructionID = new InstructionIDType { Value = "01"}
                                    },      // Total valor de venta - operaciones gravadas
                                    new BillPays
                                    {
                                        PaidAmount = new PaidAmountType { currencyID = lite["CodMonS"].ToString(), Value = decimal.Parse(lite["ImpOpEx"].ToString()) },
                                        InstructionID = new InstructionIDType { Value = "02"}
                                    },      // Total valor de venta - operaciones exoneradas
                                    new BillPays
                                    {
                                        PaidAmount = new PaidAmountType { currencyID = lite["CodMonS"].ToString(), Value = decimal.Parse(lite["ImpOpIn"].ToString()) },
                                        InstructionID = new InstructionIDType { Value = "03"}
                                    }       // Total valor de venta - operaciones inafectas
                                },
                                TaxTotal = new TaxTotalType
                                {
                                    TaxAmount = new TaxAmountType { currencyID = lite["CodMonS"].ToString(), Value = decimal.Parse(lite["ImpTigv"].ToString()) },
                                    TaxSubtotal = new TaxSubtotalType[]
                                    {
                                        new TaxSubtotalType {
                                            TaxAmount = new TaxAmountType { currencyID = lite["CodMonS"].ToString(), Value = decimal.Parse(lite["ImpTigv"].ToString()) },
                                            TaxCategory = new TaxCategoryType { 
                                                TaxScheme = new TaxSchemeType { ID = new IDType { Value = "1000" }, Name = new NameType1 { Value = "IGV" }, TaxTypeCode = new TaxTypeCodeType { Value = "VAT" } } 
                                            }
                                        }
                                    } 
                                }
                            };
                            var parClte = new parR
                            {
                                PartyLegalEntity = new PartyLegalEntityType { RegistrationName = new RegistrationNameType { Value = lite["EmisNom"].ToString() } } //legCle
                            };
                            var datClte = new aspR
                            {
                                CustomerAssignedAccountID = lite["EmisRuc"].ToString(),  // ruc del emisor
                                AdditionalAccountID = lite["EmisTDoc"].ToString(),       // tipo de documento del emisor
                                Party = parClte
                            };
                            var datXml = new SummaryDocuments
                            {
                                UBLVersionID = new UBLVersionIDType { Value = "2.0" },
                                CustomizationID = new CustomizationIDType { Value = "1.1" },
                                ID = new IDType { Value = lite["docBaja"].ToString() },                                     // identificador del resumen
                                IssueDate = new IssueDateType { Value = DateTime.Parse(lite["FecEmis"].ToString()) },       // fecha de generacion del resumen
                                ReferenceDate = lite["FecDoc"].ToString(),                                                  // fecha de emisión de las boletas
                                AccountingSupplierParty = datClte,
                                //SummaryDocumentsLine = { }
                                SummaryDocumentsLine = datDet
                            };
                            retorna = UsoUBLResumen(datXml, RAcert, Clacert,
                                Pruta, PrucEmi, lite["EmisNom"].ToString(), DocBaja);
                        }
                    }
                }
            }
            return retorna;
        }
        private static string UsoUBLResumen(SummaryDocuments datXml, string RAcert, string Clacert,
            string Pruta, string EmisRuc, string NomEmis, string iddoc)
        {
            string retorna = "Fallo";
            Stream fs = new FileStream(Pruta + EmisRuc + "-" + iddoc + ".xml", FileMode.Create, FileAccess.Write);
            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            ns.Add("", "urn:sunat:names:specification:ubl:peru:schema:xsd:SummaryDocuments-1");
            ns.Add("ext", "urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2");
            ns.Add("cbc", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");
            ns.Add("cac", "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2");
            ns.Add("sac", "urn:sunat:names:specification:ubl:peru:schema:xsd:SunatAggregateComponents-1");
            
            var xmlSerializer = new XmlSerializer(typeof(SummaryDocuments));
            XmlDocument xmlDocument = new XmlDocument();
            XmlElement firma = xmlDocument.CreateElement("ext:firma", "urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2");
            UBLExtensionType[] uBLExtensionTypes = new UBLExtensionType[] { new UBLExtensionType { ExtensionContent = firma } };
            datXml.UBLExtensions = uBLExtensionTypes;

            #region FIRMA ELECTRONICA DE LA GUIA ELECTRONICA 
            PartyType partid = new PartyType
            {
                PartyIdentification = new PartyIdentificationType[]
                    {
                        new PartyIdentificationType { ID = new IDType { Value = EmisRuc } }
                    },
                PartyName = new PartyNameType[] { new PartyNameType { Name = new NameType1 { Value = NomEmis } } } // Value = "EXPRESO EL ALTIPLANO S.R.L."
            };
            AttachmentType attach = new AttachmentType
            {
                ExternalReference = new ExternalReferenceType { URI = new URIType { Value = "SigNode" } }
            };
            SignatureType tory = new SignatureType
            {
                ID = new IDType { Value = "SignSOLORSOFT" },
                SignatoryParty = partid,
                DigitalSignatureAttachment = attach
            };
            SignatureType[] signature = new SignatureType[]
            {
                    tory
            };
            #endregion

            //datXml.Signature = signature;
            var oStringWriter = new StringWriter();
            xmlSerializer.Serialize(XmlWriter.Create(oStringWriter), datXml, ns);
            string stringXml = oStringWriter.ToString();
            XmlDocument XmlpaFirmar = new XmlDocument();
            XmlpaFirmar.LoadXml(stringXml);
            using (Stream stream = fs)
            {
                using (XmlWriter xmlWriter = new XmlTextWriter(stream, Encoding.GetEncoding("ISO-8859-1")))
                {
                    FirmarDocumentoXml(XmlpaFirmar, RAcert, Clacert).Save(xmlWriter);
                    Console.WriteLine("Exito -> " + fs.ToString());
                    retorna = "Exito";
                }
            }

            return retorna;
        }

        private static string UsoUBLBaja(VoidedDocuments datXml, string RAcert, string Clacert,
            string Pruta, string EmisRuc, string NomEmis, string iddoc)
        {
            string retorna = "Fallo";
            Stream fs = new FileStream(Pruta + EmisRuc + "-" + iddoc + ".xml", FileMode.Create, FileAccess.Write);

            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            ns.Add("", "urn:sunat:names:specification:ubl:peru:schema:xsd:VoidedDocuments-1");
            ns.Add("cbc", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");
            ns.Add("cac", "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2");
            ns.Add("ds", "http://www.w3.org/2000/09/xmldsig#");
            ns.Add("sac", "urn:sunat:names:specification:ubl:peru:schema:xsd:SunatAggregateComponents-1");
            ns.Add("ext", "urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2");
            ns.Add("xsi", "http://www.w3.org/2001/XMLSchema-instance");
            var xmlSerializer = new XmlSerializer(typeof(VoidedDocuments));

            XmlDocument xmlDocument = new XmlDocument();
            XmlElement firma = xmlDocument.CreateElement("ext:firma", "urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2");
            UBLExtensionType[] uBLExtensionTypes = new UBLExtensionType[] { new UBLExtensionType { ExtensionContent = firma }};
            datXml.UBLExtensions = uBLExtensionTypes;

            #region FIRMA ELECTRONICA DE LA GUIA ELECTRONICA 
            PartyType partid = new PartyType
            {
                PartyIdentification = new PartyIdentificationType[]
                    {
                        new PartyIdentificationType { ID = new IDType { Value = EmisRuc } }
                    },
                PartyName = new PartyNameType[] { new PartyNameType { Name = new NameType1 { Value = NomEmis } } } // Value = "EXPRESO EL ALTIPLANO S.R.L."
            };
            AttachmentType attach = new AttachmentType
            {
                ExternalReference = new ExternalReferenceType { URI = new URIType { Value = "SigNode" } }
            };
            SignatureType tory = new SignatureType
            {
                ID = new IDType { Value = "SignSOLORSOFT" },
                SignatoryParty = partid,
                DigitalSignatureAttachment = attach
            };
            SignatureType[] signature = new SignatureType[]
            {
                    tory
            };
            #endregion

            datXml.Signature = signature;
            var oStringWriter = new StringWriter();
            xmlSerializer.Serialize(XmlWriter.Create(oStringWriter), datXml, ns);
            string stringXml = oStringWriter.ToString();
            XmlDocument XmlpaFirmar = new XmlDocument();
            XmlpaFirmar.LoadXml(stringXml);
            using (Stream stream = fs)
            {
                using (XmlWriter xmlWriter = new XmlTextWriter(stream, Encoding.GetEncoding("ISO-8859-1")))
                {
                    FirmarDocumentoXml(XmlpaFirmar, RAcert, Clacert).Save(xmlWriter);
                    Console.WriteLine("Exito -> " + fs.ToString());
                    retorna = "Exito";
                }
            }
            return retorna;
        }
        private static string DatosUBLDespach(string Pruta, string PrucEmi, string PdocuDV, string IndFir, string RAcert, string Clacert, string tipg)      // ponemos los valores en la clase GRE_T
        {
            string retorna = "";
            using (SQLiteConnection cnx = new SQLiteConnection(CadenaConexion))
            {
                cnx.Open();
                string condet = "";
                string consulta = "";
                consulta = "select * from dt_cabdv where NumDVta=@dvta";
                condet = "select * from dt_detdv where NumDVta=@dvta";
                String[,] detalle = new string[9, 20] { 
                    { "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "" },
                    { "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "" },
                    { "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "" },
                    { "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "" },
                    { "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "" },
                    { "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "" },
                    { "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "" },
                    { "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "" },
                    { "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "" }
                };
                using (SQLiteCommand micon = new SQLiteCommand(condet, cnx))
                {
                    int cta = 0;
                    micon.Parameters.AddWithValue("@dvta", PdocuDV);
                    using (SQLiteDataReader lite = micon.ExecuteReader())
                    {
                        while (lite.Read())
                        {
                            double aaa = double.Parse(lite["ValVtaI"].ToString());
                            double bbb = double.Parse(lite["PreVtaU"].ToString());
                            double ccc = double.Parse(lite["ValIgvI"].ToString());
                            double ddd = double.Parse(lite["ValUnit"].ToString());
                            detalle[cta, 0] = (cta + 1).ToString();
                            detalle[cta, 1] = lite["Cantprd"].ToString();        // cantidad de la fila
                            detalle[cta, 2] = lite["CodMone"].ToString();        // codigo moneda
                            detalle[cta, 3] = aaa.ToString("#0.00");          // lite["ValVtaI"].ToString();        // valor venta del item
                            detalle[cta, 4] = bbb.ToString("#0.00");          // lite["PreVtaU"].ToString();        // precio de venta unitario
                            detalle[cta, 5] = ccc.ToString("#0.00");          // lite["ValIgvI"].ToString();        // monto del igv de la fila
                            detalle[cta, 6] = lite["DesDet1"].ToString();        // descripción 1
                            detalle[cta, 7] = lite["DesDet2"].ToString();        // descripción 2
                            detalle[cta, 8] = lite["CodIntr"].ToString();        // 
                            detalle[cta, 9] = ddd.ToString("#0.00");          // lite["ValUnit"].ToString();        // valor unitario
                            detalle[cta, 10] = lite["ValPeso"].ToString();       // peso
                            detalle[cta, 11] = lite["UniMedS"].ToString();       // codigo unidad medida sunat
                            detalle[cta, 12] = lite["GuiaTra"].ToString();       // numero guía relacionada
                            detalle[cta, 13] = lite["CodTipG"].ToString();       // codigo tipo de guía relacionada
                            detalle[cta, 14] = lite["PorcIgv"].ToString();       // % del igv en números (18)
                            detalle[cta, 15] = lite["CodSunI"].ToString();       // codigo sunat del igv, (10)
                            detalle[cta, 16] = lite["CodSunT"].ToString();       // codigo sunat del tributo, (1000)
                            detalle[cta, 17] = lite["NomSunI"].ToString();       // nombre sunat del impuesto, (IGV)
                            detalle[cta, 18] = lite["NomIntI"].ToString();       // nombre internacional del impuesto, (VAT)
                            detalle[cta, 19] = lite["GuiaRem"].ToString();       // números de guia de remitente
                            cta += 1;
                        }
                    }
                }
                using (SQLiteCommand micon = new SQLiteCommand(consulta, cnx))
                {
                    micon.Parameters.AddWithValue("@dvta", PdocuDV);
                    SQLiteDataReader lite = micon.ExecuteReader();
                    if (lite.HasRows == true)
                    {
                        if (lite.Read())
                        {
                            DVTA_ dVTA = new DVTA_
                            {
                                EmisRuc = lite["EmisRuc"].ToString(),           // ruc emisor del comprobante
                                EmisNom = lite["EmisNom"].ToString(),           // razón social del emisor
                                EmisCom = lite["EmisCom"].ToString(),           // nombre comercial del emisor
                                CodLocA = lite["CodLocA"].ToString(),           // codigo sunat local anexo emisor
                                EmisUbi = lite["EmisUbi"].ToString(),           // ubigeo de la direcc. fiscal del emisor
                                EmisDir = lite["EmisDir"].ToString(),           // direcc. fiscal del emisor
                                EmisDep = lite["EmisDep"].ToString(),           // departamento region del emisor
                                EmisPro = lite["EmisPro"].ToString(),           // provincia del emisor
                                EmisDis = lite["EmisDis"].ToString(),           // distrito del emisor
                                EmisUrb = lite["EmisUrb"].ToString(),           // urbanizacion/barrio del emisor
                                EmisPai = lite["EmisPai"].ToString(),           // código internacional del país del emisor
                                EmisCor = lite["EmisCor"].ToString(),           // correo electrónico del emisor
                                EmisTel = lite["EmisTel"].ToString(),           // telefono del emisor
                                NumDVta = lite["NumDVta"].ToString(),           // serie - número del comprobante
                                FecEmis = lite["FecEmis"].ToString(),           // fecha (aaaa-mm-dd) de la emisión del comprobante
                                HorEmis = lite["HorEmis"].ToString(),           // hora (HH:MM:SS) de la emisión del comprobante
                                CodComp = lite["CodComp"].ToString(),           // codigo del comprobante
                                FecVcto = lite["FecVcto"].ToString(),           // fecha vencimiento del comprobante
                                TipDocu = lite["TipDocu"].ToString(),           // SUNAT:Identificador de Tipo de Documento
                                CodLey1 = lite["CodLey1"].ToString(),           // codigo leyenda del monto en letras
                                MonLetr = lite["MonLetr"].ToString(),           // glosa monto en letras
                                CodMonS = lite["CodMonS"].ToString(),           // código sunat de la moneda
                                DstTipdoc = lite["DstTipdoc"].ToString(),       // Tipo documento sunat del cliente
                                DstNumdoc = lite["DstNumdoc"].ToString(),       // número documento del cliente
                                DstNomTdo = lite["DstNomTdo"].ToString(),       // nombre del tipo de documento sunat
                                DstNombre = lite["DstNombre"].ToString(),       // razón social del cliente
                                DstDirecc = lite["DstDirecc"].ToString(),       // direccion fiscal del cliente
                                DstDepart = lite["DstDepart"].ToString(),       // departamento region del cliente
                                DstProvin = lite["DstProvin"].ToString(),       // provincia del cliente
                                DstDistri = lite["DstDistri"].ToString(),       // distrito del cliente
                                DstUrbani = lite["DstUrbani"].ToString(),       // urbanizacion/barrio del cliente
                                DstUbigeo = lite["DstUbigeo"].ToString(),       // ubigeo direcc fiscal del cliente
                                DstTelef1 = lite["DstTelef"].ToString(),        // teléfono del cliente
                                DstCorreo = lite["DstCorre"].ToString(),        // correo elect del cliente
                                ImpTotImp = lite["ImpTotImp"].ToString(),       // Monto total de impuestos 
                                ImpOpeGra = lite["ImpOpeGra"].ToString(),       // Monto las operaciones gravadas
                                ImpIgvTot = lite["ImpIgvTot"].ToString(),       // Sumatoria de IGV    
                                ImpOtrosT = lite["ImpOtrosT"].ToString(),       // Sumatoria de Otros Tributos
                                IgvCodSun = lite["IgvCodSun"].ToString(),       // schemeAgencyID="6"
                                IgvConInt = lite["IgvConInt"].ToString(),       // código sunat de igv
                                IgvNomSun = lite["IgvNomSun"].ToString(),       // nombre del codigo de impuesto
                                IgvCodInt = lite["IgvCodInt"].ToString(),       // codigo internacional del impuesto igv
                                TotValVta = lite["TotValVta"].ToString(),       // Total valor de venta
                                TotPreVta = lite["TotPreVta"].ToString(),       // Total precio de venta (incluye impuestos)
                                TotDestos = lite["TotDestos"].ToString(),       // Monto total de descuentos del comprobante
                                TotOtrCar = lite["TotOtrCar"].ToString(),       // Monto total de otros cargos
                                TotaVenta = lite["TotaVenta"].ToString(),       // Importe total de la venta, cesión en uso o del servicio prestado
                                CanFilDet = int.Parse(lite["CanFilDet"].ToString()),    // Cantidad filas de detalle
                                CtaDetra = lite["CtaDetra"].ToString(),                 // Cta detracción banco de la nación
                                PorDetra = int.Parse(lite["PorDetra"].ToString()),      // % de la detracción
                                ImpDetra = lite["ImpDetra"].ToString(),                 // Importe de la detracción EN SOLES, la cuenta del BN es el soles
                                GloDetra = lite["GloDetra"].ToString(),                 // Glosa general de la detracción
                                CodTipDet = lite["CodTipDet"].ToString(),               // Código sunat tipo de detraccion (027 transporte de carga)
                                CondPago = lite["CondPago"].ToString(),         // Condicion de pago
                                CodTipOpe = lite["CodTipOpe"].ToString(),       // Tipo de operación 0101=venta interna; 1001=vta interna sujeta a detracción
                                TipoCamb = decimal.Parse(lite["TipoCamb"].ToString()),  // Tipo de cambio
                                // ENCABEZADO-TRASLADOBIENES
                                et_codPaiO = lite["cu_cpapp"].ToString(),            // Código país del punto de origen
                                et_ubiPart = lite["cu_ubipp"].ToString(),            // Ubigeo del punto de partida 
                                et_depPart = lite["cu_deppp"].ToString(),           // Departamento del punto de partida
                                et_proPart = lite["cu_propp"].ToString(),           // Provincia del punto de partida
                                et_disPart = lite["cu_dispp"].ToString(),           // Distrito del punto de partida
                                et_urbPart = lite["cu_urbpp"].ToString(),           // Urbanización del punto de partida
                                et_dirPart = lite["cu_dirpp"].ToString(),          // Dirección detallada del punto de partida
                                et_codPaiD = lite["cu_cppll"].ToString(),            // Código país del punto de llegada
                                et_ubiDest = lite["cu_ubpll"].ToString(),            // Ubigeo del punto de llegada
                                et_depDest = lite["cu_depll"].ToString(),           // Departamento del punto de llegada
                                et_proDest = lite["cu_prpll"].ToString(),           // Provincia del punto de llegada
                                et_disDest = lite["cu_dipll"].ToString(),           // Distrito del punto de llegada
                                et_dirDest = lite["cu_ddpll"].ToString(),          // Dirección detallada del punto de llegada
                                et_placa1 = lite["cu_placa"].ToString(),            // Placa del Vehículo
                                et_confve = lite["cu_confv"].ToString(),            // Configuración vehicular del transporte
                                et_coinsc = lite["cu_coins"].ToString(),           // Constancia de inscripción del vehículo o certificado de habilitación vehicular
                                et_marVe1 = lite["cu_marca"].ToString(),           // Marca del Vehículo
                                et_breVe1 = lite["cu_breve"].ToString(),           // Nro.de licencia de conducir
                                et_rucTra = lite["cu_ructr"].ToString(),           // RUC del transportista
                                et_nomTra = lite["cu_nomtr"].ToString(),            // Razón social del Transportista
                                et_modali = lite["cu_modtr"].ToString(),            // Modalidad de Transporte
                                et_pesobr = lite["cu_pesbr"].ToString(),            // Total Peso Bruto
                                // falta la unidad de medida del peso, en el formulario se especifica que el peso bruto sea en TM
                                et_codMot = lite["cu_motra"].ToString(),            // Código de Motivo de Traslado
                                et_fecTra = lite["cu_fechi"].ToString(),           // Fecha de Inicio de Traslado
                                et_mtcTra = lite["cu_remtc"].ToString(),           // Registro MTC
                                et_docCho = lite["cu_nudch"].ToString(),           // Nro.Documento del conductor
                                et_codCho= lite["cu_tidch"].ToString(),            // Tipo de Documento del conductor
                                et_placa2 = lite["cu_plac2"].ToString(),            // Placa del Vehículo secundario
                                et_indSub = lite["cu_insub"].ToString(),             // Indicador de subcontratación
                                et_cargaU = lite["cu_marCU"].ToString(),             // marca de carga única 1=carga única, 0=carga normal
                                // detalle del comprobante
                                Detalle = detalle
                            };
                            if (tipg == "01" || tipg == "03")   // factura
                            {
                                retorna = UsoUBLDocvta(Pruta, IndFir, RAcert, Clacert,
                                    dVTA.EmisRuc, dVTA.EmisNom, dVTA.EmisCom, dVTA.CodLocA, dVTA.EmisUbi, dVTA.EmisDir, dVTA.EmisDep, dVTA.EmisPro,
                                    dVTA.EmisDis, dVTA.EmisUrb, dVTA.EmisPai, dVTA.EmisCor, dVTA.EmisTel, dVTA.NumDVta, dVTA.FecEmis, dVTA.HorEmis, dVTA.CodComp,
                                    dVTA.FecVcto, dVTA.TipDocu, dVTA.CodLey1, dVTA.MonLetr, dVTA.CodMonS, dVTA.DstTipdoc, dVTA.DstNumdoc, dVTA.DstNomTdo,
                                    dVTA.DstNombre, dVTA.DstDirecc, dVTA.DstDepart, dVTA.DstProvin, dVTA.DstDistri, dVTA.DstUrbani, dVTA.DstUbigeo, dVTA.DstTelef1, dVTA.DstCorreo,
                                    dVTA.ImpTotImp, dVTA.ImpOpeGra, dVTA.ImpIgvTot, dVTA.ImpOtrosT, dVTA.IgvCodSun, dVTA.IgvConInt, dVTA.IgvNomSun,
                                    dVTA.IgvCodInt, dVTA.TotValVta, dVTA.TotPreVta, dVTA.TotDestos, dVTA.TotOtrCar, dVTA.TotaVenta, dVTA.Detalle,
                                    dVTA.CanFilDet, dVTA.CtaDetra, dVTA.PorDetra, dVTA.ImpDetra, dVTA.GloDetra, dVTA.CodTipDet, dVTA.CondPago, dVTA.CodTipOpe, dVTA.TipoCamb,
                                    dVTA.et_codPaiO, dVTA.et_ubiPart, dVTA.et_depPart, dVTA.et_proPart, dVTA.et_disPart, dVTA.et_urbPart, dVTA.et_dirPart, 
                                    dVTA.et_codPaiD, dVTA.et_ubiDest, dVTA.et_depDest, dVTA.et_proDest, dVTA.et_disDest, dVTA.et_dirDest, dVTA.et_placa1, 
                                    dVTA.et_confve, dVTA.et_coinsc, dVTA.et_marVe1, dVTA.et_breVe1, dVTA.et_rucTra, dVTA.et_nomTra, dVTA.et_modali, dVTA.et_pesobr, 
                                    dVTA.et_codMot, dVTA.et_fecTra, dVTA.et_mtcTra, dVTA.et_docCho, dVTA.et_codCho, dVTA.et_placa2, dVTA.et_indSub, dVTA.et_cargaU);
                            }
                        }
                    }
                    else
                    {
                        // no hay datos
                        //
                    }
                }
            };
            return retorna;
        }
        private static string UsoUBLDocvta(string Pruta, string IndFir, string RAcert, string Clacert,
        string EmisRuc, string EmisNom, string EmisCom, string CodLocA, string EmisUbi, string EmisDir, string EmisDep, string EmisPro,
        string EmisDis, string EmisUrb, string EmisPai, string EmisCor, string EmisTel, string NumDVta, string FecEmis, string HorEmis, string CodComp,
        string FecVcto, string TipDocu, string CodLey1, string MonLetr, string CodMonS, string DstTipdoc, string DstNumdoc, string DstNomTdo,
        string DstNombre, string DstDirecc, string DstDepart, string DstProvin, string DstDistri, string DstUrbani, string DstUbigeo, string DstTelef1, string DstCorreo,
        string ImpTotImp, string ImpOpeGra, string ImpIgvTot, string ImpOtrosT, string IgvCodSun, string IgvConInt, string IgvNomSun,
        string IgvCodInt, string TotValVta, string TotPreVta, string TotDestos, string TotOtrCar, string TotaVenta, string[,] deta,
        int nfd, string CtaDetra, int PorDetra, string ImpDetra, string GloDetra, string CodTipDet, string CondPago, string CodTipOpe, decimal TipoCamb,
        string et_codPaiO, string et_ubiPart, string et_depPart, string et_proPart, string et_disPart, string et_urbPart, string et_dirPart, 
        string et_codPaiD, string et_ubiDest, string et_depDest, string et_proDest, string et_disDest, string et_dirDest, string et_placa1,
        string et_confve, string et_coinsc, string et_marVe1, string et_breVe1, string et_rucTra, string et_nomTra, string et_modali, string et_pesobr, 
        string et_codMot, string et_fecTra, string et_mtcTra, string et_docCho, string et_codCho, string et_placa2, string et_indSub, string et_cargaU) // ,string Detalle
        {
            string retorna = "Fallo";
            decimal impDetra = 0;           // calculo de valor credito menos detracción 
            if (CodMonS == "PEN") impDetra = decimal.Parse(ImpDetra);
            else
            {
                impDetra = decimal.Parse(ImpDetra) / TipoCamb;
            }
            #region // DATOS DE EXTENSION DEL DOCUMENTO, acá va principalmente la FIRMA en el caso que el metodo de envío a sunat NO sea SFS
                XmlDocument xmlDocument = new XmlDocument();
                XmlElement firma = xmlDocument.CreateElement("ext:firma", "urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2");  // 31/05/2023
                UBLExtensionType[] uBLExtensionTypes = new UBLExtensionType[] { new UBLExtensionType { ExtensionContent = firma } };        //  ExtensionContent = firma <- 31/05/2023
            #endregion
            //IssueTimeType horEmis = new IssueTimeType { Value = HorEmis }; // DateTime.Parse(HorEmis);
            DateTime fecVcto = DateTime.Parse(FecVcto);
            #region profile del comprobante
            ProfileIDType profile = new ProfileIDType
            {
                schemeName= "SUNAT:Identificador de Tipo de Operación", schemeAgencyName = "PE:SUNAT", 
                schemeURI = "urn:pe: gob: sunat: cpe: see: gem: catalogos: catalogo17", Value = CodTipOpe
            };
            #endregion
            #region tipo comprobante, nombre, moneda
                NoteType noteDet = null;
                if (GloDetra != "")
                {
                    noteDet = new NoteType { Value = GloDetra, languageLocaleID = "2006" };
                }
                InvoiceTypeCodeType type = new InvoiceTypeCodeType 
                { 
                    //Value = TipDocu, listAgencyName = "PE:SUNAT", listName = "SUNAT:Identificador de Tipo de Documento", listURI = "urn:pe: gob: sunat: cpe: see: gem: catalogos: catalogo01" 
                    listID = CodTipOpe, listAgencyName = "PE:SUNAT", listName = "Tipo de Documento", name = "Tipo de Operacion",
                    listURI = "urn:pe:gob:sunat:cpe:see:gem:catalogos:catalogo01", listSchemeURI = "urn:pe:gob:sunat:cpe:see:gem:catalogos:catalogo51", Value = TipDocu
                };
                NoteType[] note = new NoteType[] 
                { 
                    new NoteType { Value = MonLetr, languageLocaleID = CodLey1 },
                    noteDet
                };
                DocumentCurrencyCodeType mone = new DocumentCurrencyCodeType
                {
                    Value = CodMonS, listID= "ISO 4217 Alpha", listName = "Currency", listAgencyName = "United Nations Economic Commission for Europe"
                };
            #endregion
            #region documentosRelacionados - guias
            DocumentReferenceType[] guiasRef = null;
            DocumentReferenceType docref1 = null;
            DocumentReferenceType docref2 = null;
            DocumentReferenceType docref3 = null;
            DocumentReferenceType docref4 = null;
            DocumentReferenceType docref5 = null;
            DocumentReferenceType docref6 = null;
            DocumentReferenceType docref7 = null;
            DocumentReferenceType docref8 = null;
            DocumentReferenceType docref9 = null;
            for (int x = 0; x < 9; x++)
            {
                if (deta[x, 0] != "")
                {
                    if(et_cargaU == "1")
                    {
                        if (deta[x, 0] == "1")
                        { 
                            docref1 = new DocumentReferenceType { ID = new IDType { Value = deta[x, 12] }, DocumentTypeCode = new DocumentTypeCodeType { Value = deta[x, 13] } };
                            docref2 = new DocumentReferenceType { ID = new IDType { Value = deta[x, 19] }, DocumentTypeCode = new DocumentTypeCodeType { Value = "09" } };
                        }
                        if (deta[x, 0] == "2")
                        {
                            docref3 = new DocumentReferenceType { ID = new IDType { Value = deta[x, 12] }, DocumentTypeCode = new DocumentTypeCodeType { Value = deta[x, 13] } };
                            docref4 = new DocumentReferenceType { ID = new IDType { Value = deta[x, 19] }, DocumentTypeCode = new DocumentTypeCodeType { Value = "09" } };
                        }
                        if (deta[x, 0] == "3")
                        {
                            docref5 = new DocumentReferenceType { ID = new IDType { Value = deta[x, 12] }, DocumentTypeCode = new DocumentTypeCodeType { Value = deta[x, 13] } };
                            docref6 = new DocumentReferenceType { ID = new IDType { Value = deta[x, 19] }, DocumentTypeCode = new DocumentTypeCodeType { Value = "09" } };
                        }
                        if (deta[x, 0] == "4")
                        {
                            docref7 = new DocumentReferenceType { ID = new IDType { Value = deta[x, 12] }, DocumentTypeCode = new DocumentTypeCodeType { Value = deta[x, 13] } };
                            docref8 = new DocumentReferenceType { ID = new IDType { Value = deta[x, 19] }, DocumentTypeCode = new DocumentTypeCodeType { Value = "09" } };
                        }
                    }
                    else
                    {
                        if (deta[x, 0] == "1") docref1 = new DocumentReferenceType { ID = new IDType { Value = deta[x, 12] }, DocumentTypeCode = new DocumentTypeCodeType { Value = deta[x, 13] } };
                        if (deta[x, 0] == "2") docref2 = new DocumentReferenceType { ID = new IDType { Value = deta[x, 12] }, DocumentTypeCode = new DocumentTypeCodeType { Value = deta[x, 13] } };
                        if (deta[x, 0] == "3") docref3 = new DocumentReferenceType { ID = new IDType { Value = deta[x, 12] }, DocumentTypeCode = new DocumentTypeCodeType { Value = deta[x, 13] } };
                        if (deta[x, 0] == "4") docref4 = new DocumentReferenceType { ID = new IDType { Value = deta[x, 12] }, DocumentTypeCode = new DocumentTypeCodeType { Value = deta[x, 13] } };
                        if (deta[x, 0] == "5") docref5 = new DocumentReferenceType { ID = new IDType { Value = deta[x, 12] }, DocumentTypeCode = new DocumentTypeCodeType { Value = deta[x, 13] } };
                        if (deta[x, 0] == "6") docref6 = new DocumentReferenceType { ID = new IDType { Value = deta[x, 12] }, DocumentTypeCode = new DocumentTypeCodeType { Value = deta[x, 13] } };
                        if (deta[x, 0] == "7") docref7 = new DocumentReferenceType { ID = new IDType { Value = deta[x, 12] }, DocumentTypeCode = new DocumentTypeCodeType { Value = deta[x, 13] } };
                        if (deta[x, 0] == "8") docref8 = new DocumentReferenceType { ID = new IDType { Value = deta[x, 12] }, DocumentTypeCode = new DocumentTypeCodeType { Value = deta[x, 13] } };
                        if (deta[x, 0] == "9") docref9 = new DocumentReferenceType { ID = new IDType { Value = deta[x, 12] }, DocumentTypeCode = new DocumentTypeCodeType { Value = deta[x, 13] } };
                    }
                }
            };
            guiasRef = new DocumentReferenceType[] { docref1, docref2, docref3, docref4, docref5, docref6, docref7, docref8, docref9 };
            #endregion
            #region FIRMA ELECTRONICA DE LA GUIA ELECTRONICA 
            PartyType partid = new PartyType
                {
                    PartyIdentification = new PartyIdentificationType[]
                    {
                        new PartyIdentificationType { ID = new IDType { Value = EmisRuc } }
                    },
                    PartyName = new PartyNameType[] { new PartyNameType { Name = new NameType1 { Value = EmisNom } } } // Value = nomEmi
                };
                AttachmentType attach = new AttachmentType
                {
                    ExternalReference = new ExternalReferenceType { URI = new URIType { Value = "SigNode" } }
                };
                SignatureType tory = new SignatureType
                {
                    ID = new IDType { Value = "SignSOLORSOFT" },
                    SignatoryParty = partid,
                    DigitalSignatureAttachment = attach
                };
                SignatureType[] signature = new SignatureType[]
                {
                    tory
                };
            #endregion
            #region datos del emisor
            PartyNameType[] nomcom = null;
            if (EmisCom != "")
            {
                nomcom = new PartyNameType[] { new PartyNameType { Name = new NameType1 { Value = EmisCom } } };
            } 
            SupplierPartyType emisor = new SupplierPartyType 
            {
                CustomerAssignedAccountID = new CustomerAssignedAccountIDType { Value = EmisRuc},
                AdditionalAccountID = new AdditionalAccountIDType[] { new AdditionalAccountIDType { Value = "6" } },
                Party = new PartyType 
                { 
                    //PartyName = nomcom,
                    PartyIdentification = new PartyIdentificationType[] 
                    { 
                        new PartyIdentificationType { ID = new IDType   // schemeID = "6" <- esta en duro porque en todos los casos el remitente tiene Ruc
                        { 
                            Value = EmisRuc, schemeID = "6", schemeName = "Documento de Identidad", schemeAgencyName = "PE:SUNAT", schemeURI = "urn:pe:gob:sunat:cpe:see:gem:catalogos:catalogo06"} 
                        }
                    },
                    PartyLegalEntity = new PartyLegalEntityType[] 
                    { 
                        new PartyLegalEntityType 
                        { 
                            RegistrationName = new RegistrationNameType { Value = EmisNom },
                            RegistrationAddress = new AddressType 
                            { 
                                ID = new IDType{ Value = EmisUbi},
                                AddressTypeCode = new AddressTypeCodeType { Value = CodLocA, listName = "Establecimientos anexos", listAgencyName = "PE:SUNAT"} ,
                                CityName = new CityNameType { Value = EmisDep },
                                CountrySubentity = new CountrySubentityType { Value = EmisPro },
                                District = new DistrictType { Value = EmisDis },
                                AddressLine = new AddressLineType[]{ new AddressLineType { Line = new LineType { Value = EmisDir } } },
                                Country = new CountryType{ IdentificationCode = new IdentificationCodeType { Value = EmisPai, listID = "ISO 3166-1", listAgencyName = "United Nations Economic Commission for Europe", listName = "Country" } }
                            }
                        }
                    },
                    Contact = new ContactType { ElectronicMail = new ElectronicMailType { Value = EmisCor }, Telephone = new TelephoneType { Value = EmisTel } }
                }
            };
            #endregion
            #region datos cliente
            CustomerPartyType cliente = new CustomerPartyType
            {
                Party = new PartyType
                {
                    PartyIdentification = new PartyIdentificationType[]
                    {
                        new PartyIdentificationType{ ID = new IDType{ Value = DstNumdoc, schemeID = DstTipdoc, schemeName = "Documento de Identidad", schemeAgencyName = "PE:SUNAT", schemeURI = "urn:pe:gob:sunat:cpe:see:gem:catalogos:catalogo06"} }
                    },
                    PartyLegalEntity = new PartyLegalEntityType[]
                    {
                        new PartyLegalEntityType 
                        { 
                            RegistrationName = new RegistrationNameType{ Value = DstNombre},
                            RegistrationAddress = new AddressType
                            {
                                ID = new IDType { Value = DstUbigeo },
                                CityName = new CityNameType { Value = DstDepart },
                                CountrySubentity = new CountrySubentityType { Value = DstProvin },
                                District = new DistrictType { Value = DstDistri },
                                AddressLine = new AddressLineType[] { new AddressLineType { Line = new LineType { Value = DstDirecc } } },
                                Country = new CountryType { IdentificationCode = new IdentificationCodeType { Value = "PE", listID = "ISO 3166-1", listAgencyName = "United Nations Economic Commission for Europe", listName = "Country" } }
                            }
                        }
                    },
                    Contact = new ContactType { ElectronicMail = new ElectronicMailType { Value = DstCorreo }, Telephone = new TelephoneType { Value = DstTelef1 } }
                }
            };
            #endregion
            #region forma de pago
            PaymentTermsType TipCred = null;
            PaymentTermsType TipDetr = null;
            if (CondPago == "Credito")
            {
                TipCred = new PaymentTermsType
                {
                    ID = new IDType { Value = "FormaPago" },
                    PaymentMeansID = new PaymentMeansIDType[] { new PaymentMeansIDType { Value = "Cuota001" } },
                    Amount = new AmountType2 { Value = Math.Round(Decimal.Parse(TotaVenta) - impDetra, 2), currencyID = CodMonS },
                    PaymentDueDate = new PaymentDueDateType { Value = fecVcto }
                };
            }
            if (CtaDetra != "")
            {
                TipDetr = new PaymentTermsType
                {
                    ID = new IDType { Value = "Detraccion" },
                    PaymentMeansID = new PaymentMeansIDType[] { new PaymentMeansIDType { Value = CodTipDet, schemeName = "Codigo de detraccion", schemeAgencyName = "PE:SUNAT", schemeURI = "urn:pe:gob:sunat:cpe:see:gem:catalogos:catalogo54" } },
                    PaymentPercent = new PaymentPercentType { Value = PorDetra },
                    Amount = new AmountType2 { Value = decimal.Parse(ImpDetra), currencyID = "PEN" } // PEN = soles porque la detracción si o si se paga en soles
                };
            }
            PaymentTermsType[] Tpago = new PaymentTermsType[]
            {
                TipDetr,
                new PaymentTermsType
                {
                    ID = new IDType { Value = "FormaPago" },
                    PaymentMeansID = new PaymentMeansIDType[] { new PaymentMeansIDType { Value = CondPago } },
                    Amount = new AmountType2 { Value = Math.Round(Decimal.Parse(TotaVenta) - impDetra, 2), currencyID = CodMonS }
                },
                TipCred
            };
            #endregion
            #region Delivery Shipment 
            DeliveryType[] despacho = null;
            if (et_cargaU == "1")       // (datos de cargas unicas en toneladas)
            {
                StartDateType finitra = new StartDateType { Value = DateTime.Parse(et_fecTra) };
                ShipmentType shipment = new ShipmentType
                {
                    ID = new IDType { Value = "01" },
                    HandlingCode = new HandlingCodeType { Value = et_codMot, listName = "SUNAT:Indicador de Motivo de Traslado", listAgencyName = "PE:SUNAT", listURI = "urn:pe:gob:sunat:cpe:see:gem:catalogos:catalogo20" },
                    GrossWeightMeasure = new GrossWeightMeasureType { Value = decimal.Parse(et_pesobr), unitCode = "TNE" },
                    ShipmentStage = new ShipmentStageType[]
                    {
                        new ShipmentStageType
                        {
                            TransportModeCode = new TransportModeCodeType { Value = et_modali, listName = "SUNAT:indicador de Modalidad de Transporte", listAgencyName = "PE:SUNAT", listURI = "urn:pe:gob:sunat:cpe:see:gem:catalogos:catalogo18" },
                            TransitPeriod = new PeriodType { StartDate = finitra },
                            CarrierParty = new PartyType[]
                            {
                                new PartyType
                                {
                                    PartyLegalEntity = new PartyLegalEntityType[]
                                    {
                                        new PartyLegalEntityType
                                        {
                                            CompanyID = new CompanyIDType { Value = et_rucTra, schemeID = "6", schemeName = "SUNAT:Indicador de Tipo de Documento de Identidad", schemeAgencyName = "PE:SUNAT", schemeURI = "urn:pe: gob: sunat: cpe: see: gem: catalogos: catalogo06" },
                                            RegistrationName = new RegistrationNameType { Value = et_nomTra }
                                        }
                                    }
                                }
                            },
                            TransportMeans = new TransportMeansType
                            {
                                RegistrationNationalityID = new RegistrationNationalityIDType { Value = et_coinsc },
                                RoadTransport = new RoadTransportType { LicensePlateID = new LicensePlateIDType { Value = et_placa1 } }
                            },
                            DriverPerson = new PersonType[] 
                            { 
                                new PersonType { ID = new IDType { Value = et_docCho, schemeID = "1", schemeName = "SUNAT:Indicador de Tipo de Documento de Identidad", schemeAgencyName = "PE:SUNAT", schemeURI = "urn:pe: gob: sunat: cpe: see: gem: catalogos: catalogo06" } } 
                            }
                        }
                    },
                    Delivery = new DeliveryType
                    {
                        DeliveryAddress = new AddressType
                        {
                            CountrySubentityCode = new CountrySubentityCodeType { Value = et_ubiDest },
                            AddressLine = new AddressLineType[] { new AddressLineType { Line = new LineType { Value = et_dirDest } } }
                        },
                        DeliveryParty = new PartyType { MarkAttentionIndicator = new MarkAttentionIndicatorType { Value = Boolean.Parse(et_indSub) } }
                    },
                    TransportHandlingUnit = new TransportHandlingUnitType[]
                    {
                    new TransportHandlingUnitType { TransportEquipment = new TransportEquipmentType[]{ new TransportEquipmentType { ID = new IDType { Value = et_placa1 } } } }
                    },
                    OriginAddress = new AddressType
                    {
                        CountrySubentityCode = new CountrySubentityCodeType { Value = et_ubiPart },
                        AddressLine = new AddressLineType[] { new AddressLineType { Line = new LineType { Value = et_dirPart } } }
                    },
                    /* 
                    Consignment = new ConsignmentType[]
                    {
                        new ConsignmentType
                        {
                            ID = new IDType { Value = "0" },
                            DeclaredForCarriageValueAmount = new DeclaredForCarriageValueAmountType { Value = 1.0M, currencyID = CodMonS },
                            DeliveryTerms = new DeliveryTermsType { Amount = new AmountType2 { Value = 1.0M, currencyID = CodMonS } },
                            TransportHandlingUnit = new TransportHandlingUnitType[]
                            {
                                new TransportHandlingUnitType
                                {
                                    TransportEquipment = new TransportEquipmentType[]
                                    {
                                        new TransportEquipmentType
                                        {
                                            SizeTypeCode = new SizeTypeCodeType { Value = et_confve, listAgencyName = "PE:MTC", listName = "Configuracion Vehícular" },
                                            ReturnabilityIndicator = new ReturnabilityIndicatorType { Value = true },       // no esta claro esto, alguna parte retorna?
                                            Delivery = new DeliveryType { DeliveryTerms = new DeliveryTermsType[] { new DeliveryTermsType { Amount = new AmountType2 { Value = Decimal.Parse(TotaVenta), currencyID = CodMonS } } } }   // que valor va aca ?
                                        }
                                    },
                                    MeasurementDimension = new DimensionType[]
                                    {
                                        new DimensionType{ AttributeID = new AttributeIDType { Value = "01"}, Measure = new MeasureType2{ Value = 1.0M, unitCode = "TNE"} },
                                        new DimensionType{ AttributeID = new AttributeIDType { Value = "02"}, Measure = new MeasureType2{ Value = 1.0M, unitCode = "TNE"} }
                                    }
                                }
                            }
                        }
                    } */
                };
                despacho = new DeliveryType[]
                {
                    new DeliveryType
                    {
                        DeliveryLocation = new LocationType1
                        {
                            Address = new AddressType
                            {
                                ID = new IDType {  Value = et_ubiDest, schemeAgencyName = "PE:INEI", schemeName = "Ubigeos" },
                                AddressLine = new AddressLineType[] { new AddressLineType {  Line = new LineType {  Value = et_dirDest } } },
                                /*
                                StreetName = new StreetNameType { Value = et_dirDest },
                                CitySubdivisionName = new CitySubdivisionNameType { Value = "-" },
                                CityName = new CityNameType { Value = et_proDest },
                                CountrySubentity = new CountrySubentityType { Value = et_depDest },
                                CountrySubentityCode = new CountrySubentityCodeType { Value = et_ubiDest },
                                District = new DistrictType { Value = et_disDest },
                                Country = new CountryType { IdentificationCode = new IdentificationCodeType { Value = "PE", listID = "ISO 3166-1", listAgencyName = "United Nations Economic Commission for Europe", listName = "Country" } }
                                */
                            }
                        },
                        Despatch = new DespatchType
                        {
                            Instructions = new InstructionsType[] { new InstructionsType { Value = "Transporte Consolidado" } },
                            DespatchAddress = new AddressType
                            {
                                ID = new IDType { Value = et_ubiPart, schemeAgencyName = "PE:INEI", schemeName = "Ubigeos" },
                                AddressLine = new AddressLineType[] {new AddressLineType { Line = new LineType { Value = et_dirPart} } }
                            }
                        },
                        DeliveryTerms = new DeliveryTermsType[]
                        {
                            new DeliveryTermsType { ID = new IDType { Value = "01"}, Amount = new AmountType2 { Value = 1, currencyID = "PEN" } },    // Value = 1, currencyID = CodMonS
                            new DeliveryTermsType { ID = new IDType { Value = "02"}, Amount = new AmountType2 { Value = 1, currencyID = "PEN" } },   // Value = 1, currencyID = CodMonS
                            new DeliveryTermsType { ID = new IDType { Value = "03"}, Amount = new AmountType2 { Value = 1, currencyID = "PEN" } }   // Value = 1, currencyID = CodMonS
                        },
                        Shipment = shipment
                    }
                };
            }
            else
            {
                ShipmentType shipment = new ShipmentType
                {
                    ID = new IDType { Value = "01" },
                    /*
                    Consignment = new ConsignmentType[]
                    {
                        new ConsignmentType
                        {
                            ID = new IDType { Value = "0" },
                            DeclaredForCarriageValueAmount = new DeclaredForCarriageValueAmountType { Value = 1.0M, currencyID = CodMonS },
                            DeliveryTerms = new DeliveryTermsType { Amount = new AmountType2 { Value = 1.0M, currencyID = CodMonS } },
                            TransportHandlingUnit = new TransportHandlingUnitType[]
                            {
                                new TransportHandlingUnitType
                                {
                                    TransportEquipment = new TransportEquipmentType[]
                                    { 
                                        new TransportEquipmentType 
                                        {
                                            SizeTypeCode = new SizeTypeCodeType { Value = et_confve, listAgencyName = "PE:MTC", listName = "Configuracion Vehícular" },
                                            ReturnabilityIndicator = new ReturnabilityIndicatorType { Value = true },       // no esta claro esto, alguna parte retorna?
                                            Delivery = new DeliveryType { DeliveryTerms = new DeliveryTermsType[] { new DeliveryTermsType { Amount = new AmountType2 { Value = 0, currencyID = CodMonS } } } }
                                        }

                                    },
                                    MeasurementDimension = new DimensionType[]
                                    {
                                        new DimensionType{ AttributeID = new AttributeIDType { Value = "01"}, Measure = new MeasureType2{ Value = 1.0M, unitCode = "TNE"} },
                                        new DimensionType{ AttributeID = new AttributeIDType { Value = "02"}, Measure = new MeasureType2{ Value = 1.0M, unitCode = "TNE"} }
                                    }
                                }
                            }
                        }
                    } */
                };
                despacho = new DeliveryType[]
                {
                    new DeliveryType
                    {
                        DeliveryLocation = new LocationType1
                        {
                            Address = new AddressType
                            {
                                ID = new IDType {  Value = et_ubiDest, schemeAgencyName = "PE:INEI", schemeName = "Ubigeos" },
                                AddressLine = new AddressLineType[] { new AddressLineType {  Line = new LineType {  Value = et_dirDest } } },
                                /*
                                StreetName = new StreetNameType { Value = et_dirDest },
                                CitySubdivisionName = new CitySubdivisionNameType { Value = "-" },
                                CityName = new CityNameType { Value = et_proDest },
                                CountrySubentity = new CountrySubentityType { Value = et_depDest },
                                CountrySubentityCode = new CountrySubentityCodeType { Value = et_ubiDest },
                                District = new DistrictType { Value = et_disDest },
                                Country = new CountryType { IdentificationCode = new IdentificationCodeType { Value = "PE", listID = "ISO 3166-1", listAgencyName = "United Nations Economic Commission for Europe", listName = "Country" } }
                                */
                            }
                        },
                        Despatch = new DespatchType
                        {
                            Instructions = new InstructionsType[] { new InstructionsType { Value = "Transporte Consolidado" } },
                            DespatchAddress = new AddressType
                            {
                                ID = new IDType { Value = et_ubiPart, schemeAgencyName = "PE:INEI", schemeName = "Ubigeos" },
                                AddressLine = new AddressLineType[] {new AddressLineType { Line = new LineType { Value = et_dirPart} } }
                            }
                        },
                        DeliveryTerms = new DeliveryTermsType[]
                        {
                            new DeliveryTermsType { ID = new IDType { Value = "01"}, Amount = new AmountType2 { Value = 1, currencyID = "PEN" } },    // Valor referencial del servicio de transporte 
                            new DeliveryTermsType { ID = new IDType { Value = "02"}, Amount = new AmountType2 { Value = 1, currencyID = "PEN" } },    // Valor referencial sobre la carga efectiva
                            new DeliveryTermsType { ID = new IDType { Value = "03"}, Amount = new AmountType2 { Value = 1, currencyID = "PEN" } }     // Valor referencial sobre la carga útil nominal
                        },
                        Shipment = shipment
                    }
                };
            }
            #endregion
            #region impuestos totales
            TaxTotalType[] impuestos = new TaxTotalType[]
            {
                new TaxTotalType
                {
                    TaxAmount = new TaxAmountType { currencyID = CodMonS , Value = decimal.Parse(ImpTotImp) },
                    TaxSubtotal = new TaxSubtotalType[]
                    {
                        new TaxSubtotalType
                        {
                            TaxableAmount = new TaxableAmountType { currencyID = CodMonS, Value = decimal.Parse(TotValVta) },   // TotaVenta
                            TaxAmount = new TaxAmountType { currencyID = CodMonS, Value = decimal.Parse(ImpTotImp) },
                            TaxCategory = new TaxCategoryType { 
                                TaxScheme = new TaxSchemeType
                                {
                                    TaxTypeCode = new TaxTypeCodeType { Value = IgvCodInt},
                                    ID = new IDType { Value = IgvConInt, schemeName = "Codigo de tributos", schemeAgencyName = "PE:SUNAT", schemeURI = "urn:pe:gob:sunat:cpe:see:gem:catalogos:catalogo05" },
                                    Name = new NameType1 { Value = IgvNomSun}
                                }
                            }
                        }
                    }
                }
            };
            #endregion
            #region totales del comprobante
            MonetaryTotalType totales = new MonetaryTotalType 
            { 
                LineExtensionAmount = new LineExtensionAmountType { Value = decimal.Parse(TotValVta), currencyID = CodMonS },
                TaxInclusiveAmount = new TaxInclusiveAmountType { Value = decimal.Parse(TotaVenta), currencyID = CodMonS },
                PayableAmount = new PayableAmountType { Value = decimal.Parse(TotaVenta), currencyID = CodMonS }
            };
            #endregion
            #region Detalle del comprobante
            InvoiceLineType[] detalle = null;
            InvoiceLineType detIL = null;
            InvoiceLineType detIL2 = null;
            InvoiceLineType detIL3 = null;
            InvoiceLineType detIL4 = null;
            InvoiceLineType detIL5 = null;
            InvoiceLineType detIL6 = null;
            InvoiceLineType detIL7 = null;
            InvoiceLineType detIL8 = null;
            InvoiceLineType detIL9 = null;
            for (int x = 0; x < 9; x++)
            {
                if (deta[x, 0] != "")
                {
                    decimal v_cant = decimal.Parse(deta[x, 1]);
                    string v_umed = deta[x, 11];
                    decimal v_pptn = decimal.Parse(deta[x, 4]);
                    decimal v_psig = decimal.Parse(deta[x, 9]);
                    if (et_cargaU == "1")
                    {
                        //v_cant = decimal.Parse(et_pesobr);  // tiene que ser el peso de la GR
                        v_cant = decimal.Parse(deta[x, 10]);    // el peso de la fila del detalle tiene que ser en TN y debe ser el correcto
                        v_umed = "TNE";
                        v_pptn = Math.Round(decimal.Parse(deta[x, 4]) / v_cant, 5);
                        v_psig = Math.Round(decimal.Parse(deta[x, 9]) / v_cant, 5);
                    }
                    if (deta[x, 0] == "1")
                    {
                        detIL = new InvoiceLineType
                        {
                            ID = new IDType { Value = deta[x, 0] },
                            InvoicedQuantity = new InvoicedQuantityType { Value = v_cant, unitCode = v_umed },
                            LineExtensionAmount = new LineExtensionAmountType { Value = decimal.Parse(deta[x, 9]), currencyID = deta[x, 2] },
                            PricingReference = new PricingReferenceType
                            {
                                AlternativeConditionPrice = new PriceType[]
                                    {
                                    new PriceType
                                    {
                                        PriceAmount = new PriceAmountType { Value = v_pptn, currencyID = deta[x, 2] },
                                        PriceTypeCode = new PriceTypeCodeType { listName = "Tipo de Precio", listAgencyName = "PE:SUNAT", listURI = "urn:pe:gob:sunat:cpe:see:gem:catalogos:catalogo16", Value = "01"}
                                    }
                                    }
                            },
                            Delivery = despacho,
                            TaxTotal = new TaxTotalType[]
                                {
                                new TaxTotalType
                                {
                                    TaxAmount = new TaxAmountType { Value = decimal.Parse(deta[x, 5]), currencyID = deta[x, 2] },
                                    TaxSubtotal = new TaxSubtotalType[]
                                    {
                                        new TaxSubtotalType
                                        {
                                            TaxableAmount = new TaxableAmountType { Value = decimal.Parse(deta[x, 9]), currencyID = deta[x, 2] },
                                            TaxAmount = new TaxAmountType { Value = decimal.Parse(deta[x, 5]), currencyID = deta[x, 2] },
                                            TaxCategory = new TaxCategoryType
                                            {
                                                Percent = new PercentType1{ Value = decimal.Parse(deta[x, 14]) },
                                                TaxExemptionReasonCode = new TaxExemptionReasonCodeType { Value = deta[x, 15], listAgencyName = "PE:SUNAT", listName = "Afectacion del IGV", listURI = "urn:pe:gob:sunat:cpe:see:gem:catalogos:catalogo07"},
                                                TaxScheme = new TaxSchemeType
                                                {
                                                    ID = new IDType { Value = deta[x, 16], schemeName = "Codigo de tributos", schemeAgencyName = "PE:SUNAT", schemeURI = "urn:pe:gob:sunat:cpe:see:gem:catalogos:catalogo05"},
                                                    Name = new NameType1 { Value = deta[x, 17] },
                                                    TaxTypeCode = new TaxTypeCodeType { Value = deta[x, 18] }
                                                }
                                            }
                                        }
                                    }
                                }
                                },
                            Item = new ItemType
                            {
                                Description = new DescriptionType[] { new DescriptionType { Value = deta[x, 6] + " " + deta[x, 7] } },
                                SellersItemIdentification = new ItemIdentificationType { ID = new IDType { Value = "-" } }
                            },
                            Price = new PriceType { PriceAmount = new PriceAmountType { currencyID = deta[x, 2], Value = v_psig } }
                        };
                    }
                    if (deta[x, 0] == "2")
                    {
                        detIL2 = new InvoiceLineType
                        {
                            ID = new IDType { Value = deta[x, 0] },
                            InvoicedQuantity = new InvoicedQuantityType { Value = v_cant, unitCode = v_umed },
                            LineExtensionAmount = new LineExtensionAmountType { Value = decimal.Parse(deta[x, 9]), currencyID = deta[x, 2] },
                            PricingReference = new PricingReferenceType
                            {
                                AlternativeConditionPrice = new PriceType[]
                                    {
                                    new PriceType
                                    {
                                        PriceAmount = new PriceAmountType { Value = v_pptn, currencyID = deta[x, 2] },
                                        PriceTypeCode = new PriceTypeCodeType { listName = "Tipo de Precio", listAgencyName = "PE:SUNAT", listURI = "urn:pe:gob:sunat:cpe:see:gem:catalogos:catalogo16", Value = "01"}
                                    }
                                    }
                            },
                            Delivery = despacho,
                            TaxTotal = new TaxTotalType[]
                                {
                                new TaxTotalType
                                {
                                    TaxAmount = new TaxAmountType { Value = decimal.Parse(deta[x, 5]), currencyID = deta[x, 2] },
                                    TaxSubtotal = new TaxSubtotalType[]
                                    {
                                        new TaxSubtotalType
                                        {
                                            TaxableAmount = new TaxableAmountType { Value = decimal.Parse(deta[x, 9]), currencyID = deta[x, 2] },
                                            TaxAmount = new TaxAmountType { Value = decimal.Parse(deta[x, 5]), currencyID = deta[x, 2] },
                                            TaxCategory = new TaxCategoryType
                                            {
                                                Percent = new PercentType1{ Value = decimal.Parse(deta[x, 14]) },
                                                TaxExemptionReasonCode = new TaxExemptionReasonCodeType { Value = deta[x, 15], listAgencyName = "PE:SUNAT", listName = "Afectacion del IGV", listURI = "urn:pe:gob:sunat:cpe:see:gem:catalogos:catalogo07"},
                                                TaxScheme = new TaxSchemeType
                                                {
                                                    ID = new IDType { Value = deta[x, 16], schemeName = "Codigo de tributos", schemeAgencyName = "PE:SUNAT", schemeURI = "urn:pe:gob:sunat:cpe:see:gem:catalogos:catalogo05"},
                                                    Name = new NameType1 { Value = deta[x, 17] },
                                                    TaxTypeCode = new TaxTypeCodeType { Value = deta[x, 18] }
                                                }
                                            }
                                        }
                                    }
                                }
                                },
                            Item = new ItemType
                            {
                                Description = new DescriptionType[] { new DescriptionType { Value = deta[x, 6] + " " + deta[x, 7] } },
                                SellersItemIdentification = new ItemIdentificationType { ID = new IDType { Value = "-" } }
                            },
                            Price = new PriceType { PriceAmount = new PriceAmountType { currencyID = deta[x, 2], Value = v_psig } }
                        };
                    }
                    if (deta[x, 0] == "3")
                    {
                        detIL3 = new InvoiceLineType
                        {
                            ID = new IDType { Value = deta[x, 0] },
                            InvoicedQuantity = new InvoicedQuantityType { Value = v_cant, unitCode = v_umed },
                            LineExtensionAmount = new LineExtensionAmountType { Value = decimal.Parse(deta[x, 9]), currencyID = deta[x, 2] },
                            PricingReference = new PricingReferenceType
                            {
                                AlternativeConditionPrice = new PriceType[]
                                    {
                                    new PriceType
                                    {
                                        PriceAmount = new PriceAmountType { Value = v_pptn, currencyID = deta[x, 2] },
                                        PriceTypeCode = new PriceTypeCodeType { listName = "Tipo de Precio", listAgencyName = "PE:SUNAT", listURI = "urn:pe:gob:sunat:cpe:see:gem:catalogos:catalogo16", Value = "01"}
                                    }
                                    }
                            },
                            Delivery = despacho,
                            TaxTotal = new TaxTotalType[]
                                {
                                new TaxTotalType
                                {
                                    TaxAmount = new TaxAmountType { Value = decimal.Parse(deta[x, 5]), currencyID = deta[x, 2] },
                                    TaxSubtotal = new TaxSubtotalType[]
                                    {
                                        new TaxSubtotalType
                                        {
                                            TaxableAmount = new TaxableAmountType { Value = decimal.Parse(deta[x, 9]), currencyID = deta[x, 2] },
                                            TaxAmount = new TaxAmountType { Value = decimal.Parse(deta[x, 5]), currencyID = deta[x, 2] },
                                            TaxCategory = new TaxCategoryType
                                            {
                                                Percent = new PercentType1{ Value = decimal.Parse(deta[x, 14]) },
                                                TaxExemptionReasonCode = new TaxExemptionReasonCodeType { Value = deta[x, 15], listAgencyName = "PE:SUNAT", listName = "Afectacion del IGV", listURI = "urn:pe:gob:sunat:cpe:see:gem:catalogos:catalogo07"},
                                                TaxScheme = new TaxSchemeType
                                                {
                                                    ID = new IDType { Value = deta[x, 16], schemeName = "Codigo de tributos", schemeAgencyName = "PE:SUNAT", schemeURI = "urn:pe:gob:sunat:cpe:see:gem:catalogos:catalogo05"},
                                                    Name = new NameType1 { Value = deta[x, 17] },
                                                    TaxTypeCode = new TaxTypeCodeType { Value = deta[x, 18] }
                                                }
                                            }
                                        }
                                    }
                                }
                                },
                            Item = new ItemType
                            {
                                Description = new DescriptionType[] { new DescriptionType { Value = deta[x, 6] + " " + deta[x, 7] } },
                                SellersItemIdentification = new ItemIdentificationType { ID = new IDType { Value = "-" } }
                            },
                            Price = new PriceType { PriceAmount = new PriceAmountType { currencyID = deta[x, 2], Value = v_psig } }
                        };
                    }
                    if (deta[x, 0] == "4")
                    {
                        detIL4 = new InvoiceLineType
                        {
                            ID = new IDType { Value = deta[x, 0] },
                            InvoicedQuantity = new InvoicedQuantityType { Value = v_cant, unitCode = v_umed },
                            LineExtensionAmount = new LineExtensionAmountType { Value = decimal.Parse(deta[x, 9]), currencyID = deta[x, 2] },
                            PricingReference = new PricingReferenceType
                            {
                                AlternativeConditionPrice = new PriceType[]
                                    {
                                    new PriceType
                                    {
                                        PriceAmount = new PriceAmountType { Value = v_pptn, currencyID = deta[x, 2] },
                                        PriceTypeCode = new PriceTypeCodeType { listName = "Tipo de Precio", listAgencyName = "PE:SUNAT", listURI = "urn:pe:gob:sunat:cpe:see:gem:catalogos:catalogo16", Value = "01"}
                                    }
                                    }
                            },
                            Delivery = despacho,
                            TaxTotal = new TaxTotalType[]
                                {
                                new TaxTotalType
                                {
                                    TaxAmount = new TaxAmountType { Value = decimal.Parse(deta[x, 5]), currencyID = deta[x, 2] },
                                    TaxSubtotal = new TaxSubtotalType[]
                                    {
                                        new TaxSubtotalType
                                        {
                                            TaxableAmount = new TaxableAmountType { Value = decimal.Parse(deta[x, 9]), currencyID = deta[x, 2] },
                                            TaxAmount = new TaxAmountType { Value = decimal.Parse(deta[x, 5]), currencyID = deta[x, 2] },
                                            TaxCategory = new TaxCategoryType
                                            {
                                                Percent = new PercentType1{ Value = decimal.Parse(deta[x, 14]) },
                                                TaxExemptionReasonCode = new TaxExemptionReasonCodeType { Value = deta[x, 15], listAgencyName = "PE:SUNAT", listName = "Afectacion del IGV", listURI = "urn:pe:gob:sunat:cpe:see:gem:catalogos:catalogo07"},
                                                TaxScheme = new TaxSchemeType
                                                {
                                                    ID = new IDType { Value = deta[x, 16], schemeName = "Codigo de tributos", schemeAgencyName = "PE:SUNAT", schemeURI = "urn:pe:gob:sunat:cpe:see:gem:catalogos:catalogo05"},
                                                    Name = new NameType1 { Value = deta[x, 17] },
                                                    TaxTypeCode = new TaxTypeCodeType { Value = deta[x, 18] }
                                                }
                                            }
                                        }
                                    }
                                }
                                },
                            Item = new ItemType
                            {
                                Description = new DescriptionType[] { new DescriptionType { Value = deta[x, 6] + " " + deta[x, 7] } },
                                SellersItemIdentification = new ItemIdentificationType { ID = new IDType { Value = "-" } }
                            },
                            Price = new PriceType { PriceAmount = new PriceAmountType { currencyID = deta[x, 2], Value = v_psig } }
                        };
                    }
                    if (deta[x, 0] == "5")
                    {
                        detIL5 = new InvoiceLineType
                        {
                            ID = new IDType { Value = deta[x, 0] },
                            InvoicedQuantity = new InvoicedQuantityType { Value = v_cant, unitCode = v_umed },
                            LineExtensionAmount = new LineExtensionAmountType { Value = decimal.Parse(deta[x, 9]), currencyID = deta[x, 2] },
                            PricingReference = new PricingReferenceType
                            {
                                AlternativeConditionPrice = new PriceType[]
                                    {
                                    new PriceType
                                    {
                                        PriceAmount = new PriceAmountType { Value = v_pptn, currencyID = deta[x, 2] },
                                        PriceTypeCode = new PriceTypeCodeType { listName = "Tipo de Precio", listAgencyName = "PE:SUNAT", listURI = "urn:pe:gob:sunat:cpe:see:gem:catalogos:catalogo16", Value = "01"}
                                    }
                                    }
                            },
                            Delivery = despacho,
                            TaxTotal = new TaxTotalType[]
                                {
                                new TaxTotalType
                                {
                                    TaxAmount = new TaxAmountType { Value = decimal.Parse(deta[x, 5]), currencyID = deta[x, 2] },
                                    TaxSubtotal = new TaxSubtotalType[]
                                    {
                                        new TaxSubtotalType
                                        {
                                            TaxableAmount = new TaxableAmountType { Value = decimal.Parse(deta[x, 9]), currencyID = deta[x, 2] },
                                            TaxAmount = new TaxAmountType { Value = decimal.Parse(deta[x, 5]), currencyID = deta[x, 2] },
                                            TaxCategory = new TaxCategoryType
                                            {
                                                Percent = new PercentType1{ Value = decimal.Parse(deta[x, 14]) },
                                                TaxExemptionReasonCode = new TaxExemptionReasonCodeType { Value = deta[x, 15], listAgencyName = "PE:SUNAT", listName = "Afectacion del IGV", listURI = "urn:pe:gob:sunat:cpe:see:gem:catalogos:catalogo07"},
                                                TaxScheme = new TaxSchemeType
                                                {
                                                    ID = new IDType { Value = deta[x, 16], schemeName = "Codigo de tributos", schemeAgencyName = "PE:SUNAT", schemeURI = "urn:pe:gob:sunat:cpe:see:gem:catalogos:catalogo05"},
                                                    Name = new NameType1 { Value = deta[x, 17] },
                                                    TaxTypeCode = new TaxTypeCodeType { Value = deta[x, 18] }
                                                }
                                            }
                                        }
                                    }
                                }
                                },
                            Item = new ItemType
                            {
                                Description = new DescriptionType[] { new DescriptionType { Value = deta[x, 6] + " " + deta[x, 7] } },
                                SellersItemIdentification = new ItemIdentificationType { ID = new IDType { Value = "-" } }
                            },
                            Price = new PriceType { PriceAmount = new PriceAmountType { currencyID = deta[x, 2], Value = v_psig } }
                        };
                    }
                    if (deta[x, 0] == "6")
                    {
                        detIL6 = new InvoiceLineType
                        {
                            ID = new IDType { Value = deta[x, 0] },
                            InvoicedQuantity = new InvoicedQuantityType { Value = v_cant, unitCode = v_umed },
                            LineExtensionAmount = new LineExtensionAmountType { Value = decimal.Parse(deta[x, 9]), currencyID = deta[x, 2] },
                            PricingReference = new PricingReferenceType
                            {
                                AlternativeConditionPrice = new PriceType[]
                                    {
                                    new PriceType
                                    {
                                        PriceAmount = new PriceAmountType { Value = v_pptn, currencyID = deta[x, 2] },
                                        PriceTypeCode = new PriceTypeCodeType { listName = "Tipo de Precio", listAgencyName = "PE:SUNAT", listURI = "urn:pe:gob:sunat:cpe:see:gem:catalogos:catalogo16", Value = "01"}
                                    }
                                    }
                            },
                            Delivery = despacho,
                            TaxTotal = new TaxTotalType[]
                                {
                                new TaxTotalType
                                {
                                    TaxAmount = new TaxAmountType { Value = decimal.Parse(deta[x, 5]), currencyID = deta[x, 2] },
                                    TaxSubtotal = new TaxSubtotalType[]
                                    {
                                        new TaxSubtotalType
                                        {
                                            TaxableAmount = new TaxableAmountType { Value = decimal.Parse(deta[x, 9]), currencyID = deta[x, 2] },
                                            TaxAmount = new TaxAmountType { Value = decimal.Parse(deta[x, 5]), currencyID = deta[x, 2] },
                                            TaxCategory = new TaxCategoryType
                                            {
                                                Percent = new PercentType1{ Value = decimal.Parse(deta[x, 14]) },
                                                TaxExemptionReasonCode = new TaxExemptionReasonCodeType { Value = deta[x, 15], listAgencyName = "PE:SUNAT", listName = "Afectacion del IGV", listURI = "urn:pe:gob:sunat:cpe:see:gem:catalogos:catalogo07"},
                                                TaxScheme = new TaxSchemeType
                                                {
                                                    ID = new IDType { Value = deta[x, 16], schemeName = "Codigo de tributos", schemeAgencyName = "PE:SUNAT", schemeURI = "urn:pe:gob:sunat:cpe:see:gem:catalogos:catalogo05"},
                                                    Name = new NameType1 { Value = deta[x, 17] },
                                                    TaxTypeCode = new TaxTypeCodeType { Value = deta[x, 18] }
                                                }
                                            }
                                        }
                                    }
                                }
                                },
                            Item = new ItemType
                            {
                                Description = new DescriptionType[] { new DescriptionType { Value = deta[x, 6] + " " + deta[x, 7] } },
                                SellersItemIdentification = new ItemIdentificationType { ID = new IDType { Value = "-" } }
                            },
                            Price = new PriceType { PriceAmount = new PriceAmountType { currencyID = deta[x, 2], Value = v_psig } }
                        };
                    }
                    if (deta[x, 0] == "7")
                    {
                        detIL7 = new InvoiceLineType
                        {
                            ID = new IDType { Value = deta[x, 0] },
                            InvoicedQuantity = new InvoicedQuantityType { Value = v_cant, unitCode = v_umed },
                            LineExtensionAmount = new LineExtensionAmountType { Value = decimal.Parse(deta[x, 9]), currencyID = deta[x, 2] },
                            PricingReference = new PricingReferenceType
                            {
                                AlternativeConditionPrice = new PriceType[]
                                    {
                                    new PriceType
                                    {
                                        PriceAmount = new PriceAmountType { Value = v_pptn, currencyID = deta[x, 2] },
                                        PriceTypeCode = new PriceTypeCodeType { listName = "Tipo de Precio", listAgencyName = "PE:SUNAT", listURI = "urn:pe:gob:sunat:cpe:see:gem:catalogos:catalogo16", Value = "01"}
                                    }
                                    }
                            },
                            Delivery = despacho,
                            TaxTotal = new TaxTotalType[]
                                {
                                new TaxTotalType
                                {
                                    TaxAmount = new TaxAmountType { Value = decimal.Parse(deta[x, 5]), currencyID = deta[x, 2] },
                                    TaxSubtotal = new TaxSubtotalType[]
                                    {
                                        new TaxSubtotalType
                                        {
                                            TaxableAmount = new TaxableAmountType { Value = decimal.Parse(deta[x, 9]), currencyID = deta[x, 2] },
                                            TaxAmount = new TaxAmountType { Value = decimal.Parse(deta[x, 5]), currencyID = deta[x, 2] },
                                            TaxCategory = new TaxCategoryType
                                            {
                                                Percent = new PercentType1{ Value = decimal.Parse(deta[x, 14]) },
                                                TaxExemptionReasonCode = new TaxExemptionReasonCodeType { Value = deta[x, 15], listAgencyName = "PE:SUNAT", listName = "Afectacion del IGV", listURI = "urn:pe:gob:sunat:cpe:see:gem:catalogos:catalogo07"},
                                                TaxScheme = new TaxSchemeType
                                                {
                                                    ID = new IDType { Value = deta[x, 16], schemeName = "Codigo de tributos", schemeAgencyName = "PE:SUNAT", schemeURI = "urn:pe:gob:sunat:cpe:see:gem:catalogos:catalogo05"},
                                                    Name = new NameType1 { Value = deta[x, 17] },
                                                    TaxTypeCode = new TaxTypeCodeType { Value = deta[x, 18] }
                                                }
                                            }
                                        }
                                    }
                                }
                                },
                            Item = new ItemType
                            {
                                Description = new DescriptionType[] { new DescriptionType { Value = deta[x, 6] + " " + deta[x, 7] } },
                                SellersItemIdentification = new ItemIdentificationType { ID = new IDType { Value = "-" } }
                            },
                            Price = new PriceType { PriceAmount = new PriceAmountType { currencyID = deta[x, 2], Value = v_psig } }
                        };
                    }
                    if (deta[x, 0] == "8")
                    {
                        detIL8 = new InvoiceLineType
                        {
                            ID = new IDType { Value = deta[x, 0] },
                            InvoicedQuantity = new InvoicedQuantityType { Value = v_cant, unitCode = v_umed },
                            LineExtensionAmount = new LineExtensionAmountType { Value = decimal.Parse(deta[x, 9]), currencyID = deta[x, 2] },
                            PricingReference = new PricingReferenceType
                            {
                                AlternativeConditionPrice = new PriceType[]
                                    {
                                    new PriceType
                                    {
                                        PriceAmount = new PriceAmountType { Value = v_pptn, currencyID = deta[x, 2] },
                                        PriceTypeCode = new PriceTypeCodeType { listName = "Tipo de Precio", listAgencyName = "PE:SUNAT", listURI = "urn:pe:gob:sunat:cpe:see:gem:catalogos:catalogo16", Value = "01"}
                                    }
                                    }
                            },
                            Delivery = despacho,
                            TaxTotal = new TaxTotalType[]
                                {
                                new TaxTotalType
                                {
                                    TaxAmount = new TaxAmountType { Value = decimal.Parse(deta[x, 5]), currencyID = deta[x, 2] },
                                    TaxSubtotal = new TaxSubtotalType[]
                                    {
                                        new TaxSubtotalType
                                        {
                                            TaxableAmount = new TaxableAmountType { Value = decimal.Parse(deta[x, 9]), currencyID = deta[x, 2] },
                                            TaxAmount = new TaxAmountType { Value = decimal.Parse(deta[x, 5]), currencyID = deta[x, 2] },
                                            TaxCategory = new TaxCategoryType
                                            {
                                                Percent = new PercentType1{ Value = decimal.Parse(deta[x, 14]) },
                                                TaxExemptionReasonCode = new TaxExemptionReasonCodeType { Value = deta[x, 15], listAgencyName = "PE:SUNAT", listName = "Afectacion del IGV", listURI = "urn:pe:gob:sunat:cpe:see:gem:catalogos:catalogo07"},
                                                TaxScheme = new TaxSchemeType
                                                {
                                                    ID = new IDType { Value = deta[x, 16], schemeName = "Codigo de tributos", schemeAgencyName = "PE:SUNAT", schemeURI = "urn:pe:gob:sunat:cpe:see:gem:catalogos:catalogo05"},
                                                    Name = new NameType1 { Value = deta[x, 17] },
                                                    TaxTypeCode = new TaxTypeCodeType { Value = deta[x, 18] }
                                                }
                                            }
                                        }
                                    }
                                }
                                },
                            Item = new ItemType
                            {
                                Description = new DescriptionType[] { new DescriptionType { Value = deta[x, 6] + " " + deta[x, 7] } },
                                SellersItemIdentification = new ItemIdentificationType { ID = new IDType { Value = "-" } }
                            },
                            Price = new PriceType { PriceAmount = new PriceAmountType { currencyID = deta[x, 2], Value = v_psig } }
                        };
                    }
                    if (deta[x, 0] == "9")
                    {
                        detIL9 = new InvoiceLineType
                        {
                            ID = new IDType { Value = deta[x, 0] },
                            InvoicedQuantity = new InvoicedQuantityType { Value = v_cant, unitCode = v_umed },
                            LineExtensionAmount = new LineExtensionAmountType { Value = decimal.Parse(deta[x, 9]), currencyID = deta[x, 2] },
                            PricingReference = new PricingReferenceType
                            {
                                AlternativeConditionPrice = new PriceType[]
                                    {
                                    new PriceType
                                    {
                                        PriceAmount = new PriceAmountType { Value = v_pptn, currencyID = deta[x, 2] },
                                        PriceTypeCode = new PriceTypeCodeType { listName = "Tipo de Precio", listAgencyName = "PE:SUNAT", listURI = "urn:pe:gob:sunat:cpe:see:gem:catalogos:catalogo16", Value = "01"}
                                    }
                                    }
                            },
                            Delivery = despacho,
                            TaxTotal = new TaxTotalType[]
                                {
                                new TaxTotalType
                                {
                                    TaxAmount = new TaxAmountType { Value = decimal.Parse(deta[x, 5]), currencyID = deta[x, 2] },
                                    TaxSubtotal = new TaxSubtotalType[]
                                    {
                                        new TaxSubtotalType
                                        {
                                            TaxableAmount = new TaxableAmountType { Value = decimal.Parse(deta[x, 9]), currencyID = deta[x, 2] },
                                            TaxAmount = new TaxAmountType { Value = decimal.Parse(deta[x, 5]), currencyID = deta[x, 2] },
                                            TaxCategory = new TaxCategoryType
                                            {
                                                Percent = new PercentType1{ Value = decimal.Parse(deta[x, 14]) },
                                                TaxExemptionReasonCode = new TaxExemptionReasonCodeType { Value = deta[x, 15], listAgencyName = "PE:SUNAT", listName = "Afectacion del IGV", listURI = "urn:pe:gob:sunat:cpe:see:gem:catalogos:catalogo07"},
                                                TaxScheme = new TaxSchemeType
                                                {
                                                    ID = new IDType { Value = deta[x, 16], schemeName = "Codigo de tributos", schemeAgencyName = "PE:SUNAT", schemeURI = "urn:pe:gob:sunat:cpe:see:gem:catalogos:catalogo05"},
                                                    Name = new NameType1 { Value = deta[x, 17] },
                                                    TaxTypeCode = new TaxTypeCodeType { Value = deta[x, 18] }
                                                }
                                            }
                                        }
                                    }
                                }
                                },
                            Item = new ItemType
                            {
                                Description = new DescriptionType[] { new DescriptionType { Value = deta[x, 6] + " " + deta[x, 7] } },
                                SellersItemIdentification = new ItemIdentificationType { ID = new IDType { Value = "-" } }
                            },
                            Price = new PriceType { PriceAmount = new PriceAmountType { currencyID = deta[x, 2], Value = v_psig } }
                        };
                    }
                    //
                    detalle = new InvoiceLineType[] { detIL, detIL2, detIL3, detIL4, detIL5, detIL6, detIL7, detIL8, detIL9 };
                }
            }
            #endregion
            // ARMAMOS EL XML
            XmlSerializer serial = new XmlSerializer(typeof(InvoiceType));
            Stream fs = new FileStream(Pruta + EmisRuc + "-" + TipDocu + "-" + NumDVta + ".xml", FileMode.Create, FileAccess.Write);
            var _comprobante = new InvoiceType();
            _comprobante.UBLExtensions = uBLExtensionTypes;
            _comprobante.UBLVersionID = new UBLVersionIDType { Value = "2.1" };
            _comprobante.CustomizationID = new CustomizationIDType { Value = "2.0" };
            //_comprobante.ProfileID = profile;
            _comprobante.ID = new IDType { Value = NumDVta };
            _comprobante.IssueDate = new IssueDateType { Value = DateTime.Parse(FecEmis) };
            _comprobante.IssueTime = new IssueTimeType { Value = DateTime.Parse(HorEmis) };
            _comprobante.DueDate = new DueDateType { Value = fecVcto };
            _comprobante.InvoiceTypeCode = type;
            _comprobante.Note = note;
            _comprobante.DocumentCurrencyCode = mone;
            _comprobante.DespatchDocumentReference = guiasRef;
            _comprobante.Signature = signature;
            _comprobante.AccountingSupplierParty = emisor;
            _comprobante.AccountingCustomerParty = cliente;
            if (CtaDetra != "")                                 // comprobante con detracción
            {
                _comprobante.PaymentMeans = new PaymentMeansType[] 
                { 
                    new PaymentMeansType 
                    { 
                        ID = new IDType { Value = "Detraccion" },
                        PaymentMeansCode = new PaymentMeansCodeType { Value = "001", listName = "Medio de pago", listAgencyName = "PE:SUNAT", listURI = "urn:pe:gob:sunat:cpe:see:gem:catalogos:catalogo59"},
                        PayeeFinancialAccount = new FinancialAccountType { ID = new IDType { Value = CtaDetra } } 
                    } 
                };
            }
            _comprobante.PaymentTerms = Tpago;
            _comprobante.TaxTotal = impuestos;
            _comprobante.LegalMonetaryTotal = totales;
            if (et_cargaU == "1")   // 1=carga unica, 0=carga normal
            {
                //_comprobante.Delivery = new DeliveryType[] { despacho };  // ... 15/02/2024 esta sección va dentro de InvoiceLine
            }
            _comprobante.InvoiceLine = detalle;

            var xns = new XmlSerializerNamespaces();
            xns.Add("", "urn:oasis:names:specification:ubl:schema:xsd:Invoice-2");
            xns.Add("cac", "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2");
            xns.Add("cbc", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");
            xns.Add("ccts", "urn:un:unece:uncefact:documentation:2");
            xns.Add("ds", "http://www.w3.org/2000/09/xmldsig#");
            xns.Add("ext", "urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2");
            xns.Add("qdt", "urn:oasis:names:specification:ubl:schema:xsd:QualifiedDatatypes-2");
            xns.Add("udt", "urn:un:unece:uncefact:data:specification:UnqualifiedDataTypesSchemaModule:2");
            xns.Add("xsi", "http://www.w3.org/2001/XMLSchema-instance");
            // Firma y graba el xml firmado ó solo graba el xml sin firmar
            if (IndFir == "true")           
            {
                var oStringWriter = new StringWriter();
                serial.Serialize(XmlWriter.Create(oStringWriter), _comprobante, xns);
                string stringXml = oStringWriter.ToString();
                XmlDocument XmlpaFirmar = new XmlDocument();
                XmlpaFirmar.LoadXml(stringXml);
                //FirmarDocumentoXml(XmlpaFirmar, "certificado.pfx", "190969Sorol").Save("XmlFirmado.xml");
                using (Stream stream = fs)
                {
                    using (XmlWriter xmlWriter = new XmlTextWriter(stream, Encoding.GetEncoding("ISO-8859-1")))
                    {
                        FirmarDocumentoXml(XmlpaFirmar, RAcert, Clacert).Save(xmlWriter);  //  "certificado.pfx", "190969Sorol"
                        Console.WriteLine("Exito -> " + fs.ToString());
                        retorna = "Exito";
                    }
                }
            }
            else
            {
                using (Stream stream = fs)  // graba el xml sin firmar
                {
                    using (XmlWriter xmlWriter = new XmlTextWriter(stream, Encoding.GetEncoding("ISO-8859-1")))
                    {
                        serial.Serialize(xmlWriter, _comprobante, xns);
                    }
                    Console.WriteLine("Exito -> " + fs.ToString());
                    retorna = "Exito";
                }
            }
            return retorna;
        }

        private static string UsoUBLnotCred(string Pruta, string IndFir, string RAcert, string Clacert,
        string EmisRuc, string EmisNom, string EmisCom, string CodLocA, string EmisUbi, string EmisDir, string EmisDep, string EmisPro,
        string EmisDis, string EmisUrb, string EmisPai, string EmisCor, string EmisTel, string EmisTdoc, string EmisWeb,
        string SeriNot, string NumeNot, string TipoNot, string NumNotCre, string IdenNot, string FecEmis, string HorEmis,
        string TipDocu, string CodLey1, string MonLetr, string CodMonS, string NtipNot,
        string DstTipdoc, string DstNumdoc, string DstNombre, string DstDirecc, string DstDepart, string DstProvin, string DstDistri, string DstUrbani, 
        string DstUbigeo, string DstCorreo, string DstTelef1, string CodSunImp,
        string ImpTotImp, string ImpOpeGra, string ImpIgvTot, string ImpOtrosT,
        string TotValVta, string TotPreVta, string TotDestos, string TotOtrCar,
        string TotaVenta, int nfd, string CondPago, decimal TipoCamb,
        string nipfe, string restexto, string autoriOP, string webose,
        string userCrea, string nomLocC, string desped0, string motivoA,
        string modTrans, string motiTras, string tipoComp, string fecEComp, string Comprob,
        string rutLogo, string rutNomQR, string rutNoPdf, string porcIgv, string codAfIgv, string codTrib, string[,] deta)
        {
            string retorna = "Fallo";
            //XmlCDataSection cdata;
            #region // DATOS DE EXTENSION DEL DOCUMENTO, acá va principalmente la FIRMA en el caso que el metodo de envío a sunat NO sea SFS
                XmlDocument xmlDocument = new XmlDocument();
                XmlElement firma = xmlDocument.CreateElement("ext:firma", "urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2");  // 31/05/2023
                UBLExtensionType[] uBLExtensionTypes = new UBLExtensionType[] { new UBLExtensionType { ExtensionContent = firma } };        //  ExtensionContent = firma <- 31/05/2023
            #endregion
            #region FIRMA ELECTRONICA DE LA GUIA ELECTRONICA 
            PartyType partid = new PartyType
            {
                PartyIdentification = new PartyIdentificationType[]
                    {
                        new PartyIdentificationType { ID = new IDType { Value = EmisRuc } }
                    },
                PartyName = new PartyNameType[] { new PartyNameType { Name = new NameType1 { Value = EmisNom } } }
            };
            AttachmentType attach = new AttachmentType
            {
                ExternalReference = new ExternalReferenceType { URI = new URIType { Value = "SigNode" } }
            };
            SignatureType tory = new SignatureType
            {
                ID = new IDType { Value = "SignSOLORSOFT" },
                SignatoryParty = partid,
                DigitalSignatureAttachment = attach
            };
            SignatureType[] signature = new SignatureType[]
            {
                    tory
            };
            #endregion
            #region notas
            NoteType anota = null;
            if (CodLey1 != "")
            {
                anota = new NoteType { languageLocaleID = CodLey1, Value = MonLetr };
            }
            #endregion
            #region DETALLE
            CreditNoteLineType[] detalle = null;
            CreditNoteLineType detIL1 = null;
            CreditNoteLineType detIL2 = null;
            CreditNoteLineType detIL3 = null;
            CreditNoteLineType detIL4 = null;
            CreditNoteLineType detIL5 = null;
            CreditNoteLineType detIL6 = null;
            CreditNoteLineType detIL7 = null;
            CreditNoteLineType detIL8 = null;
            CreditNoteLineType detIL9 = null;
            for (int x = 0; x < 9; x++)
            {
                if (deta[x, 0] != "")
                {
                    CreditNoteLineType dito = new CreditNoteLineType();
                    dito.ID = new IDType { Value = (x + 1).ToString() };
                    dito.CreditedQuantity = new CreditedQuantityType { Value = 1, unitCode = deta[x, 4], unitCodeListAgencyName = "United Nations Economic Commission for Europe", unitCodeListID = "UN/ECE rec 20" };
                    dito.LineExtensionAmount = new LineExtensionAmountType { Value = decimal.Parse(deta[x, 7]), currencyID = CodMonS };
                    dito.PricingReference = new PricingReferenceType {
                        AlternativeConditionPrice = new PriceType[] { new PriceType { 
                            PriceAmount = new PriceAmountType { Value = decimal.Parse(deta[x, 9]), currencyID = CodMonS },
                            PriceTypeCode = new PriceTypeCodeType {Value = deta[x, 10], listAgencyName = "PE:SUNAT", listName = "Tipo de Precio", listURI = "urn:pe:gob:sunat:cpe:see:gem:catalogos:catalogo16" } } }
                    };
                    dito.TaxTotal = new TaxTotalType[] { new TaxTotalType {
                        TaxAmount = new TaxAmountType { Value = decimal.Parse(deta[x, 9]) - decimal.Parse(deta[x, 7]), currencyID = CodMonS },
                        TaxSubtotal = new TaxSubtotalType[] { new TaxSubtotalType { 
                            TaxableAmount = new TaxableAmountType{ Value = decimal.Parse(deta[x, 7]), currencyID = CodMonS },
                            TaxAmount = new TaxAmountType { Value = decimal.Parse(deta[x, 9]) - decimal.Parse(deta[x, 7]), currencyID = CodMonS },
                            TaxCategory = new TaxCategoryType { 
                                Percent = new PercentType1 { Value = decimal.Parse(porcIgv) },    // % igv
                                TaxExemptionReasonCode = new TaxExemptionReasonCodeType { Value = codAfIgv, listAgencyName="PE:SUNAT", listName="Afectacion del IGV", listURI="urn:pe:gob:sunat:cpe:see:gem:catalogos:catalogo07"},
                                TaxScheme = new TaxSchemeType { 
                                    ID = new IDType { Value = codTrib, schemeAgencyName="PE:SUNAT", schemeName="Codigo de tributos", schemeURI="urn:pe:gob:sunat:cpe:see:gem:catalogos:catalogo05"},
                                    Name = new NameType1 { Value = "IGV" },
                                    TaxTypeCode = new TaxTypeCodeType { Value = "VAT" }
                                }
                            }
                        } }
                        }
                    };
                    dito.Item = new ItemType { Description = new DescriptionType[] { new DescriptionType { Value = deta[x, 2] } } };
                    dito.Price = new PriceType { PriceAmount = new PriceAmountType { Value = decimal.Parse(deta[x, 7]), currencyID = CodMonS } };
                    //  
                    if (x == 0) detIL1 = dito;
                    if (x == 1) detIL2 = dito;
                    if (x == 2) detIL3 = dito;
                    if (x == 3) detIL4 = dito;
                    if (x == 4) detIL5 = dito;
                    if (x == 5) detIL6 = dito;
                    if (x == 6) detIL7 = dito;
                    if (x == 7) detIL8 = dito;
                    if (x == 8) detIL9 = dito;
                }
                detalle = new CreditNoteLineType[] { detIL1, detIL2, detIL3, detIL4, detIL5, detIL6, detIL7, detIL8, detIL9 };
            }
            #endregion
            // ARMAMOS EL XML
            XmlSerializer serialNC = new XmlSerializer(typeof(CreditNote));
            Stream fs = new FileStream(Pruta + EmisRuc + "-" + TipDocu + "-" + NumNotCre + ".xml", FileMode.Create, FileAccess.Write);
            var _notcred = new CreditNote();
            _notcred.UBLExtensions = uBLExtensionTypes;
            _notcred.UBLVersionID = new UBLVersionIDType { Value = "2.1"};
            _notcred.CustomizationID = new CustomizationIDType { schemeAgencyName = "PE:SUNAT", Value = "2.0" };   
            _notcred.ID = new IDType { Value = NumNotCre };
            _notcred.IssueDate = new IssueDateType { Value = DateTime.Parse(FecEmis) };
            _notcred.Note = anota; 
            _notcred.DocumentCurrencyCode = new DocumentCurrencyCodeType 
            { 
                Value = CodMonS, listAgencyName = "United Nations Economic Commission for Europe", listID = "ISO 4217 Alpha", listName = "Currency" 
            };
            _notcred.DiscrepancyResponse = new DisResp 
            { 
                Description = new DescriptionType { Value = motivoA }, // 21/08/2024 .... Value = NtipNot 
                ResponseCode = new ResponseCodeType { listAgencyName = "PE:SUNAT", listName = "Tipo de nota de credito", listURI = "urn:pe:gob:sunat:cpe:see:gem:catalogos:catalogo09", Value = TipoNot }
            };
            _notcred.BillingReference = new BillRef 
            {
                InvoiceDocumentReference = new InvDocRef { ID = new IDType { Value = Comprob }, 
                    IssueDate = new IssueDateType { Value = DateTime.Parse(fecEComp) }, DocumentTypeCode = new DocumentTypeCodeType { listAgencyName = "PE:SUNAT", listName = "Tipo de Documento", listURI = "urn:pe:gob:sunat:cpe:see:gem:catalogos:catalogo01", Value = tipoComp } 
                }
            };
            _notcred.Signature = signature;
            _notcred.AccountingSupplierParty = new ASP { Party = new part { 
                    WebsiteURI = new WebsiteURIType { Value = EmisWeb },
                    PartyIdentification = new ParIden
                    {
                        ID = new IDType { Value = EmisRuc, schemeAgencyName = "PE:SUNAT", schemeID = EmisTdoc, schemeName = "Documento de Identidad", schemeURI = "urn:pe:gob:sunat:cpe:see:gem:catalogos:catalogo06" }
                    },
                    PartyLegalEntity = new parLegEnt { RegistrationName = new RegistrationNameType { Value = EmisNom }, 
                        RegistrationAddress  = new RegAdd { ID = new IDType { Value = EmisUbi, schemeAgencyName = "PE:INEI", schemeName = "Ubigeos" },
                            AddressTypeCode = new AddressTypeCodeType { Value = CodLocA, listAgencyName = "PE:SUNAT", listName = "Establecimientos anexos" },
                            CityName = new CityNameType { Value = EmisDep},
                            CountrySubentity = new CountrySubentityType { Value = EmisPro},
                            District = new DistrictType { Value = EmisDis},
                            AddressLine = new AddLin { Line = new LineType { Value = EmisDir} },
                            Country = new countr { IdentificationCode = new IdentificationCodeType { Value = EmisPai, listAgencyName = "United Nations Economic Commission for Europe", listID = "ISO 3166-1", listName = "Country" } }
                        }
                    },
                    Contact = new ContactType { Telephone = new TelephoneType { Value = EmisTel }, ElectronicMail = new ElectronicMailType { Value = EmisCor} }
                } 
            };
            _notcred.AccountingCustomerParty = new ACP { Party = new part {
                PartyIdentification = new ParIden { ID = new IDType { Value = DstNumdoc, schemeAgencyName = "PE:SUNAT", schemeID = DstTipdoc, schemeName = "Documento de Identidad", schemeURI = "urn:pe:gob:sunat:cpe:see:gem:catalogos:catalogo06" } },
                PartyLegalEntity = new parLegEnt { RegistrationName = new RegistrationNameType { Value = DstNombre },
                    RegistrationAddress = new RegAdd { 
                        AddressLine = new AddLin { Line = new LineType { Value = DstDirecc + " " + DstDistri + " - " + DstProvin + " - " + DstDepart} }, 
                        Country = new countr { IdentificationCode = new IdentificationCodeType { Value = "PE", listAgencyName = "United Nations Economic Commission for Europe", listID = "ISO 3166-1", listName = "Country" } } }
                    }
                } 
            };
            _notcred.TaxTotal = new TaxTotalType { 
                TaxAmount = new TaxAmountType { Value = decimal.Parse(ImpIgvTot), currencyID = CodMonS },
                TaxSubtotal = new TaxSubtotalType[] { 
                    new TaxSubtotalType { 
                        TaxableAmount = new TaxableAmountType { Value = decimal.Parse(TotValVta), currencyID = CodMonS },
                        TaxAmount = new TaxAmountType { Value = decimal.Parse(ImpIgvTot), currencyID = CodMonS },
                        TaxCategory = new TaxCategoryType { 
                            TaxScheme = new TaxSchemeType { 
                                ID = new IDType { Value = CodSunImp, schemeAgencyName = "PE:SUNAT", schemeName = "Codigo de tributos", schemeURI = "urn:pe:gob:sunat:cpe:see:gem:catalogos:catalogo05"}, 
                                Name = new NameType1 { Value = "IGV" },
                                TaxTypeCode = new TaxTypeCodeType { Value = "VAT" }
                            }
                        }
                    }
                    
                }
            };
            _notcred.LegalMonetaryTotal = new MonetaryTotalType { 
                ChargeTotalAmount = new ChargeTotalAmountType { Value = 0, currencyID = CodMonS },
                PayableRoundingAmount = new PayableRoundingAmountType { Value = 0, currencyID = CodMonS },
                PayableAmount = new PayableAmountType { Value = decimal.Parse(TotaVenta), currencyID = CodMonS }
            };
            _notcred.CreditNoteLine = detalle;

            var xns = new XmlSerializerNamespaces();
            xns.Add("", "urn:oasis:names:specification:ubl:schema:xsd:CreditNote-2");
            xns.Add("cac", "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2");
            xns.Add("cbc", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");
            xns.Add("ccts", "urn:un:unece:uncefact:documentation:2");
            xns.Add("ds", "http://www.w3.org/2000/09/xmldsig#");
            xns.Add("ext", "urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2");
            xns.Add("qdt", "urn:oasis:names:specification:ubl:schema:xsd:QualifiedDatatypes-2");
            xns.Add("udt", "urn:un:unece:uncefact:data:specification:UnqualifiedDataTypesSchemaModule:2");
            xns.Add("xsi", "http://www.w3.org/2001/XMLSchema-instance");
            xns.Add("sac", "urn:sunat:names:specification:ubl:peru:schema:xsd:SunatAggregateComponents-1");
            /*
            xns.Add("ns10", "http://uri.etsi.org/01903/v1.4.1#");
            xns.Add("sac", "urn:oasis:names:specification:ubl:schema:xsd:SignatureAggregateComponents-2");
            xns.Add("sbc", "urn:oasis:names:specification:ubl:schema:xsd:SignatureBasicComponents-2");
            //xns.Add("see", "urn:seencorp:names:specification:ubl:peru:schema:xsd:SeencorpAggregateComponents-2");
            xns.Add("sig", "urn:oasis:names:specification:ubl:schema:xsd:CommonSignatureComponents-2");
            xns.Add("xades", "http://uri.etsi.org/01903/v1.3.2#");
            */
            if (IndFir == "true")
            {
                var oStringWriter = new StringWriter();
                serialNC.Serialize(XmlWriter.Create(oStringWriter), _notcred, xns);
                string stringXml = oStringWriter.ToString();
                XmlDocument XmlpaFirmar = new XmlDocument();
                XmlpaFirmar.LoadXml(stringXml);
                using (Stream stream = fs)
                {
                    using (XmlWriter xmlWriter = new XmlTextWriter(stream, Encoding.GetEncoding("ISO-8859-1")))
                    {
                        FirmarDocumentoXml(XmlpaFirmar, RAcert, Clacert).Save(xmlWriter);  //  "certificado.pfx", "190969Sorol"
                        Console.WriteLine("Exito -> " + fs.ToString());
                        retorna = "Exito";
                    }
                }
            }
            else
            {
                using (Stream stream = fs)  // graba el xml sin firmar
                {
                    using (XmlWriter xmlWriter = new XmlTextWriter(stream, Encoding.GetEncoding("ISO-8859-1")))
                    {
                        serialNC.Serialize(xmlWriter, _notcred, xns);
                    }
                    Console.WriteLine("Exito -> " + fs.ToString());
                    retorna = "Exito";
                }
            }
            return retorna;
        }

        public static XmlDocument FirmarDocumentoXml(XmlDocument XmlparaFirmar, string RutaCertificado, string ClaveCertificado)       // 31/05/2023
        {
            XmlparaFirmar.PreserveWhitespace = true;
            //XmlNode ExtensionContent = XmlparaFirmar.GetElementsByTagName("ext", "urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2").Item(0);
            XmlNode ExtensionContent = XmlparaFirmar.GetElementsByTagName("ExtensionContent", "urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2").Item(0);
            ExtensionContent.RemoveAll();   // quitamos todos los montones de elementos porque solo vamos a firmar

            X509Certificate2 x509Certificate2 = new X509Certificate2(File.ReadAllBytes(RutaCertificado), ClaveCertificado, X509KeyStorageFlags.Exportable);
            RSACryptoServiceProvider key = new RSACryptoServiceProvider(new CspParameters(24));
            SignedXml xml = new SignedXml(XmlparaFirmar);
            XmlDsigEnvelopedSignatureTransform env = new XmlDsigEnvelopedSignatureTransform();
            KeyInfo keyInfo = new KeyInfo();
            KeyInfoX509Data keyInfoX509Data = new KeyInfoX509Data(x509Certificate2);
            Reference reference = new Reference();

            string exportaLlave = x509Certificate2.PrivateKey.ToXmlString(true);
            key.PersistKeyInCsp = false;
            key.FromXmlString(exportaLlave);
            reference.AddTransform(env);
            reference.Uri = "";                                         // 31/05/2023 despues de error 16:45

            xml.SigningKey = key;

            Signature XMLSignature = xml.Signature;
            XMLSignature.SignedInfo.AddReference(reference);
            keyInfoX509Data.AddSubjectName(x509Certificate2.Subject);

            keyInfo.AddClause(keyInfoX509Data);
            XMLSignature.KeyInfo = keyInfo;
            XMLSignature.Id = "SignatureKG";
            xml.ComputeSignature();
            
            ExtensionContent.AppendChild(xml.GetXml());

            return XmlparaFirmar;
        }

        #region original usoubldocvta antes del 15/02/2024
        private static string UsoUBLDocvta_EX(string Pruta, string IndFir, string RAcert, string Clacert,
        string EmisRuc, string EmisNom, string EmisCom, string CodLocA, string EmisUbi, string EmisDir, string EmisDep, string EmisPro,
        string EmisDis, string EmisUrb, string EmisPai, string EmisCor, string NumDVta, string FecEmis, string HorEmis, string CodComp,
        string FecVcto, string TipDocu, string CodLey1, string MonLetr, string CodMonS, string DstTipdoc, string DstNumdoc, string DstNomTdo,
        string DstNombre, string DstDirecc, string DstDepart, string DstProvin, string DstDistri, string DstUrbani, string DstUbigeo,
        string ImpTotImp, string ImpOpeGra, string ImpIgvTot, string ImpOtrosT, string IgvCodSun, string IgvConInt, string IgvNomSun,
        string IgvCodInt, string TotValVta, string TotPreVta, string TotDestos, string TotOtrCar, string TotaVenta, string[] deta,
        int nfd, string CtaDetra, int PorDetra, string ImpDetra, string GloDetra, string CodTipDet, string CondPago, string CodTipOpe,
        string et_codPaiO, string et_ubiPart, string et_depPart, string et_proPart, string et_disPart, string et_urbPart, string et_dirPart,
        string et_codPaiD, string et_ubiDest, string et_depDest, string et_proDest, string et_disDest, string et_dirDest, string et_placa1,
        string et_coinsc, string et_marVe1, string et_breVe1, string et_rucTra, string et_nomTra, string et_modali, string et_pesobr,
        string et_codMot, string et_fecTra, string et_mtcTra, string et_docCho, string et_codCho, string et_placa2, string et_indSub, string et_cargaU) // ,string Detalle
        {
            string retorna = "Fallo";
            #region // DATOS DE EXTENSION DEL DOCUMENTO, acá va principalmente la FIRMA en el caso que el metodo de envío a sunat NO sea SFS
            XmlDocument xmlDocument = new XmlDocument();
            XmlElement firma = xmlDocument.CreateElement("ext:firma", "urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2");  // 31/05/2023
            UBLExtensionType[] uBLExtensionTypes = new UBLExtensionType[] { new UBLExtensionType { ExtensionContent = firma } };        //  ExtensionContent = firma <- 31/05/2023
            #endregion
            //IssueTimeType horEmis = new IssueTimeType { Value = HorEmis }; // DateTime.Parse(HorEmis);
            DateTime fecVcto = DateTime.Parse(FecVcto);

            #region profile del comprobante
            ProfileIDType profile = new ProfileIDType
            {
                schemeName = "SUNAT:Identificador de Tipo de Operación",
                schemeAgencyName = "PE:SUNAT",
                schemeURI = "urn:pe:gob:sunat:cpe:see:gem:catalogos:catalogo17",
                Value = CodTipOpe
            };
            #endregion
            #region tipo comprobante, nombre, moneda
            NoteType noteDet = null;
            if (GloDetra != "")
            {
                noteDet = new NoteType { Value = GloDetra, languageLocaleID = "2006" };
            }
            InvoiceTypeCodeType type = new InvoiceTypeCodeType
            {
                //Value = TipDocu, listAgencyName = "PE:SUNAT", listName = "SUNAT:Identificador de Tipo de Documento", listURI = "urn:pe: gob: sunat: cpe: see: gem: catalogos: catalogo01" 
                listID = CodTipOpe,
                listAgencyName = "PE:SUNAT",
                listName = "Tipo de Documento",
                name = "Tipo de Operacion",
                listURI = "urn:pe:gob:sunat:cpe:see:gem:catalogos:catalogo01",
                listSchemeURI = "urn:pe:gob:sunat:cpe:see:gem:catalogos:catalogo51",
                Value = TipDocu
            };
            NoteType[] note = new NoteType[]
            {
                    new NoteType { Value = MonLetr, languageLocaleID = CodLey1 },
                    noteDet
            };
            DocumentCurrencyCodeType mone = new DocumentCurrencyCodeType
            {
                Value = CodMonS,
                listID = "ISO 4217 Alpha",
                listName = "Currency",
                listAgencyName = "United Nations Economic Commission for Europe"
            };
            #endregion
            #region documentosRelacionados - guias
            DocumentReferenceType[] guiasRef = new DocumentReferenceType[]
            {
                    new DocumentReferenceType{ ID = new IDType{ Value = deta[12] }, DocumentTypeCode = new DocumentTypeCodeType { Value = deta[13] } }
                    // aca deberían ir el resto de guías 
                    // una línea por cada guía
            };
            #endregion
            #region FIRMA ELECTRONICA DE LA GUIA ELECTRONICA 
            PartyType partid = new PartyType
            {
                PartyIdentification = new PartyIdentificationType[]
                {
                        new PartyIdentificationType { ID = new IDType { Value = EmisRuc } }
                },
                PartyName = new PartyNameType[] { new PartyNameType { Name = new NameType1 { Value = EmisNom } } } // Value = nomEmi
            };
            AttachmentType attach = new AttachmentType
            {
                ExternalReference = new ExternalReferenceType { URI = new URIType { Value = "SigNode" } }
            };
            SignatureType tory = new SignatureType
            {
                ID = new IDType { Value = "SignSOLORSOFT" },
                SignatoryParty = partid,
                DigitalSignatureAttachment = attach
            };
            SignatureType[] signature = new SignatureType[]
            {
                    tory
            };
            #endregion
            #region datos del emisor
            PartyNameType[] nomcom = null;
            if (EmisCom != "")
            {
                nomcom = new PartyNameType[] { new PartyNameType { Name = new NameType1 { Value = EmisCom } } };
            }
            SupplierPartyType emisor = new SupplierPartyType
            {
                Party = new PartyType
                {
                    PartyName = nomcom,
                    PartyIdentification = new PartyIdentificationType[]
                    {
                        new PartyIdentificationType { ID = new IDType   // schemeID = "6" <- esta en duro porque en todos los casos el remitente tiene Ruc
                        {
                            Value = EmisRuc, schemeID = "6", schemeName = "Documento de Identidad", schemeAgencyName = "PE:SUNAT", schemeURI = "urn:pe:gob:sunat:cpe:see:gem:catalogos:catalogo06"}
                        }
                    },
                    PartyLegalEntity = new PartyLegalEntityType[]
                    {
                        new PartyLegalEntityType
                        {
                            RegistrationName = new RegistrationNameType { Value = EmisNom},
                            RegistrationAddress = new AddressType { AddressTypeCode = new AddressTypeCodeType { Value = CodLocA, listName = "Establecimientos anexos", listAgencyName = "PE:SUNAT"} }
                        }
                    }
                }
            };
            #endregion
            #region datos cliente
            CustomerPartyType cliente = new CustomerPartyType
            {
                Party = new PartyType
                {
                    PartyIdentification = new PartyIdentificationType[]
                    {
                        new PartyIdentificationType{ ID = new IDType{ Value = DstNumdoc, schemeID = DstTipdoc, schemeName = "Documento de Identidad", schemeAgencyName = "PE:SUNAT", schemeURI = "urn:pe:gob:sunat:cpe:see:gem:catalogos:catalogo06"} }
                    },
                    PartyLegalEntity = new PartyLegalEntityType[]
                    {
                        new PartyLegalEntityType{ RegistrationName = new RegistrationNameType{ Value = DstNombre} }
                    }
                }
            };
            #endregion
            #region forma de pago
            PaymentTermsType TipCred = null;
            PaymentTermsType TipDetr = null;
            if (CondPago == "Credito")
            {
                TipCred = new PaymentTermsType
                {
                    ID = new IDType { Value = "FormaPago" },
                    PaymentMeansID = new PaymentMeansIDType[] { new PaymentMeansIDType { Value = "Cuota001" } },
                    Amount = new AmountType2 { Value = Decimal.Parse(TotaVenta), currencyID = CodMonS },
                    PaymentDueDate = new PaymentDueDateType { Value = fecVcto }
                };
            }
            if (CtaDetra != "")
            {
                TipDetr = new PaymentTermsType
                {
                    ID = new IDType { Value = "Detraccion" },
                    PaymentMeansID = new PaymentMeansIDType[] { new PaymentMeansIDType { Value = CodTipDet, schemeName = "Codigo de detraccion", schemeAgencyName = "PE:SUNAT", schemeURI = "urn:pe:gob:sunat:cpe:see:gem:catalogos:catalogo54" } },
                    PaymentPercent = new PaymentPercentType { Value = PorDetra },
                    Amount = new AmountType2 { Value = decimal.Parse(ImpDetra), currencyID = "PEN" } // PEN = soles porque la detracción si o si se paga en soles
                };
            }
            PaymentTermsType[] Tpago = new PaymentTermsType[]
            {
                TipDetr,
                new PaymentTermsType
                {
                    ID = new IDType { Value = "FormaPago" },
                    PaymentMeansID = new PaymentMeansIDType[] { new PaymentMeansIDType { Value = CondPago } },
                    Amount = new AmountType2 { Value = Decimal.Parse(TotaVenta), currencyID = CodMonS }
                },
                TipCred
            };
            #endregion
            #region Delivery Shipment (datos de cargas unicas en toneladas)
            DeliveryType despacho = null;
            if (et_cargaU == "1")
            {
                StartDateType finitra = new StartDateType { Value = DateTime.Parse(et_fecTra) };
                ShipmentType shipment = new ShipmentType
                {
                    ID = new IDType { Value = "1" },
                    HandlingCode = new HandlingCodeType { Value = et_codMot, listName = "SUNAT:Indicador de Motivo de Traslado", listAgencyName = "PE:SUNAT", listURI = "urn:pe:gob:sunat:cpe:see:gem:catalogos:catalogo20" },
                    GrossWeightMeasure = new GrossWeightMeasureType { Value = decimal.Parse(et_pesobr), unitCode = "TNE" },
                    ShipmentStage = new ShipmentStageType[]
                    {
                    new ShipmentStageType
                    {
                        TransportModeCode = new TransportModeCodeType { Value = et_modali, listName = "SUNAT:indicador de Modalidad de Transporte", listAgencyName = "PE:SUNAT", listURI = "urn:pe:gob:sunat:cpe:see:gem:catalogos:catalogo18" },
                        TransitPeriod = new PeriodType { StartDate = finitra },
                        CarrierParty = new PartyType[]
                        {
                            new PartyType
                            {
                                PartyLegalEntity = new PartyLegalEntityType[]
                                {
                                    new PartyLegalEntityType
                                    {
                                        CompanyID = new CompanyIDType { Value = et_rucTra, schemeID = "6", schemeName = "SUNAT:Indicador de Tipo de Documento de Identidad", schemeAgencyName = "PE:SUNAT", schemeURI = "urn:pe:gob:sunat:cpe:see:gem:catalogos:catalogo06" },
                                        RegistrationName = new RegistrationNameType { Value = et_nomTra }
                                    }
                                }
                            }
                        },
                        TransportMeans = new TransportMeansType
                        {
                            RegistrationNationalityID = new RegistrationNationalityIDType { Value = et_coinsc },
                            RoadTransport = new RoadTransportType { LicensePlateID = new LicensePlateIDType { Value = et_placa1 } }
                        },
                        DriverPerson = new PersonType[] { new PersonType { ID = new IDType { Value = et_docCho, schemeID = "1", schemeName = "SUNAT:Indicador de Tipo de Documento de Identidad", schemeAgencyName = "PE:SUNAT", schemeURI = "urn:pe:gob:sunat:cpe:see:gem:catalogos:catalogo06" } } }
                    }
                    },
                    Delivery = new DeliveryType
                    {
                        DeliveryAddress = new AddressType
                        {
                            CountrySubentityCode = new CountrySubentityCodeType { Value = et_ubiDest },
                            AddressLine = new AddressLineType[] { new AddressLineType { Line = new LineType { Value = et_dirDest } } }
                        },
                        DeliveryParty = new PartyType { MarkAttentionIndicator = new MarkAttentionIndicatorType { Value = Boolean.Parse(et_indSub) } }
                    },
                    TransportHandlingUnit = new TransportHandlingUnitType[]
                    {
                    new TransportHandlingUnitType { TransportEquipment = new TransportEquipmentType[]{ new TransportEquipmentType { ID = new IDType { Value = et_placa1 } } } }
                    },
                    OriginAddress = new AddressType
                    {
                        CountrySubentityCode = new CountrySubentityCodeType { Value = et_ubiPart },
                        AddressLine = new AddressLineType[] { new AddressLineType { Line = new LineType { Value = et_dirPart } } }
                    }
                };
                despacho = new DeliveryType
                {
                    Shipment = shipment,
                    DeliveryTerms = new DeliveryTermsType[]
                    { new DeliveryTermsType
                    {
                        ID = new IDType { Value = et_mtcTra },
                        DeliveryLocation = new LocationType1
                        {
                            Address = new AddressType
                            {
                                StreetName = new StreetNameType{ Value = et_dirDest},
                                CitySubdivisionName = new CitySubdivisionNameType{ Value = "-" },
                                CityName = new CityNameType { Value = et_proDest},
                                CountrySubentity = new CountrySubentityType { Value = et_depDest },
                                CountrySubentityCode = new CountrySubentityCodeType { Value = et_ubiDest },
                                District = new DistrictType { Value = et_disDest },
                                Country = new CountryType { IdentificationCode = new IdentificationCodeType { Value = "PE", listID = "ISO 3166-1", listAgencyName = "United Nations Economic Commission for Europe", listName = "Country"} }
                            }
                        }
                    }
                    }
                };
            }
            #endregion
            #region impuestos totales
            TaxTotalType[] impuestos = new TaxTotalType[]
            {
                new TaxTotalType
                {
                    TaxAmount = new TaxAmountType { currencyID = CodMonS , Value = decimal.Parse(ImpTotImp) },
                    TaxSubtotal = new TaxSubtotalType[]
                    {
                        new TaxSubtotalType
                        {
                            TaxableAmount = new TaxableAmountType { currencyID = CodMonS, Value = decimal.Parse(TotValVta) },   // TotaVenta
                            TaxAmount = new TaxAmountType { currencyID = CodMonS, Value = decimal.Parse(ImpTotImp) },
                            TaxCategory = new TaxCategoryType {
                                TaxScheme = new TaxSchemeType
                                {
                                    TaxTypeCode = new TaxTypeCodeType { Value = IgvCodInt},
                                    ID = new IDType { Value = IgvConInt, schemeName = "Codigo de tributos", schemeAgencyName = "PE:SUNAT", schemeURI = "urn:pe:gob:sunat:cpe:see:gem:catalogos:catalogo05" },
                                    Name = new NameType1 { Value = IgvNomSun}
                                }
                            }
                        }
                    }
                }
            };
            #endregion
            #region totales del comprobante
            MonetaryTotalType totales = new MonetaryTotalType
            {
                LineExtensionAmount = new LineExtensionAmountType { Value = decimal.Parse(TotValVta), currencyID = CodMonS },
                TaxInclusiveAmount = new TaxInclusiveAmountType { Value = decimal.Parse(TotaVenta), currencyID = CodMonS },
                PayableAmount = new PayableAmountType { Value = decimal.Parse(TotaVenta), currencyID = CodMonS }
            };
            #endregion
            #region Detalle del comprobante
            decimal v_cant = decimal.Parse(deta[1]);
            string v_umed = deta[11];
            decimal v_pptn = decimal.Parse(deta[4]);
            decimal v_psig = decimal.Parse(deta[9]);
            if (et_cargaU == "1")
            {
                v_cant = decimal.Parse(et_pesobr);
                v_umed = "TNE";
                v_pptn = Math.Round(decimal.Parse(deta[4]) / v_cant, 5);
                v_psig = Math.Round(decimal.Parse(deta[9]) / v_cant, 5);
            }
            InvoiceLineType[] detalle = new InvoiceLineType[]
            {
                new InvoiceLineType
                {
                    ID = new IDType { Value = deta[0] },
                    InvoicedQuantity = new InvoicedQuantityType { Value = v_cant, unitCode = v_umed },
                    LineExtensionAmount = new LineExtensionAmountType { Value = decimal.Parse(deta[9]), currencyID = deta[2] },
                    PricingReference = new PricingReferenceType
                    {
                        AlternativeConditionPrice = new PriceType[]
                        {
                            new PriceType
                            {
                                PriceAmount = new PriceAmountType { Value = v_pptn, currencyID = deta[2] },
                                PriceTypeCode = new PriceTypeCodeType { listName = "Tipo de Precio", listAgencyName = "PE:SUNAT", listURI = "urn:pe:gob:sunat:cpe:see:gem:catalogos:catalogo16", Value = "01"}
                            }
                        }
                    },
                    TaxTotal = new TaxTotalType[]
                    {
                        new TaxTotalType
                        {
                            TaxAmount = new TaxAmountType { Value = decimal.Parse(deta[5]), currencyID = deta[2] },
                            TaxSubtotal = new TaxSubtotalType[]
                            {
                                new TaxSubtotalType
                                {
                                    TaxableAmount = new TaxableAmountType { Value = decimal.Parse(deta[9]), currencyID = deta[2] },
                                    TaxAmount = new TaxAmountType { Value = decimal.Parse(deta[5]), currencyID = deta[2] },
                                    TaxCategory = new TaxCategoryType
                                    {
                                        Percent = new PercentType1{ Value = decimal.Parse(deta[14]) },
                                        TaxExemptionReasonCode = new TaxExemptionReasonCodeType { Value = deta[15], listAgencyName = "PE:SUNAT", listName = "Afectacion del IGV", listURI = "urn:pe:gob:sunat:cpe:see:gem:catalogos:catalogo07"},
                                        TaxScheme = new TaxSchemeType
                                        {
                                            ID = new IDType { Value = deta[16], schemeName = "Codigo de tributos", schemeAgencyName = "PE:SUNAT", schemeURI = "urn:pe:gob:sunat:cpe:see:gem:catalogos:catalogo05"},
                                            Name = new NameType1 { Value = deta[17] },
                                            TaxTypeCode = new TaxTypeCodeType { Value = deta[18] }
                                        }
                                    }
                                }
                            }
                        }
                    },
                    Item = new ItemType
                    {
                        Description = new DescriptionType[] { new DescriptionType { Value = deta[6] + " " + deta[7] } },
                        SellersItemIdentification = new ItemIdentificationType { ID = new IDType { Value =  "-" } }
                    },
                    Price = new PriceType { PriceAmount = new PriceAmountType { currencyID = deta[2], Value = v_psig } }
                }
            };
            #endregion
            // ARMAMOS EL XML
            XmlSerializer serial = new XmlSerializer(typeof(InvoiceType));
            Stream fs = new FileStream(Pruta + EmisRuc + "-" + TipDocu + "-" + NumDVta + ".xml", FileMode.Create, FileAccess.Write);
            var _comprobante = new InvoiceType();
            _comprobante.UBLExtensions = uBLExtensionTypes;
            _comprobante.UBLVersionID = new UBLVersionIDType { Value = "2.1" };
            _comprobante.CustomizationID = new CustomizationIDType { Value = "2.0" };
            //_comprobante.ProfileID = profile;
            _comprobante.ID = new IDType { Value = NumDVta };
            _comprobante.IssueDate = new IssueDateType { Value = DateTime.Parse(FecEmis) };
            _comprobante.IssueTime = new IssueTimeType { Value = DateTime.Parse(HorEmis) };
            _comprobante.DueDate = new DueDateType { Value = fecVcto };
            _comprobante.InvoiceTypeCode = type;
            _comprobante.Note = note;
            _comprobante.DocumentCurrencyCode = mone;
            _comprobante.DespatchDocumentReference = guiasRef;
            _comprobante.Signature = signature;
            _comprobante.AccountingSupplierParty = emisor;
            _comprobante.AccountingCustomerParty = cliente;
            if (CtaDetra != "")                                 // comprobante con detracción
            {
                _comprobante.PaymentMeans = new PaymentMeansType[]
                {
                    new PaymentMeansType
                    {
                        ID = new IDType { Value = "Detraccion" },
                        PaymentMeansCode = new PaymentMeansCodeType { Value = "001", listName = "Medio de pago", listAgencyName = "PE:SUNAT", listURI = "urn:pe:gob:sunat:cpe:see:gem:catalogos:catalogo59"},
                        PayeeFinancialAccount = new FinancialAccountType { ID = new IDType { Value = CtaDetra } }
                    }
                };
                /* _comprobante.PaymentTerms = new PaymentTermsType[]
                {
                    new PaymentTermsType 
                    { 
                        //ID = new IDType{ Value = CodTipDet, schemeName = "Codigo de detraccion", schemeAgencyName = "PE:SUNAT", schemeURI = "urn:pe:gob:sunat:cpe:see:gem:catalogos:catalogo54" },
                        ID = new IDType { Value = "Detraccion"},
                        PaymentMeansID = new PaymentMeansIDType[]{ new PaymentMeansIDType { Value = CodTipDet, schemeName = "Codigo de detraccion", schemeAgencyName = "PE:SUNAT", schemeURI = "urn:pe:gob:sunat:cpe:see:gem:catalogos:catalogo54" } },
                        PaymentPercent = new PaymentPercentType { Value = PorDetra },
                        Amount = new AmountType2 { Value = decimal.Parse(ImpDetra), currencyID = "PEN"} // PEN = soles porque la detracción si o si se paga en soles
                    }
                }; */
            }
            _comprobante.PaymentTerms = Tpago;
            _comprobante.TaxTotal = impuestos;
            _comprobante.LegalMonetaryTotal = totales;
            if (et_cargaU == "1")   // 1=carga unica, 0=carga normal
            {
                _comprobante.Delivery = new DeliveryType[] { despacho };
            }
            _comprobante.InvoiceLine = detalle;
            //_comprobante.LineCountNumeric = new LineCountNumericType { Value = nfd };

            var xns = new XmlSerializerNamespaces();
            xns.Add("", "urn:oasis:names:specification:ubl:schema:xsd:Invoice-2");
            xns.Add("cac", "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2");
            xns.Add("cbc", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");
            xns.Add("ccts", "urn:un:unece:uncefact:documentation:2");
            xns.Add("ds", "http://www.w3.org/2000/09/xmldsig#");
            xns.Add("ext", "urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2");
            xns.Add("qdt", "urn:oasis:names:specification:ubl:schema:xsd:QualifiedDatatypes-2");
            xns.Add("udt", "urn:un:unece:uncefact:data:specification:UnqualifiedDataTypesSchemaModule:2");
            xns.Add("xsi", "http://www.w3.org/2001/XMLSchema-instance");
            // Firma y graba el xml firmado ó solo graba el xml sin firmar
            if (IndFir == "true")
            {
                var oStringWriter = new StringWriter();
                serial.Serialize(XmlWriter.Create(oStringWriter), _comprobante, xns);
                string stringXml = oStringWriter.ToString();
                XmlDocument XmlpaFirmar = new XmlDocument();
                XmlpaFirmar.LoadXml(stringXml);
                //FirmarDocumentoXml(XmlpaFirmar, "certificado.pfx", "190969Sorol").Save("XmlFirmado.xml");
                using (Stream stream = fs)
                {
                    using (XmlWriter xmlWriter = new XmlTextWriter(stream, Encoding.GetEncoding("ISO-8859-1")))
                    {
                        FirmarDocumentoXml(XmlpaFirmar, RAcert, Clacert).Save(xmlWriter);  //  "certificado.pfx", "190969Sorol"
                        Console.WriteLine("Exito -> " + fs.ToString());
                        retorna = "Exito";
                    }
                }
            }
            else
            {
                using (Stream stream = fs)  // graba el xml sin firmar
                {
                    using (XmlWriter xmlWriter = new XmlTextWriter(stream, Encoding.GetEncoding("ISO-8859-1")))
                    {
                        serial.Serialize(xmlWriter, _comprobante, xns);
                    }
                    Console.WriteLine("Exito -> " + fs.ToString());
                    retorna = "Exito";
                }
            }
            return retorna;
        }

        #endregion
    }
    class DVTA_
    {
        public string EmisRuc { get; set; }
        public string EmisNom { get; set; }
        public string EmisCom { get; set; }
        public string CodLocA { get; set; }
        public string EmisUbi { get; set; }
        public string EmisDir { get; set; }
        public string EmisDep { get; set; }
        public string EmisPro { get; set; }
        public string EmisDis { get; set; }
        public string EmisUrb { get; set; }
        public string EmisPai { get; set; }
        public string EmisCor { get; set; }
        public string EmisTel { get; set; }
        public string NumDVta { get; set; }
        public string FecEmis { get; set; }
        public string HorEmis { get; set; }
        public string CodComp { get; set; }
        public string FecVcto { get; set; }
        public string TipDocu { get; set; }
        public string CodLey1 { get; set; }
        public string MonLetr { get; set; }
        public string CodMonS { get; set; }
        public string DstTipdoc { get; set; }
        public string DstNumdoc { get; set; }
        public string DstNomTdo { get; set; }
        public string DstNombre { get; set; }
        public string DstDirecc { get; set; }
        public string DstDepart { get; set; }
        public string DstProvin { get; set; }
        public string DstDistri { get; set; }
        public string DstUrbani { get; set; }
        public string DstUbigeo { get; set; }
        public string DstTelef1 { get; set; }
        public string DstCorreo { get; set; }
        public string ImpTotImp { get; set; }
        public string ImpOpeGra { get; set; }
        public string ImpIgvTot { get; set; }
        public string ImpOtrosT { get; set; }
        public string IgvCodSun { get; set; }
        public string IgvConInt { get; set; }
        public string IgvNomSun { get; set; }
        public string IgvCodInt { get; set; }
        public string TotValVta { get; set; }
        public string TotPreVta { get; set; }
        public string TotDestos { get; set; }
        public string TotOtrCar { get; set; }
        public string TotaVenta { get; set; }
        public int CanFilDet { get; set; }
        public string CtaDetra { get; set; } 
        public int PorDetra { get; set; }
        public string ImpDetra { get; set; }
        public string GloDetra { get; set; }
        public string CodTipDet { get; set; }
        public string CondPago { get; set; }
        public string CodTipOpe { get; set; }
        public decimal TipoCamb { get; set; }
        //
        public string et_codPaiO { get; set; }            // Código país del punto de origen
        public string et_ubiPart { get; set; }            // Ubigeo del punto de partida 
        public string et_depPart { get; set; }           // Departamento del punto de partida
        public string et_proPart { get; set; }           // Provincia del punto de partida
        public string et_disPart { get; set; }           // Distrito del punto de partida
        public string et_urbPart { get; set; }           // Urbanización del punto de partida
        public string et_dirPart { get; set; }          // Dirección detallada del punto de partida
        public string et_codPaiD { get; set; }            // Código país del punto de llegada
        public string et_ubiDest { get; set; }            // Ubigeo del punto de llegada
        public string et_depDest { get; set; }           // Departamento del punto de llegada
        public string et_proDest { get; set; }           // Provincia del punto de llegada
        public string et_disDest { get; set; }           // Distrito del punto de llegada
        public string et_dirDest { get; set; }          // Dirección detallada del punto de llegada
        public string et_placa1 { get; set; }             // Placa del Vehículo
        public string et_confve { get; set; }            // configuración vehicular del vehiculo 
        public string et_coinsc { get; set; }            // Constancia de inscripción del vehículo o certificado de habilitación vehicular
        public string et_marVe1 { get; set; }            // Marca del Vehículo
        public string et_breVe1 { get; set; }            // Nro.de licencia de conducir
        public string et_rucTra { get; set; }            // RUC del transportista
        public string et_nomTra { get; set; }           // Razón social del Transportista
        public string et_modali { get; set; }             // Modalidad de Transporte
        public string et_pesobr { get; set; }          // Total Peso Bruto
        public string et_codMot { get; set; }             // Código de Motivo de Traslado
        public string et_fecTra { get; set; }            // Fecha de Inicio de Traslado
        public string et_mtcTra { get; set; }            // Registro MTC
        public string et_docCho { get; set; }            // Nro.Documento del conductor
        public string et_codCho { get; set; }            // Tipo de Documento del conductor
        public string et_placa2 { get; set; }            // Placa del Vehículo secundario
        public string et_indSub { get; set; }             // Indicador de subcontratación
        public string et_cargaU { get; set; }           // marca de carga unica 1=carga unica, 0=carga normal
        //
        public string[,] Detalle { get; set; }        // Detalle
    }

}
