package org.remotequery;

import static org.remotequery.z_RemoteQuery.ENCODING;

import java.io.File;
import java.io.FileInputStream;
import java.io.IOException;
import java.io.InputStream;
import java.io.OutputStream;
import java.io.Serializable;
import java.util.ArrayList;
import java.util.Enumeration;
import java.util.HashMap;
import java.util.HashSet;
import java.util.List;
import java.util.Map;
import java.util.Map.Entry;
import java.util.Properties;
import java.util.Set;

import javax.servlet.ServletConfig;
import javax.servlet.ServletContext;
import javax.servlet.ServletException;
import javax.servlet.http.HttpServlet;
import javax.servlet.http.HttpServletRequest;
import javax.servlet.http.HttpServletResponse;
import javax.servlet.http.HttpSession;

import org.apache.commons.io.IOUtils;
import org.remotequery.z_RemoteQuery.JsonUtils;
import org.remotequery.z_RemoteQuery.ProcessLog;
import org.remotequery.z_RemoteQuery.Request;
import org.remotequery.z_RemoteQuery.Result;
import org.remotequery.z_RemoteQuery.Utils;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

/**
 * The following init parameters are checked by the RemoteQueryServlet:
 * "accessServiceId" and "requestDataHandler". The "accessServiceId" defines a
 * RemoteQuery (RQ) that will be called before the main RQ defined by the HTTP
 * request parameters. If there is a "accessServiceId" RQ and the call returns
 * an Exception string the main RQ is not executed. If the "accessServiceId" RQ
 * return all SESSION level parameters of the RQ request are written back to the
 * HTTP session object.
 */
public class RemoteQueryServlet extends HttpServlet {
	/**
	   * 
	   */
	private static final long serialVersionUID = 1L;

	/**
	 * Class containing constants for the web programming with RemoteQuery.
	 * 
	 * @author tonibuenter
	 */
	public static final class WebConstants {

		/**
		 * "$WEBROOT" is the name to which the web root directory is bound to
		 * (relevant only for web application).
		 */
		public static final String $WEBROOT = "$WEBROOT";
		/**
		 * "$DATE" is the inital parameter name which will have an iso formated
		 * date string of the time the request is created.
		 */
		public static final String $DATE = "$DATE";
		// TODO not used ... public static final String $REQUESTID =
		// "$REQUESTID";
		/**
		 * Initial level parameter name for the serviceId value (relevant only
		 * for web application).
		 */
		public static final String $SERVICEID = "$SERVICEID";

		public static final String $TIMESTAMP = "$TIMESTAMP";

		public static final String $CURRENT_TIME_MILLIS = "$CURRENT_TIME_MILLIS";

		public static final String dataurl_ = "dataurl_";

		public static final int MAX_FIELD_LENGTH = 50 * 1024 * 1024;

	}

	//
	//
	//
	private String servletName = "";
	private String webRoot = "";
	private String accessServiceId = "";
	private Set<String> headerParameters;

	private Set<String> publicServiceIds = new HashSet<String>();
	private String requestDataHandler = "";

	//
	private static final Logger logger = LoggerFactory.getLogger(RemoteQueryServlet.class);

	@Override
	public void init(ServletConfig config) throws ServletException {
		super.init(config);
		String s;
		logger.info("RemoteQueryServlet starting ...");

		ServletContext sc = config.getServletContext();
		//
		// APP PROPERTIES
		//
		Properties appProperties = new Properties();

		String appPropertiesFile = System.getProperty("REMOTEQUERY_APP_PROPERTIES_FILE");
		if (!Utils.isBlank(appPropertiesFile)) {
			System.out.println("FOUND REMOTEQUERY_APP_PROPERTIES_FILE from system properties : " + appPropertiesFile);
		}
		if (Utils.isBlank(appPropertiesFile)) {
			appPropertiesFile = sc.getInitParameter("appPropertiesFile");
		}
		if (Utils.isBlank(appPropertiesFile)) {
			logger.error(
					"No value for appPropertiesFile defined in system properties and servlet context init parameters. RemoteQueryServlet will not continue.");
			return;
		}
		InputStream in = null;
		try {
			in = new FileInputStream(new File(appPropertiesFile));
			appProperties.load(in);
		} catch (Exception e) {
			logger.error("Can not read appProperties file: " + appPropertiesFile
					+ ". RemoteQueryServlet will not continue.");
			return;
		} finally {
			IOUtils.closeQuietly(in);
		}

		//
		servletName = config.getServletName();
		//
		webRoot = this.getServletContext().getRealPath("/");
		//
		accessServiceId = appProperties.getProperty("accessServiceId");
		//
		s = appProperties.getProperty("headerParameters");
		if (!Utils.isBlank(s)) {
			headerParameters = Utils.asSet(Utils.tokenize(s));
		}
		//
		s = appProperties.getProperty("publicServiceIds");
		if (!Utils.isBlank(s)) {
			publicServiceIds = Utils.asSet(Utils.tokenize(s));
		}
		//
		requestDataHandler = appProperties.getProperty("requestDataHandler");

		logger.info("RemoteQueryServlet started");
	}

	@Override
	public void doPost(HttpServletRequest httpRequest, HttpServletResponse response)
			throws ServletException, IOException {
		doGet(httpRequest, response);
	}

	@Override
	public void doGet(final HttpServletRequest httpRequest, HttpServletResponse httpResponse)
			throws ServletException, IOException {
		logger.debug("start " + servletName + ".doGet");

		String serviceId = "";

		long startTime = System.currentTimeMillis();

		//
		// Checking user principal (userId) from the HTTP request - if empty
		// ANONYMOUS is set.
		//

		String userId = httpRequest.getUserPrincipal() != null ? httpRequest.getUserPrincipal().getName()
				: z_RemoteQuery.ANONYMOUS;
		HttpSession session = httpRequest.getSession();

		String callback = httpRequest.getParameter("callback");

		//
		// Last part in URL is the service id. If empty an exception result is
		// returned: '-no service id-'
		//

		String requestUri = httpRequest.getRequestURI();
		logger.debug("Request URI " + requestUri);
		String[] parts = requestUri.split("/");
		if (parts.length > 0) {
			serviceId = parts[parts.length - 1];
		}
		logger.debug("serviceId: " + serviceId);
		if (Utils.isBlank(serviceId)) {
			logger.error("-no service id-" + requestUri);
			returnAsJsonString(callback, JsonUtils.toJson("-no service id-"), httpResponse);
			return;
		}

		try {
			ProcessLog pLog = ProcessLog.Current();

			//
			// Create RQ request with userId.
			//

			Request request = new Request();
			request.setUserId(userId);

			//
			//
			// HTTP SESSION ATTRIBUTES
			//
			//

			String[] sessionNames = { "sessionId" };
			for (String name : sessionNames) {
				Object value = session.getAttribute(name);
				if (value instanceof String) {
					request.put(name, (String) value);
					logger.debug("http session attribute: " + name + ":" + value);
				}
			}

			//
			//
			// HTTP REQUEST PARAMETERS
			//
			//

			IRequestData requestData = null;
			if (!Utils.isBlank(requestDataHandler)) {
				try {
					IRequestDataHandler rdh = (IRequestDataHandler) Class.forName(requestDataHandler).newInstance();
					requestData = rdh.process(httpRequest);
				} catch (Exception e1) {
					logger.warn("Exception in IRequestDataHandler creation: " + e1.getMessage()
							+ " Fallback to default IRequestDataHandler!");
				}
			}
			if (requestData == null) {
				logger.debug("Use default IRequestDataHandler.");
				requestData = getRequestData(httpRequest);
			}

			// Copy String parameters
			Map<String, List<String>> param = requestData.getParameters();
			for (Entry<String, List<String>> paramEntry : param.entrySet()) {
				String name = paramEntry.getKey();
				List<String> values = paramEntry.getValue();
				String value = null;
				if (values != null) {
					if (values.size() > 1) {
						value = Utils.joinTokens(values);
						logger.debug("paramEntry (multi value joined!): " + name + ":" + value);
					} else {
						value = values.get(0);
						logger.debug("paramEntry: " + name + ":" + value);
					}
					request.put(name, value);
				}
			}

			//
			//
			// HTTP HEADERS
			//
			//

			if (headerParameters != null) {
				for (String name : headerParameters) {
					String value = httpRequest.getHeader(name);
					if (!Utils.isBlank(value)) {
						request.put(name, value);
						logger.debug("http header: " + name + ":" + value);
					}
				}
			}

			//
			//
			// PARAMETER OVERWRITES ...
			//
			//

			request.put(WebConstants.$WEBROOT, webRoot);

			request.put(WebConstants.$SERVICEID, serviceId);

			request.put(WebConstants.$CURRENT_TIME_MILLIS, "" + startTime);

			request.put(WebConstants.$TIMESTAMP, Utils.toIsoDateTimeSec(startTime));

			request.put(WebConstants.$DATE, Utils.toIsoDate(startTime));

			//
			//
			// TRANSIENT ATTRIBUTES
			//
			//

			request.setTransientAttribute("httpRequest", httpRequest);
			request.setTransientAttribute("requestData", requestData);

			//
			// Authentication and authorisation check. When accessServiceId is
			// set,
			// the RQ request is run with this accessServiceId.
			// In case the RQ result has an Exception (string) the processing
			// aborts
			// and the exception is return as a JSON string.
			//

			//
			// When accessServiceId is available the corresponding RQ is
			// executed with
			// the accessServiceId. If the
			// result of the RQ run is null or the exception of the run is null.
			// RQ SESSION parameters are written (back) to the HTTP session.
			//

			if (publicServiceIds.contains(publicServiceIds)) {
				logger.debug("ServiceId " + serviceId + " is a public service. No access service will be processed.");
			} else if (Utils.isBlank(accessServiceId)) {
				logger.debug("Servlet Parameter accessServiceId is not defined. No access service will be processed.");
			} else if (!Utils.isBlank(accessServiceId) && !publicServiceIds.contains(serviceId)) {
				logger.debug("ServiceId " + serviceId
						+ " is  not a public service and a access service is defined (servlet parameter accessServiceId). Access service will be call first.");
				request.setServiceId(accessServiceId).run();
				String sessionId = request.get("sessionId");
				if (!Utils.isBlank(sessionId)) {
					session.setAttribute("sessionId", sessionId);
				} else {

					returnAsJsonString(callback, JsonUtils.exception("no access (wrong password?)"), httpResponse);

					// httpResponse.sendError(401);
					return;
				}
			}

			//
			// RUN MAIN REQUEST
			//

			request.setServiceId(serviceId);

			Result result = request.run();
			pLog.system("Request time used (ms):" + (System.currentTimeMillis() - startTime), logger);

			//
			// RQ SESSION parameters are written (back) to the HTTP session.
			//

			// Map<String, String> newSessionMap =
			// request.getParameters(SESSION);
			// for (Entry<String, String> sessionEntry :
			// newSessionMap.entrySet()) {
			// session.setAttribute(sessionEntry.getKey(),
			// sessionEntry.getValue());
			// }

			// if (!Utils.isBlank(session.getId())) {
			// for (Entry<String, String> sessionEntry :
			// newSessionMap.entrySet()) {
			// session.setAttribute(sessionEntry.getKey(),
			// sessionEntry.getValue());
			// }
			// } else {
			// logger.warn("HttpSession session.getId is blank : session
			// invalid");
			// }

			//
			// Writing result to HttpResponse
			//

			if (result != null) {
				// prevent circular references
				result.subResult = null;
				String s = JsonUtils.toJson(result);
				returnAsJsonString(callback, s, httpResponse);
			} else {
				returnAsJsonString(callback, JsonUtils.toJson("empty"), httpResponse);
			}
		} catch (IllegalStateException ie) {
			logger.warn(ie.getMessage());
		} catch (Exception e) {
			logger.error(e.getMessage(), e);
		} finally {
			ProcessLog.RemoveCurrent();
		}
	}

	public static void returnAsJsonString(String callback, String s, HttpServletResponse response) {
		try {

			byte[] document = s.getBytes(ENCODING);
			response.setContentType("application/json");
			response.setContentLength(document.length);
			response.setCharacterEncoding(ENCODING);

			OutputStream out = response.getOutputStream();

			if (Utils.isBlank(callback)) {
				out.write(document);
			} else {
				out.write((callback + "(").getBytes());
				out.write(document);
				out.write(")".getBytes());
			}
			out.flush();
		} catch (Exception e) {
			logger.error(e.getMessage(), e);
		}
	}

	public static class RequestData implements Serializable, IRequestData {

		private final Map<String, List<String>> parameters = new HashMap<String, List<String>>();
		/**
		 * 
		 */
		private static final long serialVersionUID = 1L;

		public void add(String name, String value) {
			List<String> values = parameters.get(name);
			if (values == null) {
				values = new ArrayList<String>();
				parameters.put(name, values);
			}
			values.add(value);
		}

		public String getParameter(String name) {
			List<String> values = parameters.get(name);
			return values != null && values.size() > 0 ? values.get(0) : null;
		}

		public List<String> getParameterValues(String name) {
			return parameters.get(name);
		}

		public Map<String, List<String>> getParameters() {
			return parameters;
		}

	}

	public static RequestData getRequestData(HttpServletRequest httpRequest) {
		RequestData rd = new RequestData();
		@SuppressWarnings("rawtypes")
		Enumeration e = httpRequest.getParameterNames();
		while (e.hasMoreElements()) {
			String name = (String) e.nextElement();
			String[] values = httpRequest.getParameterValues(name);
			for (String value : values) {
				rd.add(name, value);
				logger.debug("http request parameter: " + name + ":" + value);
			}

		}
		return rd;
	}

	public interface IRequestDataHandler {
		IRequestData process(HttpServletRequest httpRequest) throws Exception;
	}

	public interface IRequestData {
		Map<String, List<String>> getParameters();
	}

}
