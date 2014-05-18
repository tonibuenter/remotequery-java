//
// Copyright (C) 2008 OOIT.com AG, Zürich CH
// All rights reserved.
//
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Principal;
using System.Text;
using System.Web;
using System.Web.SessionState;
using System.Xml;
using Org.JGround.Util;
using Org.JGround.Web;

namespace org.remotequery.web
{


 public class RemoteQueryServlet : IHttpHandler, IRequiresSessionState
    {

        //
        // CLASS LEVEL
        //
        private static Logger logger = Logger.GetLogger(typeof(RemoteQueryServlet));

        //
        // OBJECT LEVEL
        //
        private Dictionary<String, Object> attributes = new Dictionary<String, Object>();
        // private SessionFactory sessionFactory;
        // private Dictionary logout_urls = new Dictionary();
        private Dictionary<String, String> error_urls = new Dictionary<String, String>();

        public void ProcessRequest(HttpContext context)
        {
            HttpRequest request = context.Request;
            HttpResponse response = context.Response;

            response.ContentType = "text/html";
            response.ContentEncoding = Encoding.UTF8;

            HttpCookieCollection cookies = request.Cookies;
            logger.Debug("Nr of cookies: ", cookies.Count);
            //foreach (HttpCookie cookie in cookies) {
            //    logger.Debug("Path: " + cookie.Path + " Name: " + cookie.Name + " Value: " + cookie.Value);
            //}

            HWTEvent hevent = new HWTEvent(context);

            try
            {
                lock (Locker.MUTEX)
                {
                    String frameid = hevent.getFrameId();
                    logger.Debug("frameid", frameid);
                    HWTFrameFactory factory = null;
                    factory = HWTEventDispatcherInitializier.GetInstance(context).GetFactory(frameid);
                    if (factory == null)
                    {
                        logger.Warn("no factory found for : " + frameid);
                        return;
                    }
                    logger.Debug("factory", factory);
                    HFrame frame = factory.GetHFrame(hevent);
                    logger.Debug("frame", frame);
                    if (frame == null)
                    {
                        logger.Warn("no frame found for : " + frameid);
                    }
                    else
                    {
                        frame.ProcessEvent(hevent);
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error(e);
                throw e;
            }
        }

        public void SetAttribute(String key, Object value)
        {
            attributes.Put(key, value);
        }

        public Object GetAttribute(String key)
        {
            return attributes.Get(key);
        }

        public bool IsReusable
        {
            get { return false; }
        }

    }


    public interface IPreListener
    {
        void PreArrived(HEvent hevent);
    }


    public class ButtonGroup : IPreListener
    {

        protected String groupName;
        protected Dictionary<String, RadioButton> radioButtons;
        protected IEventSource hsource;

        public ButtonGroup(IEventSource hsource)
        {
            radioButtons = new Dictionary<String, RadioButton>();
            SetEventSource(hsource);
            this.hsource = hsource;
        }

        public virtual void PreArrived(HEvent hevent)
        {
            if (hevent.GetValues() == null || hevent.GetValues().Length == 0)
            {
                return;
            }
            String selected = hevent.GetValues()[0];
            foreach (RadioButton radioButton in radioButtons.Values)
            {
                radioButton.setSelected(radioButton == radioButtons.Get(selected));
            }
        }

        protected void SetEventSource(IEventSource es)
        {
            groupName = es.RegisterUpdate(this);
        }

        public void Add(RadioButton radioBt)
        {
            String name = radioBt.GetInitName();
            radioBt.setGroupName(groupName);
            radioButtons.Put(name, radioBt);
        }

    }

    public class DefaultListModel<E> : IListModel<E>
    {

        private List<E> list;

        public DefaultListModel()
        {
            list = new List<E>();
        }

        public void AddElement(E element)
        {
            list.Add(element);
        }

        public DefaultListModel(List<E> list)
        {
            this.list = list;
        }

        public void setElements(List<E> list)
        {
            this.list = list;
        }

        public int GetRowCount()
        {
            return list.Count;
        }

        public E GetValueAt(int row)
        {
            return list.Get(row);
        }
    }
    public class DefaultTableCellRenderer<E> : ICellRenderer<E>
    {

        private TD cell = new TD();

        public TD GetCell(HTable<E> table, Object value, bool isSelected, bool hasFocus, int row, int col)
        {
            cell.SetText(value.ToString());
            return cell;
        }

    }

    public class DefaultTableModel<E>

: ITableModel<E>
    {

        private E[][] values;

        public DefaultTableModel()
        {
        }

        /**
         * Constructor for DefaultTableModel.
         * 
         * @param arg0
         * @param arg1
         */
        public DefaultTableModel(E[][] values)
        {
            this.values = values;
        }

        public E GetValueAt(int row)
        {
            return values[row][0];
        }

        public int GetColumnCount()
        {
            if (values != null && values.Length > 0 && values[0] != null)
            {
                return values[0].Length;
            }
            return 0;
        }

        public int GetRowCount()
        {
            if (values != null)
            {
                return values.Length;
            }
            return 0;
        }

        public E GetValueAt(int row, int col)
        {
            // TODO Auto-generated method stub
            return values[row][col];
        }

    }

    public class DefaultTableRowRenderer<E> : ITableRowRenderer<E>
    {

        private DefaultRow render_tr = null;
        private ICellRenderer<E> cellRenderer = new DefaultTableCellRenderer<E>();
        private Dictionary<int, ICellRenderer<E>> rendererPerColumn = new Dictionary<int, ICellRenderer<E>>();

        public DefaultTableRowRenderer()
        {
            render_tr = new DefaultRow(this);
        }
        public ICellRenderer<E> getTableCellRenderer()
        {
            return cellRenderer;
        }

        public void SetCellRenderer(ICellRenderer<E> cellRenderer)
        {
            this.cellRenderer = cellRenderer;
        }

        public void SetCellRenderer(int column, ICellRenderer<E> cellRenderer)
        {
            rendererPerColumn.Put(column, cellRenderer);
        }

        public HComponent GetTableRowRendererComponent(HTable<E> table, bool isSelected,
                bool hasFocus, int row)
        {

            render_tr.setRow(table, row);
            return render_tr;
        }

        class DefaultRow : TR
        {
            private DefaultTableRowRenderer<E> rend;
            private HTable<E> table;
            private int row;

            public DefaultRow(DefaultTableRowRenderer<E> rend)
            {
                this.rend = rend;
            }
            public override int Count()
            {
                return table.getModel().GetColumnCount();
            }

            public void setRow(HTable<E> table, int row)
            {
                this.table = table;
                this.row = row;
            }

            public override HComponent Get(int col)
            {
                ICellRenderer<E> renderer = this.rend.rendererPerColumn
                        .Get(col);
                renderer = renderer == null ? this.rend.cellRenderer : renderer;
                return renderer.GetCell(table, table.getModel().GetValueAt(row,
                        col), false, false, row, col);
            }
        }
    }




    public class DefaultTextRenderer<E> : ITextRenderer<E>
    {
        public String GetRendererString(E obj, bool isSelected)
        {
            return ObjectUtils.ToString(obj);
        }

    }

    public abstract class HAbstractButton : HTag, IPreListener
    {

        // CLASS LEVEL
        private static Logger logger = Logger.GetLogger(typeof(HAbstractButton));

        protected String initName;
        protected String subId = "";

        private List<IHListener> listeners = new List<IHListener>();

        private HAttribute nameAttribute = new HAttribute(HDTD.AttName.NAME, "");

        public HAbstractButton(String tag)
            : base(tag)
        {
            SetAttribute(nameAttribute);
        }

        public virtual void AddHListener(IHListener al)
        {
            if (!listeners.Contains(al))
            {
                listeners.Add(al);
            }
        }

        public override void PreArrived(HEvent hevent)
        {

            if (hevent.GetSource() != this)
            {
                logger.Error("Source != this - this is un - expected");
                return;
            }
            SetSubId(hevent.GetSubEventId());
            foreach (IHListener listener in listeners)
            {
                listener.Arrived(hevent);
            }
        }

        protected virtual void SetEventSource(IEventSource es)
        {
            initName = es.RegisterAction(this);
            nameAttribute.SetValue(initName + "_" + subId);
        }

        public virtual String GetInitName()
        {
            return initName;
        }

        public virtual void SetSubId(String subId)
        {
            this.subId = subId;
            nameAttribute.SetValue(initName + "_" + subId);
        }

        public virtual String GetSubId()
        {
            return this.subId;
        }
    }

    public class HButton : HAbstractButton
    {

        public HButton(IEventSource hsource)
            : this(hsource, INPUT.HType.SUBMIT)
        {

        }

        public HButton(IEventSource hsource, String label)
            : base(HDTD.Element.INPUT)
        {

            SetAttribute(HDTD.AttName.TYPE, INPUT.HType.SUBMIT);
            SetAttribute(HDTD.AttName.VALUE, label);
            SetEventSource(hsource);
        }

    }
    public class HCheckBox : CHECKBOX, IPreListener
    {

        private String[] values;
        public String updateName;
        private IEventSource eventSource;

        public HCheckBox(IEventSource hsource)
            : this(hsource, false, "", "")
        {

        }

        public HCheckBox(IEventSource hsource, bool chcked)
            : this(hsource, chcked, "", "")
        {

        }

        public HCheckBox(
            IEventSource hsource,
            bool chcked,
            String value,
            String text)
            : base("")
        {
            SetEventSource(hsource);
            SetChecked(chcked);
            SetValue(value);
            SetText(text);
        }

        //public bool isSelected() {
        //    return this.IsChecked();
        //}

        //public void setSelected(bool selected) {
        //    SetChecked(selected);
        //}

        public override void SetValue(String value)
        {
            SetAttribute(HDTD.AttName.VALUE, value);
        }

        public String[] getValues()
        {
            return values;
        }

        public override String getValue()
        {
            if (values != null && values.Length > 0)
                return values[0];
            return null;
        }

        public override void PreArrived(HEvent hevent)
        {
            this.values = hevent.GetValues();
            SetChecked(true);
        }

        protected void SetEventSource(IEventSource eventSource)
        {
            this.eventSource = eventSource;
            this.updateName = eventSource.RegisterUpdate(this);
            SetAttribute(HDTD.AttName.NAME, updateName);
        }

        public override void PrintTo(TextWriter pr)
        {
            this.eventSource.FirePrintEvent(this);
            base.PrintTo(pr);
        }

        public String getUpdateName()
        {
            return updateName;
        }
    }

    public interface HDispatcher
    {
        void dispatch(HWTEvent hevent);
    }
    public class EventObject
    {

        private object source;
        public EventObject(object source)
        {
            this.source = source;
        }
        public virtual object GetSource()
        {
            return source;
        }
    }


    public interface IEventSource
    {
        String RegisterAction(IPreListener listener);
        String RegisterUpdate(IPreListener listener);
        String RegisterFile(IPreListener listener);
        void RegisterSubmit(IPreListener listener);
        void FirePrintEvent(HCheckBox listener);
    }

    public class HWTEvent
    {

        private static Logger logger = Logger.GetLogger(typeof(HWTEvent));
        //
        //
        //
        private HttpContext context;


        //  private TextWriter w;
        private String frameId;
        //   private String encodedUrl;
        private IPrincipal user;
        private HttpFileCollection uploadedFileCollection;
        private String encodedUrl;

        public HWTEvent(HttpContext context)
        {
            this.context = context;
            this.user = context.User;
            String requestpath = context.Request.Url.AbsoluteUri;
            int start = requestpath.LastIndexOf("/") + 1;
            int end = requestpath.LastIndexOf("?");
            if (end < start)
            {
                frameId = requestpath.Substring(start);
            }
            else
            {
                int length = end - start;
                frameId = requestpath.Substring(start, length);
            }
            uploadedFileCollection = context.Request.Files;
            encodedUrl = frameId;// response.encodeURL(contextPath + request.getServletPath() + "/" + frameId);
        }

        public IPrincipal GetUser()
        {
            return context.User;
        }

        public String GetSessionId()
        {
            return context.Session.SessionID;
        }

        public HttpSessionState GetSession()
        {
            return context.Session;
        }

        public void SetInSession(String key, Object value)
        {
            GetSession()[key] = value;
        }

        public Object GetFromSession(String key)
        {
            try
            {
                return GetSession()[key];
            }
            catch (Exception)
            {
                return null;
            }
        }

        internal HttpFileCollection getUploadedFileCollection()
        {
            return this.uploadedFileCollection;
        }

        public String getFrameId()
        {
            return frameId;
        }

        public String GetParameter(String name)
        {
            return context.Request.Params[name];
        }

        public String[] GetParameterNames()
        {
            return context.Request.Params.AllKeys;
        }

        public String[] GetParameterValues(String name)
        {
            return context.Request.Params.GetValues(name);
        }

        public TextWriter getWriter()
        {
            return context.Response.Output;
        }


        internal String getEncodedUrl()
        {
            return encodedUrl;
        }

        public HttpContext GetContext()
        {
            return context;
        }

    }

    public class HEvent : EventObject
    {

        public static readonly int NOTYPE_EVENT = 0;
        public static readonly int ACTION_EVENT = 1;
        public static readonly int UPDATE_EVENT = 2;
        public static readonly int UPLOAD_EVENT = 3;
        public static readonly int SUBMIT_EVENT = 4;

        protected String subEventId;
        protected String[] values;
        protected String fileName;
        protected String tempSavedFileName;
        protected String remoteName, remoteType;
        protected int eventType;
        protected HWTEvent hwtEvent;

        public HEvent(object source)
            : base(source)
        {
        }

        public HEvent(HWTEvent hwtEvent, String[] values, object source, String subEventId,
                int eventType)
            : base(source)
        {
            this.hwtEvent = hwtEvent;
            this.subEventId = subEventId;
            this.values = values;
            this.eventType = eventType;
        }

        public HEvent(HWTEvent hwtEvent, String[] values, Object source, String subEventId, String tempSavedFileName, String fileName,
                String remoteName, String remoteType)
            : this(hwtEvent, values, source, subEventId, UPLOAD_EVENT)
        {
            this.fileName = fileName;
            this.tempSavedFileName = tempSavedFileName;
            this.remoteName = remoteName;
            this.remoteType = remoteType;
        }

        /**
         * Returns the subEventId.
         * 
         * @return String
         */
        public String GetSubEventId()
        {
            return subEventId;
        }

        /**
         * Returns the values.
         * 
         * @return String[]
         */
        public String[] GetValues()
        {
            return values;
        }

        /**
         * Returns the file name. (name and extension)
         * 
         * @return File
         */
        public String GetFileName()
        {
            return fileName;
        }

        /**
         * Returns temporary saved file.
         * 
         * @return File
         */
        public String GetTempSavedFileName()
        {
            return this.tempSavedFileName;
        }

        /**
         * Returns the remoteName.
         * 
         * @return String
         */
        public String GetRemoteName()
        {
            return remoteName;
        }

        /**
         * Returns the remoteType.
         * 
         * @return String
         */
        public String GetRemoteType()
        {
            return remoteType;
        }

        /**
         * Returns the eventType.
         * 
         * @return int
         */
        public int getEventType()
        {
            return eventType;
        }

        public override String ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("source:     " + GetSource());
            sb.Append("subEventId: " + this.subEventId);
            sb.Append("values:     " + values);
            return sb.ToString();
        }

        public String getUserid()
        {
            try
            {
                return hwtEvent.GetUser().Identity.Name;
            }
            catch (Exception)
            {
                return "anonymous";
            }
        }

        public bool isUserInRole(String role)
        {
            try
            {
                return hwtEvent.GetUser().IsInRole(role);
            }
            catch (Exception)
            {
                return false;
            }
        }
    }



    public class HFileInput : INPUT, IPreListener
    {

        protected String nameAtt;

        private String fileName;
        private String tempSavedFileName;
        private String remoteName;
        private String remoteType;

        public HFileInput(IEventSource hsource)
            : this(hsource, INPUT.HType.FILE)
        {

        }

        public HFileInput(IEventSource hsource, String label)
            : base(HDTD.Element.INPUT)
        {

            SetAttribute(HDTD.AttName.TYPE, INPUT.HType.FILE);
            SetAttribute(HDTD.AttName.VALUE, label);
            nameAtt = hsource.RegisterFile(this);
            hsource.RegisterUpdate(this);
            SetAttribute(HDTD.AttName.NAME, nameAtt);
        }

        public override void PreArrived(HEvent hevent)
        {
            if (hevent.GetFileName() != null)
            {
                this.fileName = hevent.GetFileName();
                this.tempSavedFileName = hevent.GetTempSavedFileName();
                this.remoteName = hevent.GetRemoteName();
                this.remoteType = hevent.GetRemoteType();
            }
            String[] values = hevent.GetValues();
            if (ArrayUtils.IsEmpty(values))
            {
                return;
            }
            SetText(values[0]);
        }

        public String GetFileName()
        {
            return fileName;
        }

        public String GetTempSavedFileName()
        {
            return tempSavedFileName;
        }

        public String GetRemoteFileName()
        {
            return remoteName;
        }

        public String getRemoteFileType()
        {
            return remoteType;
        }

        public void Clear()
        {
            this.fileName = null;
            this.tempSavedFileName = null;
            this.remoteName = null;
            this.remoteType = null;
        }

    }



    public class HUploadifyInput : INPUT, IPreListener
    {

        protected String nameAtt;
        protected INewFileListener listener;


        public HUploadifyInput(IEventSource hsource)
            : this(hsource, INPUT.HType.FILE)
        {

        }

        public HUploadifyInput(IEventSource hsource, String label)
            : base(HDTD.Element.INPUT)
        {

            SetAttribute(HDTD.AttName.TYPE, INPUT.HType.FILE);
            SetAttribute(HDTD.AttName.VALUE, label);
            SetAttribute(HDTD.AttName.CLASS, "UPLOADIFY");
            SetAttribute(HDTD.AttName.MULTIPLE, "true");
            nameAtt = hsource.RegisterFile(this);
            hsource.RegisterUpdate(this);
            SetAttribute(HDTD.AttName.NAME, nameAtt);
            // WORKAROUND
            SetAttribute(HDTD.AttName.ID, nameAtt);
        }

        public override void PreArrived(HEvent hevent)
        {
            if (listener != null && hevent.GetFileName() != null)
            {
                UploadifyFile uf = new UploadifyFile();
                uf.fileName = hevent.GetFileName();
                uf.tempSavedFileName = hevent.GetTempSavedFileName();
                uf.remoteName = hevent.GetRemoteName();
                uf.remoteType = hevent.GetRemoteType();
                listener.Arrived(uf);
            }
            String[] values = hevent.GetValues();
            if (ArrayUtils.IsEmpty(values))
            {
                return;
            }
            SetText(values[0]);
        }

        public void SetINewFileListener(INewFileListener listener) { this.listener = listener; }


        public void Clear()
        {
        }

    }



    public class HFlowPanel : TABLE
    {

        private bool debug = false;

        private TR currentTr;
        private bool newRow;
        private String align;
        private TABLE subtable;

        public static readonly String CENTER = HDTD.AttValue.CENTER;
        public static readonly String LEFT = HDTD.AttValue.LEFT;
        public static readonly String RIGHT = HDTD.AttValue.RIGHT;

        public HFlowPanel() : this(CENTER, 2) { }

        public HFlowPanel(String horizontalAlign, int padding)
        {
            this.align = horizontalAlign;
            SetAttribute(HDTD.AttName.WIDTH, "100%");
            SetAttribute(HDTD.AttName.CELLSPACING, 0);
            SetAttribute(HDTD.AttName.CELLPADDING, 0);
            if (debug)
            {
                SetAttribute(HDTD.AttName.BORDER, "1");
            }
            SetAttribute(HDTD.AttName.NOWRAP, null);

            subtable = new TABLE();
            subtable.SetAttribute(HDTD.AttName.WIDTH, "2%");
            subtable.SetAttribute(HDTD.AttName.CELLSPACING, 0);
            subtable.SetAttribute(HDTD.AttName.CELLPADDING, padding);
            if (debug)
            {
                subtable.SetAttribute(HDTD.AttName.BORDER, "1");
            }

            TR tr = new TR();
            TD td = new TD();
            td.SetAttribute(HDTD.AttName.ALIGN, this.align);

            td.Add(subtable);
            tr.Add(td);
            Add(tr);

            newRow = true;

        }

        public HComponent addComponent(HComponent comp)
        {
            TD td = new TD();
            td.SetAttribute(HDTD.AttName.ALIGN, align);
            td.Add(comp);
            td.SetAttribute(HDTD.AttName.NOWRAP, HDTD.AttValue.TRUE);
            if (newRow)
            {
                currentTr = new TR();
                subtable.Add(currentTr);
                newRow = false;
            }
            currentTr.Add(td);
            return comp;
        }

        /**
         * Switch to a next row.
         */
        public void nextRow()
        {
            newRow = true;
        }

    }
    public class HFrame : HTML, IEventSource
    {

        //
        // CLASS LEVEL
        //

        private static Logger logger = Logger.GetLogger(typeof(HFrame));
        public static readonly String LINKACTION = "linkaction";
        //public static readonly String FOCUSELEMENT = "focuselement";
        public static readonly String MULTIPART = "multipart/form-data";
        public static readonly String REGULAR = "application/x-www-form-urlencoded";

        //
        // INSTANCE LEVEL
        //

        private int counter = 1000;

        private String actionUrl = "";

        private HWTEventDispatcher disp;

        private HEAD head;
        private BODY body;
        private DIV busyDiv;
        private FORM form;
        private DIV mainContainer;

        protected HAttribute actionAtt;
        protected HAttribute sessionidAtt;
        protected HAttribute useridAtt;
        protected String formName;
        private String icon_directory = "";
        private String userid;
        private bool newlyCreated = false;
        private bool logoutSelected = false;
        private HWTEvent hwtEvent;
        private HttpServerUtility httpServerUtility;


        public HFrame(HWTEvent hwtEvent)
            : this(hwtEvent, "HWT (c) OOIT.com AG")
        {
        }

        public HFrame(HWTEvent hwtEvent, String title)
        {
            httpServerUtility = hwtEvent.GetContext().Server;
            head = new HEAD(title);
            HTag meta = new HTag(HDTD.Element.META);
            meta.SetAttribute("http-equiv", "content-type");
            meta.SetAttribute("content", "text/html; charset=" + Encoding.UTF8.EncodingName);
            head.Add(meta);
            head.AddCss("../uploadify/uploadify.css");

            // JavaScript
            // <script type="text/javascript"	src="../jqueryjquery-1.8.2.js"></script>
            // <script type="text/javascript" src="uploadify/jquery.uploadify.js"></script>
            head.AddJavaScript("../js/jquery/jquery-1.8.2.js");
            head.AddJavaScript("../uploadify/jquery.uploadify.js");
            head.AddJavaScript("../js/hframe.js");

            body = new BODY();
            SetHEAD(head);
            SetBODY(body);

            actionAtt = new HAttribute(HDTD.AttName.ACTION, "");
            sessionidAtt = new HAttribute(HDTD.AttName.VALUE, "");
            useridAtt = new HAttribute(HDTD.AttName.VALUE, "");

            form = new FORM(FORM.POST, "", WebUtils.MULTIPART_FORM_DATA);
            body.Add(form);
            //
            // Invisible Button for SUBMIT events
            //
            HButton invisible = new HButton(this, "");
            invisible.SetAttribute(HDTD.AttName.STYLE, "display:none");
            form.Add(invisible);
            //
            // FOCUS INFORMATION
            //
            //INPUT focusFildHf = new INPUT(INPUT.HIDDEN, FOCUSELEMENT);
            //form.Add(focusFildHf);

            // INPUT sessionidHf = new INPUT(INPUT.HIDDEN, HConstants.SESSIONID);
            // INPUT useridHf = new INPUT(INPUT.HIDDEN, HConstants.USERID);
            INPUT linkActionHf = new INPUT(INPUT.HIDDEN, LINKACTION);
            form.Add(linkActionHf);

            // sessionidHf.SetAttribute(this.getSessionIdAtt());
            // useridHf.SetAttribute(this.getUserIdAtt());

            form.SetAttribute(actionAtt);
            form.Add(mainContainer = new DIV());
            SetFormName("form");
            //

            busyDiv = new DIV("Request in Progress. Bitte Warten!)");
            busyDiv.SetAttribute(HDTD.AttName.ID, "busyDiv");
            body.Add(busyDiv);
        }

        public void SetFocusElement(HTag element)
        {
            if (element.HasAttribute(HDTD.AttName.NAME))
            {
                String focusElementName = element.GetAttributeValue(HDTD.AttName.NAME);
                //logger.Debug("FOCUSELEMENT: " + focusElementName);
                // TODO
                if (StringUtils.IsNotEmpty(focusElementName))
                {
                    HAttribute att = body.GetAttribute(HDTD.AttName.ONLOAD);
                    if (att != null)
                    {
                        logger.Debug("replace ONLOAD Attribute, current: " + att.GetValue());
                    }
                    String newValue = "javascript:document.forms[0]."
                         + focusElementName + ".focus()";
                    body.SetAttribute(HDTD.AttName.ONLOAD, newValue);
                    logger.Debug("replace ONLOAD Attribute, new    : " + newValue);
                }
            }
            else
            {
                logger.Warn("Could not set focus for element: " + element);
            }
        }

        private void RemoveFocusElement()
        {
            body.RemoveAttribute(HDTD.AttName.ONLOAD);
        }


        public HttpServerUtility GetHttpServerUtility()
        {
            return this.httpServerUtility;
        }

        public HWTEvent GetHWTEvent()
        {
            return this.hwtEvent;
        }

        public void ProcessEventSingleThreaded(HWTEvent hwtEvent)
        {
            lock (Locker.MUTEX)
            {
                ProcessEvent(hwtEvent);
            }
        }

        public void ProcessEvent(HWTEvent hwtEvent)
        {
            DateTimeUtils.StartTime("HFrame.ProcessEvent");
            this.hwtEvent = hwtEvent;
            httpServerUtility = hwtEvent.GetContext().Server;
            BeforeProcess(hwtEvent);
            // TODO SetActionUrl(hwtEvent.getFrameId());
            RemoveFocusElement();
            SetActionUrl(hwtEvent.getEncodedUrl());
            Dispatch(hwtEvent);
            logger.Debug("dispatch DONE");
            if (logoutSelected)
            {
                hwtEvent.GetSession().Clear();
                hwtEvent.GetSession().Abandon();
                logger.Info("Session invalidated ", hwtEvent.GetSessionId());
            }
            // 
            // Print Page
            //
            TextWriter w = hwtEvent.getWriter();
            BeforePrint(hwtEvent);

            logger.Debug("start print");
            this.PrintTo(w);
            logger.Debug("end print");
            w.Flush();
            AfterPrint(hwtEvent);
            httpServerUtility = null;
            this.hwtEvent = null;
            DateTimeUtils.LogTime("HFrame.ProcessEvent");
        }

        private long TimeSnap(ref DateTime now)
        {
            long diff = DateTime.Now.Millisecond - now.Millisecond;
            now = DateTime.Now;
            return diff;
        }

        public virtual void BeforeProcess(HWTEvent hwtEvent)
        {
        }

        public virtual void BeforePrint(HWTEvent hwtEvent)
        {
        }

        public virtual void AfterPrint(HWTEvent hwtEvent)
        {
        }

        /**
         * Add h-components to the body part of the frame.
         * 
         * @param component
         *            h-component to Add.
         */

        protected void SetMainComponents(bool isModal, params HComponent[] hcomponents)
        {
            if (isModal)
            {
                mainContainer.SetStyleClass(HStyles.MODAL_WINDOW);
            }
            else
            {
                mainContainer.SetStyleClass("");
            }
            mainContainer.Set(hcomponents);
        }


        public void SetTitle(String title)
        {
            head.SetTitle(title);
        }

        /**
         * Register a h-dispatcher as a Form or a Link public String
         * register(HDispatcher dispatcher) { String actionid = createName();
         * dispatchers.Put(actionid, dispatcher); return actionid; }
         * 
         */

        /**
         * Get session id attribute.
         * 
         * @return session id attribute.
         */
        internal HAttribute GetSessionIdAtt()
        {
            return sessionidAtt;
        }

        /**
         * Get user id attribute.
         * 
         * @return user id attribute.
         */
        internal HAttribute GetUserIdAtt()
        {
            return useridAtt;
        }

        public HWTEventDispatcher GetDispatcher()
        {
            return disp;
        }

        private String CreateName()
        {
            return "HF" + (counter++);
        }

        internal void SetSessionId(String sessionid)
        {
            sessionid = sessionid == null ? "" : sessionid;
            sessionidAtt.SetValue(sessionid);
            logger.Debug("Sessionid = " + sessionid);
        }

        internal void SetUserid(String userid)
        {
            this.userid = userid == null ? "" : userid;
            useridAtt.SetValue(userid);
        }

        public String GetUserid()
        {
            return userid ?? "";
        }

        internal void SetIconDirectory(String icon_directory)
        {
            this.icon_directory = icon_directory;
        }

        public String GetIconDirectory()
        {
            return this.icon_directory;
        }

        public String GetFormName()
        {
            return formName == null ? "form" : formName;
        }

        protected void SetFormName(String formName)
        {
            form.SetAttribute(HDTD.AttName.NAME, formName);
            this.formName = formName;
        }

        public String GetActionUrl()
        {
            return actionUrl;
        }

        public HAttribute GetActionUrlAtt()
        {
            return actionAtt;
        }

        internal void SetActionUrl(String actionUrl)
        {
            actionAtt.SetValue(actionUrl);
            this.actionUrl = actionUrl;
        }

        public void Release()
        {
            disp = null;
            logger.Debug("UNLOAD: " + this.GetType().ToString());
        }

        // Line in of HFormPanel

        private Dictionary<String, IPreListener> updateListeners = new Dictionary<String, IPreListener>();
        private Dictionary<String, HCheckBox> checkBoxPrinted = new Dictionary<String, HCheckBox>();
        private Dictionary<String, IPreListener> actionListeners = new Dictionary<String, IPreListener>();
        private HashSet<IPreListener> SubmitListeners = new HashSet<IPreListener>();
        private Dictionary<String, IPreListener> fileListeners = new Dictionary<String, IPreListener>();

        //protected void setCodeType(String type) {
        //    form.SetAttribute(HDTD.AttName.ENCTYPE, type);
        //}

        private void Dispatch(HWTEvent hwtEvent)
        {
            if (this.newlyCreated)
            {
                logger.Debug("frame is newly created: no dispatch is executed!");
                return;
            }
            logger.Debug("start dispatch ", this);
            //
            //
            //
            if (logger.IsDebug())
            {
                logger.Debug("PRINT All VALUES");
                String[] paramNames = hwtEvent.GetParameterNames();
                foreach (String _name in paramNames)
                {
                    logger.Debug("p-name: " + _name + "=" + ArrayUtils.Join(hwtEvent.GetParameterValues(_name), ","));
                }
                logger.Debug("All VALUES END");
            }
            //
            // reset check boxes which have be written w before
            //
            foreach (HCheckBox cx in checkBoxPrinted.Values)
            {
                cx.SetChecked(false);
            }
            checkBoxPrinted.Clear();
            //
            // update listeners
            //
            String[] parameterNames = hwtEvent.GetParameterNames();
            foreach (String _name in parameterNames)
            {
                String[] ids = GetIds(_name);
                String id = ids[0];
                String subid = ids[1];
                IPreListener listener = (IPreListener)updateListeners.Get(id);
                if (listener != null)
                {
                    HEvent hevent = new HEvent(hwtEvent, hwtEvent.GetParameterValues(id), listener,
                            subid, HEvent.UPDATE_EVENT);
                    listener.PreArrived(hevent);
                }
            }
            HttpFileCollection files = hwtEvent.getUploadedFileCollection();
            foreach (String _name in files.AllKeys)
            {
                HttpPostedFile postedFile = files[_name];
                IPreListener listener = fileListeners.Get(_name);
                if (listener != null)
                {
                    String fileName = Path.GetFileName(postedFile.FileName);
                    String tmpSavedFileName = Path.Combine(this.TempDir, "uf" + DateTime.Now.Ticks);
                    try
                    {
                        postedFile.SaveAs(tmpSavedFileName);
                        HEvent hevent = new HEvent(hwtEvent, null, listener, "NA", tmpSavedFileName, fileName, _name,
                                  postedFile.ContentType);
                        listener.PreArrived(hevent);
                    }
                    catch (Exception e)
                    {
                        logger.Error("Tried to save file in tmp dir " + this.TempDir, "exception: ", e);
                    }
                }
            }
            String[] names = hwtEvent.GetParameterNames();
            foreach (String _name in names)
            {
                String[] ids = GetIds(_name);
                String id = ids[0];
                String subid = ids[1];
                IPreListener listener = actionListeners.Get(id);
                if (listener != null)
                {
                    HEvent hevent = new HEvent(hwtEvent, hwtEvent.GetParameterValues(id), listener,
                            subid, HEvent.ACTION_EVENT);
                    listener.PreArrived(hevent);
                }
            }
            //
            // Links (HLink)
            //
            String name = hwtEvent.GetParameter(LINKACTION);
            if (name != null)
            {
                String[] ids = GetIds(name);
                String id = ids[0];
                String subid = ids[1];
                IPreListener listener = actionListeners.Get(id);
                if (listener != null)
                {
                    HEvent hevent = new HEvent(hwtEvent, hwtEvent.GetParameterValues(name), listener,
                            subid, HEvent.ACTION_EVENT);
                    listener.PreArrived(hevent);
                }
            }
            foreach (IPreListener listener in SubmitListeners)
            {
                listener.PreArrived(new HEvent(hwtEvent, hwtEvent.GetParameterValues(name), listener, "",
                        HEvent.SUBMIT_EVENT));
            }
            SubmitListeners.Clear();
        }

        public String TempDir
        {
            set;
            get;
        }


        private String[] GetIds(String value)
        {
            String id = value;
            String subid = "";
            int delimit_position = id.IndexOf("_");
            if (delimit_position > 0)
            {
                logger.Debug(id);
                subid = id.Substring(delimit_position + 1);
                id = id.Substring(0, delimit_position);
            }
            return new String[] { id, subid };
        }

        public String RegisterFile(IPreListener listener)
        {
            String name = this.CreateName();
            fileListeners.Put(name, listener);
            updateListeners.Put(name, listener);
            return name;
        }

        public String RegisterAction(IPreListener listener)
        {
            String name = this.CreateName();
            actionListeners.Put(name, listener);
            return name;
        }

        public String RegisterUpdate(IPreListener listener)
        {
            String name = this.CreateName();
            updateListeners.Put(name, listener);
            return name;
        }

        public void FirePrintEvent(HCheckBox checkBox)
        {
            this.checkBoxPrinted.Put(checkBox.getUpdateName(), checkBox);
        }

        public void RegisterSubmit(IPreListener listener)
        {
            this.SubmitListeners.Add(listener);
        }

        public void SetNewlyCreated(bool newlyCreated)
        {
            this.newlyCreated = newlyCreated;
        }

        public bool IsLogoutSelected()
        {
            return logoutSelected;
        }

        public void SetLogoutSelected(bool logoutSelected)
        {
            this.logoutSelected = logoutSelected;
        }

    }


    public class HIcon : IMG
    {

        private HFrame hframe;

        public HIcon(HFrame hframe, String iconFileName)
            : this(hframe, iconFileName, "icon") { }

        public HIcon(HFrame hframe, String iconFileName, String styleClass)
        {
            this.SetStyleClass(styleClass);
            this.hframe = hframe;
            setIcon(iconFileName);
        }

        public HIcon(HFrame hframe, String iconFileName, String[][] attValueList)
            : base(attValueList)
        {
            this.hframe = hframe;
            setIcon(iconFileName);
        }


        public void setIcon(String iconFileName)
        {
            SetAttribute(new IconSrcAttribute(hframe, iconFileName));
        }

    }
    public class HLabel : SPAN
    {

        public HLabel()
        {
        }

        public HLabel(String text)
        {
            SetStyleClass(HStyles.HLABEL);
            SetText(text);
        }

        public HLabel(String text, String styleClass)
            : this(text)
        {
            this.SetStyleClass(styleClass);
        }

    }

    public class HLink : HAbstractButton
    {

        protected HIcon icon;
        protected HFrame hframe;
        protected HAttribute onClickAttribute = new HAttribute(HDTD.AttName.ONCLICK, "");

        public HLink(HLink link)
            : base(HDTD.Element.A)
        {
            SetAttribute(onClickAttribute);
            this.hframe = link.hframe;
            SetSubId(link.subId);
            this.initName = link.initName;
            this.SetText(link.GetText());
            this.SetAttribute(HDTD.AttName.HREF, "");
            this.SetSubId(subId);
            SetStyleClass(HStyles.HLINK);
        }



        public HLink(HFrame hframe, String text)
            : this(hframe)
        {
            this.SetText(text);
        }

        public HLink(HFrame hframe, HIcon icon)
            : this(hframe)
        {
            this.SetIcon(icon);
        }

        public HLink(HFrame hframe)
            : this(hframe, "", "")
        {
        }


        public override void SetSubId(String subId)
        {
            base.SetSubId(subId);
            updateOnclickAttribute();
        }

        public HLink(HFrame hframe, String text, String subId)
            : base(HDTD.Element.A)
        {
            SetAttribute(onClickAttribute);
            this.hframe = hframe;
            SetEventSource(hframe);
            this.SetAttribute(HDTD.AttName.HREF, "");
            this.SetText(text);
            this.SetSubId(subId);
            SetStyleClass(HStyles.HLINK);
        }

        private void updateOnclickAttribute()
        {
            String busyFun = "APP_instance.showBusy();";
            onClickAttribute.SetValue(busyFun + hframe.GetFormName() + "." + HFrame.LINKACTION + ".name='"
                + GetInitName() + "_" + GetSubId() + "';" + hframe.GetFormName()
                + ".submit();return false");
        }

        /**
         * Setting the icon with file name, which is specified by the
         * hframe-icon-directory element in the hframe xml specifcation
         * (hwindows.xml).
         * 
         * @param iconFileName
         *            the file name.
         */
        public void SetIcon(HIcon icon)
        {
            this.icon = icon;
            RemoveAll();
            Add(icon);
        }

        protected override void CloseAngleBracket(TextWriter w)
        {
            w.Write('>');
        }


    }
    public class HList<E> : HTag, IPreListener
    {

        private static Logger logger = Logger.GetLogger(typeof(HList<Object>));

        private IList<E> model;
        private List<E> selectSet = new List<E>();

        private HTag option_notselected = new HTag(HDTD.Element.OPTION);
        private HTag option_selected = new HTag(HDTD.Element.OPTION);
        private ITextRenderer<E> textRenderer = new DefaultTextRenderer<E>();

        public HList(IEventSource hsource) : this(hsource, null, false, 1) { }

        public HList(IEventSource hsource, IList<E> model) : this(hsource, model, false, 1) { }

        public HList(IEventSource hsource, IList<E> model, bool multiSelection, int size)
            : base(HDTD.Element.SELECT)
        {
            this.model = model;
            if (model == null)
                this.model = new List<E>();
            if (size > 1)
                SetAttribute(HDTD.AttName.SIZE, "" + size);
            if (size > 1 && multiSelection)
                SetAttribute(HDTD.AttName.MULTIPLE);
            option_selected.SetAttribute(HDTD.AttName.SELECTED);
            SetEventSource(hsource);
        }

        public override int Count()
        {
            return model != null ? model.Count : 0;
        }

        public override HComponent Get(int i)
        {
            E _object = model[i];
            if (_object != null)
            {
                if (selectSet.Contains(_object))
                {
                    option_selected.SetAttribute(HDTD.AttName.VALUE, "" + i);
                    option_selected.SetText(textRenderer.GetRendererString(model[i], true));
                    return option_selected;
                }
                else
                {
                    option_notselected.SetAttribute(HDTD.AttName.VALUE, "" + i);
                    option_notselected.SetText(textRenderer.GetRendererString(model[i], false));
                    return option_notselected;
                }
            }
            else
            {
                return null;
            }
        }

        public void SetSelectionType(bool multipleSelection)
        {
            if (multipleSelection)
                SetAttribute(HDTD.AttName.MULTIPLE);
            else
                RemoveAttribute(HDTD.AttName.MULTIPLE);
        }

        public void SetSelectedIndex(int index)
        {
            E o1 = model[index];
            selectSet.AddUnique(o1);
        }

        public void SetSelected(params E[] objs)
        {
            selectSet.Clear();
            if (ArrayUtils.IsNotEmpty(objs))
            {
                foreach (E obj in objs)
                {
                    if (obj != null && model.Contains(obj))
                    {
                        selectSet.AddUnique(obj);
                    }
                }
            }
        }

        public void SetSelectedIndices(int[] indices)
        {
            selectSet.Clear();
            for (int i = 0; i < indices.Length; i++)
            {
                SetSelectedIndex(indices[i]);
            }
        }

        public void SetModel(IList<E> model)
        {
            this.model = model;
        }

        public IEnumerable<E> GetModel()
        {
            return model;
        }

        /**
         * @return selected indices (never null)
         */
        public IList<E> GetSelected()
        {
            return selectSet;
        }

        /*
         * public Object[] getSelectedValues() { return new Object[0]; }
         */
        public override void PreArrived(HEvent hevent)
        {
            // selectSet.Clear();
            String[] values = hevent.GetValues();
            int[] sindex = new int[values.Length];
            for (int i = 0; i < values.Length; i++)
            {
                try
                {
                    int index = Int32.Parse(values[i].Trim());
                    sindex[i] = index;
                }
                catch (Exception npe)
                {
                    logger.Error(npe.ToString());
                }
            }
            SetSelectedIndices(sindex);
        }

        public void Arrived(FileInfo file, String remoteName, String remoteType) { }

        protected void SetEventSource(IEventSource es)
        {
            String name = es.RegisterUpdate(this);
            SetAttribute(HDTD.AttName.NAME, name);
        }

        public override HComponent GetHText()
        {
            return new HText("not yet implemented");
        }

        public ITextRenderer<E> getTextRenderer()
        {
            return textRenderer;
        }

        public void SetTextRenderer(ITextRenderer<E> textRenderer)
        {
            this.textRenderer = textRenderer;
        }



    }
    public interface IHListener
    {
        void Arrived(HEvent hevent);
    }

    public interface INewFileListener
    {
        void Arrived(UploadifyFile file);
    }

    public class HRadioButton
        : RADIO
        , IPreListener, RadioButton
    {

        protected String[] values;
        protected String initName;

        public HRadioButton(IEventSource hsource) : this(hsource, false, "") { }

        public HRadioButton(IEventSource hsource, bool selected) : this(hsource, selected, "") { }

        public HRadioButton(IEventSource hsource, bool selected, String text)
            : base("")
        {
            SetEventSource(hsource);
            SetText(text);
            setSelected(selected);
        }

        public bool isSelected()
        {
            return this.isChecked();
        }

        public override String getValue()
        {
            if (values != null && values.Length > 0)
                return values[0];
            return null;
        }

        public void setSelected(bool selected)
        {
            setChecked(selected);
        }

        public override void PreArrived(HEvent hevent)
        {
            this.values = hevent.GetValues();
            setChecked(true);
        }

        private void SetEventSource(IEventSource es)
        {
            initName = es.RegisterUpdate(this);
            SetAttribute(HDTD.AttName.NAME, initName);
        }

        public String GetInitName()
        {
            return initName;
        }

        public void setGroupName(String groupName)
        {
            this.SetAttribute(HDTD.AttName.VALUE, initName);
            this.SetAttribute(HDTD.AttName.NAME, groupName);
        }

    }


    public class HSimpleAttributeValuePanel : TABLE
    {

        public HSimpleAttributeValuePanel()
            : this(0, 2, 0)
        {
            SetAttribute(HDTD.AttName.WIDTH, "10%");
        }

        public HSimpleAttributeValuePanel(int border, int cellpadding,
                int cellspacing)
            : base(border, cellpadding, cellspacing) { }

        public void Add(String label, HComponent component)
        {
            TR tr = new TR();
            TD td = new TD(new DIV(new HLabel(label),
                    HStyles.HSimpleAttributeValuePanel_LABEL_DIV));
            td.SetAttribute(HDTD.AttName.VALIGN, HDTD.AttValue.TOP);
            td.SetAttribute(HDTD.AttName.NOWRAP, HDTD.AttValue.TRUE);
            tr.Add(td);
            td = new TD(new DIV(component,
                    HStyles.HSimpleAttributeValuePanel_VALUE_DIV));
            td.SetAttribute(HDTD.AttName.VALIGN, HDTD.AttValue.TOP);
            tr.Add(td);
            this.Add(tr);
        }

        public void Add(HComponent labelComponent, HComponent component)
        {
            TR tr = new TR();
            TD td = new TD(new DIV(labelComponent, HStyles.HSimpleAttributeValuePanel_LABEL_DIV));
            td.SetAttribute(HDTD.AttName.VALIGN, HDTD.AttValue.TOP);
            td.SetAttribute(HDTD.AttName.NOWRAP, HDTD.AttValue.TRUE);
            tr.Add(td);
            td = new TD(new DIV(component,
                    HStyles.HSimpleAttributeValuePanel_VALUE_DIV));
            td.SetAttribute(HDTD.AttName.VALIGN, HDTD.AttValue.TOP);
            tr.Add(td);
            this.Add(tr);
        }
    }


    public class HTabbedPane : DIV, IHListener
    {

        private List<HComponent> tabComponents = new List<HComponent>();
        private HFrame frame;
        private int selectedIndex = 0;
        private DIV tabsDIV = new DIV();
        private DIV mainDIV = new DIV();
        private List<DIV> tabs = new List<DIV>();
        private List<SPAN> tabTexts = new List<SPAN>();
        private List<HLink> tabLinks = new List<HLink>();
        private bool hasTabs = false;

        public HTabbedPane(HFrame frame)
        {
            this.frame = frame;
            //
            SetStyleClass(HStyles.HTABBEDPANE);
            tabsDIV.SetStyleClass(HStyles.HTABBEDPANE_TABS);
            mainDIV.SetStyleClass(HStyles.HTABBEDPANE_MAIN);
        }

        public override int Count()
        {
            return hasTabs ? 2 : 1;
        }

        public override HComponent Get(int index)
        {
            return hasTabs ? (index == 0 ? (HComponent)tabsDIV : (HComponent)mainDIV) : (HComponent)mainDIV;
        }


        public void AddTab(HComponent component, String tabText)
        {
            tabComponents.Add(component);
            // create Link
            HLink link = new HLink(frame);
            link.SetText(tabText);
            link.AddHListener(this);
            //
            tabLinks.Add(link);
            tabTexts.Add(new SPAN(tabText, HStyles.HTABBEDPANE_TX));
            DIV div = new DIV(link);
            tabs.Add(div);

            tabsDIV.Add(div);
            if (StringUtils.IsNotEmpty(tabText) || tabs.Count > 1)
            {
                hasTabs = true;
            }
            ShowTab(selectedIndex);
        }

        public void ShowTab(int index)
        {
            mainDIV.Set(tabComponents.Get(index));
            selectedIndex = index;
            for (int i = 0; i < tabs.Count; i++)
            {
                DIV tab = tabs.Get(i);
                if (i != selectedIndex)
                {
                    tab.Set(tabLinks.Get(i));
                    tab.SetStyleClass(HStyles.HTABBEDPANE_TAB);
                }
                else
                {
                    tab.Set(tabTexts.Get(i));
                    tab.SetStyleClass(HStyles.HTABBEDPANE_TAB_SELECTED);
                }
            }
        }

        public int GetSelectedTab()
        {
            return selectedIndex;
        }

        public HComponent GetCurrentTab()
        {
            return this.tabComponents.Get(this.selectedIndex);
        }

        public void Arrived(HEvent ae)
        {
            for (int i = 0; i < tabLinks.Count; i++)
            {
                if (tabLinks.Get(i) == ae.GetSource())
                {
                    ShowTab(i);
                    return;
                }
            }
        }


    }

    public class HTable<E> : TABLE
    {

        private ITableModel<E> model;
        private ITableRowRenderer<E> renderer;

        public HTable() : this(new DefaultTableModel<E>()) { }

        public HTable(ITableModel<E> model)
        {
            this.model = model;
            SetAttribute(HDTD.AttName.CELLSPACING, 0);
            SetAttribute(HDTD.AttName.CELLPADDING, 6);
            SetAttribute(HDTD.AttName.BORDER, 1);
            SetAttribute(HDTD.AttName.WIDTH, "100%");
            SetStyleClass(HStyles.HTABLE);

        }

        public void setModel(ITableModel<E> model)
        {
            this.model = model;
        }

        public ITableModel<E> getModel()
        {
            return model;
        }

        public void setRowRenderer(ITableRowRenderer<E> renderer)
        {
            this.renderer = renderer;
        }

        public void setCellRenderer(ICellRenderer<E> cellrenderer)
        {
            if (renderer == null)
            {
                renderer = new DefaultTableRowRenderer<E>();
            }
            renderer.SetCellRenderer(cellrenderer);
        }

        public void setCellRenderer(int column, ICellRenderer<E> cellrenderer)
        {
            if (renderer == null)
            {
                renderer = new DefaultTableRowRenderer<E>();
            }
            renderer.SetCellRenderer(column, cellrenderer);
        }

        public ITableRowRenderer<E> getRowRenderer()
        {
            return renderer;
        }

        public override int Count()
        {
            if (model == null)
                return 0;
            return model.GetRowCount();
        }

        public override HComponent Get(int row)
        {
            if (renderer == null)
            {
                renderer = new DefaultTableRowRenderer<E>();
            }
            HComponent c = (HComponent)renderer.GetTableRowRendererComponent(this, false, false, row);
            return c;
        }

    }

    public class HTextArea : TEXTAREA, IPreListener
    {

        protected String mainId;
        protected String subId;

        public HTextArea(IEventSource hsource)
            : this(hsource, 20, 5, true, "") { }

        public HTextArea(IEventSource hsource, int cols, int rows)
            : this(hsource, cols, rows, true, "")
        {

        }

        public HTextArea(IEventSource hsource, int cols, int rows, bool wrap,
                String text)
            : base("", cols, rows, wrap)
        {

            SetText(text);
            SetEventSource(hsource);
            SetStyleClass(HStyles.HTEXTAREA);
        }

        public override void PreArrived(HEvent hevent)
        {
            String[] values = hevent.GetValues();
            if (values == null || values.Length == 0)
            {
                SetText("");
            }
            SetText(values[0]);
        }

        protected void SetEventSource(IEventSource es)
        {
            mainId = es.RegisterUpdate(this);
            SetAttribute(HDTD.AttName.NAME, mainId);
        }

        public void SetSubId(String subId)
        {
            SetAttribute(HDTD.AttName.NAME, mainId + "_" + subId);
        }

    }


    public class HTextField : INPUT, IPreListener
    {

        protected String mainId;
        protected String subId;

        public HTextField(IEventSource hsource)
            : this(hsource, "")
        {
        }

        public HTextField(IEventSource hsource, int size)
            : this(hsource, "", size, 0)
        {

        }

        public HTextField(IEventSource hsource, String text)
            : this(hsource, text, 0, 0)
        {

        }

        public HTextField(IEventSource hsource, int size, int maxlength)
            : this(hsource, "", size, maxlength)
        {

        }

        public HTextField(IEventSource hsource, String text, int size, int maxlength)
            : base(INPUT.TEXT, "")
        {

            SetEventSource(hsource);
            htext = new HText();
            SetText(text);
            if (size > 0)
            {
                SetAttribute(HDTD.AttValue.SIZE, size);
            }
            if (maxlength > 0)
            {
                SetAttribute(HDTD.AttValue.MAXLENGTH, maxlength);
            }
            SetStyleClass(HStyles.HTEXTFIELD);
        }

        public override HContainer SetText(String text)
        {
            SetAttribute(HDTD.AttName.VALUE, text);
            htext.SetText(text);
            return this;
        }

        public override String GetText()
        {
            return GetAttributeValue(HDTD.AttName.VALUE);
        }

        public override void PreArrived(HEvent hevent)
        {
            String[] values = hevent.GetValues();
            if (ArrayUtils.IsEmpty(values))
            {
                return;
            }
            SetText(values[0]);
        }

        protected void SetEventSource(IEventSource es)
        {
            mainId = es.RegisterUpdate(this);
            SetAttribute(HDTD.AttName.NAME, mainId);
            //SetAttribute(HDTD.AttName.ONFOCUS, "javascript:" + HFrame.FOCUSELEMENT + ".value=this.name");
        }

        public void SetSubId(String subId)
        {
            SetAttribute(HDTD.AttName.NAME, mainId + "_" + subId);
        }

        public override HComponent GetHText()
        {
            return htext;
        }

        public void SetType(short inputType)
        {

        }
    }

    public class HWTEventDispatcher : IHttpHandler, IRequiresSessionState
    {

        //
        // CLASS LEVEL
        //
        private static Logger logger = Logger.GetLogger(typeof(HWTEventDispatcher));

        //
        // OBJECT LEVEL
        //
        private Dictionary<String, Object> attributes = new Dictionary<String, Object>();
        // private SessionFactory sessionFactory;
        // private Dictionary logout_urls = new Dictionary();
        private Dictionary<String, String> error_urls = new Dictionary<String, String>();

        public void ProcessRequest(HttpContext context)
        {
            HttpRequest request = context.Request;
            HttpResponse response = context.Response;

            response.ContentType = "text/html";
            response.ContentEncoding = Encoding.UTF8;

            HttpCookieCollection cookies = request.Cookies;
            logger.Debug("Nr of cookies: ", cookies.Count);
            //foreach (HttpCookie cookie in cookies) {
            //    logger.Debug("Path: " + cookie.Path + " Name: " + cookie.Name + " Value: " + cookie.Value);
            //}

            HWTEvent hevent = new HWTEvent(context);

            try
            {
                lock (Locker.MUTEX)
                {
                    String frameid = hevent.getFrameId();
                    logger.Debug("frameid", frameid);
                    HWTFrameFactory factory = null;
                    factory = HWTEventDispatcherInitializier.GetInstance(context).GetFactory(frameid);
                    if (factory == null)
                    {
                        logger.Warn("no factory found for : " + frameid);
                        return;
                    }
                    logger.Debug("factory", factory);
                    HFrame frame = factory.GetHFrame(hevent);
                    logger.Debug("frame", frame);
                    if (frame == null)
                    {
                        logger.Warn("no frame found for : " + frameid);
                    }
                    else
                    {
                        frame.ProcessEvent(hevent);
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error(e);
                throw e;
            }
        }

        public void SetAttribute(String key, Object value)
        {
            attributes.Put(key, value);
        }

        public Object GetAttribute(String key)
        {
            return attributes.Get(key);
        }

        public bool IsReusable
        {
            get { return false; }
        }

    }

    public class HWTEventDispatcherInitializier
    {
        //
        // CLASS LEVEL
        //
        private static Logger logger = Logger.GetLogger(typeof(HWTEventDispatcherInitializier));
        private static HWTEventDispatcherInitializier instance;
        private static Object locker = new Object();
        public static readonly String HWT_DIR = "~/App_Data/HWT-INF/hwt.xml";
        public static readonly String HWT_CSS = "../css/hwt.css";
        public static readonly String HWTPRINT_CSS = "../css/hwt-print.css";


        private Dictionary<String, HWTFrameFactory> hframeFactories = new Dictionary<String, HWTFrameFactory>();

        public static HWTEventDispatcherInitializier GetInstance(HttpContext context)
        {
            return instance == null ? instance = new HWTEventDispatcherInitializier(context) : instance;
        }

        private HWTEventDispatcherInitializier(HttpContext context)
        {
            ReadDescriptor(context);
        }

        public HWTFrameFactory GetFactory(String frameId)
        {
            return hframeFactories.Get(frameId);
        }

        /**
    * Reading the HWT descriptor file.
    */
        private void ReadDescriptor(HttpContext context)
        {
            logger.Debug("start : readDescriptor");
            String hwtXML = context.Server.MapPath(HWT_DIR);
            FileInfo fileInfo = new FileInfo(hwtXML);
            if (!fileInfo.Exists)
            {
                logger.Error(HWT_DIR + " file not found. Can not read hwt config!");
                return;
            }
            XmlDocument doc = new XmlDocument();
            doc.Load(hwtXML);
            XmlElement root = doc.DocumentElement;
            XmlNodeList list = root.GetElementsByTagName("hframe");
            foreach (XmlElement hframe in list)
            {
                String hframe_name = hframe.GetTextByTagName("hframe-name").Trim();
                String hframe_title = hframe.GetTextByTagName("hframe-title").Trim();
                String hframe_class = hframe.GetTextByTagName("hframe-class").Trim();
                String hframe_url = hframe.GetTextByTagName("hframe-url").Trim();
                String hframe_error_url = hframe.GetTextByTagName("hframe-error-url").Trim();
                String hframe_temp_directory = hframe.GetTextByTagName("hframe-temp-directory").Trim();
                bool singleThreading = "single".Equals(hframe.GetTextByTagName("hframe-threading"));
                HWTFrameFactory hframe_factory = new HWTFrameFactory(Type.GetType(hframe_class), hframe_name,
                        hframe_title, hframe_url, hframe_temp_directory, singleThreading);
                hframe_factory.SetStyleSheet(HWT_CSS);
                hframe_factory.SetStyleSheetForPrint(HWTPRINT_CSS);
                logger.Debug("HFrame factory added: " + hframe_name + " - " + hframe_url);
                hframeFactories.Put(hframe_url, hframe_factory);
                //error_urls.Put(hframe_url, hframe_error_url);
                String hframe_icon_directory = hframe.GetTextByTagName("hframe-icon-directory");
                hframe_factory.SetIconDirectory(hframe_icon_directory);
            }
        }

        public static void Shutdown()
        {
            instance = null;
        }
    }

    public class HWTFrameFactory
    {
        //
        // CLASS LEVEL
        //
        private Logger logger = Logger.GetLogger(typeof(HWTFrameFactory));
        //
        // OBJECT LEVEL
        //
        protected String hframe_name;
        protected String hframe_title;
        protected Type hframe_class;
        protected String hframe_url;
        //
        protected Dictionary<String, HFrame> frames;
        protected long accesscounter;
        protected long maxaccesscounter = 500;
        protected String stylesheet = "";
        protected String printStylesheet = "";
        protected String icon_directory = "";
        protected String temp_directory = "";
        protected bool singleThreading = false;

        public HWTFrameFactory(Type hframe_class,
                String hframe_name, String hframe_title, String hframe_url, String temp_directory, bool singleThreading)
        {
            this.hframe_class = hframe_class;
            this.hframe_name = hframe_name;
            this.hframe_title = hframe_title;
            this.hframe_url = hframe_url;
            this.temp_directory = temp_directory;
            this.singleThreading = singleThreading;
            frames = new Dictionary<String, HFrame>();
        }

        public void SetStyleSheet(String stylesheet)
        {
            this.stylesheet = stylesheet;
        }

        public void SetStyleSheetForPrint(String printStylesheet)
        {
            this.printStylesheet = printStylesheet;
        }

        public void SetIconDirectory(String icon_directory)
        {
            this.icon_directory = icon_directory;
        }

        public String GetIconDirectory()
        {
            return icon_directory == null ? "" : icon_directory;
        }

        public HFrame GetHFrame(HWTEvent hwtEvent)
        {
            HttpSessionState httpSessionState = hwtEvent.GetSession();
            String httpSessionID = httpSessionState.SessionID;
            IPrincipal p = hwtEvent.GetUser();
            HFrame hframe = (HFrame)frames.Get(httpSessionID);
            if (hframe == null)
            {
                hframe = (HFrame)Activator.CreateInstance(hframe_class, hwtEvent);
                hframe.SetTitle(hframe_title);
                hframe.SetStyleSheet(this.stylesheet);
                hframe.SetStyleSheetForPrint(this.printStylesheet);
                hframe.SetIconDirectory(this.icon_directory);
                hframe.SetActionUrl(hframe_url);
                hframe.TempDir = temp_directory;
                if (p != null)
                {
                    hframe.SetUserid(p.Identity.Name);
                }
                else
                {
                    hframe.SetUserid("anonymous");
                }
                hframe.SetSessionId(httpSessionID);
                frames.Put(httpSessionID, hframe);
                hframe.SetNewlyCreated(true);
            }
            else
            {
                hframe.SetNewlyCreated(false);
            }
            logger.Debug("return frame ", hframe);
            return hframe;
        }

        public String GetFrameId()
        {
            return hframe_name;
        }

        public String GetHFrameName()
        {
            return hframe_name;
        }

        public bool IsSingleThreading()
        {
            return this.singleThreading;
        }

        private static long requestIDCounter = 1000000L;

        public static String CreateRequestID()
        {
            return "RID" + (requestIDCounter++);
        }
    }


    internal class IconSrcAttribute : HAttribute
    {

        private String iconName;
        private HFrame hframe;

        internal IconSrcAttribute(HFrame hframe, String iconName)
            : base("", "")
        {
            this.hframe = hframe;
            this.iconName = iconName;
        }

        public String getName()
        {
            return HDTD.AttName.SRC;
        }

        public String getValue()
        {
            if (!hframe.GetIconDirectory().EndsWith("/"))
            {
                return hframe.GetIconDirectory() + "/" + iconName;
            }
            else
            {
                return hframe.GetIconDirectory() + iconName;
            }
        }

    }

    public interface IListModel<E>
    {

        int GetRowCount();

        E GetValueAt(int row);

    }

    public class NBSP : HComponent
    {

        private int nrOfNbsp;

        public NBSP() : this(1) { }

        public NBSP(int nrOfNbsp)
        {
            this.nrOfNbsp = nrOfNbsp;
        }

        public override void PrintTo(TextWriter pr)
        {
            for (int i = 0; i < this.nrOfNbsp; i++)
            {
                pr.Write("&nbsp;");
            }
        }

    }

    public class ObjectCommandEvent<E> : EventObject
    {

        protected String command;
        protected E _object;
        protected Object source;

        public ObjectCommandEvent(Object source, E _object, String command)
            : base(source)
        {
            this._object = _object;
            this.command = command;
            this.source = source;
        }

        public String getCommand()
        {
            return command;
        }
        public bool isCommand(String command)
        {
            return StringUtils.Equals(this.command, command);
        }

        public void setCommand(String command)
        {
            this.command = command;
        }

        public E getObject()
        {
            return _object;
        }

        public void setObject(E _object)
        {
            this._object = _object;
        }

        public override String ToString()
        {
            return "source:" + source + " object:" + _object + " command:" + command;
        }
    }

    public interface IObjectCommandListener<E>
    {
        void Arrived(ObjectCommandEvent<E> hevent);
    }


    public class HObjectCommandList<E> : TABLE, IHListener
    {

        class OCRow : TR
        {

            private TD commandTD;
            private ICellsRenderer<E> cellsRenderer;
            private E obj;

            public void SetObject(E obj)
            {
                this.obj = obj;
            }

            internal OCRow(DIV commandLinksDIV, ICellsRenderer<E> cellsRenderer)
            {
                this.cellsRenderer = cellsRenderer;
                this.commandTD = new TD(commandLinksDIV);
            }

            internal OCRow(DIV commandLinksDIV) : this(commandLinksDIV, new DefaultCellsRenderer()) { }


            public override HComponent Get(int index)
            {
                TD[] tds = cellsRenderer.GetCells(obj);
                if (tds.Length == index)
                {
                    return commandTD;
                }
                else
                {
                    return tds[index];
                }
            }

            public override int Count()
            {
                return cellsRenderer.ColumnCount() + 1;
            }



            class DefaultCellsRenderer : ICellsRenderer<E>
            {

                private TD[] tds = new TD[1];
                private DIV content = new DIV();

                internal DefaultCellsRenderer()
                {
                    tds[0] = new TD(content);
                    content.SetStyleClass(HStyles.S100);
                }

                public int ColumnCount()
                {
                    return 1;
                }

                public TD[] GetCells(E obj)
                {
                    //tds[0].SetAttribute(HDTD.AttName.COLSPAN, maxNrOfCells.ToString());
                    content.SetText(obj == null ? "null!" : obj.ToString());
                    return tds;
                }

            }

        }

        private OCRow renderOCRow;
        private HLink[] commandLinks;
        private DIV commandDiv;


        private String[] commands;
        private HFrame hframe;

        private List<E> objects;

        public HObjectCommandList(HFrame hframe, List<E> objects, ICellsRenderer<E> renderer, params String[] commands)
        {
            this.commands = commands;
            this.hframe = hframe;
            commandDiv = new DIV();
            commandDiv.SetStyleClass(HStyles.BUTTON_PANEL_NOBORDER);
            commandLinks = new HLink[commands.Length];
            commandDiv.Add(new NBSP());
            for (int i = 0; i < commands.Length; i++)
            {
                commandLinks[i] = new HLink(hframe, commands[i]);
                commandDiv.Add(commandLinks[i]);
                commandLinks[i].AddHListener(this);
            }
            if (renderer != null)
            {
                renderOCRow = new OCRow(commandDiv, renderer);
            }
            else
            {
                renderOCRow = new OCRow(commandDiv);
            }
            SetObjects(objects);
        }

        public HObjectCommandList(HFrame hframe, List<E> objects, params String[] commands)
            : this(hframe, objects, null, commands)
        {
        }

        public HObjectCommandList(HFrame hframe, ICellsRenderer<E> renderer, params String[] commands)
            : this(hframe, null, renderer, commands)
        {
        }

        public HObjectCommandList(HFrame hframe, params String[] commands)
            : this(hframe, null, null, commands)
        {
        }


        public override HComponent Get(int index)
        {
            // UPDATE renderOCRow
            renderOCRow.SetObject(objects.Get(index));
            foreach (HLink links in commandLinks)
            {
                links.SetSubId(index.ToString());
            }
            return renderOCRow;
        }

        public override int Count()
        {
            return objects.Count;
        }

        public void SetObjects(List<E> objects)
        {
            if (objects == null)
            {
                this.objects = new List<E>();
            }
            else
            {
                this.objects = objects;
            }
        }

        public void SetObjects(IEnumerable<E> objects)
        {
            this.objects = new List<E>();
            foreach (E obj in objects)
            {
                this.objects.Add(obj);
            }
            //UpdateTable();
        }

        public void AddObject(E element)
        {
            objects.AddUnique(element);
            //UpdateTable();
        }

        public void AddObject(int index, E element)
        {
            objects.Insert(index, element);
            //UpdateTable();
        }

        public List<E> GetObjects()
        {
            return objects;
        }

        public void RemoveObject(E element)
        {
            objects.Remove(element);
            //UpdateTable();
        }

        public void RemoveObject(int index)
        {
            objects.RemoveAt(index);
        }

        public void RemoveAllObjects()
        {
            objects.Clear();
        }

        //
        // LISTENER HANDLING
        //
        private List<IObjectCommandListener<E>> listeners = new List<IObjectCommandListener<E>>();

        public void AddObjectCommandListener(IObjectCommandListener<E> listener)
        {
            if (listeners.Contains(listener))
            {
                return;
            }
            listeners.Add(listener);
        }

        public void RemoveObjectCommandListener(IObjectCommandListener<E> listener)
        {
            listeners.Remove(listener);
        }

        private void Fire(ObjectCommandEvent<E> e)
        {
            foreach (IObjectCommandListener<E> listener in listeners)
            {
                listener.Arrived(e);
            }
        }

        public void Arrived(HEvent e)
        {
            HLink link = (HLink)e.GetSource();
            int subid = Int32.Parse(link.GetSubId());
            String command = link.GetText();
            ObjectCommandEvent<E> ocEvent = new ObjectCommandEvent<E>(link, objects.Get(subid), command);
            Fire(ocEvent);
        }

    }

    public class HObjectList<E> : HContainer, IObjectCommandListener<E>
    {




        private HObjectCommandList<E> ocl;

        public HObjectList(HFrame hframe, List<E> objects, ICellsRenderer<E> renderer)
        {
            ocl = new HObjectCommandList<E>(hframe, objects, renderer, HStyles.DELETE_ICON);
            Init();
        }

        public HObjectList(HFrame hframe, ICellsRenderer<E> renderer)
        {
            ocl = new HObjectCommandList<E>(hframe, null, renderer, HStyles.DELETE_ICON);
            Init();
        }

        public HObjectList(HFrame hframe)
        {
            ocl = new HObjectCommandList<E>(hframe, HStyles.DELETE_ICON);
            Init();
        }

        private void Init()
        {
            ocl.AddObjectCommandListener(this);
            Add(ocl);
        }

        public void SetObjects(params E[] objs)
        {
            ocl.SetObjects(objs);
        }

        public List<E> GetObjects()
        {
            return ocl.GetObjects();
        }

        public void AddObjects(E obj)
        {
            ocl.AddObject(obj);
        }

        public void Arrived(ObjectCommandEvent<E> oe)
        {
            E obj = oe.getObject();
            ocl.RemoveObject(obj);
        }

        internal void RemoveAllObjects()
        {
            ocl.RemoveAllObjects();
        }
    }



    public interface RadioButton
    {
        String GetInitName();
        void setGroupName(String groupName);
        bool isSelected();
        void setSelected(bool selected);
    }

    public class HStyles
    {

        public static readonly String DELETE_ICON = "X";
        public static readonly String HBUTTON = "HBUTTON";
        public static readonly String HLINK = "HLINK";
        public static readonly String HLABEL = "HLABEL";
        public static readonly String HSimpleAttributeValuePanel_LABEL_DIV = "HSAVP_L";
        public static readonly String HSimpleAttributeValuePanel_VALUE_DIV = "HSAVP_V";
        public static readonly String HTABBEDPANE = "HTABBEDPANE";
        public static readonly String HTABBEDPANE_MAIN = "HTABBEDPANE_MAIN";
        public static readonly String HTABBEDPANE_TAB = "HTABBEDPANE_TAB";
        public static readonly String HTABBEDPANE_TABS = "HTABBEDPANE_TABS";
        public static readonly String HTABBEDPANE_TX = "HTABBEDPANE_TX";
        public static readonly String MODAL_WINDOW = "MODAL_WINDOW";

        public static readonly String HTABLE = "HTABLE";
        public static readonly String HTEXT = "HTEXT";
        public static readonly String HTEXTFIELD = "HTEXTFIELD";
        public static readonly String HTEXTAREA = "HTEXTAREA";
        public static readonly String HTABBEDPANE_TAB_SELECTED = "HTABBEDPANE_TAB_SELECTED";
        public static readonly String S100 = "S100";
        public static readonly String BUTTON_PANEL = "BUTTON_PANEL";
        public static readonly String BUTTON_PANEL_NOBORDER = "H_BUTTON_PANEL_NOBORDER";



    }

    public interface ICellRenderer<E>
    {

        TD GetCell(
           HTable<E> table,
           Object value,
           bool isSelected,
           bool hasFocus,
           int row,
           int column);
    }

    public interface ICellsRenderer<E>
    {
        int ColumnCount();
        TD[] GetCells(E value);
    }


    public interface ITableModel<E> : IListModel<E>
    {

        int GetColumnCount();

        //new int GetRowCount();

        E GetValueAt(int row, int col);


    }

    public interface ITableRowRenderer<E>
    {

        void SetCellRenderer(ICellRenderer<E> cellrenderer);

        void SetCellRenderer(int column, ICellRenderer<E> cellrenderer);

        HComponent GetTableRowRendererComponent(HTable<E> table, bool isSelected,
               bool hasFocus, int row);

    }

    public interface ITextRenderer<E>
    {
        String GetRendererString(E _object, bool isSelected);
    }
}
