//
// Copyright (C) 2008 Vitra AG, Klünenfeldstrasse 22, Muttenz, 4127 Birsfelden
// All rights reserved.
//
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.DirectoryServices;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web.Security;
using System.Xml;
using Com.OOIT.VIPS.System;
using Org.JGround.MOM;
using Org.JGround.MOM.DB;
using Org.JGround.Util;
using Org.JGround.Web;
using Org.JGround.Codetable;

namespace Com.OOIT.VIPS.Admin {

    public class VipsDeadlineService {

        private static Logger logger = Logger.GetLogger(typeof(VipsDeadlineService));

        public static readonly String DEADLINE_PROCESS_USER = "DEADLINE_PROCESS_USER";
        public static readonly String DEADLINE_PROCESS_ROLE = "DEADLINE_PROCESS_ROLE";

        public static String DEADLINE_FILE { get; set; }

        private static VipsDeadlineService instance;

        public static VipsDeadlineService GetInstance() {
            return instance == null ? instance = new VipsDeadlineService() : instance;
        }

        //
        // OBJECT LEVEL
        //

        private List<DeadLine> deadlines = new List<DeadLine>();

        private VipsDeadlineService() {
            if(DEADLINE_FILE == null || !File.Exists(DEADLINE_FILE)) {
                logger.Error("Cannot initialize VipsDeadlineService: file not found: " + DEADLINE_FILE);
                return;
            }
            XmlDocument doc = new XmlDocument();
            doc.Load(DEADLINE_FILE);
            // DEADLINE
            foreach(XmlElement deadlineElement in doc.DocumentElement.GetElementsByTagName("deadline")) {
                deadlines.Add(new DeadLine(deadlineElement));
            }
        }

        public static int TIMER_INTERVAL_IN_SECONDS = 60 * 60;

        private Timer deadlineTimer;

        public void Startup() {
            deadlineTimer = new Timer(CallBack, "-", 10000, TIMER_INTERVAL_IN_SECONDS * 1000);
        }

        public void Shutdown() {
            deadlineTimer.Dispose();
        }



        public void CallBack(object obj) {
            try {
                lock(Locker.MUTEX) {
                    ProcessDeadLines();
                }
                MailService.GetInstance().SendAllMails();
            }
            catch(Exception e) {
                logger.Error(e, e);
            }
        }

        public void ProcessDeadLines() {
            logger.Debug("CallBack of VipsDeadlineService starts ...");
            //
            // User and Role Switch
            //
            String currentUser = VIPSSystem.GetUser();
            String[] currentRoles = VIPSSystem.GetRoles();
            VIPSSystem.RegisterUserOnThread(VipsDeadlineService.DEADLINE_PROCESS_USER);
            VIPSSystem.RegisterRolesOnThread(VipsDeadlineService.DEADLINE_PROCESS_ROLE);
            //
            // Iterate Deadlines
            foreach(DeadLine deadline in deadlines) {
                // find all data to check
                String moid = deadline.Moid;
                String dateAttRef = deadline.Attref;
                List<MODataObject> mods = MODataObject.GetByAttribute(moid, dateAttRef, true);
                //
                foreach(MODataObject mod in mods) {
                    //
                    String dateValue = mod.GetCurrentValue(dateAttRef);
                    String oidRef = mod.GetOid().ToString();
                    if(StringUtils.IsEmpty(dateValue)) {
                        continue;
                    }
                    if(DateTimeUtils.TryParse(dateValue) == false) {
                        logger.Warn("Could not process Alerting for", mod, "Attribute value is not a Date", dateAttRef, dateValue);
                        continue;
                    }
                    long dueTimeMs = DateTimeUtils.TicksToMillis(DateTimeUtils.Parse(dateValue).Ticks);
                    // check all alerts
                    foreach(Alert alert in deadline.alerts) {
                        String beforeInDays = alert.BeforeInDays.ToString();
                        long beforeMs = alert.BeforeInMillis;
                        long nowMs = DateTimeUtils.TicksToMillis(DateTime.Now.Ticks);
                        List<String> tos = new List<string>(alert.Tos);
                        bool notifyNeeded = (dueTimeMs - beforeMs) < nowMs && nowMs < dueTimeMs;
                        if(notifyNeeded) {
                            try {
                                // check if there is already an alert log entry      
                                //
                                MOSearchCriteria alertLogsFilter = new MOSearchCriteria();
                                alertLogsFilter.SetIncludedDataStates(DataState.STORED, DataState.APPROVED);
                                alertLogsFilter.AddIncludedMoids(DEF.SYSAlertLog.moid);
                                alertLogsFilter.AddExactMatches(DEF.SYSAlertLog.oidRef, oidRef);
                                alertLogsFilter.AddExactMatches(DEF.SYSAlertLog.dateAttRef, dateAttRef);
                                alertLogsFilter.AddExactMatches(DEF.SYSAlertLog.beforeInDays, beforeInDays);
                                alertLogsFilter.AddExactMatches(DEF.SYSAlertLog.dateValue, dateValue);
                                List<MODataObject> alertLogs = MODataObject.Search(alertLogsFilter);
                                MODataObject alertLog = null;
                                if(alertLogs.Count() == 0) {
                                    alertLog = MODataObject.Create(DEF.SYSAlertLog.moid);
                                    alertLog.CheckOut();
                                    alertLog.Set(DEF.SYSAlertLog.oidRef, oidRef);
                                    alertLog.Set(DEF.SYSAlertLog.dateAttRef, dateAttRef);
                                    alertLog.Set(DEF.SYSAlertLog.beforeInDays, beforeInDays);
                                    alertLog.Set(DEF.SYSAlertLog.dateValue, dateValue);
                                    alertLog.Set(DEF.SYSAlertLog.name, "Alert for: " + mod.ToString());
                                } else if(alertLogs.Count() == 1) {
                                    alertLog = alertLogs[0];
                                } else {
                                    logger.Warn("Found more than one SYSAlertLog object for", mod,
                                                "dateValue", dateValue, "deadline", deadline);
                                    continue;
                                }
                                if(alertLog.CheckOut()) {
                                    String[] alertLogEntriesOids = alertLog.GetCurrentValues(DEF.SYSAlertLog.alertlogentries);
                                    List<String> tosCollected = new List<string>();
                                    if(ArrayUtils.IsNotEmpty(alertLogEntriesOids)) {
                                        foreach(String alertLogEntryOid in alertLogEntriesOids) {
                                            MODataObject alertLogEntry = MODataObject.GetById(alertLogEntryOid);
                                            String sendtime = alertLogEntry.GetCurrentValue(DEF.SYSAlertLogEntry.sendtime);
                                            String[] tosDone = alertLogEntry.GetCurrentValues(DEF.SYSAlertLogEntry.tos);
                                            tosCollected = ListUtils.UnifyUnique(tosCollected, tosDone);
                                        }
                                    }
                                    List<String> tosDiff = tos.Subtract(tosCollected);
                                    if(ICollectionUtils.IsNotEmpty(tosDiff)) {
                                        StringBuilder buff = new StringBuilder(alert.Text);
                                        buff.AppendLine();
                                        buff.AppendLine("Origin of Alert: " + mod.ToSynopsisString());
                                        buff.AppendLine("Date Attribute: " + MOService.GetInstance().GetMOClass(mod.GetMoid()).GetMOAttribute(dateAttRef).GetLabel()); ;
                                        buff.AppendLine("Object Id: " + mod.GetOid());
                                        buff.AppendLine("Date Value: " + dateValue); ;
                                        buff.AppendLine();
                                        buff.AppendLine("This message was sent by the Alert Timer of VITRA Intellectual Property System (VIPS)");
                                        String text = buff.ToString();
                                        if(MailService.GetInstance().AddMail(tosDiff, alert.Subject, text, false)) {
                                            logger.Info("Mail sent", tosDiff.ToArray(), alert.Subject, text);
                                            MODataObject newAlertLogEntry = MODataObject.Create(DEF.SYSAlertLogEntry.moid);
                                            newAlertLogEntry.Set(DEF.SYSAlertLogEntry.sendtime, DateTimeUtils.FormatDate(DateTime.Now));
                                            newAlertLogEntry.Set(DEF.SYSAlertLogEntry.subject, alert.Subject);
                                            newAlertLogEntry.Set(DEF.SYSAlertLogEntry.text, text);
                                            newAlertLogEntry.Set(DEF.SYSAlertLogEntry.tos, tosDiff.ToArray());
                                            alertLog.Add(DEF.SYSAlertLog.alertlogentries, newAlertLogEntry.GetOid().ToString());
                                            alertLog.AddComponent(newAlertLogEntry);
                                            alertLog.Save();
                                        } else {
                                            logger.Warn("Could not send mail", alertLog.ToLongString(), tosDiff);
                                        }
                                    }
                                }
                                alertLog.CheckIn();
                            }
                            catch(Exception e) {
                                logger.Error("While processing alert ", alert, e);
                            }
                        }
                    }
                }
            }
            //
            // User and Role Switch Back
            //
            VIPSSystem.RegisterUserOnThread(currentUser);
            VIPSSystem.RegisterRolesOnThread(currentRoles);
            //
            logger.Debug("CallBack of VipsDeadlineService ends ...");
        }
    }

    public class DeadLine {

        public string Moid { set; get; }
        public string Attref { set; get; }
        public List<Alert> alerts = new List<Alert>();

        public DeadLine(XmlElement deadlineElement) {
            Moid = deadlineElement.GetAttributeValue(MO.AttName.moid);
            Attref = deadlineElement.GetAttributeValue(MO.AttName.attref);
            foreach(XmlElement alertElement in deadlineElement.GetElementsByTagName("alert")) {
                alerts.Add(new Alert(alertElement));
            }
        }

    }
    public class Alert {

        public enum AlertLevel { INFO = 1, WARN = 2 };
        public AlertLevel Level { set; get; }
        public List<String> Tos { get { return tos; } }
        public string Subject { set; get; }
        public string Text { set; get; }
        public long BeforeInMillis { get { return beforeInMillis; } }
        public int BeforeInDays { get { return beforeInDays; } }
        private long beforeInMillis;
        private int beforeInDays;
        private List<String> tos;

        public Alert(XmlElement alertElement) {
            Level = (AlertLevel)Enum.Parse(typeof(AlertLevel), alertElement.GetAttributeValue("level").ToUpper());
            beforeInDays = Int32.Parse(alertElement.GetAttributeValue("beforeInDays"));
            beforeInMillis = beforeInDays * 24 * 60 * 60 * 1000L;
            tos = new List<string>();
            foreach(XmlElement toElement in alertElement.GetElementsByTagName("to")) {
                tos.Add(toElement.InnerText);
            }
            Subject = alertElement.GetTextByTagName("subject");
            Text = alertElement.GetTextByTagName("text");
        }

    }





    public abstract class CustomActiveDirectoryRoleProvider : RoleProvider {

        private string _loginProperty = "sAMAccountName";
        private string _connectionString = string.Empty;
        private string _applicationName = string.Empty;

        public override void Initialize(string name, NameValueCollection config) {
            _connectionString = config["connectionStringName"];
            _applicationName = config["applicationName"];
            if(!string.IsNullOrEmpty(config["attributeMapUsername"]))
                _loginProperty = config["attributeMapUsername"];
            base.Initialize(name, config);
        }


        public override string[] GetRolesForUser(string userName) {
            List<String> allRoles = new List<String>();
            DirectoryEntry root = new DirectoryEntry(ConfigurationManager.ConnectionStrings["vipsConnectionString"].ConnectionString);
            // WebConfigurationManager.ConnectionStrings[_connectionString].ConnectionString
            foreach(DirectoryEntry entry in root.Children) {
                if(entry.SchemaClassName.ToLower() == "group") {
                    object members = entry.Invoke("Members", null);
                    foreach(object member in (IEnumerable)members) {
                        DirectoryEntry child = new DirectoryEntry(member);

                        if(_getProperty(child, _loginProperty) == userName) {
                            string name = _getProperty(entry, "name");
                            allRoles.Add(name != "" ? name : entry.Name);
                        }
                    }
                }
            }
            return allRoles.ToArray();
        }

        private string _getProperty(DirectoryEntry entry, string propertyName) {
            if((entry.Properties[propertyName] != null) &&
                (entry.Properties[propertyName].Value != null)) {
                return entry.Properties[propertyName].Value.ToString();
            }
            return "";
        }
    }



    public class VipsCompanyGroupHelper //: MOAccessAllTrue 
    {

        private static Logger logger = Logger.GetLogger(typeof(VipsCompanyGroupHelper));

        public static void OnNewCompanyGroupInit(MODataObject mod, MOAttribute moAttribute) {
            logger.Debug("processing OnNewCompanyGroupInit");
            if (mod.GetMoid().EndsWith("Product")){
                logger.Debug("Product!!");
            }
            MOAttribute cgAtt = mod.GetMOClass().GetMOAttribute(DEF.CompanyGroup.companyGroup);
            if(cgAtt == null) {
                return;
            }

            String[] writeCodes = VipsCompanyGroupHelper.GetInstance().GetWriteCGCodes();
            if(writeCodes.Length == 1) {
                if(mod.CheckOut()) {
                    mod.Set(cgAtt.GetName(), writeCodes);
                }
            }
        }

        private static VipsCompanyGroupHelper instance;

        public static VipsCompanyGroupHelper GetInstance() {
            return instance == null ? instance = new VipsCompanyGroupHelper() : instance;
        }

        //private String roleAttributeName = "companyGroup";
        private ICodeTable companyGroupCT;
        private String[] companyGroupCodes;

        private VipsCompanyGroupHelper() {
            companyGroupCT = CodeTable.Get(DEF.CODETABLE.COMPANYGROUP, false);
            companyGroupCodes = new String[companyGroupCT.Count];
            int i = 0;
            foreach(CodeTableElement cte in companyGroupCT) {
                companyGroupCodes[i++] = cte.GetCode();
            }
        }

        public String[] GetReadCGCodes() {
            List<String> codes = new List<String>();
            foreach(String code in companyGroupCodes) {
                String roleName = GetReadRoleName(code);
                if(HasRole(roleName)) {
                    codes.Add(code);
                }
            }
            return codes.ToArray();
        }

        public String[] GetWriteCGCodes() {
            List<String> codes = new List<String>();
            foreach(String code in companyGroupCodes) {
                String roleName = GetWriteRoleName(code);
                if(HasRole(roleName)) {
                    codes.Add(code);
                }
            }
            return codes.ToArray();
        }


        private String GetReadRoleName(String companyGroupCode) {
            return companyGroupCode + MOAccess.READ_POSTFIX;
        }

        private String GetWriteRoleName(String companyGroupCode) {
            return companyGroupCode + MOAccess.WRITE_POSTFIX;
        }

        private bool HasRole(String roleName) {
            String[] userRoles = MOSystem.GetUserRoles();
            return ArrayUtils.Contains(userRoles, roleName);
        }


        //public override bool CanRead(MODataObject moDataObject) {
        //    String companyGroupCode = moDataObject.GetCurrentValue(roleAttributeName);
        //    if(StringUtils.IsBlank(companyGroupCode)) {
        //        return true;
        //    }
        //    String roleName = GetReadRoleName(companyGroupCode);
        //    return HasRole(roleName);
        //}

        //public override bool CanWrite(MODataObject moDataObject) {
        //    String companyGroupCode = moDataObject.GetCurrentValue(roleAttributeName);
        //    if(StringUtils.IsBlank(companyGroupCode)) {
        //        return true;
        //    }
        //    String roleName = GetWriteRoleName(companyGroupCode);
        //    return HasRole(roleName);
        //}

        //public override bool CanApprove(MODataObject moDataObject) {
        //    return CanWrite(moDataObject);
        //}





    }
}