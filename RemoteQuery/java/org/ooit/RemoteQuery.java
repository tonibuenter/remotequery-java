package org.ooit;

import java.io.BufferedReader;
import java.io.File;
import java.io.FileInputStream;
import java.io.IOException;
import java.io.InputStreamReader;
import java.io.Reader;
import java.io.Serializable;
import java.io.StringWriter;
import java.io.Writer;
import java.lang.reflect.Array;
import java.lang.reflect.InvocationTargetException;
import java.lang.reflect.Method;
import java.lang.reflect.Modifier;
import java.lang.reflect.Type;
import java.sql.Clob;
import java.sql.Connection;
import java.sql.PreparedStatement;
import java.sql.ResultSet;
import java.sql.ResultSetMetaData;
import java.sql.SQLException;
import java.sql.Statement;
import java.sql.Timestamp;
import java.sql.Types;
import java.text.ParseException;
import java.text.SimpleDateFormat;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.Calendar;
import java.util.Collection;
import java.util.Date;
import java.util.HashMap;
import java.util.HashSet;
import java.util.List;
import java.util.Map;
import java.util.Set;
import java.util.logging.Level;
import java.util.logging.Logger;

import javax.sql.DataSource;

import com.google.gson.Gson;
import com.google.gson.JsonArray;
import com.google.gson.JsonElement;
import com.google.gson.JsonObject;
import com.google.gson.JsonParser;
import com.google.gson.JsonPrimitive;
import com.google.gson.reflect.TypeToken;

/**
 * TODOs set-0, set-1, ... set-initial, set-parameter:, set-session,
 * set-application, ...
 * 
 * @author tonibuenter
 * 
 */
public class RemoteQuery {

	public static String DEFAULT_DATASOURCE_NAME = "DEFAULT_DATASOURCE";

	public static String encoding = "UTF-8";

	public final static String ANONYMOUS = "ANONYMOUS";

	public static final String $WEBROOT = "$WEBROOT";

	public static final String $TODAY_ISO_DATE = "$TODAY_ISO_DATE";
	public static final String $REQUESTID = "$REQUESTID";
	public static final String $SERVICEID = "$SERVICEID";
	public static final String $SQCOMMANDS = "$SQCOMMANDS";
	public static final String $USERID = "$USERID";
	// public static final String $USERTID = "$USERTID";
	// public static final String $NEW_TID = "$NEW_TID";
	public static final String $TIMESTAMP = "$TIMESTAMP";
	// public static final String $VALUE = "$VALUE";
	public static final String $CURRENT_TIME_MILLIS = "$CURRENT_TIME_MILLIS";

	public static final int INITIAL = 0;
	public static final int REQUEST = 1;
	public static final int SESSION = 2;

	public static class MiniLanguageTokens {
		public static String set = "set";
		public static String set_if_empty = "set-if-empty";
		public static String java = "java";

	}

	//
	//
	//

	public static char STATEMENT_DELIMITER = ';';
	public static char STATEMENT_ESCAPE = '\\';

	public static class DataSourceEntry {

		public DataSourceEntry(DataSource ds) {
			DataSources.getInstance().put(ds);
		}

		public DataSourceEntry(String dataSourceName, DataSource ds) {
			DataSources.getInstance().put(dataSourceName, ds);
		}

	}

	public static class DataSources {

		private static DataSources instance;

		public static DataSources getInstance() {
			return instance == null ? instance = new DataSources() : instance;
		}

		private Map<String, DataSource> dss = new HashMap<String, DataSource>();

		public DataSource get() {
			return dss.get(DEFAULT_DATASOURCE_NAME);
		}

		public DataSource get(String dataSourceName) {
			if (dataSourceName == null) {
				dataSourceName = DEFAULT_DATASOURCE_NAME;
			}
			return dss.get(dataSourceName);
		}

		public void put(String dataSourceName, DataSource ds) {
			dss.put(dataSourceName, ds);
		}

		public void put(DataSource ds) {
			dss.put(DEFAULT_DATASOURCE_NAME, ds);
		}

	}

	public static interface IQuery extends Serializable {

		Result run(Request request);

	}

	public interface IRoleProvider {

		/**
		 * optional
		 * 
		 * @param userId
		 * @return
		 */
		Set<String> getRoles(String userId);

		/**
		 * mandatory
		 * 
		 * @param userId
		 * @param role
		 * @return
		 */
		boolean isInRole(String role);
	}

	public interface IRoleProviderFactory {
		IRoleProvider getRoleProvider(String userId);
	}

	/**
	 * RoleProviderFactorySingleton serves as a holder for a IRoleProviderFactory
	 * 
	 * @author tonibuenter
	 * 
	 */
	public static class RoleProviderFactorySingleton {

		private static IRoleProviderFactory instance;

		public static IRoleProviderFactory getInstance() {
			return instance;
		}

		public static void setInstance(IRoleProviderFactory instance) {
			RoleProviderFactorySingleton.instance = instance;
		}
	}

	public static interface IServiceRepository {

		ServiceEntry get(String serviceId) throws Exception;

		void add(ServiceEntry serviceEntry);

	}

	public static class JsonUtils {

		private static final Logger logger = Logger.getLogger(JsonUtils.class
		    .getName());

		public static String toJson(Object object) {
			Gson gson = new Gson();
			String jsonStr = gson.toJson(object);
			return jsonStr;
		}

		public static <T> T fromJson(String jsonStr, Class<T> classOfT) {
			Gson gson = new Gson();
			return gson.fromJson(jsonStr, classOfT);
		}

		public static String toJson(String name, String value) {
			Gson gson = new Gson();
			JsonObject jo = new JsonObject();
			jo.addProperty(name, value);

			return gson.toJson(jo);
		}

		public static String toJson(String name, Object value) {
			if (value instanceof String) {
				return toJson(name, (String) value);
			}
			Gson gson = new Gson();
			JsonObject o = new JsonObject();
			JsonElement e = gson.toJsonTree(value);
			o.add(name, e);
			return gson.toJson(o);
		}

		public static String toJson(JsonObject jsonObject) {
			return jsonObject.toString();
		}

		public static JsonObject toJsonObject(String s) {
			JsonParser jp = new JsonParser();
			JsonObject jo = jp.parse(s).getAsJsonObject();
			return jo;
		}

		public static String exception(String message) {
			return toJson("exception", message);
		}

		public static String message(String message) {
			return toJson("message", message);
		}

		public static Map<String, String> toStringMap(String jsonString) {
			JsonParser p = new JsonParser();
			JsonObject jsonObject = (JsonObject) p.parse(jsonString);
			Set<Map.Entry<String, JsonElement>> jsonElements = jsonObject.entrySet();
			Map<String, String> map = new HashMap<String, String>();
			for (Map.Entry<String, JsonElement> entry : jsonElements) {
				String key = entry.getKey();
				JsonElement je = entry.getValue();
				if (je.isJsonPrimitive()) {
					JsonPrimitive jp = (JsonPrimitive) je;
					if (jp.isBoolean() || jp.isNumber() || jp.isString()) {
						map.put(key, jp.getAsString());
					}
				}
			}
			return map;
		}

		public static String jsonNoop() {
			return toJson("NOOP");
		}

		public static class Parameters extends HashMap<String, String> {

			/**
	         * 
	         */
			private static final long serialVersionUID = 1L;

		}

		public static String jsonException(String message) {
			return toJson("exception", message);
		}

		public static String jsonMessage(String message) {
			return toJson("message", message);
		}

		public static <E> E toObject(String json, Class<E> claxx) {
			return new Gson().fromJson(json, claxx);
		}

		public static <E> E toObjectSilently(String json, Class<E> claxx) {
			try {
				return new Gson().fromJson(json, claxx);
			} catch (Exception e) {
			}
			return null;
		}

		//
		// LIST MAP start
		//
		private static Object _toListMapE(JsonElement je) {
			if (je.isJsonNull()) {
				return null;
			}
			if (je.isJsonPrimitive()) {
				return _toListMapP(je.getAsJsonPrimitive());
			}
			if (je.isJsonArray()) {
				return _toListMapA(je.getAsJsonArray());
			}
			if (je.isJsonObject()) {
				return _toListMapO(je.getAsJsonObject());
			}
			return null;
		}

		private static String _toListMapP(JsonPrimitive jp) {
			return jp.getAsString();
		}

		private static Object _toListMapA(JsonArray ja) {
			List<Object> list = new ArrayList<Object>(ja.size());
			for (int i = 0; i < ja.size(); i++) {
				JsonElement je = ja.get(i);
				Object v = _toListMapE(je);
				if (v != null) {
					list.add(v);
				}
			}
			return list;
		}

		private static Object _toListMapO(JsonObject jo) {
			Set<Map.Entry<String, JsonElement>> jes = jo.entrySet();
			Map<String, Object> map = new HashMap<String, Object>();
			for (Map.Entry<String, JsonElement> entry : jes) {
				String key = entry.getKey();
				Object v = _toListMapE(entry.getValue());
				if (v != null) {
					map.put(key, v);
				}
			}
			return map;
		}

		public static Object toListMap(String jsonString) {
			JsonParser p = new JsonParser();
			JsonElement je = (JsonObject) p.parse(jsonString);
			return _toListMapE(je);
		}

		public static Object toObject(String json, Type type) {
			try {
				return new Gson().fromJson(json, type);
			} catch (Exception e) {
				logger.severe(Utils.getStackTrace(e));
			}
			return null;
		}

		//
		// LIST MAP end
		//

		@SuppressWarnings("unchecked")
		public static <E> E[] toArray(String jsonString, Class<E> claxx) {

			JsonParser p = new JsonParser();
			E[] array = null;
			Gson g = new Gson();
			JsonElement je = (JsonElement) p.parse(jsonString);
			if (je.isJsonArray()) {
				JsonArray a = je.getAsJsonArray();
				array = (E[]) Array.newInstance(claxx, a.size());
				int index = 0;
				for (JsonElement e : a) {
					array[index] = g.fromJson(e, claxx);
					index++;
				}
			}
			return array;

		}

		public static <E> List<E> toList(String jsonString, Class<E> claxx) {
			return Utils.asList(toArray(jsonString, claxx));

		}
	}

	public static class MainQuery implements IQuery {

		// TODO idea how to create your own plugins like 'my-sql-extension:select
		// %SELECTLIST% where %WHERE% clause'

		public static final String constant_ = "constant:";
		public static final String setifnull_ = "set-if-null:";
		public static final String setifempty_ = "set-if-empty:";
		public static final String set_ = "set:";
		public static final String sql_ = "sql:";
		public static final String java_ = "java:";
		public static final String serviceId_ = "serviceId:";
		public static final String include_ = "include:";

		/**
		 * 
		 */
		private static final long serialVersionUID = 1L;

		private static final Logger logger = Logger.getLogger(MainQuery.class
		    .getName());

		private Map<String, Serializable> processStore = new HashMap<String, Serializable>();

		public void put(String key, Serializable value) {
			processStore.put(key, value);
		}

		public Serializable get(String key) {
			return processStore.get(key);
		}

		public RemoteQuery.Result run(RemoteQuery.Request request) {
			Result result = null;
			ProcessLog log = ProcessLog.Current();
			String serviceId = request.getServiceId();
			String userId = request.getUserId();
			if (Utils.isEmpty(userId)) {
				log.warn(
				    "Request object has no userId set. Process continues with userId="
				        + ANONYMOUS, logger);
				request.setUserId(ANONYMOUS);
				userId = request.getUserId();
			}
			// TODO better in the process object ?
			try {
				//
				ServiceEntry serviceEntry = ServiceRepository.getInstance().get(
				    serviceId);
				//
				// CHECK ACCESS
				//
				boolean hasAccess = false;
				Set<String> accessRoles = serviceEntry.getAccessRole();
				if (Utils.isEmpty(accessRoles)) {
					hasAccess = true;
				} else {
					for (String accessRole : accessRoles) {
						if (request.isInRole(accessRole)) {
							hasAccess = true;
							break;
						}
					}
				}
				if (hasAccess) {
					log.system("Access to " + serviceId + " for " + userId + " : ok",
					    logger);
				} else {
					log.warn("No access to " + serviceId + " for " + userId + " (roles: "
					    + accessRoles + ")", logger);
				}
				//
				// START PROCESSING STATEMENTS
				//

				log.system("ServiceEntry found for userId=" + userId + " is : "
				    + serviceEntry, logger);

				String serviceStatement = serviceEntry.getServiceStatement();

				String[] statements = Utils.tokenize(serviceStatement,
				    STATEMENT_DELIMITER, STATEMENT_ESCAPE);

				// parameterSupport = ParameterSupport.begin(con, sqRequest, sqEntry);

				for (String serviceStmt : statements) {
					serviceStmt = serviceStmt.trim();
					if (Utils.isEmpty(serviceStmt)) {
						log.warn("Empty query - no processing.", logger);
						continue;
					}
					// Resolve one level of include
					// TODO all levels
					// the include means acctually include service statement, no further
					// access checks are done
					if (serviceStmt.startsWith(include_)) {
						String serviceId2 = serviceStmt.substring(include_.length());
						ServiceEntry se2 = ServiceRepository.getInstance().get(serviceId2);
						serviceStmt = se2.getServiceStatement();
					}

					if (serviceStmt.startsWith(java_)) {
						result = processJava(serviceStmt, request);
					} else if (serviceStmt.startsWith(sql_)) {
						result = processSql(serviceEntry, serviceStmt, request);

					} else if (serviceStmt.startsWith(set_)) {
						try {
							String[] pairs = serviceStmt.substring(set_.length()).split(",");
							for (String pair : pairs) {
								String[] nv = pair.split("=");
								request.put(nv[0], nv[1]);
							}
						} catch (Exception e) {
							logger.severe(Utils.getStackTrace(e));
						}
					} else if (serviceStmt.startsWith(setifnull_)) {
						try {
							String[] pairs = serviceStmt.substring(setifnull_.length())
							    .split(",");
							for (String pair : pairs) {
								String[] nv = pair.split("=");
								if (request.getValue(nv[0]) == null) {
									request.put(nv[0], nv[1]);
								}
							}
						} catch (Exception e) {
							log.error("", e, logger);
						}
					} else if (serviceStmt.startsWith(serviceId_)) {
						String parentServiceId = serviceEntry.getServiceId();
						String subServiceId = serviceStmt.substring(serviceId_.length());
						request.setServiceId(subServiceId);
						result = run(request);
						request.setServiceId(parentServiceId);
					} else {
						result = processSql(serviceEntry, serviceStmt, request);
					}
				}

			} catch (Exception e) {
				log.error(e, logger);
			} finally {
				// ParameterSupport.release(parameterSupport);
			}
			if (result == null) {
				// TODO Default result object ?
			} else {
				result.setUserId(userId);
			}
			return result;
		}

		private Result processJava(String query, Request request)
		    throws InstantiationException, IllegalAccessException,
		    ClassNotFoundException {
			Result result = null;
			String className = query.substring(java_.length());

			Object serviceObject = Class.forName(className).newInstance();
			if (serviceObject instanceof IQuery) {
				IQuery service = (IQuery) serviceObject;
				result = service.run(request);
			} else {
				ProcessLog.Current().error(
				    "Class " + className + " is not an instance of "
				        + IQuery.class.getName(), logger);
			}

			return result;
		}

		public Result processSql(ServiceEntry serviceEntry, String query,
		    Request request) {

			Result result = null;
			// context
			ProcessLog log = ProcessLog.Current();

			PreparedStatement ps = null;
			ResultSet rs = null;

			QueryAndParams qap = null;

			String sql = query;
			if (sql.startsWith(sql_)) {
				sql = sql.substring(sql_.length());
			}
			log.system("sql before conversion: " + sql);

			qap = convertQuery(sql);
			sql = qap.questionMarkQuery;
			log.system("sql after conversion: " + sql);

			//
			// PREPARE SERVICE_STMT
			//

			List<Object> paramObjects = new ArrayList<Object>();

			for (String attributeName : qap.parameters) {
				String attributeValue = request.getValue(attributeName);
				if (attributeValue == null) {
					log.error("No value provided for parameter name:" + attributeName
					    + " (serviceId:" + request.getServiceId() + ")", logger);
				}
				paramObjects.add(attributeValue);
			}
			//
			// DEFAULT PARAMETER
			//

			//
			// FINALIZE SERVICE_STMT
			//

			DataSource ds = DataSources.getInstance().get(
			    serviceEntry.getDatasourceName());
			if (ds == null) {
				log.error("No DataSource found ServiceEntry.getDatasourceName="
				    + serviceEntry.getDatasourceName() + "!", logger);
			} else {
				Connection con = null;
				try {
					con = ds.getConnection();
					ps = con.prepareStatement(sql);
					int index = 0;
					for (Object v : paramObjects) {
						ps.setObject(++index, v);
					}
					boolean hasResultSet = ps.execute();
					if (hasResultSet) {
						rs = ps.getResultSet();
						result = Utils.buildResult(0, -1, rs);
						result.setName(serviceEntry.getServiceId());
					} else {
						log.system("ServiceEntry : " + serviceEntry.getServiceId()
						    + " 	rowsAffected:" + ps.getUpdateCount());
					}
				}

				catch (SQLException e) {
					String warnMsg = "Warning for service " + serviceEntry.getServiceId()
					    + " (parameters:" + qap.parameters + ") execption msg: "
					    + e.getMessage();
					log.warn(warnMsg);
					logger.warning(warnMsg);
				}

				catch (Exception e) {
					String errorMsg = "Error for service " + serviceEntry.getServiceId()
					    + " (parameters:" + qap.parameters + ") execption msg: "
					    + e.getMessage();
					log.error(errorMsg, logger);
					log.error(qap == null ? "-no qap-" : "parameterNameQuery="
					    + qap.parameterNameQuery + ", questionMarkQuery="
					    + qap.questionMarkQuery + ", parameters=" + qap.parameters,
					    logger);
				} finally {
					Utils.closeQuietly(rs);
					Utils.closeQuietly(ps);
					Utils.closeQuietly(con);
				}
			}
			return result;
		}

		//
		// NEW
		//

		static class QueryAndParams {
			String parameterNameQuery;
			String questionMarkQuery;
			List<String> parameters = new ArrayList<String>();

			@Override
			public String toString() {
				return "QueryAndParams [parameterNameQuery=" + parameterNameQuery
				    + ", questionMarkQuery=" + questionMarkQuery + ", parameters="
				    + parameters + "]";
			}

		}

		static QueryAndParams convertQuery(String query) {

			StringBuffer buf = new StringBuffer(query.length());

			boolean started = false;
			boolean prot = false;
			List<String> parameters = new ArrayList<String>();
			StringBuffer currentParam = new StringBuffer();
			for (char c : query.toCharArray()) {

				if (prot) {
					//
				} else {

					if (started) {
						if (Character.isJavaIdentifierPart(c)) {
							currentParam.append((char) c);
							continue;
						} else {
							started = false;
							parameters.add(currentParam.toString());
							currentParam = new StringBuffer();
							// buf.append(c);
							// continue;
						}
					}
					if (!started && c == ':') {
						started = true;
						buf.append('?');
						continue;
					}
				}

				if (c == '\'') {
					prot = !prot;
				}

				buf.append(c);
			}

			// end processing
			if (started) {
				parameters.add(currentParam.toString());
			}

			//
			QueryAndParams qap = new QueryAndParams();
			qap.parameterNameQuery = query;
			qap.questionMarkQuery = buf.toString();
			qap.parameters = parameters;
			return qap;
		}

	}

	//
	//
	// BuildIns ...
	//
	//

	public static class BuiltIns {

		public static class RegisterService implements IQuery {

			/**
			 * 
			 */
			private static final long serialVersionUID = 1L;

			@Override
			public Result run(Request request) {
				ProcessLog pLog = ProcessLog.Current();
				String serviceEntriesJson = request.getValue("serviceEntries");
				if (!Utils.isBlank(serviceEntriesJson)) {
					List<ServiceEntry> serviceEntries = JsonUtils.toList(
					    serviceEntriesJson, ServiceEntry.class);
					IServiceRepository sr = ServiceRepository.getInstance();
					for (ServiceEntry serviceEntry : serviceEntries) {
						sr.add(serviceEntry);
					}
				} else {
					String serviceId = request.getValue("serviceId");
					String serviceStatement = request.getValue("serviceStatement");
					String accessRoles = request.getValue("accessRoles");
					IServiceRepository sr = ServiceRepository.getInstance();
					sr.add(new ServiceEntry(serviceId, serviceStatement, accessRoles));
				}
				return new Result(pLog);
			}

		}
	}

	public static class ProcessLog implements Serializable {

		public static final int USER_OK_CODE = 10;
		public static final int USER_WARNING_CODE = 20;
		public static final int USER_ERROR_CODE = 30;
		public static final int OK_CODE = 1000;
		public static final int WARNING_CODE = 2000;
		public static final int ERROR_CODE = 3000;
		public static final int SYSTEM_CODE = 4000;

		public static final ThreadLocal<ProcessLog> TL = new ThreadLocal<ProcessLog>() {
			@Override
			protected ProcessLog initialValue() {
				return new ProcessLog();
			}
		};

		public static class LogEntry implements Serializable, Comparable<LogEntry> {

			private static final long serialVersionUID = 1L;

			private String msg;
			private int msgCode;
			private long time;

			private String state;

			public LogEntry(int msgCode, String msg, String state, long time,
			    String... data) {
				super();
				this.msg = msg;
				this.msgCode = msgCode;
				this.state = state;
				this.time = time;
			}

			public String getMsg() {
				return msg;
			}

			public String getState() {
				return this.state == null ? ProcessLog.OK : this.state;
			}

			public String getTime() {
				return Utils.toIsoDateTime(time);
			}

			public long getTimeMillis() {
				return time;
			}

			public int compareTo(LogEntry o) {
				int i = this.prio(state) - prio(o.state);
				if (i == 0) {
					i = this.time == o.time ? 0 : (this.time < o.time ? 1 : -1);
				}
				return i;
			}

			private int prio(String state) {
				if (Error.equals(state)) {
					return 1;
				}
				if (Warning.equals(state)) {
					return 2;
				}
				if (OK.equals(state)) {
					return 3;
				}
				if (System.equals(state)) {
					return 4;
				}
				return 5;
			}

			@Override
			public String toString() {
				return Utils.toIsoDateTime(time) + ":" + msg + "(" + state + ":"
				    + msgCode + ")";
			}

			@Override
			public int hashCode() {
				final int prime = 31;
				int result = 1;
				result = prime * result + ((msg == null) ? 0 : msg.hashCode());
				result = prime * result + ((state == null) ? 0 : state.hashCode());
				result = prime * result + (int) (time ^ (time >>> 32));
				return result;
			}

			@Override
			public boolean equals(Object obj) {
				if (this == obj)
					return true;
				if (obj == null)
					return false;
				if (getClass() != obj.getClass())
					return false;
				LogEntry other = (LogEntry) obj;
				if (msg == null) {
					if (other.msg != null)
						return false;
				} else if (!msg.equals(other.msg))
					return false;
				if (state == null) {
					if (other.state != null)
						return false;
				} else if (!state.equals(other.state))
					return false;
				if (time != other.time)
					return false;
				return true;
			}

			public int getMsgCode() {
				return msgCode;
			}

			public void setMsgCode(int msgCode) {
				this.msgCode = msgCode;
			}

		}

		/**
	     * 
	     */

		public static final String Warning = "Warning";
		public static final String Error = "Error";
		public static final String OK = "OK";
		public static final String System = "System";
		private static final long serialVersionUID = 1L;
		private static final String FILE_ID = "_FILE_ID_";
		private transient boolean silently = false;
		transient private long betweenTime = java.lang.System.currentTimeMillis();

		//
		// ProcessLog Object Level
		//

		private String state = OK;
		private List<LogEntry> logEntries = new ArrayList<LogEntry>();
		private Map<String, String> attributes = new HashMap<String, String>();

		private Long lastUsedTime;
		private String userId;
		private String name;

		public ProcessLog() {
		}

		public void infoUser(String line) {
			add(USER_OK_CODE, line, OK);
		}

		public void warnUser(String line, Logger logger) {
			add(USER_WARNING_CODE, line, Warning);
			_writeLog(line, Level.WARNING, logger);
		}

		public void errorUser(String line, Logger... logger) {
			add(USER_ERROR_CODE, line, Error);
			_writeLog(line, Level.SEVERE, logger);
		}

		public void error(String line, Logger... logger) {
			this.state = Error;
			add(ERROR_CODE, line, Error);
			_writeLog(line, Level.SEVERE, logger);
		}

		public void error(Throwable t, Logger... logger) {
			error(t.getMessage(), t, logger);
		}

		public void error(String line, Throwable t, Logger... logger) {
			this.state = Error;
			add(ERROR_CODE, line, Error);
			_writeLog(line + "\n" + Utils.getStackTrace(t), Level.SEVERE, logger);
		}

		public void error(int msgCode, String line, Logger... logger) {
			this.state = Error;
			add(msgCode, line, Error);
			_writeLog(line, Level.SEVERE, logger);
		}

		public void warn(String line, Logger... logger) {
			add(WARNING_CODE, line, Warning);
			_writeLog(line, Level.WARNING, logger);
		}

		public void system(String line, Logger... logger) {
			long now = java.lang.System.currentTimeMillis();
			long timeGap = now - this.betweenTime;
			this.betweenTime = now;
			add(SYSTEM_CODE, line + " [time gap : " + (timeGap / 1000) + "s]", System);
			_writeLog(line, Level.INFO, logger);
		}

		public void system(Throwable t, Logger... logger) {
			String line = Utils.getStackTrace(t);
			system(line, logger);
		}

		public void _writeLog(String line, Level level, Logger... loggers) {
			for (Logger logger : loggers)
				if (level == Level.WARNING) {
					logger.warning(line);
				} else if (level == Level.SEVERE) {
					logger.severe(line);
				} else {
					logger.info(line);
				}
		}

		private void add(int msgCode, String line, String level) {
			if (silently) {
				return;
			}
			//
			// Making sure that a ProcessLog Object does not use the same time
			// value twice.
			//
			if (lastUsedTime == null || lastUsedTime == 0) {
				lastUsedTime = java.lang.System.currentTimeMillis();
			}
			long time = java.lang.System.currentTimeMillis();
			if (time <= lastUsedTime) {
				time = lastUsedTime + 1;
			}
			lastUsedTime = time;
			logEntries.add(new LogEntry(msgCode, line, level, time));
		}

		public List<LogEntry> getLines() {
			return logEntries;
		}

		public boolean isOk() {
			return OK.equals(this.state);
		}

		public boolean isError() {
			return Error.equals(this.state);
		}

		public String getState() {
			return state;
		}

		public void setState(String state) {
			this.state = state;
		}

		public void setAttribute(String key, String value) {
			this.attributes.put(key, value);
		}

		public Map<String, String> getAttributes() {
			return attributes;
		}

		public String getAttribute(String key) {
			return this.attributes.get(key);
		}

		public void setUserId(String userId) {
			this.userId = userId;
		}

		public String getUserId() {
			return this.userId;
		}

		public void setSilently(boolean silently) {
			this.silently = silently;
		}

		public boolean isSilently() {
			return silently;
		}

		public void setName(String name) {
			this.name = name;
		}

		public String getName() {
			return this.name;
		}

		public String getId() {
			return this.attributes.get(FILE_ID);
		}

		public void setId(String id) {
			this.attributes.put(FILE_ID, id);
		}

		public static ProcessLog Current() {
			return TL.get();
		}

		public static ProcessLog newOnThread(String userId) {
			ProcessLog rl = new ProcessLog();
			TL.set(rl);
			return TL.get();
		}

	}

	/**
	 * <h2>Request for service process</h2>
	 * 
	 * A request object is similar to a HTTP request. Both have parameters string
	 * and file base. Parameter exist on different scopes or as we call them here
	 * 'levels'.
	 * 
	 * <h3>Parameters Levels</h3>
	 * 
	 * <h4>Initial Parameters</h4>
	 * 
	 * These are parameters set during the creation of the request, usually the
	 * can not be overriden later (during processing of the request)
	 * 
	 * <h4>Request Parameters</h4>
	 * 
	 * Request parameters such as Http Request
	 * 
	 * 
	 * Parameters <h4>Inter Request</h4>
	 * 
	 * A set of parameters for inter request communication.
	 * 
	 * <h4>Session Parameters</h4>
	 * 
	 * Session parameters are similar to request attributes Session : parameters
	 * from HttpSession Application : parameters from the ServletContext often
	 * called Application Context
	 * 
	 * $Parameter: the $Paramters like $USERID, $USERTID (an optional technical
	 * user id), $SERVICEID, $SESSIONID, $C
	 * 
	 * @author Toni A. Bünter, OOIT.com AG
	 * 
	 */
	public static class Request implements Serializable {

		/**
	 * 
	 */
		private static final long serialVersionUID = 1L;

		public static int INITIAL = 0;
		public static int REQUEST = 1;
		public static int INTER_REQUEST = 2;
		public static int SESSION = 3;
		public static int APPLICATION = 4;

		private String serviceId;
		private String userId;
		private IRoleProvider roleProvider;
		private Set<String> roles = new HashSet<String>();
		@SuppressWarnings("unchecked")
		private Map<String, String>[] parameterList = new Map[4];
		{
			for (int i = 0; i < 4; i++)
				parameterList[i] = new HashMap<String, String>();
		}

		@SuppressWarnings("unchecked")
		private Map<String, Serializable>[] fileList = new Map[4];
		{
			for (int i = 0; i < 4; i++)
				fileList[i] = new HashMap<String, Serializable>();
		}

		public Request(String serviceId) {
			this.serviceId = serviceId;
		}

		public String getServiceId() {
			return serviceId;
		}

		public void setServiceId(String serviceId) {
			this.serviceId = serviceId;
		}

		public Map<String, String> getParameters(int level) {
			return this.parameterList[level];
		}

		public String getValue(int level, String key) {
			return this.parameterList[level].get(key);
		}

		public String getValue(String key) {
			for (int level = 0; level < APPLICATION; level++) {
				if (parameterList[level].containsKey(key)) {
					return getValue(level, key);
				}
			}
			return null;
		}

		public String put(int level, String key, String value) {
			return this.parameterList[level].put(key, value);
		}

		public String put(String key, String value) {
			return this.parameterList[REQUEST].put(key, value);
		}

		public IRoleProvider getRoleProvider() {
			return roleProvider;
		}

		public void setRoleProvider(IRoleProvider roleProvider) {
			this.roleProvider = roleProvider;
		}

		public void addRole(String role) {
			this.roles.add(role);
		}

		public boolean isInRole(String role) {
			if (roles.contains(role)) {
				return true;
			}
			if (roleProvider != null && roleProvider.isInRole(role)) {
				return true;
			}
			return false;
		}

		public String getUserId() {
			return userId;
		}

		public void setUserId(String userId) {
			this.userId = userId;
		}

	}

	public static class Result {

		private static final Logger logger = Logger.getLogger(Result.class
		    .getName());

		//
		// Object Level
		//
		private String name = "";
		private String userId = "-";
		private int size = 0;
		private int from = 0;
		private int totalCount = 0;

		private List<List<String>> table = new ArrayList<List<String>>();
		private List<String> header = new ArrayList<String>();
		private String exception = null;
		private ProcessLog processLog = null;

		public static boolean USE_CAMEL_CASE_FOR_RESULT_HEADER = true;

		public Result(String... header) {
			this.header = Arrays.asList(header);
		}

		public Result(int from, int totalCount, List<String> header,
		    List<List<String>> table) {
			super();
			this.from = from;
			this.totalCount = totalCount;
			this.header = header;
			this.table = table;
			update();
		}

		public Result(ProcessLog processLog) {
			super();
			this.processLog = processLog;
			update();
		}

		public void update() {
			size = this.table.size();
			totalCount = Math.max(from + size, totalCount);
		}

		public void addNameValue(String name, String value) {
			header.add(name);
			table.add(Arrays.asList(value));
		}

		public void addNameValue(Map<String, String> values) {
			for (String key : values.keySet()) {
				addNameValue(key, values.get(key));
			}
		}

		public void addRow(String... row) {
			table.add(Arrays.asList(row));
		}

		public String getName() {
			return name;
		}

		public String getException() {
			return exception;
		}

		public Map<String, String> getFirstRowAsMap() {
			Map<String, String> map = new HashMap<String, String>();
			if (table != null && table.size() > 0) {
				List<String> row = table.get(0);
				for (int i = 0; i < header.size(); i++) {
					map.put(header.get(i), row.get(i));
				}

			}
			return map;
		}

		public String getSingleValue() {
			if (!Utils.isEmpty(table) && !Utils.isEmpty(table.get(0))) {
				return table.get(0).get(0);
			}
			return null;
		}

		public String getValue(int rowIndex, String head) {
			int index = getColIndex(head);
			return (index > -1 && rowIndex < table.size()) ? table.get(rowIndex).get(
			    index) : null;
		}

		public int getColIndex(String head) {
			for (int i = 0; i < header.size(); i++) {
				if (head.equalsIgnoreCase(header.get(i))) {
					return i;
				}
			}
			return -1;
		}

		public Map<String, Map<String, String>> toMap(String keyHead) {
			Map<String, Map<String, String>> map = new HashMap<String, Map<String, String>>();
			int index = getColIndex(keyHead);

			for (int i = 0; i < table.size(); i++) {
				List<String> row = table.get(i);
				Map<String, String> subMap = new HashMap<String, String>();
				String key = row.get(index);
				map.put(key, subMap);
				for (int j = 0; j < row.size(); j++) {
					subMap.put(this.header.get(j), row.get(j));
				}
			}
			return map;
		}

		public Map<String, List<String>> toMap(int... indexes) {
			Map<String, List<String>> map = new HashMap<String, List<String>>();
			for (int j = 0; j < table.size(); j++) {
				List<String> value = table.get(j);
				String key = createMapKey(value, indexes);
				map.put(key, value);
			}
			return map;
		}

		public Map<String, String> toTwoColumnMap(String keyHeader,
		    String valueHeader) {
			Map<String, String> map = new HashMap<String, String>();
			int keyIndex = getColIndex(keyHeader);
			int valueIndex = getColIndex(valueHeader);
			for (int j = 0; j < table.size(); j++) {
				List<String> row = table.get(j);
				String key = row.get(keyIndex);
				String value = row.get(valueIndex);
				map.put(key, value);
			}
			return map;
		}

		public Map<String, List<List<String>>> toMultiMap(int... indexes) {
			Map<String, List<List<String>>> map = new HashMap<String, List<List<String>>>();
			for (int j = 0; j < table.size(); j++) {
				List<String> value = table.get(j);
				String key = createMapKey(value, indexes);
				List<List<String>> rows = map.get(key);
				if (rows == null) {
					rows = new ArrayList<List<String>>();
					map.put(key, rows);
				}
				rows.add(value);
			}
			return map;
		}

		public static String createMapKey(List<String> row, int... indexes) {
			String key = "";
			for (int i = 0; i < indexes.length; i++) {
				if (i == 0) {
					key = row.get(indexes[i]);
				} else {
					key = key + "-" + row.get(indexes[i]);
				}
			}
			return key;
		}

		public void setException(String exception) {
			this.exception = exception;
		}

		public Result(Exception e) {
			this.exception = e.getMessage();
		}

		public String getUserId() {
			return userId;
		}

		public int getSize() {
			return size;
		}

		public List<List<String>> getTable() {
			return table;
		}

		public List<String> getHeader() {
			return header;
		}

		public int getTotalCount() {
			return totalCount;
		}

		public int getFrom() {
			return from;
		}

		public void setFrom(int from) {
			this.from = from;
		}

		public void setUserId(String userId) {
			this.userId = userId;
		}

		@Override
		public String toString() {
			String tableString = "[";
			for (int i = 0; i < table.size(); i++) {
				if (i != 0) {
					tableString += ", ";
				}
				List<String> row = table.get(i);
				tableString += row;
			}
			tableString += "]";
			return "Result [name=" + name + ", userId=" + userId + ", size=" + size
			    + ", from=" + from + ", totalCount=" + totalCount + ", table="
			    + tableString + ", header=" + header + ", exception=" + exception
			    + "]";
		}

		public String getSingleValue(String head) {
			int index = getColIndex(head);
			return (index > -1 && table.size() > 0) ? table.get(0).get(index) : null;
		}

		/**
		 * Creates a list of objects. The data is filled by applying corresponding
		 * set methods. E.g. for Columnt FIRST_NAME it will try to call
		 * setFIRST_NAME(String) or setFirstName(String)
		 * 
		 * @param <E>
		 * @param claxx
		 * @return
		 * @throws Exception
		 */
		public <E> List<E> asList(Class<E> claxx) {
			List<E> list = new ArrayList<E>();
			try {
				if (table.size() == 0) {
					return list;
				}

				for (int i = 0; i < table.size(); i++) {
					E e = claxx.newInstance();
					list.add(e);
				}

				int colIndex = 0;
				for (String head : header) {
					if (Utils.isBlank(head)) {
						head = "" + colIndex;
					}
					String[] res = Utils.createSetGetNames(head);
					Method m = null;
					for (String setGetName : res) {
						try {
							m = claxx.getMethod("set" + setGetName, String.class);
						} catch (Exception e) {
							logger.info("Did not find method: " + setGetName + ": ->" + e);
						}
						if (m != null) {
							break;
						}
					}
					if (m != null) {
						int rowIndex = 0;
						for (E e : list) {
							m.invoke(e, table.get(rowIndex).get(colIndex));
							rowIndex++;
						}
					}
					colIndex++;
				}
			} catch (Exception e) {
				logger.severe(Utils.getStackTrace(e));
			}
			return list;
		}

		public <E> Map<String, E> asMap(String keyProperty, Class<E> claxx) {
			Map<String, E> map = new HashMap<String, E>();
			try {
				List<E> list = this.asList(claxx);
				map = Utils.asMap(keyProperty, list);
			} catch (Exception e) {
				logger.severe(Utils.getStackTrace(e));
			}
			return map;
		}

		public <E> Map<String, List<E>> asMapList(String property, Class<E> claxx) {
			Map<String, List<E>> map = new HashMap<String, List<E>>();
			try {
				List<E> list = this.asList(claxx);
				String methodName = "get" + property.substring(0, 1).toUpperCase()
				    + property.substring(1);
				Method m = claxx.getMethod(methodName);
				for (E e : list) {
					String key = m.invoke(e).toString();
					List<E> listElement = map.get(key);
					if (listElement == null) {
						listElement = new ArrayList<E>();
						map.put(key, listElement);
					}
					listElement.add(e);
				}
			} catch (Exception e) {
				logger.severe(Utils.getStackTrace(e));
			}
			return map;
		}

		public void setName(String name) {
			this.name = name;
		}

		public ProcessLog getProcessLog() {
			return processLog;
		}

		public void setProcessLog(ProcessLog processLog) {
			this.processLog = processLog;
		}

	}

	public static class ServiceEntry implements Serializable {

		public static final String SYSTEM_ROLENAME = "SYSTEM";

		/**
     * 
     */
		private static final long serialVersionUID = 1L;

		private String serviceId;
		private String serviceStatement;
		private Set<String> accessRoles;

		private String datasourceName = DEFAULT_DATASOURCE_NAME;

		public ServiceEntry(String serviceId, String serviceStatement,
		    String accessRoles) {
			super();
			this.serviceId = serviceId;
			this.serviceStatement = serviceStatement;
			if (!Utils.isEmpty(accessRoles)) {
				String[] r = accessRoles.split(",");
				this.accessRoles = new HashSet<String>(Arrays.asList(r));
			}
		}

		public ServiceEntry(String serviceId, String serviceStatement,
		    Set<String> accessRoles) {
			super();
			this.serviceId = serviceId;
			this.serviceStatement = serviceStatement;
			this.accessRoles = accessRoles;
		}

		public String getServiceId() {
			return serviceId;
		}

		public String getServiceStatement() {
			return serviceStatement;
		}

		public Set<String> getAccessRole() {
			return this.accessRoles;
		}

		public String getDatasourceName() {
			return datasourceName;
		}

		public void setDatasourceName(String datasourceName) {
			this.datasourceName = datasourceName;
		}

		@Override
		public String toString() {
			return "ServiceEntry [serviceId=" + serviceId + ", serviceStatement="
			    + serviceStatement + ", accessRoles=" + accessRoles
			    + ", datasourceName=" + datasourceName + "]";
		}

	}

	public static class ServiceRepository implements IServiceRepository {

		private static final Logger logger = Logger
		    .getLogger(ServiceRepository.class.getName());

		private static ServiceRepository instance;

		public static ServiceRepository getInstance() {
			if (instance == null) {
				throw new RuntimeException(ServiceRepository.class.getName()
				    + " not yet created. Please create an instance first!");
			}
			return instance;
		}

		public static void closeAll() {
			if (instance != null) {
				instance = null;
			}
		}

		private ServiceRepositorySql sql;
		private ServiceRepositoryJson json;

		/**
		 * Creates a ServiceRepository and assign the static instance with it.
		 * 
		 * @param ds
		 *          datasource for ServiceEntries.
		 * @param tableName
		 *          table name (including possible schema prefix)
		 */
		public ServiceRepository(DataSource ds, String tableName) {
			logger.fine("try to create ServiceRepositorySql");
			sql = new ServiceRepositorySql(ds, tableName);
			instance = this;
		}

		/**
		 * Creates a JsonServiceRepository and assign the static instance with it.
		 * 
		 * @param jsonString
		 */
		public ServiceRepository(String jsonString) {
			logger.info("Try to create ServiceRepositoryJson ...");
			json = new ServiceRepositoryJson(jsonString);
			instance = this;
		}

		@Override
		public ServiceEntry get(String serviceId) throws SQLException, Exception {
			ServiceEntry se = null;
			if (sql != null) {
				se = sql.get(serviceId);
			} else if (json != null) {
				se = json.get(serviceId);
			} else {
				throw new RuntimeException(
				    "No service repository initialized. Currently 'sql' and 'json' service repositories are available.");
			}
			return se;
		}

		@Override
		public void add(ServiceEntry serviceEntry) {
			if (sql != null) {
				sql.add(serviceEntry);
			} else if (json != null) {
				json.add(serviceEntry);
			} else {
				throw new RuntimeException(
				    "No service repository initialized. Currently 'sql' and 'json' service repositories are available.");
			}
		}
	}

	public static class ServiceRepositoryJson implements IServiceRepository {

		public static String COL_SERVICE_STATEMENT = "SERVICE_STATEMENT";
		public static String COL_ACCESS_ROLES = "ACCESS_ROLES";
		public static String COL_SERVICE_ID = "SERVICE_ID";
		/**
		 * Version 2.0
		 */
		public static String COL_DATASOURCE_NAME = "DATASOURCE_NAME";

		private static final Logger logger = Logger
		    .getLogger(ServiceRepositoryJson.class.getName());

		private static ServiceRepositoryJson instance;

		public static ServiceRepositoryJson getInstance() {
			if (instance == null) {
				throw new RuntimeException(ServiceRepositoryJson.class.getName()
				    + " not yet created. Please create an instance first!");
			}
			return instance;
		}

		private List<ServiceEntry> entries;

		/**
		 * Creating a ServiceRepositoryJson object
		 * 
		 * @param string
		 *          file path or json string
		 * @param charsetName
		 *          encoding for reading the file
		 */
		@SuppressWarnings("unchecked")
		public ServiceRepositoryJson(String string, String charsetName) {
			File f = new File(string);
			String jsonString;
			if (f.exists()) {
				jsonString = Utils.readFileToString(f, charsetName);
			} else {
				jsonString = string;
			}
			if (Utils.isBlank(jsonString)) {
				entries = new ArrayList<ServiceEntry>();
			}
			Type tt = new TypeToken<List<ServiceEntry>>() {
			}.getType();
			entries = (List<ServiceEntry>) JsonUtils.toObject(jsonString, tt);
			logger.fine("found " + entries.size() + " service entries.");
		}

		/**
		 * Creating a ServiceRepositoryJson object
		 * 
		 * @param string
		 *          file path or json string
		 */
		public ServiceRepositoryJson(String string) {
			this(string, null);
		}

		public ServiceEntry get(String serviceId) throws SQLException {
			for (ServiceEntry sid : entries) {
				if (sid.getServiceId().equals(serviceId)) {
					return sid;
				}
			}
			return null;
		}

		@Override
		public void add(ServiceEntry serviceEntry) {
			entries.add(serviceEntry);
		}
	}

	public static class ServiceRepositorySql implements IServiceRepository {

		public static String COL_SERVICE_STATEMENT = "SERVICE_STATEMENT";
		public static String COL_ACCESS_ROLES = "ACCESS_ROLES";
		public static String COL_SERVICE_ID = "SERVICE_ID";
		/**
		 * Version 2.0
		 */
		public static String COL_DATASOURCE_NAME = "DATASOURCE_NAME";

		private static final Logger logger = Logger
		    .getLogger(ServiceRepositorySql.class.getName());

		private DataSource ds;
		private String tableName;
		private String selectQuery;

		public ServiceRepositorySql(DataSource ds, String tableName) {
			this.ds = ds;
			if (tableName.split(" ").length > 1) {
				selectQuery = tableName;
			} else {
				this.tableName = tableName;
			}
		}

		public ServiceEntry get(String serviceId) throws SQLException {
			Connection con = null;
			PreparedStatement ps = null;
			ResultSet rs = null;
			ServiceEntry se = null;
			try {
				con = getConnection();
				String sql = selectQuery != null ? selectQuery : "select "
				    + COL_SERVICE_STATEMENT + ", " + COL_ACCESS_ROLES + " from "
				    + tableName + " where " + COL_SERVICE_ID + " = ?";
				ps = con.prepareStatement(sql);
				rs = ps.getResultSet();
				if (rs.next()) {
					String serviceStatement = rs.getString(COL_SERVICE_STATEMENT);
					String accessRoles = rs.getString(COL_ACCESS_ROLES);
					se = new RemoteQuery.ServiceEntry(serviceId, serviceStatement,
					    accessRoles);
					logger.info("Found " + se);
					return se;
				}
			} finally {
				Utils.closeQuietly(rs);
				Utils.closeQuietly(ps);
				returnConnection(con);
			}
			return null;
		}

		private Connection getConnection() throws SQLException {
			if (ds == null) {
				throw new RuntimeException(ServiceRepositorySql.class.getName()
				    + " DataSource is null. Please provide a DataSource!");
			}
			return ds.getConnection();
		}

		private void returnConnection(Connection con) throws SQLException {
			if (ds != null) {
				con.close();
			}
		}

		@Override
		public void add(ServiceEntry serviceEntry) {
			// TODO Auto-generated method stub

		}
	}

	static class StringTokenizer2 {

		private int index = 0;
		private String[] tokens = null;
		private StringBuffer buf;
		private boolean ignoreWhiteSpace = true;

		public StringTokenizer2(String string, char del, char esc) {
			this(string, del, esc, true);
		}

		public StringTokenizer2(String string, char del, char esc,
		    boolean ignoreWhiteSpace) {
			this.ignoreWhiteSpace = ignoreWhiteSpace;
			// first we count the tokens
			int count = 1;
			boolean inescape = false;
			char c;
			buf = new StringBuffer();
			for (int i = 0; i < string.length(); i++) {
				c = string.charAt(i);
				if (c == del && !inescape) {
					count++;
					continue;
				}
				if (c == esc && !inescape) {
					inescape = true;
					continue;
				}
				inescape = false;
			}
			tokens = new String[count];

			// now we collect the characters and create all tokens
			int k = 0;
			for (int i = 0; i < string.length(); i++) {
				c = string.charAt(i);
				if (c == del && !inescape) {
					tokens[k] = buf.toString();
					buf.delete(0, buf.length());
					k++;
					continue;
				}
				if (c == esc && !inescape) {
					inescape = true;
					continue;
				}
				buf.append(c);
				inescape = false;
			}
			tokens[k] = buf.toString();
		}

		public boolean hasMoreTokens() {
			return index < tokens.length;
		}

		public String nextToken() {
			String token = tokens[index];
			index++;
			return ignoreWhiteSpace ? token.trim() : token;
		}

		public int countTokens() {
			return tokens.length;
		}

		public String[] getAllTokens() {
			return tokens;
		}

		public static void main(String[] args) {
			StringTokenizer2 tokenizer = new StringTokenizer2(args[0], ';', '$');

			System.err.println("countTokens: " + tokenizer.countTokens());

			while (tokenizer.hasMoreTokens()) {
				System.err.println("ST2: " + tokenizer.nextToken());
			}
		}

		/**
		 * Static convenience method for converting a string directly into an array
		 * of String by using the delimiter and escape character as specified.
		 */
		public static String[] toTokens(String line, char delim, char escape) {
			StringTokenizer2 tokenizer = new StringTokenizer2(line, delim, escape);
			return tokenizer.getAllTokens();
		}

		/**
		 * Create a string with the delimiter an escape character as specified.
		 */
		public static String toString(String[] tokens, char delim, char escape) {

			String token = null;
			int i, j;
			char c;
			StringBuffer buff = new StringBuffer();

			for (i = 0; i < tokens.length; i++) {
				token = tokens[i];
				for (j = 0; j < token.length(); j++) {
					c = token.charAt(j);
					if (c == escape || c == delim) {
						buff.append(escape);
					}
					buff.append(c);
				}
				buff.append(delim);
			}
			if (buff.length() > 0)
				buff.setLength(buff.length() - 1);
			return buff.toString();
		}

	}

	public static class Utils {

		private static final Logger logger = Logger
		    .getLogger(Utils.class.getName());

		public static final String ISO_DATE_PATTERN_yyyy_MM_dd = "yyyy-MM-dd";
		public static final String ISO_DATE_PATTERN_yyyy_MM_dd__HH_mm = "yyyy-MM-dd HH:mm";
		public static final String ISO_DATE_PATTERN_yyyy_MM_dd__HH_mm_ss = "yyyy-MM-dd HH:mm:ss SSS";
		public static final String ISO_DATE_PATTERN_HH_mm = "HH:mm";

		public static final String ISO_DATE_PATTERN_yyyy_MM_dd__HH_mm_ss__SSS = "yyyy-MM-dd HH:mm:ss SSS";

		public static ThreadLocal<SimpleDateFormat> IsoDateTL = new ThreadLocal<SimpleDateFormat>() {
			@Override
			protected SimpleDateFormat initialValue() {
				return new SimpleDateFormat(ISO_DATE_PATTERN_yyyy_MM_dd);
			}
		};

		public static ThreadLocal<SimpleDateFormat> IsoDateTimeTL = new ThreadLocal<SimpleDateFormat>() {
			@Override
			protected SimpleDateFormat initialValue() {
				return new SimpleDateFormat(ISO_DATE_PATTERN_yyyy_MM_dd__HH_mm);
			}
		};

		public static ThreadLocal<SimpleDateFormat> IsoDateTimeFullTL = new ThreadLocal<SimpleDateFormat>() {
			@Override
			protected SimpleDateFormat initialValue() {
				return new SimpleDateFormat(ISO_DATE_PATTERN_yyyy_MM_dd__HH_mm_ss);
			}
		};

		public static ThreadLocal<SimpleDateFormat> IsoTimeTL = new ThreadLocal<SimpleDateFormat>() {
			@Override
			protected SimpleDateFormat initialValue() {
				return new SimpleDateFormat(ISO_DATE_PATTERN_HH_mm);
			}
		};

		public static final String nowIsoDate() {
			return IsoDateTL.get().format(new Date());
		}

		public static final String nowIsoDateTimeFull() {
			return IsoDateTimeFullTL.get().format(new Date());
		}

		public static final String nowIsoDateTime() {
			return IsoDateTimeTL.get().format(new Date());
		}

		public static final String nowIsoTime() {
			return IsoTimeTL.get().format(new Date());
		}

		public static Date parseDate(String date) throws ParseException {
			return IsoDateTL.get().parse(date);
		}

		public static Date parseTime(String date) throws ParseException {
			return IsoTimeTL.get().parse(date);
		}

		public static Date parseDateTime(String date) throws ParseException {
			return IsoDateTimeTL.get().parse(date);
		}

		public static String formatToDateTime(Date time) {
			return IsoDateTimeTL.get().format(time);
		}

		public static String formatToDateTime(long time) {
			return formatToDateTime(new Date(time));
		}

		public static String formatToDate(Date time) {
			return IsoDateTL.get().format(time);
		}

		public static String formatToDate(long time) {
			return formatToDate(new Date(time));
		}

		public static String formatToTime(Date time) {
			return IsoTimeTL.get().format(time);
		}

		public static String formatToTime(long time) {
			return formatToTime(new Date(time));
		}

		public static Date toDate(String isoDateString) throws ParseException {
			return IsoDateTL.get().parse(isoDateString);
		}

		public static Date tryToDate(String isoDateString) {
			try {
				return toDate(isoDateString);
			} catch (Exception e) {
				return null;
			}
		}

		public static boolean isDate(String isoDateString) {
			Date d = null;
			try {
				d = IsoDateTL.get().parse(isoDateString);
			} catch (Exception e) {
			}
			return d != null;
		}

		//
		// public static long toTimeInMillis(String isoDateTimeString) {
		// try {
		// if (isoDateTimeString.length() == ISO_DATE_PATTERN_yyyy_MM_dd.length()) {
		// return toDateInMillis(isoDateTimeString);
		// }
		// return IsoDateTimeTL.get().parse(isoDateTimeString).getTime();
		// } catch (ParseException e) {
		// logger.severe(Utils.getStacktrace(e));
		// }
		// return 0;
		// }

		public static String toIsoDateTime(Date date) {
			return IsoDateTimeTL.get().format(date);
		}

		public static String toIsoDateTime(long date) {
			return IsoDateTimeTL.get().format(new Date(date));
		}

		public static int getCurrentHour() {
			Calendar c = Calendar.getInstance();
			c.setTime(new Date());
			return c.get(Calendar.HOUR_OF_DAY);
		}

		public static int getMinuteOfTheDay() {
			Calendar c = Calendar.getInstance();
			c.setTime(new Date());
			return 60 * c.get(Calendar.HOUR_OF_DAY) + c.get(Calendar.MINUTE);
		}

		public static String toIsoDate(Date date) {
			return IsoDateTL.get().format(date);
		}

		//
		// public static Date toDate2_(String isoDateString_or_numberOfdays) {
		// Date date = null;
		// try {
		// if (NumberUtils.isDigits(isoDateString_or_numberOfdays)) {
		// int d = Integer.parseInt(isoDateString_or_numberOfdays);
		// GregorianCalendar gc = new GregorianCalendar(1900,
		// Calendar.JANUARY, 1);
		// gc.add(Calendar.DATE, d - 2);
		// date = gc.getTime();
		// } else {
		// date = toDate(isoDateString_or_numberOfdays);
		// }
		// } catch (Exception e) {
		// logger.info(e);
		// }
		// return date;
		// }

		public static String toIsoDate(long date) {
			return IsoDateTL.get().format(new Date(date));
		}

		public static long toDateInMillis(String isoDateString) {
			try {
				return IsoDateTL.get().parse(isoDateString).getTime();
			} catch (ParseException e) {
				logger.severe(getStackTrace(e));
			}
			return 0;
		}

		public static long toTimeInMillis(String isoDateTimeString) {
			long res = 0;
			try {
				res = IsoDateTimeFullTL.get().parse(isoDateTimeString).getTime();
				return res;
			} catch (ParseException e) {
			}
			try {
				res = IsoDateTimeSecTL.get().parse(isoDateTimeString).getTime();
				return res;
			} catch (ParseException e) {
			}
			try {
				res = IsoDateTimeTL.get().parse(isoDateTimeString).getTime();
				return res;
			} catch (ParseException e) {
			}
			try {
				res = IsoDateTL.get().parse(isoDateTimeString).getTime();
				return res;
			} catch (ParseException e) {
			}
			logger.severe("Parse error for " + isoDateTimeString + ". Return 0.");
			return res;
		}

		public static String toIsoDateTimeSec(Date date) {
			return IsoDateTimeSecTL.get().format(date);
		}

		public static ThreadLocal<SimpleDateFormat> IsoDateTimeSecTL = new ThreadLocal<SimpleDateFormat>() {
			@Override
			protected SimpleDateFormat initialValue() {
				return new SimpleDateFormat(ISO_DATE_PATTERN_yyyy_MM_dd__HH_mm_ss);
			}
		};

		public static String nowIsoDateTimeSec() {
			return IsoDateTimeSecTL.get().format(new Date());
		}

		public static String getStackTrace(Throwable t) {
			StringBuffer buf = new StringBuffer();
			if (t != null) {
				buf.append(" <");
				buf.append(t.toString());
				buf.append(">");

				java.io.StringWriter sw = new java.io.StringWriter(1024);
				java.io.PrintWriter pw = new java.io.PrintWriter(sw);
				t.printStackTrace(pw);
				pw.close();
				buf.append(sw.toString());
			}
			return buf.toString();
		}

		//
		// STRING UTILS
		//

		public static String rne(String string, String defaultContentType) {
			if (isEmpty(string)) {
				return defaultContentType;
			} else {
				return string;
			}
		}

		public static String rnn(Object o) {
			return o == null ? "" : o.toString();
		}

		public static boolean isEmpty(String str) {
			return str == null || str.length() == 0;
		}

		public static boolean isBlank(String str) {
			int strLen;
			if (str == null || (strLen = str.length()) == 0) {
				return true;
			}
			for (int i = 0; i < strLen; i++) {
				if ((Character.isWhitespace(str.charAt(i)) == false)) {
					return false;
				}
			}
			return true;
		}

		public static String[] createSetGetNames(String head) {
			String normalCase = head.substring(0, 1).toUpperCase()
			    + head.substring(1);
			String camelCase = camelCase(head);
			camelCase = camelCase.substring(0, 1).toUpperCase()
			    + camelCase.substring(1);
			String[] res = { normalCase, camelCase };
			return res;
		}

		public static String camelCase(String columnName) {
			StringBuilder res = new StringBuilder();
			boolean upper = false;
			for (int i = 0; i < columnName.length(); i++) {
				char c = columnName.charAt(i);
				if (c == '_' || !Character.isLetterOrDigit(c)) {
					upper = true;
					continue;
				}
				if (upper) {
					res.append(Character.toUpperCase(c));
					upper = false;
				} else {
					res.append(Character.toLowerCase(c));
				}
			}
			return res.toString();
		}

		public static String[] tokenize(String string, char del, char esc) {
			if (isEmpty(string)) {
				return new String[0];
			}
			// first we count the tokens
			int count = 1;
			boolean inescape = false;
			char c;
			StringBuffer buf = new StringBuffer();
			for (int i = 0; i < string.length(); i++) {
				c = string.charAt(i);
				if (c == del && !inescape) {
					count++;
					continue;
				}
				if (c == esc && !inescape) {
					inescape = true;
					continue;
				}
				inescape = false;
			}
			String[] tokens = new String[count];

			// now we collect the characters and create all tokens
			int k = 0;
			for (int i = 0; i < string.length(); i++) {
				c = string.charAt(i);
				if (c == del && !inescape) {
					tokens[k] = buf.toString();
					buf.delete(0, buf.length());
					k++;
					continue;
				}
				if (c == esc && !inescape) {
					inescape = true;
					continue;
				}
				buf.append(c);
				inescape = false;
			}
			tokens[k] = buf.toString();
			return tokens;
		}

		public static Set<String> asSet(String... values) {
			Set<String> set = new HashSet<String>();
			if (values != null) {
				for (int i = 0; i < values.length; i++) {
					set.add(values[i]);
				}
			}
			return set;
		}

		public static Set<String> asSet(String string, char del, char esc) {
			return asSet(tokenize(string, del, esc));
		}

		public static <E> List<E> asList(E... elements) {
			ArrayList<E> list = new ArrayList<E>(1);
			for (E e : elements) {
				list.add(e);
			}
			return list;
		}

		//
		// IO UTILS
		//

		public static void copy(Reader in, Writer out) throws IOException {
			int c;
			while ((c = in.read()) != -1) {
				out.write(c);
			}
		}

		public static void closeQuietly(Writer w) {
			try {
				w.close();
			} catch (Exception e) {
				// ignore
			}
		}

		public static void closeQuietly(Reader r) {
			try {
				r.close();
			} catch (Exception e) {
				// ignore
			}
		}

		public static String readFileToString(File file, String charsetName) {
			BufferedReader r = null;
			charsetName = charsetName == null ? encoding : charsetName;
			StringBuffer buf = new StringBuffer((int) file.length());
			try {
				r = new BufferedReader(new InputStreamReader(new FileInputStream(file),
				    charsetName));
				int c = 0;
				while ((c = r.read()) != -1) {
					buf.append(c);
				}
			} catch (Exception e) {
				logger.severe(getStackTrace(e));
			} finally {
				closeQuietly(r);
			}
			return buf.toString();
		}

		//
		//
		// DB UTILS
		//
		//

		public static void closeQuietly(ResultSet rs) {
			try {
				rs.close();
			} catch (Exception e) {
				// ignore
			}
		}

		public static void closeQuietly(Statement stmt) {
			try {
				stmt.close();
			} catch (Exception e) {
				// ignore
			}
		}

		public static void commitSilently(Connection connection) {
			try {
				connection.commit();
			} catch (Exception e) {
			}
		}

		public static void closeQuietly(Connection connection) {
			try {
				connection.close();
			} catch (Exception e) {
				// ignore
			}
		}

		public static Object runQuery(Connection connection, String sqlStatement,
		    Object... parameters) {
			PreparedStatement ps = null;
			ResultSet rs = null;
			try {
				ps = connection.prepareStatement(sqlStatement);
				for (int i = 0; i < parameters.length; i++) {
					ps.setObject(i + 1, parameters[i]);
				}

				boolean hasResultSet = ps.execute();
				if (hasResultSet) {
					rs = ps.getResultSet();
					return rs;
				} else {
					return new Integer(ps.getUpdateCount());
				}
			} catch (Throwable t) {
				logger.warning(t.getMessage());
			} finally {
				Utils.closeQuietly(ps);
			}
			return new Integer(-1);
		}

		public static String sqlValueToString(Object value, int sqlType)
		    throws IllegalArgumentException, SecurityException,
		    IllegalAccessException, InvocationTargetException,
		    NoSuchMethodException {

			switch (sqlType) {
			case Types.CHAR:
			case Types.VARCHAR:
				return value == null ? "" : value.toString();// _NULL_ :
				// String.valueOf(value);
			default:
			}
			String strValue = value == null ? "" : value.toString();
			if (isBlank(strValue)) {
				return "";
			}
			if (value.getClass().getName().toString()
			    .startsWith("oracle.sql.TIMESTAMP")) {
				value = value.getClass().getMethod("toJdbc").invoke(value);
			}
			if (value instanceof Timestamp) {
				Timestamp t = (Timestamp) value;
				return toIsoDateTime(t.getTime());
			}
			if (value instanceof Date) {
				Date t = (Date) value;
				return toIsoDate(t.getTime());
			}
			return strValue;
		}

		public static String blobToString(Clob blob) {
			try {
				Writer out = new StringWriter();
				Reader in = blob.getCharacterStream();
				copy(in, out);
				out.flush();
				closeQuietly(out);
				return out.toString();
			} catch (Exception e) {
				logger.severe(getStackTrace(e));
			}
			return "";
		}

		public static Result buildResult(int start, int max, ResultSet rs) {
			Result result = null;
			try {

				//
				ResultSetMetaData md = rs.getMetaData();

				int colums = rs.getMetaData().getColumnCount();
				String[] header = new String[colums];
				int[] sqlTypes = new int[colums];
				header = new String[colums];
				for (int i = 0; i < colums; i++) {
					header[i] = md.getColumnName(i + 1);
					if (Result.USE_CAMEL_CASE_FOR_RESULT_HEADER) {
						header[i] = camelCase(header[i]);
					}
					sqlTypes[i] = md.getColumnType(i + 1);
				}
				//
				int counter = 0;
				result = new Result(header);
				result.from = -1;
				String[] row = null;
				while (rs.next()) {

					if (counter >= start && ((counter < start + max) || max == -1)) {
						if (result.from == -1) {
							result.from = counter;
						}

						row = new String[colums];

						for (int i = 0; i < colums; i++) {
							if (sqlTypes[i] == Types.BLOB) {
								logger
								    .warning("BLOB type not supported! Returning empty string!");
								row[i] = "";
							} else if (sqlTypes[i] == Types.CLOB) {
								Clob clob = (Clob) rs.getObject(i + 1);
								row[i] = blobToString(clob);
							} else {
								Object sqlValue = rs.getObject(i + 1);
								row[i] = sqlValueToString(sqlValue, sqlTypes[i]);
							}
						}
						result.addRow(row);
					} else {
						//
					}
					counter++;
				}
				result.totalCount = counter;

			} catch (Exception e) {
				result = new Result(e);
			} finally {

			}
			return result;
		}

		//
		// COLLECTION UTILS
		//

		public static <E> boolean isEmpty(Collection<E> c) {
			return c == null || c.size() == 0;
		}

		//
		// OBJECT UTILS
		//

		public static Map<String, String> asMap(Object obj) throws Exception {
			Map<String, String> values = new HashMap<String, String>();
			Method[] methods = obj.getClass().getMethods();

			for (Method method : methods) {
				if (Modifier.isStatic(method.getModifiers())) {
					continue;
				}
				if (method.getParameterTypes().length == 0
				    && method.getName().startsWith("get")
				    && method.getName().length() > 3
				    && method.getReturnType().equals(String.class)) {
					String key = method.getName().substring(3, 4).toLowerCase()
					    + method.getName().substring(4);
					Object r = method.invoke(obj, (Object[]) null);
					String value = rnn(r);
					values.put(key, value);
				}
			}
			return values;
		}

		@SuppressWarnings("unchecked")
		public static <E> Map<String, E> asMap(String keyProperty, List<E> list) {
			if (isEmpty(list)) {
				return new HashMap<String, E>();
			}
			Class<E> claxx = (Class<E>) list.get(0).getClass();
			Map<String, E> map = new HashMap<String, E>();
			try {
				String methodName = "get" + keyProperty.substring(0, 1).toUpperCase()
				    + keyProperty.substring(1);
				Method m = claxx.getMethod(methodName);
				for (E e : list) {
					map.put(m.invoke(e).toString(), e);
				}
			} catch (Exception e) {
				logger.severe(getStackTrace(e));
			}
			return map;
		}

	}

}
