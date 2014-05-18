//
// Copyright (C) 2008 OOIT.com AG, Zürich CH
// All rights reserved.
//
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using Org.JGround.MOM.DB;
using Org.JGround.Util;
using Org.JGround.Codetable;

namespace Org.JGround.MOM {

    public delegate String GetUserName_Method();
    public delegate String[] GetUserRoles_Method();

    public class MOSystem {

        public static bool IS_TEST_MODE = false;

        public static GetUserName_Method GetUserName { get; set; }
        public static GetUserRoles_Method GetUserRoles { get; set; }

        public bool CanRead(MOClass moClass) {
            String[] userRoles = GetUserRoles();
            return moClass.GetRoles().CanRead(userRoles);
        }

        public bool CanRead(MOReport moReport) {
            throw new NotImplementedException("Please, use MOAccess ...");
        }

        public bool CanRead(MODataObject moDataObject) {
            if(moDataObject != null) {
                String[] userRoles = GetUserRoles();
                return moDataObject.GetMOClass().GetRoles().CanRead(userRoles);
            }
            return true;
        }

        public bool CanRead(MODataObject moDataObject, String attributeName) {
            MOAttribute moAttribute = null;
            if(moDataObject != null && (moAttribute = moDataObject.GetMOClass().GetMOAttribute(attributeName)) != null) {
                String[] userRoles = GetUserRoles();
                return moAttribute.GetRoles().CanRead(userRoles);
            }
            return true;
        }

        public bool CanRead(MOAttribute moAttribute) {
            if(moAttribute != null) {
                String[] userRoles = GetUserRoles();
                return moAttribute.GetRoles().CanRead(userRoles);
            }
            return true;
        }

        public bool CanApprove(MODataObject moDataObject) {
            return !MOSystem.GetUserName().Equals(moDataObject.GetLastUserName()) && CanWrite(moDataObject);
        }


        public bool CanWrite(MOClass moClass) {
            String[] userRoles = GetUserRoles();
            return moClass.GetRoles().CanWrite(userRoles);
        }

        public bool CanWrite(MODataObject moDataObject) {
            if(moDataObject != null) {
                String[] userRoles = GetUserRoles();
                return moDataObject.GetMOClass().GetRoles().CanWrite(userRoles);
            }
            return true;
        }

        public bool CanWrite(MODataObject moDataObject, String attributeName) {
            MOAttribute moAttribute = null;
            if(moDataObject != null &&
                    (moAttribute = moDataObject.GetMOClass().GetMOAttribute(attributeName)) != null) {
                String[] userRoles = GetUserRoles();
                return moAttribute.GetRoles().CanWrite(userRoles);
            }
            return true;
        }

        public bool CanWrite(MOAttribute moAttribute) {
            if(moAttribute != null) {
                String[] userRoles = GetUserRoles();
                return moAttribute.GetRoles().CanWrite(userRoles);
            }
            return true;
        }

    }

    public interface IMOAccess {
        bool CanRead(params MOClass[] moClasses);
        bool CanRead(String moClass);
        bool CanRead(MOReport moReport);
        bool CanRead(MODataObject moDataObject);
        bool CanRead(MODataObject moDataObject, String attributeName);
        bool CanRead(MOAttribute moAttribute);
        bool CanApprove(MODataObject moDataObject);
        bool CanWrite(MOClass moClass);
        bool CanWrite(MODataObject moDataObject);
        bool CanWrite(MODataObject moDataObject, String attributeName);
        bool CanWrite(MOAttribute moAttribute);
        bool IsSysadmin();
        bool IsEntitled(MOView moView);
    }




    public class MOAccess : IMOAccess {

        public const String READ_POSTFIX = "-READ";
        public const String WRITE_POSTFIX = "-WRITE";


        private static MOAccess instance;

        public static MOAccess GetInstance() {
            return instance == null ? instance = new MOAccess() : instance;
        }

        private List<IMOAccess> moAccesses = new List<IMOAccess>();

        public void Add(IMOAccess imoAccess) {
            moAccesses.Add(imoAccess);
        }

        private MOAccess() {
            moAccesses.Add(new MOAccessMain());
        }

        public bool CanRead(params MOClass[] moClasses) {
            foreach(IMOAccess moAccess in moAccesses) {
                if(moAccess.CanRead(moClasses) == false) {
                    return false;
                }
            }
            return true;
        }

        public bool CanRead(String moClass) {
            foreach(IMOAccess moAccess in moAccesses) {
                if(moAccess.CanRead(moClass) == false) {
                    return false;
                }
            }
            return true;
        }

        public bool CanRead(MOReport moReport) {
            foreach(IMOAccess moAccess in moAccesses) {
                if(moAccess.CanRead(moReport) == false) {
                    return false;
                }
            }
            return true;
        }

        public bool CanRead(MODataObject moDataObject) {
            foreach(IMOAccess moAccess in moAccesses) {
                if(moAccess.CanRead(moDataObject) == false) {
                    return false;
                }
            }
            return true;
        }

        public bool CanRead(MODataObject moDataObject, String attributeName) {
            foreach(IMOAccess moAccess in moAccesses) {
                if(moAccess.CanRead(moDataObject, attributeName) == false) {
                    return false;
                }
            }
            return true;
        }

        public bool CanRead(MOAttribute moAttribute) {
            foreach(IMOAccess moAccess in moAccesses) {
                if(moAccess.CanRead(moAttribute) == false) {
                    return false;
                }
            }
            return true;
        }

        public bool CanApprove(MODataObject moDataObject) {
            foreach(IMOAccess moAccess in moAccesses) {
                if(moAccess.CanApprove(moDataObject) == false) {
                    return false;
                }
            }
            return true;
        }

        public bool CanWrite(MOClass moClass) {
            foreach(IMOAccess moAccess in moAccesses) {
                if(moAccess.CanWrite(moClass) == false) {
                    return false;
                }
            }
            return true;
        }

        public bool CanWrite(MODataObject moDataObject) {
            foreach(IMOAccess moAccess in moAccesses) {
                if(moAccess.CanWrite(moDataObject) == false) {
                    return false;
                }
            }
            return true;
        }

        public bool CanWrite(MODataObject moDataObject, String attributeName) {
            foreach(IMOAccess moAccess in moAccesses) {
                if(moAccess.CanWrite(moDataObject, attributeName) == false) {
                    return false;
                }
            }
            return true;
        }

        public bool CanWrite(MOAttribute moAttribute) {
            foreach(IMOAccess moAccess in moAccesses) {
                if(moAccess.CanWrite(moAttribute) == false) {
                    return false;
                }
            }
            return true;
        }

        public bool IsSysadmin() {
            foreach(IMOAccess moAccess in moAccesses) {
                if(moAccess.IsSysadmin() == false) {
                    return false;
                }
            }
            return true;
        }

        public bool IsEntitled(MOView moView) {
            foreach(IMOAccess moAccess in moAccesses) {
                if(moAccess.IsEntitled(moView) == false) {
                    return false;
                }
            }
            return true;
        }




    }


    public class MOAccessAllTrue : IMOAccess {


        public virtual bool CanRead(params MOClass[] moClasses) {
            return true;
        }

        public virtual bool CanRead(String moClass) {
            return true;
        }

        public virtual bool CanRead(MOReport moReport) {
            return true;
        }

        public virtual bool CanRead(MODataObject moDataObject) {
            return true;
        }

        public virtual bool CanRead(MODataObject moDataObject, String attributeName) {
            return true;
        }

        public virtual bool CanRead(MOAttribute moAttribute) {
            return true;
        }

        public virtual bool CanApprove(MODataObject moDataObject) {
            return true;
        }

        public virtual bool CanWrite(MOClass moClass) {
            return true;
        }

        public virtual bool CanWrite(MODataObject moDataObject) {
            return true;
        }

        public virtual bool CanWrite(MODataObject moDataObject, String attributeName) {
            return true;
        }

        public virtual bool CanWrite(MOAttribute moAttribute) {
            return true;
        }

        public virtual bool IsSysadmin() {
            return true;
        }

        public virtual bool IsEntitled(MOView moView) {
            return true;
        }




    }

    public class MOAccessMain : IMOAccess {

        private static Logger logger = Logger.GetLogger(typeof(MOAccessMain));

        public static String USE_ROLES_TYPE;

        public static readonly String BUILTIN_ROLES = "BUILTIN_ROLES";
        public static readonly String IPRINCIPAL_ROLES = "IPRINCIPAL_ROLES";
        public static readonly String SYSADMIN = "SYSADMIN";
        public static String INITIAL_SYSADMIN_USERNAME;
        public static bool IsInitialSysAdmin(String userName) {
            return StringUtils.IsNotEmpty(userName)
                && StringUtils.IsNotEmpty(INITIAL_SYSADMIN_USERNAME)
                && userName.Equals(INITIAL_SYSADMIN_USERNAME);
        }

        public MOAccessMain() { }

        public bool CanRead(String moid) {
            MOClass moClass = MOService.GetInstance().GetMOClass(moid);
            return moClass != null && CanRead(moClass);
        }

        public bool CanRead(params MOClass[] moClasses) {
            String[] userRoles = MOSystem.GetUserRoles();
            bool canRead = true;
            foreach(MOClass moClass in moClasses) {
                canRead = canRead && _CanRead(moClass, userRoles);
            }
            return canRead;
        }

        private bool _CanRead(MOClass moClass, String[] userRoles) {
            return moClass.GetRoles().CanRead(userRoles);
        }

        public bool CanRead(MOReport moReport) {
            String[] userRoles = MOSystem.GetUserRoles();
            return moReport.GetRoles().CanRead(userRoles);
        }


        public bool CanRead(MODataObject moDataObject) {
            if(moDataObject != null) {
                String[] userRoles = MOSystem.GetUserRoles();
                return moDataObject.GetMOClass().GetRoles().CanRead(userRoles, moDataObject);
            }
            return true;
        }

        public bool CanRead(MODataObject moDataObject, String attributeName) {
            MOAttribute moAttribute = null;
            if(moDataObject != null && (moAttribute = moDataObject.GetMOClass().GetMOAttribute(attributeName)) != null) {
                String[] userRoles = MOSystem.GetUserRoles();
                return moAttribute.GetRoles().CanRead(userRoles, moDataObject);
            }
            return true;
        }

        public bool CanRead(MOAttribute moAttribute) {
            if(moAttribute != null) {
                String[] userRoles = MOSystem.GetUserRoles();
                return moAttribute.GetRoles().CanRead(userRoles);
            }
            return true;
        }

        public bool CanApprove(MODataObject moDataObject) {
            if(
                (moDataObject.GetDataState() == DataState.STORED ||
                    moDataObject.GetDataState() == DataState.DELETED_UNAPPROVED)
                &&
                !MOSystem.GetUserName().Equals(moDataObject.GetLastUserName())
                &&
                CanWrite(moDataObject)
                ) {
                return true;
            }
            return false;
        }


        public bool CanWrite(MOClass moClass) {
            if(moClass.GetMoid().EndsWith("Product")) {
                logger.Debug("Product!!");
            }

            String[] userRoles = MOSystem.GetUserRoles();

            return moClass.GetRoles().CanWrite(userRoles);
        }

        public bool CanWrite(MODataObject moDataObject) {
            if(moDataObject != null) {
                if(moDataObject.GetMOClass().GetMoid().EndsWith("Product")) {
                    logger.Debug("Product!!");
                }

                String[] userRoles = MOSystem.GetUserRoles();
                IMORoles roles = moDataObject.GetMOClass().GetRoles();
                return roles.CanWrite(userRoles, moDataObject);
            }
            return true;
        }

        public bool CanWrite(MODataObject moDataObject, String attributeName) {
            MOAttribute moAttribute = null;
            if(moDataObject != null &&
                    (moAttribute = moDataObject.GetMOClass().GetMOAttribute(attributeName)) != null) {
                String[] userRoles = MOSystem.GetUserRoles();
                return moAttribute.GetRoles().CanWrite(userRoles, moDataObject);
            }
            return true;
        }

        public bool CanWrite(MOAttribute moAttribute) {
            if(moAttribute != null) {
                String[] userRoles = MOSystem.GetUserRoles();
                return moAttribute.GetRoles().CanWrite(userRoles);
            }
            return true;
        }

        public bool IsSysadmin() {
            String[] userRoles = MOSystem.GetUserRoles();
            return ArrayUtils.Contains(userRoles, MOAccessMain.SYSADMIN);
        }

        public bool IsEntitled(MOView moView) {
            MOAttribute moAtt = moView.GetMOAttribute();
            //IMOAccess moAccess = MOAccess.GetInstance();
            //bool entitled =
            //        (moView is MOControl && moAccess.CanWrite(moAtt))
            //        ||
            //        (!(moView is MOControl) && moAccess.CanRead(moAtt));
            bool entitled =
                    (moView is MOControl && CanWrite(moAtt))
                    ||
                    (!(moView is MOControl) && CanRead(moAtt));
            return entitled;
        }




    }


    public class MOService {
        //
        // CLASS LEVEL
        //
        private static Logger logger = Logger.GetLogger(typeof(MOService));
        private static MOService instance = null;

        public static void Initialilze(String moDirectory) {
            MOService.moDirectory = moDirectory;
            new MOService();
        }

        public static MOService GetInstance() {
            return instance;
        }

        private static String moDirectory;
        //
        // OBJECT LEVEL
        //

        private Dictionary<String, MOClass> moClasses = new Dictionary<String, MOClass>();
        private Dictionary<String, MOReport> moReports = new Dictionary<String, MOReport>();
        private String[] allRoleNames;

        private MOService() {
            logger.Info("Read files in : " + moDirectory);
            String[] files = Directory.GetFiles(moDirectory, "def-*.xml");
            foreach(String file in files) {
                logger.Info("load def-file: " + file);
                MOClass moClass = new MOClass(moDirectory, file);
                moClasses.Put(moClass.GetMoid(), moClass);
            }
            // INHERITANCE
            foreach(MOClass moClass in moClasses.Values) {
                moClass.ProcessInheritance(moClasses);
            }
            // ROLE NAMES
            List<String> roleNameList = new List<String>();
            foreach(MOClass moClass in moClasses.Values) {
                if(moClass.GetMoid().EndsWith("Product")) {
                    logger.Debug("Product!!");
                }
                foreach(String readRole in moClass.GetRoles().GetReadRoles()) {
                    if(!roleNameList.Contains(readRole)) {
                        roleNameList.Add(readRole);
                    }
                }
                foreach(String writeRole in moClass.GetRoles().GetWriteRoles()) {
                    if(!roleNameList.Contains(writeRole)) {
                        roleNameList.Add(writeRole);
                    }
                }

            }
            this.allRoleNames = roleNameList.ToArray();
            MOService.instance = this;
            InitReports();
        }

        private void InitReports() {
            logger.Info("Read report files in : " + moDirectory);
            String[] files = Directory.GetFiles(moDirectory, "rep-*.xml");
            foreach(String file in files) {
                logger.Info("load rep-file: " + file);
                MOReport moReport = new MOReport(moDirectory, file);
                moReports.Put(moReport.GetMrid(), moReport);
            }
        }

        public MOReport GetMOReport(String mrid) {
            if(mrid != null) {
                MOReport rep = moReports.Get(mrid);
                if(rep == null) {
                    logger.Error("Did not found report for mrid: ", mrid);
                }
                return rep;
            } else {
                return null;
            }
        }

        public ICollection<MOReport> GetAllMOReports() {
            return moReports.Values;
        }

        public MOClass GetMOClass(String moid) {
            MOClass moc = null;
            if(moid != null) {
                moc = moClasses.Get(moid);
                if(moc == null) {
                    logger.Warn("Did not find class for moid", moid);
                }
            }
            return moc;
        }

        public MOAttribute GetMOAttribute(String moid, String attributeName) {
            MOClass moClass = GetMOClass(moid);
            if(moClass != null) {
                return moClass.GetMOAttribute(attributeName);
            } else {
                logger.Warn("No MOAttribute found for :", moid, attributeName);
            }
            return null;
        }

        public ICollection<MOClass> GetAllMOClasses() {
            return moClasses.Values;
        }

        public String[] GetAllRoleNames() {
            return this.allRoleNames;
        }

    }



    public class MOClass {
        //
        // CLASS LEVEL
        //
        internal static Logger logger = Logger.GetLogger(typeof(MOClass));
        //
        // OBJECT LEVEL
        //
        private String moid;
        private List<String> baseMoids = new List<String>();
        private String name;
        private bool fourEyesApprovalNeededForDeletion = false;
        private bool isTopLevel = false;
        private bool isAbstract = false;
        private bool isComponent = false;
        private IMORoles roles;
        private Dictionary<String, MOAttribute> moAttributes = new Dictionary<String, MOAttribute>();
        //
        private Dictionary<String, List<MOView>> synopsisMOViewList = new Dictionary<String, List<MOView>>();
        private Dictionary<String, List<String>> synopsisAttributeNamesList = new Dictionary<string, List<string>>();
        //
        private List<MOPage> editUIPages = new List<MOPage>();
        private List<MOPage> viewUIPages = new List<MOPage>();
        //
        private Dictionary<String, MOView> namedUIViews = new Dictionary<String, MOView>();
        private Dictionary<String, MOControl> namedUIControls = new Dictionary<String, MOControl>();

        //
        private bool processInheritanceDone = false;

        public MOClass(String dir, String file) {

            file = Path.Combine(dir, file);
            XmlDocument doc = new XmlDocument();

            doc.Load(file);
            // MOID
            moid = doc.DocumentElement.Attributes[MO.AttName.moid].Value;
            // BASE
            foreach(XmlElement baseElement in doc.DocumentElement.GetElementsByTagName("base")) {
                baseMoids.Add(baseElement.GetAttribute(MO.AttName.moid));
            }
            // FOUR EYES FOR DELETION
            if(doc.DocumentElement.GetElementsByTagName("fourEyesApprovalForDeletion").Count > 0) {
                fourEyesApprovalNeededForDeletion = true;
            }
            // TOPLEVEL
            if(doc.DocumentElement.GetElementsByTagName("topLevel").Count > 0) {
                isTopLevel = true;
            }
            // COMPONENT
            if(doc.DocumentElement.GetElementsByTagName("component").Count > 0) {
                isComponent = true;
            }
            // ABSTRACT
            if(doc.DocumentElement.GetElementsByTagName("abstract").Count > 0) {
                isAbstract = true;
            }
            // NAME
            name = doc.DocumentElement.GetTextByTagName("name");
            // ACCESS ROLES
            XmlNodeList list = null;
            if((list = doc.GetElementsByTagName("accessRoles")).Count > 0) {
                roles = MORoles.CreateRoles((XmlElement)list[0]);
                // XXXXXXXXXXXXXXXXXXX roles2 = new MORoles2((XmlElement)list[0]);
            }
            // ATTRIBUTES
            XmlElement attributesElement = (XmlElement)doc.DocumentElement.GetFirstChild("attributes");
            if(attributesElement != null) {
                foreach(XmlElement e in attributesElement.GetElementsByTagName("attribute")) {
                    MOAttribute att = MOAttribute.Create(this, e);
                    moAttributes.Put(att.GetName(), att);
                }
            }
            // Synopsis UI
           
            //XmlElement synopsisUIElement = (XmlElement)doc.DocumentElement.GetFirstChild("synopsisUI");
            foreach(XmlElement synopsisUIElement in doc.GetElementsByTagName("synopsisUI")) {
                if(synopsisUIElement != null) {
                    String synopsisName = synopsisUIElement.HasAttribute(MO.AttName.name) ? synopsisUIElement.GetAttributeValue(MO.AttName.name) : "";
                    List<MOView> viewList = new List<MOView>();
                    foreach(XmlElement viewElement in synopsisUIElement.GetElementsByTagName("view")) {
                        //String attributeName = view.GetAttributeValue(MO.AttName.attref);
                        //synopsisUIAttributeNames.Add(attributeName);
                        viewList.Add(MOView.Create(this, viewElement));
                    }
                    this.synopsisMOViewList.Put(synopsisName, viewList);
                }
            }
            // EDIT UI
            XmlElement editUIElement = (XmlElement)doc.DocumentElement.GetFirstChild("editUI");
            if(editUIElement != null) {
                foreach(XmlElement pageElement in editUIElement.GetElementsByTagName("page")) {
                    editUIPages.Add(MOPage.Create(this, pageElement));
                }
            }
            // VIEW UI
            XmlElement viewUIElement = (XmlElement)doc.DocumentElement.GetFirstChild("viewUI");
            if(viewUIElement != null) {
                foreach(XmlElement pageElement in viewUIElement.GetElementsByTagName("page")) {
                    viewUIPages.Add(MOPage.Create(this, pageElement));
                }
            }
        }

        internal void ProcessInheritance(Dictionary<String, MOClass> moClasses) {
            if(!processInheritanceDone) {
                foreach(String baseMoid in baseMoids) {
                    logger.Info(moid, "inherits from", baseMoid);
                    MOClass baseMOClass = moClasses.Get(baseMoid);
                    if(baseMOClass == null) {
                        logger.Error("Base MOClass not found", baseMoid);
                    }
                    if(baseMOClass != null) {
                        baseMOClass.ProcessInheritance(moClasses);
                    } else {
                        logger.Error("MO Class does not exist!", baseMoid);
                    }
                    // ROLES INHERITANCE
                    if(this.roles == null) {
                        logger.Info(moid, "inherits roles from", baseMOClass.GetMoid());
                        this.roles = baseMOClass.GetRoles();
                    }
                    // ATTRIBUTE AND TYPE INHERITANCE
                    foreach(MOAttribute baseMOAttribute in baseMOClass.GetAllMOAttributes()) {
                        MOAttribute moAttribute = this.GetMOAttribute(baseMOAttribute.GetName());
                        if(moAttribute == null) {
                            moAttributes.Add(baseMOAttribute.GetName(), baseMOAttribute);
                            logger.Info(moid, "inherits attribute", baseMOAttribute);
                        } else if(moAttribute.GetMOType() == null) {
                            logger.Info(moid, "inherits attribute type", baseMOAttribute, baseMOAttribute.GetMOType());
                            moAttribute.SetMOType(baseMOAttribute.GetMOType());
                        }
                    }
                }
                foreach(String synopsisName in this.synopsisMOViewList.Keys) {
                    UpdateSynopsisAttributeNamesList(synopsisName, synopsisMOViewList.Get(synopsisName));
                }

            }
            processInheritanceDone = true;
        }

        internal void ProcessSanityCheck() {
            List<MOPage> pages = this.GetEditUIPages();
            foreach(MOPage page in pages) {
                List<MOView> viewControl = page.GetMOViewControls();
                foreach(MOView view in viewControl) {
                    String attRef = view.GetAttRef();
                    MOAttribute moAttribute = view.GetMOClass().GetMOAttribute(attRef);
                    if(moAttribute == null) {
                        logger.Error("no attribute defined for " + view.GetAttRef() + " for class " + view.GetMOClass().moid);
                    }
                }
            }
        }

        public List<String> GetBaseMoids() {
            return baseMoids;
        }

        public String GetMoid() {
            return moid;
        }

        public String GetName() {
            return name;
        }

        public MOAttribute GetMOAttribute(String attName) {
            return moAttributes.Get(attName);
        }

        public IEnumerable<MOAttribute> GetAllMOAttributes() {
            return moAttributes.Values;
        }

        public IMORoles GetRoles() {
            return roles == null ? MORoles.EmptyMORoles : roles;
        }

        public bool FourEyesApprovalNeededForDeletion() {
            return fourEyesApprovalNeededForDeletion;
        }

        public bool IsTopLevel() {
            return isTopLevel;
        }

        public bool IsComponent() {
            return isComponent;
        }

        public bool IsAbstract() {
            return isAbstract;
        }

        //
        //public List<String> GetSynopsisUIAttributeNames() {
        //    return synopsisUIAttributeNames;
        //}
        public List<MOView> GetSynopsisViews() {
            return GetSynopsisViews("");
        }

        public List<MOView> GetSynopsisViews(String synopsisName) {
            List<MOView> synopsisViews = synopsisMOViewList.Get(synopsisName);
            if(synopsisViews != null) {
                return synopsisViews;
            }
            if(ICollectionUtils.IsEmpty(synopsisMOViewList)) {
                foreach(String moid in baseMoids) {
                    synopsisViews = MOService.GetInstance().GetMOClass(moid).GetSynopsisViews(synopsisName);
                    if(ICollectionUtils.IsNotEmpty(synopsisViews)) {
                        return synopsisViews;
                    }
                }
            }
            //
            logger.Error("Did not find synopsis for named synopsis", synopsisName, "in", this.moid);
            return null;
        }

        public List<String> GetSynopsisUIAttributeNames() {
            return GetSynopsisUIAttributeNames("");
        }

        public List<string> GetSynopsisUIAttributeNames(string synopsisName) {
            List<String> synopsisNames = synopsisAttributeNamesList.Get(synopsisName);
            if(synopsisNames != null) {
                return synopsisNames;
            }
            foreach(String moid in baseMoids) {
                synopsisNames = MOService.GetInstance().GetMOClass(moid).GetSynopsisUIAttributeNames(synopsisName);
                if(ICollectionUtils.IsNotEmpty(synopsisNames)) {
                    return synopsisNames;
                }
            }
            logger.Error("Did not find synopsis attributes names for named synopsis", synopsisName, "in", this.moid);
            return null;
            //
        }

        private void UpdateSynopsisAttributeNamesList(String synopsisName, List<MOView> viewList) {
            List<String> synopsisNames = new List<String>();
            foreach(MOView view in viewList) {
                try {
                    MOAttribute moAttribute = view.GetMOAttribute();
                    if(moAttribute == null) {
                        logger.Warn("MOAttribute is null! ", view.GetAttRef(), view.GetMOClass().moid);
                    }
                    synopsisNames.Add(moAttribute.GetName());
                }
                catch(Exception e) {
                    logger.Error(view.GetAttRef(), view.GetMOClass().moid, e);
                }
            }
            synopsisAttributeNamesList.Put(synopsisName, synopsisNames);
            logger.Info("UpdateSynopsisAttributeNamesList", moid, synopsisName, IEnumerableUtils.Join(synopsisNames));
        }


        public List<MOPage> GetEditUIPages() {
            if(ICollectionUtils.IsEmpty(editUIPages)) {
                foreach(String moid in baseMoids) {
                    List<MOPage> pages = MOService.GetInstance().GetMOClass(moid).GetEditUIPages();
                    if(ICollectionUtils.IsNotEmpty(pages)) {
                        return pages;
                    }
                }
            }
            return editUIPages;
        }

        public List<MOPage> GetViewUIPages() {
            if(ICollectionUtils.IsEmpty(viewUIPages)) {
                foreach(String moid in baseMoids) {
                    List<MOPage> pages = MOService.GetInstance().GetMOClass(moid).GetViewUIPages();
                    if(ICollectionUtils.IsNotEmpty(pages)) {
                        return pages;
                    }
                }
            }
            return viewUIPages;
        }


        public MOView GetNamedMOView(String name) {
            return this.namedUIViews.Get(name);
        }

        public MOView GetNamedMOControl(String name) {
            return this.namedUIControls.Get(name);
        }

        internal void AddMOView(MOView moView) {
            bool overwrite = true;
            String key = moView.GetName();
            if(StringUtils.IsEmpty(key)) {
                key = moView.GetAttRef();
                overwrite = false;
            }
            if(StringUtils.IsEmpty(key)) {
                return;
            }
            if(moView is MOControl) {
                this.namedUIControls.Put(key, (MOControl)moView, overwrite);
            } else {
                this.namedUIViews.Put(key, moView, overwrite);
            }
        }

        public override String ToString() {
            StringBuilder sb = new StringBuilder();
            //
            sb.Append("[moid=");
            sb.Append(GetMoid());
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
            sb.Append("\n attributes=");
            foreach(MOAttribute att in moAttributes.Values) {
                sb.Append(att.ToString());
                sb.Append(" ");
            }
            sb.Append("; ");
            //
            sb.Append("\npages=");
            foreach(MOPage page in editUIPages) {
                sb.Append(page.ToString());
                sb.Append(" ");
            }
            sb.Append("]");
            //
            return sb.ToString();
        }



    }


    public class DefaultValue {
        private String[] values;
        private HashSet<DataState> states = new HashSet<DataState>();

        public DefaultValue(String[] values, HashSet<DataState> states) {
            this.values = values;
            this.states = states;
        }

        public String[] GetValues(MODataObject mod) {
            DataState ds = mod.GetDataState();
            return states.Contains(ds) ? values : null;
        }
    }

    public class MOAttribute {

        //
        // CLASS LEVEL
        //
        internal static Logger logger = Logger.GetLogger(typeof(MOAttribute));
        //
        public static MOAttribute Create(MOClass moclass, XmlElement attribute) {
            return attribute == null ? null : new MOAttribute(moclass, attribute);
        }

        //
        // OBJECT LEVEL
        //
        private MOClass moClass;
        private String name;
        private String label;
        private MOType type;
        private IMORoles roles;
        private String serviceName;
        private String[] serviceStatus;
        private bool fourEyesApproval;
        private bool mandatory;
        private bool partOfKey;
        private bool searchable;
        private bool searchHintNumbers;
        private bool history;
        //
        private DefaultValue defaultValue;

        private MOAttribute(MOClass moclass, XmlElement element) {
            this.moClass = moclass;
            name = element.Attributes["name"].Value;
            label = element.GetTextByTagName("label");
            label = label == null ? label : label.Trim();
            //

            XmlElement typeElement = null;
            if((typeElement = element.GetFirstChild("type")) != null) {
                type = MOType.CreateType(this, typeElement.GetFirstChild());
            }
            if(element.GetFirstChild("defaultValues") != null) {
                String[] dVals = element.GetFirstChild("defaultValues").InnerText.Split(',');
                String sValus = element.GetFirstChild("defaultValues").GetAttributeValue("states");
                this.defaultValue = new DefaultValue(dVals, DataStateUtils.AsDataStates(sValus.Split(',')));
            }
            // roles = MORoles.CreateRoles(element.GetFirstChild("accessRoles"));
            roles = MORoles.CreateRoles(element.GetFirstChild("accessRoles"));
            //
            XmlElement serviceElement = null;
            if((serviceElement = element.GetFirstChild("service")) != null) {
                serviceName = serviceElement.GetAttributeValue("name");
                String s = serviceElement.GetAttributeValue("status");
                if(StringUtils.IsNotEmpty(s)) {
                    serviceStatus = s.ToLower().Split(',');
                }
            }
            //
            fourEyesApproval = element.GetElementsByTagName("fourEyesApproval").Count > 0;
            mandatory = element.GetElementsByTagName("mandatory").Count > 0;
            partOfKey = element.GetElementsByTagName("partOfKey").Count > 0;
            //
            searchable = element.GetElementsByTagName("searchable").Count > 0;
            if(searchable) {
                XmlElement searchableElement = (XmlElement)element.GetElementsByTagName("searchable").Item(0);
                String searchHints = StringUtils.RNN(searchableElement.GetAttributeValue(MO.AttName.searchHints));
                searchHintNumbers = searchHints.Contains(MO.AttValue.numbers);
                //if(searchHintNumbers) {
                //    logger.Debug(this.GetName() + " searchHintNumbers is true ");
                //}
            }
            //
            history = element.GetElementsByTagName("history").Count > 0;
        }

        public bool CanRead(String[] userRoles) {
            return GetRoles().CanRead(userRoles);
        }

        public bool CanWrite(String[] userRoles) {
            return GetRoles().CanWrite(userRoles);
        }

        public String GetName() {
            return name;
        }

        public String GetLabel() {
            return StringUtils.RNN(label);
        }

        public MOType GetMOType() {
            return type;
        }

        internal void SetMOType(MOType type) {
            this.type = type;
        }

        internal String GetTypeId() {
            return this.GetMOType().GetTypeId();
        }

        public IMORoles GetRoles() {
            return roles == null ? moClass.GetRoles() : roles;
        }

        public bool IsMandatory() {
            return mandatory;
        }

        public bool IsPartOfKey() {
            return partOfKey;
        }

        public bool IsFourEyesApproval() {
            return fourEyesApproval;
        }

        public bool IsSearchable() {
            return searchable;
        }

        public bool IsHistory() {
            return history;
        }

        public MOClass GetMOClass() {
            return moClass;
        }

        public String GetMoid() {
            return moClass.GetMoid();
        }

        internal String GetServiceName() {
            return this.serviceName;
        }

        public String[] GetServiceStatus() {
            return this.serviceStatus;
        }


        public String[] Format(String[] p) {
            return GetMOType().Format(p);
        }

        public override String ToString() {
            StringBuilder sb = new StringBuilder();
            //
            sb.Append("[attName=");
            sb.Append(GetName());
            sb.Append("; ");
            sb.Append(mandatory ? "mandatory; " : "");
            sb.Append(searchable ? "searchable; " : "");
            sb.Append(fourEyesApproval ? "fourEyesApproval; " : "");
            sb.Append(history ? "history; " : "");
            sb.Append("type=");
            sb.Append(GetMOType().ToString());
            sb.Append("]");
            //
            return sb.ToString();
        }

        public String[] GetDefaultValues(MODataObject mod) {
            if(defaultValue == null) {
                return null;
            }
            return defaultValue.GetValues(mod);
        }


        public bool hasSearchHintNumbers() {
            return this.searchHintNumbers;
        }
    }


    public abstract class MOType {

        //
        // CLASS LEVEL
        //
        private static Logger logger = Logger.GetLogger(typeof(MOType));

        public static int MAXCHARS_DEFAULT = 1024;

        public static readonly String VALUE = "value";
        public static readonly String STRING = "string";
        public static readonly String TEXT = "text";
        public static readonly String INTEGER = "integer";
        public static readonly String FLOAT = "float";
        public static readonly String PERCENTAGE = "percentage";
        public static readonly String BOOLEAN = "boolean";
        public static readonly String CODETABLE = "codetable";
        public static readonly String TUPLE = "tuple";
        public static readonly String LIST = "list";
        public static readonly String ONEOF = "oneof";
        public static readonly String DATETIME = "datetime";
        public static readonly String DATE = "date";
        public static readonly String TIME = "time";
        public static readonly String CURRENCY = "currency";
        public static readonly String REF = "ref";
        public static readonly String BACK_REF = "back-ref";
        public static readonly String COMP = "comp";
        public static readonly String FOREACH = "foreach";
        public static readonly String OBJECTREF = "object-reference";




        public static MOType CreateType(MOAttribute moAttribute, XmlElement typeElement) {
            //

            String typeName = typeElement.Name.ToLower();
            //
            if(typeName.Equals(STRING)) {
                return new MOTypeString(moAttribute, typeElement, STRING);
            }
                //
            else if(typeName.Equals(TEXT)) {
                return new MOTypeString(moAttribute, typeElement, TEXT);
            }
                //
            else if(typeName.Equals(CODETABLE)) {
                return new MOTypeCodeTable(moAttribute, typeElement, CODETABLE);
            }
                //
            else if(typeName.Equals(FLOAT)) {
                return new MOTypeFloat(moAttribute, typeElement, FLOAT);
            }
                //
            else if(typeName.Equals(PERCENTAGE)) {
                return new MOTypePercentage(moAttribute, typeElement, PERCENTAGE);
            }
                //
            else if(typeName.Equals(INTEGER)) {
                return new MOTypeInteger(moAttribute, typeElement, INTEGER);
                //
            } else if(typeName.Equals(BOOLEAN)) {
                return new MOTypeBoolean(moAttribute, typeElement, BOOLEAN);
            }
                //
            else if(typeName.Equals(TUPLE)) {
                return new MOTypeTuple(moAttribute, typeElement, TUPLE);
            }
                //
            else if(typeName.Equals(LIST)) {
                return new MOTypeList(moAttribute, typeElement, LIST);
            }
                //
            else if(typeName.Equals(ONEOF)) {
                return new MOTypeOneOf(moAttribute, typeElement, ONEOF);
            }
                //
            else if(typeName.Equals(DATETIME)) {
                return new MOTypeDateTime(moAttribute, typeElement, DATETIME);
            }
                //
            else if(typeName.Equals(DATE)) {
                return new MOTypeDateTime(moAttribute, typeElement, DATE);
            }
                //
            else if(typeName.Equals(TIME)) {
                return new MOTypeDateTime(moAttribute, typeElement, TIME);
            }
                //
            else if(typeName.Equals(CURRENCY)) {
                return new MOTypeCurrency(moAttribute, typeElement, CURRENCY);
            }
                //
            else if(typeName.Equals(REF)) {
                return new MOTypeRef(moAttribute, typeElement, REF);
            }
                //
            else if(typeName.Equals(COMP)) {
                return new MOTypeComp(moAttribute, typeElement, COMP);
            }
                //
            else if(typeName.Equals(BACK_REF)) {
                return new MOTypeBackRef(moAttribute, typeElement, BACK_REF);
            }                 //
            else if(typeName.Equals(FOREACH)) {
                return new MOTypeForeach(moAttribute, typeElement, FOREACH);
            } else {
                logger.Warn("Did not find type for : " + typeName);
                return null;
            }
        }

        //
        // OBJECT LEVEL
        //
        private MOAttribute moAttribute;

        public MOAttribute GetMOAttribute() {
            return this.moAttribute;
        }
        //
        private String typeId;
        protected String widgetHint;
        private int maxChars;

        public int GetMaxChars() {
            if(maxChars == 0 && this.typeId.Equals(PERCENTAGE)) {
                return 4;
            }
            return maxChars == 0 ? MAXCHARS_DEFAULT : maxChars;
        }


        protected MOType(MOAttribute moAttribute, XmlElement typeElement, String typeId) {
            this.moAttribute = moAttribute;
            //MOClass.logger.Debug("element " + (element == null ? "null" : element.ToString()));
            widgetHint = typeElement.GetAttributeValue("widgetHint");
            this.typeId = String.Intern(typeId);
            //
            String s = typeElement.GetAttributeValue("maxchars");
            if(StringUtils.IsNotEmpty(s)) {
                Int32.TryParse(s, out maxChars);
            }
        }

        public String GetTypeId() {
            return typeId;
        }

        public String GetWidgetHint() {
            return widgetHint == null ? typeId : widgetHint;
        }

        public override String ToString() {
            return "[type=" + typeId + "; widgetHint=" + GetWidgetHint() + "]";
        }

        public bool IsType(String typeId) {
            return this.typeId.Equals(typeId);
        }

        public virtual bool IsMultiple() {
            return IsType(LIST);
        }

        public virtual MOType GetBaseType() {
            if(IsType(LIST)) {
                return ((MOTypeList)this).GetSubType();
            } else {
                return this;
            }
        }

        public virtual String GetFullTypeId() {
            if(IsType(LIST)) {
                return LIST + "-" + ((MOTypeList)this).GetSubType().GetTypeId();
            } else {
                return GetTypeId();
            }
        }


        public virtual String[] Format(String[] p) {
            return p;
        }
    }


    public class MOTypeString : MOType {

        public MOTypeString(MOAttribute moAttribute, XmlElement typeElement, String typeId)
            : base(moAttribute, typeElement, typeId) { }

    }

    public class MOTypeInteger : MOType {

        public MOTypeInteger(MOAttribute moAttribute, XmlElement typeElement, String typeId)
            : base(moAttribute, typeElement, typeId) {
        }


    }

    public class MOTypeFloat : MOType {

        public MOTypeFloat(MOAttribute moAttribute, XmlElement typeElement, String typeId)
            : base(moAttribute, typeElement, typeId) {
        }

    }

    public class MOTypePercentage : MOType {

        public MOTypePercentage(MOAttribute moAttribute, XmlElement typeElement, String typeId)
            : base(moAttribute, typeElement, typeId) {
        }

    }

    public class MOTypeBoolean : MOType {

        public MOTypeBoolean(MOAttribute moAttribute, XmlElement typeElement, String typeId)
            : base(moAttribute, typeElement, typeId) {
        }

    }


    public class MOTypeDateTime : MOType {

        private static Logger logger = Logger.GetLogger(typeof(MOTypeDateTime));


        private String formatPattern = "";

        public MOTypeDateTime(MOAttribute moAttribute, XmlElement typeElement, String typeId)
            : base(moAttribute, typeElement, typeId) {
            if(DATETIME.Equals(typeId)) {
                formatPattern = "{0:yyyy-MM-dd HH:mm}";
            } else if(DATE.Equals(typeId)) {
                formatPattern = "{0:yyyy-MM-dd}";
            } else if(TIME.Equals(typeId)) {
                formatPattern = "{0:HH:mm}";
            } else {
                logger.Error("Unknown typeId for MOTypeDateTime", typeId);
            }
        }

        public override String[] Format(String[] p) {
            if(ArrayUtils.IsNotEmpty(p)) {
                String[] r = new String[p.Length];
                for(int i = 0; i < p.Length; i++) {
                    try {
                        String s = p[i];
                        if(StringUtils.IsNotBlank(s)) {
                            r[i] = String.Format(formatPattern, DateTime.Parse(p[i]));
                        } else {
                            r[i] = "";
                        }
                    }
                    catch(Exception) {
                        logger.Warn("Could not format date time: >" + p[i] + "<.");
                    }
                }
                return r;
            }
            return p;
        }



    }

    public class MOTypeCurrency : MOType {

        private static Logger logger = Logger.GetLogger(typeof(MOTypeCurrency));


        public MOTypeCurrency(MOAttribute moAttribute, XmlElement typeElement, String typeId)
            : base(moAttribute, typeElement, typeId) {
        }
    }

    public class MOTypeRef : MOType {

        private String lookupMoid;

        public MOTypeRef(MOAttribute moAttribute, XmlElement typeElement, String typeId)
            : base(moAttribute, typeElement, typeId) {
            lookupMoid = typeElement.Attributes["lookup"].Value;
        }

        public String GetLookupMoid() {
            return lookupMoid;
        }

        public override String ToString() {
            return "[" + GetTypeId() + "=" + lookupMoid + "]";
        }
    }

    public class MOTypeComp : MOTypeRef {
        private String attrefBack;
        public MOTypeComp(MOAttribute moAttribute, XmlElement typeElement, String typeId)
            : base(moAttribute, typeElement, typeId) {
            attrefBack = typeElement.GetAttributeValue("attrefBack");
        }
        public String GetAttrefBack() {
            return StringUtils.RNN(attrefBack);
        }

    }

    public class MOTypeCodeTable : MOType {

        private String codeTableName;
        private String codeTableParent;
        private bool multiple;
        private bool roles;

        public MOTypeCodeTable(MOAttribute moAttribute, XmlElement typeElement, String typeId)
            : base(moAttribute, typeElement, typeId) {
            codeTableName = typeElement.GetAttributeValue("name");
            codeTableParent = typeElement.GetAttributeValue("parent");
            if(typeElement.HasAttribute("selection")) {
                multiple = "multiple".Equals(typeElement.Attributes["selection"].Value.ToLower());
            }
            if(typeElement.HasAttribute("roles")) {
                roles = "yes".Equals(typeElement.Attributes["roles"].Value.ToLower());
                roles = roles || "true".Equals(typeElement.Attributes["roles"].Value.ToLower());
            }
        }

        public bool IsChildCodeTable() {
            return StringUtils.IsNotBlank(this.codeTableParent);
        }

        public String GetParent() {
            return codeTableParent;
        }

        public override String ToString() {
            return "[" + GetTypeId() + "=" + codeTableName + "]";
        }

        public String GetCodeTableName() {
            return this.codeTableName;
        }

        public bool IsRoles() {
            return roles;
        }

        public override bool IsMultiple() {
            return multiple;
        }


        public ICodeTable CreateCodeTable(bool emptyElement, String parentCode) {
            if(IsChildCodeTable()) {
                //ICodeTable ct = this.CreateParentCodeTable(emptyElement);
                //ct.GetChildCodeTable(parentCode);
                return null;
            } else {
                ICodeTable ct = CodeTable.Create(codeTableName, true);
                return ct;
            }
        }

        public ICodeTable CreateParentCodeTable(bool emptyElement) {

            MOTypeCodeTable ctype = (MOTypeCodeTable)GetMOAttribute().GetMOClass().GetMOAttribute(this.codeTableParent).GetMOType().GetBaseType();

            if(IsChildCodeTable()) {
                //ICodeTable ct = this.CreateParentCodeTable();
                //ct.GetChildCodeTable(parentCode);
            } else {
                ICodeTable ct = CodeTable.Create(codeTableName, true);
                return ct;
            }
            return null;
        }
    }

    public class MOTypeBackRef : MOType {

        private String moidref;
        private String attref;

        public MOTypeBackRef(MOAttribute moAttribute, XmlElement typeElement, String typeId)
            : base(moAttribute, typeElement, typeId) {
            moidref = typeElement.Attributes[MO.AttName.moid].Value;
            attref = typeElement.Attributes[MO.AttName.attref].Value;
        }

        public String GetMoidRef() {
            return moidref;
        }

        public String GetAttRef() {
            return attref;
        }

        public override String ToString() {
            return "[" + GetTypeId() + "=" + moidref + "" + moidref + "]";
        }
    }

    public class MOTypeList : MOType {

        private MOType subType;

        public MOTypeList(MOAttribute moAttribute, XmlElement typeElement, String typeId)
            : base(moAttribute, typeElement, typeId) {
            subType = MOType.CreateType(moAttribute, (XmlElement)typeElement.FirstChild);
        }

        public override String ToString() {
            return "[" + GetTypeId() + " subType=" + subType.ToString() + "; widgetHint=" + GetWidgetHint() + "]";
        }

        public MOType GetSubType() {
            return subType;
        }

    }



    public class MOTypeOneOf : MOType {

        private List<String> attRefs = new List<String>();

        public MOTypeOneOf(MOAttribute moAttribute, XmlElement typeElement, String typeId)
            : base(moAttribute, typeElement, typeId) {
            Init(typeElement, typeId, "select");
        }

        public MOTypeOneOf(MOAttribute moAttribute, XmlElement typeElement, String typeId, String selectionElementName)
            : base(moAttribute, typeElement, typeId) {
            Init(typeElement, typeId, selectionElementName);
        }

        private void Init(XmlElement typeElement, String typeId, String selectionElementName) {
            foreach(XmlNode xmlNode in typeElement.ChildNodes) {
                if(xmlNode is XmlElement) {
                    XmlElement xmlElement = (XmlElement)xmlNode;
                    if(xmlElement.Name.Equals(selectionElementName)) {
                        String attref = xmlElement.GetAttributeValue(MO.AttName.attref);
                        if(StringUtils.IsNotEmpty(attref)) {
                            attRefs.AddUnique(attref);
                        }
                    }
                }
            }
        }

        public override String ToString() {
            return "[" + GetTypeId() + " selection=" + ArrayUtils.Join(attRefs.ToArray()) + "; widgetHint=" + GetWidgetHint() + "]";
        }

        public List<String> GetSelectionAttributeNames() {
            return attRefs;
        }

    }


    public class MOTypeTuple : MOTypeOneOf {

        private static Logger logger = Logger.GetLogger(typeof(MOTypeTuple));

        private List<MOType> tupleTypes = new List<MOType>();

        public MOTypeTuple(MOAttribute moAttribute, XmlElement typeElement, String typeId)
            : base(moAttribute, typeElement, typeId, "entry") { }

        public List<String> GetAttRefs() {
            return GetSelectionAttributeNames();
        }

    }



    public class MOTypeForeach : MOType {

        private static Logger logger = Logger.GetLogger(typeof(MOTypeForeach));

        private String attref;
        private MOTypeComp comp;

        public MOTypeForeach(MOAttribute moAttribute, XmlElement typeElement, String typeId)
            : base(moAttribute, typeElement, typeId) {
            attref = typeElement.GetAttributeValue(MO.AttName.attref);
            XmlElement compElement = typeElement.GetFirstChild();
            if(compElement == null) {
                logger.Error("no comp element defined ", typeId, typeElement);
            }
            if(compElement.Name.ToLower().Equals(COMP)) {
                comp = (MOTypeComp)MOType.CreateType(moAttribute, compElement);
            } else {
                logger.Error("no comp element defined ", typeId, typeElement);
            }
        }

        public MOTypeComp GetCompType() {
            return comp;
        }

        public String GetAttref() {
            return attref;
        }

    }

    public interface IMORoles {

        bool CanRead(String[] userRoles);
        bool CanWrite(String[] userRoles);
        bool CanRead(String[] userRoles, params MODataObject[] mods);
        bool CanWrite(String[] userRoles, params MODataObject[] mods);
        String[] GetReadRoles();
        String[] GetWriteRoles();
    }

    public class MORoles : IMORoles {
        //
        // CLASS LEVEL
        //
        public static MORoles EmptyMORoles = new MORoles(null);

        public static IMORoles CreateRoles(XmlElement accessRoles) {
            //return new MORoles(accessRoles);
            return new MORoles2(accessRoles);
        }
        //
        // OBJECT LEVEL
        //
        private String[] readRoles;
        private String[] writeRoles;

        private MORoles(XmlElement accessRoles) {
            if(accessRoles == null) {
                readRoles = new String[0];
                writeRoles = readRoles;
            } else {
                readRoles = ArrayUtils.Trim(accessRoles.Attributes["read"].Value.Split(','));
                writeRoles = ArrayUtils.Trim(accessRoles.Attributes["write"].Value.Split(','));
            }
        }

        public override String ToString() {
            StringBuilder sb = new StringBuilder();
            sb.Append("[readRoles=");
            sb.Append(ArrayUtils.Join(readRoles));
            sb.Append("; ");
            sb.Append("writeRoles=");
            sb.Append(ArrayUtils.Join(writeRoles));
            sb.Append("]");
            return sb.ToString();
        }


        public bool CanRead(String[] userRoles) {
            if(ArrayUtils.IsEmpty(readRoles)) {
                return true;
            }
            foreach(String r in userRoles) {
                if(ArrayUtils.Contains(readRoles, r)) {
                    return true;
                }
            }
            return false;
        }

        public bool CanWrite(String[] userRoles) {
            if(ArrayUtils.IsEmpty(writeRoles)) {
                return true;
            }
            foreach(String r in userRoles) {
                if(ArrayUtils.Contains(writeRoles, r)) {
                    return true;
                }
            }
            return false;
        }

        public bool CanRead(String[] userRoles, params MODataObject[] mods) {
            return CanRead(userRoles);
        }
        public bool CanWrite(String[] userRoles, params MODataObject[] mods) {
            return CanWrite(userRoles);
        }


        public String[] GetReadRoles() {
            return readRoles;
        }

        public String[] GetWriteRoles() {
            return writeRoles;
        }

    }

    public class MORoles2 : IMORoles {

        private AccessExp read;
        private AccessExp write;

        //public static MORoles2 CreateRoles(XmlElement accessRoles) {
        //    return new MORoles2(accessRoles);
        //}

        public MORoles2(XmlElement rolesElement) {

            if(rolesElement == null) {
                return;
            }
            String s = rolesElement.ToString();

            if(rolesElement.HasAttribute("read") || rolesElement.HasAttribute("write")) {
                String[] readRoles = ArrayUtils.Trim(rolesElement.Attributes["read"].Value.Split(','));
                String[] writeRoles = ArrayUtils.Trim(rolesElement.Attributes["write"].Value.Split(','));
                read = new AccessExpOr(readRoles);
                write = new AccessExpOr(writeRoles);
                return;
            }



            IList<XmlElement> nodes = rolesElement.GetChildren();
            foreach(XmlElement acc in nodes) {
                if(acc.Name.Equals("read")) {
                    read = AccessExpFactory.Create(acc.GetFirstChild(), MOAccess.READ_POSTFIX);
                }
                if(acc.Name.Equals("write")) {
                    write = AccessExpFactory.Create(acc.GetFirstChild(), MOAccess.WRITE_POSTFIX);
                }
            }
        }

        public bool CanRead(String[] roles) {
            return read == null ? true : read.IsGranted(roles);
        }
        public bool CanRead(String[] roles, params MODataObject[] mods) {
            return read == null ? true : read.IsGranted(roles, mods);
        }
        public bool CanWrite(String[] roles) {
            return write == null ? true : write.IsGranted(roles);
        }
        public bool CanWrite(String[] roles, params MODataObject[] mods) {
            return write == null ? true : write.IsGranted(roles, mods);
        }

        public String[] GetWriteRoles() {
            return write.GetAllRoles();
        }
        public String[] GetReadRoles() {
            return read.GetAllRoles();
        }
    }


    public class AccessExpFactory {
        public static AccessExp Create(XmlElement expElement, String postFix) {
            if(expElement.Name.Equals("or")) {
                return new AccessExpOr(expElement, postFix);
            }
            if(expElement.Name.Equals("and")) {
                return new AccessExpAnd(expElement, postFix);
            }
            if(expElement.Name.Equals("role")) {
                return new AccessExpRole(expElement, postFix);
            }
            if(expElement.Name.Equals("attribute")) {
                return new AccessExpAttribute(expElement, postFix);
            }
            throw new Exception("Unsupported Access Expression : " + expElement);
        }
    }

    public static class Access {
        public const String READ = "READ";
        public const String WRITE = "WRITE";
        public const String DELETE = "DELETE";
        public const String CREATE = "CREATE";
    };

    public abstract class AccessExp {
        public virtual bool IsGranted(IList roles, params MODataObject[] mods) {
            return false;
        }
        public abstract String[] GetAllRoles();
    }

    public class AccessExpOr : AccessExp {

        protected List<AccessExp> subList = new List<AccessExp>();
        public AccessExpOr(XmlElement expElement, String postFix) {
            foreach(XmlElement c in expElement.GetChildren()) {
                subList.Add(AccessExpFactory.Create(c, postFix));
            }
        }

        public AccessExpOr(IList<String> roles) {
            foreach(String role in roles) {
                subList.Add(new AccessExpRole(role));
            }
        }

        public override bool IsGranted(IList roles, params MODataObject[] mods) {
            foreach(AccessExp exp in subList) {
                if(exp.IsGranted(roles, mods)) {
                    return true;
                }
            }
            return false;
        }

        public override String[] GetAllRoles() {
            HashSet<String> roles = new HashSet<string>();
            foreach(AccessExp exp in subList) {
                roles.UnionWith(exp.GetAllRoles());
            }
            return roles.ToList().ToArray();
        }
    }


    public class AccessExpAnd : AccessExpOr {

        public AccessExpAnd(XmlElement expElement, String postFix)
            : base(expElement, postFix) {
        }

        public override bool IsGranted(IList roles, params MODataObject[] mods) {
            foreach(AccessExp exp in subList) {
                if(!exp.IsGranted(roles, mods)) {
                    return false;
                }
            }
            return true;
        }

    }

    public class AccessExpRole : AccessExp {
        protected String role;
        public AccessExpRole(XmlElement expElement, String postFix) {
            this.role = expElement.InnerText;
        }
        public AccessExpRole(String role) {
            this.role = role;
        }
        public override bool IsGranted(IList roles, params MODataObject[] mods) {
            return roles.Contains(role);
        }
        public override String[] GetAllRoles() {
            return new String[] { role };
        }

    }

    public class AccessExpAttribute : AccessExp {
        protected String attref;
        protected String postFix;
        public AccessExpAttribute(XmlElement expElement, String postFix) {
            attref = expElement.GetAttributeValue("attref");
            this.postFix = postFix;
        }

        public override bool IsGranted(IList roles, params MODataObject[] mods) {
            if(ArrayUtils.IsNotEmpty(mods)) {
                foreach(MODataObject mod in mods) {
                    String roleValue = mod.GetCurrentValue(attref);
                    if(StringUtils.IsNotBlank(roleValue) && !roles.Contains(roleValue + postFix)) {
                        return false;
                    }
                }
            }
            return true;
        }

        public override String[] GetAllRoles() {
            return new String[] { };
        }
    }

    public class MOPage {

        public static MOPage Create(MOClass moClass, XmlElement pageElement) {
            return new MOPage(moClass, pageElement);
        }

        private MOClass moClass;
        private List<MOView> moViews = new List<MOView>();
        private String label;


        private MOPage(MOClass moClass, XmlElement pageElement) {
            this.moClass = moClass;
            MOView tmpMOView = null;
            foreach(XmlNode xmlNode in pageElement.ChildNodes) {
                if(xmlNode is XmlElement) {
                    XmlElement xmlElement = (XmlElement)xmlNode;
                    if(xmlElement.Name.Equals("label")) {
                        label = xmlElement.InnerText.Trim();
                    }
                    if(xmlElement.Name.Equals("control")) {
                        moViews.Add(tmpMOView = MOControl.Create(moClass, xmlElement));
                        moClass.AddMOView(tmpMOView);
                    }
                    if(xmlElement.Name.Equals("view")) {
                        moViews.Add(tmpMOView = MOView.Create(moClass, xmlElement));
                        moClass.AddMOView(tmpMOView);
                    }
                }
            }
        }

        public String GetLabel() {
            return StringUtils.RNN(label);
        }

        public List<MOView> GetMOViewControls() {
            return moViews;
        }

        public override String ToString() {
            StringBuilder sb = new StringBuilder();
            sb.Append("[page=");
            sb.Append(GetLabel());
            sb.Append("; attributes=");
            foreach(MOControl control in moViews) {
                sb.Append(control.GetMOAttribute().GetName());
                sb.Append(" ");
            }
            sb.Append("]");
            return sb.ToString();
        }
    }

    public class MOView {

        public static class AttName {
            public static readonly String maxcols = "maxcols";
            public static readonly String rows = "rows";
            public static readonly String widgetHint = "widgetHint";
            public static readonly String synopsisName = "synopsisName";
        }

        public static class ElementName {
            public static readonly String control = "control";
            public static readonly String view = "view";
        }


        public static int MAXCOLS_DEFAULT = 40;

        public static MOView Create(MOClass moClass, XmlElement controlElement) {
            return new MOView(moClass, controlElement);
        }

        private MOClass moClass;
        private String attref;
        private String name;
        private String synopsisName;
        /*
         * 
         *  <view objref="product" attref="startDate" />
         */
        private String widgetHint;
        //private String[] uiHints;
        private UIHints uiHints;
        private int maxCols;
        private List<MOView> subViews = new List<MOView>();
        private XmlElement controlElement;

        protected MOView(MOClass moClass, XmlElement controlElement) {
            this.moClass = moClass;
            this.controlElement = controlElement;
            attref = controlElement.GetAttributeValue(MO.AttName.attref);
            name = controlElement.GetAttributeValue(MO.AttName.name);
            synopsisName = controlElement.GetAttributeValue(AttName.synopsisName);
            widgetHint = controlElement.GetAttributeValueIgnoreCase(AttName.widgetHint);
            //
            uiHints = UIHints.Create(controlElement.GetAttributeValue(MO.AttName.uihints));
            //
            Int32.TryParse(controlElement.GetAttributeValue(AttName.maxcols), out maxCols);
            foreach(XmlNode xmlNode in controlElement.ChildNodes) {
                if(xmlNode is XmlElement) {
                    XmlElement xmlElement = (XmlElement)xmlNode;
                    if(xmlElement.Name.Equals(ElementName.control)) {
                        subViews.Add(MOControl.Create(moClass, xmlElement));
                    }
                    if(xmlElement.Name.Equals(ElementName.view)) {
                        subViews.Add(MOView.Create(moClass, xmlElement));
                    }
                }
            }
            moClass.AddMOView(this);
        }

        public String GetAttributeValue(String name) {
            return controlElement.GetAttributeValue(name);
        }

        public MOView GetSubView(String attref) {
            foreach(MOView view in subViews) {
                if(view.GetAttRef().Equals(attref)) {
                    return view;
                }
            }
            return null;
        }

        public List<MOView> GetAllSubViews() {
            return this.subViews;
        }

        public MOAttribute GetMOAttribute() {
            return moClass.GetMOAttribute(attref);
        }

        public String GetAttRef() {
            return this.attref;
        }

        public String GetWidgetHint() {
            return widgetHint;
        }

        public MOClass GetMOClass() {
            return moClass;
        }

        public int GetMaxCols() {
            return maxCols == 0 ? MAXCOLS_DEFAULT : maxCols;
        }

        public bool HasUIHint(String uiHint) {
            return uiHints != null && uiHints.HasKey(uiHint);
        }

        public String GetUIHintValue(String uiHint) {
            return uiHints.GetValue(uiHint);
        }
        public UIHints GetUIHints() {
            return uiHints;
        }
        public String GetName() {
            return name;
        }
        public String GetSynopsisName() {
            return StringUtils.RNN(synopsisName);
        }
    }

    public class MOControl : MOView {

        public new static MOControl Create(MOClass moClass, XmlElement controlElement) {
            return new MOControl(moClass, controlElement);
        }

        private MOControl(MOClass moClass, XmlElement controlElement)
            : base(moClass, controlElement) {
        }
    }

    public class MODataObject {
        //
        // CLASS LEVEL
        //
        private static Logger logger = Logger.GetLogger(typeof(MODataObject));
        public static readonly String SYNOPSIS_MODE = "synopsis";

        public static MODataObject Create(String moid) {
            return MOServiceProcessor.RunServices(new MODataObject(moid), MOServiceProcessor.ONNEW);
        }

        public static MODataObject Create(MOClass moClass) {
            return MOServiceProcessor.RunServices(new MODataObject(moClass), MOServiceProcessor.ONNEW);
        }

        public static MODataObject GetById(long oid) {
            DBObject dbObject = DBService.GetInstance().GetById(oid);
            if(dbObject != null) {
                return new MODataObject(dbObject);
            } else {
                return null;
            }
        }

        public static MODataObject GetById(String oid) {
            try {
                return GetById(Int64.Parse(oid));
            }
            catch(Exception e) {
                logger.Error(e, e, "GetById:oid", oid);
            }
            return null;
        }

        public static List<MODataObject> GetByIds(params String[] oids) {
            if(ArrayUtils.IsEmpty(oids)) {
                return ListUtils.EmptyList<MODataObject>();
            }
            List<MODataObject> res = new List<MODataObject>();
            foreach(String oid in oids) {
                MODataObject mod = GetById(Int64.Parse(oid));
                if(mod != null) {
                    res.Add(mod);
                }
            }
            return res;
        }


        private static MODataObject Wrap(DBObject dbObject) {
            return new MODataObject(dbObject);
        }

        private static List<MODataObject> Wrap(IEnumerable<DBObject> dbObjects) {
            List<MODataObject> list = new List<MODataObject>();
            foreach(DBObject dbObject in dbObjects) {
                list.Add(Wrap(dbObject));
            }
            return list;
        }


        public static int GetMaxSavedIntValue(MOAttribute moAttribute) {
            String attributeName = moAttribute.GetName();
            var dbObjects = DBService.GetInstance().GetByHasAttribute(attributeName);
            int max = 0;
            foreach(DBObject dbObject in dbObjects) {
                try {
                    max = Math.Max(max, Int32.Parse(dbObject.GetCurrentValues(attributeName)[0]));
                }
                catch(Exception e) {
                    logger.Error("Error in GetMaxSavedIntValue: ", attributeName,
                            ArrayUtils.Join(dbObject.GetCurrentValues(attributeName)), e);
                }
            }
            return max;

        }

        public static List<MODataObject> GetByAttribute(String moid, String attributeName, bool onlyReleasedData) {
            List<String> moids = GetSubclassMoids(moid);
            return Wrap(DBService.GetInstance().GetByHasAttribute(moids, attributeName, onlyReleasedData));
        }

        public static IList<MODataObject> GetByValue(String moid, String attributeName, String value, bool exact) {
            List<String> moids = GetSubclassMoids(moid);
            return Wrap(DBService.GetInstance().GetByValue(moids, attributeName, value, exact));
        }


        private static List<String> GetSubclassMoids(String moid) {
            List<String> moids = new List<String>();
            moids.AddUnique(moid);
            foreach(MOClass moClass in MOService.GetInstance().GetAllMOClasses()) {
                List<String> baseMoids = moClass.GetBaseMoids();
                if(baseMoids.Contains(moid)) {
                    moids.AddUnique(moClass.GetMoid());
                }
            }
            return moids;
        }


        internal static void GetByAttribute(MOAttribute moAttribute) {
            throw new NotImplementedException();
        }

        private static List<String> UpdateMatchStrings(DBObject dbObject, bool forceUpdate) {
            List<String> searchStrings = dbObject.GetMatchStrings();
            if(searchStrings.Count > 0 && !forceUpdate) {
                return searchStrings;
            }
            searchStrings.Clear();
            searchStrings.Add(dbObject.oid.ToString());
            IEnumerable moAttributes = MOService.GetInstance().GetMOClass(dbObject.moid).GetAllMOAttributes();
            foreach(MOAttribute moAttribute in moAttributes) {
                if(moAttribute.IsSearchable()) {
                    String[] values = dbObject.GetCurrentValues(moAttribute.GetName());
                    if(values == null) {
                        continue;
                    }
                    if(moAttribute.GetMOType().GetBaseType() is MOTypeComp) {
                        foreach(String value in values) {
                            long compOid = 0;
                            if(Int64.TryParse(value, out compOid) && compOid > 0) {
                                DBObject compDBObject = DBService.GetInstance().GetById(compOid);
                                if(compDBObject != null) {
                                    List<String> compSearchStrings = MODataObject.UpdateMatchStrings(compDBObject, false);
                                    searchStrings.AddRange(compSearchStrings);
                                }
                            }
                        }
                    } else {
                        if(ArrayUtils.IsNotEmpty(values)) {
                            foreach(String value in values) {
                                if(StringUtils.IsNotEmpty(value)) {
                                    searchStrings.Add(value.ToLower());
                                    searchStrings.Add(value);
                                    if(moAttribute.hasSearchHintNumbers()) {
                                        String nv = StringUtils.toNumbers(value);
                                        if(StringUtils.IsNotBlank(nv)) {
                                            searchStrings.Add(nv);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            logger.Debug(dbObject.oid, "searchStrings", ListUtils.Join(searchStrings));
            return searchStrings;
        }


        public static List<MODataObject> Search(MOSearchCriteria searchCriteria) {
            //DateTimeUtils.PrintTimeDiff(logger, "before search", 

            List<MODataObject> result = new List<MODataObject>();
            RatingList<MODataObject> ratingList = new RatingList<MODataObject>();
            foreach(DBObject dbObject in DBService.GetInstance().GetByDataState(searchCriteria.GetIncludedDataStates())) {
                //logger.Debug("try dbObject", dbObject);
                //logger.Debug("IncludesMoid", searchCriteria.IsMoidIncluded(dbObject.moid));
                //logger.Debug("SearchString", dbObject.Match(searchCriteria.GetOperationalSearchStrings()));

                int matchRating = 0;
                if(searchCriteria.IsMoidIncluded(dbObject.moid) &&
                    searchCriteria.IsDataStateIncluded((DataState)dbObject.stat) &&
                    searchCriteria.CheckOneExactAttribute(dbObject) &&
                    searchCriteria.CheckMultipleExactAttributes(dbObject) &&
                    searchCriteria.CheckStartsWithAttributes(dbObject) &&
                    searchCriteria.CheckContainsAttributes(dbObject) &&
                   (matchRating = Match(dbObject, searchCriteria.GetOperationalSearchStrings())) > 0) {
                    MODataObject mod = Wrap(dbObject);
                    // NEW ACCESS
                    if(MOAccess.GetInstance().CanRead(mod)) {
                        ratingList.Insert(matchRating, Wrap(dbObject));
                    }
                }
            }
            return ratingList.ToList();
        }



        private static int Match(DBObject dbObject, String[] matchStrings) {
            if(ArrayUtils.IsEmpty(matchStrings)) {
                return 1;
            }
            UpdateMatchStrings(dbObject, false);
            return dbObject.Match(matchStrings);
        }

        //
        // OBJECT LEVEL
        //

        private DBObject dbobj;
        private DBObject dbcopy;
        //
        private Dictionary<String, MODataObject> compCache = new Dictionary<String, MODataObject>();



        private MODataObject(DBObject dbobject) {
            this.dbobj = dbobject;
        }

        private MODataObject(String moid) {
            dbobj = new DBObject(moid);
        }

        private MODataObject(MOClass moclass) {
            dbobj = new DBObject(moclass.GetMoid());
        }

        //public void StartEdit() {
        //    if (IsCheckedOut(MOSystem.GetUserName())) {
        //        if (copyDBObject == null) {
        //            copyDBObject = dbObject;
        //            dbObject = (DBObject)copyDBObject.Clone();
        //        }
        //    } else {
        //        throw new Exception("Object not checked out before edit!");
        //    }
        //}

        //private bool InEdit() {
        //    return copyDBObject != null;
        //}

        //private void EndEdit() {
        //    copyDBObject = null;
        //}

        // 
        // MO
        //

        public MOClass GetMOClass() {
            return MOService.GetInstance().GetMOClass(dbobj.moid);
        }

        public String GetMoid() {
            return dbobj.moid;
        }

        public long GetOid() {
            return dbobj.oid;
        }

        public int GetLastVNR() {
            return dbobj.last_vnr;
        }

        public long GetLastModifyTime() {
            return dbobj.last_mtime;
        }

        public long GetCreationTime() {
            return dbobj.ctime;
        }

        public String GetLastUserName() {
            return DBService.GetInstance().GetUserName(dbobj.last_uid);
        }

        //
        //
        //

        public List<MODataObject> GetCurrentComps(String attributeName) {
            List<MODataObject> result = new List<MODataObject>();
            String[] currentCompOids = GetCurrentValues(attributeName);
            if(currentCompOids == null) {
                return result;
            }
            foreach(String oid in currentCompOids) {
                MODataObject mod = GetComponent(oid);
                if(mod != null) {
                    result.Add(mod);
                }
            }
            return result;
        }

        public MODataObject GetComponent(String oid) {
            MODataObject mod = compCache.Get(oid);
            if(mod == null) {
                mod = MODataObject.GetById(oid);
                if(mod != null) {
                    compCache.Add(mod.GetOid().ToString(), mod);
                }
            }
            return mod;
        }

        public void AddComponent(MODataObject mod) {
            compCache.Put(mod.GetOid().ToString(), mod);
        }

        public MODataObject GetComp(long oid) {
            return GetComponent(oid.ToString());
        }


        //
        // VALUES
        //
        public void Set(String attributeName, params String[] values) {
            if(values == null) {
                logger.Warn("Set called with null value, nothing done", GetMoid(), GetOid(), attributeName);
                return;
            }
            if(GetMOClass().IsComponent()) {
                CheckOut();
            }
            if(dbcopy == null) {
                logger.Warn("CheckOut needed before edit!", GetOid(), "of", GetMoid());
                throw new Exception("CheckOut needed before edit!");
            }
            MOAttribute moAtt = GetMOClass().GetMOAttribute(attributeName);
            if(MOAccess.GetInstance().CanWrite(moAtt)) {
                bool fey = moAtt.IsFourEyesApproval();
                if(moAtt.IsHistory()) {
                    dbcopy.UpdateAttributeValuesWithHistory(attributeName, values, MOSystem.GetUserName(), fey);
                } else {
                    dbcopy.UpdateAttributeValues(attributeName, values, MOSystem.GetUserName(), fey);
                }
            } else {
                logger.Warn("Can not write attribute " + moAtt + " without permission : " + MOSystem.GetUserName());
            }
        }

        public void Add(String attributeName, params String[] additionalValues) {
            String[] newValues = ArrayUtils.AddUnique(GetCurrentValues(attributeName), additionalValues);
            Set(attributeName, newValues);
        }

        private DBObject GetDBO() {
            return (dbcopy == null) ? dbobj : dbcopy;
        }


        public String[] GetCurrentValues(String attributeName) {
            String[] values = GetDBO().GetCurrentValues(attributeName);
            if(ArrayUtils.IsEmpty(values)) {
                MOAttribute moAtt = GetMOClass().GetMOAttribute(attributeName);
                if(moAtt != null) {
                    values = moAtt.GetDefaultValues(this);
                }
            }
            return values;
        }

        // TOJDONE
        public String[] GetBackRefValues(MOAttribute backRefAttribute) {
            return UpdateBackRefs(GetDBO(), backRefAttribute);
        }

        // TOJDONE
        private String[] UpdateBackRefs(DBObject dbObject, MOAttribute backRefAttribute) {
            //
            String[] resValues = dbObject.GetBackRefs(backRefAttribute.GetName());
            if(resValues != null) {
                return resValues;
            }
            //

            MOTypeBackRef moType = (MOTypeBackRef)backRefAttribute.GetMOType().GetBaseType();
            String moidRef = moType.GetMoidRef();
            String attRef = moType.GetAttRef();
            //isMultiple = moAttribute.GetMOType().IsMultiple();
            //
            MOSearchCriteria searchCriteria = new MOSearchCriteria();
            searchCriteria.AddIncludedDataStates(DataState.APPROVED, DataState.STORED, DataState.NEW);
            searchCriteria.AddIncludedMoids(moidRef);
            //
            MOAttribute searchAttribute = MOService.GetInstance().GetMOAttribute(moidRef, attRef);
            searchCriteria.RequestExactMatch(searchAttribute, dbObject.oid.ToString());
            List<MODataObject> res = MODataObject.Search(searchCriteria);
            List<String> resList = new List<String>();
            foreach(MODataObject m in res) {
                resList.Add(m.GetOid().ToString());
            }
            resValues = resList.ToArray();
            dbObject.UpdateBackRefs(backRefAttribute.GetName(), resValues);
            return resValues;
        }



        public String[] FormatValues(String attributeName, params String[] values) {
            if(ArrayUtils.IsEmpty(values)) {
                return null;
            }
            return GetMOClass().GetMOAttribute(attributeName).Format(values);
        }

        public String GetCurrentValue(String attributeName) {
            String[] vs = GetCurrentValues(attributeName);
            if(ArrayUtils.IsEmpty(vs)) {
                return null;
            } else {
                return vs[0];
            }
        }

        // HISTORY

        public List<MOVValue> GetHistoricalValues(String attributeName) {
            List<MOVValue> res = new List<MOVValue>();
            List<DBVValue> list = GetDBO().GetHistoricalValues(attributeName);
            if(ICollectionUtils.IsNotEmpty(list)) {
                foreach(DBVValue vv in list) {
                    res.Add(MOVValue.Create(vv));
                }
            }
            return res;
        }

        public String GetLastApprovedValue(String attributeName) {
            return (String)ArrayUtils.First(GetDBO().GetLastApprovedValues(attributeName));
        }

        //public String[] GetLastApprovedValues(String attributeName) {
        //    return GetDBO().GetLastApprovedValues(attributeName);
        //}


        public bool IsDirty() {
            return GetDBO().IsDirty();
        }

        public DataState GetDataState() {
            return (DataState)GetDBO().stat;
        }

        public String ToSynopsisString() {
            return ToSynopsisString(1024, "");
        }

        public String ToSynopsisString(int maxCols, String synopsisName) {

            StringBuilder sb = new StringBuilder();
            // List<String> list = GetMOClass().GetSynopsisUIAttributeNames();
            List<String> attributeNames = GetMOClass().GetSynopsisUIAttributeNames(synopsisName);
            foreach(String attributeName in attributeNames) {
                String[] cv = GetCurrentValues(attributeName);
                MOType ft = GetMOClass().GetMOAttribute(attributeName).GetMOType();
                String[] sa = ft.Format(cv);
                if(sb.Length > 0) {
                    sb.Append(", ");
                }
                sb.Append(ArrayUtils.Join(sa, " "));
            }
            if(!GetDataState().Equals(DataState.APPROVED) && !GetDataState().Equals(DataState.STORED)) {
                sb.Append(" (");
                sb.Append(GetDataState());
                sb.Append(")");
            }
            String s = sb.ToString();
            if(sb.Length == 0) {
                sb.Append(this.GetMOClass().GetName() + " (" + dbobj.oid + ")");
            }
            return StringUtils.CutAndEllipsis(sb.ToString(), maxCols);
        }



        public string ToSynopsisString(String synopsisName) {
            return ToSynopsisString(1024, synopsisName);
        }


        public override String ToString() {
            StringBuilder sb = new StringBuilder();
            sb.Append(GetMOClass().GetName().ToString());
            sb.Append(":");
            sb.Append(GetOid());
            sb.Append(":");
            return sb.ToString();
        }

        public bool CheckOut() {
            if(GetMOClass().IsComponent()) {
                if(dbcopy == null) {
                    dbcopy = (DBObject)dbobj.Clone();
                }
                return true;
            }
            if(!MOAccess.GetInstance().CanWrite(this)) {
                return false;
            }
            //if (RefreshCheckOut()) {
            //    return true;
            //}
            DBObject dbObject2 = DBService.GetInstance().CheckOut(dbobj, MOSystem.GetUserName());
            if(dbObject2 == null) {
                return false;
            }
            if(dbcopy == null || dbobj != dbObject2) {
                dbobj = dbObject2;
                dbcopy = (DBObject)dbobj.Clone();
                return true;
            } else {
                return true;
            }
        }

        public void CheckIn() {
            DBService.GetInstance().CheckIn(dbobj, MOSystem.GetUserName());
        }

        public String GetCheckedOutUser() {
            return DBService.GetInstance().GetCheckedOutUser(dbobj);
        }

        //
        // Data Operations
        //
        // 
        // TOJDONE
        public void Save() {
            // if (MOAccess.GetInstance().CanWrite(this) && RefreshCheckOut()) {
            if((CheckOut() && MOAccess.GetInstance().CanWrite(this)) || GetMOClass().IsComponent()) {
                MOServiceProcessor.RunServices(this, MOServiceProcessor.ONSAVE);
                // TOJDONE
                UpdateTargetBackRefValues(dbobj);
                dbcopy.Save(MOSystem.GetUserName());
                dbobj = dbcopy;
                foreach(MODataObject mod in compCache.Values) {
                    mod.Save();
                }
                UpdateMatchStrings(dbcopy, true);
                UpdateTargetBackRefValues(dbcopy);
            } else {
                logger.Warn(@"tried to write without permission 
                    or checked-out not possible: " + MOSystem.GetUserName());
            }
        }

        // 
        private void UpdateTargetBackRefValues(DBObject dbObject) {
            if(dbObject == null) {
                logger.Warn("UpdateTargetBackRefValues : unexpected null value of dbObject");
                return;
            }
            foreach(MOAttribute moa in this.GetMOClass().GetAllMOAttributes()) {
                if(moa.GetMOType().GetBaseType().GetTypeId().Equals(MOType.REF)) {
                    String[] oids = dbObject.GetCurrentValues(moa.GetName());
                    if(ArrayUtils.IsNotEmpty(oids)) {
                        foreach(String oid in oids) {
                            try {
                                long id = Int64.Parse(oid);
                                DBService.GetInstance().GetById(id).ClearAllBackRefs();
                            }
                            catch(Exception e) {
                                logger.Error(e, e);
                            }
                        }
                    }
                }
            }
        }


        public void Approve() {
            // if (MOAccess.GetInstance().CanWrite(this) && RefreshCheckOut()) {
            if(CheckOut()) {
                dbcopy.Approve(MOSystem.GetUserName());
                dbobj = DBService.GetInstance().GetById(dbobj.oid);
            } else {
                logger.Warn(@"tried to write without permission 
                    or checked-out not possible: " + MOSystem.GetUserName());
            }
            CheckIn();
        }

        // TOJDONE
        public void Delete() {
            // if (MOAccess.GetInstance().CanWrite(this) && RefreshCheckOut()) {
            if(CheckOut()) {
                UpdateTargetBackRefValues(dbobj);
                dbcopy.Delete(MOSystem.GetUserName(), GetMOClass().FourEyesApprovalNeededForDeletion());
                dbobj = dbcopy;
                UpdateTargetBackRefValues(dbcopy);
                CheckIn();
            } else {
                logger.Warn("tried to approve without permission : " + MOSystem.GetUserName());
            }
        }

        public void UndoAllChanges() {
            if(dbcopy != null) {
                dbcopy = (DBObject)dbobj.Clone();
            }
        }


        //
        // Equals
        //

        public override bool Equals(object obj) {
            if(obj != null && obj is MODataObject) {
                MODataObject m = (MODataObject)obj;
                return m.GetOid().Equals(GetOid());
            }
            return false;
        }

        public override int GetHashCode() {
            return GetOid().GetHashCode();
        }



        public String ToLongString() {
            return ToString();
        }
    }

    public class MOVValue {

        public static MOVValue Create(DBVValue dbVV) {
            return new MOVValue(dbVV);
        }

        private DBVValue dbVV;
        private MOVValue(DBVValue dbVV) {
            this.dbVV = dbVV;
        }

        public String GetUserName() {
            return DBService.GetInstance().GetUserName(dbVV.GetUid());
        }

        public String[] GetStringValues() {
            return dbVV.GetValues();
        }

        public long GetTime() {
            return dbVV.mtime;
        }

        public int GetState() {
            return dbVV.stat;
        }
        public int GetVersion() {
            return dbVV.vnr;
        }
    }

    public class MODataObjectHelper {

        private static Logger logger = Logger.GetLogger(typeof(MODataObjectHelper));

        public static ICodeTable GetCodeTable(MODataObject mod, MOTypeCodeTable ctType, bool withEmtpyElement, params String[] access) {
            if(ctType.IsChildCodeTable()) {
                String parent = ctType.GetParent();
                MOAttribute moAttribute = mod.GetMOClass().GetMOAttribute(parent);
                if(moAttribute == null) {
                    logger.Error("Parent attribute for parent codeTable does not exist.", mod.GetMOClass(), ctType);
                    return null;
                }
                MOTypeCodeTable parentCtType = (MOTypeCodeTable)moAttribute.GetMOType().GetBaseType();
                ICodeTable parentCt = GetCodeTable(mod, parentCtType, withEmtpyElement);
                if(parentCt == null) {
                    logger.Error("Parent code table does not exist. (MOClass, CTTYPE, Parent CTTYPE)", mod.GetMOClass(), ctType, parentCt);
                    return null;
                }
                String parentAttributeVale = mod.GetCurrentValue(moAttribute.GetName());
                if(StringUtils.IsBlank(parentAttributeVale)) {
                    return null;
                } else {
                    if(parentCt.GetSubCodeTable(parentAttributeVale) == null) {
                        logger.Debug("No code table found");
                        return CodeTable.EMTPY_CODE_TABLE;
                    }
                    return Filter(parentCt.GetSubCodeTable(parentAttributeVale), ctType, access);
                }
            } else {
                return Filter(CodeTable.Get(ctType.GetCodeTableName(), withEmtpyElement), ctType, access);
            }

        }




        private static ICodeTable Filter(ICodeTable ct, MOTypeCodeTable ctType, params String[] access) {
            ICodeTable nct = ct;
            if(ArrayUtils.IsNotEmpty(access) && ListUtils.IsNotEmpty(ct) && ctType.IsRoles()) {
                String[] roles = MOSystem.GetUserRoles();
                nct = (ICodeTable)ObjectUtils.Clone(ct);
                foreach(ICodeTableElement cte in ct) {
                    String code = cte.GetCode();
                    String role = code + "-" + access[0];
                    if(ArrayUtils.Contains(roles, role)) {
                    } else {
                        nct.Remove(cte);
                    }
                }
            }
            return nct;
        }

    }


    public class MOSearchCriteria {

        private class StringBool {
            internal String s;
            internal bool b;
        }
        private int pagingSize;
        private int startPagingIndex;
        private List<DataState> includedDataStates = new List<DataState>();
        private List<String> includedMoids = new List<String>();
        private List<String> operationalSearchStrings = new List<String>();
        private String searchString;
        private String title;
        private MOAttribute exactSearchAttribute;
        private String exactMatchSearchValue;
        private Dictionary<String, String[]> exactAttributeValues;
        private Dictionary<String, String> startsWithAttributeValues;
        private Dictionary<String, StringBool> containsAttributeValues;


        public void RequestExactMatch(MOAttribute exactSearchAttribute, String exactMatchSearchValue) {
            this.exactSearchAttribute = exactSearchAttribute;
            this.exactMatchSearchValue = exactMatchSearchValue;
        }

        public void AddExactMatches(String attributeName, params String[] values) {
            exactAttributeValues = exactAttributeValues == null ? exactAttributeValues = new Dictionary<String, String[]>() : exactAttributeValues;
            exactAttributeValues.Put(attributeName, values);
        }

        public void AddStartsWithMatches(String attributeName, String value) {
            startsWithAttributeValues = startsWithAttributeValues == null ? startsWithAttributeValues = new Dictionary<String, String>() : startsWithAttributeValues;
            startsWithAttributeValues.Put(attributeName, value);
        }

        public void AddContainsMatches(String attributeName, String value, bool ignoreCase) {
            containsAttributeValues = containsAttributeValues == null ? containsAttributeValues = new Dictionary<String, StringBool>() : containsAttributeValues;
            containsAttributeValues.Put(attributeName, new StringBool() { s = value, b = ignoreCase });
        }

        public void SetTitle(String title) {
            this.title = title;
        }

        public String GetTitle() {
            return this.title;
        }

        public void FillInSearchString(String searchString) {
            this.searchString = searchString;
            operationalSearchStrings.Clear();
            if(searchString.Length != 0) {
                this.searchString = searchString.Trim();
                operationalSearchStrings.Add(this.searchString);
                String[] sa = this.searchString.Split(',', ';', '\t', ' ');
                foreach(String s in sa) {
                    String s2 = s.Trim();
                    if(StringUtils.IsNotEmpty(s)) {
                        operationalSearchStrings.Add(s2);
                        operationalSearchStrings.Add(s2.ToLower());
                    }
                }
            }
        }

        public void AddIncludedDataStates(params DataState[] states) {
            foreach(DataState state in states) {
                includedDataStates.AddUnique(state);
            }
        }

        public void SetIncludedDataStates(params DataState[] states) {
            includedDataStates.Clear();
            AddIncludedDataStates(states);
        }

        public void AddIncludedMoids(params String[] moids) {
            foreach(String moid in moids) {
                includedMoids.AddUnique(moid);
            }
        }

        public void AddAllTopLevelMoid() {
            foreach(MOClass moClass in MOService.GetInstance().GetAllMOClasses()) {
                if(moClass.IsTopLevel()) {
                    AddIncludedMoids(moClass.GetMoid());
                }
            }
        }


        public void Clear() {
            includedDataStates.Clear();
            includedMoids.Clear();
            operationalSearchStrings.Clear();
            exactAttributeValues = null;
        }

        public DataState[] GetIncludedDataStates() {
            return includedDataStates.ToArray();
        }

        public bool IsMoidIncluded(String moid) {
            return includedMoids.Contains(moid);
        }

        public IEnumerable<String> GetIncludedMoids() {
            return includedMoids;
        }

        public String[] GetOperationalSearchStrings() {
            return operationalSearchStrings.ToArray();
        }

        public String GetSearchString() {
            return StringUtils.RNN(searchString);
        }

        public override String ToString() {
            StringBuilder sb = new StringBuilder();
            sb.Append("[ searchStrings=");
            sb.Append(ArrayUtils.Join(this.operationalSearchStrings.ToArray()));
            sb.Append("; includedMoids=");
            sb.Append(ArrayUtils.Join(this.includedMoids.ToArray()));
            sb.Append("; includedDataStates=");
            String[] ds = EnumUtils.ToStringArray(this.includedDataStates);
            sb.Append(ArrayUtils.Join(ds));
            sb.Append("]");
            return sb.ToString();
        }

        public bool IsDataStateIncluded(DataState dataState) {
            return includedDataStates.Contains(dataState);
        }

        internal bool CheckOneExactAttribute(DBObject dbObject) {
            if(this.exactSearchAttribute == null || StringUtils.IsEmpty(this.exactMatchSearchValue)) {
                return true;
            }
            String[] values = dbObject.GetApprovedValues(exactSearchAttribute.GetName());
            if(ArrayUtils.IsNotEmpty(values)) {
                foreach(String v in values) {
                    if(this.exactMatchSearchValue.Equals(v)) {
                        return true;
                    }
                }
            }
            return false;
        }

        internal bool CheckMultipleExactAttributes(DBObject dbObject) {
            if(exactAttributeValues == null) {
                return true;
            }
            foreach(String attributeName in exactAttributeValues.Keys) {
                String[] values = dbObject.GetCurrentValues(attributeName);
                if(!ArrayUtils.AreEquals(values, exactAttributeValues.Get(attributeName))) {
                    return false;
                }
            }
            return true;
        }

        internal bool CheckStartsWithAttributes(DBObject dbObject) {
            if(startsWithAttributeValues == null) {
                return true;
            }
            foreach(String attributeName in startsWithAttributeValues.Keys) {
                String[] values = dbObject.GetCurrentValues(attributeName);
                if(!ArrayUtils.OneStartsWith(values, startsWithAttributeValues.Get(attributeName))) {
                    return false;
                }
            }
            return true;
        }

        internal bool CheckContainsAttributes(DBObject dbObject) {
            if(containsAttributeValues == null) {
                return true;
            }
            foreach(String attributeName in containsAttributeValues.Keys) {
                String[] values = dbObject.GetCurrentValues(attributeName);
                if(!ArrayUtils.OneContains(values, containsAttributeValues.Get(attributeName).s, containsAttributeValues.Get(attributeName).b)) {
                    return false;
                }
            }
            return true;
        }

        public void SetStartPagingIndex(int index) {
            this.startPagingIndex = index;
        }

        public int GetStartPagingIndex() {
            return this.startPagingIndex;
        }

        public void SetPageSize(int pagingSize) {
            this.pagingSize = pagingSize;
        }

        public int GetPageSize() {
            return pagingSize;
        }
    }

    public static class MOServiceProcessor {

        public static readonly String ONNEW = "onnew";
        public static readonly String ONSAVE = "onsave";

        public delegate void processServices(MODataObject moDataObject, MOAttribute moAttribute);

        private static Dictionary<String, processServices> processors = new Dictionary<String, processServices>();

        public static void RegisterService(String name, String status, processServices service) {
            processors.Put(CreateProcessorsKey(name, status), service);
        }

        static MOServiceProcessor() {
            RegisterService("CreationDateTime", ONNEW, CreationDateTime);
            RegisterService("UniqueAttribueNumber", ONSAVE, UniqueAttribueNumber);
        }

        public static void CreationDateTime(MODataObject moDataObject, MOAttribute moAttribute) {
            String[] values = moDataObject.GetCurrentValues(moAttribute.GetName());
            if(ArrayUtils.IsEmpty(values)) {
                DateTime dt = DateTime.Now;
                moDataObject.Set(moAttribute.GetName(), String.Format("{0:yyyy-MM-dd HH:mm}", dt));
            }
        }

        public static void UniqueAttribueNumber(MODataObject moDataObject, MOAttribute moAttribute) {
            String[] vs = moDataObject.GetCurrentValues(moAttribute.GetName());
            String v = "";
            if(ArrayUtils.IsNotEmpty(vs)) {
                v = vs[0];
            }
            int r = 0;
            if(Int32.TryParse(v, out r)) {
                return;
            }
            int max = MODataObject.GetMaxSavedIntValue(moAttribute);
            max++;
            moDataObject.Set(moAttribute.GetName(), max.ToString());
        }

        public static MODataObject RunServices(MODataObject moDataObject, String processStatus) {
            foreach(MOAttribute moAtt in moDataObject.GetMOClass().GetAllMOAttributes()) {
                String serviceName = moAtt.GetServiceName();
                String[] status = moAtt.GetServiceStatus();
                if(serviceName != null) {
                    foreach(String stat in status) {
                        if(StringUtils.EqualsIgnoreCase(stat, processStatus)) {
                            var f = processors.Get(CreateProcessorsKey(moAtt.GetServiceName(), stat));
                            if(f != null) {
                                f(moDataObject, moAtt);
                            }
                        }
                    }
                }
            }
            return moDataObject;
        }

        private static String CreateProcessorsKey(String serviceName, String processState) {
            return serviceName.ToLower() + "-" + processState.ToLower();
        }

    }


    public static class MO {
        public static class AttName {
            public static readonly String attref = "attref";
            public static readonly String attpath = "attpath";
            public static readonly String moid = "moid";
            public static readonly String name = "name";
            public static readonly String uihints = "uihints";
            public static readonly String widgetHint = "widgethint";
            public static readonly String searchHints = "searchHints";
        }
        public static class AttValue {
            public static readonly String numbering = "numbering";
            public static readonly String numbers = "numbers";
        }
    }

    public static class Workaround {
        public static bool CheckOutIsOKIfComponent(MOClass moClass) {
            return moClass.IsComponent();
        }
    }



    public class UIHints {

        public static readonly String WIDTH = "width";

        public static UIHints Create(String uiHintsString) {
            if(StringUtils.IsNotEmpty(uiHintsString)) {
                return new UIHints(uiHintsString);
            } else {
                return null;
            }
        }

        //

        private Dictionary<String, String> uiHints = new Dictionary<string, string>();

        private UIHints(String uiHintsString) {
            String[] uihs = uiHintsString.Split(';');
            foreach(String s in uihs) {
                String s1 = s.Trim().ToLower();
                String[] ss2 = s1.Split(':');
                if(ss2.Length == 1) {
                    uiHints.Put(ss2[0].Trim(), "");
                }
                if(ss2.Length == 2) {
                    uiHints.Put(ss2[0].Trim(), ss2[1].Trim());
                }
            }
        }

        public bool HasKey(String uiHintKey) {
            if(StringUtils.IsNotEmpty(uiHintKey)) {
                return uiHints.ContainsKey(uiHintKey.ToLower());
            } else {
                return false;
            }
        }
        public String GetValue(String uiHintKey) {
            if(StringUtils.IsNotEmpty(uiHintKey)) {
                return uiHints.Get(uiHintKey.ToLower());
            } else {
                return null;
            }
        }
    }

}