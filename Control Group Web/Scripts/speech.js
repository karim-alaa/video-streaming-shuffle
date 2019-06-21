function startDictation() {
    if (window.hasOwnProperty('webkitSpeechRecognition')) {

        var recognition = new webkitSpeechRecognition();
        alert("inside");
        recognition.continuous = false;
        recognition.interimResults = false;

        recognition.lang = "en-US";
        recognition.start();

        recognition.onresult = function (e) {
            document.getElementById('transcript').value
                                     = e.results[0][0].transcript;
            recognition.stop();
            //document.getElementById('labnol').submit();
            var model = new Object();
            model.speech = e.results[0][0].transcript;
            alert(model.speech);
            jQuery.ajax({
                type: "POST",
                url: "@Url.Action('parse_and_response','committe')",
                dataType: "json",
                contentType: "application/json; charset=utf-8",
                data: JSON.stringify({ data: model }),
                success: function (response) {
                  
                    video_ref = response.school_number + "/" + response.committe_nubmer + "/" + response.video_number + ".mp4";
                    var storageRef = firebase.storage().ref();
                    var tangRef = storageRef.child(video_ref);
                    tangRef.getDownloadURL().then(function (url) {
                        alert(url);
                    }).catch(function (error) {
                        console.error(error);
                    });


                },
                failure: function (errMsg) {
                    alert(errMsg);
                }
            });




        };

        recognition.onerror = function (e) {
            recognition.stop();
        }

    }
}
