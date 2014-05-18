//
// Copyright (C) 2008 OOIT.com AG, Zürich CH
// All rights reserved.
//
using System;
using System.IO;
using System.Collections.Generic;
using System.Collections;

namespace Org.JGround.HWT {

    using Org.JGround.Util;

    public static class HDTD {

        public static class AttName {
            public static readonly String ACTION = "action";
            public static readonly String ALIGN = "align";
            public static readonly String BGCOLOR = "bgcolor";
            public static readonly String BORDER = "border";
            public static readonly String CELLPADDING = "cellpadding";
            public static readonly String CELLSPACING = "cellspacing";
            public static readonly String CHECKED = "checked";
            public static readonly String CLASS = "class";
            public static readonly String COLS = "cols";
            public static readonly String COLSPAN = "colspan";
            public static readonly String ENCTYPE = "enctype";
            public static readonly String ID = "id";

            public static readonly String HEIGHT = "height";
            public static readonly String HREF = "href";
            public static readonly String MEDIA = "media";
            public static readonly String METHOD = "method";
            public static readonly String MULTIPLE = "multiple";
            public static readonly String NAME = "name";
            public static readonly String NOWRAP = "nowrap";
            public static readonly String ONCLICK = "onclick";
            public static readonly String ONLOAD = "onload";
            public static readonly String ONFOCUS = "onFocus";
            public static readonly String REL = "rel";
            public static readonly String RADIO = "radio";
            public static readonly String ROWS = "rows";
            public static readonly String ROWSPAN = "rowspan";
            public static readonly String SELECTED = "selected";
            public static readonly String SIZE = "size";
            public static readonly String SPAN = "span";
            public static readonly String SRC = "src";
            public static readonly String STYLE = "style";
            public static readonly String TARGET = "target";
            public static readonly String TITLE = "title";
            public static readonly String TYPE = "type";
            public static readonly String VALIGN = "valign";
            public static readonly String VALUE = "value";
            public static readonly String WIDTH = "width";
            public static readonly String WRAP = "wrap";
        }

        public static class Element {

            public static readonly String A = "a";
            public static readonly String BODY = "body";
            public static readonly String BASE = "base";
            public static readonly String BORDER = "border";
            public static readonly String BR = "br";
            public static readonly String CAPTION = "caption";
            public static readonly String DIV = "div";
            public static readonly String FORM = "form";
            public static readonly String HTML = "html";
            public static readonly String IMG = "img";
            public static readonly String INPUT = "input";
            public static readonly String HEAD = "head";
            public static readonly String LINK = "link";
            public static readonly String LI = "li";
            public static readonly String META = "meta";
            public static readonly String HR = "hr";
            public static readonly String H1 = "h1";
            public static readonly String H2 = "h2";
            public static readonly String H3 = "h3";
            public static readonly String OL = "ol";
            public static readonly String OPTION = "option";
            public static readonly String P = "p";
            public static readonly String SCRIPT = "script";
            public static readonly String SELECT = "select";
            public static readonly String SPAN = "span";
            public static readonly String TITLE = "title";
            public static readonly String TABLE = "table";
            public static readonly String TEXTAREA = "textarea";
            public static readonly String TD = "td";
            public static readonly String TR = "tr";
            public static readonly String TH = "th";
            public static readonly String THEAD = "thead";
            public static readonly String TBODY = "tbody";
            public static readonly String TFOOT = "tfood";
            public static readonly String UL = "ul";
        }

        public static class AttValue {

            public static readonly String _BLANK = "_blank";
            public static readonly String BUTTON = "button";
            public static readonly String BOTTOM = "bottom";
            public static readonly String CHECKBOX = "checkbox";
            public static readonly String CENTER = "center";
            public static readonly String HIDDEN = "hidden";
            public static readonly String LEFT = "left";
            public static readonly String MAXLENGTH = "maxlength";
            public static readonly String PASSWORD = "password";
            public static readonly String RESET = "reset";
            public static readonly String RIGHT = "right";
            public static readonly String RADIO = "radio";
            public static readonly String SEARCH = "search";
            public static readonly String SIZE = "size";
            public static readonly String STYLESHEET = "stylesheet";
            public static readonly String SUBMIT = "submit";
            public static readonly String TEXT = "text";
            public static readonly String TOP = "top";
            public static readonly String VIRTUAL = "virtual";
            public static readonly String TRUE = "true";

        }
    }

    public class HAttribute {

        protected String name;
        protected String value;

        public HAttribute(String name, String value) {
            this.name = name;//.ToLower();
            this.value = value;
        }

        public String GetName() {
            return name;
        }

        public String GetValue() {
            return value;
        }

        public void SetValue(String value) {
            this.value = value;
        }
    }

    public abstract class HComponent {

        public static readonly HText SPACE_TX = new HText(" ");
        public static readonly NBSP NBSP = new NBSP();

        internal HContainer parent;
        private Object serverSideObject;

        private Dictionary<String, HAttribute> attributes;

        public HComponent(HAttribute[] attArr) {
            if(attArr == null)
                return;
            for(int i = 0; i < attArr.Length; i++)
                SetAttribute(attArr[i]);
        }

        public HComponent() { }

        public virtual HAttribute GetAttribute(String name) {
            if(attributes == null) {
                return null;
            } else {
                return ((HAttribute)attributes.Get(name));
            }
        }

        public virtual String GetAttributeValue(String name) {
            if(attributes == null)
                return null;
            HAttribute att = ((HAttribute)attributes[name]);
            if(att == null)
                return null;
            return att.GetValue();
        }

        public virtual String GetStyleClass() {
            return GetAttributeValue(HDTD.AttName.CLASS);
        }

        public virtual HComponent SetStyleClass(String styleClass) {
            SetAttribute(HDTD.AttName.CLASS, styleClass);
            return this;
        }

        public virtual HComponent SetStyleClass(params String[] styleClasses) {
            SetAttribute(HDTD.AttName.CLASS, ArrayUtils.Join(styleClasses, " "));
            return this;
        }

        public virtual HComponent SetCss(String css) {
            this.SetAttribute(HDTD.AttName.STYLE, css);
            return this;
        }

        public virtual void RemoveStyleClass()
        {
            RemoveAttribute(HDTD.AttName.CLASS);
        }

        public virtual void RemoveCss()
        {
            RemoveAttribute(HDTD.AttName.STYLE);
        }

        protected virtual void CloseAngleBracket(TextWriter w)
        {
            w.Write('>');
        }

        protected virtual void PrintEndTag(TextWriter w, String tag) {
            w.Write("</");
            w.Write(tag);
            CloseAngleBracket(w);
        }

        protected virtual void PrintOpenTag(TextWriter w, String tag) {
            w.Write('<');
            w.Write(tag);
            HAttribute att;
            if(attributes != null) {
                IEnumerable<String> e = GetAttributeNames();

                foreach(String attributeName in e) {
                    att = (HAttribute)GetAttribute(attributeName);
                    w.Write(" ");
                    w.Write(att.GetName());
                    if(att.GetValue() != null) {
                        w.Write("=");
                        w.Write("\"");
                        w.Write(att.GetValue());
                        w.Write("\"");
                    }
                }
            }
            CloseAngleBracket(w);
        }

        public virtual IEnumerable<String> GetAttributeNames() {
            return attributes.Keys;
        }

        public abstract void PrintTo(TextWriter w);

        public virtual void RemoveAttribute(String name) {
            if(attributes == null) {
                return;
            }
            attributes.Remove(name);
        }

        public virtual void SetAttribute(HAttribute att) {
            if(attributes == null) {
                attributes = new Dictionary<String, HAttribute>();
            }
            attributes.Put(att.GetName(), att);
        }

        public virtual void SetAttribute(String name) {
            SetAttribute(name, null);
        }

        public virtual void SetAttribute(String name, int value) {
            SetAttribute(name, "" + value);
        }

        public virtual void SetAttribute(String name, long value) {
            SetAttribute(name, "" + value);
        }

        public virtual void SetAttribute(String name, float value) {
            SetAttribute(name, "" + value);
        }

        public virtual void SetAttribute(String name, double value) {
            SetAttribute(name, "" + value);
        }

        public virtual void SetAttribute(String name, bool value) {
            SetAttribute(name, "" + value);
        }

        public virtual void SetAttribute(String name, String value) {
            // CHECK
            if(attributes == null)
                attributes = new Dictionary<String, HAttribute>();

            String name_l = name.ToLower();
            foreach(HAttribute att_ in attributes.Values) {
                if(att_.GetName().ToLower().Equals(name_l) && !name.Equals(att_.GetName())) {
                    throw new Exception("Dublicate attribute with different cap size! " + name + " <-> " + att_.GetName());
                }
            }


            HAttribute att = attributes.Get(name);
            if(att == null) {
                att = new HAttribute(name, value);
                attributes.Put(att.GetName(), att);
            } else {
                att.SetValue(value);
            }
        }

        public bool HasAttribute(String name) {
            return attributes != null && attributes.ContainsKey(name);
        }

        public virtual HContainer SetText(String text) {
            return null;
        }

        public virtual String GetText() {
            return "";
        }

        protected bool enable = true;


        public virtual HContainer GetParent() {
            return parent;
        }

        public virtual HComponent GetHText() {
            return null;
        }

        public virtual void AddServersideObject(Object obj) {
            serverSideObject = obj;
        }
        public virtual Object GetServerSideObject() {
            return this.serverSideObject;
        }

    }

    //public interface IHContainer {
    //    HComponent Get(int index);
    //    int Count();
    //}

    public class HContainer : HComponent {

        private static readonly HComponent[] emptyHComponentsArray = new HComponent[0];

        private static Logger logger = Logger.GetLogger(typeof(HContainer));

        // protected PropertyChangeSupport psupport;

        protected List<HComponent> hComponents = null;

        //public HContainer() {
        //}

        public HContainer(params HComponent[] components) {
            foreach(HComponent component in components) {
                Add(new HComponent[] { component });
            }
        }

        private List<HComponent> InitHComponents() {
            if(hComponents == null) {
                hComponents = new List<HComponent>();
            }
            return hComponents;
        }

        /**
         * Add a HComponent to this HContainer. If the HComponent has no FONT set,
         * the FONT of the container will expicitly assigned to the HComponent.
         */
        public virtual HComponent Add(params HComponent[] hcomp) {
            InitHComponents();
            for(int i = 0; i < hcomp.Length; i++) {
                hComponents.Add(hcomp[i]);
                hcomp[i].parent = this;
            }
            return this;
        }


        /**
         * Add a HComponent to this HContainer. If the HComponent has no FONT set,
         * the FONT of the container will expicitly assigned to the HComponent.
         */
        public virtual void RemoveAll() {
            if(ICollectionUtils.IsNotEmpty(hComponents)) {
                hComponents.Clear();
            }
        }

        /**
         * Remove all existing entries from the container and Add all components in
         * the component array.
         * 
         * @param components
         *            the components which replaces the current entries.
         */
        //public virtual void SetAll(HComponent component) {
        //    SetAll(new HComponent[] { component });
        //}

        public virtual HContainer Set(params HComponent[] components) {
            InitHComponents();
            hComponents.Clear();
            for(int i = 0; i < components.Length; i++) {
                hComponents.Add(components[i]);
            }
            return this;
        }

        /**
         * Returns all components as a array;
         * 
         * @return the components in the container.
         */
        public virtual HComponent[] GetAll() {
            if(ICollectionUtils.IsEmpty(hComponents)) {
                return emptyHComponentsArray;
            }
            return (HComponent[])hComponents.ToArray();
        }

        public virtual HComponent Set(int i, HComponent hcomp) {
            InitHComponents();
            hComponents.RemoveAt(i);
            hComponents.Insert(i, hcomp);
            hcomp.parent = this;
            return this;
        }

        public virtual HComponent Add(int i, HComponent hcomp) {
            InitHComponents();
            hComponents.Insert(i, hcomp);
            hcomp.parent = this;
            return this;
        }

        public virtual HComponent Get(int index) {
            HComponent c = null;
            try {
                c = hComponents[index];
            }
            catch(Exception e) {
                logger.Warn(e);
            }
            return c;
        }

        protected virtual void PrintComponentsTo(TextWriter w) {
            for(int i = 0; i < Count(); i++) {
                HComponent c = Get(i);
                if(c != null) {
                    c.PrintTo(w);
                } else {
                    logger.Warn("component " + i + " was null (in class: " + this.GetType() + ")"
                            + " parent class (in class: "
                            + (parent != null ? parent.GetType().ToString() : "null") + ")");
                }
            }
        }

        public override void PrintTo(TextWriter w) {
            PrintComponentsTo(w);
        }

        public virtual int Count() {
            if(ICollectionUtils.IsEmpty(hComponents)) {
                return 0;
            }
            return hComponents.Count;
        }


        public virtual bool Remove(HComponent hcomponent) {
            if(hcomponent != null && ICollectionUtils.IsNotEmpty(hComponents)) {
                bool rm = hComponents.Remove(hcomponent);
                if(rm) {
                    hcomponent.parent = null;
                }
                return rm;
            }
            return false;
        }

        public virtual HComponent Remove(int index) {
            HComponent comp = hComponents[index];
            hComponents.RemoveAt(index);
            if(parent == this) {
                comp.parent = null;
            }
            return comp;
        }

        public virtual void AddSpace() {
            Add(new HComponent[] { HComponent.SPACE_TX });
        }

    }


    public class HText : HComponent {

        public static readonly HText NULLText = new HText("");

        protected String text = "";

        public HText() { }

        public HText(String text) {
            this.text = text;
        }

        public override void PrintTo(TextWriter w) {
            printContent(w);
        }

        public override HComponent GetHText() {
            return this;
        }

        protected void printContent(TextWriter w) {
            if(StringUtils.IsNotEmpty(text)) {
                PrintHtmlString(w, text);
            }
        }

        public void PrintHtmlString(TextWriter w, String str) {
            for(int i = 0; i < str.Length; i++) {
                char c = str[i];
                switch(c) {
                    case '\n':
                        w.Write("<BR>\n");
                        break;
                    case '&':
                        w.Write("&amp;");
                        break;
                    case '<':
                        w.Write("&lt;");
                        break;
                    case '>':
                        w.Write("&gt;");
                        break;
                    case '"':
                        w.Write("&quot;");
                        break;
                    // case ' ' : w.Write("&auml;");
                    default:
                        w.Write(c);
                        break;
                }
            }
        }

        public override HContainer SetText(String text) {
            this.text = text;
            return null;
        }

        public override String GetText() {
            return text;
        }

    }


    //
    //
    //


    public class HTag : HContainer, IPreListener {

        private static readonly Logger logger = Logger.GetLogger(typeof(HTag));

        public static readonly String[][] EmptyAttList = { };
        /**
         * Break Tag <code>BR</code>.
         */
        public static readonly HTag BR = new HSingleTag(HDTD.Element.BR);

        /**
         * Default Horizontal Ruler Tag <code>HR</code>.
         */
        //public static readonly HTag HR = new HSingleTag(HDTD.Element.HR);

        protected String tagName;
        protected HText htext;
        //private bool added = false;

        public HTag(String tagName, params HComponent[] hcomponents)
            : this(tagName, null, hcomponents) {
        }

        public HTag(String tagName, String[][] att_value_arr, params HComponent[] hcomponents)
            : this(tagName) {

            if(att_value_arr != null) {
                for(int i = 0; i < att_value_arr.Length; i++) {
                    String[] att_val = att_value_arr[i];
                    if(att_val.Length > 1) {
                        this.SetAttribute(att_val[0], att_val[1]);
                    } else if(att_val.Length == 1) {
                        this.SetAttribute(att_val[0], null);
                    }
                }
            }

            for(int i = 0; i < hcomponents.Length; i++) {
                if(hcomponents[i] != null) {
                    this.Add(hcomponents[i]);
                }
            }
        }

        public HTag(String tagName) {
            this.tagName = tagName;
            this.htext = new HText();
        }

        public override void PrintTo(TextWriter pr) {
            RegisterSubmitEventOnPrint();
            PrintOpenTag(pr, tagName);
            PrintComponentsTo(pr);
            PrintEndTag(pr, tagName);
            pr.WriteLine();
        }

        protected override void CloseAngleBracket(TextWriter w) {
            w.Write('>');
            w.WriteLine();
        }

        public override HContainer SetText(String text) {
            RemoveAll();
            if(text == null) {
                return this;
            }
            if(htext == null) {
                htext = new HText();
            }
            htext.SetText(text);
            Add(htext);
            return this;
        }

        public override String GetText() {
            return htext.GetText();
        }

        public override HComponent GetHText() {
            return htext;
        }

        //
        // SUBMIT LISTENER LOGIC
        //

        private IEventSource submitEventSource;

        public void InitSubmitEventSource(IEventSource submitEventSource) {
            this.submitEventSource = submitEventSource;
        }

        private List<IHListener> submitListeners = new List<IHListener>();

        protected virtual List<IHListener> GetSubmitListeners() {

            if(submitListeners == null) {
                submitListeners = new List<IHListener>();
            }
            return submitListeners;
        }

        public void AddSubmitListener(IHListener submitListener) {
            if(submitEventSource == null) {
                return;
            } else {
                logger.Warn(this + ": Submit hevent source not initialized!");
            }
            List<IHListener> submitListeners = GetSubmitListeners();
            if(!submitListeners.Contains(submitListener)) {
                submitListeners.Add(submitListener);
            }
        }

        public void RemoveSubmitListener(IHListener submitListener) {
            GetSubmitListeners().Remove(submitListener);
        }

        protected void RegisterSubmitEventOnPrint() {
            if(submitEventSource != null && submitListeners != null && submitListeners.Count > 0) {
                submitEventSource.RegisterSubmit(this);
            }
        }

        public virtual void PreArrived(HEvent hevent) {
            FireSubmitEvent(hevent);
        }

        protected void FireSubmitEvent(HEvent hevent) {
            if(hevent.GetSource() != this) {
                logger.Error("Source != this - this is un - expected");
                return;
            }
        }


        public class HR : HSingleTag {

            public HR() : base(HDTD.Element.HR) { }

            public HR(String styleClass)
                : base(HDTD.Element.HR) {
                SetStyleClass(styleClass);
            }

        }

    }

    public class CHECKBOX : HTag {

        private HText labeltx = new HText("");

        public CHECKBOX(String name)
            : base(HDTD.Element.INPUT) {
            SetAttribute(HDTD.AttName.NAME, name);
            SetAttribute(HDTD.AttName.TYPE, HDTD.AttValue.CHECKBOX);
            Add(labeltx);
        }

        public virtual void SetChecked(bool check) {
            if(check) {
                SetAttribute(HDTD.AttName.CHECKED);
                htext.SetText(labeltx.GetText());
            } else {
                RemoveAttribute(HDTD.AttName.CHECKED);
                htext.SetText("");
            }
        }

        public virtual bool IsChecked() {
            return GetAttribute(HDTD.AttName.CHECKED) != null;
        }

        /*
         * public void setContent(String content) { if (content != null &&
         * content.length() > 0) setChecked(true); else setChecked(false); }
         */

        public virtual void SetValue(String value) {
            SetAttribute(HDTD.AttName.VALUE, value);
        }

        public override HContainer SetText(String text) {
            labeltx.SetText(text);
            htext.SetText(text);
            return this;
        }

        public virtual String getValue() {
            return GetAttributeValue(HDTD.AttName.VALUE);
        }

    }

    public class BODY : HContainer {

        public override void PrintTo(TextWriter pr) {
            PrintOpenTag(pr, "body");
            PrintComponentsTo(pr);
            PrintEndTag(pr, "body");
        }
    }

    public class BR : HSingleTag {

        public readonly static BR element = new BR();

        public BR()
            : base(HDTD.Element.BR) {
        }

    }

    public class HSingleTag : HTag {

        public HSingleTag(String tagName)
            : base(tagName) {
        }

        public HSingleTag(String tagName, String[][] att_value_arr)
            : this(tagName) {
            for(int i = 0; i < att_value_arr.Length; i++) {
                String[] att_val = att_value_arr[i];
                if(att_val.Length > 1) {
                    this.SetAttribute(att_val[0], att_val[1]);
                } else if(att_val.Length == 1) {
                    this.SetAttribute(att_val[0], null);
                }
            }
        }

        public override void PrintTo(TextWriter pr) {
            base.RegisterSubmitEventOnPrint();
            PrintOpenTag(pr, tagName);
        }

        protected override void CloseAngleBracket(TextWriter pr) {
            pr.Write(" />");
        }
    }

    public class DIV : HTag {

        public DIV() : this(default(String)) { }

        public DIV(String content)
            : base(HDTD.Element.DIV) {
            this.SetText(content);
        }

        public DIV(String content, String styleClass)
            : base(HDTD.Element.DIV) {
            this.SetText(content);
            SetAttribute(HDTD.AttName.CLASS, styleClass);
        }

        public DIV(params HComponent[] hcomponents)
            : base(HDTD.Element.DIV, hcomponents) {
        }


        public DIV(HComponent hcomponent, params String[] styleClasses)
            : base(HDTD.Element.DIV) {
            Add(hcomponent);
            SetStyleClass(styleClasses);
        }

        public DIV(String styleClass, params HComponent[] hcomponents)
            : base(HDTD.Element.DIV, HTag.EmptyAttList, hcomponents) {
            SetAttribute(HDTD.AttName.CLASS, styleClass);
        }

    }


    public class FORM : HTag {

        public static readonly String POST = "POST";
        public static readonly String GET = "GET";
        public static readonly String DELETE = "DELETE";

        public FORM() : this(POST, "") { }

        public FORM(String method) : this(method, "") { }

        public FORM(String method, String actioncmd) : this(method, actioncmd, "application/x-www-form-urlencoded") { }

        public FORM(String method, String actioncmd, String enctype)
            : base(HDTD.Element.FORM) {

            SetAttribute(new HAttribute(HDTD.AttName.METHOD, method));
            SetAttribute(new HAttribute(HDTD.AttName.ACTION, actioncmd));
            if(enctype != null) {
                SetAttribute(new HAttribute(HDTD.AttName.ENCTYPE, enctype));
            }
        }
    }


    public class H1 : HTag {

        public H1(String text)
            : base(HDTD.Element.H1) {
            this.SetText(text);
        }

    }

    public class H2 : HTag {

        public H2(String text)
            : base(HDTD.Element.H2) {
            this.SetText(text);
        }

    }

    public class H3 : HTag {

        public H3(String text)
            : base(HDTD.Element.H3) {
            this.SetText(text);
        }

    }


    public class A : HTag {

        public A()
            : base(HDTD.Element.A) {
        }

        public A(String text)
            : this() {
            this.SetText(text);
        }

        public A(String text, String hrefValue)
            : this(text) {
            SetAttribute(HDTD.AttName.HREF, hrefValue);
        }

        public A(String text, String hrefValue, String target)
            : this(text, hrefValue) {
            SetAttribute(HDTD.AttName.TARGET, target);
        }

    }


    public class HEAD : HTag {

        private HTag htitle;
        private HText text;

        public HEAD()
            : base(HDTD.Element.HEAD) {
        }

        public HEAD(String title)
            : this() {
            SetTitle(title);
        }

        public void SetTitle(String title) {
            if(htitle == null) {
                htitle = new HTag(HDTD.Element.TITLE);
                text = new HText("");
                htitle.Add(text);
                Add(htitle);
            }
            text.SetText(title);
        }

        public void AddCss(String cssHref)
        {
            HTag link = new HTag(HDTD.Element.LINK);
            link.SetAttribute(HDTD.AttName.REL, "stylesheet");
            link.SetAttribute(HDTD.AttName.TYPE, "text/css");
            link.SetAttribute(HDTD.AttName.HREF, cssHref);
            Add(link);
        }
      //  <script type="text/javascript" src="uploadify/jquery.uploadify.js"></script>
        public void AddJavaScript(String javascriptSrc)
        {
            HTag script = new HTag(HDTD.Element.SCRIPT);
            script.SetAttribute(HDTD.AttName.SRC, javascriptSrc);
            script.SetAttribute(HDTD.AttName.TYPE, "text/javascript");
            Add(script);
        }

        /**
         * Setting the base URL for the relative links in the HTML document
         */
        public void SetBase(String href) {
            HTag b = new HTag(HDTD.Element.BASE);
            base.SetAttribute(HDTD.AttName.HREF, href);
            Add(b);
        }
    }



    public class HTML : HComponent {

        private HEAD head;
        private BODY body;
        private LINK styleLink = null;
        private LINK styleLinkForPrint = null;

        public HTML() : this(new HEAD(), new BODY()) { }

        public HTML(HEAD head, BODY body) {
            this.head = head;
            this.body = body;
        }

        public override void PrintTo(TextWriter pr) {
            PrintOpenTag(pr, HDTD.Element.HTML);
            if(head != null)
                head.PrintTo(pr);
            if(body != null)
                body.PrintTo(pr);
            PrintEndTag(pr, HDTD.Element.HTML);
        }

        public void SetBODY(BODY body) {
            this.body = body;
        }

        public BODY GetBODY() {
            return body;
        }

        public void SetHEAD(HEAD head) {
            this.head = head;
        }

        public HEAD GetHEAD() {
            return head;
        }
        /*
         * 
        <link rel="stylesheet" media="screen" href="website.css">
        <link rel="stylesheet" media="print, embossed" href="druck.css">
         * 
         */
        public void SetStyleSheet(String stylesheet) {
            if(styleLink == null) {
                styleLink = new LINK();
                styleLink.SetAttribute(HDTD.AttName.REL, HDTD.AttValue.STYLESHEET);
                styleLink.SetAttribute(HDTD.AttName.TYPE, "text/css");
                styleLink.SetAttribute(HDTD.AttName.TITLE, "Style");
                styleLink.SetAttribute(HDTD.AttName.MEDIA, "screen");
                head.Add(styleLink);
            }
            styleLink.SetAttribute(HDTD.AttName.HREF, stylesheet);
        }

        public void SetStyleSheetForPrint(String stylesheetForPrint) {
            if(styleLinkForPrint == null) {
                styleLinkForPrint = new LINK();
                styleLinkForPrint.SetAttribute(HDTD.AttName.REL, HDTD.AttValue.STYLESHEET);
                styleLinkForPrint.SetAttribute(HDTD.AttName.TYPE, "text/css");
                styleLinkForPrint.SetAttribute(HDTD.AttName.TITLE, "Style");
                styleLinkForPrint.SetAttribute(HDTD.AttName.MEDIA, "print");
                head.Add(styleLinkForPrint);
            }
            styleLinkForPrint.SetAttribute(HDTD.AttName.HREF, stylesheetForPrint);
        }

    }



    public class HttpQuery {

        private String url;
        private Dictionary<String, String> parameter;

        public HttpQuery(String url) {
            this.url = url;
            parameter = new Dictionary<String, String>(5);
        }

        public void setUrlAction(String url) {
            this.url = url;
        }

        public void setParameter(String name, String value) {
            if(value == null || name == null)
                return;
            parameter.Put(name, value);
        }

        public String GetParameter(String name) {
            return (String)parameter.Get(name);
        }

        public override String ToString() {
            String tmp = url + "?";
            int i = 0;
            foreach(KeyValuePair<String, String> kv in parameter) {
                if(i > 0)
                    tmp += "&";
                tmp += kv.Key + "=" + kv.Value;
                i++;
            }
            return tmp;
        }

    }


    public class IMG : HSingleTag {

        public IMG()
            : base(HDTD.Element.IMG) {
            SetAttribute(HDTD.AttName.BORDER, "0");
        }

        public IMG(String[][] attValList)
            : base(HDTD.Element.IMG, attValList) {

        }

        public IMG(String source)
            : this() {
            SetAttribute(HDTD.AttName.SRC, source);
        }

    }



    public class INPUT : HSingleTag {

        public static class HType {
            public static readonly String FILE = "file";
            public static readonly String SUBMIT = "SUBMIT";
            public static readonly String HIDDEN = "HIDDEN";
        }

        public const short HIDDEN = 0;
        public const short PASSWORD = 1;
        public const short CHECKBOX = 2;
        public const short RADIO = 3;
        public const short SUBMIT = 4;
        public const short RESET = 5;
        public const short TEXT = 6;
        public const short FILE = 7;
        public const short BUTTON = 8;
        public const short SEARCH = 9;
        public const short EMAIL = 10;
        //
        private short inputType;

        public INPUT(short inputType) : this(inputType, "", null) { }

        public INPUT(short inputType, String name, String value)
            : base(HDTD.Element.INPUT) {
            this.inputType = inputType;
            SetAttribute(HDTD.AttName.NAME, name);
            if(value != null) {
                SetAttribute(HDTD.AttName.VALUE, value);
            }
            switch(inputType) {
                case HIDDEN:
                    SetAttribute(HDTD.AttName.TYPE, HDTD.AttValue.HIDDEN);
                    break;
                case PASSWORD:
                    SetAttribute(HDTD.AttName.TYPE, HDTD.AttValue.PASSWORD);
                    break;
                case BUTTON:
                    SetAttribute(HDTD.AttName.TYPE, HDTD.AttValue.BUTTON);
                    break;
                case CHECKBOX:
                    SetAttribute(HDTD.AttName.TYPE, HDTD.AttValue.CHECKBOX);
                    break;
                case RADIO:
                    SetAttribute(HDTD.AttName.TYPE, HDTD.AttValue.RADIO);
                    break;
                case SUBMIT:
                    SetAttribute(HDTD.AttName.TYPE, HDTD.AttValue.SUBMIT);
                    break;
                case RESET:
                    SetAttribute(HDTD.AttName.TYPE, HDTD.AttValue.RESET);
                    break;
                case SEARCH:
                    SetAttribute(HDTD.AttName.TYPE, HDTD.AttValue.SEARCH);
                    break;
                default:
                    SetAttribute(HDTD.AttName.TYPE, HDTD.AttValue.TEXT);
                    break;
            }
            text = new HText();
        }

        public INPUT(String name) : this(TEXT, name, null) { }

        public INPUT(short inputType, String name) : this(inputType, name, null) { }
        public void setContent(String value) {
            SetValue(value);
        }

        public void SetValue(String value) {
            SetAttribute("VALUE", value);
        }

        public virtual void SetLabel(String label)
        {
            base.SetText(label);
        }

        public virtual void SetTitle(String title)
        {
            SetAttribute(HDTD.AttName.TITLE,title);
        }

        private HText text;

        public override void PrintTo(TextWriter pr) {
            if(enable)
                base.PrintTo(pr);
            else {
                text.SetText(GetAttributeValue("VALUE"));
                text.PrintTo(pr);
            }
        }

        public override HComponent GetHText() {
            if(inputType == HIDDEN)
                return this;
            if(GetAttributeValue("VALUE") != null)
                text.SetText(GetAttributeValue("VALUE"));
            else
                text.SetText("");
            return text;
        }

    }



    public class LINK : HSingleTag {

        public LINK() : base(HDTD.Element.LINK) { }

    }



    public class P : HTag {

        public P()
            : base(HDTD.Element.P) {

        }

        public P(String content)
            : base(HDTD.Element.P) {

            this.SetText(content);
        }

    }





    public class RADIO : HTag {

        private HText labeltx = new HText("");

        public RADIO(String name)
            : base(HDTD.Element.INPUT) {

            SetAttribute(HDTD.AttName.NAME, name);
            SetAttribute(HDTD.AttName.TYPE, HDTD.AttValue.RADIO);
            Add(labeltx);
        }

        public virtual void setChecked(bool check) {
            if(check) {
                SetAttribute(HDTD.AttName.CHECKED);
                htext.SetText(labeltx.GetText());
            } else {
                RemoveAttribute(HDTD.AttName.CHECKED);
                htext.SetText("");
            }
        }

        public virtual bool isChecked() {
            return GetAttribute(HDTD.AttName.CHECKED) != null;
        }

        public override HContainer SetText(String text) {
            labeltx.SetText(text);
            htext.SetText(text);
            return this;
        }


        public virtual String getValue() {
            return GetAttributeValue(HDTD.AttName.VALUE);
        }

        /**
         * Don't use this method in the <code>HCheckBox</code> class 
         * when used together with the <code>ButtonGroup</code>.
         */
        public virtual void setValue(String value) {
            SetAttribute(HDTD.AttName.VALUE, value);
        }




    }
    public class ResetButton : INPUT {

        public ResetButton(String label)
            : base(RESET, "") {

            SetAttribute(HDTD.AttName.VALUE, label);
        }


        public override void SetLabel(String label) {
            SetAttribute(HDTD.AttName.VALUE, label);
        }


    }
    public class SELECT : HTag {

        public SELECT(String name) : this(name, false, 1) { }

        public SELECT(String name, bool multipleSelection, int size)
            : base(HDTD.Element.SELECT) {

            SetAttribute(HDTD.AttName.NAME, name);
            if(multipleSelection) {
                SetAttribute(HDTD.AttName.MULTIPLE);
            }
            if(size > 1) {
                SetAttribute(HDTD.AttName.SIZE, "" + size);
            }
        }

        public void addOption(bool select, String value, HText text) {
            HTag option = new HTag(HDTD.Element.OPTION);
            if(select)
                option.SetAttribute(HDTD.AttName.SELECTED);
            if(value != null)
                option.SetAttribute(HDTD.AttName.VALUE, value);
            option.Add(text);
            Add(option);
        }

        public void Clear() {
            for(int i = 0; i < Count(); i++) {
                Get(i).RemoveAttribute(HDTD.AttName.SELECTED);
            }
        }

        public override HContainer SetText(String value) {
            setSelected(value);
            return this;
        }

        public void SetSelected(String[] values) {
            HComponent hcom;

            for(int i = 0; i < Count(); i++) {
                hcom = Get(i);
                hcom.RemoveAttribute(HDTD.AttName.SELECTED);
                for(int k = 0; k < values.Length; k++) {
                    if(hcom.GetAttributeValue(HDTD.AttName.VALUE).Equals((values[k]))) {
                        hcom.SetAttribute(HDTD.AttName.SELECTED);
                    }
                }
            }
        }

        public void setSelected(String value) {
            SetSelected(new String[] { value });
        }

        private TABLE table = new TABLE();

        public override HComponent GetHText() {
            HTag option;
            table.RemoveAll();
            for(int i = 0; i < Count(); i++) {
                option = (HTag)Get(i);
                if(option.GetAttribute(HDTD.AttName.SELECTED) != null) {
                    table.Add(new TR(new TD(option.GetHText())));
                }
            }
            return table;
        }

    }



    public class SPAN : HTag {

        public SPAN()
            : base(HDTD.Element.SPAN) {
        }

        public SPAN(String content)
            : this() {
            this.SetText(content);
        }

        public SPAN(params HComponent[] hcomponents)
            : base(HDTD.Element.SPAN, HTag.EmptyAttList, hcomponents) {
        }

        public SPAN(String styleClass, params HComponent[] hcomponents)
            : base(HDTD.Element.SPAN, HTag.EmptyAttList, hcomponents) {
            SetStyleClass(styleClass);
        }

        public SPAN(String[] styleClasses, params HComponent[] hcomponents)
            : base(HDTD.Element.SPAN, HTag.EmptyAttList, hcomponents) {
            SetStyleClass(styleClasses);
        }

        public SPAN(String content, String styleClass)
            : this() {
            this.SetText(content);
            SetAttribute(HDTD.AttName.CLASS, styleClass);
        }

    }

    public class TABLE : HTag {

        public TABLE()
            : this(0, 0, 0) {
        }

        public TABLE(int border, int cellpadding, int cellspacing)
            : base(HDTD.Element.TABLE) {
            SetBorder(border, cellpadding, cellspacing);
        }

        public TABLE(TR tr)
            : this() {
            Add(tr);
        }

        public TABLE(TR[] comps)
            : this(EmptyAttList, comps) { }

        public TABLE(String[][] attValList, TR[] comps)
            : base(HDTD.Element.TABLE, attValList, comps) {
        }

        public TABLE(String[][] attValList, TR comp)
            : base(HDTD.Element.TABLE, attValList, comp) {

        }

        public void AddAsRow(HComponent[] comps) {
            TR tr = new TR();
            for(int i = 0; i < comps.Length; i++) {
                TD td = new TD(comps[i]);
                tr.Add(td);
            }
            Add(tr);
        }

        public void RemoveRow(int row) {
            Remove(row);
        }

        public void SetBorder(int border, int cellpadding, int cellspacing) {
            SetAttribute(HDTD.AttName.BORDER, border);
            SetAttribute(HDTD.AttName.CELLPADDING, cellpadding);
            SetAttribute(HDTD.AttName.CELLSPACING, cellspacing);
        }

    }

    public class THEAD : HTag {
        public THEAD()
            : base(HDTD.Element.THEAD) {
            //SetAttribute(HDTD.AttName.VALIGN, HDTD.AttValue.TOP);
        }

        public THEAD(TR tr) : base(HDTD.Element.THEAD, tr) { }

        public void SetRow(TR tr) {
            Set(tr);
        }

    }

    public class TBODY : HTag {
        public TBODY()
            : base(HDTD.Element.TBODY) {
            //SetAttribute(HDTD.AttName.VALIGN, HDTD.AttValue.TOP);
        }

        public TBODY(params TR[] trs) : base(HDTD.Element.THEAD, trs) { }

        public void SetRow(params TR[] trs) {
            Set(trs);
        }

    }

    public class TH : TD {

        public TH()
            : base() {
            this.tagName = HDTD.Element.TH;
        }

        public TH(String content)
            : base() {
            this.tagName = HDTD.Element.TH;
            this.SetText(content);
        }

        public TH(String content, String styleClass)
            : base() {
            this.tagName = HDTD.Element.TH;
            this.SetText(content);
            SetAttribute(HDTD.AttName.CLASS, styleClass);
        }

        public TH(params HComponent[] comps)
            : base(comps) {
            this.tagName = HDTD.Element.TH;
        }


    }



    public class TD : HTag {

        //private static readonly String[][] emptyAttList = new String[0][];

        public TD()
            : base(HDTD.Element.TD) {
            SetAttribute(HDTD.AttName.VALIGN, HDTD.AttValue.TOP);
        }

        public TD(params HComponent[] comps) : base(HDTD.Element.TD, comps) { }

        public TD(String styleClass, params HComponent[] comps) : base(HDTD.Element.TD, comps) {
            this.SetStyleClass(styleClass);
        }


        public TD(int columnSpan, params HComponent[] comps)
            : base(HDTD.Element.TD, comps) {
            SetAttribute(HDTD.AttName.COLSPAN, columnSpan);
        }

        public void SetCell(params HComponent[] hcomponents) {
            Set(hcomponents);
        }

        public void SetColSpan(int colspan) {
            SetAttribute(HDTD.AttName.COLSPAN, colspan);
        }

    }


    public class TEXTAREA : HTag {

        public TEXTAREA(String name, int cols, int rows, bool wrap)
            : base(HDTD.Element.TEXTAREA) {

            if(wrap)
                SetAttribute(HDTD.AttName.WRAP, HDTD.AttValue.VIRTUAL);
            SetAttribute(HDTD.AttName.NAME, name);
            SetSize(cols, rows);
        }

        public override void PrintTo(TextWriter pr) {
            base.RegisterSubmitEventOnPrint();
            PrintOpenTag(pr, tagName);
            pr.Write(GetText());
            PrintEndTag(pr, tagName);
            pr.WriteLine();
        }

        public void SetSize(int cols, int rows) {
            SetAttribute(HDTD.AttName.ROWS, rows + "");
            SetAttribute(HDTD.AttName.COLS, cols + "");
        }

        public void SetWrap(bool wrap) {
            //if (true)
            SetAttribute(HDTD.AttName.WRAP, HDTD.AttValue.VIRTUAL);
            //else
            //    removeAttribute(HDTD.AttName.WRAP);
        }

    }


    public class TR : HTag {

        public TR()
            : base(HDTD.Element.TR) {

        }

        public TR(params TD[] tds)
            : this() {
            foreach(TD td in tds) {
                if(td != null)
                    Add(td);
            }
        }

        public TR(String[][] attValList, params TD[] comps)
            : base(HDTD.Element.TR, attValList, comps) {

        }

        public void AddTD(params HComponent[] hcomponents) {
            Add(new TD(hcomponents));
        }

        public void AddTD(String text) {
            Add(new TD(new HText(text)));
        }

    }



    public class UL : HTag {

        public UL() : base(HDTD.Element.UL) { }

        public void AddLI(params HComponent[] hcomponents) {
            Add(new LI(hcomponents));
        }

        public void AddLI(String text) {
            Add(new LI(new HText(text)));
        }

    }



    public class LI : HTag {

        public LI()
            : base(HDTD.Element.LI) {
        }

        public LI(params HComponent[] hcomponents)
            : base(HDTD.Element.LI, hcomponents) {
        }

    }

}

