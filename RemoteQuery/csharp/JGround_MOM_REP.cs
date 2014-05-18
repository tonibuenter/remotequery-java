//
// Copyright (C) 2008 OOIT.com AG, Zürich CH
// All rights reserved.
//
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using Org.JGround.Util;

namespace Org.JGround.MOM {

    public class MOReport {
        //
        // CLASS LEVEL
        //
        internal static Logger logger = Logger.GetLogger(typeof(MOReport));
        public static class ElementName {
            public static readonly String source = "source";
            public static readonly String name = "name";
            public static readonly String description = "description";
            
        }
        public static class AttName {
            public static readonly String sid = "sid";
            public static readonly String mrid = "mrid";
        }
        //
        // OBJECT LEVEL
        //
        private String mrid;
        private String name;
        private String description;
        private List<String> sourceMoids;
        private List<MOClass> sourceClasses;
        private String reportProgramType ;
        private IMORoles roles;
        //
        private List<MOReportPage> reportPages = new List<MOReportPage>();

        public MOReport(String dir, String file) {


            file = Path.Combine(dir, file);
            XmlDocument doc = new XmlDocument();

            doc.Load(file);
            // MOID
            mrid = doc.DocumentElement.Attributes[AttName.mrid].Value;
            // SOURCE
            sourceMoids = new List<string>();
            foreach(XmlElement sourceElement in doc.DocumentElement.GetElementsByTagName(ElementName.source)) {
                String sid = null;
                if(StringUtils.IsEmpty(sid = sourceElement.GetAttribute(AttName.sid))) {
                    sourceMoids.Add( sourceElement.GetAttributeValue(MO.AttName.moid));
                }
            }
            // NAME
            name = doc.DocumentElement.GetTextByTagName(ElementName.name);
            description = doc.DocumentElement.GetTextByTagName(ElementName.description);
            // ACCESS ROLES
            XmlNodeList list = null;
            if((list = doc.GetElementsByTagName("accessRoles")).Count > 0) {
                roles = MORoles.CreateRoles((XmlElement)list[0]);
            }
            // REPORT UI
            XmlElement reportUIElement = (XmlElement)doc.DocumentElement.GetFirstChild("reportUI");
            if(reportUIElement != null) {
                foreach(XmlElement pageElement in reportUIElement.GetElementsByTagName("page")) {
                    reportPages.Add(MOReportPage.Create(this, pageElement));
                }
            }
            XmlElement reportProgramElement = (XmlElement)doc.DocumentElement.GetFirstChild("reportProgram");
            if(reportProgramElement != null) {
                reportProgramType = reportProgramElement.GetAttributeValue("type");
            }
        }

        public bool IsProgram() {
            return StringUtils.IsNotBlank(reportProgramType);
        }

        public String GetReportProgramClass() {
            return reportProgramType;
        }

        public String GetMrid() {
            return mrid;
        }

        public List<String> GetSourceMoids() {
            return sourceMoids;
        }

        public List<MOClass> GetSourceMOClasses() {
            if(sourceClasses == null) {
              List<MOClass> l   = new List<MOClass>();
                foreach(String sourceMoid in sourceMoids) {
                    l.Add(MOService.GetInstance().GetMOClass(sourceMoid));
                }
                sourceClasses = l;
            }
            return sourceClasses;
        }

        public String GetName() {
            return StringUtils.RNA(name);
        }

        public String GetDescription() {
            return StringUtils.RNA(description);
        }

        public IMORoles GetRoles() {
            return roles == null ? MORoles.EmptyMORoles : roles;
        }

        public IList<MOReportPage> GetMOReportPages() {
            return reportPages;
        }

        public override String ToString() {
            StringBuilder sb = new StringBuilder();
            //
            sb.Append("[mrid=");
            sb.Append(GetMrid());
            sb.Append("; ");
            //
            sb.Append(" name=");
            sb.Append(GetName());
            sb.Append("; ");
            //
            sb.Append("\n roles=");
            sb.Append(roles);
            sb.Append("; ");
            //
            sb.Append("]");
            //
            return sb.ToString();
        }

    }



    public class MOReportPage {

        public static MOReportPage Create(MOReport moReport, XmlElement pageElement) {
            return new MOReportPage(moReport, pageElement);
        }

        private MOReport moReport;
        private List<MOColumn> columns = new List<MOColumn>();
        private String label;


        private MOReportPage(MOReport moReport, XmlElement pageElement) {
            this.moReport = moReport;
            MOClass moClass = moReport.GetSourceMOClasses()[0];
            foreach(XmlNode xmlNode in pageElement.ChildNodes) {
                if(xmlNode is XmlElement) {
                    XmlElement xmlElement = (XmlElement)xmlNode;
                    if(xmlElement.Name.Equals("label")) {
                        label = xmlElement.InnerText.Trim();
                    }
                    if(xmlElement.Name.Equals("column")) {
                        columns.Add(MOColumn.Create(moReport, moClass, xmlElement));
                    }
                }
            }
        }

        public String GetLabel() {
            return StringUtils.RNN(label);
        }

        public List<MOColumn> GetMOColumns() {
            return columns;
        }

        public override String ToString() {
            StringBuilder sb = new StringBuilder();
            sb.Append("[reportPage=");
            sb.Append(GetLabel());
            sb.Append("; columns=");
            foreach(MOColumn moColumn in columns) {
                sb.Append(moColumn.GetAttRef());
                sb.Append(" ");
            }
            sb.Append("]");
            return sb.ToString();
        }

        public MOReport GetMOReport() {
            return this.moReport;
        }
    }

    public class MOColumn : MOView {

        new public static class  AttName {
            public static readonly String sortable = "sortable";
            public static readonly String filter = "filter";
        }

        public static readonly String FT_DEFAULT = "default";
        public static readonly String FT_YEAR = "year";
        public static readonly String FT_SELECT = "select";
        public static readonly String FT_STARTSWITH = "startswith";
        public static readonly String FT_CONTAINS = "contains";

        public static MOColumn Create(MOReport moReport, MOClass moClass, XmlElement controlElement) {
            return new MOColumn(moReport, moClass, controlElement);
        }

        //
        //
        //

        private MOReport moReport;
        private String sortable;
        private String filter;

        protected MOColumn(MOReport moReport, MOClass moClass, XmlElement controlElement)
            : base(moClass, controlElement) {
            this.moReport = moReport;
            sortable = controlElement.GetAttributeValue(AttName.sortable);
            filter = controlElement.GetAttributeValue(AttName.filter);
        }

        public MOReport GetMOReport() {
            return moReport;
        }

        public bool IsSortingEnabled() {
            return StringUtils.IsNotEmpty(sortable) && !"false".Equals(sortable.ToLower());
        }

        public String GetFilterType() {
            if(StringUtils.IsEmpty(filter)) {
                return "";
            } else {
                return filter.ToLower();
            }
        }

    }



}