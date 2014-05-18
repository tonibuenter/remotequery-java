//
// Copyright (C) 2008 OOIT.com AG, Zürich CH
// All rights reserved.
//
using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Transactions;
using jground;
using Org.JGround.Util;
using Com.OOIT.VIPS;


namespace Org.JGround.MOM.DB {

    //public class Constants {
    //    //public const String ASSEMBLY_DIR = ;
    //}

    public enum DataState { NOTSET = 0, NEW = 1, STORED = 2, APPROVED = 3, DELETED_UNAPPROVED = 4, DELETED = 5 };

    public class DataStateUtils {
        public static HashSet<DataState> AsDataStates(params String[] dataStateStrings) {
            HashSet<DataState> dataStates = new HashSet<DataState>();
            if(ArrayUtils.IsNotEmpty(dataStateStrings)) {
                foreach(String stat in dataStateStrings) {
                    DataState ds = (DataState)Enum.Parse(typeof(DataState), stat, true);
                    dataStates.Add(ds);
                }
            }
            return dataStates;
        }
    }


    /// <summary>
    /// DBService offers methods for reading and writing to the database, 
    /// convert names back and forward to ids ,such as attribute names and user names, and
    /// created database entries for the MO data objects and DEF files.
    /// </summary>
    public class DBService {
        //
        // CLASS LEVEL
        //
        public static long CHECK_OUT_TIMEOUT_MILLIS = 30 * 60 * 1000;
        //
        private static Logger logger = Logger.GetLogger(typeof(DBService));
        //
        private static String connectionString;
        private static DBService instance = null;

        /// <summary>
        /// Initialize the DBService by analysing new MOM Data (MOATT) and loading instance data (TUSR, DBOBJ, DBVVAL, DBVAL).
        /// Be aware, this method is not thread-safe!
        /// </summary>
        /// <param name="connectionString"></param>
        public static void Initialize(string connectionString) {
            if(instance != null) {
                logger.Warn("Service already initialized. {NO ACTION TAKEN}.");
                return;
            }
            DBService.connectionString = connectionString;
            instance = new DBService();
            instance.Init();
        }

        public static DBService GetInstance() {
            return instance;
        }

        /// <summary>
        /// Save the complete DB to a file.
        /// </summary>
        public static void SaveDBToFile(String filePath, String connectionString) {
            using(StreamWriter w = new StreamWriter(File.Open(filePath, FileMode.Create)))
            using(var dataContext = GetDataContext(connectionString)) {
                Table<TUSR> tusrs = dataContext.TUSRs;
                foreach(TUSR tusr in tusrs) {
                    ToLineString ts = new ToLineString();
                    ts.Add(tusr.uid);
                    ts.Add(tusr.userName);
                    ts.WriteLine(w);
                }
                Table<MOATT> moatts = dataContext.MOATTs;
                foreach(MOATT moatt in moatts) {
                    ToLineString ts = new ToLineString();
                    ts.Add(moatt.aid);
                    ts.Add(moatt.moid);
                    ts.Add(moatt.attributeName);
                    ts.WriteLine(w);
                }
                Table<DBOBJ> dbobjs = dataContext.DBOBJs;
                foreach(DBOBJ dbobj in dbobjs) {
                    ToLineString ts = new ToLineString();
                    ts.Add(dbobj.moid);
                    ts.Add(dbobj.oid);
                    ts.Add(dbobj.stat);
                    ts.Add(dbobj.ctime);
                    ts.Add(dbobj.last_vnr);
                    ts.Add(dbobj.last_mtime);
                    ts.Add(dbobj.last_uid);
                    ts.WriteLine(w);
                }
                Table<DBVVAL> dbvvals = dataContext.DBVVALs;
                foreach(DBVVAL dbvval in dbvvals) {
                    ToLineString ts = new ToLineString();
                    ts.Add(dbvval.oid);
                    ts.Add(dbvval.aid);
                    ts.Add(dbvval.vnr);
                    ts.Add(dbvval.mtime);
                    ts.Add(dbvval.stat);
                    ts.Add(dbvval.uid);
                    ts.WriteLine(w);
                }
                Table<DBVAL> dbvals = dataContext.DBVALs;
                foreach(DBVAL dbval in dbvals) {
                    ToLineString ts = new ToLineString();
                    ts.Add(dbval.oid);
                    ts.Add(dbval.aid);
                    ts.Add(dbval.vnr);
                    ts.Add(dbval.indx);
                    ts.Add(dbval.stringValue);
                    ts.WriteLine(w);
                }
            }
        }




        /// <summary>
        /// Load file into DB
        /// </summary>
        public static void LoadFileToDB(String filePath, String connectionString) {
            int counter = 0;
            using(StreamReader r = new StreamReader(filePath))
            using(var dataContext = GetDataContext(connectionString)) {
                DateTimeUtils.StartTime("LoadFileToDB-DeleteAll");
                dataContext.DBVVALs.DeleteAllOnSubmit(dataContext.DBVVALs);
                dataContext.DBOBJs.DeleteAllOnSubmit(dataContext.DBOBJs);
                dataContext.DBVALs.DeleteAllOnSubmit(dataContext.DBVALs);
                dataContext.MOATTs.DeleteAllOnSubmit(dataContext.MOATTs);
                dataContext.TUSRs.DeleteAllOnSubmit(dataContext.TUSRs);
                dataContext.SubmitChanges();
                DateTimeUtils.LogTime("LoadFileToDB-DeleteAll");
                DateTimeUtils.StartTime("LoadFileToDB-InsertALL");
                String line = null;
                while((line = r.ReadLine()) != null) {
                    logger.Info("Import Line: " + line);
                    counter++;
                    FromLineString ls = new FromLineString(line);
                    switch(ls.Count()) {
                        case 2:
                            TUSR tusr = new TUSR {
                                uid = ls.GetInt(),
                                userName = ls.GetString()
                            };
                            dataContext.TUSRs.InsertOnSubmit(tusr);
                            //dataContext.SubmitChanges();
                            break;
                        case 3:
                            MOATT moatt = new MOATT {
                                aid = ls.GetInt(),
                                moid = ls.GetString(),
                                attributeName = ls.GetString()
                            };
                            dataContext.MOATTs.InsertOnSubmit(moatt);
                            //dataContext.SubmitChanges();
                            break;
                        case 7:
                            DBOBJ dbobj = new DBOBJ {
                                moid = ls.GetString(),
                                oid = ls.GetLong(),
                                stat = ls.GetInt(),
                                ctime = ls.GetLong(),
                                last_vnr = ls.GetInt(),
                                last_mtime = ls.GetLong(),
                                last_uid = ls.GetInt()
                            };
                            dataContext.DBOBJs.InsertOnSubmit(dbobj);
                            //dataContext.SubmitChanges();
                            break;
                        case 6:
                            DBVVAL dbvval = new DBVVAL {
                                oid = ls.GetLong(),
                                aid = ls.GetInt(),
                                vnr = ls.GetInt(),
                                mtime = ls.GetLong(),
                                stat = ls.GetInt(),
                                uid = ls.GetInt()
                            };
                            dataContext.DBVVALs.InsertOnSubmit(dbvval);
                            //dataContext.SubmitChanges();
                            break;
                        case 5:
                            DBVAL dbval = new DBVAL {
                                oid = ls.GetLong(),
                                aid = ls.GetInt(),
                                vnr = ls.GetInt(),
                                indx = ls.GetInt(),
                                stringValue = ls.GetString()
                            };
                            dataContext.DBVALs.InsertOnSubmit(dbval);
                            //dataContext.SubmitChanges();
                            break;
                    }
                }
                dataContext.SubmitChanges();
                DateTimeUtils.LogTime("LoadFileToDB-InsertALL");
            }
            logger.Info("Load into DB number of lines", counter);
        }
        /// <summary>
        /// Load file into DB
        /// </summary>
        public static void LoadFileToDB2(String filePath, String connectionString) {
            int counter = 0;
            using(StreamReader r = new StreamReader(filePath))
            using(var dataContext = GetDataContext(connectionString)) {

                dataContext.DBVVALs.DeleteAllOnSubmit(dataContext.DBVVALs);
                dataContext.DBOBJs.DeleteAllOnSubmit(dataContext.DBOBJs);
                dataContext.DBVALs.DeleteAllOnSubmit(dataContext.DBVALs);
                dataContext.MOATTs.DeleteAllOnSubmit(dataContext.MOATTs);
                dataContext.TUSRs.DeleteAllOnSubmit(dataContext.TUSRs);
                dataContext.SubmitChanges();

                String line = null;
                while((line = r.ReadLine()) != null) {
                    logger.Info("Import Line: " + line);
                    counter++;
                    FromLineString ls = new FromLineString(line);
                    switch(ls.Count()) {
                        case 2:
                            TUSR tusr = new TUSR {
                                uid = ls.GetInt(),
                                userName = ls.GetString()
                            };
                            dataContext.TUSRs.InsertOnSubmit(tusr);
                            dataContext.SubmitChanges();
                            break;
                        case 3:
                            MOATT moatt = new MOATT {
                                aid = ls.GetInt(),
                                moid = ls.GetString(),
                                attributeName = ls.GetString()
                            };
                            dataContext.MOATTs.InsertOnSubmit(moatt);
                            dataContext.SubmitChanges();
                            break;
                        case 7:
                            DBOBJ dbobj = new DBOBJ {
                                moid = ls.GetString(),
                                oid = ls.GetLong(),
                                stat = ls.GetInt(),
                                ctime = ls.GetLong(),
                                last_vnr = ls.GetInt(),
                                last_mtime = ls.GetLong(),
                                last_uid = ls.GetInt()
                            };
                            dataContext.DBOBJs.InsertOnSubmit(dbobj);
                            dataContext.SubmitChanges();
                            break;
                        case 6:
                            DBVVAL dbvval = new DBVVAL {
                                oid = ls.GetLong(),
                                aid = ls.GetInt(),
                                vnr = ls.GetInt(),
                                mtime = ls.GetLong(),
                                stat = ls.GetInt(),
                                uid = ls.GetInt()
                            };
                            dataContext.DBVVALs.InsertOnSubmit(dbvval);
                            dataContext.SubmitChanges();
                            break;
                        case 5:
                            DBVAL dbval = new DBVAL {
                                oid = ls.GetLong(),
                                aid = ls.GetInt(),
                                vnr = ls.GetInt(),
                                indx = ls.GetInt(),
                                stringValue = ls.GetString()
                            };
                            dataContext.DBVALs.InsertOnSubmit(dbval);
                            dataContext.SubmitChanges();
                            break;
                    }
                }
            }
            logger.Info("Load into DB number of lines", counter);
        }

        public static void Shutdown() {
            instance = null;
        }

        //
        // OBJECT LEVEL
        //

        private long maxOid;
        // private Hash
        private Dictionary<long, DBObject> dbObjects = new Dictionary<long, DBObject>();
        /// <summary>
        /// Creates a unique object id.
        /// </summary>
        /// <returns>New Object Id minimum 1000</returns>
        public long NewOid() {
            ++maxOid;
            return maxOid;
        }

        private int maxAid = 1;
        private Dictionary<String, Dictionary<String, int>> moidAtt2aid = new Dictionary<String, Dictionary<String, int>>();
        private Dictionary<int, String[]> aid2moidAtt = new Dictionary<int, String[]>();
        public int NewAid() {
            ++maxAid;
            return maxAid;
        }
        // 
        private int maxUid = 1;
        private Dictionary<String, int> user2uid = new Dictionary<String, int>();
        private Dictionary<int, String> uid2user = new Dictionary<int, String>();
        public int NewUid() {
            ++maxUid;
            return maxUid;
        }

        private MOService moService;
        private DBService() {
            moService = MOService.GetInstance();
        }

        private void Init() {
            LoadAndSynchMOATT();
            LoadDB();
            LoadUSR();
        }
        //
        // MOATT
        //
        private void LoadAndSynchMOATT() {
            LoadMOATT();
            using(var dataContext = GetDataContext()) {
                Table<MOATT> moAttTable = dataContext.GetTable<MOATT>();
                foreach(MOClass moClass in moService.GetAllMOClasses()) {
                    String moid = moClass.GetMoid();
                    foreach(MOAttribute moAtt in moClass.GetAllMOAttributes()) {
                        if(GetAid(moid, moAtt.GetName()) == -1) {
                            if(dataContext.MOATTs.Count(o => o.attributeName == moAtt.GetName() && o.moid == moAtt.GetMoid()) == 0) {
                                MOATT newMOATT = new MOATT { aid = NewAid(), moid = moAtt.GetMoid(), attributeName = moAtt.GetName() };
                                moAttTable.InsertOnSubmit(newMOATT);
                                dataContext.SubmitChanges();
                            }
                        }
                    }
                }
                dataContext.SubmitChanges();
            }
            LoadMOATT();
            logger.Info("LoadAndSynchMOATT done.");
        }

        private void LoadMOATT() {
            using(var dataContext = GetDataContext()) {
                Table<MOATT> moAtts = dataContext.MOATTs;
                foreach(MOATT o in moAtts) {
                    InsertMOATT(o.moid, o.attributeName, o.aid);
                    maxAid = Math.Max(maxAid, o.aid);
                }
            }
            logger.Info("LoadMOATT done.");
            logger.Info("maxAid", maxAid);
        }

        private void InsertMOATT(String moid, String attributeName, int aid) {
            aid2moidAtt.Put(aid, new String[] { moid, attributeName });
            Dictionary<String, int> att2aid = moidAtt2aid.Get(moid);
            if(att2aid == null) {
                att2aid = new Dictionary<String, int>();
                moidAtt2aid.Put(moid, att2aid);
            }
            att2aid.Put(attributeName, aid);
        }


        /// <summary>
        /// Read all database records of DBOBJ, DBVVAL and DBVAL into the cache (DBObject, DBAttribute)
        /// </summary>
        private void LoadDB() {
            DateTimeUtils.StartTime("LoadDB");
            // DBOBJ
            maxOid = 1000;
            dbObjects.Clear();
            try {
                using(var dataContext = GetDataContext()) {
                    //
                    // DBOBJ
                    //
                    Table<DBOBJ> dbobjs = dataContext.DBOBJs;
                    foreach(DBOBJ dbobj in dbobjs) {
                        logger.Debug("DBOBJ", dbobj.moid, dbobj.oid, dbobj.stat, dbobj.ctime, dbobj.last_vnr, dbobj.last_mtime, dbobj.last_uid);
                        //DBObject dbObject = new DBObject(dbobj);
                        DBObject dbObject = new DBObject();
                        //
                        dbObject.moid = dbobj.moid;
                        dbObject.oid = dbobj.oid;
                        dbObject.stat = dbobj.stat;
                        dbObject.ctime = dbobj.ctime;
                        dbObject.last_vnr = dbobj.last_vnr;
                        dbObject.last_mtime = dbobj.last_mtime;
                        dbObject.last_uid = dbobj.last_uid;
                        //
                        dbObjects.Put(dbObject.oid, dbObject);
                        maxOid = Math.Max(maxOid, dbObject.oid);
                    }
                    //
                    // DBVAL to Dictionary
                    //
                    Table<DBVAL> dbvals = dataContext.DBVALs;
                    Dictionary<String, ValueArray> dbvalValues = new Dictionary<string, ValueArray>();
                    DateTimeUtils.StartTime("DBVAL");
                    foreach(DBVAL dbval in dbvals) {
                        String key = dbval.oid + "-" + dbval.aid + "-" + dbval.vnr;
                        ValueArray va = dbvalValues.Get(key);
                        if(va == null) {
                            va = new ValueArray();
                            dbvalValues.Put(key, va);
                        }
                        va.Add(dbval.indx, dbval.stringValue);
                    }
                    DateTimeUtils.LogTime("DBVAL");
                    //
                    // DBVVAL
                    //
                    Table<DBVVAL> dbvvals = dataContext.DBVVALs;
                    foreach(DBVVAL dbvval in dbvvals) {
                        logger.Debug("DBVVAL", dbvval.oid, dbvval.aid, dbvval.vnr, dbvval.mtime, dbvval.stat, dbvval.uid);
                        // Find DBObject
                        DBObject dbObject = dbObjects.Get(dbvval.oid);
                        if(dbObject == null) {
                            logger.Error("Database is inconsistent! Could not find DBObject", dbvval.oid);
                            continue;
                        }
                        // Find/Create DBAttribute
                        String[] res = aid2moidAtt.Get(dbvval.aid);
                        String moid = res[0];
                        String attributeName = res[1];
                        //
                        DBAttribute dbAttribute = dbObject.GetDBAttribute(attributeName);
                        if(dbAttribute == null) {
                            dbAttribute = new DBAttribute(dbvval.aid);
                            dbObject.AddDBAttribute(attributeName, dbAttribute);
                        }
                        DBVValue dbVvalue = new DBVValue();
                        dbVvalue.vnr = dbvval.vnr;
                        dbVvalue.mtime = dbvval.mtime;
                        dbVvalue.stat = dbvval.stat;
                        dbVvalue.uid = dbvval.uid;
                        //
                        MOAttribute moAttribute = MOService.GetInstance().GetMOAttribute(moid, attributeName);
                        if(moAttribute != null) {
                            dbVvalue.feyOpen = moAttribute.IsFourEyesApproval();
                        } else {
                            logger.Info("MOAttribute not found for: ", moid, attributeName);
                        }
                        // slow version
                        // var dbvals = dataContext.DBVALs.Where(o => o.oid == dbObject.oid && o.aid == dbAttribute.aid && o.vnr == dbvval.vnr);
                        ValueArray va = dbvalValues.Get(dbvval.oid + "-" + dbvval.aid + "-" + dbvval.vnr);
                        if(va == null) {
                            va = new ValueArray();
                        }
                        va.FinalCheck(dbObject, dbAttribute);
                        String[] stringValues = va.array;
                        if(isDate(dbvval.aid)) {
                            toISODate(stringValues);
                        }
                        dbVvalue.SetValues(va.array);
                        dbAttribute.dbVValues.Add(dbVvalue);
                    }
                }
            }
            catch(Exception e) {
                logger.Error(e, e);
                logger.Error("NO DATA LOADED FROM DATABASE!");
                dbObjects.Clear();
            }
            DateTimeUtils.LogTime("LoadDB");
            logger.Info("LoadDB done.");
            logger.Info("maxOid", maxOid);
        }


        private bool isDate(int aid) {
            try {
                String[] moidAtt = aid2moidAtt.Get(aid);
                MOAttribute moAtt = MOService.GetInstance().GetMOClass(moidAtt[0]).GetMOAttribute(moidAtt[1]);
                if(moAtt == null) {
                    logger.Warn("Did not find MOAttribute for aid", aid);
                    return false;
                }
                return moAtt.GetMOType().GetBaseType().GetTypeId().Equals(MOType.DATE);
            }
            catch(Exception e) {
                logger.Warn(e, e);
            }
            return false;
        }


        private String toISODate(String value) {
            DateTime dt;
            if(StringUtils.IsNotBlank(value) && DateTime.TryParse(value, out dt)) {
                value = String.Format("{0:yyyy-MM-dd}", dt);
            }
            return value;
        }

        private void toISODate(String[] values) {
            for(int i = 0; i < values.Length; i++) {
                values[i] = toISODate(values[i]);
            }
        }

        private class ValueArray {

            internal String[] array = new String[0];

            internal void Add(int index, String value) {
                if(array.Length <= index) {
                    ExtendTo(index);
                    if(array[index] != null) {
                        logger.Error("Double entry for DBVValue object: '" + array[index] + "' and '" + value);
                    }
                    array[index] = value;
                }
            }

            private void ExtendTo(int index) {
                String[] nArray = new String[index + 1];
                Array.Copy(array, nArray, array.Length);
                array = nArray;
            }

            internal void FinalCheck(DBObject dbObject, DBAttribute dbAttribute) {
                for(int index = 0; index < array.Length; index++) {
                    String s = array[index];
                    if(s == null) {
                        logger.Error("Empty entry at index : " + index);
                    }
                }
            }
        }

        private void LoadUSR() {
            using(var dataContext = GetDataContext()) {
                Table<TUSR> users = dataContext.TUSRs;
                foreach(TUSR u in users) {
                    InsertUSR(u.uid, u.userName);
                    maxUid = Math.Max(u.uid, maxUid);
                }
            }
        }

        private void InsertUSR(int uid, String userName) {
            uid2user.Put(uid, userName);
            user2uid.Put(userName, uid);
        }

        public int GetUid(String userName) {
            userName = userName.Trim();
            int uid = user2uid.Get(userName);
            if(uid == default(int)) {
                using(var dataContext = GetDataContext()) {
                    int newUid = this.NewUid();
                    InsertUSR(newUid, userName);
                    TUSR usr = new TUSR { uid = newUid, userName = userName };
                    dataContext.TUSRs.InsertOnSubmit(usr);
                    dataContext.SubmitChanges();
                }
            }
            return user2uid.Get(userName);
        }

        public String GetUserName(int uid) {
            return uid2user.Get(uid);
        }

        public DBObject GetById(long oid) {
            return this.dbObjects.Get(oid);
        }

        public DBObject CheckOut(DBObject dbObject, String userName) {
            int uid = GetUid(userName);
            DBObject dbObject2 = dbObjects.Get(dbObject.oid);
            if(dbObject2 == null) {
                return dbObject;
            }
            if(dbObject2.checkingOutUid == uid
                ||
                CheckOutIsTimeOut(dbObject2)
                ) {
                dbObject2.checkingOutUid = uid;
                dbObject2.checkingOutTimeInMillis = DateTimeUtils.NowInMillis();
                return dbObject2;
            }
            return null;
        }

        public bool RefreshCheckOut(DBObject dbObject, String userName) {
            DBObject dbObject2 = dbObjects.Get(dbObject.oid);
            if(dbObject2 == null) {
                return true;
            }
            int uid = GetUid(userName);
            if(dbObject2.checkingOutUid == uid) {
                dbObject2.checkingOutTimeInMillis = DateTimeUtils.NowInMillis();
                return true;
            }
            return false;
        }

        public bool CheckIn(DBObject dbObject, String userName) {
            int uid = GetUid(userName);
            DBObject dbObject2 = dbObjects.Get(dbObject.oid);
            if(dbObject2 == null) {
                return true;
            }
            if(dbObject2.checkingOutUid == uid) {
                dbObject2.checkingOutUid = -1;
                dbObject2.checkingOutTimeInMillis = 0;
                return true;
            }
            return false;
        }


        public String GetCheckedOutUser(DBObject dbObject) {
            DBObject dbObject2 = dbObjects.Get(dbObject.oid);
            if(dbObject2 == null) {
                return null;
            }
            if(CheckOutIsTimeOut(dbObject2)) {
                return null;
            }
            return GetUserName(dbObject2.checkingOutUid);
        }

        private bool CheckOutIsTimeOut(DBObject dbObject) {
            long nowInMillis = DateTimeUtils.NowInMillis();
            return (nowInMillis - dbObject.checkingOutTimeInMillis) > CHECK_OUT_TIMEOUT_MILLIS;
        }




        public IEnumerable<DBObject> GetByDataState(params DataState[] dataStates) {
            logger.Debug("GetByDataState");
            foreach(DBObject dbObject in dbObjects.Values) {
                logger.Debug(dbObject);
                foreach(DataState ds in dataStates) {
                    if(dbObject.stat == (int)ds) {
                        yield return dbObject;
                        break;
                    }
                }
            }
        }

        public IEnumerable<DBObject> GetByHasAttribute(String attributeName) {
            logger.Debug("GetByHasAttribute");
            foreach(DBObject dbObject in dbObjects.Values) {
                String[] values = dbObject.GetCurrentValues(attributeName);
                if(ArrayUtils.IsNotEmpty(values)) {
                    yield return dbObject;
                }
            }
        }

        public IEnumerable<DBObject> GetByValue(List<String> moids, String attributeName, String value, bool exact) {
            foreach(DBObject dbObject in dbObjects.Values) {
                if(dbObject.IsReleased()
                    &&
                    moids.Contains(dbObject.moid)
                    &&
                    CheckValue(dbObject.GetCurrentValues(attributeName), value, exact)) {
                    yield return dbObject;
                }
            }
        }

        private bool CheckValue(String[] values, String valueToCheck, bool exact) {
            if(ArrayUtils.IsEmpty(values)) {
                return false;
            }
            if(exact) {
                logger.Debug("CheckValue", values[0], valueToCheck);
                return values.Length == 1 && values[0].Equals(valueToCheck);
            }
            foreach(String value in values) {
                if(value.IndexOf(valueToCheck) > -1) {
                    return true;
                }
            }
            return false;
        }


        public IEnumerable<DBObject> GetByHasAttribute(List<String> moids, String attributeName, bool onlyReleasedData) {
            foreach(DBObject dbObject in dbObjects.Values) {
                if((!onlyReleasedData || dbObject.IsReleased())
                    &&
                    moids.Contains(dbObject.moid)
                    &&
                    ArrayUtils.IsNotEmpty(dbObject.GetCurrentValues(attributeName))) {
                    yield return dbObject;
                }
            }
        }

        public DBObject WriteToDB(DBObject dbObject) {
            int trials = 1;
            while(trials < 3) {
                try {
                    if(trials > 1) {
                        logger.Warn("Retry <_WriteToDB>, nr of trials : ", trials);
                    }
                    trials++;
                    return _WriteToDB(dbObject);
                }
                catch(System.Transactions.TransactionAbortedException txe) {
                    logger.Error(txe, txe);
                }
            }
            throw new Exception("Could not save DBObject: " + dbObject);
        }

        public DBObject _WriteToDB(DBObject dbObject) {
            if(!dbObject.IsDirty()) {
                return dbObject;
            }
            using(TransactionScope ts = new TransactionScope())
            using(var dataContext = GetDataContext()) {
                //
                // DBOBJ 
                //
                var res = dataContext.DBOBJs.Where(o => o.oid == dbObject.oid);
                foreach(DBOBJ dbobj in res) {
                    dataContext.DBOBJs.DeleteOnSubmit(dbobj);
                }
                dataContext.SubmitChanges();
                //         

                DBOBJ dbobj1 = new DBOBJ {
                    oid = dbObject.oid,
                    moid = dbObject.moid,
                    stat = dbObject.stat,
                    last_vnr = dbObject.last_vnr,
                    ctime = dbObject.ctime,
                    last_uid = dbObject.last_uid,
                    last_mtime = dbObject.last_mtime
                };
                dataContext.DBOBJs.InsertOnSubmit(dbobj1);
                dataContext.SubmitChanges();
                //
                // DBVVAL AND DBVALS
                //
                foreach(DBVVAL dbvval in dataContext.DBVVALs.Where(o => o.oid == dbObject.oid)) {
                    dataContext.DBVVALs.DeleteOnSubmit(dbvval);
                }
                foreach(DBVAL dbvals in dataContext.DBVALs.Where(o => o.oid == dbObject.oid)) {
                    dataContext.DBVALs.DeleteOnSubmit(dbvals);
                }
                dataContext.SubmitChanges();
                //
                foreach(DBAttribute dbAttribute in dbObject.GetDBAttributes()) {
                    // DBVVAL
                    foreach(DBVValue dbvv in dbAttribute.dbVValues) {
                        DBVVAL dbvval = new DBVVAL { oid = dbObject.oid, aid = dbAttribute.aid, vnr = dbvv.vnr, stat = dbvv.stat, uid = dbvv.uid, mtime = dbvv.mtime };
                        logger.Debug("DBVVAL", dbvval.oid, dbvval.aid, dbvval.vnr, dbvval.stat, dbvval.uid, dbvval.mtime);
                        dataContext.DBVVALs.InsertOnSubmit(dbvval);
                        dataContext.SubmitChanges();
                        // DBVALS
                        for(int index = 0; index < dbvv.GetValues().Length; index++) {
                            String value = dbvv.GetValues()[index];
                            DBVAL dbvals = new DBVAL { oid = dbObject.oid, aid = dbvval.aid, vnr = dbvv.vnr, indx = index, stringValue = value };
                            if(logger.IsDebug()) {
                                String[] moidAtt = aid2moidAtt.Get(dbvals.aid);
                                logger.Debug("DBVALS", dbvals.oid, dbvals.aid, dbvals.indx, dbvals.stringValue, moidAtt[0], moidAtt[1]);
                            }
                            dataContext.DBVALs.InsertOnSubmit(dbvals);
                            dataContext.SubmitChanges();
                        }
                    }
                }
                dataContext.SubmitChanges();
                ts.Complete();
                // RE-REGISTER IN MEMORY DB
                dbObjects.Put(dbObject.oid, dbObject);
            }
            return dbObject;
        }

        public static void _Migration_2010_08_01(String connectionString) {
            using(var dataContext = GetDataContext(connectionString)) {
                MOATT newDivisionAttribute = null;
                var res = dataContext.MOATTs.Where(o => o.moid == "ch.vitra.vips.Division");
                foreach(MOATT moatt in res) {
                    if(DEF.CompanyGroup.division.Equals(moatt.attributeName)) {
                        newDivisionAttribute = new MOATT { aid = moatt.aid, moid = DEF.CompanyGroup.moid, attributeName = DEF.CompanyGroup.division };
                    }
                    logger.Warn("_Migration_2010_08_01 elements found: ", moatt);
                }
                //
                dataContext.MOATTs.DeleteAllOnSubmit(res);
                dataContext.SubmitChanges();
                //
                if(newDivisionAttribute != null) {
                    var res2 = dataContext.MOATTs.Where(o => o.moid == DEF.CompanyGroup.moid && o.attributeName == DEF.CompanyGroup.division);
                    dataContext.MOATTs.DeleteAllOnSubmit(res2);
                    dataContext.SubmitChanges();
                    dataContext.MOATTs.InsertOnSubmit(newDivisionAttribute);
                    dataContext.SubmitChanges();
                }
                //
            }
        }


        public static void _RemoveAllData(String connectionString) {
            using(var dataContext = GetDataContext(connectionString)) {
                dataContext.DBVVALs.DeleteAllOnSubmit(dataContext.DBVVALs);
                dataContext.DBOBJs.DeleteAllOnSubmit(dataContext.DBOBJs);
                dataContext.DBVALs.DeleteAllOnSubmit(dataContext.DBVALs);
                dataContext.MOATTs.DeleteAllOnSubmit(dataContext.MOATTs);
                dataContext.TUSRs.DeleteAllOnSubmit(dataContext.TUSRs);
                dataContext.SubmitChanges();
            }
        }

        /// <summary>
        /// Mapping a moid and attributeName to an attribute id.
        /// Be aware that the attribute itself can be inherited and inherited attributes have always
        /// the same attribute id!
        /// </summary>
        /// <param name="moid"></param>
        /// <param name="attributeName"></param>
        /// <returns></returns>
        public int GetAid(String moid, String attributeName) {
            MOAttribute moAttribute = moService.GetMOAttribute(moid, attributeName);
            Dictionary<String, int> d1 = moidAtt2aid.Get(moAttribute.GetMOClass().GetMoid());
            if(d1 != null && d1.ContainsKey(attributeName)) {
                return d1.Get(attributeName);
            }
            logger.Debug("No aid found for: " + moAttribute.GetMoid() + "::" + moAttribute.GetName());
            return -1;
        }


        private static Linq2SQLDataContext GetDataContext() {
            if(connectionString != null) {
                return new Linq2SQLDataContext(connectionString);
            } else {
                throw new Exception("no connectionString set");
            }
        }

        private static Linq2SQLDataContext GetDataContext(String cs) {
            return new Linq2SQLDataContext(cs);
        }


    }
    //
    internal class MoidAtt {

        private String moid;
        private String att;
        private String toString;
        private int hashCode;

        public MoidAtt(String moid, String att) {
            this.moid = moid;
            this.att = att;
            this.toString = moid + ":" + att;
            this.hashCode = toString.GetHashCode();
        }
        public String GetMoid() {
            return moid;
        }
        public String GetAtt() {
            return att;
        }
        public override bool Equals(object obj) {
            if(obj is MoidAtt) {
                MoidAtt moidAtt = (MoidAtt)obj;
                return moidAtt.GetMoid().EndsWith(moid) && moidAtt.GetAtt().Equals(att);
            }
            return false;
        }

        public override int GetHashCode() {
            return this.hashCode;
        }
        public override string ToString() {
            return this.toString;
        }
    }


    [Serializable]
    public class DBObject : ICloneable {

        private static Logger logger = Logger.GetLogger(typeof(DBObject));

        //
        // MO based attributes
        //
        private List<String> matchStrings = new List<string>();
        // 
        private Dictionary<String, String[]> backRefValues = new Dictionary<String, String[]>();
        //
        // Database attributes
        //
        internal long oid;
        internal String moid = "-";
        internal long ctime;
        internal int last_vnr;
        internal long last_mtime;
        internal int stat;
        internal int last_uid;
        private Dictionary<String, DBAttribute> attributes = new Dictionary<String, DBAttribute>();
        //
        private bool dirty = false;

        internal DBObject() { }


        internal DBObject(String moid) {
            this.oid = DBService.GetInstance().NewOid();
            this.moid = moid;
            this.stat = (int)DataState.NEW;
            this.last_vnr = 0;
            this.last_uid = DBService.GetInstance().GetUid(MOSystem.GetUserName());
            this.ctime = System.DateTime.Now.Ticks;
            this.last_mtime = this.ctime;
        }

        internal DBObject(DBOBJ dbobj) {
            this.oid = dbobj.oid;
            this.moid = dbobj.moid;
            this.stat = dbobj.stat;
            this.last_vnr = dbobj.last_vnr;
            this.ctime = dbobj.ctime;
            this.last_uid = dbobj.last_uid;
            this.last_mtime = dbobj.last_mtime;
        }

        internal bool IsApprovalNeeded(String attributeName, bool fey) {
            if(fey) {
                int aid = DBService.GetInstance().GetAid(moid, attributeName);
                DBVValue dbVV = attributes.Get(attributeName).dbVValues.Last();
                if(dbVV.stat == (int)DataState.STORED) {
                    return true;
                }
            }
            return false;
        }

        internal bool CanApprove(String attributeName, String userName) {
            DBVValue dbVV = attributes.Get(attributeName).dbVValues.Last();
            int uid = DBService.GetInstance().GetUid(userName);
            if(dbVV.stat == (int)DataState.STORED && dbVV.uid != uid) {
                return true;
            }
            return false;
        }

        public bool UpdateAttributeValues(String attributeName, String[] values, String userName, bool fey) {
            if(AreCurrentValuesEqual(attributeName, values)) {
                return false;
            }
            int uid = DBService.GetInstance().GetUid(userName);
            DBAttribute dbAttribute = GetOrCreateDBAttribute(attributeName);
            dbAttribute.Update(values, uid, fey);
            dirty = true;
            return true;
        }


        public bool UpdateAttributeValuesWithHistory(String attributeName, String[] values, String userName, bool fey) {
            if(AreCurrentValuesEqual(attributeName, values)) {
                return false;
            }
            int uid = DBService.GetInstance().GetUid(userName);
            DBAttribute dbAttribute = GetOrCreateDBAttribute(attributeName);
            dbAttribute.UpdateWithHistory(values, uid, fey);
            dirty = true;
            return true;
        }

        private bool AreCurrentValuesEqual(String attributeName, String[] newValues) {
            String[] values = GetCurrentValues(attributeName);
            return ArrayUtils.AreEquals(values, newValues);
        }

        internal DBAttribute GetDBAttribute(String attributeName) {
            return attributes.Get(attributeName);
        }

        internal IEnumerable<DBAttribute> GetDBAttributes() {
            return attributes.Values;
        }

        internal void AddDBAttribute(String attributeName, DBAttribute dbAttribute) {
            attributes.Add(attributeName, dbAttribute);
        }

        private bool TryGetDBVValues(String attributeName, out List<DBVValue> dbVValuesOut) {
            dbVValuesOut = null;
            if(attributes != null && attributes.ContainsKey(attributeName)) {
                DBAttribute dbAttribute = attributes.Get(attributeName);
                if(dbAttribute.dbVValues != null) {
                    dbVValuesOut = dbAttribute.dbVValues;
                    return true;
                }
            }
            return false;
        }

        private DBAttribute GetOrCreateDBAttribute(String attributeName) {
            DBAttribute dbAttribute = attributes.Get(attributeName);
            if(dbAttribute == null) {
                int aid = DBService.GetInstance().GetAid(this.moid, attributeName);
                dbAttribute = new DBAttribute(aid);
                attributes.Put(attributeName, dbAttribute);
            }
            return dbAttribute;
        }

        public void Save(string userName) {
            // Clear all back ref values
            backRefValues.Clear();
            //
            int uid = DBService.GetInstance().GetUid(userName);
            last_vnr = 0;
            bool attributeApprovalNeeded = false;
            foreach(DBAttribute dbAtt in attributes.Values) {
                foreach(DBVValue dbVV in dbAtt.dbVValues) {
                    last_vnr = Math.Max(dbVV.vnr, last_vnr);
                    last_mtime = Math.Max(dbVV.mtime, last_mtime);
                    if(dbVV.stat == (int)DataState.NEW) {
                        if(dbVV.feyOpen) {
                            dbVV.stat = (int)DataState.STORED;
                            attributeApprovalNeeded = true;
                        } else {
                            dbVV.stat = (int)DataState.APPROVED;
                        }
                    }
                    if(dbVV.stat == (int)DataState.STORED && dbVV.feyOpen) {
                        attributeApprovalNeeded = true;
                    }
                }
            }
            if(attributeApprovalNeeded) {
                stat = (int)DataState.STORED;
            } else {
                stat = (int)DataState.APPROVED;
            }
            last_uid = uid;
            dirty = true;
            DBService.GetInstance().WriteToDB(this);
            dirty = false;
        }

        public void Approve(String userName) {
            if(this.stat == (int)DataState.APPROVED || this.stat == (int)DataState.NEW || this.stat == (int)DataState.DELETED) {
                logger.Warn("Nothing to approve!", this.stat, this.moid, this.oid);
                return;
            }
            int userId = DBService.GetInstance().GetUid(userName);
            // approve1
            // approve2
            if(this.stat == (int)DataState.STORED) {
                int tmpStat = (int)DataState.APPROVED;
                foreach(DBAttribute dbAtt in attributes.Values) {
                    List<DBVValue> dbVVs = dbAtt.dbVValues;
                    if(ICollectionUtils.IsNotEmpty(dbVVs)) {
                        DBVValue lastDbVV = dbVVs.Last();
                        if(lastDbVV.stat == (int)DataState.STORED) {
                            if(lastDbVV.uid != userId) {
                                lastDbVV.stat = (int)DataState.APPROVED;
                            } else {
                                tmpStat = (int)DataState.STORED;
                            }
                        }
                    }
                }
                this.stat = tmpStat;
            }
            // approve3
            if(this.stat == (int)DataState.DELETED_UNAPPROVED) {
                if(userId != this.last_uid) {
                    this.stat = (int)DataState.DELETED;
                } else {
                    return;
                }
            }
            this.dirty = true;
            this.last_uid = userId;
            DBService.GetInstance().WriteToDB(this);
            this.dirty = false;
        }


        public void Delete(string userName, Boolean fourEyesApprovalNeededForDeletion) {
            if(fourEyesApprovalNeededForDeletion) {
                this.stat = (int)DataState.DELETED_UNAPPROVED;
            } else {
                this.stat = (int)DataState.DELETED;
            }
            this.last_uid = DBService.GetInstance().GetUid(userName);
            this.last_mtime = DateTime.Now.Ticks;
            dirty = true;
            DBService.GetInstance().WriteToDB(this);
            dirty = false;
        }

        public String[] GetCurrentValues(String attributeName) {
            DBVValue vv = GetCurrentVValues(attributeName);
            return vv == null ? null : vv.GetValues();
        }

        public DBVValue GetCurrentVValues(String attributeName) {
            DBAttribute dbAttribute = attributes.Get(attributeName);
            if(dbAttribute == null) {
                logger.Debug("attributeName : " + attributeName + " not found");
                return null;
            }
            if(dbAttribute.dbVValues.Count == 0) {
                logger.Warn("No version values exist for DB attribute : ", this.oid, attributeName);
                return null;
            }
            return dbAttribute.dbVValues.Last();
        }


        public String[] GetApprovedValues(String attributeName) {
            DBAttribute dbAttribute = attributes.Get(attributeName);
            if(dbAttribute == null) {
                logger.Debug("DB attribute with name : ", attributeName, " does not exist (yet).");
                return null;
            }
            if(dbAttribute.dbVValues.Count == 0) {
                logger.Warn("No values exist for DB attribute : ", attributeName);
                return null;
            }
            return dbAttribute.dbVValues.Last().GetValues();
        }

        public List<DBVValue> GetHistoricalValues(String attributeName) {
            List<DBVValue> res = new List<DBVValue>();
            List<DBVValue> dbVValues = null;
            if(TryGetDBVValues(attributeName, out dbVValues)) {
                foreach(DBVValue vvalue in dbVValues) {
                    if(vvalue.stat == (int)DataState.APPROVED) {
                        res.Add(vvalue);
                    }
                }
            }
            //foreach(DBVValue vvalue in GetOrCreateDBAttribute(attributeName).dbVValues) {
            //    if(vvalue.stat == (int)DataState.APPROVED) {
            //        res.Add(vvalue);
            //    }
            //}
            if(res.Count() > 0) {
                res.Sort(new DBVValueTimeComparer());
                // res.Remove(res.Last());
            }
            return res;
        }

        class DBVValueTimeComparer : IComparer<DBVValue> {
            public int Compare(DBVValue x, DBVValue y) {
                return x.mtime < y.mtime ? 1 : (x.mtime > y.mtime ? -1 : 0);
            }

        }


        public Object Clone() {
            MemoryStream memoryStream = new MemoryStream();
            BinaryFormatter binaryFormatter = new BinaryFormatter();

            binaryFormatter.Serialize(memoryStream, this);
            memoryStream.Seek(0, SeekOrigin.Begin);

            return binaryFormatter.Deserialize(memoryStream);
        }

        public bool IsDirty() {
            return dirty;
        }

        public String[] GetLastApprovedValues(string attributeName) {
            return GetLastValueByState(attributeName, DataState.APPROVED);
        }

        //public String[] GetLastStoredValues(string attributeName) {
        //    return GetLastValueByState(attributeName, DataState.STORED);
        //}

        private String[] GetLastValueByState(String attributeName, DataState dataState) {
            DBAttribute dbAttribute = GetOrCreateDBAttribute(attributeName);
            List<DBVValue> dbVValues = null;
            TryGetDBVValues(attributeName, out dbVValues);
            if(dbVValues != null) {
                for(int index = dbVValues.Count - 1; index > -1; index--) {
                    DBVValue dbvv = dbVValues[index];
                    if(dbvv.stat == (int)dataState) {
                        return dbvv.GetValues();
                    }
                }
            }
            return null;
        }


        //
        // Search Stuff
        //
        internal List<String> GetMatchStrings() {
            return matchStrings == null ? matchStrings = new List<string>() : matchStrings;
        }

        internal int Match(String[] searchStrings) {
            int matchRating = 0;
            foreach(String matchString in GetMatchStrings()) {
                foreach(String searchString in searchStrings) {
                    if(matchString.Equals(searchString)) {
                        matchRating += 10;
                    } else if(matchString.IndexOf(searchString) > -1) {
                        matchRating += 1;
                    }
                }
            }
            return matchRating;
        }

        // 
        internal String[] GetBackRefs(String attributeName) {
            return backRefValues.Get(attributeName);
        }

        // 
        internal void UpdateBackRefs(String attributeName, String[] backRefs) {
            backRefValues.Put(attributeName, backRefs);
        }

        // 
        internal void ClearAllBackRefs() {
            backRefValues.Clear();
        }

        public override string ToString() {
            StringBuilder sb = new StringBuilder();
            sb.Append("[");
            sb.Append(this.moid);
            sb.Append(";");
            sb.Append(this.oid);
            sb.Append(";");
            sb.Append((DataState)this.stat);
            sb.Append("]");
            return sb.ToString();
        }

        public bool IsReleased() {
            return stat == (int)DataState.APPROVED || stat == (int)DataState.STORED;
        }

        //
        // Checking Out
        //

        internal int checkingOutUid = -1;
        internal long checkingOutTimeInMillis;

    }

    [Serializable]
    public class DBAttribute {

        public int aid;
        internal List<DBVValue> dbVValues = new List<DBVValue>();

        public DBAttribute(int aid) {
            this.aid = aid;
        }

        public String CurrentValue() {
            String[] currentValues = CurrentValues();
            return currentValues == null || currentValues.Length == 0 ? null : currentValues[0];
        }

        public String[] CurrentValues() {
            if(dbVValues.Count == 0) {
                return null;
            }
            return dbVValues.GetLast().GetValues();
        }

        public void Update(String[] values, int uid, bool fey) {
            Update(values, uid, fey, true);
        }

        public void UpdateWithHistory(String[] values, int uid, bool fey) {
            Update(values, uid, fey, true);
        }

        private void Update(String[] values, int uid, bool fey, bool history) {
            DBVValue dbvv = null;
            int nextVnr = NextVersionNumber(dbVValues);
            if(dbVValues.Count == 0) {
                dbvv = new DBVValue();
                dbVValues.Add(dbvv);
            } else {
                dbvv = dbVValues.Last();
                if(dbvv.stat == (int)DataState.NEW) {
                    nextVnr = dbvv.vnr;
                } else {
                    if(history) {
                        dbvv = new DBVValue();
                        dbVValues.Add(dbvv);
                    }
                }
            }
            // assignement in all cases
            dbvv.vnr = nextVnr;
            dbvv.mtime = DateTime.Now.Ticks;
            dbvv.stat = (int)DataState.NEW;
            dbvv.uid = uid;
            dbvv.SetValues(values);
            dbvv.feyOpen = fey;
        }

        private int NextVersionNumber(List<DBVValue> dbVValues) {
            int nextVn = 0;
            if(ICollectionUtils.IsEmpty(dbVValues)) {
                return nextVn;
            }
            foreach(DBVValue dbvv in dbVValues) {
                nextVn = Math.Max(nextVn, dbvv.vnr);
            }
            nextVn++;
            return nextVn;
        }

    }


    [Serializable]
    public class DBVValue {
        private static String[] emptyString = { };
        internal int vnr;
        internal int stat;
        internal Int64 mtime;
        private String[] stringValues;
        internal int uid = -1;
        //
        internal bool feyOpen = false;

        public String[] GetValues() {
            if(ArrayUtils.IsNotEmpty(stringValues))
                return stringValues;
            else
                return emptyString;
        }

        public void SetValues(String[] values) {
            this.stringValues = values;
        }


        public long GetTime() {
            return mtime;
        }

        public int GetUid() {
            return uid;
        }

    }
}
