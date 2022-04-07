package org.remotequery;


public class RequestLogger {

  private RequestLogger() {
  }

  private static IRequestLogger iRequestLogger;

  public static void setRequestLogger(IRequestLogger logger) {
    iRequestLogger = logger;
  }


  public static void log(RemoteQuery.Request request) {
    if (iRequestLogger != null) {
      iRequestLogger.log(request);
    }
  }
}
