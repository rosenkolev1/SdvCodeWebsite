﻿$(document).ready(function () {
    $('#themeButton').click(function (e) {
        e.preventDefault();
        e.stopPropagation();
        let span = document.getElementById("themeSpan");
        if (span) {
            if (span.style.visibility == "visible") {
                span.style.visibility = "";
            } else {
                span.style.visibility = "visible";
            }
        }
    });

    $('body').click(function () {
        let span = document.getElementById("themeSpan");
        if (span) {
            span.style.visibility = "";
        }
    });
});

function changeTheme(imageTag, themeId, themeName, themeUrl) {
    let panel = document.getElementById("chatPanel");
    if (panel) {
        panel.style.backgroundImage = `url(${themeUrl})`;
    }

    let oldBadge = document.querySelector(".theme-image .select-theme-badge");

    if (oldBadge) {
        let child = oldBadge.parentElement.children[1];
        oldBadge.parentElement.removeChild(child);
    }
    imageTag.parentElement.innerHTML += `<span class="select-theme-badge">
                                          <i class="fas fa-check"></i>
                                      </span>`;

    addGroupThemeToDatabase(themeId);
}

function addGroupThemeToDatabase(themeId) {
    let group = document.getElementById("groupName").textContent;
    let toUser = document.getElementById("toUser").textContent;

    $.ajax({
        type: "POST",
        url: `/PrivateChat/With/${toUser}/Group/${group}/ChangeChatTheme`,
        contentType: "application/x-www-form-urlencoded",
        dataType: "json",
        data: {
            'username': toUser,
            'group': group,
            'themeId': themeId
        },
        headers: {
            RequestVerificationToken:
                $('input:hidden[name="__RequestVerificationToken"]').val()
        }
    })
}