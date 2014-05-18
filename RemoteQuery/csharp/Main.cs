//
// Copyright (C) 2008 Vitra AG, Klünenfeldstrasse 22, Muttenz, 4127 Birsfelden
// All rights reserved.
//
using System;
using System.Collections;
using System.Collections.Generic;
using Com.OOIT.VIPS.System;
using Org.JGround.Codetable;
using Org.JGround.MOM;
using Org.JGround.MOM.DB;
using Org.JGround.Util;
using Org.JGround.MOM.Generate;

namespace Com.OOIT.VIPS {


    public class VipsDefGenerator {

        static readonly String LogDir = @"E:\tab\data\vipsdata-dev";
        static readonly String DefDir = @"E:\tab\a_proj\csharp-ws\DataStore\DataStore\";
        static readonly String ClassOutputDir = @"E:\tab\a_proj\csharp-ws\DataStore\VIPS";
        static readonly String ClassFile = @"VIPS_DEF.cs";
        static readonly String ClassNameSpace = "Com.OOIT.VIPS";

        public static void Exec(params String[] args) {
            InitServices();
            DefGenerator.ProcessSanityCheck();
            DefGenerator.ProcessDEFClassGeneration(ClassOutputDir, ClassFile, ClassNameSpace);
        }

        private static void InitServices() {
            //
            Logger.LOG_DIR = LogDir;
            Logger.GLOBAL_LOG_LEVEL = Logger.LogLevels.INFO;
            Logger.LOG_LEVEL(Logger.LogLevels.WARN, typeof(MOService));
            Logger.LOG_LEVEL(Logger.LogLevels.DEBUG, typeof(Assert));
            Logger.LOG_LEVEL(Logger.LogLevels.DEBUG, typeof(MODataObject));
            Logger.LOG_LEVEL(Logger.LogLevels.INFO, typeof(DBService));
            //
            MOService.Initialilze(DefDir);
            //
            DBService.Initialize(null);
        }

    }
}
