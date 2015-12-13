var tid = setInterval(reload, 1000);
var Questions ;
var lastTime = "2014121303531735";
var UserName = "";
function reload() {
    var scroll = 0;
    $.ajax({
        dataType: "json",
        url: "/OnAir/GetData/" + lastTime
    }).done(function (data) {
        if ($("#chatWindow").scrollTop() + $("#chatWindow").innerHeight() >= $("#chatWindow")[0].scrollHeight) {
            scroll = 1;
        }
        lastTime = data["LastTime"];
        $.each(data["chats"], function(key, value) {
            var userName = data["chats"][key]["UserName"];
            var time = data["chats"][key]["TimeStampHuman"];
            var message = data["chats"][key]["Message"];
            $("#chatWindow").append("[" + userName + "](" + time + "): " + message + "<br/>");
        });
        var found = 0;
        if (typeof Questions !== 'undefined') {
            $.each(data["questions"], function (x, y) {
                found = 0;
                $.each(Questions, function(k, v) {
                        if (data["questions"][x]["Id"] === Questions[k]["Id"] ) {
                            found = 1;
                            Questions[k]["Vote"] = data["questions"][x]["Vote"];
                            Questions[k]["Answering"] = data["questions"][x]["Answering"];
                            Questions[k]["Answered"] = data["questions"][x]["Answered"];
                            Questions[k]["AdminMenu"] = data["questions"][x]["AdminMenu"];
                        }
                    });
                    if (found == 0) {
                        Questions.push(data["questions"][x]);
                    }
            });
           
        } else {
            $.each(data["questions"], function(key, value) {
                if (typeof Questions !== 'undefined') {
                    Questions.push(data["questions"][key]);
                } else {
                    Questions = [data["questions"][key]];
                }
            });
        }
      

        if(data["questions"].length !== 0) {
            sortResults();
        }
        if (scroll == 1) {
            $("#chatWindow").scrollTop($("#chatWindow")[0].scrollHeight);
        }
           
    });
}
function sortResults() {
    Questions = Questions.sort(function (a, b) {
        return (b["Vote"] > a["Vote"]);
    });
    $("#QuestionBar").html("");
    showQuestions();
}
function showQuestions() {
   
    $.each(Questions, function (key, value) {
        var userName = Questions[key]["UserName"];
        var Question = Questions[key]["Question"];
        var Id = Questions[key]["Id"];
        var Votes = Questions[key]["Vote"];
        var Answering = Questions[key]["Answering"];
        var Answered = Questions[key]["Answered"];
        var AdminMenu = Questions[key]["AdminMenu"];
        if (Answering == 1) {
            $("#QuestionBar").append("<div class=\"panel panel-answering OnAir\" id=\"questionId" + Id + "\">\n" +
                    "<div class=\"panel-heading SmallPaddingNoMargin\"><div style=\"float:left\"> Answering</div><div style=\"float:right\">"+ AdminMenu+"</div><div style=\"clearfix\">&nbsp;</div></div>\n" +
                    "<div class=\"panel-body SmallPaddingNoMargin Question ScrollOverFlow\">" + Question + "</div>\n" +
                    "<div class=\"panel-footer SmallPaddingNoMargin\"><div style=\"float:left\">" + userName + "</div><div style=\"float:right\"></div><div style=\"clearfix\">&nbsp;</div></div>\n" +
                    "</div>");
        }
        else if (Answered == 1) {
            $("#QuestionBar").append("<div class=\"panel panel-answered OnAir\" id=\"questionId" + Id + "\">\n" +
                    "<div class=\"panel-heading SmallPaddingNoMargin\"><div style=\"float:left\"> Answered</div><div style=\"float:right\"></div><div style=\"clearfix\">&nbsp;</div></div>\n" +
                    "<div class=\"panel-body SmallPaddingNoMargin Question ScrollOverFlow\">" + Question + "</div>\n" +
                    "<div class=\"panel-footer SmallPaddingNoMargin\"><div style=\"float:left\">" + userName + "</div><div style=\"float:right\"></div><div style=\"clearfix\">&nbsp;</div></div>\n" +
                    "</div>");
        }
        else {
                $("#QuestionBar").append("<div class=\"panel panel-primary OnAir\" id=\"questionId" + Id + "\">\n" +
                    "<div class=\"panel-heading SmallPaddingNoMargin\"><div style=\"float:left\"> Pending</div><div style=\"float:right\">" + AdminMenu + "</div><div style=\"clearfix\">&nbsp;</div></div>\n" +
                    "<div class=\"panel-body SmallPaddingNoMargin Question ScrollOverFlow\">" + Question + "</div>\n" +
                    "<div class=\"panel-footer SmallPaddingNoMargin\"><div style=\"float:left\">" + userName + "</div><div style=\"float:right\">Votes " + Votes + "<a href=\"/onAir/PlusOne/" + Id + "\" class=\"PlusOne\">+1</a></div><div style=\"clearfix\">&nbsp;</div></div>\n" +
                    "</div>");
        }
        $("#questionId" + Id + " a").click(function (e) {
        e.preventDefault();
        $.ajax({
            url: $(this).attr('href'),
            success: function (html) {
            }
        });
    });
    });
}
$(window).resize(function () {
    SetMaxHeight($('iframe').height());

});

$(window).load(function () {
    SetMaxHeight($('iframe').height());
    
});

$("#chat").submit(function (e) {
    if ($("#Message").val() != "") {
        $.ajax({
            url: $(this).attr('action'),
            type: $(this).attr('method'),
            data: $(this).serialize(),
            success: function(html) {
                $("#Message").val("");
            }
        });
    }
    e.preventDefault();
});

$("#AskQuestion").submit(function (e) {
    if ($("#Question").val() != "") {
        $.ajax({
            url: $(this).attr('action'),
            type: $(this).attr('method'),
            data: $(this).serialize(),
            success: function (html) {
                $("#Question").val("");
            }
        });
    }
    e.preventDefault();
});

function SetMaxHeight(height) {
    $("#QuestionBar").css({ "max-height": height + 'px' });
}