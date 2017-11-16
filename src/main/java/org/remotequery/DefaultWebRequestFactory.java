package org.remotequery;

import java.util.ArrayList;
import java.util.List;
import java.util.Map;
import java.util.Properties;
import java.util.Set;

import javax.servlet.http.HttpServletRequest;
import javax.servlet.http.HttpServletResponse;

import org.remotequery.z_RemoteQuery.Request;
import org.remotequery.z_RemoteQuery.Utils;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

/**
 * 
 * @author tonibuenter
 * 
 */
public class DefaultWebRequestFactory implements IWebRequestFactory {

	private static Logger logger = LoggerFactory.getLogger(DefaultWebRequestFactory.class);

	protected Properties appProperties;

	@Override
	public void init(Properties appProperties) {

		this.appProperties = appProperties;

	}

	IWebSecurity webSecurity(HttpServletRequest httpRequest, HttpServletResponse httpResponse) {
		IWebSecurity webSecurity = null;
		String className = appProperties.getProperty("IWebRequestFactory.webSecurity");
		try {
			webSecurity = (IWebSecurity) Class.forName(className).newInstance();
			webSecurity.init(this.appProperties, httpRequest, httpResponse);
		} catch (Exception e) {
			logger.error(
					"Can not create IWebSecurity. " + "Please check the app property IWebRequestFactory.webSecurity",
					e);
		}
		return webSecurity;
	}

	@Override
	public List<Request> prepare(HttpServletRequest httpRequest, HttpServletResponse httpResponse) {

		List<Request> requests = new ArrayList<z_RemoteQuery.Request>();

		String requestUri = httpRequest.getRequestURI();

		IWebSecurity webSecurity = webSecurity(httpRequest, httpResponse);
		String service = null;
		String userId = "ANONYMOUS";
		Set<String> roles = null;

		//
		// WEB SECURITY
		//

		if (webSecurity == null) {
			return null;
		}

		//
		// SERVICE
		//

		service = webSecurity.service();
		if (Utils.isBlank(service)) {
			logger.warn("No service id found in " + requestUri);
			return null;
		}

		//
		// USER ID
		//

		userId = webSecurity.userId();
		if (Utils.isBlank(userId)) {
			logger.warn("No user id found for " + requestUri);
			return null;
		}

		//
		// ROLES
		//

		roles = webSecurity.roles();
		if (roles == null) {
			logger.warn("No roles found for " + userId + " in " + requestUri);
			return null;
		}

		//
		// PARAMETERS
		//

		Map<String, String> parameters = webSecurity.parameters();

		//
		// REQEUSTS
		//

		if (parameters == null) {
			logger.warn("No parameters found for " + service + " in " + requestUri);
			return null;
		}
		String[] serviceIds = service.split(",");
		for (String serviceId : serviceIds) {
			Request request = new Request();
			request.setServiceId(serviceId);
			request.setUserId(userId);
			request.addRoles(roles);
			request.put(parameters);
			requests.add(request);
		}

		return requests;
	}

}

// if (!Utils.isBlank(requestDataHandler)) {
// try {
// IRequestDataHandler rdh = (IRequestDataHandler)
// Class.forName(requestDataHandler).newInstance();
// requestData = rdh.process(httpRequest);
// } catch (Exception e1) {
// logger.warn("Exception in IRequestDataHandler creation: " +
// e1.getMessage()
// + " Fallback to default IRequestDataHandler!");
// }
// }
// if (requestData == null) {
// logger.debug("Use default IRequestDataHandler.");
// requestData = getRequestData(httpRequest);
// }

// analyse http request
