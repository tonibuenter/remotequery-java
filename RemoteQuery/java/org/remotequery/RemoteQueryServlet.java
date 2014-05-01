package org.remotequery;

import static org.remotequery.RemoteQuery.ENCODING;

import java.io.IOException;
import java.io.OutputStream;
import java.util.Enumeration;
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

		public static final int INITIAL = 0;
		public static final int REQUEST = 10;
		public static final int HEADER = 20;
		public static final int INTER_REQUEST = 30;
		public static final int SESSION = 40;
		public static final int APPLICATION = 50;

	}

	//
	//
	//
	private String servletName = "";
	private String webRoot = "";
	private String accessServiceId = "";
	//...
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
	}

	@Override
	public void doPost(HttpServletRequest request, HttpServletResponse response)
	    throws ServletException, IOException {
		doGet(request, response);
	}

	@Override
	public void doGet(final HttpServletRequest httpRequest,
	    HttpServletResponse httpResponse) throws ServletException, IOException {
		logger.fine("start " + servletName + ".doGet");
		ProcessLog rLog = ProcessLog.Current();

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
			if (Utils.isBlank(serviceId)) {
				logger.severe("serviceId is blank! requestUri: " + requestUri);
				return;
			}
			Request request = new Request(WebConstants.REQUEST);

			//
			// userId (independent from $USERID !
			//
			request.setUserId(userId);

			//
			//
			// RoleProvider :: use RoleProviderFactorySingleton or the web
			//
			//

			// IRoleProviderFactory rpf = RoleProviderFactorySingleton.getInstance();
			// if (rpf != null) {
			// request.setRoleProvider(rpf.getRoleProvider(userId));
			// } else {
			// request.setRoleProvider(new IRoleProvider() {
			//
			// @Override
			// public boolean isInRole(String role) {
			// return httpRequest.isUserInRole(role);
			// }
			//
			// @Override
			// public Set<String> getRoles(String userId) {
			// throw new RuntimeException("'getRoles' not implemented!");
			// }
			// });
			//
			// }

			//
			//
			// INITIAL parameter
			//
			//

			long currentTimeMillis = System.currentTimeMillis();

			request.put(WebConstants.INITIAL, WebConstants.$WEBROOT, webRoot);

			request.put(WebConstants.INITIAL, WebConstants.$SERVICEID, serviceId);

			request.put(WebConstants.INITIAL, WebConstants.$USERID, userId);

			request.put(WebConstants.INITIAL, WebConstants.$CURRENT_TIME_MILLIS, ""
			    + currentTimeMillis);

			request.put(WebConstants.INITIAL, WebConstants.$TIMESTAMP,
			    Utils.toIsoDateTimeSec(currentTimeMillis));

			request.put(WebConstants.INITIAL, WebConstants.$DATE,
			    Utils.toIsoDate(currentTimeMillis));

			//
			//
			// REQUEST PARAMETERS
			//

			@SuppressWarnings("rawtypes")
			Enumeration e = httpRequest.getParameterNames();
			while (e.hasMoreElements()) {
				String name = (String) e.nextElement();
				String value = httpRequest.getParameter(name);
				request.put(WebConstants.REQUEST, name, value);
				logger.fine("http request parameter: " + name + ":" + value);
			}

			//
			//
			// REQUEST HEADERS
			//

			e = httpRequest.getHeaderNames();
			while (e.hasMoreElements()) {
				String name = (String) e.nextElement();
				String value = httpRequest.getHeader(name);
				request.put(WebConstants.HEADER, name, value);
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
					request.put(WebConstants.SESSION, name, (String) value);
					logger.fine("http session parameter: " + name + ":" + value);
				}

			}

			//
			// Check for accessServiceId and run ti
			//

			request.setTransientAttribute("httpRequest", httpRequest);

			if (!Utils.isBlank(accessServiceId)) {
				request.setServiceId(accessServiceId);
				MainQuery process = new MainQuery();
				Result r = process.run(request);
				String exception = r.getException();
				if (Utils.isBlank(exception)) {
					Map<String, String> map = r.getFirstRowAsMap();
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

			request.setServiceId(serviceId);
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

}
