module.exports = {
    "dataSource": "issues",
    "prefix": "LoA " + process.env.LOA_VERSION + " - EasyLOU ",
    "onlyMilestones": false,
    "groupBy": "label",
    "changelogFilename": "CHANGELOG.md",
	"template": {
		issue: function (placeholders) {
			return '- ' + placeholders.name + ' [' + placeholders.text + '](' + placeholders.url + ')';
        },
		noLabel: function (placeholders) {
			return '';
        },
		group: function (placeholders) {
			var heading = placeholders.heading;
			switch (heading) {
				case "enhancement":
					heading = "Enhancements:";
					break;
				case "bug":
					heading = "Bugfixes:";
					break;
			}
            return '\n#### ' + heading + '\n';
        }
	}
}
