
Servlet Configurations (web.xml)
--------------------------------

Let me explain the configuration with a real world Example:

  <servlet>
    <servlet-name>RemoteQueryServlet</servlet-name>
    <servlet-class>org.remotequery.RemoteQueryServlet</servlet-class>
    <init-param>
      <param-name>accessServiceId</param-name>
      <param-value>RQUserAction</param-value>
    </init-param>
  <init-param>
      <param-name>publicServiceIds</param-name>
      <param-value>VIP.selectAllLabels,VIP.selectLanguages</param-value>
    </init-param>
    <init-param>
      <param-name>headerParameters</param-name>
      <param-value>JSESSION</param-value>
    </init-param>
    <init-param>
      <param-name>requestDataHandler</param-name>
      <param-value>org.jground.anakapa.web.RqFileRequestHandler</param-value>
    </init-param>
    <load-on-startup>1</load-on-startup>
  </servlet>

  <servlet-mapping>
    <servlet-name>RemoteQueryServlet</servlet-name>
    <url-pattern>/remoteQuery/*</url-pattern>
  </servlet-mapping>



RemoteQueryServlet
------------------ 
This is the RQ servlet name used for the mapping
/remoteQuery/*

This mapping pattern is actually the default. So, when using the JavaScript client libraries from 
RemoteQueryWeb no changes have to be made for accessing the server.


Parameter accessServiceId (not mandatory)
------------------------- 

The value of accessServiceId parameter is the name of RQ service responsible for the authentication and authorization.
It will be call before every other request.


Parameter publicServiceIds (not mandatory)
------------------------- 
The value of the 'publicServiceIds' parameter contains a list of RQ services. No access service will be called.


Parameter 'headerParameters' (not mandatory)
------------------------------
The value of the 'headerParameters' parameter contains a list of header names. Their name and value are used for the RQ request parameters. If the 
'headerParameters' parameter is not defined, no header are read for the RQ request.


Parameter 'requestDataHandler' (not mandatory)
------------------------------
In case of specific requirements for handling the HTTP request the class of the parameter value is used. This class has to implement the 
IRequestDataHandler interface.
Here the definition.

  public interface IRequestDataHandler {
    IRequestData process(HttpServletRequest httpRequest) throws Exception;
  }

  public interface IRequestData {
    Map<String, List<String>> getParameters();
  }





