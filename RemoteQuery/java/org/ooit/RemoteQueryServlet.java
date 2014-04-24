package org.ooit;

import static org.ooit.RemoteQuery.$CURRENT_TIME_MILLIS;
import static org.ooit.RemoteQuery.$SERVICEID;
import static org.ooit.RemoteQuery.$TIMESTAMP;
import static org.ooit.RemoteQuery.$TODAY_ISO_DATE;
import static org.ooit.RemoteQuery.$USERID;
import static org.ooit.RemoteQuery.$WEBROOT;
import static org.ooit.RemoteQuery.ANONYMOUS;
import static org.ooit.RemoteQuery.INITIAL;
import static org.ooit.RemoteQuery.REQUEST;
import static org.ooit.RemoteQuery.SESSION;
import static org.ooit.RemoteQuery.ENCODING;

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

import org.ooit.RemoteQuery.IRoleProvider;
import org.ooit.RemoteQuery.IRoleProviderFactory;
import org.ooit.RemoteQuery.JsonUtils;
import org.ooit.RemoteQuery.ProcessLog;
import org.ooit.RemoteQuery.Request;
import org.ooit.RemoteQuery.Result;
import org.ooit.RemoteQuery.RoleProviderFactorySingleton;
import org.ooit.RemoteQuery.Utils;

public class RemoteQueryServlet extends HttpServlet {
	/**
     * 
     */
	private static final long serialVersionUID = 1L;
	//
	//
	//
	private String servletName = "";
	private String webRoot = "";
	//

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
		    .getUserPrincipal().getName() : ANONYMOUS;
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

			rqRequest.put(INITIAL, $SERVICEID, serviceId);
			rqRequest.put(INITIAL, $USERID, userId);
			rqRequest.put(INITIAL, $TIMESTAMP, Utils.nowIsoDateTimeSec());
			rqRequest.put(INITIAL, $TODAY_ISO_DATE, Utils.nowIsoDate());
			rqRequest.put(INITIAL, $CURRENT_TIME_MILLIS,
			    "" + System.currentTimeMillis());
			rqRequest.put(INITIAL, $WEBROOT, webRoot);

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
