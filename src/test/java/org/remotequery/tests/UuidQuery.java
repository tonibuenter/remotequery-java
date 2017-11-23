package org.remotequery.tests;

import java.util.UUID;

import org.remotequery.RemoteQuery.IQuery;
import org.remotequery.RemoteQuery.Request;
import org.remotequery.RemoteQuery.Result;

public class UuidQuery implements IQuery {

	@Override
	public Result run(Request request) {
		UUID uuid = UUID.randomUUID();
		Result result = new Result("uuid");
		result.addRowVar(uuid.toString());
		return result;
	}

}
