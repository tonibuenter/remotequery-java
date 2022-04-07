package org.remotequery;

import com.google.gson.*;
import org.apache.commons.lang3.StringUtils;
import org.apache.commons.lang3.tuple.ImmutableTriple;
import org.apache.commons.lang3.tuple.Triple;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import javax.sql.DataSource;
import java.io.*;
import java.lang.reflect.Array;
import java.lang.reflect.*;
import java.math.BigDecimal;
import java.math.BigInteger;
import java.sql.*;
import java.text.ParseException;
import java.text.SimpleDateFormat;
import java.util.Date;
import java.util.*;
import java.util.Map.Entry;
import java.util.logging.Level;

/**
 * @author tonibuenter
 */
public class RemoteQuery {

  private static final Logger logger = LoggerFactory.getLogger(RemoteQuery.class);
  private static final Logger rqMainLogger = logger;
  private static final Logger rqDebugLogger = LoggerFactory.getLogger(RemoteQuery.class.getName() + ".rq");
  private static final Logger sqlLogger = LoggerFactory.getLogger(RemoteQuery.class.getName() + ".sql");

  /**
   * UTF-8 is the default encoding.
   */
  public static String ENCODING = "UTF-8";

  public static int MAX_RECURSION = 40;
  public static int MAX_INCLUDES = 100;
  public static int MAX_WHILE = 100000;

  public static final char DEFAULT_DEL = ',';
  public static final char DEFAULT_ESC = '\\';

  public static final String ANONYMOUS = "ANONYMOUS";

  public static char STATEMENT_DELIMITER = ';';
  public static char STATEMENT_ESCAPE = '\\';

  /**
   * "DEFAULT_DATASOURCE" is the default data source name.
   */
  public static String DEFAULT_DATASOURCE = "DEFAULT_DATASOURCE";

  public static String COLUMN_STATEMENTS = "STATEMENTS";
  public static String COLUMN_ROLES = "ROLES";
  public static String COLUMN_SERVICE_ID = "SERVICE_ID";
  public static String COLUNM_DATASOURCE = "DATASOURCE";

  public static class Commands {

    private Commands() {
    }

    public static final Set<String> StartBlock = new HashSet<>();

    static {
      StartBlock.add("if");
      StartBlock.add("if-empty");
      StartBlock.add("switch");
      StartBlock.add("while");
      StartBlock.add("foreach");
    }

    public static final Set<String> EndBlock = new HashSet<>();

    static {
      EndBlock.add("fi");
      EndBlock.add("done");
      EndBlock.add("end");
    }

    public static final Map<String, Object> Registry = new HashMap<>();

    static {
      Registry.put("serviceRoot", new ServiceRootCommand());
      Registry.put("sql", new SqlCommand());
      Registry.put("set", new SetCommand());
      Registry.put("put", new SetCommand());
      Registry.put("set-if-empty", new SetIfEmptyCommand());
      Registry.put("copy", new CopyCommand());
      Registry.put("copy-if-empty", new CopyIfEmptyCommand());
      Registry.put("parameters", new ParametersCommand());
      Registry.put("parameters-if-empty", new ParametersIfEmptyCommand());
      Registry.put("parameters-concat", new ParametersConcatCommand());
      Registry.put("parameters-concat-if-empty", new ParametersConcatIfEmptyCommand());
      Registry.put("serviceId", new ServiceIdCommand());
      Registry.put("java", new JavaCommand());
      Registry.put("class", new JavaCommand());
      Registry.put("if", new IfCommand());
      Registry.put("if-empty", new IfCommand(true));
      Registry.put("include", new FailCommand());
      Registry.put("switch", new SwitchCommand());
      Registry.put("while", new WhileCommand());
      Registry.put("foreach", new ForeachCommand());
      Registry.put("then", new NoOpCommand());
      Registry.put("else", new NoOpCommand());
      Registry.put("case", new NoOpCommand());
      Registry.put("default", new NoOpCommand());
      Registry.put("break", new NoOpCommand());
      Registry.put("fi", new NoOpCommand());
      Registry.put("do", new NoOpCommand());
      Registry.put("done", new NoOpCommand());
      Registry.put("end", new NoOpCommand());
      Registry.put("add-role", new AddRoleCommand());
      Registry.put("remove-role", new RemoveRoleCommand());
      Registry.put("comment", new CommentCommand());
      //
      Registry.put("python", new NoOpCommand());
      Registry.put("node", new NoOpCommand());
    }

    public static boolean isCmd(String cmd) {
      return StartBlock.contains(cmd) || EndBlock.contains(cmd) || Registry.containsKey(cmd);
    }

  }

  //
  // START OF SQL VALUE
  //

  public static final Map<Integer, ISqlValue> SqlValueMap = new HashMap<>();

  /**
   * This interface is used for creating customized conversions of JDBC ResultSet
   * values to Strings. A customized conversion can be registered with
   * <code>RemoteQuery.SqlValueMap.put(Sql-Type, ISqlValue-Instance)</code>
   *
   * @author tonibuenter
   */
  interface ISqlValue {
    String toString(Object o);
  }

  static {
    RemoteQuery.SqlValueMap.put(Types.CHAR, new SqlValueChar());
    RemoteQuery.SqlValueMap.put(Types.VARCHAR, new SqlValueChar());
    RemoteQuery.SqlValueMap.put(Types.CLOB, new SqlValueClob());
  }

  public static class SqlValueChar implements ISqlValue {
    public String toString(Object o) {
      return o.toString().trim();
    }
  }

  public static class SqlValueClob implements ISqlValue {
    public String toString(Object o) {
      Clob clob = (Clob) o;
      return Utils.blobToString(clob);
    }
  }

  //
  // END OF SQL VALUE
  //

  /**
   * Singleton for keeping the data source objects. Remark: not thread save.
   *
   * @author tonibuenter
   */
  public static class DataSources {

    private DataSources() {
    }

    private static final Map<String, DataSource> dss = new HashMap<>();

    public static DataSource get() {
      return dss.get(DEFAULT_DATASOURCE);
    }

    public static DataSource get(String datasource) {
      if (Utils.isBlank(datasource)) {
        datasource = DEFAULT_DATASOURCE;
      }
      return dss.get(datasource);
    }

    public static void register(String dataSourceName, DataSource ds) {
      dss.put(dataSourceName, ds);
    }

    public static void register(DataSource ds) {
      dss.put(DEFAULT_DATASOURCE, ds);
    }

  }

  /**
   * @author tonibuenter
   */
  public interface IQuery {
    Result run(Request request);
  }

  /**
   * @author tonibuenter
   */
  public interface ICommand {
    Result run(Request request, Result currentResult, StatementNode statementNode, ServiceEntry serviceEntry);
  }

  public static class RQException extends Exception {
    public RQException(String exception) {
      super(exception);
    }
  }

  /**
   * Interface for service repositories. Currently we have a JSON base and a SQL
   * base service repository.
   *
   * @author tonibuenter
   */
  public interface IServiceRepository {

    ServiceEntry get(String serviceId) throws RQException, SQLException;

  }

  public interface IResultListener {

    IResultListener start();

    IResultListener done();

    IResultListener setName(String name);

    IResultListener setFrom(int from);

    IResultListener setHeader(List<String> header);

    IResultListener setTypes(List<String> header);

    IResultListener addRow(String[] row);

    IResultListener setTable(List<List<String>> table);

    IResultListener setTotalCount(int totalCount);

    IResultListener setException(String e);

    IResultListener setRowsAffected(int rowsAffected);

  }

  public static final Map<String, String> PARAM_CONVERT = new HashMap<>();

  public static int buildCommandBlockTree(StatementNode root, List<String> statementList, int pointer) {

    while (pointer < statementList.size()) {
      StatementNode statementNode;

      Triple<String, String, String> t = Utils.parse_statement(statementList.get(pointer));
      pointer++;

      if (t == null) {
        continue;
      }

      String cmd = t.getLeft();
      String parameter = t.getMiddle();
      String statement = t.getRight();

      if (Utils.isBlank(cmd)) {
        continue;
      }

      statementNode = new StatementNode(cmd, parameter, statement);
      root.children.add(statementNode);

      if (Commands.EndBlock.contains(cmd)) {
        return pointer;
      }

      if (Commands.StartBlock.contains(cmd)) {
        pointer = buildCommandBlockTree(statementNode, statementList, pointer);
      }
    }
    return pointer;
  }

  public static List<String> resolveIncludes(String statements, Map<String, Integer> recursionCounter) {
    List<String> statementList = Utils.asList(Utils.tokenize(statements.trim(), STATEMENT_DELIMITER, STATEMENT_ESCAPE));
    List<String> resolvedList = new ArrayList<>(statementList.size());
    for (String stmt : statementList) {
      stmt = Utils.trim(stmt);
      if (stmt.startsWith("include")) {
        String serviceId = "";
        try {
          Triple<String, String, String> t = Utils.parse_statement(stmt);
          if (t == null || t.getMiddle() == null) {
            throw new RQException("Can not process include stmt:  " + stmt);
          }
          serviceId = t.getMiddle();
          ServiceEntry se = ServiceRepositoryHolder.get().get(serviceId);
          if (se == null) {
            throw new RQException("Did not find service for include command:  " + serviceId);
          }
          String includeStatements = se.statements;
          int counter = recursionCounter.get(serviceId) == null ? 0 : recursionCounter.get(serviceId);
          counter++;
          if (counter < MAX_INCLUDES) {
            recursionCounter.put(serviceId, counter);
            List<String> resolvedList2 = resolveIncludes(includeStatements, recursionCounter);
            resolvedList.addAll(resolvedList2);
          } else {
            throw new RQException("include command overflow " + serviceId);
          }
        } catch (Exception e) {
          ProcessLog.Current().error(e, rqMainLogger);
          resolvedList.add("systemMessage:include-of-error-serviceId=" + serviceId);
        }
      } else {
        resolvedList.add(stmt);
      }
    }
    return resolvedList;
  }

  public static StatementNode prepareCommandBlock(ServiceEntry serviceEntry) {
    List<String> statementList = resolveIncludes(serviceEntry.statements, new HashMap<>());
    StatementNode statementNode = new StatementNode("serviceRoot", serviceEntry.serviceId, "");
    buildCommandBlockTree(statementNode, statementList, 0);
    return statementNode;
  }

  /**
   * Convenience method for running a main query with a serviceId and a simple
   * map.
   *
   * @author tonibuenter
   */
  public static Result run(String serviceId, Map<String, String> parameters, String userId, Set<String> roles) {
    Request request = new Request();
    request.setServiceId(serviceId);
    request.setParameters(parameters);
    request.setUserId(userId);
    request.setRoles(roles);
    Runner runner = new Runner();
    return runner.run(request);
  }

  public static Result processCommandBlock(StatementNode statementNode, Request request, Result currentResult,
                                           ServiceEntry serviceEntry) {
    ProcessLog log = ProcessLog.Current();
    try {
      log.incrRecursion();
      if (log.getRecursionValue() > MAX_RECURSION) {
        log.error("Recursion limit reached " + MAX_RECURSION + ". Stop processing.", logger);
        return null;
      }

      //
      Object o = Commands.Registry.get(statementNode.cmd);

      if (o instanceof ICommand) {
        ICommand aq = (ICommand) o;
        return aq.run(request, currentResult, statementNode, serviceEntry);
      }
      if (o instanceof IQuery) {
        IQuery iq = (IQuery) o;
        return iq.run(request);
      }
      log.error("Unknown command " + statementNode.cmd + " (in statement: " + statementNode.statement + ")", logger);
    } catch (Exception e) {
      log.error("Statement " + statementNode.statement + " failed. Exception: " + e.getMessage(), logger);
    } finally {
      log.decrRecursion();
    }
    return null;
  }

  public static Result processCommand(String commandString, Request request, Result currentResult,
                                      ServiceEntry serviceEntry) {
    Triple<String, String, String> t = Utils.parse_statement(commandString);
    if (t == null || t.getLeft() == null) {
      logger.error("Can not process command: " + commandString);
      return currentResult;
    }
    String cmd = t.getLeft();

    String parameter = t.getMiddle();
    String statement = t.getRight();
    StatementNode statementNode = new StatementNode(cmd, parameter, statement);
    return processCommandBlock(statementNode, request, currentResult, serviceEntry);
  }

  public static Result processJava(String parameter, String parameterString, Request request, Result currentResult,
                                   StatementNode statementNode, ServiceEntry serviceEntry) {

    ProcessLog pLog = ProcessLog.Current();
    String[] parts = StringUtils.split(parameter, "::");
    String className = parts[0];
    String methodName = parts.length > 1 ? parts[1] : null;
    try {
      logger.debug("Process Java :: start with " + className);
      Object o = Class.forName(className).newInstance();

      if (o instanceof ICommand) {
        ICommand aq = (ICommand) o;
        return aq.run(request, currentResult, statementNode, serviceEntry);
      }
      if (o instanceof IQuery && methodName == null) {
        IQuery iq = (IQuery) o;
        return iq.run(request);
      }
      if (o instanceof IQuery) {
        Method m = o.getClass().getMethod(methodName, Request.class);
        return (Result) m.invoke(o, request);
      }
      pLog.error("Class: " + className + " is nether and IMtlCommand  nor a IQuery based class.", logger);

    } catch (Exception e) {
      pLog.error(e, logger);
      if (currentResult != null) {
        currentResult.setException(e.getMessage());
        return currentResult;
      } else {
        return new Result(new RQException(e.getMessage()));
      }
    }

    return null;
  }

  public static void processSql(String sql, Request request, ServiceEntry serviceEntry, IResultListener irl) {
    ProcessLog log = ProcessLog.Current();

    DataSource ds = DataSources.get(serviceEntry.datasource);
    if (ds == null) {
      RQException e = new RQException("Missing datasourceName: " + serviceEntry.datasource + " in service: "
              + serviceEntry.serviceId + ". Can not continue!");
      log.error(e.getMessage(), logger);
      irl.setException(e.getMessage());
      return;
    }
    processSql(ds, sql, request, irl, serviceEntry.cacheSeconds);
  }

  public static void processSql(DataSource ds, String sql, Request request, IResultListener irl, int cacheSeconds) {

    Connection con = null;
    try {
      con = ds.getConnection();
      processSql(con, sql, request, irl, cacheSeconds);
    } catch (Exception e) {
      logger.warn(e.getMessage(), e);
    } finally {
      Utils.closeQuietly(con);
    }
  }

  private static final DataCache dataCache = new DataCache();

  public static void processSql(Connection con, String sql, Request request, IResultListener irl, int cacheSeconds) {

    String serviceId = request.serviceId;
    serviceId = Utils.isBlank(serviceId) ? "-n/a-" : serviceId;
    ProcessLog pLog = ProcessLog.Current();

    PreparedStatement ps = null;
    ResultSet rs = null;

    pLog.system("sql before conversion: " + sql);
    sqlLogger.debug("start sql **************************************");
    sqlLogger.debug("Start sql (in service : " + serviceId + ")\n" + sql);

    QueryAndParams qap = new QueryAndParams(sql, request.getParameterSnapshhot());
    qap.convertQuery();
    sql = qap.qm_query;
    // pLog.system("sql after conversion: " + sql);

    //
    // PREPARE SERVICE_STMT
    //

    List<Object> paramObjects = new ArrayList<>();
    StringBuilder cacheKey = new StringBuilder(serviceId + ";");

    for (String param : qap.param_list) {
      String val = qap.req_params.get(param);
      String conv = PARAM_CONVERT.getOrDefault(param, "");
      if (val == null) {
        pLog.system("processSql: No value provided for parameter: " + param + " (serviceId: " + request.serviceId
                + "). Will use empty string.", logger);
        paramObjects.add("");
        sqlLogger.debug("sql-parameter:  " + param + " : ");
      } else {
        Object obj;
        String _type = PARAM_CONVERT.getOrDefault(param, "");
        if ("int".equalsIgnoreCase(_type)) {
          obj = Integer.parseInt(val);
        } else if ("long".equalsIgnoreCase(_type)) {
          obj = Long.parseLong(val);
        } else if ("float".equalsIgnoreCase(_type)) {
          obj = Float.parseFloat(val);
        } else if ("double".equalsIgnoreCase(_type)) {
          obj = Double.parseDouble(val);
        } else if ("boolean".equalsIgnoreCase(_type)) {
          obj = Boolean.valueOf(val);
        } else if ("BigInteger".equalsIgnoreCase(_type)) {
          obj = new BigInteger(val);
        } else if ("BigDecimal".equalsIgnoreCase(_type)) {
          obj = new BigDecimal(val);
        } else {
          obj = val;
        }
        paramObjects.add(obj);
        sqlLogger.debug("sql-parameter:  " + param + " : " + val);
        cacheKey.append(param).append(":").append(val);
      }
    }
    //
    // DEFAULT PARAMETER
    //

    //
    // FINALIZE SERVICE_STMT
    //

    // Check data cache

    Result r_cache = dataCache.get(cacheKey.toString(), cacheSeconds);
    if (r_cache != null) {
      Utils.copyResult(r_cache, irl);
    }

    try {
      ps = con.prepareStatement(sql);
      int index = 0;
      for (Object v : paramObjects) {
        ps.setObject(++index, v);
      }
      boolean hasResultSet = ps.execute();
      if (hasResultSet) {
        rs = ps.getResultSet();
        Utils.buildResult(0, -1, rs, irl);
        irl.setName(serviceId);
        irl.setFrom(0);
      } else {
        int rowsAffected = ps.getUpdateCount();
        pLog.system("ServiceEntry : " + serviceId + "; rowsAffected : " + rowsAffected);
        irl.setRowsAffected(rowsAffected);
        sqlLogger.debug("sql-rows-affected : " + rowsAffected);
        dataCache.put(cacheKey.toString(), irl, cacheSeconds);
      }
    } catch (SQLException e) {
      String warnMsg = "Warning for " + serviceId + " (parameters:" + qap.param_list + ") execption message: "
              + e.getMessage();
      sqlLogger.warn(warnMsg, logger);
    } catch (Exception e) {
      String errorMsg = "Error for " + serviceId + " (parameters:" + qap.param_list + ") execption message: "
              + e.getMessage();
      pLog.error(errorMsg, logger);
      pLog.error("parameterNameQuery=" + qap.named_query + ", questionMarkQuery=" + qap.qm_query + ", parameters="
                      + qap.param_list,
              logger);
    } finally {
      Utils.closeQuietly(rs);
      Utils.closeQuietly(ps);
    }
    sqlLogger.debug("end sql **************************************");

  }

  public static class QueryAndParams {

    public String named_query;
    public String qm_query;
    public Map<String, String> req_params;
    public List<String> param_list;
    private boolean in_param = false;
    private String current_param;

    public QueryAndParams(String named_query, Map<String, String> req_params) {
      this.named_query = named_query;
      this.req_params = req_params;
    }

    public void convertQuery() {

      this.qm_query = "";
      this.in_param = false;

      this.param_list = new ArrayList<>();
      this.current_param = "";

      boolean prot = false;

      for (char c : this.named_query.toCharArray()) {
        if (!prot) {
          if (this.in_param) {
            if (Character.isJavaIdentifierPart(c) || c == '[' || c == ']') {
              this.current_param += c;
              continue;
            } else {
              this._process_param();
            }
          }
          if (!this.in_param && c == ':') {
            this.in_param = true;
            continue;
          }
        }
        if (c == '\'') {
          prot = !prot;
        }
        this.qm_query += c;
      }
      // end processing
      if (this.in_param) {
        this._process_param();
      }
    }

    private void _process_param() {

      String param = this.current_param;
      if (param.endsWith("[]")) {
        String param_base = param.substring(0, param.length() - 2);
        String a_value = this.req_params.get(param_base);
        if (StringUtils.isBlank(a_value)) {
          this.param_list.add(param);
          this.qm_query += "?";
        } else {
          String[] request_values = a_value.split(",");
          int index = 0;
          for (String request_value : request_values) {
            String param_indexed = param_base + "[" + index + "]";
            this.req_params.put(param_indexed, request_value);
            this.param_list.add(param_indexed);
            if (index == 0) {
              this.qm_query += "?";

            } else {
              this.qm_query += ",?";
            }
            index += 1;
          }
        }
      } else {
        this.param_list.add(param);
        this.qm_query += "?";
      }
      this.current_param = "";
      this.in_param = false;
    }

  }

  /**
   * Runner is the main class the provides the processing of a RemoteQuery
   * request. It takes care of the service statement parsing and processing.
   *
   * @author tonibuenter
   */
  public static class Runner {

    // private final Map<String, Serializable> processStore = new
    // HashMap<String, Serializable>();
    //
    // public void put(String key, Serializable value) {
    // processStore.put(key, value);
    // }
    //
    // public Serializable get(String key) {
    // return processStore.get(key);
    // }

    public Result run(Request request) {
      Result result = null;
      ProcessLog log = ProcessLog.Current();

      String serviceId = request.getServiceId();
      String userId = request.getUserId();
      if (Utils.isEmpty(userId)) {
        log.system("Request object has no userId set. Process continues with userId=" + ANONYMOUS + " (serviceId: "
                + request.getServiceId() + ")", logger);
        request.setUserId(ANONYMOUS);
        userId = request.getUserId();
      }

      log.incrRecursion();
      try {
        //

        ServiceEntry serviceEntry = ServiceRepositoryHolder.get().get(serviceId);
        log.serviceEntry_Start(serviceEntry);
        if (serviceEntry == null) {
          String exception = String.format("No ServiceEntry found for %s.", serviceId);
          log.error(exception, logger);
          return new Result(new RQException(exception));
        }
        //
        // CHECK ACCESS
        //
        boolean hasAccess = false;
        Set<String> roles = serviceEntry.roles;
        if (Utils.isEmpty(roles)) {
          hasAccess = true;
        } else {
          for (String role : roles) {
            if (request.getRoles().contains(role)) {
              hasAccess = true;
              break;
            }
          }
        }
        if (hasAccess) {
          log.system("Access to " + serviceId + " for " + userId + " : ok", logger);
        } else {
          log.warn("No access to " + serviceId + " for " + userId + " (service roles: " + roles + ", request roles: "
                  + request.getRoles() + ")", logger);
          log.statusCode = "403";
          Result r = new Result(log);
          r.setException(String.format("Access denied to '%s'!", serviceId));
          return r;
        }
        //
        // START PROCESSING STATEMENTS
        //

        log.system("ServiceEntry found for userId=" + userId + " is : " + serviceEntry, logger);

        StatementNode statementNode = prepareCommandBlock(serviceEntry);

        result = processCommandBlock(statementNode, request, null, serviceEntry);

      } catch (Exception e) {
        log.error(e, logger);
      } finally {
        // ParameterSupport.release(parameterSupport);
        log.decrRecursion();
        log.serviceEntry_End();
      }
      if (result != null) {
        result.userId = userId;
      }
      return result;
    }

  }

  //
  //
  // BuildIns ...
  //
  //

  /**
   * BuildIns is a collection of Java IQueries which solve rather universal tasks
   * such as : register a service,
   *
   * @author tonibuenter
   */
  public static class BuiltIns {

    public static class MultiService implements IQuery {

      @SuppressWarnings("rawtypes")
      @Override
      public Result run(Request request) {
        Request requestC;
        Result resultC;
        ProcessLog pLog;
        ProcessLog mainLog = ProcessLog.Current();
        Result mainResult = new Result("MultiResult");
        mainResult.processLog = mainLog;
        String requestArray = request.get("requestArray");
        List requestList = JsonUtils.toObject(requestArray, List.class);
        Runner mainQuery = new Runner();
        for (int i = 0; i < (requestList == null ? 0 : requestList.size()); i++) {
          pLog = ProcessLog.Current(new ProcessLog(request.getUserId()));
          requestC = request.deepCopy();
          requestC.remove("requestArray");
          Object r = requestList.get(i);
          if (r instanceof Map) {
            Map rm = (Map) r;
            String serviceId = rm.get("serviceId") + "";
            if (Utils.isBlank(serviceId)) {
              pLog.error("no serviceId in request", logger);
            } else {
              requestC.setServiceId(serviceId);
              Object p = rm.get("parameters");
              if (p instanceof Map) {
                Map pm = (Map) p;
                for (Object o : pm.entrySet()) {
                  Entry e = (Entry) o;
                  requestC.put(e.getKey() + "", e.getValue() + "");
                }
              }
              resultC = mainQuery.run(requestC);
              if (resultC == null) {
                pLog.error("Unexpected 'null' for service " + serviceId + " (index:" + i + ")", logger);
                Result re = new Result(pLog);
                mainResult.addRowVar(JsonUtils.toJson(re));
              } else {
                resultC.processLog = pLog;
                String rs = JsonUtils.toJson(resultC);
                mainResult.addRowVar(rs);
              }
            }

          } else {
            mainLog.error("Request " + i + " is not an object : " + r);
          }
        }
        return mainResult;
      }
    }
  }

  /**
   * ProcessLog class is a collector of log information specific to a single
   * request resolution.
   *
   * @author tonibuenter
   */
  public static class ProcessLog implements Serializable {

    public static final int USER_OK_CODE = 10;
    public static final int USER_WARNING_CODE = 20;
    public static final int USER_ERROR_CODE = 30;
    public static final int OK_CODE = 1000;
    public static final int WARNING_CODE = 2000;
    public static final int ERROR_CODE = 3000;
    public static final int SYSTEM_CODE = 4000;

    public static final String WARNING = "Warning";
    public static final String ERROR = "Error";
    public static final String OK = "OK";
    public static final String SYSTEM = "System";
    private static final long serialVersionUID = 1L;
    private static final String FILE_ID = "_FILE_ID_";

    public static final Map<String, Integer> USER_CODE_MAP = new HashMap<>();

    static {
      USER_CODE_MAP.put(OK, USER_OK_CODE);
      USER_CODE_MAP.put(WARNING, USER_WARNING_CODE);
      USER_CODE_MAP.put(ERROR, USER_ERROR_CODE);
    }

    public static final ThreadLocal<ProcessLog> TL = ThreadLocal.withInitial(ProcessLog::new);

    public static class LogLine implements Serializable, Comparable<LogLine> {

      private static final long serialVersionUID = 1L;

      public final String message;
      public int code;
      private final long time;
      private final String state;

      public LogLine(int code, String message, String state, long time, String... data) {
        super();
        this.message = message;
        this.code = code;
        this.state = state;
        this.time = time;
      }

      public String getMsg() {
        return message;
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

      public int compareTo(LogLine o) {
        int i = this.prio(state) - prio(o.state);
        if (i == 0) {
          i = Long.compare(o.time, this.time);
        }
        return i;
      }

      private int prio(String state) {
        if (ERROR.equals(state)) {
          return 1;
        }
        if (WARNING.equals(state)) {
          return 2;
        }
        if (OK.equals(state)) {
          return 3;
        }
        if (SYSTEM.equals(state)) {
          return 4;
        }
        return 5;
      }

      @Override
      public String toString() {
        return Utils.toIsoDateTime(time) + ":" + message + "(" + state + ":" + code + ")";
      }

      @Override
      public int hashCode() {
        final int prime = 31;
        int result = 1;
        result = prime * result + ((message == null) ? 0 : message.hashCode());
        result = prime * result + ((state == null) ? 0 : state.hashCode());
        result = prime * result + (int) (time ^ (time >>> 32));
        return result;
      }

      @Override
      public boolean equals(Object obj) {
        if (this == obj) {
          return true;
        }
        if (obj == null) {
          return false;
        }
        if (getClass() != obj.getClass()) {
          return false;
        }
        LogLine other = (LogLine) obj;
        if (message == null) {
          if (other.message != null) {
            return false;
          }
        } else if (!message.equals(other.message)) {
          return false;
        }
        if (state == null) {
          if (other.state != null) {
            return false;
          }
        } else if (!state.equals(other.state)) {
          return false;
        }
        return time == other.time;
      }

      public int getMsgCode() {
        return code;
      }

      public void setMsgCode(int code) {
        this.code = code;
      }

    }

    /**
     *
     */

    //
    // ProcessLog Object Level
    //

    private transient boolean silently = false;

    private String state = OK;
    public String statusCode = OK;
    public List<LogLine> lines = new ArrayList<>();
    private final Map<String, String> attributes = new HashMap<>();

    private Long lastUsedTime;
    private String userId;
    private String name;
    private int recursion = 0;

    public ProcessLog() {
    }

    public ProcessLog(String userId) {
      this.userId = userId;
    }

    public void reduceLinesByCode(int maxCode) {
      List<LogLine> lines = new ArrayList<>();
      for (LogLine p : this.lines) {
        if (p.code <= maxCode) {
          lines.add(p);
        }
      }
      this.lines = lines;
    }

    public void incrRecursion() {
      recursion++;
    }

    public void decrRecursion() {
      recursion--;
    }

    public int getRecursionValue() {
      return recursion;
    }

    public void infoUser(String line) {
      add(USER_OK_CODE, line, OK);
    }

    public void user(String level, String line) {
      Integer code = USER_CODE_MAP.get(level);
      code = code == null ? USER_OK_CODE : code;
      add(code, line, level);
    }

    public void warnUser(String line, Logger logger) {
      add(USER_WARNING_CODE, line, WARNING);
      _writeLog(line, Level.WARNING, logger);
    }

    public void errorUser(String line, Logger... logger) {
      add(USER_ERROR_CODE, line, ERROR);
      _writeLog(line, Level.SEVERE, logger);
    }

    public void error(String line, Logger... logger) {
      this.state = ERROR;
      add(ERROR_CODE, line, ERROR);
      _writeLog(line, Level.SEVERE, logger);
    }

    public void error(Throwable t, Logger... logger) {
      error(t.getMessage(), t, logger);
    }

    public void error(String line, Throwable t, Logger... logger) {
      this.state = ERROR;
      add(ERROR_CODE, line, ERROR);
      _writeLog(line + "\n" + Utils.getStackTrace(t), Level.SEVERE, logger);
    }

    public void error(int code, String line, Logger... logger) {
      this.state = ERROR;
      add(code, line, ERROR);
      _writeLog(line, Level.SEVERE, logger);
    }

    public void warn(String line, Logger... logger) {
      add(WARNING_CODE, line, WARNING);
      _writeLog(line, Level.WARNING, logger);
    }

    public void system(String line, Logger... logger) {
      add(SYSTEM_CODE, line, SYSTEM);
      _writeLog(line, Level.INFO, logger);
    }

    public void system(Throwable t, Logger... logger) {
      String line = Utils.getStackTrace(t);
      system(line, logger);
    }

    public void _writeLog(String line, Level level, Logger... loggers) {
      for (Logger logger : loggers) {
        if (level == Level.WARNING) {
          logger.warn(line);
        } else if (level == Level.SEVERE) {
          logger.error(line);
        } else {
          logger.info(line);
        }
      }
    }

    private void add(int code, String line, String level) {
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
      lines.add(new LogLine(code, line, level, time));
    }

    public List<LogLine> getLines() {
      return lines;
    }

    public boolean isOk() {
      return OK.equals(this.state);
    }

    public boolean isError() {
      return ERROR.equals(this.state);
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

    public static ProcessLog Current(ProcessLog pLog) {
      TL.set(pLog);
      return TL.get();
    }

    public static void RemoveCurrent() {
      TL.remove();
    }

    //
    // DEBUG CONTEXT -start-
    //

    public DebugContext debugContext;
    public transient DebugContext current;

    public static class DebugContext {
      public static int SE = 0;

      public int type;
      public String name;
      public List<String[]> contents;
      public Map<String, String> parameters;
      public List<DebugContext> children = new ArrayList<>();
      public transient DebugContext parent;

      public DebugContext(int type) {
        this.type = type;
      }

    }

    public void serviceEntry_Start(ServiceEntry se) {
      if (current == null) {
        return;
      }
      DebugContext dc = new DebugContext(DebugContext.SE);
      dc.parent = this.current;
      dc.name = se.serviceId;
      this.current = dc;
      rqDebugLogger.debug("==>>> Start SE : " + dc.name);
    }

    public void serviceEntry_End() {
      if (current == null) {
        return;
      }
      rqDebugLogger.debug("<<<== End SE : " + current.name);
      this.current = this.current.parent;
    }

    //
    // DEBUG CONTEXT -end-
    //

  }

  /**
   * @author tonibuenter
   */
  public static class Request implements Serializable {

    /**
     *
     */
    private static final long serialVersionUID = 1L;

    public String serviceId;
    public Map<String, String> parameters = new HashMap<>();
    public String userId;
    public Set<String> roles = new HashSet<>();

    public transient Map<String, Object> transientAttributes = new HashMap<>();

    @SuppressWarnings("unchecked")
    public final Map<String, Serializable>[] fileList = new Map[4];

    {
      for (int i = 0; i < 4; i++) {
        fileList[i] = new HashMap<>();
      }
    }

    public Request() {
    }

    public Request deepCopy() {
      Request copy = (Request) Utils.deepClone(this);
      copy.transientAttributes = transientAttributes;
      return copy;
    }

    public String getServiceId() {
      return serviceId;
    }

    public Request setServiceId(String serviceId) {
      this.serviceId = serviceId;
      return this;
    }

    public Result run() {
      Runner runner = new Runner();
      return runner.run(this);
    }

    public Result run(String serviceId) {
      this.setServiceId(serviceId);
      return run();
    }

    public Result run(String serviceId, Map<String, String> map) {
      this.setServiceId(serviceId);
      this.put(map);
      return run();
    }

    public Result runWith(String serviceId, Object object) {
      this.setServiceId(serviceId);
      this.putObject(object);
      return run();
    }

    public Request setParameters(Map<String, String> parametersIn) {
      this.parameters = parametersIn;
      this.parameters = this.parameters == null ? new HashMap<>() : this.parameters;
      return this;
    }

    public Map<String, String> getParameters() {
      return this.parameters;
    }

    public String get(String name) {
      return this.parameters.get(name);
    }

    public String get(String name, String defaultValue) {
      String v = get(name);
      return Utils.isBlank(v) ? defaultValue : v;
    }

    // TODO junit
    public Map<String, String> getParameterSnapshhot() {
      return new HashMap<>(this.parameters);
    }

    public Request put(String key, Object value) {
      String v = value == null ? null : value.toString();
      this.parameters.put(key, v);
      return this;
    }

    public Request remove(String key) {
      this.parameters.remove(key);
      return this;
    }

    // public Request put(String key, String value) {
    // String v = value == null ? null : value.toString();
    // this.parameters.put(key, v);
    // return this;
    // }

    public Request put(Map<String, String> map) {
      this.parameters.putAll(map);
      return this;
    }

    public Request putObject(Object object) {
      if (object == null) {
        return this;
      }
      Map<String, String> params = Utils.asMap(object);
      return put(params);
    }

    public Request addRole(String role) {
      this.roles.add(role);
      return this;
    }

    public Request addRoles(Collection<String> roles) {
      if (roles != null) {
        for (String role : roles) {
          if (!Utils.isBlank(role)) {
            addRole(role);
          }
        }
      }
      return this;
    }

    public Request removeRole(String role) {
      this.roles.remove(role);
      return this;
    }

    public String getUserId() {
      return userId;
    }

    public Request setUserId(String userId) {
      this.userId = userId;
      return this;
    }

    public Set<String> getRoles() {
      return roles;
    }

    public Request setRoles(Collection<String> roles) {
      this.roles = new HashSet<>();
      return addRoles(roles);
    }

    public Request setTransientAttribute(String name, Object value) {
      this.transientAttributes.put(name, value);
      return this;

    }

    public Object getTransientAttribute(String name) {
      return this.transientAttributes.get(name);
    }

    public <E> E asObject(Class<E> claxx) {
      try {
        return Utils.newObject(getParameterSnapshhot(), claxx);
      } catch (Exception e) {
        logger.error(e.getMessage(), e);
      }
      return null;
    }

  }

  public static class ResultUtils {
    @SuppressWarnings("rawtypes")
    public static Result asResult(Object obj) {
      Map<String, String> pm;
      Result result = null;
      String[] header;
      String[] row;
      Collection coll;
      int i;
      if (obj instanceof Collection) {
        coll = (Collection) obj;
      } else {
        coll = Utils.asList(obj);
      }

      boolean first = true;
      for (Object o : coll) {
        pm = Utils.asPropertyMap(o);
        if (first) {
          header = new String[pm.size()];
          i = 0;
          for (String key : pm.keySet()) {
            header[i] = key;
            i++;
          }
          result = new Result(header);
          first = false;
        }
        row = new String[pm.size()];
        i = 0;
        for (String key : pm.keySet()) {
          row[i] = pm.get(key);
          i++;
        }
        result.addRow(row);
      }
      return result;
    }
  }

  public static class Result implements IResultListener {

    private static final Logger logger = LoggerFactory.getLogger(Result.class);
    public static boolean USE_CAMEL_CASE_FOR_RESULT_HEADER = true;

    //
    // Object Level
    //
    public String name = "";
    public String userId = "-";
    public int from = 0;
    public int totalCount = 0;
    public int rowsAffected = 0;
    public boolean hasMore = false;

    public List<List<String>> table = new ArrayList<>();
    public List<String> header = new ArrayList<>();
    public List<String> types = new ArrayList<>();
    public String exception = null;
    public ProcessLog processLog = null;

    public Result(String... header) {
      _header(header);
    }

    private void _header(String[] header) {
      this.header.addAll(Arrays.asList(header));
    }

    public void append(Result result) {
      // TODO Auto-generated method stub

    }

    public Result(int from, int totalCount, List<String> header, List<List<String>> table) {
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

    public int size() {
      return table == null ? 0 : table.size();
    }

    public Result update() {
      totalCount = Math.max(from + size(), totalCount);
      return this;
    }

    @Override
    public Result addRow(String[] row) {
      List<String> l = new ArrayList<>(row.length);
      l.addAll(Arrays.asList(row));
      table.add(l);
      update();
      return this;
    }

    public Result addRow(List<String> row) {
      table.add(row);
      update();
      return this;
    }

    public Result addRowVar(String... row) {
      addRow(row);
      return this;
    }

    public Result addObject(Object o) {
      Map<String, String> map = Utils.asPropertyMap(o);
      List<String> row = new ArrayList<>(header.size());
      for (String s : header) {
        String e = map.get(s);
        row.add(e == null ? "" : e);
      }
      addRow(row);
      return this;
    }


    /**
     * If the table list is empty it add an empty list. Then all entries in the map
     * are added as new header (column names) and values of the first row;
     *
     * @param map name/value for columns
     * @return Result
     */
    public Result addColumns(Map<String, String> map) {
      if (table.size() == 0) {
        table.add(new ArrayList<>());
      }
      for (Entry<String, String> e : map.entrySet()) {
        addColumn(e.getKey(), e.getValue());
      }
      return this;
    }

    public Result addColumn(String head, String value) {
      if (table.size() == 0) {
        table.add(new ArrayList<>());
      }
      List<String> row = table.get(0);
      header.add(head);
      row.add(value);
      return this;
    }

    public Map<String, String> getRowAsMap(int index) {
      Map<String, String> map = new HashMap<>();
      if (table != null && table.size() > index) {
        List<String> row = table.get(index);
        for (int i = 0; i < header.size(); i++) {
          map.put(header.get(i), row.get(i));
        }
      }
      return map;
    }

    public Map<String, String> getFirstRowAsMap() {
      return getRowAsMap(0);
    }

    public String getSingleValue() {
      if (!Utils.isEmpty(table) && !Utils.isEmpty(table.get(0))) {
        return table.get(0).get(0);
      }
      return null;
    }

    public String getValue(int rowIndex, String head) {
      int index = getColIndex(head);
      return (index > -1 && rowIndex < table.size()) ? table.get(rowIndex).get(index) : null;
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
      Map<String, Map<String, String>> map = new HashMap<>();
      int index = getColIndex(keyHead);

      for (List<String> row : table) {
        Map<String, String> subMap = new HashMap<>();
        String key = row.get(index);
        map.put(key, subMap);
        for (int j = 0; j < row.size(); j++) {
          subMap.put(this.header.get(j), row.get(j));
        }
      }
      return map;
    }

    public Map<String, List<String>> toMap(int... indexes) {
      Map<String, List<String>> map = new HashMap<>();
      for (List<String> value : table) {
        String key = createMapKey(value, indexes);
        map.put(key, value);
      }
      return map;
    }

    public Map<String, String> toTwoColumnMap(String keyHeader, String valueHeader) {
      Map<String, String> map = new HashMap<>();
      int keyIndex = getColIndex(keyHeader);
      int valueIndex = getColIndex(valueHeader);
      for (List<String> row : table) {
        String key = row.get(keyIndex);
        String value = row.get(valueIndex);
        map.put(key, value);
      }
      return map;
    }

    public Map<String, List<List<String>>> toMultiMap(int... indexes) {
      Map<String, List<List<String>>> map = new HashMap<>();
      for (List<String> value : table) {
        String key = createMapKey(value, indexes);
        List<List<String>> rows = map.computeIfAbsent(key, k -> new ArrayList<>());
        rows.add(value);
      }
      return map;
    }

    public static String createMapKey(List<String> row, int... indexes) {
      StringBuilder key = new StringBuilder();
      for (int i = 0; i < indexes.length; i++) {
        if (i == 0) {
          key = new StringBuilder(row.get(indexes[i]));
        } else {
          key.append("-").append(row.get(indexes[i]));
        }
      }
      return key.toString();
    }

    public Result(Exception e) {
      this.exception = e.getMessage();
    }

    @Override
    public String toString() {
      StringBuilder tableString = new StringBuilder("[");
      for (int i = 0; i < table.size(); i++) {
        if (i != 0) {
          tableString.append("\n");
        }
        List<String> row = table.get(i);
        tableString.append(row);
      }
      tableString.append("]");
      return "Result [name=" + name + ", userId=" + userId + ", size=" + size() + "\nfrom=" + from + ", totalCount="
              + totalCount + "\n" + header + "\n" + tableString + ",\nexception=" + exception + "]";
    }

    public String getSingleValue(String head) {
      int index = getColIndex(head);
      return (index > -1 && table.size() > 0) ? table.get(0).get(index) : null;
    }

    /**
     * Creates a list of objects. The data is filled by applying corresponding set
     * methods. E.g. for Columnt FIRST_NAME it will try to call
     * setFIRST_NAME(String) or setFirstName(String)
     */
    public <E> List<E> asList(Class<E> claxx) {
      List<E> list = new ArrayList<>();
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
          Field f = null;
          for (String setGetName : res) {
            try {
              m = claxx.getMethod("set" + setGetName, String.class);
            } catch (Exception e) {
              // logger.debug("Did not find method: " + setGetName
              // + ": ->" + e);
            }
            if (m != null) {
              break;
            }
            // try field
            try {
              String fieldName = setGetName.substring(0, 1).toLowerCase() + setGetName.substring(1);
              f = claxx.getField(fieldName);
            } catch (Exception e) {
              // logger.debug("Did not find field: " + setGetName
              // + ": ->" + e);
            }
            if (f != null) {
              break;
            }
          }
          if (m != null) {
            int rowIndex = 0;
            for (E e : list) {
              m.invoke(e, table.get(rowIndex).get(colIndex));
              rowIndex++;
            }
          } else if (f != null) {
            int rowIndex = 0;
            for (E e : list) {
              f.set(e, table.get(rowIndex).get(colIndex));
              rowIndex++;
            }
          } else {
            logger.info("Did not find field for head: " + head);
          }
          colIndex++;
        }
      } catch (Exception e) {
        logger.error(e.getMessage(), e);
      }
      return list;
    }

    /**
     * Return the first row of the result as instance of the class specified. If
     * result is empty return null;
     *
     * @return first row of the result as instance of the class specified
     */
    public <E> E asObject(Class<E> claxx) {
      List<E> list = asList(claxx);
      if (list.size() > 0) {
        return list.get(0);
      }
      return null;
    }

    public <E> E as(Class<E> claxx) {
      return asObject(claxx);
    }

    public List<String> getColumn(String columnName) {
      int index = getColIndex(columnName);
      if (index == -1) {
        return null;
      }
      List<String> res = new ArrayList<>(this.size());
      for (List<String> row : this.table) {
        res.add(row.get(index));
      }
      return res;
    }

    public <E> Map<String, E> asMap(String keyProperty, Class<E> claxx) {
      Map<String, E> map = new HashMap<>();
      try {
        List<E> list = this.asList(claxx);
        map = Utils.asMap(keyProperty, list);
      } catch (Exception e) {
        logger.error(e.getMessage(), e);
      }
      return map;
    }

    public Set<String> asSet(int index) {
      Set<String> set = new HashSet<>();
      if (index == -1 || index >= header.size()) {
        return set;
      }
      if (this.table != null) {
        for (List<String> row : this.table) {
          if (row != null && row.size() > 0) {
            set.add(row.get(index));
          }
        }
      }
      return set;
    }

    public Set<String> asSet() {
      return asSet(0);
    }

    public Set<String> asSet(String head) {
      int index = this.getColIndex(head);
      return asSet(index);
    }

    public <E> Map<String, List<E>> asMapList(String property, Class<E> claxx) {
      Map<String, List<E>> map = new HashMap<>();
      PropertyGetSet<E> propertyReader = new PropertyGetSet<>(claxx, property);

      try {
        List<E> list = this.asList(claxx);
        // String methodName = "get" + property.substring(0,
        // 1).toUpperCase()
        // + property.substring(1);
        // Method m = claxx.getMethod(methodName);
        for (E e : list) {
          // String key = m.invoke(e).toString();
          String key = propertyReader.get(e);
          List<E> listElement = map.computeIfAbsent(key, k -> new ArrayList<>());
          listElement.add(e);
        }
      } catch (Exception e) {
        logger.error(e.getMessage(), e);
      }
      return map;
    }

    public List<Map<String, String>> asList() {
      List<Map<String, String>> list = new ArrayList<>();
      for (List<String> row : this.table) {
        Map<String, String> map = new HashMap<>(header.size());
        for (int i = 0; i < header.size(); i++) {
          map.put(header.get(i), row.get(i));
        }
        list.add(map);
      }
      return list;
    }

    public Map<String, String> asMap(String headKey, String headValue) {
      int keyIndex = getHeaderIndex(headKey);
      int valueIndex = getHeaderIndex(headValue);
      return asMap(keyIndex, valueIndex);
    }

    public Map<String, String> asMap(int keyIndex, int valueIndex) {
      Map<String, String> map = new HashMap<>();
      if (table != null) {
        for (List<String> row : table) {
          map.put(row.get(keyIndex), row.get(valueIndex));
        }
      }
      return map;
    }

    public static Result createSingleValue(String header, String value) {
      Result r = new Result(header);
      r.addRowVar(value);
      return r;
    }

    public void join(String joinKey, String joinCol, Result joinResult) {
      int joinKeyIndex = joinResult.getHeaderIndex(joinCol);

      Map<String, Integer> keyIndexMap = getKeyIndexMap(joinCol);
      Map<String, List<List<String>>> joinSubtables = new HashMap<>();
      for (List<String> joinRow : joinResult.table) {
        String key = joinRow.get(joinKeyIndex);
        if (keyIndexMap.containsKey(key)) {
          List<List<String>> list = joinSubtables.computeIfAbsent(key, k -> new ArrayList<>());
          list.add(joinRow);
        }
      }

      this.header.add(joinCol);

      for (Entry<String, List<List<String>>> e : joinSubtables.entrySet()) {
        String key = e.getKey();
        List<List<String>> r = e.getValue();
        Result tmpResult = new Result(0, r.size(), joinResult.header, r);
        String s = JsonUtils.toJson(tmpResult);
        List<String> row = this.table.get(keyIndexMap.get(key));
        row.add(s);
      }

    }

    public Map<String, Integer> getKeyIndexMap(String colName) {
      Map<String, Integer> map = new HashMap<>();
      Set<Object> processCheckSet = new HashSet<>();
      int keyIndex = getHeaderIndex(colName);
      for (int i = 0; i < this.table.size(); i++) {
        List<String> row = this.table.get(i);
        String key = row.get(keyIndex);
        if (processCheckSet.contains(key)) {
          continue;
        }
        processCheckSet.add(key);
        map.put(key, i);
      }
      return map;
    }

    public int getHeaderIndex(String colName) {
      for (int i = 0; i < header.size(); i++) {
        if (header.get(i).equals(colName)) {
          return i;
        }
      }
      return -1;
    }

    public static boolean isEmpty(Result r) {
      return r == null || r.table == null || r.table.size() == 0;
    }

    public static Result union(Result p1, Result p2) {
      if (p1 == null) {
        return p2;
      }
      if (p2 == null) {
        return p1;
      }
      if (p2.table != null) {
        p1.table.addAll(p2.table);
      }
      return p1;
    }

    @Override
    public IResultListener start() {
      return this;
    }

    @Override
    public IResultListener done() {
      return this;
    }

    @Override
    public IResultListener setName(String name) {
      this.name = name;
      return this;
    }

    @Override
    public IResultListener setFrom(int from) {
      this.from = from;
      return this;
    }

    @Override
    public IResultListener setHeader(List<String> header) {
      this.header.clear();
      this.header.addAll(header);
      // _header(header);
      return this;
    }

    @Override
    public IResultListener setTypes(List<String> types) {
      this.types.clear();
      this.types.addAll(types);
      // _header(header);
      return this;
    }

    @Override
    public IResultListener setTotalCount(int totalCount) {
      this.totalCount = totalCount;
      return this;
    }

    @Override
    public IResultListener setException(String exception) {
      this.exception = exception;
      return this;
    }

    @Override
    public IResultListener setRowsAffected(int rowsAffected) {
      this.rowsAffected = rowsAffected;
      return this;
    }

    @Override
    public IResultListener setTable(List<List<String>> table) {
      this.table = table;
      return this;
    }

  }

  /**
   * ServiceEntry class holds all information a service such as serviceId,
   * statements, data source name and access.
   *
   * @author tonibuenter
   */
  public static class ServiceEntry {

    public String serviceId;
    public String statements;
    public Set<String> roles;
    public String datasource = DEFAULT_DATASOURCE;
    public int cacheSeconds;

    public ServiceEntry(String serviceId, String statements, String roles) {
      super();
      this.serviceId = serviceId;
      this.statements = statements;
      if (!Utils.isEmpty(roles)) {
        String[] r = roles.split(",");
        for (int i = 0; i < r.length; i++) {
          r[i] = r[i].trim();
        }
        this.roles = new HashSet<>(Arrays.asList(r));
      }
    }

    public ServiceEntry() {
    }

    public ServiceEntry(String serviceId, String statements, Set<String> roles) {
      super();
      this.serviceId = serviceId;
      this.statements = statements;
      this.roles = roles;
    }

    public String getRoles() {
      return Utils.joinTokens(roles);
    }

    @Override
    public String toString() {
      return "ServiceEntry [serviceId=" + serviceId + ", statements=\n" + statements + "\n, roles=" + roles
              + ", datasource=" + datasource + "]";
    }

    @Override
    public int hashCode() {
      return serviceId.hashCode();
    }

    @Override
    public boolean equals(Object obj) {
      if (obj instanceof ServiceEntry) {
        ServiceEntry _se = (ServiceEntry) obj;
        return this.serviceId.equals(_se.serviceId);
      }
      return false;
    }

  }

  public static class ServiceRepositoryHolder {

    private static IServiceRepository value;

    public static void set(IServiceRepository serviceRepository) {
      if (value != null) {
        throw new RuntimeException(ServiceRepositoryHolder.class.getName() + " already initialized!");
      }
      value = serviceRepository;
    }

    public static IServiceRepository get() {
      return value;
    }

  }

  public static class ServiceRepositorySql implements IServiceRepository {

    private static final Logger logger = LoggerFactory.getLogger(ServiceRepositorySql.class.getName());

    private final DataSource ds;
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
      String sql = selectQuery != null ? selectQuery
              : "select * from " + tableName + " where " + COLUMN_SERVICE_ID + " = ?";
      List<ServiceEntry> r = _get(sql, serviceId);
      if (Utils.isEmpty(r)) {
        logger.warn("No service entry found for: " + serviceId);
        return null;
      }
      if (r.size() > 1) {
        logger.warn("More than one service entry found for: " + serviceId);
      }
      return r.get(0);
    }

    private List<ServiceEntry> _get(String sql, String... parameters) throws SQLException {
      Connection con = null;
      PreparedStatement ps = null;
      ResultSet rs = null;
      ServiceEntry se;
      List<ServiceEntry> result = new ArrayList<>();
      try {
        con = getConnection();

        ps = con.prepareStatement(sql);
        for (int i = 0; i < parameters.length; i++) {
          ps.setString(i + 1, parameters[i]);
        }

        rs = ps.executeQuery();
        while (rs.next()) {
          String serviceId = rs.getString(COLUMN_SERVICE_ID);
          String statements = rs.getString(COLUMN_STATEMENTS);
          String roles = rs.getString(COLUMN_ROLES);
          String datasource = "";
          try {
            datasource = rs.getString(COLUNM_DATASOURCE);
          } catch (Exception e) {
            sqlLogger.info(COLUNM_DATASOURCE + " missing in sql: " + sql + "! Will use empty string.");
          }
          se = new RemoteQuery.ServiceEntry(serviceId, statements, roles);
          se.datasource = datasource;
          logger.info("Found " + se);
          result.add(se);
        }
      } finally {
        Utils.closeQuietly(rs);
        Utils.closeQuietly(ps);
        returnConnection(con);
      }
      return result;
    }

    private Connection getConnection() throws SQLException {
      if (ds == null) {
        throw new RuntimeException("DataSource is null. Please provide a DataSource!");
      }
      return ds.getConnection();
    }

    private void returnConnection(Connection con) throws SQLException {
      if (ds != null) {
        con.close();
      }
    }

  }

  static class StringTokenizer2 {

    private int index = 0;
    private final String[] tokens;
    private final boolean ignoreWhiteSpace;

    public StringTokenizer2(String string, char del, char esc) {
      this(string, del, esc, true);
    }

    public StringTokenizer2(String string, char del, char esc, boolean ignoreWhiteSpace) {
      this.ignoreWhiteSpace = ignoreWhiteSpace;
      // first we count the tokens
      int count = 1;
      boolean inescape = false;
      char c;
      StringBuilder buf = new StringBuilder();
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

    /**
     * Static convenience method for converting a string directly into an array of
     * String by using the delimiter and escape character as specified.
     */
    public static String[] toTokens(String line, char delim, char escape) {
      StringTokenizer2 tokenizer = new StringTokenizer2(line, delim, escape);
      return tokenizer.getAllTokens();
    }

    /**
     * Create a string with the delimiter an escape character as specified.
     */
    public static String toString(String[] tokens, char delim, char escape) {

      String token;
      int i, j;
      char c;
      StringBuilder buff = new StringBuilder();

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
      if (buff.length() > 0) {
        buff.setLength(buff.length() - 1);
      }
      return buff.toString();
    }

    public static String toString(List<String> tokenList, char delim, char escape) {
      String[] t = {};
      String[] tokens = tokenList.toArray(t);
      return toString(tokens, delim, escape);
    }

  }

  public static class Utils {

    private static final Logger logger = LoggerFactory.getLogger(Utils.class);

    public static final String ISO_DATE_PATTERN_yyyy_MM_dd = "yyyy-MM-dd";
    public static final String ISO_DATE_PATTERN_yyyy_MM_dd__HH_mm = "yyyy-MM-dd HH:mm";
    public static final String ISO_DATE_PATTERN_yyyy_MM_dd__HH_mm_ss = "yyyy-MM-dd HH:mm:ss SSS";
    public static final String ISO_DATE_PATTERN_HH_mm = "HH:mm";

    public static final String ISO_DATE_PATTERN_yyyy_MM_dd__HH_mm_ss__SSS = "yyyy-MM-dd HH:mm:ss SSS";

    public static ThreadLocal<SimpleDateFormat> IsoDateTL = ThreadLocal.withInitial(() -> new SimpleDateFormat(ISO_DATE_PATTERN_yyyy_MM_dd));

    public static ThreadLocal<SimpleDateFormat> IsoDateTimeTL = ThreadLocal.withInitial(() -> new SimpleDateFormat(ISO_DATE_PATTERN_yyyy_MM_dd__HH_mm));

    public static ThreadLocal<SimpleDateFormat> IsoDateTimeFullTL = ThreadLocal.withInitial(() -> new SimpleDateFormat(ISO_DATE_PATTERN_yyyy_MM_dd__HH_mm_ss));

    public static ThreadLocal<SimpleDateFormat> IsoTimeTL = ThreadLocal.withInitial(() -> new SimpleDateFormat(ISO_DATE_PATTERN_HH_mm));

    public static String nowIsoDateTimeFull() {
      return IsoDateTimeFullTL.get().format(new Date());
    }

    public static String nowIsoDateTime() {
      return IsoDateTimeTL.get().format(new Date());
    }

    public static String nowIsoTime() {
      return IsoTimeTL.get().format(new Date());
    }

    public static String nowIsoDate() {
      return IsoDateTL.get().format(new Date());
    }

    public static Date parseDate(String date) throws ParseException {
      return IsoDateTL.get().parse(date);
    }

    public static String toIsoDate(long timeMillis) {
      return IsoDateTL.get().format(new Date(timeMillis));
    }

    public static String toIsoDate(Date time) {
      return IsoDateTL.get().format(time);
    }

    public static Date parseTime(String date) throws ParseException {
      return IsoTimeTL.get().parse(date);
    }

    public static Triple<String, String, String> parse_statement(String statement) {
      statement = statement.trim();
      if (statement.isEmpty()) {
        return null;
      }

      int firstWhiteSpace = statement.length();
      for (int i = 0; i < statement.length(); i++) {
        char ch = statement.charAt(i);
        if (Character.isWhitespace(ch)) {
          firstWhiteSpace = i;
          break;
        }
      }

      String cmd = statement.substring(0, firstWhiteSpace).trim();
      String parameters = "";
      if (firstWhiteSpace != statement.length()) {
        parameters = statement.substring(firstWhiteSpace).trim();
      }

      if (Commands.isCmd(cmd)) {
        return new ImmutableTriple<>(cmd, parameters, statement);
      }
      return new ImmutableTriple<>("sql", statement, statement);
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

    public static String formatToDate(long time) {
      return toIsoDate(new Date(time));
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
      } catch (Exception ignored) {
      }
      return d != null;
    }

    //
    // public static long toTimeInMillis(String isoDateTimeString) {
    // try {
    // if (isoDateTimeString.length() ==
    // ISO_DATE_PATTERN_yyyy_MM_dd.length()) {
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

    public static long toDateInMillis(String isoDateString) {
      try {
        return IsoDateTL.get().parse(isoDateString).getTime();
      } catch (ParseException e) {
        logger.error(e.getMessage(), e);
      }
      return 0;
    }

    public static long toTimeInMillis(String isoDateTimeString) {
      long res = 0;
      try {
        res = IsoDateTimeFullTL.get().parse(isoDateTimeString).getTime();
        return res;
      } catch (ParseException ignored) {
      }
      try {
        res = IsoDateTimeSecTL.get().parse(isoDateTimeString).getTime();
        return res;
      } catch (ParseException ignored) {
      }
      try {
        res = IsoDateTimeTL.get().parse(isoDateTimeString).getTime();
        return res;
      } catch (ParseException ignored) {
      }
      try {
        res = IsoDateTL.get().parse(isoDateTimeString).getTime();
        return res;
      } catch (ParseException ignored) {
      }
      logger.error("Parse error for " + isoDateTimeString + ". Return 0.");
      return res;
    }

    public static String toIsoDateTimeSec(long timeMillis) {
      return toIsoDateTimeSec(new Date(timeMillis));
    }

    public static String toIsoDateTimeSec(Date date) {
      return IsoDateTimeSecTL.get().format(date);
    }

    public static ThreadLocal<SimpleDateFormat> IsoDateTimeSecTL = ThreadLocal.withInitial(() -> new SimpleDateFormat(ISO_DATE_PATTERN_yyyy_MM_dd__HH_mm_ss));

    public static String nowIsoDateTimeSec() {
      return toIsoDateTimeSec(new Date());
    }

    public static String getStackTrace(Throwable t) {
      StringBuilder buf = new StringBuilder();
      if (t != null) {
        buf.append(" <");
        buf.append(t);
        buf.append(">");

        java.io.StringWriter sw = new java.io.StringWriter(1024);
        java.io.PrintWriter pw = new java.io.PrintWriter(sw);
        t.printStackTrace(pw);
        pw.close();
        buf.append(sw);
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

    public static String trim(String str) {
      if (str == null) {
        return null;
      }
      return str.trim();
    }

    public static boolean isBlank(String str) {
      int strLen;
      if (str == null || (strLen = str.length()) == 0) {
        return true;
      }
      for (int i = 0; i < strLen; i++) {
        if ((!Character.isWhitespace(str.charAt(i)))) {
          return false;
        }
      }
      return true;
    }

    public static String removeWhitespace(String str) {
      if (str == null) {
        return null;
      }
      StringBuilder sb = new StringBuilder();
      for (char ch : str.toCharArray()) {
        if (Character.isWhitespace(ch)) {
          continue;
        }
        sb.append(ch);
      }
      return sb.toString();
    }

    public static String[] createSetGetNames(String head) {
      String normalCase = head.substring(0, 1).toUpperCase() + head.substring(1);
      String camelCase = camelCase(head);
      camelCase = camelCase.substring(0, 1).toUpperCase() + camelCase.substring(1);
      return new String[]{normalCase, camelCase};
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

    public static String[] tokenize(String string) {
      return tokenize(string, DEFAULT_DEL, DEFAULT_ESC);
    }

    public static String[] tokenize(String string, char del) {
      return tokenize(string, del, DEFAULT_ESC);
    }

    public static String joinTokens(Collection<String> list) {
      return joinTokens(list, DEFAULT_DEL, DEFAULT_ESC);
    }

    public static String joinTokens(Collection<String> list, char del, char esc) {
      if (isEmpty(list)) {
        return "";
      }
      StringBuilder res = new StringBuilder();
      int i = 0;
      for (String s : list) {
        s = escape(s, del, esc);
        if (i == 0) {
          res = new StringBuilder(s);
        } else {
          res.append(del).append(s);
        }
        i++;
      }
      return res.toString();
    }

    public static String joinTokens(String[] arr) {
      return joinTokens(arr, 0, arr.length, DEFAULT_DEL, DEFAULT_ESC);
    }

    public static String joinTokens(String[] arr, int start, char del, char esc) {
      return joinTokens(arr, start, arr.length, del, esc);
    }

    public static String joinTokens(String[] arr, int start, int end, char del, char esc) {
      List<String> list = new ArrayList<>(end - start);
      for (int i = start; i < arr.length && i < end; i++) {
        list.add(arr[i]);
      }
      return joinTokens(list, del, esc);
    }

    public static String escape(String in, char del, char esc) {
      char[] chars = in.toCharArray();
      StringBuilder res = new StringBuilder();
      for (char c : chars) {
        if (c == del || c == esc) {
          res.append(esc);
        }
        res.append(c);
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
      char c, pc = 0;
      StringBuilder buf = new StringBuilder();
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
          pc = c;
          continue;
        }
        if (c == esc && !inescape) {
          inescape = true;
          pc = c;
          continue;
        }
        //
        // append
        //
        if (c != del && pc == esc) {
          buf.append(pc);
        }
        buf.append(c);
        pc = c;
        inescape = false;
      }
      tokens[k] = buf.toString();
      return tokens;
    }

    public static Set<String> asSet(String... values) {
      Set<String> set = new HashSet<>();
      if (values != null) {
        set.addAll(Arrays.asList(values));
      }
      return set;
    }

    public static Set<String> asSet(String string, char del, char esc) {
      return asSet(tokenize(string, del, esc));
    }

    @SuppressWarnings("unchecked")
    public static <E> List<E> asList(E... elements) {
      ArrayList<E> list = new ArrayList<>(1);
      list.addAll(Arrays.asList(elements));
      return list;
    }

    //
    // Collection Utils
    //

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
      charsetName = charsetName == null ? ENCODING : charsetName;
      StringBuilder buf = new StringBuilder((int) file.length());
      try {
        r = new BufferedReader(new InputStreamReader(new FileInputStream(file), charsetName));
        int c;
        while ((c = r.read()) != -1) {
          buf.append(c);
        }
      } catch (Exception e) {
        logger.error(e.getMessage(), e);
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
      } catch (Exception ignored) {
      }
    }

    public static void closeQuietly(Connection connection) {
      try {
        connection.close();
      } catch (Exception e) {
        // ignore
      }
    }

    public static Object runQuery(Connection connection, String sqlStatement, Object... parameters) {
      PreparedStatement ps = null;
      ResultSet rs;
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
          return ps.getUpdateCount();
        }
      } catch (Throwable t) {
        logger.warn(t.getMessage());
      } finally {
        Utils.closeQuietly(ps);
      }
      return -1;
    }

    public static String sqlValueToString(Object value, int sqlType) throws IllegalArgumentException, SecurityException,
            IllegalAccessException, InvocationTargetException, NoSuchMethodException {

      if (value == null) {
        return "";
      }

      ISqlValue sqlValue = SqlValueMap.get(sqlType);
      if (sqlValue != null) {
        String s = sqlValue.toString(value);
        return s == null ? "" : s;
      }

      String strValue = value.toString().trim();
      if (value.getClass().getName().startsWith("oracle.sql.TIMESTAMP")) {
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
        logger.error(e.getMessage(), e);
      }
      return "";
    }

    public static void buildResult(int start, int max, ResultSet rs, IResultListener irl) {
      boolean fromDone = false;
      try {

        //
        ResultSetMetaData md = rs.getMetaData();

        int columnCount = rs.getMetaData().getColumnCount();
        List<String> header = new ArrayList<>(columnCount);
        List<Integer> sqlTypes = new ArrayList<>(columnCount);
        for (int i = 0; i < columnCount; i++) {
          String h = md.getColumnName(i + 1);
          if (Result.USE_CAMEL_CASE_FOR_RESULT_HEADER) {
            h = camelCase(h);
          }
          header.add(h);
          sqlTypes.add(md.getColumnType(i + 1));
        }
        //
        int counter = 0;
        irl.setHeader(header);

        sqlLogger.debug("sql-result-header " + header);
        irl.setFrom(-1);
        String[] row;
        boolean firstRow = true;
        List<String> sqlTypeNames = new ArrayList<>();

        while (rs.next()) {

          if (counter >= start && ((counter < start + max) || max == -1)) {

            row = new String[columnCount];

            for (int i = 0; i < columnCount; i++) {
              int t = sqlTypes.get(i);
              if (t == Types.BLOB) {
                logger.warn("BLOB type not supported! Returning empty string!");
                row[i] = "";
              } else {
                Object sqlValue = rs.getObject(i + 1);
                if (firstRow) {
                  sqlTypeNames.add(sqlValue == null ? "" : sqlValue.getClass().getSimpleName().toLowerCase());
                }
                row[i] = sqlValueToString(sqlValue, t);
              }
            }

            if (firstRow) {
              irl.setTypes(sqlTypeNames);
              firstRow = false;
            }

            irl.addRow(row);
            sqlLogger.debug("sql-result-row " + counter + " : " + Arrays.toString(row));
          }  //

          counter++;
        }
        irl.setFrom(-1);
        irl.setTotalCount(counter);

      } catch (Exception e) {
        irl.setException(e.getMessage());
      }
    }

    public static void copyResult(Result src, IResultListener dst) {
      dst.setName(src.name);
      dst.setHeader(src.header);
      dst.setTable(src.table);
      dst.setRowsAffected(src.rowsAffected);
      dst.setFrom(src.from);
      dst.setTotalCount(src.totalCount);
      dst.setException(src.exception);
    }

    //
    // COLLECTION UTILS
    //

    public static <E> boolean isEmpty(Collection<E> c) {
      return c == null || c.size() == 0;
    }

    //
    // MAP UTILS
    //

    public static <E, F> boolean isEmpty(Map<E, F> c) {
      return c == null || c.size() == 0;
    }

    public static Map<String, String> asMap(Object obj) {
      Map<String, String> map = new HashMap<>();
      Method[] methods = obj.getClass().getMethods();
      Field[] fields = obj.getClass().getFields();

      try {
        for (Method method : methods) {
          if (Modifier.isStatic(method.getModifiers())) {
            continue;
          }
          if (method.getParameterTypes().length == 0 && method.getName().startsWith("get")
                  && method.getName().length() > 3 && method.getReturnType().equals(String.class)) {
            String key = method.getName().substring(3, 4).toLowerCase() + method.getName().substring(4);
            Object r = method.invoke(obj, (Object[]) null);
            String value = rnn(r);
            map.put(key, value);
          }
        }

        for (Field field : fields) {
          if (Modifier.isStatic(field.getModifiers())) {
            continue;
          }
          String key = field.getName();
          if (map.containsKey(key)) {
            continue;
          }
          String value = rnn(field.get(obj));
          map.put(key, value);
        }
      } catch (Exception e) {
        logger.error(e.getMessage(), e);
      }
      return map;
    }

    public static <K, V> V firstValue(Map<K, V> map) {
      if (map == null) {
        return null;
      }
      for (Entry<K, V> e : map.entrySet()) {
        return e.getValue();
      }
      return null;
    }

    //
    // OBJECT UTILS
    //

    public static <E> E newObject(Map<String, String> values, Class<E> claxx) throws InstantiationException, IllegalAccessException {
      E e;
      e = claxx.newInstance();
      int colIndex = 0;
      for (String propertyName : values.keySet()) {
        if (isBlank(propertyName)) {
          propertyName = "" + colIndex;
        }
        String[] res = createSetGetNames(propertyName);
        Method m = null;
        Field f;
        for (String setGetName : res) {
          try {
            m = claxx.getMethod("set" + setGetName, String.class);
          } catch (Exception ignored) {
          }
          if (m != null) {
            break;
          }

        }
        if (m != null) {
          try {
            m.invoke(e, values.get(propertyName));
          } catch (Exception ignored) {
          }
        } else {
          try {
            f = claxx.getField(propertyName);
            f.set(e, values.get(propertyName));
          } catch (Exception ignored) {
          }
        }
        colIndex++;
      }
      return e;
    }

    public static Serializable deepClone(Serializable s) {
      if (s == null) {
        return null;
      }
      Serializable object = null;
      try {
        ByteArrayOutputStream bos = new ByteArrayOutputStream();
        ObjectOutputStream oos = new ObjectOutputStream(bos);
        oos.writeObject(s);
        oos.flush();
        oos.close();
        bos.close();
        byte[] byteData = bos.toByteArray();

        ByteArrayInputStream bais = new ByteArrayInputStream(byteData);
        ObjectInputStream bis = new ObjectInputStream(bais);
        object = (Serializable) bis.readObject();
        bis.close();
        bais.close();
      } catch (Exception e1) {
        logger.error(String.format("%s", e1));
      }
      return object;
    }

    public static String getStringGetterMethodName(Method method) {

      if (method.getParameterTypes().length != 0) {
        return null;
      }
      // if (String.class.equals(method.getReturnType()) == false) {
      // return null;
      // }
      String methodName = method.getName();

      if (!methodName.startsWith("get") || !(methodName.length() > 3) || !Character.isUpperCase(methodName.charAt(3))) {
        return null;
      }
      return Character.toLowerCase(methodName.charAt(3)) + methodName.substring(4);

    }

    @SuppressWarnings("unchecked")
    public static <E> Map<String, E> asMap(String keyProperty, List<E> list) {
      if (isEmpty(list)) {
        return new HashMap<>();
      }
      Class<E> claxx = (Class<E>) list.get(0).getClass();
      Map<String, E> map = new HashMap<>();
      try {
        String methodName = "get" + keyProperty.substring(0, 1).toUpperCase() + keyProperty.substring(1);
        Method m = null;
        Field f = null;
        try {
          m = claxx.getMethod(methodName);
        } catch (Exception e1) {
          f = claxx.getField(keyProperty);
        }

        for (E e : list) {
          if (m != null) {
            map.put(m.invoke(e).toString(), e);
          } else {
            map.put(f.get(e).toString(), e);
          }
        }
      } catch (Exception e) {
        logger.error(e.getMessage(), e);
      }
      return map;
    }

    public static Map<String, String> asPropertyMap(Object object) {
      Map<String, String> map = new HashMap<>();
      Method[] methods = object.getClass().getMethods();
      for (Method method : methods) {
        String name = Utils.getStringGetterMethodName(method);
        if (!Utils.isBlank(name)) {
          try {
            Object o = method.invoke(object, (Object[]) null);
            String value = o == null ? null : o.toString();
            if (!Utils.isBlank(value)) {
              map.put(name, value);
            }
          } catch (Exception e) {
            logger.warn(e.getMessage());
          }
        }
      }
      return map;
    }

    public static String resolve_value(String term, Request request) {
      String val;
      // reference to parameter
      term = term == null ? "" : term.trim();
      try {
        if (term.charAt(0) == ':') {
          val = request.get(term.substring(1));
          return val == null ? "" : val;
        }
      } catch (Exception e) {
        logger.debug(e.getMessage());
      }
      try {
        if (term.charAt(0) == '\'' && term.charAt(term.length() - 1) == '\'') {
          val = term.substring(1, term.length() - 1);
          return val;
        }
      } catch (Exception e) {
        logger.debug(e.getMessage());
      }
      return term;
    }

  }

  public static class JsonUtils {

    private static final Logger logger = LoggerFactory.getLogger(JsonUtils.class);

    public static String toJson(Object object) {
      Gson gson = new Gson();
      return gson.toJson(object);
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
      return JsonParser.parseString(s).getAsJsonObject();
    }

    public static String exception(String message) {
      return toJson("exception", message);
    }

    public static String message(String message) {
      return toJson("message", message);
    }

    public static Map<String, String> toStringMap(String jsonString) {
      JsonObject jsonObject = JsonParser.parseString(jsonString).getAsJsonObject();
      Set<Map.Entry<String, JsonElement>> jsonElements = jsonObject.entrySet();
      Map<String, String> map = new HashMap<>();
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
      try {
        return new Gson().fromJson(json, claxx);
      } catch (Exception e) {
        logger.warn("Could not convert json string to object. Class:" + claxx.getSimpleName() + ", json string:" + json,
                e);
      }
      return null;
    }

    public static <E> E toObjectSilently(String json, Class<E> claxx) {
      try {
        return new Gson().fromJson(json, claxx);
      } catch (Exception ignored) {
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
      List<Object> list = new ArrayList<>(ja.size());
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
      Map<String, Object> map = new HashMap<>();
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
      JsonElement je = JsonParser.parseString(jsonString).getAsJsonObject();
      return _toListMapE(je);
    }

    public static Object toObject(String json, Type type) {
      try {
        return new Gson().fromJson(json, type);
      } catch (Exception e) {
        logger.error(e.getMessage(), e);
      }
      return null;
    }

    //
    // LIST MAP end
    //

    @SuppressWarnings("unchecked")
    public static <E> E[] toArray(String jsonString, Class<E> claxx) {
      E[] array = null;
      Gson g = new Gson();
      JsonElement je = JsonParser.parseString(jsonString).getAsJsonObject();
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

  /**
   * Light weight persistency utility for working with a specific class.
   */
  public static class ObjectStore<E> {

    private static final Logger logger = LoggerFactory.getLogger(ObjectStore.class);

    public final Class<E> resultClass;
    public Set<String> roles;
    public final String userId;

    public ObjectStore(Class<E> resultClass, String userId, Set<String> roles) {

      this.resultClass = resultClass;
      // this.basicParams = basicParams;
      this.userId = userId;
      if (roles != null) {
        this.roles = roles;
      } else {
        this.roles = new HashSet<>();
      }
    }

    public ObjectStore(Class<E> resultClass, Set<String> roles) {
      this(resultClass, RemoteQuery.ANONYMOUS, roles);
    }

    public ObjectStore(Class<E> resultClass) {
      this(resultClass, RemoteQuery.ANONYMOUS, new HashSet<>());
    }

    public E asObject(Map<String, String> params) {
      try {
        return Utils.newObject(params, this.resultClass);
      } catch (Exception e) {
        logger.error(e.getMessage());
        return null;
      }
    }

    public E asObject(Request request) {
      return asObject(request.getParameterSnapshhot());
    }

    private Result _process(String serviceId, Map<String, String> params) {
      Request request = new Request();
      request.setUserId(userId);
      request.setRoles(roles);
      request.setServiceId(serviceId);
      request.setParameters(params);

      Runner runner = new Runner();
      return runner.run(request);
    }

    public List<E> search(String serviceId, Map<String, String> params) {
      Result pr = _process(serviceId, params);
      List<E> result = null;
      if (pr != null) {
        result = pr.asList(resultClass);
      }
      return result;
    }

    public List<E> search(String serviceId, Object object) {
      if (object == null) {
        return search(serviceId);
      }
      Map<String, String> params = Utils.asMap(object);
      return search(serviceId, params);
    }

    public List<E> search(String serviceId) {
      return search(serviceId, null);
    }

    public E get(String serviceId, Map<String, String> params) {
      List<E> result = this.search(serviceId, params);
      return !Utils.isEmpty(result) ? result.get(0) : null;
    }

    public E get(String serviceId, E object) {
      List<E> result = this.search(serviceId, object);
      return !Utils.isEmpty(result) ? result.get(0) : null;
    }

    /**
     * @return map
     * @deprecated use Request.run() ...
     */
    public Map<String, String> getMap(String serviceId, Map<String, String> parameters) {
      Request request = new Request();
      request.setUserId(userId);
      request.setRoles(roles);
      request.setServiceId(serviceId);
      for (Entry<String, String> e : parameters.entrySet()) {
        request.put(e.getKey(), e.getValue());
      }

      Runner runner = new Runner();
      Result r = runner.run(request);
      if (r.size() > 0) {
        return r.getRowAsMap(0);
      }
      return new HashMap<>();
    }

    public E get(String serviceId) {
      return get(serviceId, new HashMap<>());
    }

    public Result update(String serviceId, Map<String, String> parameters) {
      return _process(serviceId, parameters);
    }

    public Result update(String serviceId, E object) {
      Map<String, String> params = Utils.asMap(object);
      return _process(serviceId, params);
    }

    public Result update(String serviceId) {
      return _process(serviceId, null);
    }

  }

  public static void shutdown() {
    Utils.IsoDateTimeSecTL.remove();
    Utils.IsoDateTimeTL.remove();
    Utils.IsoDateTL.remove();
    Utils.IsoTimeTL.remove();
    Utils.IsoDateTimeFullTL.remove();
  }

  public static class PropertyGetSet<C> {

    private Method getMethod;
    private Method setMethod;
    private Field field;

    public PropertyGetSet(Class<C> claxx, String property) {

      String baseName = property.substring(0, 1).toUpperCase() + property.substring(1);

      try {
        getMethod = claxx.getMethod("get" + baseName);
      } catch (Exception ignored) {

      }
      try {
        setMethod = claxx.getMethod("set" + baseName, String.class);
      } catch (Exception ignored) {

      }
      try {
        field = claxx.getField(property);
      } catch (Exception ignored) {
      }
      if (field == null && getMethod == null) {
        logger.info("No read access to property : " + property + " in class " + claxx.getSimpleName());
      }
      if (field == null && setMethod == null) {
        logger.info("No write access to property : " + property + " in class " + claxx.getSimpleName());
      }
    }

    public String get(C e) {
      if (getMethod != null)
        try {
          return getMethod.invoke(e).toString();
        } catch (Exception ignored) {

        }
      if (field != null)
        try {
          return field.get(e).toString();
        } catch (Exception e1) {
          // TODO
        }
      return null;
    }

    public void set(C e, String value) {
      if (setMethod != null)
        try {
          setMethod.invoke(e, value);
          return;
        } catch (Exception ignored) {
        }
      if (field != null)
        try {
          field.set(e, value);
        } catch (Exception e1) {
          // TODO
        }
    }

  }

  public static class MLTokenizer {

    public static String trimLeft(String s, String separators, char esc) {
      if (s == null) {
        return "";
      }
      for (int current = 0; current < s.length(); current++) {
        char c = s.charAt(current);

        if (c == esc) {
          return s.substring(Math.min(current + 1, s.length() - 1));
        } else if (separators.contains("" + c)) {
          continue;
        }
        return s.substring(current);
      }
      return "";
    }

    public static void addText(List<String> list, String text, boolean stripQuotes) {
      if (text != null) {
        text = text.trim();
        if (stripQuotes) {
          if (text.startsWith("'")) {
            text = text.substring(1);
          }
          if (text.endsWith("'")) {
            text = text.substring(0, text.length() - 1);
          }
          text = text.trim();
        }

        if (text.length() > 0) {
          list.add(text);
        }
      }
    }

  }

  /**
   * A Statement Node represents a single statement like a sql, if, end, set and
   * other statements. Some statements like if or while statements have children
   * which might run conditionally.
   *
   * @author tonibuenter
   */
  public static class StatementNode {

    public String statement;
    public String cmd;
    public String parameter;
    public List<StatementNode> children;
    public int pointer = 0;

    public StatementNode(String cmd, String parameter, String statement) {
      super();
      this.cmd = cmd;
      this.parameter = parameter;
      this.statement = statement;
      this.children = new ArrayList<>();
    }

    public StatementNode(String cmd) {
      this(cmd, "", "");
    }

    public StatementNode append(StatementNode... cbChildren) {
      children.addAll(Arrays.asList(cbChildren));
      return this;
    }

    public String toString() {
      StringBuffer sb = new StringBuffer();
      this._toString("", sb);
      return sb.toString();
    }

    private void _toString(String indent, StringBuffer sb) {
      sb.append(indent).append(this.cmd).append(" -> ").append(parameter).append("\n");
      for (StatementNode child : this.children) {
        child._toString(" " + indent, sb);
      }
    }

  }

  //
  //
  // BUILD IN COMMANDS -start-
  //
  //

  public static class SqlCommand implements ICommand {
    public Result run(Request request, Result currentResult, StatementNode statementNode, ServiceEntry serviceEntry) {
      Result r = new Result();
      processSql(statementNode.parameter, request, serviceEntry, r);
      return r;
    }
  }

  public static class SetCommand implements ICommand {

    public boolean overwrite = true;

    public Result run(Request request, Result currentResult, StatementNode statementNode, ServiceEntry serviceEntry) {
      String[] nv = Utils.tokenize(statementNode.parameter, '=');
      String n = nv[0];
      String v = nv.length > 1 ? nv[1] : null;
      n = Utils.trim(n);
      v = Utils.resolve_value(v, request);
      String requestValue = request.get(n);

      if (overwrite || Utils.isBlank(requestValue)) {
        request.put(n, v);
      }
      return currentResult;
    }
  }

  public static class SetIfEmptyCommand extends SetCommand {
    {
      overwrite = false;
    }
  }

  public static class CopyCommand implements ICommand {

    public boolean overwrite = true;

    public Result run(Request request, Result currentResult, StatementNode statementNode, ServiceEntry serviceEntry) {
      String[] nv = Utils.tokenize(statementNode.parameter, '=');
      if (nv.length != 2) {
        logger.warn("Expected parameter-part should be like name1 = name2. But was: " + statementNode.parameter);
        return currentResult;
      }
      String targetName = Utils.trim(nv[0]);
      String sourceName = Utils.trim(nv[1]);

      String oldValue = request.get(targetName);
      String newValue = request.get(sourceName);

      if (overwrite || Utils.isBlank(oldValue)) {
        request.put(targetName, newValue);
      }
      return currentResult;
    }
  }

  public static class CopyIfEmptyCommand extends CopyCommand {
    {
      overwrite = false;
    }
  }

  public static class ParametersCommand implements ICommand {

    protected boolean overwrite = true;

    @Override
    public Result run(Request request, Result currentResult, StatementNode statementNode, ServiceEntry serviceEntry) {

      StatementNode iCb;

      Triple<String, String, String> t = Utils.parse_statement(statementNode.parameter);
      if (t == null || t.getLeft() == null) {
        logger.error("Could not parse statement: " + statementNode.parameter);
        return currentResult;
      }
      String cmd = t.getLeft();
      String parameter = t.getMiddle();
      String statement = t.getRight();
      iCb = new StatementNode(cmd, parameter, statement);

      Result iResult = processCommandBlock(iCb, request, currentResult, serviceEntry);

      if (iResult == null || (iResult.header == null || iResult.header.size() == 0)) {
        return currentResult;
      }

      // empty all paramters if overwrite == true
      if (overwrite) {
        for (String head : iResult.header) {
          request.put(head, "");
        }
      }

      Map<String, String> paramters = iResult.size() > 0 ? iResult.getRowAsMap(0) : new HashMap<>();

      for (String key : iResult.header) {
        if (Utils.isEmpty(request.get(key)) || overwrite) {
          request.put(key, paramters.get(key));
        }
      }

      return currentResult;
    }

  }

  public static class ParametersIfEmptyCommand extends ParametersCommand {
    {
      overwrite = false;
    }
  }

  public static class ParametersConcatCommand implements ICommand {

    protected boolean overwrite = true;

    @Override
    public Result run(Request request, Result currentResult, StatementNode statementNode, ServiceEntry serviceEntry) {

      StatementNode iCb;

      Triple<String, String, String> t = Utils.parse_statement(statementNode.parameter);
      if (t == null || t.getLeft() == null) {
        logger.error("Could not parse statement: " + statementNode.parameter);
        return currentResult;
      }
      String cmd = t.getLeft();
      String parameter = t.getMiddle();
      String statement = t.getRight();
      iCb = new StatementNode(cmd, parameter, statement);

      Result iResult = processCommandBlock(iCb, request, currentResult, serviceEntry);

      if (iResult == null || (iResult.header == null || iResult.header.size() == 0)) {
        return currentResult;
      }

      // empty all paramters if overwrite == true
      if (overwrite) {
        for (String head : iResult.header) {
          request.put(head, "");
        }
      }

      Map<String, List<String>> listMap = new HashMap<>(iResult.header.size());

      for (String head : iResult.header) {
        listMap.put(head, new ArrayList<>(iResult.table.size()));
      }

      for (int i = 0; i < iResult.table.size(); i++) {
        List<String> row = iResult.table.get(i);
        for (int j = 0; j < row.size(); j++) {
          String head = iResult.header.get(j);
          listMap.get(head).add(row.get(j));
        }
      }

      for (String key : listMap.keySet()) {
        List<String> values = listMap.get(key);
        String value = StringTokenizer2.toString(values, ',', '\\');
        if (Utils.isEmpty(request.get(key)) || overwrite) {
          request.put(key, value);
        }
      }

      return currentResult;
    }

  }

  public static class ParametersConcatIfEmptyCommand extends ParametersCommand {
    {
      overwrite = false;
    }
  }

  public static class ServiceIdCommand implements ICommand {
    public Result run(Request request, Result currentResult, StatementNode statementNode, ServiceEntry serviceEntry) {
      Request iRequest = request.deepCopy().setServiceId(statementNode.parameter);
      return iRequest.run();
    }
  }

  public static class JavaCommand implements ICommand {
    public Result run(Request request, Result currentResult, StatementNode statementNode, ServiceEntry serviceEntry) {
      try {
        return processJava(statementNode.parameter, statementNode.statement, request, currentResult, statementNode,
                serviceEntry);
      } catch (Exception e) {
        logger.error(e.getMessage(), e);
      }
      return currentResult;
    }
  }

  public static class ServiceRootCommand implements ICommand {
    public Result run(Request request, Result currentResult, StatementNode statementNode, ServiceEntry serviceEntry) {
      for (StatementNode cbChild : statementNode.children) {
        Result r = processCommandBlock(cbChild, request, currentResult, serviceEntry);
        currentResult = r != null ? r : currentResult;
      }
      return currentResult;
    }
  }

  public static class NoOpCommand implements ICommand {
    public Result run(Request request, Result currentResult, StatementNode statementNode, ServiceEntry serviceEntry) {
      return currentResult;
    }
  }

  public static class IfCommand implements ICommand {

    private boolean ifEmpty = false;

    public IfCommand(boolean ifEmpty) {
      this.ifEmpty = ifEmpty;
    }

    public IfCommand() {

    }

    public Result run(Request request, Result currentResult, StatementNode statementNode, ServiceEntry serviceEntry) {

      String condition = Utils.resolve_value(statementNode.parameter, request);

      // boolean isThen =
      // !Utils.isBlank(request.get(statementNode.parameter));
      boolean isThen = condition.length() > 0;

      isThen = ifEmpty != isThen;

      for (StatementNode cbChild : statementNode.children) {
        if ("else".equals(cbChild.cmd)) {
          isThen = !isThen;
          continue;
        }
        if (isThen) {
          Result r = processCommandBlock(cbChild, request, currentResult, serviceEntry);
          currentResult = r != null ? r : currentResult;
        }
      }

      return currentResult;
    }
  }

  public static class FailCommand implements ICommand {

    public Result run(Request request, Result currentResult, StatementNode statementNode, ServiceEntry serviceEntry) {
      logger.error("Fail Command! You should never reached this point! statementNode: " + statementNode);
      return currentResult;
    }
  }

  public static class SwitchCommand implements ICommand {

    public Result run(Request request, Result currentResult, StatementNode statementNode, ServiceEntry serviceEntry) {

      String switchValue = Utils.resolve_value(statementNode.parameter, request);

      boolean inSwitch = false;
      boolean caseFound = false;

      for (StatementNode cbChild : statementNode.children) {

        if ("break".equals(cbChild.cmd)) {
          inSwitch = false;
          continue;
        }

        if ("case".equals(cbChild.cmd)) {
          String caseParameter = Utils.resolve_value(cbChild.parameter, request);
          if (caseParameter.equals(switchValue)) {
            caseFound = true;
            inSwitch = true;
          }
        }

        if ("default".equals(cbChild.cmd)) {
          inSwitch = !caseFound || inSwitch;
          continue;
        }

        if (inSwitch) {
          Result r = processCommandBlock(cbChild, request, currentResult, serviceEntry);
          currentResult = r != null ? r : currentResult;
        }
      }

      return currentResult;
    }
  }

  public static class ForeachCommand implements ICommand {

    public Result run(Request request, Result currentResult, StatementNode statementNode, ServiceEntry serviceEntry) {

      Result indexResult = RemoteQuery.processCommand(statementNode.parameter, request, currentResult, serviceEntry);

      if (indexResult == null || indexResult.table.size() == 0) {
        return currentResult;
      }

      List<Map<String, String>> list = indexResult.asList();

      Request iRequest = request.deepCopy();

      int index = 1;
      for (Map<String, String> map : list) {
        iRequest.put(map);
        iRequest.put("$INDEX", index + "");
        for (StatementNode child : statementNode.children) {
          Result r = RemoteQuery.processCommandBlock(child, iRequest, currentResult, serviceEntry);
          currentResult = r == null ? currentResult : r;
        }
        index += 1;
      }
      return currentResult;
    }
  }

  public static class WhileCommand implements ICommand {

    public Result run(Request request, Result currentResult, StatementNode statementNode, ServiceEntry serviceEntry) {
      int counter = 0;
      while (counter < MAX_WHILE) {
        String whileCondition = Utils.resolve_value(statementNode.parameter, request);
        if (Utils.isBlank(whileCondition)) {
          break;
        }
        counter++;
        for (StatementNode child : statementNode.children) {
          Result r = RemoteQuery.processCommandBlock(child, request, currentResult, serviceEntry);
          currentResult = r == null ? currentResult : r;
        }
      }
      return currentResult;
    }
  }

  public static class AddRoleCommand implements ICommand {

    public Result run(Request request, Result currentResult, StatementNode statementNode, ServiceEntry serviceEntry) {
      if (!Utils.isBlank(statementNode.parameter)) {
        request.addRole(statementNode.parameter);
      }
      return currentResult;
    }
  }

  public static class RemoveRoleCommand implements ICommand {

    public Result run(Request request, Result currentResult, StatementNode statementNode, ServiceEntry serviceEntry) {
      if (!Utils.isBlank(statementNode.parameter)) {
        request.removeRole(statementNode.parameter);
      }
      return currentResult;
    }
  }

  public static class CommentCommand implements ICommand {

    public Result run(Request request, Result currentResult, StatementNode statementNode, ServiceEntry serviceEntry) {
      if (!Utils.isBlank(statementNode.parameter)) {
        String comment = RemoteQueryUtils.texting(statementNode.parameter, request.getParameterSnapshhot());
        logger.info("comment: " + comment);
        ProcessLog.Current().infoUser(comment);
      }
      return currentResult;
    }
  }

  //
  //
  // BUILD IN COMMANDS -end-
  //
  //

  //
  //
  // DATA CACHE FOR SQL STATEMENTS
  //
  //
  public static class DataCache {

    private final Object lock = new Object();
    private final Map<String, Result> cache = new HashMap<>();
    private final Map<String, Long> times = new HashMap<>();

    public Result get(String cacheKey, Integer cacheSeconds) {
      Long time = times.get(cacheKey);
      if (time == null || cacheSeconds == null || cacheSeconds < 1) {
        return null;
      }
      if ((System.currentTimeMillis() - time) > cacheSeconds * 1000) {
        synchronized (lock) {
          cache.remove(cacheKey);
          times.remove(cacheKey);
        }
        return null;
      }
      return cache.get(cacheKey);
    }

    public void put(String cacheKey, IResultListener irl, Integer cacheSeconds) {
      Result r = null;
      try {
        r = (Result) irl;
      } catch (Exception e) {
        logger.debug("irl is not a Result object");
      }

      if (cacheSeconds == null || cacheSeconds < 1 || r == null) {
        return;
      }
      synchronized (lock) {
        cache.put(cacheKey, r);
        times.put(cacheKey, System.currentTimeMillis());
      }
    }
  }


}
