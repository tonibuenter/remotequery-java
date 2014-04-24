package org.remotequery;

import static org.remotequery.RemoteQuery.ENCODING;
import static org.remotequery.RemoteQuery.INITIAL;
import static org.remotequery.RemoteQuery.REQUEST;
import static org.remotequery.RemoteQuery.SESSION;

import java.io.IOException;
import java.io.OutputStream;
import java.util.Enumeration;
import java.util.Set;
import java.util.logging.Logger;

import javax.servlet.ServletConfig;
import javax.servlet.ServletException;
import javax.servlet.http.HttpServlet;
import javax.servlet.http.HttpServletRequest;
import javax.servlet.http.HttpServletResponse;
import javax.servlet.http.HttpSession;

import org.remotequery.RemoteQuery.IRoleProvider;
import org.remotequery.RemoteQuery.IRoleProviderFactory;
import org.remotequery.RemoteQuery.JsonUtils;
import org.remotequery.RemoteQuery.ProcessLog;
import org.remotequery.RemoteQuery.Request;
import org.remotequery.RemoteQuery.Result;
import org.remotequery.RemoteQuery.RoleProviderFactorySingleton;
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

	}

	//
	//
	//
	private String servletName = "";
	private String webRoot = "";
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
	}

	@Override
	public void doPost(HttpServletRequest request, HttpServletResponse response)
	    throws ServletException, IOException {
		doGet(request, response);
	}

	@Override
	public void doGet(final HttpServletRequest request,
	    HttpServletResponse response) throws ServletException, IOException {
		logger.fine("start " + servletName + ".doGet");
		ProcessLog rLog = ProcessLog.Current();
		String userId = request.getUserPrincipal() != null ? request
		    .getUserPrincipal().getName() : WebConstants.ANONYMOUS;
		HttpSession session = request.getSession();

		// String rootPath = session.getServletContext().getRealPath("/");

		String requestUri = request.getRequestURI();
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
			Request rqRequest = new Request(serviceId);

			//
			// userId (independent from $USERID !
			//
			rqRequest.setUserId(userId);

			//
			//
			// RoleProvider :: use RoleProviderFactorySingleton or the web
			//
			//

			IRoleProviderFactory rpf = RoleProviderFactorySingleton.getInstance();
			if (rpf != null) {
				rqRequest.setRoleProvider(rpf.getRoleProvider(userId));
			} else {
				rqRequest.setRoleProvider(new IRoleProvider() {

					@Override
					public boolean isInRole(String role) {
						return request.isUserInRole(role);
					}

					@Override
					public Set<String> getRoles(String userId) {
						throw new RuntimeException("'getRoles' not implemented!");
					}
				});

			}

			//
			//
			// INITIAL parameter
			//
			//

			long currentTimeMillis = System.currentTimeMillis();

			rqRequest.put(INITIAL, WebConstants.$WEBROOT, webRoot);

			rqRequest.put(INITIAL, WebConstants.$SERVICEID, serviceId);

			rqRequest.put(INITIAL, WebConstants.$USERID, userId);

			rqRequest.put(INITIAL, WebConstants.$CURRENT_TIME_MILLIS, ""
			    + currentTimeMillis);

			rqRequest.put(INITIAL, WebConstants.$TIMESTAMP,
			    Utils.toIsoDateTimeSec(currentTimeMillis));

			rqRequest.put(INITIAL, WebConstants.$DATE,
			    Utils.toIsoDate(currentTimeMillis));

			//
			//
			// REQUEST PARAMETERS
			//

			@SuppressWarnings("rawtypes")
			Enumeration enumer = request.getParameterNames();
			while (enumer.hasMoreElements()) {
				String name = (String) enumer.nextElement();
				String value = request.getParameter(name);
				rqRequest.put(REQUEST, name, value);
				logger.fine("http request parameter: " + name + ":" + value);
			}

			//
			//
			// SESSION ATTRIBUTES
			//
			//

			enumer = session.getAttributeNames();
			while (enumer.hasMoreElements()) {
				String name = (String) enumer.nextElement();
				Object value = session.getAttribute(name);
				if (value instanceof String) {
					rqRequest.put(SESSION, name, (String) value);
					logger.fine("http session parameter: " + name + ":" + value);
				}

			}

			//
			//
			long startTime = System.currentTimeMillis();
			RemoteQuery.MainQuery process = new RemoteQuery.MainQuery();
			Result result = process.run(rqRequest);
			rLog.system("Request time used (ms):"
			    + (System.currentTimeMillis() - startTime), logger);
			if (result != null) {
				String s = JsonUtils.toJson(result);
				returnString(s, response);
			} else {
				returnString(JsonUtils.toJson("empty"), response);
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
