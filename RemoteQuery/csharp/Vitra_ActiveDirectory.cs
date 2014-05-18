//
// Copyright (C) 2008 Vitra AG, Klünenfeldstrasse 22, Muttenz, 4127 Birsfelden
// All rights reserved.
//
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Configuration.Provider;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Web.Hosting;
using System.Web.Security;
using Org.JGround.Util;

namespace Com.VITRA.ActiveDirectory {

    public sealed class ADRoleProvider : RoleProvider {

        private static Logger logger = Logger.GetLogger(typeof(ADRoleProvider));

        private String applicationName;
        private String adConnectionStr;
        private String domainDN;

        public override String ApplicationName {
            get { return applicationName; }
            set { applicationName = value; }
        }

        public override void Initialize(String name, NameValueCollection config) {
            logger.Info("ADRoleProvider::Initialize");
            try {
                if(config == null) {
                    throw new ArgumentNullException("config");
                }
                if(String.IsNullOrEmpty(name)) {
                    name = "ADRoleProvider for VITRA";
                }
                if(String.IsNullOrEmpty(config["description"])) {
                    config.Remove("description");
                    config.Add("description", "Active Directory Role Provider for VITRA Applications");
                }

                // Initialize the abstract base class.
                base.Initialize(name, config);

                // Retrieve Active Directory Connection String from config
                String temp = config["activeDirectoryConnectionString"];
                if(String.IsNullOrEmpty(temp)) {
                    throw new ProviderException("The attribute 'activeDirectoryConnectionString' is missing or empty.");
                }
                ConnectionStringSettings connObj = ConfigurationManager.ConnectionStrings[temp];
                if(connObj != null) {
                    adConnectionStr = connObj.ConnectionString;
                }
                if(String.IsNullOrEmpty(adConnectionStr)) {
                    throw new ProviderException("The connection name 'activeDirectoryConnectionString' was not found in the applications configuration or the connection string is empty.");
                }
                if(adConnectionStr.Substring(0, 10) == "LDAP://DC=") {
                    domainDN = adConnectionStr.Substring(7, adConnectionStr.Length - 7);
                } else {
                    throw new ProviderException("The connection string specified in 'activeDirectoryConnectionString' does not appear to be a valid LDAP connection string.");
                }

                // Retrieve Application Name
                applicationName = config["applicationName"];
                if(String.IsNullOrEmpty(applicationName)) {
                    applicationName = GetDefaultAppName();
                }
                if(applicationName.Length > 256) {
                    throw new ProviderException("The application name is too long.");
                }
            }
            catch(Exception e) {
                logger.Error(e);
                throw e;
            }
        }

        public override String[] GetRolesForUser(String userName) {
            logger.Info("ADRoleProvider::GetRolesForUser", userName);
            List<String> results = new List<String>();
            using(PrincipalContext context = new PrincipalContext(ContextType.Domain, null, domainDN)) {
                try {
                    UserPrincipal p = UserPrincipal.FindByIdentity(context, IdentityType.SamAccountName, userName);
                    var groups = p.GetAuthorizationGroups();
                    foreach(GroupPrincipal group in groups) {
                        if (group != null && group.SamAccountName != null) {
                            results.Add(group.SamAccountName);
                        }
                    }
                }
                catch(Exception ex) {
                    ProviderException pe = new ProviderException("Unable to query Active Directory.", ex);
                    logger.Error(pe);
                    throw pe;
                }
            }
            logger.Info("ADRoleProvider::GetRolesForUser", userName, ArrayUtils.Join(results.ToArray()));
            return results.ToArray();
        }

        public override String[] GetUsersInRole(String roleName) {
            logger.Info("ADRoleProvider::GetUsersInRole", roleName);
            if(!RoleExists(roleName)) {
                throw new ProviderException(String.Format("The role '{0}' was not found.", roleName));
            }
            List<String> results = new List<String>();
            using(PrincipalContext context = new PrincipalContext(ContextType.Domain, null, domainDN)) {
                try {
                    GroupPrincipal p = GroupPrincipal.FindByIdentity(context, IdentityType.SamAccountName, roleName);
                    var users = p.GetMembers(true);
                    foreach(UserPrincipal user in users) {
                        results.Add(user.SamAccountName);
                    }
                }
                catch(Exception ex) {
                    ProviderException pe = new ProviderException("Unable to query Active Directory.", ex);
                    throw pe;
                }
            }
            return results.ToArray();
        }

        public override bool IsUserInRole(string userName, string roleName) {
            logger.Info("ADRoleProvider::IsUserInRole", userName, roleName);
            foreach(String u in GetUsersInRole(roleName)) {
                if(userName.Equals(u)) {
                    return true;
                }
            }
            return false;
        }

        public override string[] GetAllRoles() {
            logger.Info("ADRoleProvider::GetAllRoles");
            List<String> results = new List<String>();
            String[] roles = ADSearch(adConnectionStr, "(&(objectCategory=group)(|(groupType=-2147483646)(groupType=-2147483644)(groupType=-2147483640)))", "samAccountName");
            foreach(String role in roles) {
                results.Add(role);
            }
            logger.Info("ADRoleProvider::GetAllRoles", ArrayUtils.Join(results.ToArray()));
            return results.ToArray();
        }

        public override bool RoleExists(string rolename) {
            logger.Info("ADRoleProvider::RoleExists");
            foreach(String strRole in GetAllRoles()) {
                if(rolename == strRole) return true;
            }
            return false;
        }

        public override string[] FindUsersInRole(string roleName, string userNameToMatch) {
            logger.Info("ADRoleProvider::FindUsersInRole", roleName, userNameToMatch);
            if(!RoleExists(roleName)) {
                ProviderException pe = new ProviderException(String.Format("The role '{0}' was not found.", roleName));
                logger.Error(pe);
                throw pe;
            }
            List<String> results = new List<String>();
            String[] roles = GetAllRoles();
            foreach(String role in roles) {
                if(role.ToLower().Contains(userNameToMatch.ToLower())) {
                    results.Add(role);
                }
            }
            results.Sort();
            return results.ToArray();
        }

        public override void AddUsersToRoles(string[] usernames, string[] rolenames) {
            throw new NotSupportedException("Unable to add users to roles.  For security and management purposes, ADRoleProvider only supports read operations against Active Direcory.");
        }

        public override void CreateRole(string rolename) {
            throw new NotSupportedException("Unable to create new role.  For security and management purposes, ADRoleProvider only supports read operations against Active Direcory.");
        }

        public override bool DeleteRole(string rolename, bool throwOnPopulatedRole) {
            throw new NotSupportedException("Unable to delete role.  For security and management purposes, ADRoleProvider only supports read operations against Active Direcory.");
        }

        public override void RemoveUsersFromRoles(string[] usernames, string[] rolenames) {
            throw new NotSupportedException("Unable to remove users from roles.  For security and management purposes, ADRoleProvider only supports read operations against Active Direcory.");
        }

        /// <summary>
        /// Performs an extremely constrained query against Active Directory.  Requests only a single value from
        /// AD based upon the filtering parameter to minimize performance hit from large queries.
        /// </summary>
        /// <param name="ConnectionString">Active Directory Connection String</param>
        /// <param name="filter">LDAP format search filter</param>
        /// <param name="field">AD field to return</param>
        /// <param name="scopeQuery">Display name of the distinguished name attribute to search in</param>
        /// <returns>String array containing values specified by 'field' parameter</returns>
        private String[] ADSearch(String ConnectionString, String filter, String field) {
            String strResults = "";
            DirectorySearcher searcher = new DirectorySearcher();
            searcher.SearchRoot = new DirectoryEntry(ConnectionString);
            searcher.Filter = filter;
            searcher.PropertiesToLoad.Clear();
            searcher.PropertiesToLoad.Add(field);
            searcher.PageSize = 500;
            SearchResultCollection results;
            try {
                results = searcher.FindAll();
            }
            catch(Exception ex) {
                throw new ProviderException("Unable to query Active Directory.", ex);
            }
            foreach(SearchResult result in results) {
                int resultCount = result.Properties[field].Count;
                for(int c = 0; c < resultCount; c++) {
                    String temp = result.Properties[field][c].ToString();
                    strResults += temp + "|";
                }
            }
            // IMPORTANT - Dispose SearchResulCollection to prevent memory leak
            results.Dispose();
            if(strResults.Length > 0) {
                // Remove trailing |.
                strResults = strResults.Substring(0, strResults.Length - 1);
                return strResults.Split('|');
            }
            return new string[0];
        }

        private static string GetDefaultAppName() {
            try {
                string appName = HostingEnvironment.ApplicationVirtualPath;
                if(String.IsNullOrEmpty(appName)) {
                    appName = System.Diagnostics.Process.GetCurrentProcess().MainModule.ModuleName;
                    int indexOfDot = appName.IndexOf('.');
                    if(indexOfDot != -1) {
                        appName = appName.Remove(indexOfDot);
                    }
                }
                if(String.IsNullOrEmpty(appName)) {
                    return "/";
                } else {
                    return appName;
                }
            }
            catch {
                return "/";
            }
        }
    }
}
