//
// Copyright (C) 2008 Vitra AG, Klünenfeldstrasse 22, Muttenz, 4127 Birsfelden
// All rights reserved.
//
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Web;
using Com.OOIT.VIPS.Admin;
using Com.OOIT.VIPS.System;
using Com.VITRA.ActiveDirectory;
using Org.JGround.Codetable;
using Org.JGround.HWT;
using Org.JGround.HWT.Components;
using Org.JGround.HWT.MOM;
using Org.JGround.HWT.MOM.Docu;
using Org.JGround.MOM;
using Org.JGround.MOM.DB;
using Org.JGround.MOM.Generate;
using Org.JGround.Util;
using Org.JGround.Web;

namespace Com.OOIT.VIPS.HWT {

    public static class VipsAsp {

        private static Logger logger = Logger.GetLogger(typeof(VipsAsp));

        public static void Initialize(HttpServerUtility server) {
            try {
                String s = ResolveStringValue("testMode");
                bool testMode = StringUtils.IsNotEmpty(s) && s.ToLower().Equals("on");
                s = ResolveStringValue("debugMode");
                bool debugMode = StringUtils.IsNotEmpty(s) && s.ToLower().Equals("on");
                //
                // INIT LOGGER
                //
                Logger.LOG_DIR = ResolvePath(server, "vipsLogDir");
                Logger.GLOBAL_LOG_LEVEL = Logger.ToLevel(ConfigurationManager.AppSettings["vipsGlobalLogLevel"]);
                logger.Info("Logger.LOG_DIR", Logger.LOG_DIR);
                //
                // LOG LEVELS IN DEBUG MODE
                //

                

                if(debugMode) {
                    Logger.LOG_LEVEL(Logger.LogLevels.DEBUG, typeof(HFrame));
                    Logger.GLOBAL_LOG_LEVEL = Logger.LogLevels.WARN;
                    Logger.LOG_LEVEL(Logger.LogLevels.WARN, typeof(MOClass));
                    Logger.LOG_LEVEL(Logger.LogLevels.INFO, typeof(HFrame));
                    Logger.LOG_LEVEL(Logger.LogLevels.WARN, typeof(ADRoleProvider));
                    Logger.LOG_LEVEL(Logger.LogLevels.WARN, typeof(VIPSDBBackup));
                    Logger.LOG_LEVEL(Logger.LogLevels.WARN, typeof(VipsDeadlineService));
                    Logger.LOG_LEVEL(Logger.LogLevels.WARN, typeof(VipsMainFrame));
                    Logger.LOG_LEVEL(Logger.LogLevels.WARN, typeof(DBService));
                    Logger.LOG_LEVEL(Logger.LogLevels.DEBUG, typeof(MOAttribute));
                    Logger.LOG_LEVEL(Logger.LogLevels.DEBUG, typeof(UIReportWindow));
                }
                //
                // PERFORMANCE LOGGER (OFF=LogLevel.WARN, ON=LogLevel.INFO)
                //
                Logger.LOG_LEVEL(Logger.LogLevels.INFO, DateTimeUtils.PerformanceLoggerName);
                Logger.LOG_LEVEL(Logger.LogLevels.INFO, "global.asax");
                Logger.LOG_LEVEL(Logger.LogLevels.DEBUG, typeof(DocuServlet));
                //
                // INIT MAILSERVICE
                //
                MailService.WEB_CONFIG_PATH = @"~\web.config";
                string startMessage = "VIPS Server started at " + DateTime.Now;
                MailService.GetInstance().AddMail(
                        new String[] { ConfigurationManager.AppSettings["vipsSysAdminMail"] }, startMessage, startMessage, false);
                MailService.GetInstance().SendAllMails();
                //
                // INIT MOSYSTEM and MOACCESS
                //
                MOSystem.IS_TEST_MODE = testMode;
                MOSystem.GetUserName = VIPSSystem.GetUser;
                MOSystem.GetUserRoles = VIPSSystem.GetRoles;
                MOAccessMain.USE_ROLES_TYPE = MOAccessMain.BUILTIN_ROLES.Equals(ResolveStringValue("vipsRolesType")) ?
                    MOAccessMain.BUILTIN_ROLES : MOAccessMain.IPRINCIPAL_ROLES;
                MOAccessMain.INITIAL_SYSADMIN_USERNAME = ResolveStringValue("vipsSysAdminUserName");
                MOAccess.GetInstance();
                //
                // INIT CODETABLE
                //
                CodeTable.CODETABLE_DIR = ResolvePath(server, "vipsCodeTableDir");
                logger.Info("CodeTable.CODETABLE_DIR: " + CodeTable.CODETABLE_DIR);
                //
                // INIT MOSERVICE
                //
                String moDir = ResolvePath(server, "vipsMoDir");
                MOService.Initialilze(moDir);
                logger.Info("MO Directory [MOService]: " + moDir);
                //
                // INIT DBSERVICE
                //
                String connectionString = ConfigurationManager.ConnectionStrings["vipsConnectionString"].ConnectionString;
                //DBService._Migration_2010_08_01(connectionString);
                //
                // DB Data From File (eg. "E:\tab\_aaa\vitra\2008-2009\vipsdb-2009-08-03.txt")
                //
                String vipsLoadFileToDB = ResolvePath(server, "vipsLoadFileToDB");
                if(StringUtils.IsNotEmpty(vipsLoadFileToDB)) {
                    logger.Info("load DB from file", vipsLoadFileToDB);
                    DBService.LoadFileToDB(vipsLoadFileToDB, connectionString);
                }
                //
                // VIPS DB BACKUP SERVICE
                //
                String vipsDataDir = ResolvePath(server, "vipsDataDir");
                if("on".Equals(ResolveStringValue("vipsFileBackup"))) {
                    VIPSDBBackup.GetInstance().Startup(vipsDataDir, connectionString);
                }
                // REPORT DIR
                String reportStoreDir = Path.Combine(vipsDataDir, "reports");
                Directory.CreateDirectory(reportStoreDir);
                DocuServlet.REPORT_STORE_DIR = reportStoreDir;
                //
                //
                //
                if(ResolveStringValue("vipsCheckoutMaxInMinutes") != null) {
                    try {
                        DBService.CHECK_OUT_TIMEOUT_MILLIS = 1000 * 60 * Int32.Parse(ResolveStringValue("vipsCheckoutMaxInMinutes"));
                    }
                    catch(Exception) {
                        logger.Warn("Could not parse vipsCheckoutMaxInMinutes", ResolveStringValue("vipsCheckoutMaxInMinutes"));
                    }
                }
                logger.Info("CHECK_OUT_TIMEOUT_MILLIS [DBService]: " + DBService.CHECK_OUT_TIMEOUT_MILLIS);
                DateTimeUtils.StartTime("DBService.Initialize");
                DBService.Initialize(connectionString);
                DateTimeUtils.LogTime("DBService.Initialize");
                logger.Info("Connection String [DBService]: " + connectionString);
                //
                // INIT DOCUSERVICE
                //
                String vipsFileUploadDir = ResolvePath(server, "vipsFileUploadDir");
                DocuServlet.DOCU_STORE_DIR = vipsFileUploadDir;

                //
                //
                // INIT DEADLINE SERVICE
                //
                VipsDeadlineService.DEADLINE_FILE = ResolvePath(server, "vipsDeadlineFile");
                try {
                    s = ResolveStringValue("vipsDeadlineChecksInMinutes");
                    int deadlineChecks = 0;
                    if(Int32.TryParse(s, out deadlineChecks)) {
                        VipsDeadlineService.TIMER_INTERVAL_IN_SECONDS = deadlineChecks * 60;
                    }
                }
                catch(Exception e) {
                    logger.Error(e, e);
                }
                logger.Info("DEADLINE_FILE             [DeadLine Service]: " + VipsDeadlineService.DEADLINE_FILE);
                logger.Info("TIMER_INTERVAL_IN_SECONDS [DeadLine Service]: " + VipsDeadlineService.TIMER_INTERVAL_IN_SECONDS);
                VipsDeadlineService.GetInstance().Startup();
                //
                // GENERATE DEF CLASS (only in debug mode)
                //
                if(debugMode) {
                    try {
                        String ClassOutputDir = @"e:\";
                        String ClassFile = @"VIPS_DEF.cs";
                        String ClassNameSpace = "Com.OOIT.VIPS";
                        DefGenerator.ProcessSanityCheck();
                        DefGenerator.ProcessDEFClassGeneration(ClassOutputDir, ClassFile, ClassNameSpace);
                    }
                    catch(Exception e) {
                        logger.Warn(e);
                    }
                }
                //
                // INIT UI COMPONENTS
                //
                UIStyles.SetInstance(new VipsStyles());
                //
                // WIDGET REGISTRATION BY WIDGETHINTS
                //
                UIAttributeViewControlFactory.RegisterUIAttributeControlCreator("fileUpload", UIAttributeControlDocu.Create);
                UIAttributeViewControlFactory.RegisterUIAttributeControlCreator("pictureUpload", UIAttributeControlDocu.Create);
                UIAttributeViewControlFactory.RegisterUIAttributeViewCreator("docuLink", UIAttributeViewDocu.Create);
                UIAttributeViewControlFactory.RegisterUIAttributeViewCreator("picture", UIAttributeViewDocu.Create);

                UIAttributeViewControlFactory.RegisterUIAttributeControlCreator("uploadify", UIAttributeControlUploadifyDocu.Create);
                UIAttributeViewControlFactory.RegisterUIAttributeViewCreator("uploadify", UIAttributeViewUploadifyDocu.Create);




                //
                // MISC PARAMETER
                //
                VIPSSearchPanel.PAGE_SIZE = ResolveIntValue("UI-RESULT_PAGE_SIZE", 20);
                VipsMainFrame.TITLE = ResolveStringValue("UI-FRAME_TITLE", "VITRA Intellectual Property System");
                VipsMainFrame.LOGO_SRC = ResolveStringValue("UI-FRAME_LOGO_SRC", null);
                //
                //
                //
                MOServiceProcessor.RegisterService("ACCESS_CG", MOServiceProcessor.ONNEW, VipsCompanyGroupHelper.OnNewCompanyGroupInit);
                VipsCompanyGroupHelper accessPlugin = VipsCompanyGroupHelper.GetInstance();
                // The following is not needed, access is done with the access expressions
                // MOAccess.GetInstance().Add(accessPlugin);
                //
                //
                //
                Migration_2010_09_21();
            }
            catch(Exception e) {
                logger.Error(e);
            }
        }


        private static void Migration_2010_09_21() {
            VIPSSystem.RegisterUserOnThread("MIGAdmin");
            VIPSSystem.RegisterRolesOnThread(new String[] { "IPA", "CG_VITRA-READ", "CG_VITRA-WRITE" });
            MOSearchCriteria sc = new MOSearchCriteria();
            sc.SetIncludedDataStates(DataState.APPROVED, DataState.STORED);
            sc.AddIncludedMoids(DEF.Product.moid, DEF.Contract.moid, DEF.Infringement.moid, DEF.Patent.moid, DEF.Trademark.moid, DEF.Design.moid, DEF.Domain.moid);
            MOSystem.GetUserRoles();
            int counter = 0;
            List<MODataObject> res = MODataObject.Search(sc);
            foreach(MODataObject mod in res) {
                String value = mod.GetCurrentValue(DEF.CompanyGroup.companyGroup);

                if(mod.CheckOut()) {
                    mod.Set(DEF.CompanyGroup.companyGroup, value);
                    mod.Save();
                    mod.CheckIn();
                    counter++;
                }

            }
            int i = res.Count;
            logger.Info("Migration_2010_09_21 added ", counter, " companyGroup attribut values ");
            VIPSSystem.RegisterRolesOnThread(new String[] { });
        }



        private static String ResolvePath(HttpServerUtility server, String key) {
            String value = ConfigurationManager.AppSettings[key];
            if(StringUtils.IsNotEmpty(value)) {
                return value.StartsWith("~") ? server.MapPath(value) : value;
            } else {
                return null;
            }
        }

        private static String ResolveStringValue(String key) {
            String value = null;
            try {
                value = ConfigurationManager.AppSettings[key];
            }
            catch(Exception e) {
                logger.Error(e, e);
            }
            return value;
        }

        private static int ResolveIntValue(String key, int defaultValue) {
            String value = null;
            try {
                value = ConfigurationManager.AppSettings[key];
                return Int32.Parse(value);
            }
            catch(Exception e) {
                logger.Error(e, e);
            }
            return defaultValue;
        }

        private static String ResolveStringValue(String key, String defaultValue) {
            String v = ResolveStringValue(key);
            return v == null ? defaultValue : v;
        }

        public static void Shutdown(HttpServerUtility httpServerUtility) {
            HWTEventDispatcherInitializier.Shutdown();
            VIPSDBBackup.Shutdown();
            DBService.Shutdown();
        }
    }

    /// <summary>
    /// Main HST Frame for the VIPS Application
    /// </summary>
    /// 
    public class VipsMainFrame : UIFrame {
        //
        // CLASS LEVEL
        //

        public static String TITLE = "VITRA Intellectual Property System";
        public static String LOGO_SRC = null;

        private static Logger logger = Logger.GetLogger(typeof(VipsMainFrame));

        private static Dictionary<String, String> vipsRole2ADRole = new Dictionary<string, string>();

        static VipsMainFrame() {

            vipsRole2ADRole.Put("IPA", "APP_VIPS-IPA");
            vipsRole2ADRole.Put("CO", "APP_VIPS-CO");
            vipsRole2ADRole.Put("MA", "APP_VIPS-MA");
            vipsRole2ADRole.Put("PM", "APP_VIPS-PM");
            vipsRole2ADRole.Put("DM", "APP_VIPS-DM");
            vipsRole2ADRole.Put("AW", "APP_VIPS-AW");
            vipsRole2ADRole.Put("DM", "APP_VIPS-DM");
            vipsRole2ADRole.Put("DI", "APP_VIPS-DI");
            vipsRole2ADRole.Put("DI-CH", "APP_VIPS-DI-CH");
            vipsRole2ADRole.Put("DI-DE", "APP_VIPS-DI-DE");
            vipsRole2ADRole.Put("DI-FR", "APP_VIPS-DI-FR");
            vipsRole2ADRole.Put("DI-IT", "APP_VIPS-DI-IT");
            vipsRole2ADRole.Put("DI-ES", "APP_VIPS-DI-ES");
            vipsRole2ADRole.Put("DI-GB", "APP_VIPS-DI-GB");
            //
            vipsRole2ADRole.Put(VipsDeadlineService.DEADLINE_PROCESS_ROLE, "APP_VIPS-DEADLINE_PROCESS_ROLE");
            vipsRole2ADRole.Put(MOAccessMain.SYSADMIN, "APP_VIPS-Sysadmin");
            //
            // Company Group Access Roles
            //
            //APP_VIPS-CG-VITRA-READ
            //APP_VIPS-CG-VITRA-WRITE
            //APP_VIPS-CG-BELUX-READ
            //APP_VIPS-CG-BELUX-WRITE
            //APP_VIPS-CG-ARTEK-READ
            //APP_VIPS-CG-ARTEK-WRITE
            //APP_VIPS-CG-VITRASHOP-READ
            //APP_VIPS-CG-VITRASHOP-WRITE

            vipsRole2ADRole.Put("CG_VITRA" + MOAccess.READ_POSTFIX, "APP_VIPS-CG-VITRA-READ");
            vipsRole2ADRole.Put("CG_VITRA" + MOAccess.WRITE_POSTFIX, "APP_VIPS-CG-VITRA-WRITE");
            //
            vipsRole2ADRole.Put("CG_BELUX" + MOAccess.READ_POSTFIX, "APP_VIPS-CG-BELUX-READ");
            vipsRole2ADRole.Put("CG_BELUX" + MOAccess.WRITE_POSTFIX, "APP_VIPS-CG-BELUX-WRITE");
            //
            vipsRole2ADRole.Put("CG_VITRASHOP" + MOAccess.READ_POSTFIX, "APP_VIPS-CG-VITRASHOP-READ");
            vipsRole2ADRole.Put("CG_VITRASHOP" + MOAccess.WRITE_POSTFIX, "APP_VIPS-CG-VITRASHOP-WRITE");
            //
            vipsRole2ADRole.Put("CG_ARTEK" + MOAccess.READ_POSTFIX, "APP_VIPS-CG-ARTEK-READ");
            vipsRole2ADRole.Put("CG_ARTEK" + MOAccess.WRITE_POSTFIX, "APP_VIPS-CG-ARTEK-WRITE");
            //


        }

        public static String MapVipsToADRole(String vipsRole) {
            //logger.Info("vipsRole2ADRole", vipsRole2ADRole.Count);
            if(vipsRole2ADRole.ContainsKey(vipsRole)) {
                return vipsRole2ADRole.Get(vipsRole);
            }
            return "";
        }

        //
        // OBJECT LEVEL
        //

        private VipsNavigationPanel navigationPanel;

        public VipsMainFrame(HWTEvent hwtEvent)
            : base(hwtEvent) {
            if(ArrayUtils.IsEmpty(MOSystem.GetUserRoles())) {
                SetNavigationPanel(new DIV("NO ACCESS, SORRY!"));
                return;
            }
            //
            // COMPONENTS
            //
            HComponent header = CreateHeader(TITLE, LOGO_SRC);
            //
            navigationPanel = new VipsNavigationPanel(this);
            //
            // LAYOUT
            //
            base.SetHeader(header);
            base.SetNavigationPanel(navigationPanel);
            base.SetHomePanel(navigationPanel.GetSearchPanel());
            // Event handling
        }

        protected override void SetupUserAndRoles(HWTEvent hwtEvent) {
            String userName;
            String[] userRoles = { };
            //
            // TEST MODE ON
            //
            if(MOSystem.IS_TEST_MODE) {
                logger.Info("MOSystem.IS_TEST_MODE = " + MOSystem.IS_TEST_MODE);
                String[] tua = hwtEvent.GetContext().Request.Params.GetValues("testuser");

                userRoles = hwtEvent.GetContext().Request.Params.GetValues("testgroup");
                String[] allVipsRoles = MOService.GetInstance().GetAllRoleNames();
                userRoles = ArrayUtils.Intersection(userRoles, allVipsRoles);
                userName = "nouser";
                hiddenStuff.RemoveAll();
                if(ArrayUtils.IsNotEmpty(tua)) {
                    userName = tua[0];
                    //userName= this.GetHttpServerUtility().UrlDecode(userName);
                }
                userName = userName.ToLower();
                this.hiddenStuff.Add(new INPUT(INPUT.HIDDEN, "testuser", userName));
                if(ArrayUtils.IsEmpty(userRoles)) {
                    userRoles = new String[0];
                }
                logger.Info("Test user  = " + userName);
                logger.Info("Test roles = " + ArrayUtils.Join(userRoles));
                foreach(String role in userRoles) {
                    this.hiddenStuff.Add(new INPUT(INPUT.HIDDEN, "testgroup", role));
                }
            } else {
                //
                // USER NAME FROM IPRINCIPAL
                //
                logger.Info("Using IPrincipal user name");
                IPrincipal principal = hwtEvent.GetUser();
                if(principal != null) {
                    userName = principal.Identity.Name;
                    userName.ToLower();
                    logger.Info("Found IPrincipal", userName);
                    //
                    // ROLES
                    //
                    if(MOAccessMain.USE_ROLES_TYPE.Equals(MOAccessMain.IPRINCIPAL_ROLES)) {
                        logger.Info("Using IPrincipal role data");
                        userRoles = (String[])hwtEvent.GetFromSession(UIFrame.ROLES_SESSIONKEY);
                        if(userRoles == null) {
                            logger.Debug("No roles in session. Try to get them from role manager (IsInRole) for", userName);
                            if(principal != null) {
                                String[] allVipsRoles = MOService.GetInstance().GetAllRoleNames();
                                List<String> userRolesList = new List<string>();
                                foreach(String vipsRole in allVipsRoles) {
                                    try {
                                        String adRole = MapVipsToADRole(vipsRole);
                                        logger.Info("vipsRole", vipsRole);
                                        logger.Info("adRole", adRole);
                                        if(StringUtils.IsNotEmpty(adRole)) {
                                            try {
                                                if(principal.IsInRole(adRole)) {
                                                    userRolesList.Add(vipsRole);
                                                    logger.Info("VipsRole", vipsRole, "mapped to ADRole:", adRole);
                                                }
                                            }
                                            catch(Exception) {
                                                logger.Error("principal.IsInRole", adRole);
                                            }
                                        } else {
                                            logger.Error("No AD role found for", vipsRole);
                                        }
                                    }
                                    catch(Exception roleEx) {
                                        logger.Error("Problems with reading roles for user", principal.Identity.Name, roleEx);
                                    }
                                }
                                userRoles = userRolesList.ToArray();
                                logger.Debug("Found roles from role manager (IsInRole).", userRoles);
                            }
                            logger.Debug("Set roles into session", userRoles, "for", userName);
                            hwtEvent.SetInSession(UIFrame.ROLES_SESSIONKEY, userRoles);
                            logger.Info("IPrincipal : roles for user", userName, ArrayUtils.Join(userRoles));
                        }
                    }
                    if(MOAccessMain.USE_ROLES_TYPE.Equals(MOAccessMain.BUILTIN_ROLES)) {
                        logger.Info("Using BUILT-IN role data");
                        IList<MODataObject> users = MODataObject.GetByValue(DEF.SYSUserRoles.moid, DEF.SYSUserRoles.userName, userName, true);
                        if(ICollectionUtils.IsEmpty(users)) {
                            logger.Warn("No BUILT-IN roles found for user", userName);
                        } else if(users.Count() == 1) {
                            userRoles = users[0].GetCurrentValues(DEF.SYSUserRoles.roles);
                            logger.Info("BUILT-IN roles found in SYSUserRoles", userName, userRoles);
                        } else {
                            logger.Warn("Found more than one SYSUserRoles for user", userName);
                        }
                    }
                } else {
                    logger.Info("No IPrinzipal found");
                    userName = "anonymous";
                }
                if(MOAccessMain.IsInitialSysAdmin(userName)) {
                    userRoles = ArrayUtils.AddUnique(userRoles, MOAccessMain.SYSADMIN);
                }
            }
            VIPSSystem.RegisterUserOnThread(userName);
            VIPSSystem.RegisterRolesOnThread(userRoles);
            base.SetFooter(new SPAN(UIStyles.GetInstance().GetFooterStyle(),
                new HText(MOSystem.GetUserName()), new NBSP(2), new HText(ArrayUtils.Join(MOSystem.GetUserRoles()))));
        }

        private HComponent CreateHeader(String title, String logo_src) {
            TD titleTD;
            TD manualTD;
            
            SPAN titleSPAN = new SPAN(title);
            DIV titleDIV = new DIV(titleSPAN);
            titleDIV.SetCss("margin-left:40px");
            //if(StringUtils.IsNotBlank(logo_src)) {
            //    titleDIV = new DIV(new IMG(logo_src), titleSPAN);
            //} else {
            //    titleDIV = new DIV(titleSPAN);
            //}
            DIV manualDIV;
            TABLE table = new TABLE(new TR(
                titleTD = new TD(titleDIV),
                manualTD = new TD(manualDIV = new DIV(new A("User Manual", "../vips-user-manual.pdf", "manual")))));
            table.SetStyleClass(UIStyles.MAIN_TITLE);
            table.SetAttribute(HDTD.AttName.BORDER, 0);
            titleTD.SetAttribute(HDTD.AttName.NOWRAP, HDTD.AttValue.TRUE);
            titleTD.SetAttribute(HDTD.AttName.VALIGN, HDTD.AttValue.BOTTOM);
            titleDIV.SetStyleClass(UIStyles.MAIN_TITLE);
            titleSPAN.SetStyleClass(UIStyles.MAIN_TITLE);
            manualTD.SetAttribute(HDTD.AttName.NOWRAP, HDTD.AttValue.TRUE);
            manualTD.SetAttribute(HDTD.AttName.VALIGN, HDTD.AttValue.BOTTOM);
            manualTD.SetAttribute(HDTD.AttName.ALIGN, HDTD.AttValue.RIGHT);
            manualTD.SetAttribute(HDTD.AttName.WIDTH, "90%");
            manualDIV.SetAttribute(HDTD.AttName.STYLE, "font-size:0.7em; font-style:italic; text-align:right; padding-left:20px; padding-right:2px");
            return table;
        }

    }

    public class VipsNavigationPanel : DIV, IHListener {
        //
        // CLASS LEVEL
        //
        private static Logger logger = Logger.GetLogger(typeof(VipsMainFrame));

        //
        // OBJECT LEVEL
        //	private HLink searchBt;
        //
        private DIV newMenuGroupsDIV;
        private HLink searchBt;
        private HLink reportBt;
        private HLink newBt;
        private DIV newDIV;
        private HContainer newContainer;
        private VIPSSearchPanel searchPanel;
        private UIReportOverviewPanel reportOverviewPanel;
        //
        private List<HLink> newMenuLinks = new List<HLink>();
        private UIFrame uiFrame;

        public VipsNavigationPanel(UIFrame uiFrame) {
            this.uiFrame = uiFrame;
            SetStyleClass(VipsStyles.GROUP2);
            //
            // 
            //
            //
            searchPanel = new VIPSSearchPanel(uiFrame);
            reportOverviewPanel = new UIReportOverviewPanel(uiFrame);
            //
            //toBeApprovedBt = new HLink(uiFrame, "To be approved");
            //toBeApprovedBt.SetStyleClass(VipsStyles.HLINK_MENU);
            //
            // COMPONENTS & LAYOUT
            //
            //
            // Menu Groups
            //
            DIV searchDIV = new DIV();
            searchDIV.SetStyleClass(VipsStyles.B);
            searchBt = new HLink(uiFrame, "Home (Search)");
            searchBt.SetStyleClass(VipsStyles.B);
            searchDIV.Add(new DIV(searchBt, VipsStyles.B2));
            //
            DIV reportDIV = new DIV();
            reportDIV.SetStyleClass(VipsStyles.B);
            reportBt = new HLink(uiFrame, "Reports");
            reportBt.SetStyleClass(VipsStyles.B);
            reportDIV.Add(new DIV(reportBt, VipsStyles.B2));
            //
            newDIV = new DIV();
            newDIV.SetStyleClass(VipsStyles.B);
            newBt = new HLink(uiFrame, "New ...");
            newBt.SetStyleClass(VipsStyles.B);
            newDIV.Add(new DIV(newBt, VipsStyles.B2));
            //
            newMenuGroupsDIV = new DIV();
            //
            DIV div = null;
            //
            InitializeUIFrameWindows();
            //
            div = CreateMenuGroup(uiFrame, VipsHelper.FilterWritableTopLevelMoids(VipsHelper.ProductGroup), newMenuLinks, VipsStyles.O, VipsStyles.O2);
            if(div.Count() > 0) {
                newMenuGroupsDIV.Add(div);
            }
            div = CreateMenuGroup(uiFrame, VipsHelper.FilterWritableTopLevelMoids(VipsHelper.RegisteredRightsGroup), newMenuLinks, VipsStyles.G, VipsStyles.G2);
            if(div.Count() > 0) {
                newMenuGroupsDIV.Add(div);
            }
            div = CreateMenuGroup(uiFrame, VipsHelper.FilterWritableTopLevelMoids(VipsHelper.LegalGroup), newMenuLinks, VipsStyles.R, VipsStyles.R2);
            if(div.Count() > 0) {
                newMenuGroupsDIV.Add(div);
            }
            div = CreateMenuGroup(uiFrame, VipsHelper.FilterWritableTopLevelMoids(VipsHelper.SysAdminGroup), newMenuLinks, VipsStyles.S, VipsStyles.S2);
            if(div.Count() > 0) {
                newMenuGroupsDIV.Add(div);
            }
            newContainer = new HContainer();
            SetNewContainerToButton();
            this.Add(searchDIV);
            this.Add(reportDIV);
            this.Add(newContainer);
            //
            // EVENTS
            //
            searchBt.AddHListener(this);
            reportBt.AddHListener(this);
            newBt.AddHListener(this);
            foreach(HLink link in newMenuLinks) {
                link.AddHListener(this);
            }
            //toBeApprovedBt.AddHListener(this);
        }

        private void InitializeUIFrameWindows() {
            foreach(MOClass mc in MOService.GetInstance().GetAllMOClasses()) {
                if(mc.IsAbstract()) {
                    continue;
                }
                if(MOAccess.GetInstance().CanRead(mc)) {
                    uiFrame.GetViewWindow(mc.GetMoid());
                    uiFrame.GetSynopsisViews(mc.GetMoid());
                }
                if(MOAccess.GetInstance().CanWrite(mc)) {
                    uiFrame.GetEditWindow(mc.GetMoid());
                }
            }
        }

        public static DIV CreateMenuGroup(UIFrame uiFrame, IEnumerable<String> moids, List<HLink> newMenuLinks, String style, String style2) {
            DIV groupDIV = new DIV();
            groupDIV.SetStyleClass(style);
            foreach(String moid in moids) {
                MOClass moClass = MOService.GetInstance().GetMOClass(moid);
                HLink menuBt = CreateMenuButton(uiFrame, moClass, style);
                newMenuLinks.Add(menuBt);
                groupDIV.Add(new DIV(menuBt, style2));
            }
            return groupDIV;
        }

        public static HLink CreateMenuButton(UIFrame uiFrame, MOClass moClass, String style) {
            HLink link = new HLink(uiFrame);
            link.SetText("new " + moClass.GetName());
            link.SetSubId(moClass.GetMoid());
            link.SetStyleClass(style);
            return link;
        }

        public void Arrived(HEvent hevent) {
            if(hevent.GetSource() == searchBt) {
                logger.Debug("Search Button");
                SetNewContainerToButton();
                uiFrame.SetMainPanel(searchPanel);
            } else if(hevent.GetSource() == reportBt) {
                logger.Debug("Report Button");
                SetNewContainerToButton();
                uiFrame.SetMainPanel(reportOverviewPanel);
            } else if(hevent.GetSource() == newBt) {
                logger.Debug("New Button");
                ToggleNewContainer();
            } else if(hevent.GetSource() is HLink) {
                HLink link = (HLink)hevent.GetSource();
                SetNewContainerToButton();
                if(newMenuLinks.Contains(link)) {
                    String moid = link.GetSubId();
                    logger.Debug("MO Link ", moid);
                    UIEditWindow editPanel = uiFrame.GetEditWindow(moid);
                    editPanel.GoToPage(0);
                    editPanel.SetData(MODataObject.Create(moid));
                    uiFrame.SetMainPanel(editPanel);
                }
            }
        }


        private void SetNewContainerToNewObjects() {
            if(newMenuGroupsDIV.Count() > 0) {
                newContainer.Set(newMenuGroupsDIV);
            } else {
                newContainer.RemoveAll();
            }
        }

        private void SetNewContainerToButton() {
            if(newMenuGroupsDIV.Count() > 0) {
                newContainer.Set(newDIV);
            } else {
                newContainer.RemoveAll();
            }
        }

        private void ToggleNewContainer() {
            if(newContainer.Get(0) == newDIV) {
                SetNewContainerToNewObjects();
            } else {
                SetNewContainerToButton();
            }
        }


        public VIPSSearchPanel GetSearchPanel() {
            return this.searchPanel;
        }



    }


    public static class VipsHelper {

        public static readonly String[] ProductGroup = { DEF.Designer.moid, DEF.Product.moid };
        public static readonly String[] RegisteredRightsGroup = { DEF.Trademark.moid, DEF.Design.moid, DEF.Patent.moid, DEF.Domain.moid };
        public static readonly String[] LegalGroup = { DEF.Contract.moid, DEF.Infringement.moid };
        public static readonly String[] SysAdminGroup = { //DEF.SYSUserRoles.moid,
                                                             DEF.SYSAlertLog.moid , DEF.CompanyDocument.moid};

        public static String GetIconLetter(String moid) {
            return iconLetter.Get(moid);
        }

        public static readonly Dictionary<String, String> iconLetter = new Dictionary<string, string>();

        static VipsHelper() {
            iconLetter.Put(DEF.Product.moid, "P");
            iconLetter.Put(DEF.Designer.moid, "D");
            iconLetter.Put(DEF.Trademark.moid, "T");
            iconLetter.Put(DEF.Design.moid, "D");
            iconLetter.Put(DEF.Patent.moid, "P");
            iconLetter.Put(DEF.Domain.moid, "D");
            iconLetter.Put(DEF.Contract.moid, "C");
            iconLetter.Put(DEF.Infringement.moid, "I");
        }

        public static IEnumerable<String> FilterWritableTopLevelMoids(IEnumerable<String> moids) {
            String[] userRoles = MOSystem.GetUserRoles();
            foreach(String moid in moids) {
                MOClass moClass = MOService.GetInstance().GetMOClass(moid);
                if(moClass == null) {
                    continue;
                }
                if(moClass.IsTopLevel() && moClass.GetRoles().CanWrite(userRoles)) {
                    yield return moid;
                }
            }
        }

        public static IEnumerable<String> FilterReadableTopLevelMoids(IEnumerable<String> moids) {
            String[] userRoles = MOSystem.GetUserRoles();
            foreach(String moid in moids) {
                MOClass moClass = MOService.GetInstance().GetMOClass(moid);
                if(moClass == null) {
                    continue;
                }
                if(moClass.IsTopLevel() && moClass.GetRoles().CanRead(userRoles)) {
                    yield return moid;
                }
            }
        }

        public static bool CanReadMoid(String moid) {
            String[] userRoles = MOSystem.GetUserRoles();
            MOClass moClass = MOService.GetInstance().GetMOClass(moid);
            return moClass.GetRoles().CanRead(userRoles);
        }

        public static bool CanWriteMoid(String moid) {
            String[] userRoles = MOSystem.GetUserRoles();
            MOClass moClass = MOService.GetInstance().GetMOClass(moid);
            return moClass.GetRoles().CanWrite(userRoles);
        }


    }

    public class VipsStyles : UIStyles {

        public new static readonly string[] SMTITLE = { "VIPS_SMTITLE" };
        public new static readonly string GROUP2 = "VIPS_GROUP2";
        public static readonly string S = "VIPS_S";
        public static readonly string S2 = "VIPS_S2";
        public static readonly string A = "VIPS_A";
        public static readonly string A2 = "VIPS_A2";
        public static readonly string B = "VIPS_B";
        public static readonly string B2 = "VIPS_B2";
        public static readonly string O = "VIPS_O";
        public static readonly string O2 = "VIPS_O2";
        public static readonly string G = "VIPS_G";
        public static readonly string G2 = "VIPS_G2";
        public static readonly string R = "VIPS_R";
        public static readonly string R2 = "VIPS_R2";
        public static readonly string BLIND = "VIPS_BLIND";
        public static readonly string[] FOOTER = { "VIPS_FOOTER" };

        private Dictionary<String, DIV> typeIcons = new Dictionary<string, DIV>();
        private DIV defaultTypeIcon = new DIV();
        public VipsStyles() {
            InsertTypeIcon(DEF.Product.moid, "Prod");
            InsertTypeIcon(DEF.Designer.moid, "Dsg");
            InsertTypeIcon(DEF.Trademark.moid, "Tm");
            InsertTypeIcon(DEF.Design.moid, "Dg");
            InsertTypeIcon(DEF.Patent.moid, "Pat");
            InsertTypeIcon(DEF.Domain.moid, "Dom");
            InsertTypeIcon(DEF.Contract.moid, "Con");
            InsertTypeIcon(DEF.Infringement.moid, "Inf");
            InsertTypeIcon(DEF.Address.moid, "Adr");
            InsertTypeIcon(DEF.Document.moid, "Doc");
            InsertTypeIcon(DEF.JournalEntry.moid, "JE");
            InsertTypeIcon(DEF.Picture.moid, "Pic");
        }

        private void InsertTypeIcon(String moid, String letters) {
            DIV div = new DIV(letters);
            div.SetStyleClass(GetTopLevelSubTitleStyle(moid));
            typeIcons.Put(moid, div);
        }

        public override String[] GetTopLevelTitleStyle(String moid) {
            if(ArrayUtils.Contains(VipsHelper.SysAdminGroup, moid)) {
                return ObjectUtils.ToArray(S);
            }
            if(ArrayUtils.Contains(VipsHelper.ProductGroup, moid)) {
                return ObjectUtils.ToArray(O);
            }
            if(ArrayUtils.Contains(VipsHelper.RegisteredRightsGroup, moid)) {
                return ObjectUtils.ToArray(G);
            }
            if(ArrayUtils.Contains(VipsHelper.LegalGroup, moid)) {
                return ObjectUtils.ToArray(R);
            }
            return ObjectUtils.ToArray(B);
        }

        public override DIV GetTypeIcon(String moid) {
            DIV hc = typeIcons.Get(moid);
            if(hc == null) {
                hc = defaultTypeIcon;
            }
            return hc;
        }

        public override String[] GetTopLevelSubTitleStyle(String moid) {
            return GetTopLevelTitleStyle(moid);
        }

        public override String[] GetInfoBarStyle() {
            return SMTITLE;
        }

        public override string[] GetFooterStyle() {
            return FOOTER;
        }

    }


    ///
    //
    public class VIPSSearchPanel : DIV, IHListener,
         IMainPanel {
        //
        // CLASS LEVEL
        //
        private static readonly Logger logger = Logger.GetLogger(typeof(VIPSSearchPanel));
        public static int PAGE_SIZE = 20;
        //
        // OBJECT LEVEL
        //
        private UIFrame uiFrame;
        private H1 titleH1;
        private HTextField queryTf;
        //private HList<ICodeTableElement> codeTableHList;
        //
        private List<HCheckBox> moClassIncludeCBs;
        private HCheckBox includeDeletedCB;
        private HCheckBox restrictToBeApprovedCB;
        private HCheckBox includeAllCB;
        //
        private HLink searchBt;
        private HLink resetBt;
        //
        //
        private DIV resultTableDIV;
        private MOSearchCriteria currentSC;

        private DIV pagingResultLabel;
        private UIDataSelectionList resultList;
        private SimplePagingList<MODataObject> simplePagingList = new SimplePagingList<MODataObject>();
        private UIPagingLinks pagingLinks;


        public VIPSSearchPanel(UIFrame uiFrame) {
            this.uiFrame = uiFrame;
            titleH1 = new H1("Search");
            titleH1.SetStyleClass(UIStyles.GetInstance().GetTopLevelTitleStyle(null));
            TABLE table = new TABLE(0, 1, 0);
            moClassIncludeCBs = new List<HCheckBox>();
            DIV sysDIV = CreateCheckBoxDIV(uiFrame, VipsHelper.FilterReadableTopLevelMoids(VipsHelper.SysAdminGroup),
                    moClassIncludeCBs, VipsStyles.S);
            DIV prodDIV = CreateCheckBoxDIV(uiFrame, VipsHelper.FilterReadableTopLevelMoids(VipsHelper.ProductGroup),
                    moClassIncludeCBs, VipsStyles.O);
            DIV regrDIV = CreateCheckBoxDIV(uiFrame, VipsHelper.FilterReadableTopLevelMoids(VipsHelper.RegisteredRightsGroup),
                    moClassIncludeCBs, VipsStyles.G);
            DIV leglDIV = CreateCheckBoxDIV(uiFrame, VipsHelper.FilterReadableTopLevelMoids(VipsHelper.LegalGroup),
                    moClassIncludeCBs, VipsStyles.R);
            //
            queryTf = new HTextField(uiFrame, 64);
            //
            //TODO String codeTableName = DEF.CODETABLE.DIVISION;
            // ICodeTable ct = CodeTable.Create(codeTableName, true);
            // codeTableHList = new HList<ICodeTableElement>(uiFrame, ct, false, 1);
            //
            includeDeletedCB = new HCheckBox(uiFrame);
            restrictToBeApprovedCB = new HCheckBox(uiFrame);
            includeAllCB = new HCheckBox(uiFrame);
            //
            searchBt = new HLink(uiFrame, "Search");
            resetBt = new HLink(uiFrame, "Reset");
            //
            pagingResultLabel = new DIV("", UIStyles.PAGING_RESULT_LABEL);
            resultList = new UIDataSelectionList(uiFrame);
            pagingLinks = new UIPagingLinks(uiFrame, this);
            //

            //
            // LAYOUT
            //
            TD td1 = null;
            TD td2 = null;
            //
            Add(titleH1);
            Add(table);
            //
            table.Add(new TR(
                    td1 = new TD(new HLabel("Search Text "),
                    td2 = new TD(queryTf))));
            td1.SetAttribute(HDTD.AttName.ALIGN, HDTD.AttValue.RIGHT);
            td2.SetAttribute(HDTD.AttName.ALIGN, HDTD.AttValue.LEFT);
            //
            // TODO
            //table.Add(new TR(
            //        td1 = new TD(new HLabel("Division Selection "),
            //        td2 = new TD(codeTableHList))));
            td1.SetAttribute(HDTD.AttName.ALIGN, HDTD.AttValue.RIGHT);
            td2.SetAttribute(HDTD.AttName.ALIGN, HDTD.AttValue.LEFT);
            //
            table.Add(new TR(
                    td1 = new TD(new HLabel("Include All"),
                    td2 = new TD(includeAllCB))));
            td1.SetAttribute(HDTD.AttName.ALIGN, HDTD.AttValue.RIGHT);
            td2.SetAttribute(HDTD.AttName.ALIGN, HDTD.AttValue.LEFT);
            //
            //foreach (HCheckBox cb in moClassIncludeCBs) {
            //    table.Add(new TR(
            //             td1 = new TD(new HLabel(((MOClass)cb.GetServerSideObject()).GetName()),
            //             td2 = new TD(cb))));
            //    td1.SetAttribute(HDTD.AttName.ALIGN, HDTD.AttValue.RIGHT);
            //    td2.SetAttribute(HDTD.AttName.ALIGN, HDTD.AttValue.LEFT);
            //}
            if(sysDIV.Count() > 0) {
                table.Add(new TR(
                         td1 = new TD(new NBSP()),
                         td2 = new TD(sysDIV)));
            }
            if(prodDIV.Count() > 0) {
                table.Add(new TR(
                         td1 = new TD(new NBSP()),
                         td2 = new TD(prodDIV)));
            }
            if(regrDIV.Count() > 0) {
                table.Add(new TR(
                         td1 = new TD(new NBSP()),
                         td2 = new TD(regrDIV)));
            }
            if(leglDIV.Count() > 0) {
                table.Add(new TR(
                         td1 = new TD(new NBSP()),
                         td2 = new TD(leglDIV)));
            }
            // INCLUDE DELETED
            if(MOAccess.GetInstance().IsSysadmin()) {
                table.Add(new TR(
                        td1 = new TD(new HLabel("Include Deleted"),
                        td2 = new TD(includeDeletedCB))));
                td1.SetAttribute(HDTD.AttName.ALIGN, HDTD.AttValue.RIGHT);
                td2.SetAttribute(HDTD.AttName.ALIGN, HDTD.AttValue.LEFT);
            }
            //
            table.Add(new TR(
                    td1 = new TD(new HLabel("To Be Approved"),
                    td2 = new TD(restrictToBeApprovedCB))));
            td1.SetAttribute(HDTD.AttName.ALIGN, HDTD.AttValue.RIGHT);
            td2.SetAttribute(HDTD.AttName.ALIGN, HDTD.AttValue.LEFT);


            //table.Add(new TR(td1 = new TD(2, new DIV(UIStyles.GROUP, searchBt, resetBt))));
            //td1.SetAttribute(HDTD.AttName.ALIGN, HDTD.AttValue.LEFT);
            Add(new DIV(UIStyles.GROUP, searchBt, resetBt));


            // Add(new DIV(UIStyles.GROUP2, resDiv = new DIV()));
            Add(resultTableDIV = new DIV());
            //
            uiFrame.SetMainPanel(this);
            //
            // EVENTS
            //
            //InitSubmitEventSource(uiFrame);
            searchBt.AddHListener(this);
            resetBt.AddHListener(this);
            //this.AddSubmitListener(this);
        }

        private static DIV CreateCheckBoxDIV(UIFrame uiFrame, IEnumerable<String> moids, List<HCheckBox> cbList, String style) {
            DIV div = new DIV();
            div.SetStyleClass(style);
            foreach(String moid in moids) {
                MOClass moClass = MOService.GetInstance().GetMOClass(moid);
                if(moClass == null) {
                    continue;
                }
                HCheckBox checkBox = new HCheckBox(uiFrame);
                checkBox.AddServersideObject(moClass);
                cbList.Add(checkBox);
                div.Add(new SPAN(style, new HLabel(moClass.GetName()), checkBox));
            }
            return div;

        }

        public void Arrived(HEvent ae) {
            if(ae.GetSource() == searchBt) {
                DoSearch(CreateSearchCriteriaFromGUI(0));
                uiFrame.SetMainPanel(this);
            } else if(ae.GetSource() == this.resetBt) {
                resultList.RemoveAllObjects();
                //
                queryTf.SetText("");
                //TODO codeTableHList.SetSelected(CodeTable.EMTPY_CODE_TABLE_ELEMENT);
                //
                foreach(HCheckBox cb in moClassIncludeCBs) {
                    cb.SetChecked(false);
                }
                //
                includeAllCB.SetChecked(false);
                includeDeletedCB.SetChecked(false);
                restrictToBeApprovedCB.SetChecked(false);
                resultTableDIV.RemoveAll();
                currentSC = null;
                uiFrame.SetMainPanel(this);
            }
                //else if(ae.getEventType() == HEvent.SUBMIT_EVENT) {
                //    DoSearch(CreateSearchCriteriaFromGUI());
                //    logger.Debug("SUBMIT EVENT EXECUTED : ");
                //} 
            else if(ae.GetSource() == pagingLinks) {
                DoSearch(CreateSearchCriteriaFromGUI(pagingLinks.GetNewPagingStartIndex()));
            }

        }

        public void DoSearch() {
            if(currentSC != null) {
                logger.Debug(currentSC);
                List<MODataObject> result = MODataObject.Search(currentSC);
                logger.Debug("result list ", result.Count());

                simplePagingList.SetObjects(result, currentSC.GetStartPagingIndex(), currentSC.GetPageSize());
                pagingLinks.SetObjects(simplePagingList);
                resultList.SetObjects(simplePagingList.GetPage());
                // resDiv.Set(new H3("Found " + result.Count() + " results"), resultList);
                pagingResultLabel.Set(new HText(IPagingListUtils.GetResultsLabel(simplePagingList)));
                resultTableDIV.Set(pagingResultLabel, resultList, pagingLinks);

            } else {
                logger.Warn("no search critera object!");
                resultTableDIV.Set(new H3("No search criteria!"));
            }
        }

        public void DoSearch(MOSearchCriteria searchCriteria) {
            SetSearchCriteria(searchCriteria);
            DoSearch();
        }



        private MOSearchCriteria CreateSearchCriteriaFromGUI(int startIndex) {
            MOSearchCriteria sc = new MOSearchCriteria();
            sc.SetStartPagingIndex(startIndex);
            sc.SetPageSize(PAGE_SIZE);
            sc.SetTitle("Search");
            //
            String searchString = this.queryTf.GetText().Trim();
            sc.FillInSearchString(searchString);
            logger.Debug("query: " + queryTf.GetText());
            // DATA STATES
            sc.AddIncludedDataStates(DataState.APPROVED, DataState.STORED);
            //
            if(includeDeletedCB.IsChecked()) {
                sc.AddIncludedDataStates(DataState.DELETED, DataState.DELETED_UNAPPROVED);
            }
            if(restrictToBeApprovedCB.IsChecked()) {
                sc.SetIncludedDataStates(DataState.DELETED_UNAPPROVED, DataState.STORED);
                includeDeletedCB.SetChecked(false);
            }
            // DIVISION
            // TODO sc.RequestExactMatch(MOService.GetInstance().GetMOAttribute(DEF.Division.moid, DEF.Division.division), codeTableHList.GetSelected()[0].GetCode());
            // 
            foreach(HCheckBox cb in this.moClassIncludeCBs) {
                if(cb.IsChecked()) {
                    sc.AddIncludedMoids(((MOClass)cb.GetServerSideObject()).GetMoid());
                }
            }
            if(includeAllCB.IsChecked()) {
                foreach(MOClass moClass in MOService.GetInstance().GetAllMOClasses()) {
                    if(moClass.IsTopLevel() && MOAccess.GetInstance().CanRead(moClass)) {
                        sc.AddIncludedMoids(moClass.GetMoid());
                    }
                }
            }
            return sc;
        }

        private void RefreshSearch() {
            if(currentSC != null) {
                DoSearch();
            }
        }

        //
        // IMainPanel
        //

        public void BeforeClose() {

        }

        public void BeforeShow() {
            uiFrame.SetFocusElement(this.queryTf);
            RefreshSearch();
        }

        public HComponent GetView() {
            return this;
        }

        public override void PrintTo(TextWriter pr) {

            base.PrintTo(pr);
        }


        public void SetSearchCriteria(MOSearchCriteria searchCriteria) {
            this.titleH1.SetText(searchCriteria.GetTitle());
            this.currentSC = searchCriteria;

            foreach(HCheckBox cb in this.moClassIncludeCBs) {
                if(currentSC.IsMoidIncluded(((MOClass)cb.GetServerSideObject()).GetMoid())) {
                    cb.SetChecked(true);
                }
            }
            this.queryTf.SetText(searchCriteria.GetSearchString());
        }
    }
}