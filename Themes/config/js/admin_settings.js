$(document).ready(function () {

    $(document).on("nbxgetcompleted", Admin_Settings_nbxgetCompleted); // assign a completed event for the ajax calls

    // start load all ajax data, continued by js in Settings.js file
    $('.processing').show();

    nbxget('settings_admin_get', '#settingsparams', '#datadisplay');

    // function to do actions after an ajax call has been made.
    function Admin_Settings_nbxgetCompleted(e) {

        $('.actionbuttonwrapper').show();
        $('.editlanguage').show();

        setupbackoffice(); // run JS to deal with standard BO functions like accordian.


        //NBS - Tooltips
        $('[data-toggle="tooltip"]').tooltip({
            animation: 'true',
            placement: 'auto top',
            viewport: { selector: '#content', padding: 0 },
            delay: { show: 100, hide: 200 }
        });

        if (e.cmd == 'Settings_admin_save') {
            $("#editlang").val($("#nextlang").val());
            $("#editlanguage").val($("#nextlang").val());
            nbxget('settings_admin_get', '#settingsparams', '#datadisplay');
        };


        if (e.cmd == 'settings_admin_get') {

            $('.actionbuttonwrapper').show();
            $('.processing').hide();
           
            $("#SettingsAdmin_cmdSave").show();

            // ---------------------------------------------------------------------------
            // ACTION BUTTONS
            // ---------------------------------------------------------------------------
            
            $('#settingadmin_cmdSave').unbind("click");
            $('#settingadmin_cmdSave').click(function () {
                $('.actionbuttonwrapper').hide();
                $('.editlanguage').hide();
                $('.processing').show();
                //move data to update postback field
                nbxget('settings_admin_save', '#settingsdata', '#datadisplay');
            }); 

            // ---------------------------------------------------------------------------
            // FILE UPLOAD
            // ---------------------------------------------------------------------------
            var filecount = 0;
            var filesdone = 0;
            $(function () {
                'use strict';
                var url = '/DesktopModules/NBright/NBrightBuy/XmlConnector.ashx?cmd=fileupload';
                $('#fileupload').unbind('fileupload');
                $('#fileupload').fileupload({
                    url: url,
                    maxFileSize: 5000000,
                    acceptFileTypes: /(\.|\/)(gif|jpe?g|png)$/i,
                    dataType: 'json'
                }).prop('disabled', !$.support.fileInput).parent().addClass($.support.fileInput ? undefined : 'disabled')
                    .bind('fileuploadprogressall', function (e, data) {
                        var progress = parseInt(data.loaded / data.total * 100, 10);
                        $('#progress .progress-bar').css('width', progress + '%');
                    })
                    .bind('fileuploadadd', function (e, data) {
                        $.each(data.files, function (index, file) {
                            $('input[id*="imguploadlist"]').val($('input[id*="imguploadlist"]').val() + file.name + ',');
                            filesdone = filesdone + 1;
                        });
                    })
                    .bind('fileuploadchange', function (e, data) {
                        filecount = data.files.length;
                        $('.processing').show();
                    })
                    .bind('fileuploaddrop', function (e, data) {
                        filecount = data.files.length;
                        $('.processing').show();
                    })
                    .bind('fileuploadstop', function (e) {
                        if (filesdone == filecount) {
                            nbxget('settings_updatelogo', '#settingsdata', '#datadisplay'); // load images
                            filesdone = 0;
                            $('input[id*="imguploadlist"]').val('');
                            $('.processing').hide();
                            $('#progress .progress-bar').css('width', '0');
                        }
                    });
            });

            $('#removeimage').unbind("click");
            $('#removeimage').click(function () {
                $('.processing').show();
                nbxget('settings_removelogo', '#settingsdata', '#datadisplay');
            });

        }

        if (e.cmd == 'settings_updatelogo') {
            $('.processing').hide();
        }
        if (e.cmd == 'settings_removelogo') {
            $('.processing').hide();
        }        
        if (e.cmd == 'settings_admin_save') {
            $('.processing').hide();
        }        

    };

});

