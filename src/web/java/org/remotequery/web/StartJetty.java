package org.remotequery.web;

import java.io.File;
import java.lang.management.ManagementFactory;

import org.eclipse.jetty.jmx.MBeanContainer;
import org.eclipse.jetty.server.Server;
import org.eclipse.jetty.webapp.WebAppContext;

//
// This code was deducted from the Jetty Documentation 
// and is therefore not for production usage.
// Reference: http://www.eclipse.org/jetty/documentation/current/embedding-jetty.html#_embedding_web_applications
//

public class StartJetty {

	public static void main(String[] args) throws Exception {

		Server server = new Server(8080);

		// Setup JMX
		MBeanContainer mbContainer = new MBeanContainer(ManagementFactory.getPlatformMBeanServer());
		server.addBean(mbContainer);


		WebAppContext webapp = new WebAppContext();
		webapp.setContextPath("/");
		File warFile = new File("src/web/webapp");
		webapp.setWar(warFile.getAbsolutePath());

		server.setHandler(webapp);

		server.start();

		//server.dumpStdErr();

		server.join();
	}

}
