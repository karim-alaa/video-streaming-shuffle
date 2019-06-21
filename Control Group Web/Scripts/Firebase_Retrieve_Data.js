
$(document).ready(function () {
    var databaseRef = firebase.database().ref().child("admins/admin_1");

    var model = new Object();

    databaseRef.on("value", function (snapshot) {
        prepareLists(snapshot);

    }, function (error) {
        console.log("Error: " + error.code);
    });
});


function prepareLists(obj) {
    var videos = new Array();
    var i = 0;
    obj.forEach(function(childSnapshot) {
        childSnapshot.forEach(function (childSnapshot2) {
            var value = Object.values(childSnapshot2);
            var vi = JSON.parse(value[0]);
            for (video in vi) {
                videos[i] = vi[video];
                i++;
            }
            videos[i] = "##";
            i++;
        });
        videos[i] = "$$";
        i++;
    });

    var model = new Object();
    model.videos_urls = videos;
    jQuery.ajax({
        type: "POST",
        url: "@Url.Action('CreateList')",
        dataType: "json",
    contentType: "application/json; charset=utf-8",
    data: JSON.stringify({ data: model }),
    success: function (response) {0
        if (response != null && response.success) {
            window.location.href = '../Video/Index';
        } else {
            // DoSomethingElse()
            alert(response.responseText);
        }
    },
    error: function (response) {
        alert("error!");  // 
    }
});

      
}
