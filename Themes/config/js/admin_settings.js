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
            nbxget('settings_admin_get', '#Settingsadminsearch', '#datadisplay');
        };


        if (e.cmd == 'settings_admin_get') {

            $('.actionbuttonwrapper').show();
            $('.processing').hide();

            $('.Settingssearchpanel').hide();
            
            $("#SettingsAdmin_cmdSave").show();
            $("#SettingsAdmin_cmdReturn").show();

            // ---------------------------------------------------------------------------
            // ACTION BUTTONS
            // ---------------------------------------------------------------------------
            $('#SettingsAdmin_cmdReturn').unbind("click");
            $('#SettingsAdmin_cmdReturn').click(function () {
                $('.processing').show();
                $('#razortemplate').val('Admin_SettingsList.cshtml');
                $('#selecteditemid').val('');
                nbxget('Settings_admin_getlist', '#Settingsadminsearch', '#datadisplay');
            });
            
            $('#SettingsAdmin_cmdSave').unbind("click");
            $('#SettingsAdmin_cmdSave').click(function () {
                $('.actionbuttonwrapper').hide();
                $('.editlanguage').hide();
                $('.processing').show();
                //move data to update postback field
                $('#xmlupdatemodeldata').val($.fn.genxmlajaxitems('#Settingsmodels', '.modelitem'));
                nbxget('Settings_admin_save', '#Settingsdatasection', '#actionreturn');
            }); 


        }

    };

});

