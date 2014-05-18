//
// Copyright (C) 2008 OOIT.com AG, Zürich CH
// All rights reserved.
//
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
//
using Org.JGround.Codetable;
using Org.JGround.HWT.Components;
using Org.JGround.MOM;
using Org.JGround.MOM.DB;
using Org.JGround.Util;

namespace Org.JGround.HWT.MOM {

    public class UIFrame : HFrame, IUIViewPanel {

        //
        //
        //

        private static Logger logger = Logger.GetLogger(typeof(UIFrame));
        public static readonly String ROLES_SESSIONKEY = "_UIFRAME_ROLES_";

        //
        //
        // 

        private Dictionary<String, UIEditWindow> editWindows = new Dictionary<String, UIEditWindow>();
        private Dictionary<String, UIViewWindow> viewWindows = new Dictionary<String, UIViewWindow>();
        private Dictionary<String, UIViewSynopsis> synopsisPanels = new Dictionary<String, UIViewSynopsis>();

        protected HContainer hiddenStuff;

        private DIV navigationDIV;
        private DIV mainDIV;
        private DIV footerDIV;
        private DIV headerDIV;
        private HComponent homePanel;
        private TABLE mainTable;
        //private HComponent previousMainPanel;
        //private IMainPanel currentDialog;
        private Stack<IMainPanel> stackedDialogs;
        // private IMainPanel printView;

        private UIDataObjectRefSearchPanel refSearchPanel;



        public UIFrame(HWTEvent hwtEvent)
            : base(hwtEvent) {
            stackedDialogs = new Stack<IMainPanel>();
            //
            // MOM STUFF
            //
            headerDIV = new DIV();
            navigationDIV = new DIV();
            mainDIV = new DIV();
            footerDIV = new DIV();
            footerDIV.SetStyleClass(UIStyles.GetInstance().GetFooterStyle());
            //
            // LAYOUT
            //
            int border = logger.IsDebug() ? 1 : 0;
            int cellpadding = 1;
            int cellspacing = 0;
            hiddenStuff = new HContainer();
            mainTable = new TABLE(border, cellpadding, cellspacing);

            TD headerTD = new TD(2, headerDIV);
            //
            TD navigationTD = new TD(navigationDIV);
            navigationTD.SetAttribute(HDTD.AttName.WIDTH, "200px");
            navigationTD.SetAttribute(HDTD.AttName.VALIGN, HDTD.AttValue.TOP);
            navigationTD.SetAttribute(HDTD.AttName.NOWRAP, HDTD.AttValue.TRUE);
            //
            TD mainTD = new TD(mainDIV);
            mainTD.SetAttribute(HDTD.AttName.WIDTH, "90%");
            mainTD.SetAttribute(HDTD.AttName.VALIGN, HDTD.AttValue.TOP);
            //
            TD footerTD = new TD(2, footerDIV);
            //
            mainTable.Add(new TR(headerTD));
            mainTable.Add(new TR(navigationTD, mainTD));
            mainTable.Add(new TR(footerTD));
            //
            base.SetMainComponents(false, mainTable, hiddenStuff);
            //
            SetupUserAndRoles(hwtEvent);
        }

        protected virtual void SetupUserAndRoles(HWTEvent hwtEvent) { }



        public override void BeforeProcess(HWTEvent hwtEvent) {
            SetupUserAndRoles(hwtEvent);
        }


        public UIEditWindow CreateEditWindow(String moid) {
            return new UIEditWindow(this, moid);
        }

        public UIViewWindow CreateViewWindow(String moid) {
            return new UIViewWindow(this, moid);
        }

        public UIViewSynopsis CreateSynopsisViews(String moid) {
            return new UIViewSynopsis(this, moid);
        }


        public UIEditWindow GetEditWindow(String moid) {
            UIEditWindow editWindow = editWindows.Get(moid);
            if(editWindow == null) {
                editWindow = CreateEditWindow(moid);
                editWindows.Put(moid, editWindow);
            }
            return editWindow;
        }

        public UIViewWindow GetViewWindow(String moid) {
            UIViewWindow viewWindow = viewWindows.Get(moid);
            if(viewWindow == null) {
                viewWindow = CreateViewWindow(moid);
                viewWindows.Put(moid, viewWindow);
            }
            return viewWindow;
        }

        public UIViewSynopsis GetSynopsisViews(String moid) {
            UIViewSynopsis views = synopsisPanels.Get(moid);
            if(views == null) {
                views = CreateSynopsisViews(moid);
                synopsisPanels.Put(moid, views);
            }
            return views;
        }


        public void Close(HComponent hcomponent) {
            if(hcomponent == GetCurrentMainPanel()) {
                ProcessBeforeClose(hcomponent);
                SetMainPanel(GetHomePanel());
            }
        }


        public HComponent GetCurrentMainPanel() {
            if(mainDIV.Count() > 0) {
                return mainDIV.Get(0);
            } else {
                return null;
            }
        }

        public void SetMainPanel(HComponent hcomponent) {
            if(hcomponent == null) {
                return;
            }
            if(GetCurrentMainPanel() != hcomponent) {
                //previousMainPanel = GetCurrentMainPanel();
                ProcessBeforeShow(hcomponent);
            }
            base.SetMainComponents(false, mainTable, hiddenStuff);
            mainDIV.Set(hcomponent);
            stackedDialogs.Clear();
        }

        protected virtual void ProcessBeforeClose(HComponent hcomponent) {
            if(hcomponent is IMainPanel) {
                IMainPanel mp = (IMainPanel)hcomponent;
                mp.BeforeClose();
            }
        }

        protected virtual void ProcessBeforeShow(HComponent hcomponent) {
            if(hcomponent is IMainPanel) {
                IMainPanel mp = (IMainPanel)hcomponent;
                mp.BeforeShow();
            }
        }
        //public void SetPreviousMainPanel() {
        //    SetMainPanel(previousMainPanel);
        //}

        public void SetHeader(params HComponent[] hcomponents) {
            headerDIV.Set(hcomponents);
        }

        public void SetFooter(params HComponent[] hcomponents) {
            footerDIV.Set(hcomponents);
        }


        public void SetNavigationPanel(HComponent hcomponent) {
            navigationDIV.Set(hcomponent);
        }

        public void SetHomePanel(HComponent hcomponent) {
            this.homePanel = hcomponent;
        }

        public void ShowHomePanel() {
            SetMainPanel(GetHomePanel());
        }

        public HComponent GetHomePanel() {
            return homePanel == null ? homePanel = new DIV(new HLabel("NO HOME PANEL SET")) : homePanel;
        }


        public UIDataObjectRefSearchPanel GetRefSearchPanel() {
            return refSearchPanel == null ? refSearchPanel = new UIDataObjectRefSearchPanel(this) : refSearchPanel;
        }

        //public void OpenDialog(params HComponent[] dialogPanels) {
        //    base.SetMainComponents(dialogPanels);
        //}

        //public void OpenDialog(IMainPanel dialog) {
        //    currentDialog = dialog;
        //    dialog.BeforeShow();
        //    base.SetMainComponents(dialog.GetView(), hiddenStuff);
        //}

        public void OpenDialogOnStack(IMainPanel dialog) {
            stackedDialogs.Push(dialog);
            dialog.BeforeShow();
            base.SetMainComponents(true, dialog.GetView(), hiddenStuff);
        }

        //public void CloseDialog() {
        //    if(currentDialog != null) {
        //        currentDialog.BeforeClose();
        //    }
        //    base.SetMainComponents(mainTable, hiddenStuff);
        //}

        public void CloseDialogOnStack() {
            if(stackedDialogs.Count > 0) {
                IMainPanel dialog = stackedDialogs.Pop();
                dialog.BeforeClose();
                if(stackedDialogs.Count > 0) {
                    dialog = stackedDialogs.Peek();
                    dialog.BeforeShow();
                    base.SetMainComponents(true, dialog.GetView(), hiddenStuff);
                }
            } else {
                stackedDialogs.Clear();
            }
            if(stackedDialogs.Count == 0) {
                base.SetMainComponents(false, mainTable, hiddenStuff);
            }
        }

        public void AddUIListener(IUIListener listener) {
        }
        public void RemoveUIListener(IUIListener listener) {
        }
        public UIFrame GetUIFrame() {
            return this;
        }
    }


    public class UIDataObjectInfoBar : DIV {

        public UIDataObjectInfoBar() {
            SetStyleClass(UIStyles.GetInstance().GetInfoBarStyle());
        }

        public void SetMODataObject(MODataObject moDataObject) {
            String userName = moDataObject.GetCheckedOutUser();
            SetText("Nr:" + moDataObject.GetOid() + " created:"
                + new DateTime(moDataObject.GetCreationTime()).ToString("d") + " modified:"
                + new DateTime(moDataObject.GetLastModifyTime()).ToString("d") + " by:"
                + moDataObject.GetLastUserName() + " stat:"
                + moDataObject.GetDataState() +
                (userName != null ? " check out by:" + userName : ""));
        }
    }

    public class UIViewPanel : HTabbedPane, IUIViewPanel {

        private static Logger logger = Logger.GetLogger(typeof(UIViewPanel));

        protected UIFrame uiFrame;
        protected MOClass moClass;
        protected MODataObject moDataObject;
        protected List<IUIAttribute> uiAttributes = new List<IUIAttribute>();

        public UIViewPanel(UIFrame uiFrame, String moid)
            : base(uiFrame) {
            this.uiFrame = uiFrame;
            this.moClass = MOService.GetInstance().GetMOClass(moid);
            Init();
        }

        protected virtual void Init() {
            logger.Debug("Start Init of UIViewPanel", this.moClass.GetMoid());
            foreach(MOPage moPage in moClass.GetViewUIPages()) {
                UIPage moPageUI = new UIPage(this, moPage, uiAttributes);
                if(!moPageUI.IsEmpty()) {
                    AddTab(moPageUI.GetPage(), moPageUI.GetLabel());
                }
            }
        }

        public virtual void SetData(MODataObject moDataObject) {
            this.moDataObject = moDataObject;
            foreach(IUIAttribute uiAttribute in uiAttributes) {
                uiAttribute.ClearData();
                uiAttribute.SetData(moDataObject);
            }
        }

        public void ClearData() {
            foreach(IUIAttribute uiAttribute in uiAttributes) {
                uiAttribute.ClearData();
            }
            moDataObject = null;
        }

        public UIViewPanel ToFirstTab() {
            this.ShowTab(0);
            return this;
        }

        internal void GoToTab(int p) {
            this.ShowTab(p);
        }
        //
        // UI LISTENER
        //
        List<IUIListener> uiListeners = new List<IUIListener>();
        public void AddUIListener(IUIListener listener) {
            uiListeners.AddUnique(listener);
        }

        public void RemoveUIListener(IUIListener listener) {
            uiListeners.Remove(listener);
        }

        public void FireUIEvent(String subject, params Object[] values) {
            foreach(IUIListener listener in uiListeners) {
                listener.processEvent(this, subject, values);
            }
        }

        public UIFrame GetUIFrame() {
            return this.uiFrame;
        }

    }

    public class UIViewSynopsis : IHListener {
        //
        //
        //
        private static Logger logger = Logger.GetLogger(typeof(UIViewSynopsis));
        private static NBSP nbsp = new NBSP();
        //
        //
        // 
        protected UIFrame uiFrame;
        protected UIViewPanel uiPanel;
        protected MOClass moClass;
        //protected MODataObject moDataObject;
        protected List<IUIAttribute> uiAttributes = new List<IUIAttribute>();
        protected List<DIV> panels = new List<DIV>();
        protected List<DIV> panelsWithLink = new List<DIV>();
        protected List<HLink> links = new List<HLink>();
        protected DIV typeIcon;


        public UIViewSynopsis(UIFrame uiFrame, String moid) {
            this.uiFrame = uiFrame;
            uiPanel = new UIViewPanel(uiFrame, moid);
            this.moClass = MOService.GetInstance().GetMOClass(moid);
            Init();
        }

        protected virtual void Init() {
            logger.Debug("Start Init of UIViewSynopsis", this.moClass.GetMoid());
            typeIcon = UIStyles.GetInstance().GetTypeIcon(this.moClass.GetMoid());
            panels.Add(typeIcon);
            // panelsWithLink.Add(UIStyles.GetInstance().GetTypeIcon(this.moClass.GetMoid()));
            panelsWithLink.Add(typeIcon);
            foreach(MOView moView in moClass.GetSynopsisViews()) {
                IUIAttribute uiAttribute = UIAttributeFactory.CreateUIAttribute(uiPanel, moView);
                uiAttributes.Add(uiAttribute);
                //
                panelsWithLink.Add(new DIV(UIStyles.SYNOPSIS, CreateAndRegisterLink().Set(uiAttribute.GetControlComponent())));
                //
                panels.Add(new DIV(UIStyles.SYNOPSIS, uiAttribute.GetControlComponent()));
            }
        }

        private HLink CreateAndRegisterLink() {
            HLink link = new HLink(uiFrame);
            link.SetStyleClass(UIStyles.VIEW_LINK);
            link.AddHListener(this);
            links.Add(link);
            return link;
        }

        public virtual void RenderTo(MODataObject mod, IList<HContainer> comps) {
            //this.moDataObject = mod;
            if(mod.GetDataState() == DataState.DELETED || mod.GetDataState() == DataState.DELETED_UNAPPROVED) {
                typeIcon.SetAttribute(HDTD.AttName.STYLE, "text-decoration:line-through");
            } else {
                typeIcon.SetAttribute(HDTD.AttName.STYLE, "");
            }

            foreach(IUIAttribute uiAttribute in uiAttributes) {
                uiAttribute.ClearData();
                uiAttribute.SetData(mod);
            }
            List<DIV> ps = null;
            if(!mod.GetMOClass().IsComponent() && MOAccess.GetInstance().CanRead(mod)) {
                ps = panelsWithLink;
                foreach(HLink link in links) {
                    link.SetSubId(mod.GetOid().ToString());
                }
            } else {
                ps = panels;
            }
            //
            for(int i = 0; i < comps.Count; i++) {
                if(i < ps.Count()) {
                    comps[i].Set(ps[i]);
                } else {
                    comps[i].Set(nbsp);
                }
            }
        }

        public void Arrived(HEvent he) {
            if(he.GetSource() is HLink) {
                HLink link = (HLink)he.GetSource();
                MODataObject mod = MODataObject.GetById(link.GetSubId());
                if(mod != null) {
                    uiFrame.OpenDialogOnStack(new UIViewWindowDelegator(uiFrame.GetViewWindow(mod.GetMoid()), mod));
                }
            }
        }

    }

    public class UIEditPanel : UIViewPanel {

        private static Logger logger = Logger.GetLogger(typeof(UIEditPanel));

        //

        public UIEditPanel(UIFrame uiFrame, String moid)
            : base(uiFrame, moid) {
        }

        protected override void Init() {
            logger.Debug("Start Init of UIEditPanel", this.moClass.GetMoid());
            foreach(MOPage moPage in moClass.GetEditUIPages()) {
                UIPage moPageUI = new UIPage(this, moPage, uiAttributes);
                if(!moPageUI.IsEmpty()) {
                    AddTab(moPageUI.GetPage(), moPageUI.GetLabel());
                }
            }
        }

        public override void SetData(MODataObject moDataObject) {
            base.SetData(moDataObject);
            foreach(IUIAttribute uiAttribute in uiAttributes) {
                uiAttribute.CheckMandatory();
            }
        }

        public MODataObject GetData() {
            if(moDataObject != null) {
                foreach(IUIAttribute uiAttribute in uiAttributes) {
                    uiAttribute.UpdateData();
                }
            }
            return moDataObject;
        }

    }

    public class UIViewWindowDelegator : IMainPanel {

        private UIViewWindow viewWindow;
        private MODataObject mod;

        public UIViewWindowDelegator(UIViewWindow viewWindow, MODataObject mod) {
            this.viewWindow = viewWindow;
            this.mod = mod;
        }

        public void BeforeShow() {
            viewWindow.SetData(mod);
        }

        public void BeforeClose() {
            viewWindow.ClearData();
        }

        public HComponent GetView() {
            return viewWindow;
        }

    }


    public class UIViewWindow : DIV, IMainPanel {

        protected UIFrame uiFrame;
        protected MOClass moClass;
        protected UIDataObjectInfoBar infoBar;
        protected H1 titleH1;

        protected UIViewPanel viewPanel;
        private MODataObject moDataObject;
        private UIWindowViewCommandPanel viewCommandPanel;

        public UIViewWindow(UIFrame uiFrame, String moid) {
            this.uiFrame = uiFrame;
            this.moClass = MOService.GetInstance().GetMOClass(moid);
            //
            // COMPONENTS
            //
            titleH1 = new H1("no title");
            infoBar = new UIDataObjectInfoBar();
            viewPanel = new UIViewPanel(uiFrame, moClass.GetMoid());
            viewCommandPanel = new UIWindowViewCommandPanel(uiFrame, this);

            //
            // LAYOUT
            //
            this.Add(titleH1);
            this.Add(infoBar);
            this.Add(viewPanel);
            this.Add(viewCommandPanel);
        }

        public void BeforeShow() { }
        public void BeforeClose() { }

        public HComponent GetView() {
            return this;
        }


        //
        // Data Methods
        //

        public void SetData(MODataObject moDataObject) {
            this.moDataObject = moDataObject;
            MOClass moClass = moDataObject.GetMOClass();
            titleH1.SetText(moClass.GetName());
            String[] st = UIStyles.GetInstance().GetTopLevelTitleStyle(moClass.GetMoid());
            titleH1.SetStyleClass(st);

            infoBar.SetMODataObject(moDataObject);
            viewPanel.SetData(moDataObject);
            viewCommandPanel.SetData(moDataObject);
        }

        internal void ClearData() {
            viewPanel.ClearData();
        }

        public void Close() {
            ClearData();
            //uiFrame.Close(this);
            uiFrame.CloseDialogOnStack();
        }


    }

    public class UIEditWindow : DIV, IMainPanel {

        private UIFrame uiFrame;
        protected MOClass moClass;
        protected UIDataObjectInfoBar infoBar;
        protected H1 titleH1;
        private UIEditPanel editPanel;
        private UIWindowEditCommandPanel windowEditCommandPanel;
        private UIInfoDialog noCheckOutInfoDialog;

        public UIEditWindow(UIFrame uiFrame, String moid) {
            this.uiFrame = uiFrame;
            this.moClass = MOService.GetInstance().GetMOClass(moid);
            //
            // COMPONENTS
            //
            titleH1 = new H1("no title");
            infoBar = new UIDataObjectInfoBar();
            editPanel = new UIEditPanel(uiFrame, moClass.GetMoid());
            windowEditCommandPanel = new UIWindowEditCommandPanel(uiFrame, new WindowEditAction(uiFrame, this));
            noCheckOutInfoDialog = new UIInfoDialog(this.uiFrame);
            //
            // LAYOUT
            //
            this.Add(titleH1);
            this.Add(infoBar);
            this.Add(editPanel);
            this.Add(windowEditCommandPanel);
        }


        public void SetData(MODataObject moDataObject) {
            if(moDataObject == null) {
                noCheckOutInfoDialog.SetConfirmContent("Data can not be opened for edit!", 
                   new DIV("No data available"));
                Set(noCheckOutInfoDialog.GetView());
                return;
            }
            if(moDataObject.CheckOut()) {
                MOClass moClass = moDataObject.GetMOClass();
                titleH1.SetText(moClass.GetName());
                titleH1.SetStyleClass(UIStyles.GetInstance().GetTopLevelTitleStyle(moClass.GetMoid()));
                infoBar.SetMODataObject(moDataObject);
                editPanel.SetData(moDataObject);
                windowEditCommandPanel.SetData(moDataObject);
                Set(titleH1, infoBar, editPanel, windowEditCommandPanel);
            } else {
                noCheckOutInfoDialog.SetConfirmContent("Data can not be opened for edit!",
                    new DIV(moDataObject.ToSynopsisString()));
                Set(noCheckOutInfoDialog.GetView());
            }
        }

        public void ClearData() {
            editPanel.ClearData();
        }

        private class WindowEditAction : IWindowEditAction {

            private static Logger logger = Logger.GetLogger(typeof(WindowEditAction));
            private UIEditWindow editWindow;
            private UIFrame uiFrame;
            private TABLE dispTable;
            private TD[] tds = { new TD(), new TD(), new TD(), new TD(), new TD() };

            public WindowEditAction(UIFrame uiFrame, UIEditWindow editWindow) {
                this.editWindow = editWindow;
                this.uiFrame = uiFrame;
                dispTable = new TABLE(new TR(tds));
            }

            public void DeleteData() {
                MODataObject moDataObject = editWindow.GetData();
                //List<MODataObject> subMoDataObjects = editWindow.editPanel.GetDataOfSubComponents();
                if(moDataObject != null) {
                    moDataObject.Delete();
                }
                //foreach (MODataObject mo in subMoDataObjects) {
                //    mo.Delete();
                //}
            }

            public void OpenConfirmDialog() {
                MODataObject moDataObject = editWindow.editPanel.GetData();
                String confirmMessage = "Do you really want to delete the following items?";
                UIViewSynopsis syn = uiFrame.GetSynopsisViews(moDataObject.GetMOClass().GetMoid());
                //syn.SetData(moDataObject);
                //syn.Fill(tds);
                syn.RenderTo(moDataObject, tds);
                uiFrame.SetMainPanel(editWindow.windowEditCommandPanel.GetDialog(confirmMessage, dispTable));
            }

            public void SaveData() {
                MODataObject mod = editWindow.GetData();
                String moid = mod.GetMOClass().GetMoid();
                if (mod.GetMOClass().GetMoid().EndsWith("Patent")){
                    String cg = mod.GetCurrentValue("companyGroup");
                    String div = mod.GetCurrentValue("division");
                    logger.Debug("Patent " +div+" for " + cg);
                }
                if(mod != null) {
                    mod.Save();
                    editWindow.SetData(MODataObject.GetById(mod.GetOid()));
                } else {
                    logger.Warn("unexpected null value for moDataObject");
                    editWindow.ClearData();
                }
            }

            public void Close() {
                MODataObject moDataObject = editWindow.editPanel.GetData();
                if(moDataObject != null) {
                    moDataObject.CheckIn();
                }
                editWindow.ClearData();
                uiFrame.Close(editWindow);

            }

            public void Show() {
                uiFrame.SetMainPanel(editWindow);
            }

        }
        //
        // Data Methods
        //
        public MODataObject GetData() {
            return editPanel.GetData();
        }

        public void BeforeShow() { }
        public void BeforeClose() { }

        public HComponent GetView() {
            return this;
        }

        public void GoToPage(int p) {
            editPanel.GoToTab(p);
        }
    }

    public class UIPage {

        private MOPage moPage;
        private List<IUIAttribute> uiLocalAttributes = new List<IUIAttribute>();
        private DIV uiPage = new DIV();

        public UIPage(IUIViewPanel viewPanel, MOPage moPage, List<IUIAttribute> uiAttributes) {
            this.moPage = moPage;
            IMOAccess moAccess = MOAccess.GetInstance();
            HSimpleAttributeValuePanel panel = new HSimpleAttributeValuePanel(0, 0, 0);
            foreach(MOView moView in moPage.GetMOViewControls()) {
                if(moAccess.IsEntitled(moView)) {
                    IUIAttribute uiAttribute = UIAttributeFactory.CreateUIAttribute(viewPanel, moView);
                    panel.Add(uiAttribute.GetLabelComponent(), uiAttribute.GetControlComponent());
                    uiAttributes.AddUnique(uiAttribute);
                    uiLocalAttributes.Add(uiAttribute);
                }
            }
            uiPage.Add(panel);
        }

        public String GetLabel() {
            return moPage.GetLabel();
        }

        public DIV GetPage() {
            return uiPage;
        }

        public bool IsEmpty() {
            return uiLocalAttributes.Count == 0;
        }
    }

    public interface IUIListener {
        void processEvent(Object source, String subject, params Object[] values);
    }

    //
    // UI Attribute
    //

    public delegate IUIAttribute UIAttributeCreator(IUIViewPanel viewPanel, MOView moControl);



    public interface IUIViewPanel {
        UIFrame GetUIFrame();
        void AddUIListener(IUIListener listener);
        void RemoveUIListener(IUIListener listener);
    }



    public interface IUIAttribute {
        String GetLabel();
        HComponent GetLabelComponent();
        HComponent GetControlComponent();
        IUIAttributeView GetUIView();
        void SetData(MODataObject moDataObject);
        MODataObject UpdateData();
        void ClearData();
        void Set(params String[] values);
        String[] Get();
        bool CheckMandatory();
        MOView GetMOView();
    }

    public delegate IUIAttributeControl UIAttributeControlCreator(IUIViewPanel viewPanel, MOControl moControl);
    public delegate IUIAttributeView UIAttributeViewCreator(IUIViewPanel viewPanel, MOView moView);


    public interface IUIAttributeView {
        void SetDataContext(MODataObject moDataObject);
        MODataObject GetDataContext();
        void Set(params String[] values);
        void Clear();
        HComponent GetComponent();
        IEnumerable<String> RenderToString();
    }

    public interface IUIAttributeControl : IUIAttributeView {
        String[] Get();
    }


    public abstract class UIAttributeViewBase : HContainer, IUIAttributeView {

        protected IUIViewPanel viewPanel;
        protected UIFrame uiFrame;
        protected MOView moView;
        protected String[] renderToStringS = { };
        //
        private MODataObject contextObject;

        public UIAttributeViewBase(IUIViewPanel viewPanel, MOView moView) {
            this.viewPanel = viewPanel;
            this.moView = moView;
            this.uiFrame = viewPanel.GetUIFrame();
        }

        public void SetDataContext(MODataObject moDataObject) {
            this.contextObject = moDataObject;
        }

        public MODataObject GetDataContext() {
            return this.contextObject;
        }

        public abstract void Set(params String[] values);

        public abstract void Clear();

        public virtual HComponent GetComponent() {
            return this;
        }

        public virtual IEnumerable<String> RenderToString() {
            return this.renderToStringS;
        }
    }

    public abstract class UIAttributeControlBase : UIAttributeViewBase, IUIAttributeControl {
        public UIAttributeControlBase(IUIViewPanel viewPanel, MOControl moControl) : base(viewPanel, moControl) { }
        public abstract String[] Get();

        public override IEnumerable<String> RenderToString() {
            throw new NotImplementedException();
        }
    }




    public class UIHistoryNavigation
        : HContainer, IHListener {

        private MODataObject moDataObject;
        private MOAttribute moAttribute;
        private List<MOVValue> hv;
        //
        private DIV lineDIV;
        private DIV titleDIV;
        private HLink actionLink;
        private HLink toggleLink;
        private String onText = "historical values";
        private String offText = "close historical";
        private bool toggleOn = false;
        private IUIAttribute ui;

        public UIHistoryNavigation(UIFrame uiFrame, MOAttribute moAttribute, IUIAttribute ui) {
            this.moAttribute = moAttribute;
            this.ui = ui;
            //
            if(!moAttribute.IsHistory()) {
                return;
            }
            //
            actionLink = new HLink(uiFrame);
            actionLink.SetStyleClass(UIStyles.HIST_LINE);
            lineDIV = new DIV(actionLink, UIStyles.HIST_LINE);
            toggleLink = new HLink(uiFrame);
            toggleLink.SetStyleClass(UIStyles.HIST_TITLE);
            titleDIV = new DIV();
            titleDIV.SetStyleClass(UIStyles.HIST_TITLE);
            titleDIV.Add(toggleLink);
            //
            actionLink.AddHListener(this);
            toggleLink.AddHListener(this);
        }

        public override HComponent Get(int index) {
            if(index == 0) {
                if(toggleOn) {
                    toggleLink.SetText(offText);
                } else {
                    toggleLink.SetText(onText);
                }
                return titleDIV;
            }
            int lineIndex = index - 1;
            MOVValue v = hv[lineIndex];
            String stat = ((DataState)v.GetState()).ToString().Substring(0, 1); ;
            String dateTime = DateTimeUtils.FormatDateTime(v.GetTime());
            actionLink.SetText(stat + " " + dateTime + " " + v.GetUserName());
            actionLink.SetSubId(lineIndex.ToString());
            return lineDIV;
        }

        public override int Count() {
            if(toggleLink == null) {
                return 0;
            }
            if(toggleOn) {
                return (hv != null ? hv.Count() : 0) + 1;
            } else {
                return 1;
            }
        }

        public void SetMoDataObject(MODataObject moDataObject) {
            this.moDataObject = moDataObject;
            hv = moDataObject.GetHistoricalValues(moAttribute.GetName());
        }

        public void Arrived(HEvent he) {
            hv = moDataObject.GetHistoricalValues(moAttribute.GetName());
            if(he.GetSource() == toggleLink) {
                toggleOn = !toggleOn;
            } else if(he.GetSource() == actionLink) {
                int index = Int32.Parse(actionLink.GetSubId());
                ui.Set(hv[index].GetStringValues());
            }
        }

    }

    public abstract class UIAttributeBase : IUIAttribute {

        protected MOAttribute moAttribute;
        protected MOView moView;
        protected IUIViewPanel viewPanel;
        //
        protected IUIAttributeView uiAttributeView;
        //
        protected MODataObject moDataObject;
        //
        private DIV controlComponentDIV;
        private DIV labelComponent;

        private UIHistoryNavigation historyNavigation;
        private HContainer mandatoryMessageCont;
        protected UIFrame uiFrame;

        public UIAttributeBase(IUIViewPanel viewPanel, MOView moView) {
            this.viewPanel = viewPanel;
            this.moView = moView;
            this.uiFrame = viewPanel.GetUIFrame();
            this.moAttribute = moView.GetMOAttribute();

            controlComponentDIV = new DIV();
            controlComponentDIV.SetStyleClass(UIStyles.UI_CONTROL);
            mandatoryMessageCont = new HContainer();
            // LABEL
            //
            labelComponent = new DIV();
            DIV labelPart = new DIV();
            labelPart.SetStyleClass(UIStyles.UI_LABEL);
            labelComponent.Add(labelPart);
            //
            List<String> flags = new List<String>();
            String label = moAttribute.GetLabel();
            if(moAttribute.IsMandatory()) {
                flags.Add("M");
            }
            if(moAttribute.IsHistory() && moView is MOControl) {
                DIV historyPart = new DIV();
                historyNavigation = new UIHistoryNavigation(uiFrame, moAttribute, this);
                historyPart.Add(historyNavigation);
                labelComponent.Add(historyPart);
            }
            if(moAttribute.IsFourEyesApproval()) {
                flags.Add("IKS");
            }
            if(ICollectionUtils.IsNotEmpty(flags)) {
                label += " [";
                label += ArrayUtils.Join(flags.ToArray(), " ");
                label += "]";
            }
            labelPart.SetText(label);
            //
            labelComponent.Add(mandatoryMessageCont);
        }

        public String GetLabel() {
            return moAttribute.GetLabel();
        }

        public HComponent GetLabelComponent() {
            return labelComponent;
        }

        public HComponent AddToLabelComponent(HComponent component) {
            return labelComponent.Add(component);
        }

        public MOAttribute GetMOAttribute() {
            return this.moAttribute;
        }

        public bool CheckMandatory() {
            if(moAttribute.IsMandatory()) {
                String[] sarray = StringUtils.RemoveEmptyElements(Get());
                mandatoryMessageCont.RemoveAll();
                if(ArrayUtils.IsEmpty(sarray)) {
                    DIV div = null;
                    this.mandatoryMessageCont.Add(div = new DIV(new HText(GetMandatoryCheckMessage())));
                    div.SetStyleClass(UIStyles.MO_MANDATORY_TEXT);
                    return false;
                }
            }
            return true;
        }

        public virtual void SetData(MODataObject moDataObject) {
            this.moDataObject = moDataObject;
            if(uiAttributeView != null) {
                uiAttributeView.SetDataContext(moDataObject);
            }
            if(historyNavigation != null) {
                historyNavigation.SetMoDataObject(moDataObject);
            }
            String[] values = moDataObject.GetCurrentValues(moAttribute.GetName());
            Set(values);
        }

        public virtual MODataObject UpdateData() {
            if(uiAttributeView is UIAttributeControlBase) {
                this.moDataObject.Set(moAttribute.GetName(), ((UIAttributeControlBase)uiAttributeView).Get());
            }
            return this.moDataObject;
        }

        public virtual void ClearData() {
            moDataObject = null;
            if(uiAttributeView != null) {
                uiAttributeView.Clear();
            }
        }

        public virtual void Set(params String[] values) {
            uiAttributeView.Set(values);
        }

        public virtual String[] Get() {
            if(uiAttributeView is UIAttributeControlBase) {
                return ((UIAttributeControlBase)uiAttributeView).Get();
            }
            return null;
        }

        public virtual HComponent GetControlComponent() {
            return controlComponentDIV;
        }

        public void AddToControlComponentDIV(params HComponent[] components) {
            this.controlComponentDIV.Add(components);
        }

        public virtual String GetMandatoryCheckMessage() {
            return "Mandatory!";
        }

        public virtual MOView GetMOView() {
            return this.moView;
        }

        public IUIAttributeView GetUIView() {
            return uiAttributeView;
        }

    }


    public class UIAttributeSimple : UIAttributeBase {

        public static UIAttributeSimple Create(IUIViewPanel viewPanel, MOView moControl) {
            return new UIAttributeSimple(viewPanel, moControl);
        }

        public UIAttributeSimple(IUIViewPanel viewPanel, MOView moControl)
            : base(viewPanel, moControl) {
            base.uiAttributeView = UIAttributeViewControlFactory.Create(viewPanel, moControl);
            base.AddToControlComponentDIV(uiAttributeView.GetComponent());
        }

    }


    public class UIAttributeObjectReference : IUIAttribute {

        private String moidRef;
        private String viewRef;
        private String attref;
        private IUIAttribute uiAttributeRef;
        private MODataObject mod;
        private MODataObject modRef;
        private MOView moViewRef;
        private MOClass moClassRef;

        public static UIAttributeObjectReference Create(IUIViewPanel viewPanel, MOView moView) {
            return new UIAttributeObjectReference(viewPanel, moView);
        }

        public UIAttributeObjectReference(IUIViewPanel viewPanel, MOView moView) {
            try {
                moidRef = moView.GetAttributeValue("moidref");
                viewRef = moView.GetAttributeValue("viewref");
                attref = moView.GetAttRef();
                moClassRef = MOService.GetInstance().GetMOClass(moidRef);
                moViewRef = moClassRef.GetNamedMOView(viewRef);
                uiAttributeRef = UIAttributeFactory.CreateUIAttribute(viewPanel, moViewRef);
            }
            catch(Exception) {
            }
        }

        public String GetLabel() {
            return uiAttributeRef.GetLabel();
        }

        public HComponent GetLabelComponent() {
            return uiAttributeRef.GetLabelComponent();
        }

        public HComponent GetControlComponent() {
            return uiAttributeRef.GetControlComponent();
        }

        public IUIAttributeView GetUIView() {
            return uiAttributeRef.GetUIView();
        }

        public void SetData(MODataObject moDataObject) {
            mod = moDataObject;
            String oid = mod.GetCurrentValue(attref);
            modRef = MODataObject.GetById(oid);
            uiAttributeRef.SetData(modRef);
            Set(modRef.GetCurrentValues(viewRef));
        }

        public MODataObject UpdateData() {
            return mod;
        }

        public void ClearData() {
        }

        public void Set(params String[] values) {
            uiAttributeRef.Set(values);
        }

        public String[] Get() {
            return uiAttributeRef.Get();
        }

        public bool CheckMandatory() {
            return uiAttributeRef.CheckMandatory();
        }

        public MOView GetMOView() {
            return uiAttributeRef.GetMOView();
        }

    }


    public class UIAttributeTuple : UIAttributeBase {

        private static Logger logger = Logger.GetLogger(typeof(UIAttributeTuple));

        public static UIAttributeTuple Create(IUIViewPanel viewPanel, MOView moView) {
            return new UIAttributeTuple(viewPanel, moView);
        }

        //

        private List<IUIAttributeView> uis = new List<IUIAttributeView>();
        private bool isControl;
        //private DIV tupleDIV;
        private String[] tupleValues;
        //
        private List<IUIAttribute> subAttributes = new List<IUIAttribute>();

        private UIAttributeTuple(IUIViewPanel viewPanel, MOView moView)
            : base(viewPanel, moView) {
            base.uiAttributeView = null;
            this.moView = moView;
            MOTypeTuple tupleType = (MOTypeTuple)moView.GetMOAttribute().GetMOType();
            tupleValues = tupleType.GetAttRefs().ToArray();
            isControl = moView is MOControl;

            //
            //tupleDIV = new DIV("", UIStyles.GROUP2);
            IMOAccess moAccess = MOAccess.GetInstance();
            foreach(MOView subView in moView.GetAllSubViews()) {
                if(moAccess.IsEntitled(moView)) {
                    IUIAttribute uiAttribute = UIAttributeFactory.CreateUIAttribute(uiFrame, subView);
                    subAttributes.Add(uiAttribute);
                    HComponent controlComponent = uiAttribute.GetControlComponent();
                    controlComponent.SetStyleClass(UIStyles.UI_CONTROL_INLINE);
                    base.AddToControlComponentDIV(controlComponent);
                } else {
                    logger.Debug("No entitlement for ", moView);
                }
            }
            //
            if(isControl) {
                // ?
            }
        }

        public override void Set(params String[] values) {
            logger.Debug("Set values", values);
        }

        public override MODataObject UpdateData() {
            if(isControl) {
                foreach(IUIAttribute uiAttribute in subAttributes) {
                    uiAttribute.UpdateData();
                }
                base.moDataObject.Set(base.moAttribute.GetName(), tupleValues);
            }
            return base.moDataObject;
        }

        public override void ClearData() {
            base.ClearData();
            foreach(IUIAttribute attribute in subAttributes) {
                attribute.ClearData();
            }
        }

        public override void SetData(MODataObject moDataObject) {
            foreach(IUIAttribute attribute in subAttributes) {
                attribute.SetData(moDataObject);
            }
            base.SetData(moDataObject);
        }

    }


    public class UIAttributeList : UIAttributeBase {

        public static UIAttributeList Create(IUIViewPanel viewPanel, MOView moView) {
            return new UIAttributeList(viewPanel, moView);
        }

        //

        private List<IUIAttributeView> uis = new List<IUIAttributeView>();
        private String subTypeId;
        private HLink addLink;
        private int currentCount;
        private bool isViewOnly;

        private UIAttributeList(IUIViewPanel viewPanel, MOView moView)
            : base(viewPanel, moView) {
            this.moView = moView;
            MOTypeList listType = (MOTypeList)moView.GetMOAttribute().GetMOType();
            subTypeId = listType.GetSubType().GetTypeId();
            if(moView is MOControl) {
                addLink = new HLink(uiFrame, "Add");
                ControlRenderer controlRenderer = new ControlRenderer(this);
                addLink.AddHListener(controlRenderer);
                base.AddToControlComponentDIV(controlRenderer);
                base.AddToControlComponentDIV(new DIV(addLink, HStyles.HLINK));
                isViewOnly = false;
            } else {
                base.AddToControlComponentDIV(new ViewRenderer(this));
                isViewOnly = true;
            }
        }

        public override void Set(params String[] values) {
            if(ArrayUtils.IsEmpty(values)) {
                currentCount = 0;
                return;
            }
            for(int i = 0; i < values.Length; i++) {
                if(i >= uis.Count) {
                    AddOneIUIAttributeViewControl();
                }
                uis[i].Set(values[i]);
            }
            currentCount = values.Length;
        }

        public override MODataObject UpdateData() {
            if(isViewOnly) {
                return base.moDataObject;
            }
            List<String> avalues = new List<String>();
            for(int i = 0; i < currentCount; i++) {
                String[] tmp = ((UIAttributeControlBase)uis[i]).Get();
                if(ArrayUtils.IsNotEmpty(tmp)) {
                    avalues.Add(tmp[0]);
                }
            }
            base.moDataObject.Set(base.moAttribute.GetName(), avalues.ToArray());
            return base.moDataObject;
        }

        public override void ClearData() {
            currentCount = 0;
        }

        public override void SetData(MODataObject moDataObject) {
            foreach(IUIAttributeView ui in uis) {
                ui.SetDataContext(moDataObject);
            }
            base.SetData(moDataObject);
        }

        private class ViewRenderer : HContainer {

            protected UIAttributeList outer;
            protected DIV lineDiv;

            public ViewRenderer(UIAttributeList outer) {
                this.outer = outer;
                lineDiv = new DIV();
                lineDiv.SetAttribute(HDTD.AttName.STYLE, "white-space:nowrap;");
            }

            public override HComponent Get(int index) {
                lineDiv.Set(outer.uis[index].GetComponent());
                return lineDiv;
            }

            public override int Count() {
                return outer.currentCount;
            }
        }

        private class ControlRenderer : ViewRenderer, IHListener {

            private HLink deleteLink;

            public ControlRenderer(UIAttributeList outer)
                : base(outer) {
                deleteLink = new HLink(outer.uiFrame, HStyles.DELETE_ICON);
                deleteLink.AddHListener(this);
            }

            public override HComponent Get(int index) {
                deleteLink.SetSubId(index.ToString());
                lineDiv.Set(outer.uis[index].GetComponent(), deleteLink);
                return lineDiv;
            }

            public void Arrived(HEvent he) {
                if(he.GetSource() == deleteLink) {
                    int deleteIndex = Int32.Parse(deleteLink.GetSubId());
                    outer.RemoveOneIUIAttributeViewControl(deleteIndex);

                } else if(he.GetSource() == outer.addLink) {
                    outer.AddOneIUIAttributeViewControl();
                }
            }
        }

        private void RemoveOneIUIAttributeViewControl(int index) {
            IUIAttributeView ui = uis[index];
            ui.SetDataContext(null);
            uis.RemoveAt(index);
            currentCount--;
        }

        private void AddOneIUIAttributeViewControl() {
            IUIAttributeView ui = UIAttributeViewControlFactory.Create(viewPanel, moView);
            ui.Clear();
            ui.SetDataContext(base.moDataObject);
            uis.Add(ui);
            currentCount++;
        }

    }


    public class UIAttributeOneOf : UIAttributeBase, IHListener {

        private static Logger logger = Logger.GetLogger(typeof(UIAttributeOneOf));

        public static UIAttributeOneOf Create(IUIViewPanel viewPanel, MOView moView) {
            return new UIAttributeOneOf(viewPanel, moView);
        }

        //

        //private List<IUIAttributeView> uis = new List<IUIAttributeView>();
        private bool isControl;
        private DIV navigationDIV;
        private DIV selectedAttributeDIV;
        //
        private DIV selectionButtonsDIV;
        private DIV changeButtonDIV;
        private HLink changeSelectionBt;
        private DIV selectOneLabelDIV;
        //
        private List<IUIAttribute> subAttributes = new List<IUIAttribute>();
        private IUIAttribute selectedAttribute;

        private UIAttributeOneOf(IUIViewPanel viewPanel, MOView moView)
            : base(viewPanel, moView) {
            base.uiAttributeView = null;
            this.moView = moView;
            MOTypeOneOf oneOfType = (MOTypeOneOf)moView.GetMOAttribute().GetMOType();
            isControl = moView is MOControl;

            //
            IMOAccess moAccess = MOAccess.GetInstance();
            foreach(MOView subView in moView.GetAllSubViews()) {
                if(moAccess.IsEntitled(moView)) {
                    IUIAttribute uiAttribute = UIAttributeFactory.CreateUIAttribute(uiFrame, subView);
                    subAttributes.Add(uiAttribute);
                } else {
                    logger.Debug("No entitlement for ", moView);
                }
            }
            //
            navigationDIV = new DIV();
            selectedAttributeDIV = new DIV("", UIStyles.GROUP2);
            if(isControl) {
                //
                selectOneLabelDIV = new DIV("Select One", HStyles.HLABEL);
                changeSelectionBt = new HLink(uiFrame, "Change Selection");
                changeButtonDIV = new DIV(changeSelectionBt, UIStyles.GROUP);
                changeSelectionBt.AddHListener(this);
                //
                selectionButtonsDIV = new DIV("", UIStyles.GROUP);
                foreach(MOView subView in moView.GetAllSubViews()) {
                    HLink hlink = new HLink(uiFrame, subView.GetMOAttribute().GetLabel(), subView.GetMOAttribute().GetName());
                    hlink.AddHListener(this);
                    selectionButtonsDIV.Add(new DIV(hlink, HStyles.HLINK));
                }
            }
            base.AddToControlComponentDIV(selectedAttributeDIV);

            base.AddToLabelComponent(navigationDIV);

        }

        public override void Set(params String[] values) {
            ClearUI();
            if(ArrayUtils.IsNotEmpty(values)) {
                String attref = values[0];
                selectedAttribute = GetLocalUIAttribute(attref);
                if(selectedAttribute == null) {
                    return;
                }
                DisplaySelectedAttribute();
            }
        }

        private void DisplaySelectedAttribute() {
            if(selectedAttribute == null) {
                ClearUI();
            }
            if(isControl) {
                navigationDIV.Set(changeButtonDIV);
            } else {
                navigationDIV.RemoveAll();
            }
            selectedAttributeDIV.Set(selectedAttribute.GetLabelComponent(), selectedAttribute.GetControlComponent());
        }

        private IUIAttribute GetLocalUIAttribute(String attref) {
            foreach(IUIAttribute uiAttribute in subAttributes) {
                if(uiAttribute.GetMOView().GetMOAttribute().GetName().Equals(attref)) {
                    return uiAttribute;
                }
            }
            return null;
        }

        public override MODataObject UpdateData() {
            if(isControl) {
                String attref = null;
                if(selectedAttribute != null) {
                    attref = selectedAttribute.GetMOView().GetMOAttribute().GetName();
                    selectedAttribute.UpdateData();
                    base.moDataObject.Set(base.moAttribute.GetName(), attref);
                }
            }
            return base.moDataObject;
        }

        public override void ClearData() {
            base.ClearData();
            foreach(IUIAttribute attribute in subAttributes) {
                attribute.ClearData();
            }
            ClearUI();
        }

        private void ClearUI() {
            if(isControl) {
                navigationDIV.RemoveAll();
                navigationDIV.Add(selectOneLabelDIV);
                navigationDIV.Add(selectionButtonsDIV);
            } else {
                navigationDIV.RemoveAll();
            }
            selectedAttributeDIV.RemoveAll();
        }

        public override void SetData(MODataObject moDataObject) {
            foreach(IUIAttribute attribute in subAttributes) {
                attribute.SetData(moDataObject);
            }
            base.SetData(moDataObject);
        }

        public void Arrived(HEvent he) {
            if(he.GetSource() == changeSelectionBt) {
                ClearUI();
                return;
            }
            selectedAttribute = GetLocalUIAttribute(he.GetSubEventId());
            DisplaySelectedAttribute();
        }

    }


    public class UIAttributeComp : UIAttributeBase {

        public static UIAttributeComp Create(IUIViewPanel viewPanel, MOView moControl) {
            return new UIAttributeComp(viewPanel, moControl);
        }

        //


        private UIAttributeComp(IUIViewPanel viewPanel, MOView moControl)
            : base(viewPanel, moControl) {
            uiAttributeView = UIAttributeViewControlFactory.Create(viewPanel, moControl);
            AddToControlComponentDIV(uiAttributeView.GetComponent());
        }

        public override MODataObject UpdateData() {
            if(uiAttributeView is UIAttributeControlComp) {
                UIAttributeControlComp ui = (UIAttributeControlComp)uiAttributeView;
                ui.Update();
            }
            return base.UpdateData();
        }

    }

    public class UIAttributeForeach : UIAttributeBase {

        public static UIAttributeForeach Create(IUIViewPanel viewPanel, MOView moControl) {
            return new UIAttributeForeach(viewPanel, moControl);
        }

        //

        private UIAttributeForeach(IUIViewPanel viewPanel, MOView moControl)
            : base(viewPanel, moControl) {
            uiAttributeView = UIAttributeViewControlFactory.Create(viewPanel, moControl);
            AddToControlComponentDIV(uiAttributeView.GetComponent());
        }

        public override MODataObject UpdateData() {
            if(uiAttributeView is UIAttributeControlForeach) {
                UIAttributeControlForeach ui = (UIAttributeControlForeach)uiAttributeView;
                ui.Update();
            }
            return base.UpdateData();
        }


    }


    public class UIAttributeFactory {

        private static Logger logger = Logger.GetLogger(typeof(UIAttributeFactory));

        private static Dictionary<String, UIAttributeCreator> createMethods = new Dictionary<String, UIAttributeCreator>();

        public static void RegisterUIAttributeCreator(String key, UIAttributeCreator createMethod) {
            createMethods.Put(key, createMethod);
        }

        static UIAttributeFactory() {
            createMethods.Put(MOType.STRING, UIAttributeSimple.Create);
            createMethods.Put(MOType.TEXT, UIAttributeSimple.Create);
            createMethods.Put(MOType.INTEGER, UIAttributeSimple.Create);
            createMethods.Put(MOType.FLOAT, UIAttributeSimple.Create);
            createMethods.Put(MOType.PERCENTAGE, UIAttributeSimple.Create);
            createMethods.Put(MOType.DATE, UIAttributeSimple.Create);
            createMethods.Put(MOType.DATETIME, UIAttributeSimple.Create);
            createMethods.Put(MOType.CURRENCY, UIAttributeSimple.Create);
            createMethods.Put(MOType.TUPLE, UIAttributeTuple.Create);
            createMethods.Put(MOType.LIST, UIAttributeList.Create);
            createMethods.Put(MOType.REF, UIAttributeSimple.Create);
            createMethods.Put(MOType.LIST + "-" + MOType.REF, UIAttributeSimple.Create);
            createMethods.Put(MOType.COMP, UIAttributeComp.Create);
            createMethods.Put(MOType.LIST + "-" + MOType.COMP, UIAttributeComp.Create);
            createMethods.Put(MOType.CODETABLE, UIAttributeSimple.Create);
            createMethods.Put(MOType.ONEOF, UIAttributeOneOf.Create);
            createMethods.Put(MOType.FOREACH, UIAttributeForeach.Create);
            createMethods.Put(MOType.OBJECTREF, UIAttributeObjectReference.Create);
        }

        public static IUIAttribute CreateUIAttribute(IUIViewPanel viewPanel, MOView moView) {
            // View Ref
            //
            UIFrame uiFrame = viewPanel.GetUIFrame();
            if(moView.GetAttributeValue("moidref") != null) {
                logger.Info(moView);
            }
            String widgetId = moView.GetWidgetHint();
            if(widgetId == null) {
                try {
                    widgetId = moView.GetMOAttribute().GetMOType().GetFullTypeId();
                }
                catch(Exception e) {
                    logger.Error(moView, e);
                }
            }
            logger.Debug(widgetId);
            if(createMethods.ContainsKey(widgetId)) {
                return createMethods.Get(widgetId)(viewPanel, moView);
            }
            if(widgetId.StartsWith(MOType.LIST)) {
                return createMethods.Get(MOType.LIST)(viewPanel, moView);
            } else {
                return UIAttributeSimple.Create(viewPanel, moView);
            }
        }

    }

    //

    public class UIAttributeControlDateTime : UIAttributeControlBase {

        private static Logger logger = Logger.GetLogger(typeof(UIAttributeControlDateTime));

        public static UIAttributeControlDateTime Create(IUIViewPanel viewPanel, MOControl moControl) {
            return new UIAttributeControlDateTime(viewPanel, moControl);
        }

        //

        private HTextField dateTf;
        private HTextField timeTf;

        private UIAttributeControlDateTime(IUIViewPanel viewPanel, MOControl moControl)
            : base(viewPanel, moControl) {
            Add(new SPAN(UIStyles.MO_NOWRAP, new HText("[yyyy-MM-dd] "), dateTf = new HTextField(uiFrame, 11, 10), new NBSP(),
                new HText(" [HH:mm] "), timeTf = new HTextField(uiFrame, 5)));
        }

        public override void Set(params String[] values) {
            Clear();
            if(ArrayUtils.IsNotEmpty(values) && StringUtils.IsNotEmpty(values[0])) {
                try {
                    DateTime dt = DateTimeUtils.Parse(values[0]);
                    dateTf.SetText(String.Format("{0:yyyy-MM-dd}", dt));
                    timeTf.SetText(String.Format("{0:HH:mm}", dt));
                }
                catch(Exception) {
                    logger.Warn("could not parse date time: ", ArrayUtils.Join(values));
                    Clear();
                }
            }
        }

        public override String[] Get() {
            return ObjectUtils.ToArray(dateTf.GetText() + " " + timeTf.GetText());
        }

        public override void Clear() {
            dateTf.SetText("");
            timeTf.SetText("");
        }

    }


    public class UIAttributeViewDateTime : UIAttributeViewBase {

        private static Logger logger = Logger.GetLogger(typeof(UIAttributeViewDateTime));

        public static UIAttributeViewDateTime Create(IUIViewPanel viewPanel, MOView moView) {
            return new UIAttributeViewDateTime(viewPanel, moView);
        }

        // 

        private DIV dateTimeDIV;

        private UIAttributeViewDateTime(IUIViewPanel viewPanel, MOView moView)
            : base(viewPanel, moView) {
            Add(dateTimeDIV = new DIV());
            dateTimeDIV.SetStyleClass(UIStyles.UI_VALUE, UIStyles.MO_NOWRAP);
        }

        public override void Set(params String[] values) {
            Clear();
            if(ArrayUtils.IsNotEmpty(values) && StringUtils.IsNotEmpty(values[0])) {
                try {
                    DateTime dt = DateTimeUtils.Parse(values[0]);
                    String value = String.Format("{0:yyyy-MM-dd HH:mm}", dt);
                    dateTimeDIV.SetText(value);
                    renderToStringS = new String[] { value };
                }
                catch(Exception) {
                    logger.Warn("could not parse date time: ", ArrayUtils.Join(values));
                    Clear();
                }
            }
        }

        public override void Clear() {
            dateTimeDIV.SetText("");
            renderToStringS = ArrayUtils.EmptyList<String>();
        }

    }

    internal class UIAttributeControlDate : UIAttributeControlBase {

        private static Logger logger = Logger.GetLogger(typeof(UIAttributeControlDate));

        public static UIAttributeControlDate Create(IUIViewPanel viewPanel, MOControl moControl) {
            return new UIAttributeControlDate(viewPanel, moControl);
        }

        //

        private HTextField dateTf;

        private UIAttributeControlDate(IUIViewPanel viewPanel, MOControl moControl)
            : base(viewPanel, moControl) {
            Add(new SPAN(UIStyles.MO_NOWRAP, new HText("[yyyy-MM-dd] "), dateTf = new HTextField(uiFrame, 11, 10)));
        }

        public override void Set(params String[] values) {
            Clear();
            if(ArrayUtils.IsNotEmpty(values) && StringUtils.IsNotEmpty(values[0])) {
                try {
                    dateTf.SetText(DateTimeUtils.tryIsoDate(values[0]));
                }
                catch(Exception) {
                    logger.Warn("could not parse date : ", ArrayUtils.Join(values));
                    Clear();
                }
            }
        }

        public override String[] Get() {
            return ObjectUtils.ToArray(DateTimeUtils.tryIsoDate(dateTf.GetText()));
        }

        public override void Clear() {
            dateTf.SetText("");
        }
    }

    public class UIAttributeViewDate : UIAttributeViewBase {

        private static Logger logger = Logger.GetLogger(typeof(UIAttributeViewDate));

        public static UIAttributeViewDate Create(IUIViewPanel viewPanel, MOView moView) {
            return new UIAttributeViewDate(viewPanel, moView);
        }

        //

        private DIV dateDIV;

        private UIAttributeViewDate(IUIViewPanel viewPanel, MOView moView)
            : base(viewPanel, moView) {
            Add(dateDIV = new DIV());
            dateDIV.SetStyleClass(UIStyles.UI_VALUE, UIStyles.MO_NOWRAP);
        }

        public override void Set(params String[] values) {
            Clear();
            if(ArrayUtils.IsNotEmpty(values) && StringUtils.IsNotEmpty(values[0])) {
                try {
                    String value = DateTimeUtils.tryIsoDate(values[0]);
                    dateDIV.SetText(value);
                    renderToStringS = new String[] { value };
                }
                catch(Exception) {
                    logger.Warn("could not parse date : ", ArrayUtils.Join(values));
                }
            }
        }

        public override void Clear() {
            dateDIV.SetText("");
            renderToStringS = ArrayUtils.EmptyList<String>();
        }
    }

    public class UIAttributeControlCurrency : UIAttributeControlBase {

        public static readonly String CURRENCY_CODETABLE_NAME = "currencies";

        private static Logger logger = Logger.GetLogger(typeof(UIAttributeControlCurrency));

        public static UIAttributeControlCurrency Create(IUIViewPanel viewPanel, MOControl moControl) {
            return new UIAttributeControlCurrency(viewPanel, moControl);
        }

        //

        private HTextField amountTf;
        private HList<ICodeTableElement> codeTableHList;


        private UIAttributeControlCurrency(IUIViewPanel viewPanel, MOControl moControl)
            : base(viewPanel, moControl) {
            ICodeTable ct = CodeTable.Create(CURRENCY_CODETABLE_NAME, false);
            codeTableHList = new HList<ICodeTableElement>(uiFrame, ct, false, 1);
            Add(new SPAN(UIStyles.MO_NOWRAP, amountTf = new HTextField(uiFrame, 10), codeTableHList));
        }

        public override void Set(params String[] values) {
            Clear();
            if(ArrayUtils.IsNotEmpty(values) && values.Length >= 2) {
                amountTf.SetText(values[0]);
                String code = values[1];
                codeTableHList.SetSelected(new CodeTableElement(code, "-"));
            }
        }

        public override String[] Get() {
            IList<ICodeTableElement> codeTableElements = codeTableHList.GetSelected();
            if(ICollectionUtils.IsNotEmpty(codeTableElements)) {
                return ObjectUtils.ToArray(amountTf.GetText(), codeTableElements[0].GetCode());
            } else {
                logger.Warn("Unexpected state: No currency selected!");
            }
            return ObjectUtils.ToArray(amountTf.GetText(), UIConstants.N_A);
        }

        public override void Clear() {
            amountTf.SetText("");
            codeTableHList.SetSelectedIndex(0);
        }
    }

    public class UIAttributeViewCurrency : UIAttributeViewBase {
        private static Logger logger = Logger.GetLogger(typeof(UIAttributeViewCurrency));

        public static UIAttributeViewCurrency Create(IUIViewPanel viewPanel, MOView moView) {
            return new UIAttributeViewCurrency(viewPanel, moView);
        }

        //

        private DIV currencyDIV;
        private ICodeTable ct;


        private UIAttributeViewCurrency(IUIViewPanel viewPanel, MOView moView)
            : base(viewPanel, moView) {
            Add(currencyDIV = new DIV());
            currencyDIV.SetStyleClass(UIStyles.UI_VALUE, UIStyles.MO_NOWRAP);
            ct = CodeTable.Create(UIAttributeControlCurrency.CURRENCY_CODETABLE_NAME, false);
        }

        public override void Set(params String[] values) {
            if(ArrayUtils.IsNotEmpty(values) && values.Length >= 2) {
                ICodeTableElement ce = ct.GetElement(values[1]);
                if(ce != null) {
                    currencyDIV.SetText(values[0] + " " + ce.GetName());
                } else {
                    currencyDIV.SetText(values[0] + " " + UIConstants.N_A);
                    logger.Warn("Currency code not found:", "amount", values[0], "code", values[1]);
                }

            } else {
                currencyDIV.SetText(UIConstants.N_A);
            }
            base.renderToStringS = new String[] { currencyDIV.GetText() };
        }

        public override void Clear() {
            currencyDIV.SetText("");
        }
    }

    public class UIAttributeControlString : UIAttributeControlBase {

        public static UIAttributeControlString Create(IUIViewPanel viewPanel, MOControl moControl) {
            return new UIAttributeControlString(viewPanel, moControl);
        }

        private HTextField textField;
        private HText pcTx;

        private UIAttributeControlString(IUIViewPanel viewPanel, MOControl moControl)
            : base(viewPanel, moControl) {
            MOType moType = moControl.GetMOAttribute().GetMOType().GetBaseType();
            int maxChars = moType.GetMaxChars();
            int maxCols = moControl.GetMaxCols();
            if(maxCols > maxChars) {
                maxCols = maxChars + 2;
            }
            Add(textField = new HTextField(uiFrame, maxCols, maxChars));
            if(moType.GetTypeId().Equals(MOType.PERCENTAGE)) {
                Add(pcTx = new HText("%"));
            }
        }

        public override void Set(params String[] values) {
            if(ArrayUtils.IsNotEmpty(values)) {
                textField.SetText(values[0]);
            } else {
                Clear();
            }
        }

        public override String[] Get() {
            String s = textField.GetText();
            return ObjectUtils.ToArray(s);
        }

        public override void Clear() {
            textField.SetText("");
        }
    }


    public class UIAttributeViewBoolean : UIAttributeViewBase {

        public static UIAttributeViewBoolean Create(IUIViewPanel viewPanel, MOView moView) {
            return new UIAttributeViewBoolean(viewPanel, moView);
        }

        protected DIV stringDIV;

        protected UIAttributeViewBoolean(IUIViewPanel viewPanel, MOView moView)
            : base(viewPanel, moView) {
            Add(stringDIV = new DIV());
            stringDIV.SetStyleClass(UIStyles.UI_VALUE, UIStyles.MO_NOWRAP);
        }

        public override void Set(params String[] values) {
            if(ArrayUtils.IsNotEmpty(values)) {
                String v = values[0];
                stringDIV.SetText(v);
                base.renderToStringS = new String[] { v };
            } else {
                Clear();
            }
        }

        public override void Clear() {
            stringDIV.RemoveAll();
        }
    }

    public class UIAttributeControlBoolean : UIAttributeControlBase {

        public static UIAttributeControlBoolean Create(IUIViewPanel viewPanel, MOControl moControl) {
            return new UIAttributeControlBoolean(viewPanel, moControl);
        }

        private HCheckBox trueCb;

        private UIAttributeControlBoolean(IUIViewPanel viewPanel, MOControl moControl)
            : base(viewPanel, moControl) {
            Add(trueCb = new HCheckBox(uiFrame));
        }
        public override void Set(params String[] values) {
            trueCb.SetChecked(ArrayUtils.IsNotEmpty(values) && values[0].Equals("true"));
        }
        public override String[] Get() {
            return ObjectUtils.ToArray(trueCb.IsChecked() ? "true" : "false");
        }
        public override void Clear() {
            trueCb.SetChecked(false);
        }
    }


    public class UIAttributeViewString : UIAttributeViewBase {

        public static UIAttributeViewString Create(IUIViewPanel viewPanel, MOView moView) {
            return new UIAttributeViewString(viewPanel, moView);
        }

        private DIV stringDIV;
        private bool isPercentage;

        protected UIAttributeViewString(IUIViewPanel viewPanel, MOView moView)
            : base(viewPanel, moView) {
            Add(stringDIV = new DIV(default(String), UIStyles.UI_VALUE));
            isPercentage = MOType.PERCENTAGE.Equals(moView.GetMOAttribute().GetMOType().GetBaseType().GetTypeId());
        }

        public override void Set(params String[] values) {
            Clear();
            if(ArrayUtils.IsNotEmpty(values)) {
                String v = values[0] + (isPercentage ? "%" : "");
                stringDIV.SetText(v);
                renderToStringS = new String[] { v };
            }
        }

        public override void Clear() {
            stringDIV.RemoveAll();
            renderToStringS = ArrayUtils.EmptyList<String>();
        }
    }



    public class UIAttributeControlCodeTable : UIAttributeControlBase {

        private static Logger logger = Logger.GetLogger(typeof(UIAttributeControlCodeTable));

        public static UIAttributeControlCodeTable Create(IUIViewPanel viewPanel, MOControl moControl) {
            return new UIAttributeControlCodeTable(viewPanel, moControl);
        }

        private HList<ICodeTableElement> codeTableHList;

        private MOTypeCodeTable ctType;

        private UIAttributeControlCodeTable(IUIViewPanel viewPanel, MOControl moControl)
            : base(viewPanel, moControl) {
            MOAttribute moAttribute = moControl.GetMOAttribute();
            ctType = (MOTypeCodeTable)moAttribute.GetMOType().GetBaseType();
          codeTableHList = new HList<ICodeTableElement>(uiFrame, null, ctType.IsMultiple(), 1);
            Add(codeTableHList);
        }

        public override void Set(params String[] values) {
            ICodeTable ct = MODataObjectHelper.GetCodeTable(GetDataContext(), ctType, true, Access.WRITE);
            codeTableHList.SetModel(ct);
            if(ArrayUtils.IsNotEmpty(values)) {
                try {
                    String code = values[0].Trim();
                    codeTableHList.SetSelected(new CodeTableElement(code, "-"));
                }
                catch(Exception) {
                    Clear();
                }
            } else {
                Clear();
            }
        }

        public override String[] Get() {
            List<String> res = new List<String>();
            foreach(CodeTableElement cte in codeTableHList.GetSelected()) {
                if(!cte.Equals(CodeTable.EMTPY_CODE_TABLE_ELEMENT)) {
                    res.AddUnique(cte.GetCode());
                }
            }
            return res.ToArray();
        }

        public override void Clear() {
            codeTableHList.SetSelected(null);
        }
    }

    public class UIAttributeViewCodeTable : UIAttributeViewBase {

        private static Logger logger = Logger.GetLogger(typeof(UIAttributeViewCodeTable));

        public static UIAttributeViewCodeTable Create(IUIViewPanel viewPanel, MOView moView) {
            return new UIAttributeViewCodeTable(viewPanel, moView);
        }

        private MOTypeCodeTable ctType;
        private String[] codes;
        private DIV textDIV;

        protected UIAttributeViewCodeTable(IUIViewPanel viewPanel, MOView moView)
            : base(viewPanel, moView) {
            MOAttribute moAttribute = moView.GetMOAttribute();
            ctType = (MOTypeCodeTable)moAttribute.GetMOType().GetBaseType();
            textDIV = new DIV(default(String), UIStyles.UI_VALUE);
        }


        public override void Set(params String[] codes) {
            this.codes = codes;
        }

        public override HComponent Get(int index) {
            ICodeTable ct = MODataObjectHelper.GetCodeTable(GetDataContext(), ctType, true);

            if(ct == null) {
                logger.Debug("Unexpected null value for code table ");
            }
            ICodeTableElement ce = null;
            if(ArrayUtils.IsNotEmpty(codes)) {
                  ce =  ct.GetElement(codes[index]);
            }
            if(ce == null) {
                logger.Warn(ct.GetName(), "no code table element at", index);
                textDIV.SetText(UIConstants.N_A);
            } else {
                textDIV.SetText(ce.GetName());
            }
            return textDIV;
        }

        public override int Count() {
            return ArrayUtils.Length(codes);
        }

        public override void Clear() {
            codes = null;
        }

        public override IEnumerable<String> RenderToString() {
            ICodeTable ct = MODataObjectHelper.GetCodeTable(GetDataContext(), ctType, true); 
            StringBuilder sb = new StringBuilder();
            for(int i = 0; i < Count(); i++) {
                if(i > 0) {
                    sb.Append(", ");
                }
                ICodeTableElement ce = ct.GetElement(codes[i]);
                if(ce == null) {
                    logger.Warn(ct.GetName(), "no code table element at", i);
                } else {
                    sb.Append(ce.GetName());
                }
            }
            base.renderToStringS = new String[] { sb.ToString() };
            return base.renderToStringS;
        }
    }





    public class UIAttributeViewBackRef : UIAttributeViewBase {

        private static readonly Logger logger = Logger.GetLogger(typeof(UIAttributeViewBackRef));

        public static UIAttributeViewBackRef Create(IUIViewPanel viewPanel, MOView moControl) {
            return new UIAttributeViewBackRef(viewPanel, moControl);
        }

        private String moidRef;
        private String attRef;
        private MOAttribute moAttribute;
        //private bool isMultiple;
        private MOSearchCriteria searchCriteria;
        private MOAttribute searchAttribute;
        private UIDataSelectionList referedData;


        private UIAttributeViewBackRef(IUIViewPanel viewPanel, MOView moView)
            : base(viewPanel, moView) {
            this.moAttribute = moView.GetMOAttribute();
            MOTypeBackRef moType = (MOTypeBackRef)moAttribute.GetMOType().GetBaseType();
            moidRef = moType.GetMoidRef();
            attRef = moType.GetAttRef();
            //isMultiple = moAttribute.GetMOType().IsMultiple();
            //
            searchCriteria = new MOSearchCriteria();
            searchCriteria.AddIncludedDataStates(DataState.APPROVED, DataState.STORED, DataState.NEW);
            searchCriteria.AddIncludedMoids(moidRef);
            //
            searchAttribute = MOService.GetInstance().GetMOAttribute(moidRef, attRef);
            if(searchAttribute == null) {
                logger.Warn("attribute does not exist! ", moidRef, attRef);
            }
            referedData = new UIDataSelectionList(uiFrame, true);
            Add(new DIV(referedData));
        }

        public override void Set(params String[] values) {
            Clear();
            StringBuilder sb = new StringBuilder();
            MODataObject d = base.GetDataContext();
            if(d == null) {
                logger.Warn("Data context not set - can not search for back-ref objects");
                return;
            }
            values = d.GetBackRefValues(moView.GetMOAttribute());
            List<MODataObject> res = MODataObject.GetByIds(values);
            //searchCriteria.RequestExactMatch(searchAttribute, d.GetOid().ToString());
            //List<MODataObject> res = MODataObject.Search(searchCriteria);
            referedData.SetObjects(res);

            base.renderToStringS = new String[res.Count];
            int index = 0;
            foreach(MODataObject mod in res) {
                base.renderToStringS[index] = mod.ToSynopsisString(moView.GetSynopsisName());
                index++;
            }
        }

        public override void Clear() {
            referedData.RemoveAllObjects();
            base.renderToStringS = ArrayUtils.EmptyList<String>();
        }

    }


    public class UIAttributeControlRef : UIAttributeControlBase, IHListener, IObjectCommandListener<MODataObject> {

        internal static UIAttributeControlRef Create(IUIViewPanel viewPanel, MOControl moControl) {
            return new UIAttributeControlRef(viewPanel, moControl);
        }

        private HObjectList<MODataObject> dataList;
        private HLink searchBt;
        private String refMoid;
        private MOAttribute moAttribute;
        private bool isMultiple;
        // HListSelectionPanel<MODataObject>

        private UIAttributeControlRef(IUIViewPanel viewPanel, MOControl moControl)
            : base(viewPanel, moControl) {
            this.moAttribute = moControl.GetMOAttribute();
            MOTypeRef moType = (MOTypeRef)moAttribute.GetMOType().GetBaseType();
            refMoid = moType.GetLookupMoid();
            isMultiple = moAttribute.GetMOType().IsMultiple();
            Add(new DIV(dataList = new HObjectList<MODataObject>(uiFrame, new UIDataObjectCellsRenderer(uiFrame))));
            Add(new DIV(searchBt = new HLink(uiFrame, "Lookup ..."), HStyles.HLINK));
            //
            searchBt.AddHListener(this);
        }

        public override void Set(params String[] values) {
            dataList.RemoveAllObjects();
            if(ArrayUtils.IsNotEmpty(values)) {
                foreach(String oid in values) {
                    dataList.AddObjects(MODataObject.GetById(Int64.Parse(oid)));
                }
            }
        }

        public override String[] Get() {
            List<MODataObject> l = dataList.GetObjects();
            String[] res = new String[l.Count];
            for(int i = 0; i < l.Count; i++) {
                res[i] = l[i].GetOid().ToString();
            }
            return res;
        }

        public override void Clear() {
            dataList.RemoveAllObjects();
        }

        public void Arrived(HEvent he) {
            if(he.GetSource() == searchBt) {
                UIDataObjectRefSearchPanel refSearchPanel = uiFrame.GetRefSearchPanel();
                MOClass moClass = MOService.GetInstance().GetMOClass(refMoid);
                String className = moClass.GetName();
                // TODO moClass.get
                refSearchPanel.ReInit(className + " :: Search for " + moAttribute.GetLabel(), isMultiple, this, refMoid);
                //uiFrame.OpenDialog(panel);
                uiFrame.OpenDialogOnStack(refSearchPanel);
            }
        }

        public void Arrived(ObjectCommandEvent<MODataObject> he) {
            UIDataObjectRefSearchPanel searchPanel = uiFrame.GetRefSearchPanel();
            if(he.GetSource() == searchPanel) {
                if(he.getCommand() == UIDataObjectRefSearchPanel.DONE) {
                    IList<MODataObject> selection = searchPanel.GetSelected();
                    if(isMultiple) {
                        foreach(MODataObject d in selection) {
                            dataList.AddObjects(d);
                        }
                    } else {
                        if(ICollectionUtils.IsNotEmpty(selection)) {
                            dataList.RemoveAllObjects();
                            dataList.AddObjects(selection.First());
                        }
                    }
                }
                searchPanel.Clear();
                uiFrame.CloseDialogOnStack();
            }
        }
    }


    public class UIAttributeViewRef : UIAttributeViewBase {

        internal static UIAttributeViewRef Create(IUIViewPanel viewPanel, MOView moControl) {
            return new UIAttributeViewRef(viewPanel, moControl);
        }

        private MOAttribute moAttribute;
        private UIDataSelectionList resultList;

        private UIAttributeViewRef(IUIViewPanel viewPanel, MOView moControl)
            : base(viewPanel, moControl) {
            this.moAttribute = moControl.GetMOAttribute();
            MOTypeRef moType = (MOTypeRef)moAttribute.GetMOType().GetBaseType();
            Add(new DIV(resultList = new UIDataSelectionList(uiFrame, true)));
        }

        public override void Set(params String[] values) {
            resultList.RemoveAllObjects();
            StringBuilder sb = new StringBuilder();
            List<String> renderList = new List<string>();
            if(ArrayUtils.IsNotEmpty(values)) {
                for(int i = 0; i < values.Length; i++) {
                    if(i > 0) {
                        sb.Append(", ");
                    }
                    String oid = values[i];
                    MODataObject mod = MODataObject.GetById(oid);
                    if(mod != null) {
                        resultList.AddObject(mod);
                        renderList.Add(mod.ToSynopsisString());
                    }
                }
            }
            base.renderToStringS = renderList.ToArray();
        }

        public override void Clear() {
            resultList.RemoveAllObjects();
            base.renderToStringS = ArrayUtils.EmptyList<String>();
        }

    }

    public class UIAttributeControlComp : UIAttributeControlBase, IHListener, IObjectCommandListener<MODataObject> {

        private static readonly Logger logger = Logger.GetLogger(typeof(UIAttributeControlComp));

        public static UIAttributeControlComp Create(IUIViewPanel viewPanel, MOControl moControl) {
            return new UIAttributeControlComp(viewPanel, moControl);
        }

        private static readonly String DELETE = "Delete", EDIT = "Edit";
        private static readonly String[] commands = { DELETE, EDIT };

        // MO
        private MOAttribute moAttribute;
        private String compMoid;
        // LIST DATA
        private HObjectCommandList<MODataObject> dataList;
        private bool isMultiple;
        private DIV dataDIV;
        // EDIT COMP
        private UIEditPanel compEditPanel;
        private DIV compEditDIV;
        private HLink newBt;
        private HLink doneBt;
        private HLink cancelBt;
        private DIV buttonDIV;
        private bool isEditPanelOpen;
        //

        private UIAttributeControlComp(IUIViewPanel viewPanel, MOControl moControl)
            : base(viewPanel, moControl) {
            this.moAttribute = moControl.GetMOAttribute();

            MOTypeComp moTypeComp = (MOTypeComp)moAttribute.GetMOType().GetBaseType();
            compMoid = moTypeComp.GetLookupMoid();
            isMultiple = moAttribute.GetMOType().IsMultiple();
            //
            //
            dataList = new HObjectCommandList<MODataObject>(uiFrame, new UIDataObjectCellsRenderer(uiFrame), commands);
            dataDIV = new DIV(dataList);
            //
            compEditPanel = new UIEditPanel(uiFrame, compMoid);
            newBt = new HLink(uiFrame, "New");
            doneBt = new HLink(uiFrame, "Done");
            cancelBt = new HLink(uiFrame, "Cancel");
            buttonDIV = new DIV(HStyles.BUTTON_PANEL, doneBt, cancelBt);
            compEditDIV = new DIV(newBt, HStyles.HLINK);
            //
            doneBt.AddHListener(this);
            cancelBt.AddHListener(this);
            newBt.AddHListener(this);
            dataList.AddObjectCommandListener(this);
            //
            CloseEditPanel();
        }

        public override HComponent Get(int index) {
            switch(index) {
                case 0: return this.dataDIV;
                case 1: return compEditDIV;
            }
            return null;
        }

        public override int Count() {
            return buttonDIV.Count() == 0 ? 1 : 2;
        }

        public override void Set(params String[] values) {
            dataList.RemoveAllObjects();
            MODataObject modContext = GetDataContext();
            if(ArrayUtils.IsNotEmpty(values)) {
                foreach(String oid in values) {
                    MODataObject mod = modContext.GetComponent(oid);
                    if(mod != null) {
                        dataList.AddObject(mod);
                    } else {
                        logger.Error("no MODataObject found with: " + oid);
                    }
                }
            }
            CloseEditPanel();
        }

        public override String[] Get() {
            List<MODataObject> l = dataList.GetObjects();
            String[] res = new String[l.Count];
            for(int i = 0; i < l.Count; i++) {
                res[i] = l[i].GetOid().ToString();
            }
            return res;
        }

        public override void Clear() {
            dataList.RemoveAllObjects();
        }

        public void Arrived(HEvent he) {
            if(he.GetSource() == newBt) {
                MODataObject mod = MODataObject.Create(compMoid);
                mod.CheckOut();
                compEditPanel.SetData(mod);
                OpenEditPanel();
            }
            if(he.GetSource() == doneBt) {
                Update();
                CloseEditPanel();
            }
            if(he.GetSource() == cancelBt) {
                CloseEditPanel();
            }
        }

        public void OpenEditPanel() {
            buttonDIV.Set(doneBt, cancelBt);
            compEditDIV.Set(compEditPanel, buttonDIV);
            isEditPanelOpen = true;
        }

        public void CloseEditPanel() {
            compEditPanel.ClearData();
            if(isMultiple || dataList.Count() == 0) {
                buttonDIV.Set(newBt);
            } else {
                buttonDIV.RemoveAll();
            }
            compEditDIV.Set(buttonDIV);
            isEditPanelOpen = false;
        }

        public bool IsEditPanelOpen() {
            return isEditPanelOpen;
        }

        public void Arrived(ObjectCommandEvent<MODataObject> he) {
            if(he.getCommand() == DELETE) {
                dataList.RemoveObject(he.getObject());
                CloseEditPanel();
            }
            if(he.getCommand() == EDIT) {
                compEditPanel.SetData(he.getObject());
                OpenEditPanel();
            }
        }

        public IList<MODataObject> GetMoDataObjects() {
            return dataList.GetObjects();
        }

        public void Update() {
            if(IsEditPanelOpen()) {
                MODataObject modContext = GetDataContext();
                MODataObject mod = compEditPanel.GetData();
                modContext.AddComponent(mod);
                if(isMultiple) {
                    dataList.AddObject(mod);
                } else {
                    dataList.RemoveAllObjects();
                    dataList.AddObject(mod);
                }
            }
        }
    }

    public class UIDataObjectCellsRenderer : ICellsRenderer<MODataObject> {

        private int maxDataCell = 5;
        private TD[] tds;
        private UIFrame uiFrame;

        public UIDataObjectCellsRenderer(UIFrame uiFrame) {
            this.uiFrame = uiFrame;
            tds = new TD[maxDataCell];
            for(int i = 0; i < tds.Length; i++) {
                tds[i] = new TD();
            }
        }

        public int ColumnCount() {
            return maxDataCell;
        }

        //public TD[] GetCells(MODataObject obj) {
        //    UIViewSynopsis views = uiFrame.GetSynopsisViews(obj.GetMoid());
        //    views.SetData(obj);
        //    views.Fill(tds);
        //    return tds;
        //}

        public TD[] GetCells(MODataObject mod) {
            UIViewSynopsis views = null;
            views = uiFrame.GetSynopsisViews(mod.GetMoid());
            //views.SetData(obj);
            //views.Fill(tds);
            views.RenderTo(mod, tds);
            return tds;
        }
    }



    public class UIAttributeViewComp : UIAttributeViewBase {

        private static readonly Logger logger = Logger.GetLogger(typeof(UIAttributeViewComp));

        public static UIAttributeViewComp Create(IUIViewPanel viewPanel, MOView moControl) {
            return new UIAttributeViewComp(viewPanel, moControl);
        }

        //

        private DIV missingObjectDIV;
        private List<long> oids;
        private String lookupMoid;
        private List<UIViewPanel> compViewPanels = new List<UIViewPanel>();
        private HContainer container;
        private DIV numberDIV;
        private bool numbering;
        private UIViewPanel localViewPanel;

        private UIAttributeViewComp(IUIViewPanel viewPanel, MOView moControl)
            : base(viewPanel, moControl) {
            oids = new List<long>();
            //
            MOTypeComp moType = (MOTypeComp)moControl.GetMOAttribute().GetMOType().GetBaseType();
            lookupMoid = moType.GetLookupMoid();
            missingObjectDIV = new DIV();
            container = new HContainer();
            numberDIV = new DIV("", UIStyles.COMP_NUMBER);
            numbering = moControl.HasUIHint(MO.AttValue.numbering);
        }

        public override void Set(params String[] values) {
            Clear();
            if(ArrayUtils.IsNotEmpty(values)) {
                foreach(String s in values) {
                    long oid;
                    if(Int64.TryParse(s, out oid)) {
                        oids.Add(oid);
                    }
                }
            }
        }

        public override void Clear() {
            oids.Clear();
        }

        public override HComponent Get(int index) {
            if(numbering) {
                numberDIV.SetText((1 + index).ToString());
                container.Set(numberDIV);
            } else {
                container.RemoveAll();
            }

            MODataObject mod = MODataObject.GetById(oids[index]);
            if(mod != null) {
                UIViewPanel uiViewPanel = GetCompViewPanel(index);
                uiViewPanel.SetData(mod);
                container.Add(uiViewPanel);
            } else {
                missingObjectDIV.SetText("Data object " + oids[index] + " is not available");
                container.Add(missingObjectDIV);
            }
            return container;
        }

        private UIViewPanel GetCompViewPanel(int index) {
            if(index >= compViewPanels.Count) {
                compViewPanels.Add(new UIViewPanel(uiFrame, lookupMoid));
            }
            return compViewPanels[index];
        }

        private UIViewPanel GetViewPanel() {
            return localViewPanel == null ? localViewPanel = new UIViewPanel(uiFrame, lookupMoid) : localViewPanel;
        }

        public override int Count() {
            return oids.Count;
        }

        public override IEnumerable<String> RenderToString() {
            String[] sb = new String[oids.Count];
            for(int i = 0; i < oids.Count; i++) {
                //if(i > 0) {
                //    sb.Append(", ");
                //}
                MODataObject mod = MODataObject.GetById(oids[i]);
                sb[i] = mod.ToSynopsisString();
            }
            return sb;
        }


    }

    public class UIAttributeControlForeach : UIAttributeControlBase, IHListener, IObjectCommandListener<MODataObject> {

        private static readonly Logger logger = Logger.GetLogger(typeof(UIAttributeControlComp));

        public static UIAttributeControlForeach Create(IUIViewPanel viewPanel, MOControl moControl) {
            return new UIAttributeControlForeach(viewPanel, moControl);
        }

        // MO
        private MOAttribute moAttribute;
        private String compMoid;
        private String attref;
        private String attrefBack;
        // LIST DATA
        private List<String> compOids = new List<String>();
        private String[] targetOids;
        private HObjectCommandList<MODataObject> targetDataList;
        private DIV targetDataDIV;
        // EDIT COMP
        private UIEditPanel compEditPanel;
        private DIV compEditDIV;
        private HLink doneBt;
        private HLink cancelBt;
        private DIV buttonDIV;
        //

        private UIAttributeControlForeach(IUIViewPanel viewPanel, MOControl moControl)
            : base(viewPanel, moControl) {
            this.moAttribute = moControl.GetMOAttribute();

            MOTypeForeach moTypeForeach = (MOTypeForeach)moAttribute.GetMOType().GetBaseType();
            compMoid = moTypeForeach.GetCompType().GetLookupMoid();
            attref = moTypeForeach.GetAttref();
            attrefBack = moTypeForeach.GetCompType().GetAttrefBack();
            String compClassName = MOService.GetInstance().GetMOClass(compMoid).GetName();
            String[] commands = new String[] { compClassName };
            //
            targetDataList = new HObjectCommandList<MODataObject>(uiFrame, new UIDataObjectCellsRenderer(uiFrame), commands);
            targetDataDIV = new DIV(targetDataList);
            //
            compEditPanel = new UIEditPanel(uiFrame, compMoid);
            doneBt = new HLink(uiFrame, "Done");
            cancelBt = new HLink(uiFrame, "Cancel");
            buttonDIV = new DIV(HStyles.BUTTON_PANEL, doneBt, cancelBt);
            compEditDIV = new DIV();
            //
            doneBt.AddHListener(this);
            cancelBt.AddHListener(this);
            targetDataList.AddObjectCommandListener(this);
            //
        }

        public override HComponent Get(int index) {
            switch(index) {
                case 0: return this.targetDataDIV;
                case 1: return compEditDIV;
            }
            return null;
        }

        public override int Count() {
            return !IsEditPanelOpen() ? 1 : 2;
        }

        public override void Set(params String[] values) {
            UpdateTargets();
            SetCompOids(values);
            CloseEditPanel();
        }

        private void UpdateTargets() {
            targetDataList.RemoveAllObjects();
            MODataObject context = GetDataContext();
            targetOids = context.GetCurrentValues(attref);
            List<MODataObject> foreachMods = MODataObject.GetByIds(targetOids);
            targetDataList.SetObjects(foreachMods);
        }

        private void SetCompOids(IList<String> compOids) {
            this.compOids.Clear();
            if(compOids != null) {
                this.compOids.AddRange(compOids);
            }
        }

        public override String[] Get() {
            UpdateTargets();
            List<String> newCompOids = new List<String>();
            foreach(String compOid in compOids) {
                MODataObject compMod = GetDataContext().GetComponent(compOid);
                if(compMod != null) {
                    String referred = compMod.GetCurrentValue(attrefBack);
                    if(ArrayUtils.Contains(targetOids, referred)) {
                        newCompOids.AddUnique(compOid);
                    }
                }
            }
            SetCompOids(newCompOids);
            return newCompOids.ToArray();
        }

        public override void Clear() {
            targetDataList.RemoveAllObjects();
            compOids.Clear();
        }

        public void Arrived(HEvent he) {
            if(he.GetSource() == doneBt) {
                Update();
                CloseEditPanel();
            }
            if(he.GetSource() == cancelBt) {
                CloseEditPanel();
            }
        }

        private void CloseEditPanel() {
            compEditPanel.ClearData();
            compEditDIV.RemoveAll();
        }

        public void OpenEditPanel() {
            compEditDIV.Set(compEditPanel, buttonDIV);
        }

        private bool IsEditPanelOpen() {
            return compEditDIV.Count() > 0;
        }

        public void Arrived(ObjectCommandEvent<MODataObject> he) {
            CloseEditPanel();
            MODataObject targetMod = he.getObject();
            MODataObject compMod = GetOrCreateComp(targetMod);
            compEditPanel.SetData(compMod);
            OpenEditPanel();
        }

        private MODataObject GetOrCreateComp(MODataObject targetMod) {
            MODataObject context = GetDataContext();
            MODataObject compMod = null;
            String targetOid = targetMod.GetOid().ToString();
            foreach(String compOid in this.compOids) {
                compMod = context.GetComponent(compOid);
                if(compMod == null) {
                    logger.Warn("Comp object not found", compMoid, compOid, "for", targetOid, "in context", context);
                    continue;
                }
                String attrefBackValue = compMod.GetCurrentValue(attrefBack);
                if(targetOid.Equals(attrefBackValue)) {
                    return compMod;
                } else {
                    compMod = null;
                }
            }
            if(compMod == null) {
                compMod = MODataObject.Create(compMoid);
                compMod.CheckOut();
                compMod.Set(attrefBack, targetOid);
            }
            compMod.CheckOut();
            return compMod;
        }


        public void Update() {
            if(IsEditPanelOpen()) {
                MODataObject context = GetDataContext();
                MODataObject compMod = compEditPanel.GetData();
                context.AddComponent(compMod);
                compOids.AddUnique(compMod.GetOid().ToString());
            }
        }

    }

    public class UIAttributeViewForeach : UIAttributeViewBase {

        private static readonly Logger logger = Logger.GetLogger(typeof(UIAttributeViewForeach));

        public static UIAttributeViewForeach Create(IUIViewPanel viewPanel, MOView moControl) {
            return new UIAttributeViewForeach(viewPanel, moControl);
        }

        //

        private String compMoid;

        private MOTypeForeach moTypeForeach;
        private String attrefBack;
        //
        private List<String> compOids = new List<String>();
        //
        //
        private DIV numberDIV;
        private TR targetTR;
        private UIDataObjectCellsRenderer targetRenderer;
        private DIV displayDIV;
        private bool isTargetDisplayed = false;
        private List<UIViewPanel> compViewPanels = new List<UIViewPanel>();
        private HContainer compViewPanelHolder;


        // HListSelectionPanel<MODataObject>

        private UIAttributeViewForeach(IUIViewPanel viewPanel, MOView moControl)
            : base(viewPanel, moControl) {
            //
            moTypeForeach = (MOTypeForeach)moControl.GetMOAttribute().GetMOType().GetBaseType();
            compMoid = moTypeForeach.GetCompType().GetLookupMoid();
            attrefBack = moTypeForeach.GetCompType().GetAttrefBack();
            targetRenderer = new UIDataObjectCellsRenderer(uiFrame);
            compViewPanelHolder = new HContainer();
            displayDIV = new DIV(UIStyles.GROUP2, numberDIV = new DIV(), new TABLE(targetTR = new TR()), compViewPanelHolder);
        }

        public override void Set(params String[] values) {
            Clear();
            if(values != null) {
                compOids.AddRange(values);
            }
        }

        public override void Clear() {
            compOids.Clear();
        }

        public override int Count() {
            return compOids.Count;
        }

        public override HComponent Get(int index) {

            MODataObject compMod = MODataObject.GetById(compOids[index]);
            String targetOid = compMod.GetCurrentValue(attrefBack);

            numberDIV.SetText(index + 1 + "");
            MODataObject targetMod = MODataObject.GetById(targetOid);
            if(isTargetDisplayed) {
                targetTR.Set(targetRenderer.GetCells(targetMod));
            } else {
                targetTR.RemoveAll();
            }
            UIViewPanel uiPanel = GetCompViewPanel(index);
            uiPanel.SetData(compMod);
            compViewPanelHolder.Set(uiPanel);
            return displayDIV;
        }

        public UIViewPanel GetCompViewPanel(int index) {
            if(index >= compViewPanels.Count) {
                compViewPanels.Add(new UIViewPanel(uiFrame, compMoid));
            }
            return compViewPanels[index];
        }

        public override IEnumerable<String> RenderToString() {
            String[] sb = new String[compOids.Count];
            for(int i = 0; i < compOids.Count; i++) {
                MODataObject mod = MODataObject.GetById(compOids[i]);
                sb[i] = mod.ToSynopsisString();
            }
            return sb;
        }


    }

    public class UIAttributeControlText : UIAttributeControlBase {

        internal static UIAttributeControlText Create(IUIViewPanel viewPanel, MOControl moControl) {
            return new UIAttributeControlText(viewPanel, moControl);
        }

        private HTextArea textArea;

        private UIAttributeControlText(IUIViewPanel viewPanel, MOControl moControl)
            : base(viewPanel, moControl) {
            Add(textArea = new HTextArea(uiFrame, 42, 4));
        }

        public override void Set(params String[] values) {
            if(ArrayUtils.IsNotEmpty(values)) {
                textArea.SetText(values[0]);
            } else {
                Clear();
            }
        }

        public override String[] Get() {
            String s = textArea.GetText();
            return ObjectUtils.ToArray(s);
        }

        public override void Clear() {
            textArea.SetText("");
        }
    }

    public class UIAttributeViewControlFactory {

        private static Logger logger = Logger.GetLogger(typeof(UIAttributeViewControlFactory));

        private static Dictionary<String, UIAttributeControlCreator> createControlMethods = new Dictionary<String, UIAttributeControlCreator>();
        private static Dictionary<String, UIAttributeViewCreator> createViewMethods = new Dictionary<String, UIAttributeViewCreator>();

        public static void RegisterUIAttributeControlCreator(String key, UIAttributeControlCreator createMethod) {
            createControlMethods.Put(key, createMethod);
        }

        public static void RegisterUIAttributeViewCreator(String key, UIAttributeViewCreator createMethod) {
            createViewMethods.Put(key, createMethod);
        }

        public class DummyViewControl : UIAttributeViewBase, IUIAttributeControl {

            private String[] values;
            private DIV textDIV;

            public DummyViewControl(IUIViewPanel viewPanel, MOView moView)
                : base(viewPanel, moView) {
                Add(textDIV = new DIV());
            }

            public String[] Get() {
                return values;
            }

            public override void Set(params String[] values) {
                this.values = values;
                textDIV.Set(new DIV("{dummy view}"));
                foreach(String s in ArrayUtils.RNN(values)) {
                    textDIV.Add(new DIV(s));
                }
            }

            public override void Clear() {
                values = null;
                textDIV.RemoveAll();
            }
        }

        static UIAttributeViewControlFactory() {
            //
            createControlMethods.Put(MOType.STRING, UIAttributeControlString.Create);
            createControlMethods.Put(MOType.TEXT, UIAttributeControlText.Create);
            createControlMethods.Put(MOType.INTEGER, UIAttributeControlString.Create);
            createControlMethods.Put(MOType.FLOAT, UIAttributeControlString.Create);
            createControlMethods.Put(MOType.PERCENTAGE, UIAttributeControlString.Create);
            createControlMethods.Put(MOType.BOOLEAN, UIAttributeControlBoolean.Create);
            createControlMethods.Put(MOType.DATE, UIAttributeControlDate.Create);
            createControlMethods.Put(MOType.DATETIME, UIAttributeControlDateTime.Create);
            createControlMethods.Put(MOType.CURRENCY, UIAttributeControlCurrency.Create);
            createControlMethods.Put(MOType.CODETABLE, UIAttributeControlCodeTable.Create);
            createControlMethods.Put(MOType.REF, UIAttributeControlRef.Create);
            createControlMethods.Put(MOType.COMP, UIAttributeControlComp.Create);
            createControlMethods.Put(MOType.FOREACH, UIAttributeControlForeach.Create);

            //
            createViewMethods.Put(MOType.STRING, UIAttributeViewString.Create);
            createViewMethods.Put(MOType.TEXT, UIAttributeViewString.Create);
            createViewMethods.Put(MOType.INTEGER, UIAttributeViewString.Create);
            createViewMethods.Put(MOType.FLOAT, UIAttributeViewString.Create);
            createViewMethods.Put(MOType.PERCENTAGE, UIAttributeViewString.Create);
            createViewMethods.Put(MOType.BOOLEAN, UIAttributeViewBoolean.Create);
            createViewMethods.Put(MOType.DATE, UIAttributeViewDate.Create);
            createViewMethods.Put(MOType.DATETIME, UIAttributeViewDateTime.Create);
            createViewMethods.Put(MOType.CURRENCY, UIAttributeViewCurrency.Create);
            createViewMethods.Put(MOType.CODETABLE, UIAttributeViewCodeTable.Create);
            createViewMethods.Put(MOType.REF, UIAttributeViewRef.Create);
            createViewMethods.Put(MOType.COMP, UIAttributeViewComp.Create);
            createViewMethods.Put(MOType.BACK_REF, UIAttributeViewBackRef.Create);
            createViewMethods.Put(MOType.FOREACH, UIAttributeViewForeach.Create);
        }

        public static IUIAttributeView Create(IUIViewPanel viewPanel, MOView moView) {
            MOAttribute moAttribute = moView.GetMOAttribute();
            String name = moAttribute.GetMOType().GetBaseType().GetTypeId();
            String widgetHint = moView.GetWidgetHint();
            name = widgetHint != null ? widgetHint : name;
            logger.Debug("try to create AttributeControl ", name);
            IUIAttributeView viewControl = null;
            if(moView is MOControl) {
                if(createControlMethods.ContainsKey(name)) {
                    return createControlMethods.Get(name)(viewPanel, (MOControl)moView);
                } else {
                    logger.Error("No control found for " + name);
                    viewControl = new DummyViewControl(viewPanel, moView);
                }
            } else {
                if(createViewMethods.ContainsKey(name)) {
                    return createViewMethods.Get(name)(viewPanel, moView);
                } else {
                    logger.Error("No view found for " + name);
                    viewControl = new DummyViewControl(viewPanel, moView);
                }
            }
            return viewControl;
        }

    }

    public class UIDialogConfirm : DIV {

        private HText confirmTx;
        private DIV dispDIV;
        private DIV yDIV;
        private DIV nDIV;

        public UIDialogConfirm() {
            SetStyleClass(UIStyles.CONFIRMATION_DIALOG);

            TABLE table = new TABLE();

            confirmTx = new HText();
            table.Add(new TR(new TD(2, new DIV(UIStyles.CONFIRMATION_DIALOG_TEXT, confirmTx))));
            table.Add(new TR(new TD(2, dispDIV = new DIV(default(String), UIStyles.CONFIRMATION_DIALOG_CONTEXT))));
            TD yTd;
            TD nTd;
            table.Add(new TR(yTd = new TD(yDIV = new DIV()), nTd = new TD(nDIV = new DIV())));
            yTd.SetAttribute(HDTD.AttName.ALIGN, HDTD.AttValue.LEFT);
            nTd.SetAttribute(HDTD.AttName.ALIGN, HDTD.AttValue.RIGHT);
            nDIV.SetStyleClass(HStyles.HLINK);
            yDIV.SetStyleClass(HStyles.HLINK);
            Add(table);
        }

        public void SetContentAndButtons(String confirmText, HLink yesBt, HLink noBt) {
            confirmTx.SetText(confirmText);
            yDIV.Set(yesBt);
            nDIV.Set(noBt);
        }

        public void SetButtons(HLink yesBt, HLink noBt) {
            yDIV.Set(yesBt);
            nDIV.Set(noBt);
        }

        //public void SetConfirmText(String confirmText) {
        //    confirmTx.SetText(confirmText);
        //}
        public void SetConfirmContent(String confirmText, HComponent content) {
            confirmTx.SetText(confirmText);
            dispDIV.Set(content);
        }
    }


    public class UIWindowEditCommandPanel : HContainer, IHListener {
        //
        private HLink saveBt;
        private HLink saveAndCloseBt;
        private HLink closeBt;
        private HLink deleteBt;
        private HLink deletionConfirmBt;
        private HLink deletionAbortBt;
        private DIV buttonDIV;
        private UIFrame uiFrame;
        //
        private UIDialogConfirm confirmDialog = new UIDialogConfirm();
        //

        private IWindowEditAction windowEditAction;

        //
        public UIWindowEditCommandPanel(UIFrame uiFrame, IWindowEditAction windowEditAction) {
            this.uiFrame = uiFrame;
            this.windowEditAction = windowEditAction;
            //
            saveBt = new HLink(uiFrame, "Save");
            saveAndCloseBt = new HLink(uiFrame, "Save and close");
            closeBt = new HLink(uiFrame, "Close");
            deleteBt = new HLink(uiFrame, "Delete");
            deletionConfirmBt = new HLink(uiFrame, "Yes, delete it");
            deletionAbortBt = new HLink(uiFrame, "No");
            //
            Add(buttonDIV = new DIV(UIStyles.GROUP, saveBt, saveAndCloseBt, closeBt, deleteBt));
            //
            saveBt.AddHListener(this);
            saveAndCloseBt.AddHListener(this);
            closeBt.AddHListener(this);
            deleteBt.AddHListener(this);
            deletionConfirmBt.AddHListener(this);
            deletionAbortBt.AddHListener(this);
            //
            confirmDialog.SetButtons(deletionConfirmBt, deletionAbortBt);
        }

        public void Arrived(HEvent hEvent) {
            if(hEvent.GetSource() == saveBt) {
                windowEditAction.SaveData();
            }
            if(hEvent.GetSource() == saveAndCloseBt) {
                windowEditAction.SaveData();
                windowEditAction.Close();
            }
            if(hEvent.GetSource() == closeBt) {
                windowEditAction.Close();
            }
            if(hEvent.GetSource() == deleteBt) {
                windowEditAction.OpenConfirmDialog();
            }
            if(hEvent.GetSource() == deletionAbortBt) {
                windowEditAction.Show();
            }
            if(hEvent.GetSource() == deletionConfirmBt) {
                windowEditAction.DeleteData();
                uiFrame.ShowHomePanel();
                //editPanel.Close();
            }
        }

        public UIDialogConfirm GetDialog(String confirmText, HComponent content) {
            this.confirmDialog.SetConfirmContent(confirmText, content);
            return confirmDialog;
        }

        public void SetData(MODataObject moDataObject) {
            buttonDIV.RemoveAll();
            if(MOAccess.GetInstance().CanWrite(moDataObject)) {
                buttonDIV.Add(saveBt);
                buttonDIV.Add(saveAndCloseBt);
            }
            if(MOAccess.GetInstance().CanWrite(moDataObject)) {
                buttonDIV.Add(deleteBt);
            }
            buttonDIV.Add(closeBt);
        }
    }

    public class UIWindowViewCommandPanel : HContainer, IHListener {

        //
        private HLink approveBt;
        private HLink approveAndCloseBt;
        private HLink closeBt;
        private HLink homeBt;
        private HLink editBt;
        private DIV buttonDIV;
        //
        private UIFrame uiFrame;
        private UIViewWindow viewWindow;
        //
        private MODataObject moDataObject;

        //
        public UIWindowViewCommandPanel(UIFrame uiFrame, UIViewWindow viewWindow) {
            this.uiFrame = uiFrame;
            this.viewWindow = viewWindow;
            //
            editBt = new HLink(uiFrame, "Edit");
            approveBt = new HLink(uiFrame, "Approve");
            approveAndCloseBt = new HLink(uiFrame, "Approve and close");
            closeBt = new HLink(uiFrame, "Close");
            homeBt = new HLink(uiFrame, "Home");
            //
            Add(buttonDIV = new DIV());
            buttonDIV.SetStyleClass(UIStyles.GROUP);
            //
            editBt.AddHListener(this);
            approveBt.AddHListener(this);
            approveAndCloseBt.AddHListener(this);
            closeBt.AddHListener(this);
            homeBt.AddHListener(this);
        }

        public void Arrived(HEvent hEvent) {
            if(hEvent.GetSource() == approveBt) {
                moDataObject.Approve();
                // moDataObject.Save();
                viewWindow.SetData(moDataObject);
            }
            if(hEvent.GetSource() == closeBt) {
                viewWindow.Close();
            }
            if(hEvent.GetSource() == homeBt) {
                uiFrame.ShowHomePanel();
            }
            if(hEvent.GetSource() == editBt) {
                UIEditWindow editWindow = uiFrame.GetEditWindow(moDataObject.GetMoid());
                editWindow.SetData(moDataObject);
                uiFrame.SetMainPanel(editWindow);
            }
        }

        public void SetData(MODataObject moDataObject) {
            this.moDataObject = moDataObject;
            buttonDIV.RemoveAll();
            if(MOAccess.GetInstance().CanWrite(moDataObject)) {
                buttonDIV.Add(editBt);
                if(MOAccess.GetInstance().CanApprove(moDataObject)) {
                    buttonDIV.Add(approveBt);
                }
            }
            buttonDIV.Add(closeBt);
            buttonDIV.Add(homeBt);
        }

    }

    public interface IWindowEditAction {
        void SaveData();
        void DeleteData();
        void Close();
        void Show();
        void OpenConfirmDialog();
    }

    public interface IWindowViewAction {
        void ApproveData();
        //void DeleteData();
        void Close();
        //void Show();
        //void OpenConfirmDialog();
    }

    public interface IUIStyles {

        String[] GetTopLevelSubTitleStyle(String moid);
        String[] GetTopLevelTitleStyle(String moid);
        String[] GetInfoBarStyle();
        String[] GetFooterStyle();
        DIV GetTypeIcon(String moid);
        String[] GetModalWindowStyle();
    }

    public class UIStyles : IUIStyles {

        private static IUIStyles instance;

        public static void SetInstance(IUIStyles instance) {
            UIStyles.instance = instance;
        }

        public static IUIStyles GetInstance() {
            return instance == null ? instance = new UIStyles() : instance;
        }

        public static String EXCEL_ICON_SRC = "../img/xls.png";

        public static readonly String MAIN_TITLE = "MAIN_TITLE";
        public static readonly String CONFIRMATION_DIALOG = "CONFIRMATION_DIALOG";
        public static readonly String CONFIRMATION_DIALOG_CONTEXT = "CONFIRMATION_DIALOG_CONTEXT";
        public static readonly String CONFIRMATION_DIALOG_TEXT = "CONFIRMATION_DIALOG_TEXT";
        public static readonly String GROUP = "GROUP";
        public static readonly String[] MODAL_WINDOW = { "MODAL_WINDOW" };
        public static readonly String GROUP2 = "GROUP2";
        public static readonly String UI_LABEL = "UI_LABEL";
        public static readonly String UI_CONTROL = "UI_CONTROL";
        public static readonly String UI_CONTROL_INLINE = "UI_CONTROL_INLINE";
        public static readonly String UI_CONTROL_FIRST = "UI_CONTROL_FIRST";
        public static readonly String UI_VALUE = "UI_VALUE";
        public static readonly String PREFIX_LABEL_CONTROL_UI = "PREFIX_LABEL_CONTROL_UI";
        public static readonly String[] SMTITLE = { "SMTITLE" };
        public static readonly String BOLD = "BOLD";
        public static readonly String HIST_LINE = "HIST_LINE";
        public static readonly String HIST_TITLE = "HIST_TITLE";
        public static readonly String MO_MANDATORY_TEXT = "MO_MANDATORY_TEXT";
        public static readonly String MO_NOWRAP = "MO_NOWRAP";
        public static readonly String MO_NOWRAP2 = "MO_NOWRAP2";
        public static readonly String MO_LEFTCELL = "MO_LEFTCELL";
        public static readonly String SYNOPSIS = "SYNOPSIS";
        public static readonly String REPORT_TABLE = "REPORT_TABLE";
        public static readonly String REPORT_CELL_HEADER = "REPORT_CELL_HEADER";
        public static readonly String REPORT_CELL_FILTER = "REPORT_CELL_FILTER";
        public static readonly String REPORT_CELL_0 = "REPORT_CELL_0";
        public static readonly String REPORT_CELL_1 = "REPORT_CELL_1";
        public static readonly String REPORT_VIEWLINK = "REPORT_VIEWLINK";
        public static readonly String SIMPLE_LINK = "SIMPLE_LINK";
        public static readonly String COMP_NUMBER = "COMP_NUMBER";
        public static readonly String PAGING_RESULT_LABEL = "PAGING_RESULT_LABEL";
        public static readonly String NOLINK = "NOLINK";
        public static readonly String VIEW_LINK = "VIEW_LINK";

        //        public static readonly String REPORT_HEADER = "REPORT_HEADER";


        public virtual String[] GetTopLevelSubTitleStyle(String moid) {
            return new String[] { };
        }
        public virtual String[] GetTopLevelTitleStyle(String moid) {
            return new String[] { };
        }

        public virtual DIV GetTypeIcon(String moid) {
            String[] s = moid.Split('.');
            String name = s.Last();
            String iconLetters = null;
            if(name.Length > 2) {
                iconLetters = name.Substring(0, 3);
            } else {
                iconLetters = name;
            }
            return new DIV(iconLetters);
        }

        public virtual String[] GetInfoBarStyle() {
            return SMTITLE;
        }

        public virtual String[] GetFooterStyle() {
            return SMTITLE;
        }

        public virtual String[] GetModalWindowStyle() {
            return MODAL_WINDOW;
        }

    }


    public static class UIConstants {
        public static readonly String N_A = "N/A";
    }

    public class UIDataObjectRefSearchPanel : IHListener, IObjectCommandListener<MODataObject>,
         IMainPanel {


        //
        // CLASS LEVEL
        //
        public static readonly String CANCEL = "Cancel";
        public static readonly String DONE = "Done";
        private static readonly Logger logger = Logger.GetLogger(typeof(UIDataObjectRefSearchPanel));
        private static readonly String SELECT = "Select";
        private static readonly String[] commands = { SELECT };
        public static int PAGE_SIZE = 10;
        //
        // OBJECT LEVEL
        //
        private UIFrame uiFrame;
        private DIV view;
        private HTextField queryTf;
        //
        private HLink searchBt;
        private HLink resetBt;
        private HLink doneBt;
        private HLink cancelBt;
        private IObjectCommandListener<MODataObject> callBackListener;
        //
        //
        private HObjectCommandList<MODataObject> resultList;
        private SimplePagingList<MODataObject> simplePagingList;
        private UIPagingLinks pagingLinks;
        //
        private DIV resDiv;
        private MOSearchCriteria searchCriteria;
        private H1 titleH1;
        private bool multipleSelection;
        private List<MODataObject> selectedMODataObjects;

        public UIDataObjectRefSearchPanel(UIFrame frame) {
            this.uiFrame = frame;
            //
            view = new DIV();
            //view.SetStyleClass(UIStyles.GetInstance().GetModalWindowStyle());
            searchCriteria = new MOSearchCriteria();
            //
            TABLE table = new TABLE(0, 4, 4);
            //
            queryTf = new HTextField(frame, 64);
            //
            searchBt = new HLink(frame, "Search");
            resetBt = new HLink(frame, "Reset");
            doneBt = new HLink(frame, "Done");
            cancelBt = new HLink(frame, "Cancel");
            resultList = new HObjectCommandList<MODataObject>(frame, null, new RefSearchCellsRenderer(this), commands);
            simplePagingList = new SimplePagingList<MODataObject>();
            pagingLinks = new UIPagingLinks(uiFrame, this);
            //
            // LAYOUT
            //
            TD td1 = null;
            TD td2 = null;
            //
            view.Add(titleH1 = new H1("Search"));
            titleH1.SetStyleClass(UIStyles.GetInstance().GetTopLevelTitleStyle(null));
            view.Add(table);
            //
            table.Add(new TR(
                    td1 = new TD(new HLabel("Search Text "),
                    td2 = new TD(queryTf))));
            td1.SetAttribute(HDTD.AttName.ALIGN, HDTD.AttValue.RIGHT);
            td2.SetAttribute(HDTD.AttName.ALIGN, HDTD.AttValue.LEFT);

            table.Add(new TR(td1 = new TD(2, new DIV(UIStyles.GROUP, searchBt, resetBt))));
            td1.SetAttribute(HDTD.AttName.ALIGN, HDTD.AttValue.LEFT);

            view.Add(new DIV(UIStyles.GROUP2, resDiv = new DIV()));
            //
            view.Add(new DIV(UIStyles.GROUP, doneBt, cancelBt));
            //
            //
            // EVENTS
            //
            view.InitSubmitEventSource(frame);
            searchBt.AddHListener(this);
            resetBt.AddHListener(this);
            doneBt.AddHListener(this);
            cancelBt.AddHListener(this);
            resultList.AddObjectCommandListener(this);
            //this.AddSubmitListener(this);
        }

        public void ReInit(String title, bool multipleSelection, IObjectCommandListener<MODataObject> listener, params String[] moids) {
            titleH1.SetText(title);
            this.multipleSelection = multipleSelection;
            searchCriteria.Clear();
            searchCriteria.AddIncludedMoids(moids);
            selectedMODataObjects = new List<MODataObject>();
            this.callBackListener = listener;
            ResetGUI();
        }


        public void DoSearch() {
            logger.Debug(searchCriteria);
            searchCriteria.AddIncludedDataStates(DataState.APPROVED, DataState.STORED);
            List<MODataObject> result = MODataObject.Search(searchCriteria);
            logger.Debug("Select one: " + result.Count());
            simplePagingList.SetObjects(result, searchCriteria.GetStartPagingIndex(), PAGE_SIZE);
            pagingLinks.SetObjects(simplePagingList);
            resultList.SetObjects(simplePagingList.GetPage());
            //resDiv.Set(new H3("Found " + result.Count() + " results"), resultList, pagingLinks);
            resDiv.Set(new H3(IPagingListUtils.GetResultsLabel(simplePagingList)), resultList, pagingLinks);
        }

        private void CreateSearchCriteriaFromGUI(int startIndex) {
            String searchString = this.queryTf.GetText().Trim();
            searchCriteria.FillInSearchString(searchString);
            searchCriteria.SetStartPagingIndex(startIndex);
        }

        private void RefreshSearch() {
            DoSearch();
        }

        public void Arrived(HEvent ae) {
            if(ae.GetSource() == searchBt) {
                CreateSearchCriteriaFromGUI(0);
                DoSearch();
            } else if(ae.GetSource() == pagingLinks) {
                CreateSearchCriteriaFromGUI(pagingLinks.GetNewPagingStartIndex());
                DoSearch();
            } else if(ae.GetSource() == this.resetBt) {
                ResetGUI();
            } else if(ae.GetSource() == doneBt) {
                ResetGUI();
                callBackListener.Arrived(new ObjectCommandEvent<MODataObject>(this, null, DONE));
            } else if(ae.GetSource() == cancelBt) {
                ResetGUI();
                callBackListener.Arrived(new ObjectCommandEvent<MODataObject>(this, null, CANCEL));
            }
        }

        private void ResetGUI() {
            resultList.RemoveAllObjects();
            queryTf.SetText("");
            resDiv.RemoveAll();
        }

        public void Arrived(ObjectCommandEvent<MODataObject> e) {
            if(e.isCommand(SELECT)) {
                selectedMODataObjects.AddUnique(e.getObject());
                if(!multipleSelection) {
                    ResetGUI();
                    callBackListener.Arrived(new ObjectCommandEvent<MODataObject>(this, selectedMODataObjects.First(), DONE));
                    return;
                }
            }
            logger.Debug(e);
        }

        //
        // IMainPanel
        //


        public HComponent GetView() {
            return view;
        }

        public void BeforeShow() {
            RefreshSearch();
        }

        public void BeforeClose() {
            Clear();
        }

        public IList<MODataObject> GetSelected() {
            return selectedMODataObjects;
        }


        class RefSearchCellsRenderer : ICellsRenderer<MODataObject> {

            private int maxDataCell = 3;

            private TD[] tds;
            //private DIV content = new DIV();
            private DIV selectToggleDIV = new DIV();
            private UIDataObjectRefSearchPanel outer;

            internal RefSearchCellsRenderer(UIDataObjectRefSearchPanel outer) {
                this.outer = outer;
                tds = new TD[maxDataCell + 1];
                for(int i = 0; i < tds.Length; i++) {
                    tds[i] = new TD();
                }
                //content.SetStyleClass(HStyles.S100);
                selectToggleDIV.SetStyleClass(HStyles.S100);
            }

            public int ColumnCount() {
                return maxDataCell + 1;
            }

            public TD[] GetCells(MODataObject obj) {
                UIViewSynopsis viewSynopsis = outer.uiFrame.GetSynopsisViews(obj.GetMoid());
                viewSynopsis.RenderTo(obj, tds);
                tds[maxDataCell].Set(selectToggleDIV);
                selectToggleDIV.SetText(outer.selectedMODataObjects.Contains(obj) ? "X" : "-");
                return tds;
            }
        }

        public void Clear() {
            ResetGUI();
            searchCriteria.Clear();
            selectedMODataObjects.Clear();
            titleH1.SetText("Search");
            callBackListener = null;
        }

    }

    public static class UIWorkaround {
        public static void AddNBSPAsFirstElement(HContainer hcontainer) {
            hcontainer.Add(new NBSP());
        }
    }


}