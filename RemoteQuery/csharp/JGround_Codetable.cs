//
// Copyright (C) 2008 OOIT.com AG, Zürich CH
// All rights reserved.
//

using System;
using System.Collections.Generic;
using System.Xml;
using System.IO;
using Org.JGround.Util;

namespace Org.JGround.Codetable {



    public interface ICodeTableElement {
        String GetCode();
        String GetName();
    }

    //public interface ICCodeTableElement<E> : ICodeTableElement{
    //    E GetContextObject();
    //}


    public interface ICodeTable : IList<ICodeTableElement> {

        ICodeTableElement GetElement(String code);

        String GetName();

        // String GetDescription();

        ICodeTableElement Get(int index);

        ICodeTable GetSubCodeTable(String code);

    }

    [Serializable]
    public class CodeTable : List<ICodeTableElement>, ICodeTable {
        //
        // CLASS LEVEL
        //
        private static Logger logger = Logger.GetLogger(typeof(CodeTable));

        public static CodeTableElement EMTPY_CODE_TABLE_ELEMENT = new CodeTableElement("", "-");
        public static ICodeTable EMTPY_CODE_TABLE = new CodeTable("-empty code table-");
        public static String CODETABLE_DIR = "";
        private static Dictionary<String, ICodeTable> codeTableCache = new Dictionary<String, ICodeTable>();


        public static ICodeTable Get(String codeTableName, bool withEmptyElement) {
            ICodeTable ct = codeTableCache.Get(codeTableName + "<>" + withEmptyElement);
            if(ct == null) {
                ct = Create(codeTableName, withEmptyElement);
                codeTableCache.Put(codeTableName + "<>" + withEmptyElement, ct);
            }
            return ct;
        }

        public static ICodeTable Create(String codeTableName, bool withEmptyElement) {
            if(codeTableName == null) {
                logger.Error("Codetable name is null - can not find codetable file.");
                return null;
            }
            String file = Path.Combine(CODETABLE_DIR, "codelist-" + codeTableName.ToLower() + ".xml");
            if(File.Exists(file) == false) {
                logger.Error("Codetable file does not exist.", file);
                return null;
            }
            XmlDocument doc = new XmlDocument();
            doc.Load(file);
            return new CodeTable(doc.DocumentElement, withEmptyElement);
        }

        private String name;
        private Dictionary<String, CodeTable> subCodeTables = new Dictionary<String, CodeTable>();
        //private String description;

        /**
         */
        public CodeTable(XmlElement codeTableElement, bool withEmptyElement) {
            this.name = StringUtils.RNA(codeTableElement.GetAttributeValue("name"));
            if(withEmptyElement) {
                Add(EMTPY_CODE_TABLE_ELEMENT);
            }

            foreach(Object o in codeTableElement) {
                if(o is XmlElement) {
                    XmlElement element = (XmlElement)o;
                    if(element.Name.Equals("element")) {
                        String code = element.GetFirstChild("code").InnerText;
                        String name = element.GetFirstChild("name").InnerText;
                        this.Add(new CodeTableElement(code, name));
                        XmlElement subCodeTableElement = (XmlElement)element.GetFirstChild("codetable");
                        if(subCodeTableElement != null) {
                            CodeTable subCodeTable = new CodeTable(subCodeTableElement, withEmptyElement);
                            subCodeTables.Put(code, subCodeTable);
                        }
                    }
                }
            }
            //this.description = codeTableElement.GetTextByTagName("description");
        }

        public CodeTable(String name, params ICodeTableElement[] codeTableElements) {
            this.name = name;
            this.AddRange(codeTableElements);
        }

        public ICodeTable GetSubCodeTable(String code) {
            return this.subCodeTables.Get(code);
        }



        public ICodeTableElement GetElement(String code) {
            code = code.Trim();
            for(int i = 0; i < Count; i++) {
                if(Get(i).GetCode().Equals(code)) {
                    return Get(i);
                }
            }
            return null;
        }

        public String GetName() {
            return name;
        }

        //public String GetDescription() {
        //    return description;
        //}

        public ICodeTableElement Get(int i) {
            return this[i];
        }


    }
    [Serializable]
    public class CodeTableElement : ICodeTableElement {

        private String code;
        private String name;

        public CodeTableElement(String code, String name) {
            this.code = code.Trim();
            this.name = name;
        }

        public String GetCode() {
            return StringUtils.RNN(this.code);
        }

        public String GetName() {
            return StringUtils.RNA(this.name);
        }

        public override String ToString() {
            return GetName();
        }


        public override bool Equals(Object obj) {
            if(obj is ICodeTableElement) {
                CodeTableElement cte = (CodeTableElement)obj;
                return cte.GetCode().Equals(this.GetCode());
            }
            return false;
        }

        public override int GetHashCode() {
            return GetCode().GetHashCode();
        }
    }


    [Serializable]
    public class CCodeTableElement<E>    // : ICCodeTableElement<E> 
    {

        private String code;
        private String name;
        private E contextObject;

        public CCodeTableElement(String code, String name) {
            this.code = code;
            this.name = name;
        }

        public CCodeTableElement(String code, String name, E contextObject) {
            this.code = code;
            this.name = name;
            this.contextObject = contextObject;
        }

        public String GetCode() {
            return StringUtils.RNN(this.code);
        }

        public String GetName() {
            return StringUtils.RNA(this.name);
        }

        public virtual E GetContextObject() {
            return this.contextObject;
        }

        public override String ToString() {
            return GetName();
        }

        public override bool Equals(Object obj) {
            if(obj is CCodeTableElement<E>) {
                CCodeTableElement<E> cte = (CCodeTableElement<E>)obj;
                return cte.GetCode().Equals(this.GetCode());
            }
            return false;
        }

        public override int GetHashCode() {
            return GetCode().GetHashCode();
        }
    }







}
