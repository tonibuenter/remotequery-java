package org.remotequery;

import java.util.Map;
import java.util.Properties;
import java.util.Set;

import javax.servlet.http.HttpServletRequest;
import javax.servlet.http.HttpServletResponse;

public interface IWebSecurity {

	public void init(Properties appProperties, HttpServletRequest httpRequest, HttpServletResponse httpResponse) ;

	public String service();

	public Map<String, String> parameters();

	public String userId();

	public Set<String> roles();

}