var async = require('async');
var request = require('request');
var _ = require('lodash');
var diff = require('deep-diff').diff;
var ProgressBar = require('progress');

var compare = module.exports.compare = function compare(host1, host2, paths, callback) {
	paths.forEach(function runCase(path) {
		async.parallel({
			host1: function host1Request(cb) {
				request(host1 + path, function(e, r, b) {
					return cb(e, b);
				});
			},
			host2: function host1Request(cb) {
				request(host2 + path, function(e, r, b) {
					return cb(e, b);
				});
			}
		}, function parallelCb(error, results) {
			if (error) {
				return callback(error);
			}

			var differences = diff(results.host1, results.host2);

			return callback(undefined, differences);
		});
	});
};

var bench = module.exports.bench = function bench(hosts, paths, callback) {
	var bar = new ProgressBar('benchmarking [:bar] :percent/ (:current from :total) :etas', {
		complete: '=',
		incomplete: ' ',
		width: 20,
		total: paths.length*hosts.length
	});

	var acumulators = {};

	hosts.forEach(function createAcumulator(host, i) {
		acumulators[host] = {
			time: 0,
			calls: 0,
			errors: []
		};
	});

	async.mapLimit(paths, 5, function runCase(path, mapCb) {


		var clients = hosts.map(function buildClient(host) {
			return function(path, cb) {

				var start = new Date();

				var opts = {
					uri: host+path,
					timeout: 5000
				};

				request(opts, function requestCb(error, response, body) {

					var data = {
						host: host,
						path: path,
						error: error,
						body: body,
						time: Date.now() - start.getTime()
					};

					bar.tick(1);
					return cb(undefined, data);
				});
			};
		});

		clients = clients.map(function applyPathToClient(client) {
			return async.apply(client, path);
		});

		async.parallel(clients, function(error, results) {
			if (error) {
				return mapCb(error);
			}

			results.forEach(function(result, index) {
				if (result.error) {
					acumulators[result.host].errors.push({
						path: path,
						host: result.host,
						error: result.error
					});
				} else {
					// console.log("[%s][%s]: %s", result.host, result.path, result.time);
					acumulators[result.host].time += result.time;
					acumulators[result.host].calls++;
				}
			});

			return mapCb();
		});

	}, function done(error) {
		return callback(error, acumulators);
	});
};