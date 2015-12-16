var Questions;

$(window).resize(function() {
    SetMaxHeight($("iframe").height());

});

$(window).load(function() {
    SetMaxHeight($("iframe").height());

});

function SetMaxHeight(height) {
    $("#QuestionBar").css({ "max-height": height + "px" });
}


$(function() {
    $("#JoinConversation").html("Connecting");
    $("#JoinConversation").prop("disabled", true);
    var hub = $.connection.chatHub;

    hub.client.updateMessages = function(message) {
        var scroll = 0;
        if ($("#chatWindow").scrollTop() + $("#chatWindow").innerHeight() >= $("#chatWindow")[0].scrollHeight) {
            scroll = 1;
        }
        $("#chatWindow").append(message + "<br/>");
        if (scroll === 1) {
            $("#chatWindow").scrollTop($("#chatWindow")[0].scrollHeight);
        }
    };

    hub.client.newQuestion = function(data, menu) {
        if (menu !== "")
            data["AdminMenu"] = menu;
        else
            data["AdminMenu"] = "";
        data["Show"] = 1;
        if (typeof Questions !== "undefined") {

            Questions.push(data);
        } else {
            Questions = [data];
        }
        sortResults();
    };
    hub.client.updateQuestion = function(data, menu) {
        $.each(Questions, function(key, value) {
            if (Questions[key].Id === data.Id) {
                Questions[key]["Answering"] = data.Answering;
                Questions[key]["Answered"] = data.Answered;
                if (typeof menu !== "undefined" && menu !== "") {
                    Questions[key]["AdminMenu"] = menu;
                }
                Questions[key]["Vote"] = data.Vote;
            }
        });
        sortResults();
    };


    hub.client.join = function(message) {
        $("#chatbar").html(message);
        $("#Join").html("");
        $("#new-message").bind("keypress", function(e) {
            var code = e.keyCode || e.which;
            if (code === 13) { //Enter keycode
                var message = $("#new-message").val();
                hub.server.send(message);
                $("#new-message").val("");
            }
        });

        $("#ask").click(function() {
            hub.server.askQuestion($("#question").val());
            $("#question").val("");
        });
    };
    $.connection.hub.start().done(function() {
        $("#JoinConversation").prop("disabled", false);
        $("#JoinConversation").html("Join Conversation");
        $("#JoinConversation").click(function () {
            if ($("#ChatHandle").val().length < 18) {
                $("#JoinConversation").hide();
                var message = $("#ChatHandle").val();
                hub.server.setChatHandle(message);
                $("#chatHandle").val("");
            } else {
                alert($("#ChatHandle").val() + " is to long. Max length 18 chars.");
            }
        });
    });

    function sortResults() {
        Questions = Questions.slice(0).sort(function(a, b) {
            return (b["Vote"] - a["Vote"]);
        });
        $("#QuestionBar").html("");
        showQuestions();
    }

    function showQuestions() {

        $.each(Questions, function(key, value) {
            var userName = Questions[key]["UserName"];
            var question = Questions[key]["Question"];
            var id = Questions[key]["Id"];
            var votes = Questions[key]["Vote"];
            var answering = Questions[key]["Answering"];
            var answered = Questions[key]["Answered"];
            var adminMenu = Questions[key]["AdminMenu"];
            var show = Questions[key]["Show"];
            var plusOne = "Votes " + votes;
            if (show === 1)
                plusOne = "Votes " + votes + "<a href=\"#\" id=\"" + id + "\" action=\"PlusOne\" class=\"PlusOne\">+1</a>";
            if (answering == 1) {
                $("#QuestionBar").append("<div class=\"panel panel-answering OnAir\" id=\"questionId" + id + "\">\n" +
                    "<div class=\"panel-heading SmallPaddingNoMargin\"><div style=\"float:left\"> Answering</div><div style=\"float:right\">" + adminMenu + "</div><div style=\"clearfix\">&nbsp;</div></div>\n" +
                    "<div class=\"panel-body SmallPaddingNoMargin Question ScrollOverFlow\">" + question + "</div>\n" +
                    "<div class=\"panel-footer SmallPaddingNoMargin\"><div style=\"float:left\">" + userName.substring(0, 18) + "</div><div style=\"float:right\"></div><div style=\"clearfix\">&nbsp;</div></div>\n" +
                    "</div>");
            } else if (answered == 1) {
                $("#QuestionBar").append("<div class=\"panel panel-answered OnAir\" id=\"questionId" + id + "\">\n" +
                    "<div class=\"panel-heading SmallPaddingNoMargin\"><div style=\"float:left\"> Answered</div><div style=\"float:right\"></div><div style=\"clearfix\">&nbsp;</div></div>\n" +
                    "<div class=\"panel-body SmallPaddingNoMargin Question ScrollOverFlow\">" + question + "</div>\n" +
                    "<div class=\"panel-footer SmallPaddingNoMargin\"><div style=\"float:left\">" + userName.substring(0, 18) + "</div><div style=\"float:right\"></div><div style=\"clearfix\">&nbsp;</div></div>\n" +
                    "</div>");
            } else {
                $("#QuestionBar").append("<div class=\"panel panel-primary OnAir\" id=\"questionId" + id + "\">\n" +
                    "<div class=\"panel-heading SmallPaddingNoMargin\"><div style=\"float:left\"> Pending</div><div style=\"float:right\">" + adminMenu + "</div><div style=\"clearfix\">&nbsp;</div></div>\n" +
                    "<div class=\"panel-body SmallPaddingNoMargin Question ScrollOverFlow\">" + question + "</div>\n" +
                    "<div class=\"panel-footer SmallPaddingNoMargin\"><div style=\"float:left\">" + userName.substring(0, 18) + "</div><div style=\"float:right\">" + plusOne + "</div><div style=\"clearfix\">&nbsp;</div></div>\n" +
                    "</div>");
            }
            $("#questionId" + id + " a").click(function(e) {
                e.preventDefault();
                var myId = $(this).attr("id");
                hub.server.updateQuestion(id, $(this).attr("action"));
                $.each(Questions, function(key, value) {
                    if (Questions[key]["Id"] === myId) {
                        Questions[key]["Show"] = 0;
                    }
                });
            });
        });
    }
});