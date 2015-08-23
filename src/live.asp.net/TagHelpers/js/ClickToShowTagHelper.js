// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

$("body").on("click", "p[data-hidden-value]", function() {
	var $self = $(this),
		state = $self.data("state") || "hidden";

	if (state === "hidden") {
		$self.text($self.data("hidden-value"));
		$self.data("state", "showing");
		$self.addClass("click-to-show-revealed");
	} else {
		$self.text("click to show");
		$self.data("state", "hidden");
		$self.removeClass("click-to-show-revealed");
	}
});