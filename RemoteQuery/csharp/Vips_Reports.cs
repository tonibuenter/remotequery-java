//
// Copyright (C) 2008 Vitra AG, Klünenfeldstrasse 22, Muttenz, 4127 Birsfelden
// All rights reserved.
//
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Org.JGround.HWT;
using Org.JGround.HWT.MOM;
using Org.JGround.MOM;
using Org.JGround.MOM.DB;
using Org.JGround.Util;

namespace Com.OOIT.VIPS.Reports {

    public class LicenseRateRegional {
        internal LicenseRateRegional(String percentage, String note) {
            this.percentage = percentage;
            this.note = note;
        }
        internal String percentage { set; get; }
        internal String note { set; get; }
    }

    public class YearlyRate {
        internal YearlyRate(String percentage, String year) {
            this.percentage = percentage;
            this.year = year;
        }
        internal String percentage { set; get; }
        internal String year { set; get; }
    }

    public class ContractProductLicenseData {

        internal MODataObject contract;
        internal MODataObject product;
        internal List<MODataObject> cDesigners;
        internal List<MODataObject> pDesigners;


        private String productionStart;
        internal String ProductionStart {
            set { productionStart = value; }
            get { return StringUtils.RNN(productionStart); }
        }

        private String licenseEndDate;
        internal String LicenseEndDate {
            set { licenseEndDate = value; }
            get { return StringUtils.RNN(licenseEndDate); }
        }

        private String licenseRateType;
        internal String LicenseRateType {
            set { licenseRateType = value; }
            get { return StringUtils.RNN(licenseRateType); }
        }

        private String licenseRateFixPercentage;
        internal String LicenseRateFixPercentage {
            set { licenseRateFixPercentage = value; }
            get { return StringUtils.RNN(licenseRateFixPercentage); }
        }

        private List<LicenseRateRegional> licenseRateRegional;
        internal List<LicenseRateRegional> LicenseRateRegional {
            get { return ListUtils.RNN(licenseRateRegional); }
        }

        private List<YearlyRate> yearlyRates;
        internal List<YearlyRate> YearlyRates {
            get { return ListUtils.RNN(yearlyRates); }
        }


        internal ContractProductLicenseData(MODataObject contract, MODataObject product) {
            this.contract = contract;
            this.product = product;

        }



        internal long GetContractOid() {
            return contract.GetOid();
        }

        internal MODataObject GetContract() {
            return contract;
        }

        internal MODataObject GetProduct() {
            return product;
        }

        internal bool HasContractDesigners() {
            return ListUtils.IsNotEmpty(cDesigners);
        }

        internal List<MODataObject> GetContractDesigners() {
            return cDesigners;
        }

        internal bool HasProductDesigners() {
            return ListUtils.IsNotEmpty(pDesigners);
        }

        internal List<MODataObject> GetProductDesigners() {
            return pDesigners;
        }

        internal void SetProductDesigners(List<MODataObject> designers) {
            this.pDesigners = designers;
        }

        internal void SetContractDesigners(List<MODataObject> designers) {
            this.cDesigners = designers;
        }

        internal void AddLicenseRateRegional(LicenseRateRegional element) {
            if(licenseRateRegional == null) {
                licenseRateRegional = new List<LicenseRateRegional>();
            }
            licenseRateRegional.Add(element);
        }

        internal void AddYearlyRate(string percentage, string inYear) {
            if(yearlyRates == null) {
                this.yearlyRates = new List<YearlyRate>();
            }
            yearlyRates.Add(new YearlyRate(percentage, inYear));
        }
    }





    public class UIDesignerProductContractPanel : IUIReportPanel {

        private static Logger logger = Logger.GetLogger(typeof(UIDesignerProductContractPanel));

        private List<ContractProductLicenseData> filteredData = new List<ContractProductLicenseData>(300);

        private UIContractProductReportTable contractProductTable;

        private DIV mainDiv = new DIV();

        public UIDesignerProductContractPanel(UIFrame uiFrame) {
            contractProductTable = new UIContractProductReportTable(uiFrame, this);
            mainDiv.Add(contractProductTable);
            Reset();
        }

        public HComponent GetView() {
            return mainDiv;
        }

        public bool IsPrintEnabled() {
            return true;
        }

        public void Update() {
            // 
            // process filter
            //
            MOSearchCriteria sc = new MOSearchCriteria();
            sc.SetIncludedDataStates(DataState.APPROVED, DataState.STORED);
            sc.AddIncludedMoids(DEF.Contract.moid);


            List<MODataObject> contracts = MODataObject.Search(sc);
            //
            // Create Filtered Data
            // 
            this.filteredData.Clear();
            foreach(MODataObject contract in contracts) {
                List<MODataObject> contractLicenseRates = MODataObject.GetByIds(contract.GetCurrentValues(DEF.Contract.licensePerProduct));
                List<MODataObject> designers = MODataObject.GetByIds(contract.GetCurrentValues(DEF.Contract.designers));


                foreach(MODataObject contractLicenseRate in contractLicenseRates) {
                    //
                    MODataObject product = MODataObject.GetById(contractLicenseRate.GetCurrentValue(DEF.ContractLicenseRate.product));
                    if(!MOAccess.GetInstance().CanRead(product)) {
                        continue;
                    }
                    //
                    ContractProductLicenseData reportData = new ContractProductLicenseData(contract, product);
                    this.filteredData.Add(reportData);
                    //
                    reportData.SetContractDesigners(designers);
                    //
                    List<MODataObject> designersFromProduct = MODataObject.GetByIds(product.GetCurrentValues(DEF.Product.designers));
                    reportData.SetProductDesigners(designersFromProduct);
                    reportData.ProductionStart = product.GetCurrentValue(DEF.Product.startDate);
                    reportData.LicenseEndDate = contractLicenseRate.GetCurrentValue(DEF.ContractLicenseRate.licenseEndDate);
                    reportData.LicenseRateType = contractLicenseRate.GetCurrentValue(DEF.ContractLicenseRate.licenseRateType);
                    String lrt = StringUtils.RNN(reportData.LicenseRateType);
                    if(lrt.Equals(DEF.ContractLicenseRate.licenseRateFixPercentage)) {
                        //
                        // licenseRateFixPercentage
                        //
                        reportData.LicenseRateFixPercentage = contractLicenseRate.GetCurrentValue(DEF.ContractLicenseRate.licenseRateFixPercentage);
                    } else if(lrt.Equals(DEF.ContractLicenseRate.licenseRateRegional)) {
                        //
                        // licenseRateRegional
                        //
                        /*
                         <list>
                             <comp lookup="ch.vitra.vips.CurrencyNote" />
                         </list>
                         */
                        List<MODataObject> licenseRateRegionals = MODataObject.GetByIds(contractLicenseRate.GetCurrentValues(DEF.ContractLicenseRate.licenseRateRegional));
                        foreach(MODataObject licenseRateRegional in licenseRateRegionals) {
                            String percentage = licenseRateRegional.GetCurrentValue(DEF.CurrencyNote.percentage);
                            String note = licenseRateRegional.GetCurrentValue(DEF.CurrencyNote.note);
                            reportData.AddLicenseRateRegional(new LicenseRateRegional(percentage, note));
                        }
                    } else if(lrt.Equals(DEF.ContractLicenseRate.licenseRatePercentage)) {
                        //
                        // licenseRatePercentage
                        //
                        /*
                        <list>
                            <comp lookup="ch.vitra.vips.ContractLicenseRatePercentage" />
                        </list>
                        */
                        List<MODataObject> cLRercentages = MODataObject.GetByIds(contractLicenseRate.GetCurrentValues(DEF.ContractLicenseRate.licenseRatePercentage));
                        foreach(MODataObject cLRercentage in cLRercentages) {
                            String inYear = cLRercentage.GetCurrentValue(DEF.ContractLicenseRatePercentage.year);
                            String percentage = cLRercentage.GetCurrentValue(DEF.ContractLicenseRatePercentage.percentage);
                            reportData.AddYearlyRate(percentage, inYear);
                        }
                    }
                }
            }
        }

        public void Reset() {
            logger.Debug("DesignerContractReport reset");
            Update();
        }

        public void WriteReportToFile(String filePath) {
            Update();
            logger.Debug("DesignerContractReport write file");

            StreamWriter w = null;
            Encoding we = Encoding.Default;
            using(w = new StreamWriter(File.Open(filePath, FileMode.Create), we)) {
                foreach(String headerLabel in this.contractProductTable.headerLabels) {
                    w.WriteExcel(headerLabel);
                    w.Write("\t");
                }
                w.WriteLine();
                foreach(ContractProductLicenseData data in this.filteredData) {
                    //
                    MODataObject contract = data.GetContract();
                    w.WriteExcel(R.ContractToString(contract));
                    w.Write("\t");
                    MODataObject product = data.GetProduct();
                    w.WriteExcel(R.ProductToString(product));
                    w.Write("\t");
                    //if(data.HasContractDesigners()) {
                    //    foreach(MODataObject d in data.GetContractDesigners()) {
                    //        w.WriteExcel(R.DesignerToString(d));
                    //    }
                    //} else {
                    //    w.Write(" ");
                    //}
                    //w.Write("\t");
                    if(data.HasProductDesigners()) {
                        foreach(MODataObject d in data.GetProductDesigners()) {
                            w.WriteExcel(R.DesignerToString(d));
                        }
                    } else {
                        w.Write(" ");
                    }
                    w.Write("\t");
                    w.WriteExcel(data.ProductionStart);
                    w.Write("\t");
                    w.WriteExcel(data.LicenseEndDate);
                    w.Write("\t");
                    String lrt = data.LicenseRateType;
                    if(StringUtils.IsNotBlank(lrt)) {
                        if(DEF.ContractLicenseRate.licenseRateFixPercentage.Equals(lrt)) {
                            w.WriteExcelPercentage(data.LicenseRateFixPercentage);
                        }
                        if(DEF.ContractLicenseRate.licenseRateRegional.Equals(lrt)) {
                            foreach(LicenseRateRegional lrr in data.LicenseRateRegional) {
                                String v1 = StringUtils.IsBlank(lrr.percentage) ? "-" : lrr.percentage + "%";
                                String v2 = StringUtils.IsBlank(lrr.note) ? "-" : lrr.note;
                                w.WriteExcel(v1);
                                w.WriteExcel(",");
                                w.WriteExcel(v2);
                            }
                        }
                        if(DEF.ContractLicenseRate.licenseRatePercentage.Equals(lrt)) {
                            foreach(YearlyRate yr in data.YearlyRates) {
                                String v1 = StringUtils.IsBlank(yr.percentage) ? "-" : yr.percentage + "%";
                                String v2 = StringUtils.IsBlank(yr.year) ? "-" : yr.year;
                                w.WriteExcel(v1);
                                w.WriteExcel(",");
                                w.WriteExcel(v2);
                            }
                        }
                    } else {
                        w.Write(" ");
                    }
                    w.Write("\t");
                    w.WriteLine();
                }
            }
        }


        public void Close() {
            Reset();
        }

        public List<ContractProductLicenseData> GetFilteredData() {
            return this.filteredData;
        }

        public HComponent GetPrintPart() {
            return this.contractProductTable;
        }

    }



    public class UIContractProductReportTable : TABLE {

        internal String[] headerLabels = { DEF.Contract.MO_NAME, DEF.Product.MO_NAME, //DEF.Designer.MO_NAME + " (C)", 
                                            DEF.Designer.MO_NAME + " (P)","Prod. Start", "Lic. End",  
                                            "Rate Details (div)" };



        public UIContractProductReportTable(UIFrame uiFrame, UIDesignerProductContractPanel uiReportPanel) {
            //
            SetStyleClass(UIStyles.REPORT_TABLE);
            SetAttribute(HDTD.AttName.CELLPADDING, 0);
            SetAttribute(HDTD.AttName.CELLSPACING, 0);
            SetAttribute(HDTD.AttName.BORDER, 0);

            THEAD thead = new THEAD();
            TR tr = new TR();
            thead.Add(tr);

            foreach(String header in headerLabels) {
                tr.Add(new TH(new DIV(header, UIStyles.REPORT_CELL_HEADER)));
            }

            UIContractProductReportTBody tbody = new UIContractProductReportTBody(uiFrame, uiReportPanel);

            Add(thead, tbody);
        }

        public int GetNumberOfColumns() {
            return headerLabels.Count();
        }

    }

    public class UIContractProductReportTBody : TBODY {

        private UIDesignerProductContractPanel uiReportPanel;

        private UIContractProductReportRow reportRow;

        public UIContractProductReportTBody(UIFrame uiFrame, UIDesignerProductContractPanel uiReportPanel) {
            this.uiReportPanel = uiReportPanel;
            reportRow = new UIContractProductReportRow(uiFrame, this.uiReportPanel);
            //
        }

        public override int Count() {
            return uiReportPanel.GetFilteredData().Count;
        }

        public override HComponent Get(int index) {
            reportRow.SetRenderValues(uiReportPanel.GetFilteredData()[index], index);
            return reportRow;
        }

    }

    public class UIContractProductReportRow : TR, IHListener {

        private static readonly TD emptyTD = new TD();

        private TD td = new TD();
        private ContractProductLicenseData data;
        private UIDesignerProductContractPanel uiReportPanel;

        private TD linkTD;
        private String cellStyle;
        private HLink linkBt;
        private DIV textDiv;
        private DIV noWrapDiv = new DIV("", UIStyles.MO_NOWRAP);
        private UIFrame uiFrame;
        private DIV emptyDiv = new DIV();

        public UIContractProductReportRow(UIFrame uiFrame, UIDesignerProductContractPanel uiReportPanel) {
            this.uiFrame = uiFrame;
            this.uiReportPanel = uiReportPanel;
            this.linkBt = (HLink)new HLink(uiFrame, "View").SetStyleClass(UIStyles.SIMPLE_LINK);
            this.textDiv = new DIV();
            this.linkTD = new TD();
            linkTD.SetAttribute(HDTD.AttName.VALIGN, HDTD.AttValue.TOP);
            //
            linkBt.AddHListener(this);
        }

        public void SetRenderValues(ContractProductLicenseData data, int rowIndex) {
            if(rowIndex % 2 == 0) {
                this.cellStyle = UIStyles.REPORT_CELL_0;
                td.SetStyleClass(UIStyles.REPORT_CELL_0);
                emptyTD.SetStyleClass(UIStyles.REPORT_CELL_0);
                linkTD.SetStyleClass(UIStyles.REPORT_CELL_0);
            } else {
                this.cellStyle = UIStyles.REPORT_CELL_1;
                td.SetStyleClass(UIStyles.REPORT_CELL_1);
                emptyTD.SetStyleClass(UIStyles.REPORT_CELL_1);
                linkTD.SetStyleClass(UIStyles.REPORT_CELL_1);
            }
            this.data = data;
        }

        public override int Count() {
            return 6;
        }

        public override HComponent Get(int index) {
            //


            this.td.RemoveAll();
            switch(index) {
                case 0:
                    MODataObject contract = data.GetContract();
                    return LinkedCell(contract, R.ContractToString(contract));
                case 1:
                    MODataObject product = data.GetProduct();
                    return LinkedCell(product, R.ProductToString(product));
                //case 2:
                //    if(data.HasContractDesigners()) {
                //        foreach(MODataObject d in data.GetContractDesigners()) {
                //            HComponent comp = CreateLinkedCell(d, R.DesignerToString(d));
                //            td.Add(comp);
                //        }
                //    } else {
                //        td.Add(NBSP.NBSP);
                //    }
                //    return td;
                case 2:
                    if(data.HasProductDesigners()) {
                        foreach(MODataObject d in data.GetProductDesigners()) {
                            HComponent comp = CreateLinkedCell(d, R.DesignerToString(d));
                            td.Add(comp);
                        }
                    } else {
                        td.Add(NBSP.NBSP);
                    }
                    return td;
                case 3:
                    return SimpleTD(data.ProductionStart, true);
                case 4:
                    return SimpleTD(data.LicenseEndDate, true);
                case 5:
                    String lrt = data.LicenseRateType;
                    if(StringUtils.IsNotBlank(lrt)) {
                        if(DEF.ContractLicenseRate.licenseRateFixPercentage.Equals(lrt)) {
                            return td.Set(new DIV(data.LicenseRateFixPercentage + "%"));
                        }
                        if(DEF.ContractLicenseRate.licenseRateRegional.Equals(lrt)) {
                            TABLE table = new TABLE(0, 0, 0);
                            foreach(LicenseRateRegional lrr in data.LicenseRateRegional) {
                                // TR tr = new TR(new TD(cellStyle, new DIV(lrr.percentage + "%", UIStyles.MO_LEFTCELL)), new TD(cellStyle, new DIV(lrr.note, UIStyles.MO_NOWRAP)));
                                table.Add(InnerTableRow(StringUtils.IsNotBlank(lrr.percentage) ? lrr.percentage + "%" : "", lrr.note));
                                //DIV div = new DIV(lrr.percentage + " : " + lrr.note, UIStyles.MO_NOWRAP);
                                // table.Add(tr);
                            }
                            td.Set(table);
                            return td;
                        }
                        if(DEF.ContractLicenseRate.licenseRatePercentage.Equals(lrt)) {


                            TABLE table = new TABLE(0, 0, 0);
                            foreach(YearlyRate yr in data.YearlyRates) {
                                //TR tr = new TR(new TD(cellStyle, new DIV(yr.percentage + "%", UIStyles.MO_LEFTCELL)), new TD(cellStyle, new DIV(yr.year, UIStyles.MO_NOWRAP)));
                                //DIV div = new DIV(lrr.percentage + " : " + lrr.note, UIStyles.MO_NOWRAP);
                                table.Add(InnerTableRow(StringUtils.IsNotBlank(yr.percentage) ? yr.percentage + "%" : "", yr.year));
                            }
                            td.Set(table);
                            return td;
                        }
                    } else {
                        return td.Set(NBSP.NBSP);
                    }
                    break;


            }
            return emptyDiv;
        }

        private TR InnerTableRow(String s1, String s2) {
            TR tr = new TR(new TD(cellStyle + "i", new DIV(s1, UIStyles.MO_LEFTCELL)), new TD(cellStyle + "i", new DIV(s2, UIStyles.MO_NOWRAP)));
            return tr;
        }

        private HComponent SimpleTD(String value, bool nowrap) {
            if(StringUtils.IsBlank(value)) {
                return td.Set(NBSP.NBSP);
            }
            DIV div = null;
            if(nowrap) {
                div = new DIV(value, UIStyles.MO_NOWRAP);
            } else {
                div = new DIV(value);
            }
            return td.Set(div);
        }

        private HComponent LinkedCell(MODataObject mod, String label) {
            if(mod == null) {
                return td.Set(textDiv.SetText("-"));
            }
            this.linkTD.Set(linkBt);
            linkBt.SetText(label);
            linkBt.SetSubId("" + mod.GetOid());
            linkTD.Set(linkBt);
            return linkTD;
        }

        private HComponent CreateLinkedCell(MODataObject mod, String label) {
            HLink viewBt = new HLink(uiFrame, label);
            viewBt.SetStyleClass(UIStyles.SIMPLE_LINK);
            viewBt.AddHListener(this);
            viewBt.SetSubId("" + mod.GetOid());
            return new DIV(viewBt);
        }

        public void Arrived(HEvent he) {
            Object s = he.GetSource();
            if(s is HLink) {
                HLink hlink = (HLink)s;
                String oid = hlink.GetSubId();
                MODataObject mod = MODataObject.GetById(oid);
                if(mod != null) {
                    uiFrame.OpenDialogOnStack(new UIViewWindowDelegator(uiFrame.GetViewWindow(mod.GetMoid()), mod));
                }
            }
        }


    }




    public class R {

        public static String DesignerToString(MODataObject designer) {
            String ln = designer.GetCurrentValue(DEF.Designer.lastName);
            String fn = designer.GetCurrentValue(DEF.Designer.firstName);
            return StringUtils.IsNotBlank(fn) ? ln + ", " + fn : ln;
        }
        public static String ProductToString(MODataObject product) {
            String pn = product.GetCurrentValue(DEF.Product.productName);
            String proj = product.GetCurrentValue(DEF.Product.projectName);
            return StringUtils.IsNotBlank(pn) ? pn : StringUtils.IsNotBlank(proj) ? proj : "...";
        }

        public static string ContractToString(MODataObject contract) {
            String t = contract.GetCurrentValue(DEF.Contract.contractTitle);
            String n = contract.GetCurrentValue(DEF.Contract.contractNumber);
            long oid = contract.GetOid();
            return StringUtils.IsNotBlank(t) ? t : StringUtils.IsNotBlank(n) ? n : "(" + oid + ")";
        }

    }


}