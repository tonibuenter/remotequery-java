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
 * 
 * 
 * */
public class RemoteQueryServlet extends HttpServlet {
	/**
     * 
     */
	private static final long serialVersionUID = 1L;

	/**
	 * Class containing constants for the web programming with RemoteQuery.
	 * 
	 * @author tonibuenter
	 * 
	 */
	public static final class WebConstants {

		//
		/**
		 * "ANONYMOUS" is the default user name.
		 */
		public final static String ANONYMOUS = "ANONYMOUS";
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

		public static final String $USERID = "$USERID";

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

		String userId = httpRequest.getUserPrincipal() != null ? httpRequest
		    .getUserPrincipal().getName() : WebConstants.ANONYMOUS;
		HttpSession session = httpRequest.getSession();

		// String rootPath = session.getServletContext().getRealPath("/");

		String requestUri = httpRequest.getRequestURI();
		logger.info("Request URI " + requestUri);
		String[] parts = requestUri.split("/");

		String serviceId = parts[parts.length - 1];

		logger.fine("serviceId " + serviceId);
		//
		//
		//

		try {
			ProcessLog rLog = ProcessLog.Current();
			if (Utils.isBlank(serviceId)) {
				logger.severe("serviceId is blank! requestUri: " + requestUri);
				return;
			}
			Request request = new Request(REQUEST);

			//
			// userId (independent from $USERID !
			//
			request.setUserId(userId);

			//
			//
			// INITIAL parameter
			//
			//

			long currentTimeMillis = System.currentTimeMillis();

			request.put(INITIAL, WebConstants.$WEBROOT, webRoot);

			request.put(INITIAL, WebConstants.$SERVICEID, serviceId);

			request.put(INITIAL, WebConstants.$USERID, userId);

			request.put(INITIAL, WebConstants.$CURRENT_TIME_MILLIS, ""
			    + currentTimeMillis);

			request.put(INITIAL, WebConstants.$TIMESTAMP,
			    Utils.toIsoDateTimeSec(currentTimeMillis));

			request.put(INITIAL, WebConstants.$DATE,
			    Utils.toIsoDate(currentTimeMillis));

			//
			// REQUEST DATA
			//

			RequestData requestData = null;
			if (!Utils.isBlank(requestDataHandler)) {
				try {
					IRequestDataHandler rdh = (IRequestDataHandler) Class.forName(
					    requestDataHandler).newInstance();
					requestData = rdh.process(httpRequest);
				} catch (Exception e1) {
					logger.warning(e1.getMessage());
				}
			}
			if (requestData == null) {
				requestData = getRequestData(httpRequest);
			}

			Map<String, List<String>> param = requestData.getParameters();
			for (Entry<String, List<String>> entry : param.entrySet()) {
				String name = entry.getKey();
				String value = Utils.joinTokens(entry.getValue());
				request.put(REQUEST, name, value);
				logger.fine("request data: " + name + ":" + value);
			}

			//
			//
			// REQUEST HEADERS
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
			// REQUEST HEADERS
			//

			e = httpRequest.getHeaderNames();
			while (e.hasMoreElements()) {
				String name = (String) e.nextElement();
				String value = httpRequest.getHeader(name);
				request.put(HEADER, name, value);
				logger.fine("http header: " + name + ":" + value);
			}

			//
			//
			// SESSION ATTRIBUTES
			//
			//

			e = session.getAttributeNames();
			while (e.hasMoreElements()) {
				String name = (String) e.nextElement();
				Object value = session.getAttribute(name);
				if (value instanceof String) {
					request.put(SESSION, name, (String) value);
					logger.fine("http session parameter: " + name + ":" + value);
				}

			}

			request.setTransientAttribute("httpRequest", httpRequest);
			request.setTransientAttribute("requestData", requestData);

			//
			// Check for accessServiceId and run it
			//

			if (!Utils.isBlank(accessServiceId)) {
				request.setServiceId(accessServiceId);
				MainQuery process = new MainQuery();
				Result r = process.run(request);
				String exception = r == null ? null : r.getException();
				if (Utils.isBlank(exception)) {
					Map<String, String> map = request.getParameters(SESSION);
					for (Entry<String, String> entry : map.entrySet()) {
						session.setAttribute(entry.getKey(), entry.getValue());
					}
				} else {
					returnString(JsonUtils.exception(exception), httpResponse);
					return;
				}
			} else {
				logger.fine("No accessServiceId defined. No accessService processing.");
			}

			//
			// prepare main RemoteQuery call
			//

			// reset serviceId
			request.setServiceId(serviceId);
			// reset userId
			userId = request.getUserId();
			request.put(INITIAL, WebConstants.$USERID, userId);

			//
			//
			//
			long startTime = System.currentTimeMillis();
			MainQuery process = new MainQuery();
			Result result = process.run(request);
			rLog.system("Request time used (ms):"
			    + (System.currentTimeMillis() - startTime), logger);
			if (result != null) {
				String s = JsonUtils.toJson(result);
				returnString(s, httpResponse);
			} else {
				returnString(JsonUtils.toJson("empty"), httpResponse);
			}
		} catch (Exception e) {
			logger.severe(Utils.getStackTrace(e));
		} finally {
			ProcessLog.RemoveCurrent();
		}
	}

	public static void returnString(String s, HttpServletResponse response) {
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

	public interface IRequestDataHandler {
		RequestData process(HttpServletRequest httpRequest) throws Exception;
	}

	public static class RequestData implements Serializable {

		private Map<String, List<String>> parameters = new HashMap<String, List<String>>();
		private Map<String, String> fileInfo = new HashMap<String, String>();
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
			return values != null && values.size() > 0 ? Utils.joinTokens(values)
			    : null;
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
			String value = Utils.joinTokens(values);
			rd.add(name, value);
			logger.fine("http request parameter: " + name + ":" + value);
		}
		return rd;
	}

}
