package org.remotequery;

import static org.remotequery.RemoteQuery.ENCODING;
import static org.remotequery.RemoteQuery.LevelConstants.HEADER;
import static org.remotequery.RemoteQuery.LevelConstants.INITIAL;
import static org.remotequery.RemoteQuery.LevelConstants.REQUEST;
import static org.remotequery.RemoteQuery.LevelConstants.SESSION;

import java.io.IOException;
import java.io.OutputStream;
import java.io.Serializable;
import java.util.ArrayList;
import java.util.Enumeration;
import java.util.HashMap;
import java.util.List;
import java.util.Map;
import java.util.Map.Entry;
import java.util.logging.Logger;

import javax.servlet.ServletConfig;
import javax.servlet.ServletException;
import javax.servlet.http.HttpServlet;
import javax.servlet.http.HttpServletRequest;
import javax.servlet.http.HttpServletResponse;
import javax.servlet.http.HttpSession;

import org.remotequery.RemoteQuery.JsonUtils;
import org.remotequery.RemoteQuery.MainQuery;
import org.remotequery.RemoteQuery.ProcessLog;
import org.remotequery.RemoteQuery.Request;
import org.remotequery.RemoteQuery.Result;
import org.remotequery.RemoteQuery.Utils;

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
		 * "$DATE" is the inital parameter name which will have an iso formated date
		 * string of the time the request is created.
		 */
		public static final String $DATE = "$DATE";
		// TODO not used ... public static final String $REQUESTID = "$REQUESTID";
		/**
		 * Initial level parameter name for the serviceId value (relevant only for
		 * web application).
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
	private String requestDataHandler = "";

	//
	private static final Logger logger = Logger
	    .getLogger(RemoteQueryServlet.class.getName());

	@Override
	public void init(ServletConfig config) throws ServletException {
		super.init(config);
		logger.info("RemoteQueryServlet starting ...");
		config.getServletContext().getAttributeNames();

		//
		servletName = config.getServletName();
		//
		webRoot = this.getServletContext().getRealPath("/");
		//
		accessServiceId = config.getInitParameter("accessServiceId");
		//
		requestDataHandler = config.getInitParameter("requestDataHandler");

	}

	@Override
	public void doPost(HttpServletRequest httpRequest,
	    HttpServletResponse response) throws ServletException, IOException {
		doGet(httpRequest, response);
	}

	@Override
	public void doGet(final HttpServletRequest httpRequest,
	    HttpServletResponse httpResponse) throws ServletException, IOException {
		logger.fine("start " + servletName + ".doGet");

		String serviceId = "";

		//
		// Checking user principal (userId) from the HTTP request - if empty
		// ANONYMOUS is set.
		//

		String userId = httpRequest.getUserPrincipal() != null ? httpRequest
		    .getUserPrincipal().getName() : RemoteQuery.ANONYMOUS;
		HttpSession session = httpRequest.getSession();

		// String rootPath = session.getServletContext().getRealPath("/");

		//
		// Selecting last part in URL as service id. If empty an exception result is
		// returned: '-no service id-'
		//

		String requestUri = httpRequest.getRequestURI();
		logger.fine("Request URI " + requestUri);
		String[] parts = requestUri.split("/");
		if (parts.length > 0) {
			serviceId = parts[parts.length - 1];
		}
		logger.fine("serviceId: " + serviceId);
		if (Utils.isBlank(serviceId)) {
			logger.severe("-no service id-" + requestUri);
			returnAsJsonString(JsonUtils.toJson("-no service id-"), httpResponse);
			return;
		}

		try {
			ProcessLog pLog = ProcessLog.Current();

			//
			// Create RQ request with userId.
			//

			Request request = new Request(REQUEST);
			request.setUserId(userId);

			//
			//
			// Setting RQ INITIAL parameters such as $WEBROOT, $SERVICEID, $USERID,
			// $CURRENT_TIME_MILLIS, $TIMESTAMP and $DATE.
			//
			//

			long currentTimeMillis = System.currentTimeMillis();

			request.put(INITIAL, WebConstants.$WEBROOT, webRoot);

			request.put(INITIAL, WebConstants.$SERVICEID, serviceId);

			request.put(INITIAL, WebConstants.$CURRENT_TIME_MILLIS, ""
			    + currentTimeMillis);

			request.put(INITIAL, WebConstants.$TIMESTAMP,
			    Utils.toIsoDateTimeSec(currentTimeMillis));

			request.put(INITIAL, WebConstants.$DATE,
			    Utils.toIsoDate(currentTimeMillis));

			//
			//
			// Setting RQ REQUEST parameters with build-in RequestDataHandler or with
			// Java class defined by servlet init parameter 'requestDataHandler'.
			//
			//

			RequestData requestData = null;
			if (!Utils.isBlank(requestDataHandler)) {
				try {
					IRequestDataHandler rdh = (IRequestDataHandler) Class.forName(
					    requestDataHandler).newInstance();
					requestData = rdh.process(httpRequest);
				} catch (Exception e1) {
					logger.warning("Exception in IRequestDataHandler creation: "
					    + e1.getMessage() + " Fallback to default IRequestDataHandler!");
				}
			}
			if (requestData == null) {
				logger.fine("Use default IRequestDataHandler.");
				requestData = getRequestData(httpRequest);
			}

			// copy String parameters
			Map<String, List<String>> param = requestData.getParameters();
			for (Entry<String, List<String>> paramEntry : param.entrySet()) {
				String name = paramEntry.getKey();
				List<String> values = paramEntry.getValue();
				String value = null;
				if (values != null) {
					if (values.size() > 1) {
						value = Utils.joinTokens(values);
						logger.fine("paramEntry (multi value joined!): " + name + ":"
						    + value);
					} else {
						value = values.get(0);
						logger.fine("paramEntry: " + name + ":" + value);
					}
					request.put(REQUEST, name, value);
				}
			}
			// copy File parameters
			request.setFileInfo(requestData.getFileInfo());

			//
			//
			// Setting RQ HEADER parameters from the HTTP request headers.
			//
			//

			@SuppressWarnings("rawtypes")
			Enumeration e = httpRequest.getHeaderNames();
			while (e.hasMoreElements()) {
				String name = (String) e.nextElement();
				String value = httpRequest.getHeader(name);
				request.put(HEADER, name, value);
				logger.fine("http header: " + name + ":" + value);
			}

			//
			//
			// Setting RQ SESSION paramters from the HTTP session if value is a
			// string.
			//
			//

			e = session.getAttributeNames();
			while (e.hasMoreElements()) {
				String name = (String) e.nextElement();
				Object value = session.getAttribute(name);
				if (value instanceof String) {
					request.put(SESSION, name, (String) value);
					logger.fine("http session parameter: " + name + ":" + value);
				} else {
					logger.fine("http session parameter: " + name
					    + ". Skipped! Value is not a string.");
				}
			}

			//
			//
			// Setting RQ request transient attribute with
			// 'httpRequest'-> HTTP request and 'requestData' -> request data
			// (RequestDataHandler).
			//
			//

			request.setTransientAttribute("httpRequest", httpRequest);
			request.setTransientAttribute("requestData", requestData);

			//
			// Authentication and authorisation check. When accessServiceId is set,
			// the RQ request is run with this accessServiceId.
			// In case the RQ result has an Exception (string) the processing aborts
			// and the exception is return as a JSON string.
			//

			//
			// When accessServiceId is available the corresponding RQ is executed with
			// the accessServiceId. If the
			// result of the RQ run is null or the exception of the run is null.
			// RQ SESSION parameters are written (back) to the HTTP session.
			//

			if (!Utils.isBlank(accessServiceId)) {
				request.setServiceId(accessServiceId);
				MainQuery accessRq = new MainQuery();
				Result r = accessRq.run(request);
				String exception = (r == null) ? null : r.getException();
				if (Utils.isBlank(exception)) {
					Map<String, String> map = request.getParameters(SESSION);
					for (Entry<String, String> entry : map.entrySet()) {
						session.setAttribute(entry.getKey(), entry.getValue());
					}
				} else {
					returnAsJsonString(JsonUtils.exception(exception), httpResponse);
					return;
				}
			} else {
				logger
				    .fine("No accessServiceId defined. No accessService will be processing.");
			}

			//
			// Set serviceId to RQ request.
			//

			request.setServiceId(serviceId);

			long startTime = System.currentTimeMillis();
			MainQuery mainRq = new MainQuery();
			Result result = mainRq.run(request);
			pLog.system("Request time used (ms):"
			    + (System.currentTimeMillis() - startTime), logger);

			//
			// RQ SESSION parameters are written (back) to the HTTP session.
			//

			Map<String, String> newSessionMap = request.getParameters(SESSION);
			for (Entry<String, String> sessionEntry : newSessionMap.entrySet()) {
				session.setAttribute(sessionEntry.getKey(), sessionEntry.getValue());
			}

			//
			// Writing result to HttpResponse
			//

			if (result != null) {
				String s = JsonUtils.toJson(result);
				returnAsJsonString(s, httpResponse);
			} else {
				returnAsJsonString(JsonUtils.toJson("empty"), httpResponse);
			}
		} catch (Exception e) {
			logger.severe(Utils.getStackTrace(e));
		} finally {
			ProcessLog.RemoveCurrent();
		}
	}

	public static void returnAsJsonString(String s, HttpServletResponse response) {
		try {
			byte[] document = s.getBytes(ENCODING);
			response.setContentType("application/json");
			response.setContentLength(document.length);
			response.setCharacterEncoding(ENCODING);

			OutputStream out = response.getOutputStream();
			out.write(document);
			out.flush();
		} catch (Exception e) {
			logger.severe(Utils.getStackTrace(e));
		}
	}


	public static class RequestData implements Serializable {

		private final Map<String, List<String>> parameters = new HashMap<String, List<String>>();
		private final Map<String, String> fileInfo = new HashMap<String, String>();
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

		public Map<String, String> getFileInfo() {
			return fileInfo;
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
				logger.fine("http request parameter: " + name + ":" + value);
			}

		}
		return rd;
	}
	
	public interface IRequestDataHandler {
		RequestData process(HttpServletRequest httpRequest) throws Exception;
	}

}
