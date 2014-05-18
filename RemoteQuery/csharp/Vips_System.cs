//
// Copyright (C) 2008 Vitra AG, Klünenfeldstrasse 22, Muttenz, 4127 Birsfelden
// All rights reserved.
//
using System;
using System.IO;
using System.Threading;
using Org.JGround.MOM.DB;
using Org.JGround.Util;

namespace Com.OOIT.VIPS.System {

    public static class VIPSSystem {

        private static readonly String USER_SLOT_KEY = "vips.user.slot";
        private static readonly String ROLES_SLOT_KEY = "vips.roles.slot";

        private static LocalDataStoreSlot userSlot = Thread.GetNamedDataSlot(USER_SLOT_KEY);
        private static LocalDataStoreSlot rolesSlot = Thread.GetNamedDataSlot(ROLES_SLOT_KEY);

        public static void RegisterUserOnThread(String userName) {
            Thread.SetData(userSlot, userName);
        }

        public static void RegisterRolesOnThread(params String[] roles) {
            Thread.SetData(rolesSlot, roles);
        }

        public static String GetUser() {
            return (String)Thread.GetData(userSlot);
        }

        public static String[] GetRoles() {
            String[] roles = (String[])Thread.GetData(rolesSlot);
            if(roles == null) {
                roles = new String[] { };
            }
            return roles;
        }

    }

    public class VIPSDBBackup {

        private static Logger logger = Logger.GetLogger(typeof(VIPSDBBackup));
        private static VIPSDBBackup instance;

        public static VIPSDBBackup GetInstance() {
            return instance == null ? instance = new VIPSDBBackup() : instance;
        }

        //

        private Timer backupTimer;
        private String outputDir;
        private String connectionString;


        private VIPSDBBackup() { }

        public void Startup(String outputDir, String connectionString) {
            backupTimer = new Timer(CallBack, "-", 30 * 60 * 1000, 60 * 60 * 1000);
            this.outputDir = outputDir;
            this.connectionString = connectionString;
        }

        private void CallBack(Object obj) {
            try {
                lock(Locker.MUTEX) {
                    TryBackup();
                }
            }
            catch(Exception e) {
                logger.Error(e, e);
            }
        }

        private void TryBackup() {
            String d = DateTimeUtils.FormatDate(DateTime.Now);
            String outputFile = Path.Combine(outputDir, "vipsdb-" + d + ".txt");
            if(//DateTime.Now.Hour == 23 && 
                !File.Exists(outputDir)) {
                DBService.SaveDBToFile(outputFile, connectionString);
                logger.Info("SaveDBToFile", outputFile, "Done");
            }
        }

        public static void Shutdown() {
            if(instance != null) {
                instance.backupTimer.Dispose();
                instance = null;
            }
        }
    }
}
