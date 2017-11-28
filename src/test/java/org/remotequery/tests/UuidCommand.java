package org.remotequery.tests;

import java.util.UUID;

import org.remotequery.RemoteQuery.CommandNode;
import org.remotequery.RemoteQuery.ICommand;
import org.remotequery.RemoteQuery.Request;
import org.remotequery.RemoteQuery.Result;
import org.remotequery.RemoteQuery.ServiceEntry;
import org.remotequery.RemoteQuery.Utils;

public class UuidCommand implements  ICommand {

	@Override
	public Result run(Request request, Result currentResult, CommandNode commandNode, ServiceEntry serviceEntry) {
		
		String name = commandNode.parameter;
		String value = request.get(name);

		if (Utils.isBlank(value)) {
			request.put(name, UUID.randomUUID().toString());
		}
		return null;
	}

}
